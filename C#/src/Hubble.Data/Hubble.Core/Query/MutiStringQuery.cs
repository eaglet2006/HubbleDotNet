/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;
using Hubble.Core.Data;

namespace Hubble.Core.Query
{
    /// <summary>
    /// This query analyze input words just using
    /// tf/idf. The poisition informations are no useful.
    /// Syntax: MutiStringQuery('xxx','yyy','zzz')
    /// </summary>
    public class MutiStringQuery : IQuery
    {
        class WordIndexForQuery
        {
            private int _CurIndex;
            private int _OldIndexForDoc = -1;
            private int _OldIndexForWordCount = -1;
            private long _CurDocmentId;
            private int _CurWordCount;

            private Index.InvertedIndex.WordIndexReader _WordIndex;
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

            public Index.InvertedIndex.WordIndexReader WordIndex
            {
                get
                {
                    return _WordIndex;
                }
            }

            public WordIndexForQuery(Index.InvertedIndex.WordIndexReader wordIndex, long totalDocuments)
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
        private int _TabIndex;
        private DBProvider _DBProvider;

        AppendList<Entity.WordInfo> _QueryWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        AppendList<WordIndexForQuery> _WordIndexList = new AppendList<WordIndexForQuery>();

        AppendList<WordIndexForQuery> _TempSelect = new AppendList<WordIndexForQuery>();

        long _Norm_Ranks = 0; //sqrt(sum_t(rank)^2))

        private int CaculateRank(long docId, WordIndexForQuery wq)
        {
            //int numDocWords = InvertedIndex.GetDocumentWordCount(docId);

            int numDocWords = _DBProvider.GetDocWordsCount(docId, TabIndex);

            long rank = wq.Rank * wq.Idf_t * wq.CurWordCount * 1000000 / (_Norm_Ranks * wq.Norm_d_t * numDocWords);

            if (rank > int.MaxValue - 4000000)
            {
                long high = rank % (int.MaxValue - 4000000);

                return int.MaxValue - 4000000 + (int)(high / 1000);
            }
            else
            {
                return (int)rank;
            }

        }

        private int CaculateRank(long docId)
        {
            long rank = 0;

            //int numDocWords = InvertedIndex.GetDocumentWordCount(docId);

            int numDocWords = _DBProvider.GetDocWordsCount(docId, TabIndex);

            foreach (WordIndexForQuery wq in _TempSelect)
            {
                rank += wq.Rank * wq.Idf_t * wq.CurWordCount * 1000000 / (_Norm_Ranks * wq.Norm_d_t * numDocWords);
            }

            if (rank > int.MaxValue - 4000000)
            {
                long high = rank % (int.MaxValue - 4000000);

                return int.MaxValue - 4000000 + (int)(high / 1000);
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
            if (_QueryWords.Count <= 0)
            {
                return new DocumentRank(-1);
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
                return new DocumentRank(-1);
            }

            //Put min doc id into _TempSelect
            _TempSelect.Clear();
            for (int i = 0; i < _WordIndexList.Count; i++)
            {
                long curDocId = _WordIndexList[i].CurDocumentId;

                if (curDocId == minDocId)
                {
                    _TempSelect.Add(_WordIndexList[i]);
                }
            }

            int rank = CaculateRank(minDocId);

            foreach(WordIndexForQuery wq in _TempSelect)
            {
                wq.IncCurIndex();
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

        public int TabIndex
        {
            get
            {
                return _TabIndex;
            }
            set
            {
                _TabIndex = value;
            }
        }

        public string Command
        {
            get 
            {
                return "Muti";
            }
        }

        public DBProvider DBProvider
        {
            get
            {
                return _DBProvider;
            }
            set
            {
                _DBProvider = value;
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

                _QueryWords.Clear();
                _WordIndexList.Clear();
                wordIndexDict.Clear();

                foreach (Hubble.Core.Entity.WordInfo wordInfo in value)
                {
                    _QueryWords.Add(wordInfo);

                    if (!wordIndexDict.ContainsKey(wordInfo.Word))
                    {
                     
                        Hubble.Core.Index.InvertedIndex.WordIndexReader wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word);

                        if (wordIndex == null)
                        {
                            continue;
                        }

                        _WordIndexList.Add(new WordIndexForQuery(wordIndex,
                            InvertedIndex.DocumentCount));
                        wordIndexDict.Add(wordInfo.Word, 0);
                    }

                    _WordIndexList[_WordIndexList.Count - 1].Rank += wordInfo.Rank;
                }

                _Norm_Ranks = 0;
                foreach (WordIndexForQuery wq in _WordIndexList)
                {
                    _Norm_Ranks += wq.Rank * wq.Rank;
                }

                _Norm_Ranks = (long)Math.Sqrt(_Norm_Ranks);
            }
        }

        public Dictionary<long, DocumentRank> Search()
        {
            Dictionary<long, DocumentRank> result = new Dictionary<long, DocumentRank>();
            
            if (_QueryWords.Count <= 0)
            {
                return result;
            }

            //Get min document id
            for (int i = 0; i < _WordIndexList.Count; i++)
            {
                long curDocId = _WordIndexList[i].CurDocumentId;

                while (curDocId >= 0)
                {
                    int rank = CaculateRank(curDocId, _WordIndexList[i]);

                    DocumentRank docRank;

                    if (result.TryGetValue(curDocId, out docRank))
                    {
                        docRank.Rank += rank;
                    }
                    else
                    {
                        docRank = new DocumentRank(curDocId, rank);
                        result.Add(curDocId, docRank);
                    }

                    _WordIndexList[i].IncCurIndex();
                    curDocId = _WordIndexList[i].CurDocumentId;
                }

            }
            
            return result;
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
