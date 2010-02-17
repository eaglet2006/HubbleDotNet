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
using Hubble.Core.SFQL.Parse;
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
using Hubble.Core.SFQL.Parse;

namespace Hubble.Core.Query
{
    /// <summary>
    /// This query analyze input words just using
    /// tf/idf. The poisition informations are no useful.
    /// Syntax: MutiStringQuery('xxx','yyy','zzz')
    /// </summary>
    public class FullTextQuery : IQuery, INamedExternalReference
    {
        class WordIndexForQuery : IComparable<WordIndexForQuery>
        {
            public int CurDocIdIndex;
            public int WordIndexesLength;
            public int Norm_d_t;
            public int Idf_t;
            public int WordRank;
            public int FieldRank;

            private int _CurIndex;
            private int _OldIndexForWordCount = -1;
            private int _CurWordCount;

            private Index.InvertedIndex.WordIndexReader _WordIndex;
            private int _Rank;

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
                    return _Rank;
                }

                set
                {
                    _Rank = value;

                    if (_Rank <= 0)
                    {
                        _Rank = 1;
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

            public WordIndexForQuery(Index.InvertedIndex.WordIndexReader wordIndex, int totalDocuments, int wordRank, int fieldRank)
            {
                _OldIndexForWordCount = -1;
                FieldRank = fieldRank;
                WordRank = wordRank;


                if (FieldRank <= 0)
                {
                    FieldRank = 1;
                }

                if (WordRank <= 0)
                {
                    WordRank = 1;
                }


                if (wordIndex.Count <= 0)
                {
                    _CurIndex = -1;
                }
                else
                {
                    _CurIndex = 0;
                }

                _WordIndex = wordIndex;

                Norm_d_t = (int)Math.Sqrt(_WordIndex.WordCount);
                Idf_t = (int)Math.Log10((double)totalDocuments / (double)_WordIndex.Count + 1) + 1;
                CurDocIdIndex = 0;
                WordIndexesLength = _WordIndex.Count;
            }

            public void IncCurIndex()
            {
                CurIndex++;

                if (CurIndex >= _WordIndex.Count)
                {
                    CurIndex = -1;
                }
            }


            #region IComparable<WordIndexForQuery> Members

            public int CompareTo(WordIndexForQuery other)
            {
                return this.WordIndexesLength.CompareTo(other.WordIndexesLength);
            }

            #endregion
        }

        #region Private fields
        string _FieldName;
        Hubble.Core.Index.InvertedIndex _InvertedIndex;
        private int _TabIndex;
        private DBProvider _DBProvider;

        long _Norm_Ranks = 0; //sqrt(sum_t(rank)^2))

        AppendList<Entity.WordInfo> _QueryWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        AppendList<WordIndexForQuery> _WordIndexList = new AppendList<WordIndexForQuery>();
        WordIndexForQuery[] _WordIndexes;


        #endregion

        private void Calculate(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            //Get max word doc list count
            int maxWordDocListCount = 0;
            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                if (maxWordDocListCount < wifq.WordIndex.WordDocList.Count)
                {
                    maxWordDocListCount = wifq.WordIndex.WordDocList.Count;
                }
            }

            maxWordDocListCount += maxWordDocListCount / 2;

            if (docIdRank.Count == 0)
            {
                docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(maxWordDocListCount);
            }

            //Merge
            bool beginFilter = false;

            for (int i = 0; i < wordIndexes.Length; i++)
            {
                WordIndexForQuery wifq = wordIndexes[i];

                if (docIdRank.Count > 32768)
                {
                    beginFilter = true;
                }

                for (int j = 0; j < wifq.WordIndexesLength; j++)
                {
                    Entity.DocumentPositionList docList = wifq.WordIndex[j];

                    if (beginFilter)
                    {
                        if (!docIdRank.ContainsKey(docList.DocumentId))
                        {
                            continue;
                        }
                    }

                    long rank = (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

                    int score = 0;

                    if (rank > int.MaxValue - 4000000)
                    {
                        long high = rank % (int.MaxValue - 4000000);

                        score = int.MaxValue - 4000000 + (int)(high / 1000);
                    }
                    else
                    {
                        score = (int)rank;
                    }

                    rank = (long)wifq.FieldRank * (long)wifq.WordRank * (long)score;

                    Query.DocumentResult docResult;

                    if (docIdRank.TryGetValue(docList.DocumentId, out docResult))
                    {
                        docResult.Score += rank;

                        docIdRank[docList.DocumentId] = docResult;
                    }
                    else
                    {

                        if (upDict == null)
                        {
                            docResult = new DocumentResult(docList.DocumentId, rank);
                            docIdRank.Add(docList.DocumentId, docResult);
                        }
                        else
                        {
                            if (!upDict.Not)
                            {
                                if (upDict.ContainsKey(docList.DocumentId))
                                {
                                    docResult = new DocumentResult(docList.DocumentId, rank);
                                    docIdRank.Add(docList.DocumentId, docResult);
                                }
                            }
                            else
                            {
                                if (!upDict.ContainsKey(docList.DocumentId))
                                {
                                    docResult = new DocumentResult(docList.DocumentId, rank);
                                    docIdRank.Add(docList.DocumentId, docResult);
                                }
                            }
                        }
                    }
                }
            }

            DeleteProvider delProvider = _DBProvider.DelProvider;
            delProvider.Filter(docIdRank);


            //for (int i = 0; i < _DocScoreList.Length; i++)
            //{
            //    DocScore docScore = _DocScoreList[i];
            //    if (docScore.DocId < 0)
            //    {
            //        continue;
            //    }

            //    long rank = docScore.Score;// / Norm_Ranks;
            //    int score = 0;

            //    if (rank > int.MaxValue - 4000000)
            //    {
            //        long high = rank % (int.MaxValue - 4000000);

            //        score = int.MaxValue - 4000000 + (int)(high / 1000);
            //    }
            //    else
            //    {
            //        score = (int)rank;
            //    }

            //    rank = (long)fieldRank * (long)wordRank * (long)score;

            //    Query.DocumentResult docResult;

            //    if (docIdRank.TryGetValue(docScore.DocId, out docResult))
            //    {
            //        //docResult.Score += score;
            //        docResult.Score += rank;
            //        docIdRank[docScore.DocId] = docResult;
            //    }
            //    else
            //    {
            //        //docResult = new Query.DocumentResult(docScore.DocId, score);

            //        if (upDict == null)
            //        {
            //            docResult = new Query.DocumentResult(docScore.DocId, rank);
            //            docIdRank.Add(docScore.DocId, docResult);
            //        }
            //        else
            //        {
            //            if (!upDict.Not)
            //            {
            //                if (upDict.ContainsKey(docScore.DocId))
            //                {
            //                    docResult = new Query.DocumentResult(docScore.DocId, rank);
            //                    docIdRank.Add(docScore.DocId, docResult);
            //                }
            //            }
            //            else
            //            {
            //                if (!upDict.ContainsKey(docScore.DocId))
            //                {
            //                    docResult = new Query.DocumentResult(docScore.DocId, rank);
            //                    docIdRank.Add(docScore.DocId, docResult);
            //                }
            //            }
            //        }
            //    }
            //}


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
                return "Like";
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

        private int _FieldRank = 1;
        public int FieldRank
        {
            get
            {
                return _FieldRank;
            }
            set
            {
                _FieldRank = value;
                if (_FieldRank <= 0)
                {
                    _FieldRank = 1;
                }
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
#if PerformanceTest
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                sw.Reset();
                sw.Start();
#endif

                Dictionary<string, int> wordIndexDict = new Dictionary<string, int>();

                _QueryWords.Clear();
                _WordIndexList.Clear();
                wordIndexDict.Clear();

                List<WordIndexForQuery> wordIndexList = new List<WordIndexForQuery>(value.Count);


                foreach (Hubble.Core.Entity.WordInfo wordInfo in value)
                {
                    _QueryWords.Add(wordInfo);

                    if (!wordIndexDict.ContainsKey(wordInfo.Word))
                    {

                        Hubble.Core.Index.InvertedIndex.WordIndexReader wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, CanLoadPartOfDocs);

                        if (wordIndex == null)
                        {
                            continue;
                        }

                        wordIndexList.Add(new WordIndexForQuery(wordIndex,
                            InvertedIndex.DocumentCount, wordInfo.Rank, this.FieldRank));
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

                _WordIndexes = new WordIndexForQuery[wordIndexList.Count];
                wordIndexList.CopyTo(_WordIndexes, 0);
                wordIndexList = null;

#if PerformanceTest
                sw.Stop();

                Console.WriteLine("QueryWords elapse:" + sw.ElapsedMilliseconds + "ms");

#endif
            }
        }

        public Core.SFQL.Parse.DocumentResultWhereDictionary Search()
        {
#if PerformanceTest
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //int[] gcCounts = new int[GC.MaxGeneration + 1];
            //for (int i = 0; i <= GC.MaxGeneration; i++)
            //{
            //    gcCounts[i] = GC.CollectionCount(i);
            //}

            sw.Reset();
            sw.Start();
#endif

            Core.SFQL.Parse.DocumentResultWhereDictionary result = new Core.SFQL.Parse.DocumentResultWhereDictionary();

            if (_QueryWords.Count <= 0)
            {
                return result;
            }

            if (this.Not)
            {
                Calculate(null, ref result, _WordIndexes);
            }
            else
            {
                Calculate(this.UpDict, ref result, _WordIndexes);
            }

            //Get min document id
            //for (int i = 0; i < _WordIndexList.Count; i++)
            //{
            //    if (this.Not)
            //    {
            //        _WordIndexList[i].Calculate(null, ref result, _Norm_Ranks);
            //    }
            //    else
            //    {
            //        _WordIndexList[i].Calculate(this.UpDict, ref result, _Norm_Ranks);
            //    }
            //}

            if (this.Not)
            {
                result.Not = true;

                if (UpDict != null)
                {
                    result = result.AndMerge(result, UpDict);
                }
            }

#if PerformanceTest
            sw.Stop();

            Console.WriteLine("Search elapse:" + sw.ElapsedMilliseconds + "ms");

            //for (int i = 0; i <= GC.MaxGeneration; i++)
            //{
            //    int count = GC.CollectionCount(i) - gcCounts[i];
            //    Console.WriteLine("\tGen " + i + ": \t\t" + count);
            //}

#endif
            return result;
        }

        Core.SFQL.Parse.DocumentResultWhereDictionary _UpDict;

        public Core.SFQL.Parse.DocumentResultWhereDictionary UpDict
        {
            get
            {
                return _UpDict;
            }
            set
            {
                _UpDict = value;
            }
        }

        bool _Not = false;

        public bool Not
        {
            get
            {
                return _Not;
            }
            set
            {
                _Not = value;
            }
        }

        bool _CanLoadPartOfDocs;

        public bool CanLoadPartOfDocs
        {
            get
            {
                return _CanLoadPartOfDocs;
            }
            set
            {
                _CanLoadPartOfDocs = value;
            }
        }

        bool _NoAndExpression = false;

        public bool NoAndExpression
        {
            get
            {
                return _NoAndExpression;
            }
            set
            {
                _NoAndExpression = value;
            }
        }
        #endregion


        #region INamedExternalReference Members

        public string Name
        {
            get
            {
                return Command;
            }
        }

        #endregion
    }
}
