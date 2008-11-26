using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataType;

namespace Hubble.Core.Query
{
    /// <summary>
    /// This query analyze documents with position
    /// belong to the input words.
    /// Syntax: FullTextQuery('xxxyyyzzz')
    /// </summary>
    public class FullTextQuery : IQuery
    {
        class WordIndexForQuery
        {
            private int _CurPosition;
            private int _CurIndex;
            private Index.InvertedIndex.WordIndex _WordIndex;

            public IEnumerator<int> _CurPositionValues;

            public IEnumerator<int> CurPositionValues
            {
                get
                {
                    return _CurPositionValues;
                }
            }

            /// <summary>
            /// Cur Position
            /// -1 means end
            /// </summary>
            public int CurPosition
            {
                get
                {
                    return _CurPosition;
                }

                set
                {
                    _CurPosition = value;
                }
            }

            public int CurIndex
            {
                get
                {
                    return _CurIndex;
                }

                set
                {
                    _CurIndex = value;
                }
            }

            public long CurDocumentId
            {
                get
                {
                    if (CurIndex < 0)
                    {
                        return -1;
                    }
                    else
                    {
                        return WordIndex[CurIndex].DocumentId;
                    }
                }
            }

            public Index.InvertedIndex.WordIndex WordIndex
            {
                get
                {
                    return _WordIndex;
                }
            }

            public WordIndexForQuery(Index.InvertedIndex.WordIndex wordIndex)
            {
                _CurPositionValues = null;

                if (wordIndex.Count <= 0)
                {
                    _CurIndex = -1;
                    _CurPosition = -1;
                }
                else
                {
                    _CurIndex = 0;
                    _CurPosition = wordIndex[0].Values.Current;
                }

                _WordIndex = wordIndex;
            }

            public void SetCurPositionValues(IEnumerator<int> curPositionValues)
            {
                _CurPositionValues = curPositionValues;

                if (!_CurPositionValues.MoveNext())
                {
                    _CurPosition = -1;
                }
                else
                {
                    _CurPosition = _CurPositionValues.Current;
                }
            }

            public void IncCurIndex()
            {
                CurIndex++;

                if (CurIndex >= _WordIndex.Count)
                {
                    CurIndex = -1;
                }
            }
        }

        string _FieldName;
        Hubble.Core.Index.InvertedIndex _InvertedIndex;
        AppendList<Entity.WordInfo> _QueryWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        AppendList<Entity.WordInfo> _HitWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        Dictionary<string, int> _WordIndexDict = new Dictionary<string, int>();
        AppendList<WordIndexForQuery> _WordIndexList = new AppendList<WordIndexForQuery>();

        AppendList<WordIndexForQuery> _TempSelect = new AppendList<WordIndexForQuery>();
        AppendList<int> _TempPositionList = new AppendList<int>();

        private IList<Entity.WordInfo> GetNextHitWords(out long docId)
        {
            _HitWords.Clear();

            if (_QueryWords.Count <= 0)
            {
                docId = -1;
                return _HitWords;
            }

            long minDocId = long.MaxValue;

            //Get min document id
            for (int i = 0; i < _WordIndexList.Count; i++)
            {
                long curDocId = _WordIndexList[i].CurDocumentId;

                if (curDocId < 0)
                {
                    continue;
                }

                if (minDocId > curDocId)
                {
                    minDocId = curDocId;
                }
            }

            if (minDocId == long.MaxValue)
            {
                docId = -1;
                return _HitWords;
            }

            //Put min doc id into WordIndexes
            _TempSelect.Clear();
            for (int i = 0; i < _WordIndexList.Count; i++)
            {
                long curDocId = _WordIndexList[i].CurDocumentId;

                if (curDocId == minDocId)
                {
                    _TempSelect.Add(_WordIndexList[i]);
                    _WordIndexList[i].SetCurPositionValues(
                        _WordIndexList[i].WordIndex[_WordIndexList[i].CurIndex].Values);
                }
            }

            docId = minDocId;

            while (true)
            {
                int minPos = int.MaxValue;
                int repeatTimes = 0;
                int minIndex = -1;

                for (int i = 0; i < _TempSelect.Count; i++)
                {
                    int curPos = _TempSelect[i].CurPosition;
                    if (curPos >= 0)
                    {
                        if (minPos > curPos)
                        {
                            minPos = curPos;
                            repeatTimes = 1;
                            minIndex = i;
                        }
                        else if (minPos == curPos)
                        {
                            repeatTimes++;
                        }
                    }
                }

                //Get min position list
                _TempPositionList.Clear();

                if (repeatTimes == 1)
                {
                    _TempPositionList.Add(minIndex);
                }
                else if (repeatTimes > 1)
                {
                    for (int i = 0; i < _TempSelect.Count; i++)
                    {
                        int curPos = _TempSelect[i].CurPosition;

                        if (curPos == minPos)
                        {
                            _TempPositionList.Add(i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _WordIndexList.Count; i++)
                    {
                        long curDocId = _WordIndexList[i].CurDocumentId;

                        if (curDocId == minDocId)
                        {
                            _WordIndexList[i].IncCurIndex();
                        }
                    }

                    return _HitWords;
                }


                foreach (int index in _TempPositionList)
                {
                    WordIndexForQuery indexForQuery = _TempSelect[index];
                    int curIndex = indexForQuery.CurIndex;

                    _HitWords.Add(new Hubble.Core.Entity.WordInfo(indexForQuery.WordIndex.Word,
                        minPos));

                    if (_TempSelect[index].CurPositionValues.MoveNext())
                    {
                        _TempSelect[index].CurPosition = _TempSelect[index].CurPositionValues.Current;
                    }
                    else
                    {
                        _TempSelect[index].CurPosition = -1;
                    }
                }

            }

        }

        private int CaculateRank(IList<Entity.WordInfo> wordInfoList)
        {
            long rank = 0;

            foreach (Entity.WordInfo wordInfo in wordInfoList)
            {
                rank += wordInfo.Rank <= 0 ? 1 : wordInfo.Rank;
            }

            if (rank > int.MaxValue - 4000000)
            {
                return int.MaxValue - 4000000;
            }
            else
            {
                return (int)rank;
            }

        }


        #region IQuery Members

        public string FieldName
        {
            get
            {
                return _FieldName;
            }

            set
            {
                _FieldName = value;
            }
        }

        public Hubble.Core.Index.InvertedIndex InvertedIndex
        {
            get
            {
                return _InvertedIndex;
            }

            set
            {
                _InvertedIndex = value;
            }
        }

        public IList<Hubble.Core.Entity.WordInfo> QueryWords
        {
            get
            {
                return _QueryWords;
            }

            set
            {
                _QueryWords.Clear();
                _WordIndexList.Clear();
                _WordIndexDict.Clear();

                foreach (Hubble.Core.Entity.WordInfo wordInfo in value)
                {
                    _QueryWords.Add(wordInfo);

                    if (!_WordIndexDict.ContainsKey(wordInfo.Word))
                    {
                        _WordIndexList.Add(new WordIndexForQuery(InvertedIndex.GetWordIndex(wordInfo.Word)));
                        _WordIndexDict.Add(wordInfo.Word, _WordIndexList.Count - 1);
                    }
                }

                _QueryWords.Sort();
            }
        }


        public IEnumerable<DocumentRank> GetRankEnumerable()
        {
            long docId;
            IList<Entity.WordInfo> wordInfoList = GetNextHitWords(out docId);

            while (docId >= 0)
            {
                yield return new DocumentRank(docId, CaculateRank(wordInfoList));
                wordInfoList = GetNextHitWords(out docId);
            }
        }

        #endregion
    }
}
