using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Query
{
    /// <summary>
    /// This query analyze input words just using
    /// tf/idf. The poisition informations are no useful.
    /// Syntax: MutiStringQuery('xxx','yyy','zzz')
    /// </summary>
    public class MutiStringQuery : IQuery
    {
        class LongComparer : IComparer<long>
        {
            #region IComparer<long> Members

            public int Compare(long x, long y)
            {
                return x.CompareTo(y);
            }

            #endregion
        }

        class WordIndexForQuery
        {
            private int _CurIndex;
            private int _OldIndexForDoc = -1;
            private int _OldIndexForWordCount = -1;
            private long _CurDocmentId;
            private int _CurWordCount;

            private Index.InvertedIndex.WordIndex _WordIndex;
            private int _Rank;
            private int _Norm_d_t;
            private int _Idf_t;

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

            public int Norm_d_t
            {
                get
                {
                    return _Norm_d_t;
                }
            }

            public int Idf_t
            {
                get
                {
                    return _Idf_t;
                }
            }

            public int CurWordCount
            {
                get
                {
                    //Because WordIndex.[CurIndex].Count excute slowly.
                    if (CurIndex == _OldIndexForWordCount)
                    {
                        return _CurWordCount;
                    }
                    else
                    {
                        _OldIndexForWordCount = CurIndex;
                    }

                    _CurWordCount = WordIndex[CurIndex].Count;
                    return _CurWordCount;
                }
            }

            public int Rank
            {
                get
                {
                    if (_Rank <= 0)
                    {
                        return 1;
                    }

                    return _Rank;
                }

                set
                {
                    _Rank = value;
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
                        //Because WordIndex.GetDocumentId excute slowly.
                        if (CurIndex == _OldIndexForDoc)
                        {
                            return _CurDocmentId;
                        }
                        else
                        {
                            _OldIndexForDoc = CurIndex;
                        }

                        _CurDocmentId = WordIndex.GetDocumentId(CurIndex);
                        return _CurDocmentId;
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

            public WordIndexForQuery(Index.InvertedIndex.WordIndex wordIndex, long totalDocuments)
            {
                _OldIndexForDoc = -1;
                _OldIndexForWordCount = -1;

                if (wordIndex.Count <= 0)
                {
                    _CurIndex = -1;
                }
                else
                {
                    _CurIndex = 0;
                }

                _WordIndex = wordIndex;

                _Norm_d_t = (int)Math.Sqrt(_WordIndex.WordCount);
                _Idf_t = (int)Math.Log10((double)totalDocuments / (double)_WordIndex.Count + 1) + 1;
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

        AppendList<WordIndexForQuery> _IncWordIndexList = new AppendList<WordIndexForQuery>();

        SingleSortedLinkedTable<long, WordIndexForQuery> _TempSelect = 
            new SingleSortedLinkedTable<long, WordIndexForQuery>(new LongComparer());

        long _Norm_Ranks = 0; //sqrt(sum_t(rank)^2))

        private int CaculateRank(long docId)
        {
            long rank = 0;

            int numDocWords = InvertedIndex.GetDocumentWordCount(docId);

            foreach (SingleSortedLinkedTable<long, WordIndexForQuery>.Node wqNode in _TempSelect.GetFirstKeys())
            {
                WordIndexForQuery wq = wqNode.Value;
                rank += wq.Rank * wq.Idf_t * wq.CurWordCount * 1000000 / (_Norm_Ranks * wq.Norm_d_t * numDocWords);
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

        /// <summary>
        /// Get next document rank
        /// </summary>
        /// <returns>
        /// document rank.
        /// The end flag is that documentid equal -1
        /// </returns>
        private Query.DocumentRank GetNexDocumentRank()
        {
            if (_TempSelect.IsEmpty)
            {
                return new DocumentRank(-1);
            }

            long minDocId = _TempSelect.First.Key;
            int rank = CaculateRank(minDocId);

            _IncWordIndexList.Clear();

            foreach (SingleSortedLinkedTable<long, WordIndexForQuery>.Node wqNode in _TempSelect.GetFirstKeys())
            {
                wqNode.Value.IncCurIndex();
                _IncWordIndexList.Add(wqNode.Value);
            }

            _TempSelect.RemoveFirstKeys();

            foreach (WordIndexForQuery wq in _IncWordIndexList)
            {
                long curDocId = wq.CurDocumentId;

                if (curDocId < 0)
                {
                    continue;
                }

                _TempSelect.Add(curDocId, wq);
            }

            return new DocumentRank(minDocId, rank);
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
                Dictionary<string, int> wordIndexDict = new Dictionary<string, int>();
                AppendList<WordIndexForQuery> wordIndexList = new AppendList<WordIndexForQuery>();

                _QueryWords.Clear();
                wordIndexList.Clear();
                wordIndexDict.Clear();

                foreach (Hubble.Core.Entity.WordInfo wordInfo in value)
                {
                    _QueryWords.Add(wordInfo);

                    if (!wordIndexDict.ContainsKey(wordInfo.Word))
                    {
                     
                        Hubble.Core.Index.InvertedIndex.WordIndex wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word);

                        if (wordIndex == null)
                        {
                            continue;
                        }

                        wordIndexList.Add(new WordIndexForQuery(wordIndex,
                            InvertedIndex.DocumentCount));
                        wordIndexDict.Add(wordInfo.Word, 0);
                    }

                    wordIndexList[wordIndexList.Count - 1].Rank += wordInfo.Rank;
                }

                _Norm_Ranks = 0;
                foreach (WordIndexForQuery wq in wordIndexList)
                {
                    _Norm_Ranks += wq.Rank * wq.Rank;
                }

                _Norm_Ranks = (long)Math.Sqrt(_Norm_Ranks);

                //Init _TempSelect
                if (_QueryWords.Count <= 0)
                {
                    return;
                }

                _TempSelect.Clear();

                //Get min document id
                for (int i = 0; i < wordIndexList.Count; i++)
                {
                    long curDocId = wordIndexList[i].CurDocumentId;

                    if (curDocId < 0)
                    {
                        continue;
                    }

                    _TempSelect.Add(curDocId, wordIndexList[i]);
                }
            }
        }

        public IEnumerable<DocumentRank> GetRankEnumerable()
        {
            Query.DocumentRank docRank = GetNexDocumentRank();

            while (docRank.DocumentId >= 0)
            {
                yield return docRank;
                docRank = GetNexDocumentRank();
            }

        }

        #endregion
    }
}
