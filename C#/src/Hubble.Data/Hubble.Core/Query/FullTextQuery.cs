using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;

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
        Dictionary<string, List<Entity.WordInfo>> _WordQueryWordTable = new Dictionary<string, List<Entity.WordInfo>>();

        AppendList<Entity.WordInfo> _HitWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        Dictionary<string, int> _WordIndexDict = new Dictionary<string, int>();
        AppendList<WordIndexForQuery> _WordIndexList = new AppendList<WordIndexForQuery>();

        AppendList<WordIndexForQuery> _TempSelect = new AppendList<WordIndexForQuery>();
        AppendList<int> _TempPositionList = new AppendList<int>();

        private int _QueryStringLength = 0;

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

        readonly static int[] RadixRank = {
        100, 98, 90,  80, 70, 60, 50, 45,
        40, 38, 36, 34, 32, 30, 28, 26,
        20, 18, 16, 14, 12, 11, 10, 9,
        8, 7, 6, 5, 4, 3, 2, 1,
        };

        readonly static int[] ContinueTimesBase = {
        1, 1, 10, 30, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900
        };

        /// <summary>
        /// Find same position wordinfos and 
        /// cacluate their rank sum.
        /// </summary>
        /// <param name="wordInfoList">wordinfo list</param>
        /// <param name="start">start index in wordInfoList</param>
        /// <param name="end">end index in wordInfoList</param>
        /// <param name="lastMinPosition">min position in query words that last time this function has been called.</param>
        /// <returns>if no result, return -1</returns>
        private int CaculateRankSamePosition(IList<Entity.WordInfo> wordInfoList,
            int start, int end, ref int lastMinPosition, out int distance, out int maxEndPosition)
        {
            int minPosition = int.MaxValue;
            int minWordInfoRank = 1;
            int rank = 0;
            distance = 31;
            maxEndPosition = 0;

            if (end < wordInfoList.Count && start >= 0)
            {
                for (int i = start; i <= end; i++)
                {
                    List<Entity.WordInfo> queryWords;

                    maxEndPosition = Math.Max(maxEndPosition, wordInfoList[i].Position + wordInfoList[i].Word.Length - 1);

                    if (!_WordQueryWordTable.TryGetValue(wordInfoList[i].Word, out queryWords))
                    {
                        continue;
                    }

                    foreach (Entity.WordInfo queryWordInfo in queryWords)
                    {
                        if (queryWordInfo.Position > lastMinPosition)
                        {
                            int curPosition = queryWordInfo.Position;

                            if (curPosition < minPosition)
                            {
                                minPosition = queryWordInfo.Position;
                                minWordInfoRank = queryWordInfo.Rank;
                            }

                            rank += queryWordInfo.Rank <= 0 ? 1 : queryWordInfo.Rank;
                        }
                    }
                }
            }

            if (minPosition == int.MaxValue)
            {
                return -1;
            }

            if (lastMinPosition < 0 || start == 0)
            {
                //First word when scan
                lastMinPosition = minPosition;

                return rank;
            }
            else
            {
                distance = Math.Abs((minPosition - lastMinPosition) -
                    (wordInfoList[start].Position - wordInfoList[start - 1].Position));

                if (distance > 31)
                {
                    distance = 31;
                }

                //rank += minWordInfoRank <= 0 ? RadixRank[distance] : RadixRank[distance] * minWordInfoRank;
                rank += RadixRank[distance] * (end + 1 - start);
                lastMinPosition = minPosition;

                return rank;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordInfoList"></param>
        /// <returns></returns>
        private int CaculateRank(IList<Entity.WordInfo> wordInfoList)
        {
            if (wordInfoList.Count <= 0)
            {
                return 0;
            }

            long rank = 0;

            int lastMinPosition = -1; //Last min position

            int start = 0;
            int end = 1;
            int lastScanPostition = wordInfoList[0].Position;
            int continueTimes = 0;
            int distance;
            int fstStart = 0;
            int maxEndPosition = 0;
            int lastMaxEndPosition = 0;

            while (end < wordInfoList.Count)
            {
                if (wordInfoList[end].Position != lastScanPostition)
                {
                    int r = CaculateRankSamePosition(wordInfoList, start, end - 1, ref lastMinPosition, out distance, out maxEndPosition);

                    if (r < 0)
                    {
                        //Scan to end of query words
                        //backdate to the begining
                        continueTimes = 1;
                        lastMinPosition = -1;
                        fstStart = start;
                        r = CaculateRankSamePosition(wordInfoList, start, end - 1, ref lastMinPosition, out distance, out maxEndPosition);
                    }
                    else
                    {
                        if (distance < 8 && lastScanPostition > lastMaxEndPosition)
                        {
                            continueTimes++;
                        }
                    }

                    if (r > 0)
                    {
                        if (continueTimes >= ContinueTimesBase.Length)
                        {
                            continueTimes = ContinueTimesBase.Length - 1;
                        }

                        if ((wordInfoList[start].Position + wordInfoList[start].Word.Length - 
                            wordInfoList[fstStart].Position ==  _QueryStringLength) &&
                            end - fstStart == _QueryWords.Count)
                        {
                            rank += 5000 * r;
                        }
                        else
                        {
                            rank += ContinueTimesBase[continueTimes] * r;
                        }
                    }

                    start = end;
                }

                lastScanPostition = wordInfoList[end].Position;
                lastMaxEndPosition = maxEndPosition;
                end++;
            }

            //deal with end word
            int r_End = CaculateRankSamePosition(wordInfoList, start, end - 1, ref lastMinPosition, out distance, out maxEndPosition);

            if (r_End < 0)
            {
                //Scan to end of query words
                //backdate to the begining
                lastMinPosition = -1;
                r_End = CaculateRankSamePosition(wordInfoList, start, end - 1, ref lastMinPosition, out distance, out maxEndPosition);
            }

            if (r_End > 0)
            {
                if (continueTimes >= ContinueTimesBase.Length)
                {
                    continueTimes = ContinueTimesBase.Length - 1;
                }

                rank += ContinueTimesBase[continueTimes] * r_End;
            }
            //end deal with end word

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
                        Hubble.Core.Index.InvertedIndex.WordIndex wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word);

                        if (wordIndex == null)
                        {
                            continue;
                        }

                        _WordIndexList.Add(new WordIndexForQuery(wordIndex));
                        _WordIndexDict.Add(wordInfo.Word, _WordIndexList.Count - 1);
                    }
                }

                _QueryWords.Sort();
                bool fst = true;
                int fstPosition = 0;
                //Init _PositionQueryWordTable
                //This table use to caculate the document rank
                foreach (Entity.WordInfo wordInfo in _QueryWords)
                {
                    if (fst)
                    {
                        fstPosition = wordInfo.Position;
                        fst = false;
                    }

                    _QueryStringLength = Math.Max(_QueryStringLength, wordInfo.Word.Length + wordInfo.Position - fstPosition);

                    if (!_WordQueryWordTable.ContainsKey(wordInfo.Word))
                    {
                        _WordQueryWordTable.Add(wordInfo.Word,
                            new List<Hubble.Core.Entity.WordInfo>());
                    }

                    _WordQueryWordTable[wordInfo.Word].Add(wordInfo);
                }
            }
        }


        public IEnumerable<DocumentRank> GetRankEnumerable()
        {
            long docId;

            //GetNextHitWords has returned the word info list sorted by position 
            IList<Entity.WordInfo> wordInfoList = GetNextHitWords(out docId);

            while (docId >= 0)
            {
                if (docId == 971)
                {
                    docId = 971;
                }

                if (docId == 809)
                {
                    docId = 809;
                }


                yield return new DocumentRank(docId, CaculateRank(wordInfoList));
                wordInfoList = GetNextHitWords(out docId);
            }
        }

        #endregion
    }
}
