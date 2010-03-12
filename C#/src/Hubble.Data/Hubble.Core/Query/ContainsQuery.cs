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
    public class ContainsQuery : IQuery, INamedExternalReference
    {
        class WordIndexForQuery : IComparable<WordIndexForQuery>
        {
            public int CurDocIdIndex;
            public int WordIndexesLength;
            public int Norm_d_t;
            public int Idf_t;
            public int WordRank;
            public int FieldRank;
            public int RelTotalCount;

            public int QueryCount; //How many time is this word in query string.
            public int FirstPosition; //First position in query string.

            private int _CurIndex;

            private Index.InvertedIndex.WordIndexReader _WordIndex;

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


            public Index.InvertedIndex.WordIndexReader WordIndex
            {
                get
                {
                    return _WordIndex;
                }
            }

            public WordIndexForQuery(Index.InvertedIndex.WordIndexReader wordIndex,
                int totalDocuments, int wordRank, int fieldRank)
            {
                FieldRank = fieldRank;
                WordRank = wordRank;
                RelTotalCount = wordIndex.RelDocCount;

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
        const int MinResultCount = 32768;

        string _FieldName;
        Hubble.Core.Index.InvertedIndex _InvertedIndex;
        private int _TabIndex;
        private DBProvider _DBProvider;
        private int _TotalDocuments;

        AppendList<Entity.WordInfo> _QueryWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        WordIndexForQuery[] _WordIndexes;

        long _Norm_Ranks = 0; //sqrt(sum_t(rank)^2))

        #endregion

        unsafe private void CalculateWithPosition(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            //Get max word doc list count
            int minWordDocListCount = 4 * 1024 * 1024; //4M

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                minWordDocListCount = Math.Min(minWordDocListCount, wifq.WordIndex.WordDocList.Count);
            }

            if (docIdRank.Count == 0)
            {
                if (minWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(minWordDocListCount);
                }
            }

#if PerformanceTest
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Reset();
            sw.Start();
#endif

            //Merge
            bool oneWordOptimize = this.CanLoadPartOfDocs && this.NoAndExpression && wordIndexes.Length == 1;
            int oneWordMaxCount = 0;
            double ratio = 1;
            
            if (wordIndexes.Length > 1)
            {
                ratio = (double)2 / (double)(wordIndexes.Length - 1);
            }

            if (oneWordOptimize)
            {
                //One word
                WordIndexForQuery wifq = wordIndexes[0];

                Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                for (int j = 0; j < wifq.WordIndexesLength; j++)
                {
                    //Entity.DocumentPositionList docList = wifq.WordIndex[j];
                    Entity.DocumentPositionList docList = wifqDocBuf[j];

                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (j > MinResultCount)
                    {
                        if (oneWordMaxCount > docList.Count)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (oneWordMaxCount < docList.Count)
                        {
                            oneWordMaxCount = docList.Count;
                        }
                    }

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }


                    if (upDict == null)
                    {
                        docIdRank.Add(docList.DocumentId, score);
                    }
                    else
                    {
                        if (!upDict.Not)
                        {
                            if (upDict.ContainsKey(docList.DocumentId))
                            {
                                docIdRank.Add(docList.DocumentId, score);
                            }
                        }
                        else
                        {
                            if (!upDict.ContainsKey(docList.DocumentId))
                            {
                                docIdRank.Add(docList.DocumentId, score);
                            }
                        }
                    }

                }
            }
            else
            {
                int[] steps = new int [wordIndexes.Length];
                int[] curIndexes = new int[wordIndexes.Length];
                int curWord = 0;
                Entity.DocumentPositionList[][] wifqDocBufs =
                    new Hubble.Core.Entity.DocumentPositionList[wordIndexes.Length][];

                steps[0] = 1;
                wifqDocBufs[0] = wordIndexes[0].WordIndex.DocPositionBuf;

                for (int i = 1; i < steps.Length; i++)
                {
                    steps[i] = wordIndexes[i].WordIndexesLength / wordIndexes[i - 1].WordIndexesLength;
                    wifqDocBufs[i] = wordIndexes[i].WordIndex.DocPositionBuf;

                    if (steps[i] <= 0)
                    {
                        steps[i] = 1;
                    }
                }

                WordIndexForQuery fstWifq = wordIndexes[0];
                int wordIndexesLen = wordIndexes.Length;

                bool terminate = false;

                for (int j = 0; j < fstWifq.WordIndexesLength && !terminate; j++)
                {
                    curWord = 1;
                    int firstDocId = wifqDocBufs[0][j].DocumentId;

                    while (curWord < wordIndexesLen)
                    {
                        if (wifqDocBufs[curWord][curIndexes[curWord]].DocumentId > firstDocId)
                        {
                            //first doc id less then left
                            break;
                        }

                        if (wifqDocBufs[curWord][curIndexes[curWord]].DocumentId == firstDocId)
                        {
                            //Equal with left
                            curWord++;
                        }
                        else
                        {
                            //Large then left, begin step

                            int k = curIndexes[curWord] + steps[curWord];
                            int curWordListLen = wifqDocBufs[curWord].Length;
                            if (k >= curWordListLen)
                            {
                                k = wifqDocBufs[curWord].Length - 1;
                            }

                            while (wifqDocBufs[curWord][k].DocumentId < firstDocId)
                            {
                                k += steps[curWord];
                                if (k >= curWordListLen)
                                {
                                    k = curWordListLen - 1;
                                    if (wifqDocBufs[curWord][k].DocumentId < firstDocId)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (wifqDocBufs[curWord][k].DocumentId < firstDocId)
                            {
                                //End of list, break scan
                                terminate = true;
                                break;
                            }

                            if (wifqDocBufs[curWord][k].DocumentId == firstDocId)
                            {
                                //Equal with right
                                curIndexes[curWord] = k;
                                curWord++;
                                break;
                            }

                            k--;

                            int leftIndex = curIndexes[curWord];
                            bool continueNextWord = true;

                            while (k > leftIndex)
                            {
                                if (wifqDocBufs[curWord][k].DocumentId <= firstDocId)
                                {
                                    curIndexes[curWord] = k;

                                    if (wifqDocBufs[curWord][k].DocumentId == firstDocId)
                                    {
                                        //Equal
                                        curWord++;
                                        break;
                                    }
                                    else
                                    {
                                        //Not find
                                        continueNextWord = false;
                                        break;
                                    }

                                }
                                else
                                {
                                    k--;
                                }
                            }

                            if (k <= leftIndex)
                            {
                                //Not find
                                break;
                            }


                            if (!continueNextWord)
                            {
                                break;
                            }
                        }

                    } //While

                    if (curWord >= wordIndexesLen)
                    {
                        //Matched
                        //Caculate score

                        long totalScore = 0;
                        Entity.DocumentPositionList lastDocList 
                            = new Hubble.Core.Entity.DocumentPositionList();

                        for (int i = 0; i < wordIndexesLen; i++)
                        {
                            WordIndexForQuery wifq = wordIndexes[i];
                            Entity.DocumentPositionList docList;

                            if (i == 0)
                            {
                                docList = wifqDocBufs[i][j];
                            }
                            else
                            {
                                docList = wifqDocBufs[i][curIndexes[i]];
                            }

                            long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

                            if (score < 0)
                            {
                                //Overflow
                                score = long.MaxValue - 4000000;
                            }

                            double delta = 1;

                            if (i > 0)
                            {
                                //Calculate with position
                                double queryPositionDelta = wifq.FirstPosition - wordIndexes[i-1].FirstPosition;
                                double positionDelta = docList.FirstPosition - lastDocList.FirstPosition;

                                delta = Math.Abs(queryPositionDelta - positionDelta);

                                if (delta < 0.031)
                                {
                                    delta = 0.031;
                                }
                                else if (delta <= 1.1)
                                {
                                    delta = 0.5;
                                }
                                else if (delta <= 2.1)
                                {
                                    delta = 1;
                                }

                                delta = Math.Pow((1 / delta), ratio) * docList.Count * lastDocList.Count /
                                    (double)(wifq.QueryCount * wordIndexes[i-1].QueryCount);
                            }

                            lastDocList = docList;

                            totalScore += (long)(score * delta);
                        }

                        if (upDict == null)
                        {
                            docIdRank.Add(firstDocId, totalScore);
                        }
                        else
                        {
                            if (!upDict.Not)
                            {
                                if (upDict.ContainsKey(firstDocId))
                                {
                                    docIdRank.Add(firstDocId, totalScore);
                                }
                            }
                            else
                            {
                                if (!upDict.ContainsKey(firstDocId))
                                {
                                    docIdRank.Add(firstDocId, totalScore);
                                }
                            }
                        }

                        
                        for (int i = 1; i < wordIndexesLen; i++)
                        {
                            curIndexes[i]++;
                        }
                    }

                }
            }

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int delCount = delProvider.Filter(docIdRank);

            if (oneWordOptimize && CanLoadPartOfDocs && upDict == null)
            {
                docIdRank.RelTotalCount = wordIndexes[0].RelTotalCount - delCount;
            }
            else
            {
                docIdRank.RelTotalCount = docIdRank.Count;
            }


#if PerformanceTest
            sw.Stop();

            Console.WriteLine(string.Format("Calculate {0} ms", sw.ElapsedMilliseconds));

#endif
        
        }


        unsafe private void Calculate(DocumentResultWhereDictionary upDict,
            ref DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            //Get max word doc list count
            int minWordDocListCount = 4 * 1024 * 1024; //4M
             
            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                minWordDocListCount = Math.Min(minWordDocListCount, wifq.WordIndex.WordDocList.Count);
            }

            if (docIdRank.Count == 0)
            {
                if (minWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(minWordDocListCount);
                }
            }

#if PerformanceTest
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Reset();
            sw.Start();
#endif

            //Merge
            bool oneWordOptimize = this.CanLoadPartOfDocs && this.NoAndExpression && wordIndexes.Length == 1;
            int oneWordMaxCount = 0;

            if (oneWordOptimize)
            {
                //One word
                WordIndexForQuery wifq = wordIndexes[0];

                Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                for (int j = 0; j < wifq.WordIndexesLength; j++)
                {
                    //Entity.DocumentPositionList docList = wifq.WordIndex[j];
                    Entity.DocumentPositionList docList = wifqDocBuf[j];

                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (j > MinResultCount)
                    {
                        if (oneWordMaxCount > docList.Count)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (oneWordMaxCount < docList.Count)
                        {
                            oneWordMaxCount = docList.Count;
                        }
                    }

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }


                    if (upDict == null)
                    {
                        docIdRank.Add(docList.DocumentId, score);
                    }
                    else
                    {
                        if (!upDict.Not)
                        {
                            if (upDict.ContainsKey(docList.DocumentId))
                            {
                                docIdRank.Add(docList.DocumentId, score);
                            }
                        }
                        else
                        {
                            if (!upDict.ContainsKey(docList.DocumentId))
                            {
                                docIdRank.Add(docList.DocumentId, score);
                            }
                        }
                    }

                }
            }
            else
            {
                int[] steps = new int [wordIndexes.Length];
                int[] curIndexes = new int[wordIndexes.Length];
                int curWord = 0;
                Entity.DocumentPositionList[][] wifqDocBufs =
                    new Hubble.Core.Entity.DocumentPositionList[wordIndexes.Length][];

                steps[0] = 1;
                wifqDocBufs[0] = wordIndexes[0].WordIndex.DocPositionBuf;

                for (int i = 1; i < steps.Length; i++)
                {
                    steps[i] = wordIndexes[i].WordIndexesLength / wordIndexes[i - 1].WordIndexesLength;
                    wifqDocBufs[i] = wordIndexes[i].WordIndex.DocPositionBuf;

                    if (steps[i] <= 0)
                    {
                        steps[i] = 1;
                    }
                }

                WordIndexForQuery fstWifq = wordIndexes[0];
                int wordIndexesLen = wordIndexes.Length;

                bool terminate = false;

                for (int j = 0; j < fstWifq.WordIndexesLength && !terminate; j++)
                {
                    curWord = 1;
                    int firstDocId = wifqDocBufs[0][j].DocumentId;

                    while (curWord < wordIndexesLen)
                    {
                        if (wifqDocBufs[curWord][curIndexes[curWord]].DocumentId > firstDocId)
                        {
                            //first doc id less then left
                            break;
                        }

                        if (wifqDocBufs[curWord][curIndexes[curWord]].DocumentId == firstDocId)
                        {
                            //Equal with left
                            curWord++;
                        }
                        else
                        {
                            //Large then left, begin step

                            int k = curIndexes[curWord] + steps[curWord];
                            int curWordListLen = wifqDocBufs[curWord].Length;
                            if (k >= curWordListLen)
                            {
                                k = wifqDocBufs[curWord].Length - 1;
                            }

                            while (wifqDocBufs[curWord][k].DocumentId < firstDocId)
                            {
                                k += steps[curWord];
                                if (k >= curWordListLen)
                                {
                                    k = curWordListLen - 1;
                                    if (wifqDocBufs[curWord][k].DocumentId < firstDocId)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (wifqDocBufs[curWord][k].DocumentId < firstDocId)
                            {
                                //End of list, break scan
                                terminate = true;
                                break;
                            }

                            if (wifqDocBufs[curWord][k].DocumentId == firstDocId)
                            {
                                //Equal with right
                                curIndexes[curWord] = k;
                                curWord++;
                                break;
                            }

                            k--;

                            int leftIndex = curIndexes[curWord];
                            bool continueNextWord = true;

                            while (k > leftIndex)
                            {
                                if (wifqDocBufs[curWord][k].DocumentId <= firstDocId)
                                {
                                    curIndexes[curWord] = k;

                                    if (wifqDocBufs[curWord][k].DocumentId == firstDocId)
                                    {
                                        //Equal
                                        curWord++;
                                        break;
                                    }
                                    else
                                    {
                                        //Not find
                                        continueNextWord = false;
                                        break;
                                    }

                                }
                                else
                                {
                                    k--;
                                }
                            }

                            if (k <= leftIndex)
                            {
                                //Not find
                                break;
                            }


                            if (!continueNextWord)
                            {
                                break;
                            }
                        }

                    } //While

                    if (curWord >= wordIndexesLen)
                    {
                        //Matched

                        long totalScore = 0;
                        for (int i = 0; i < wordIndexesLen; i++)
                        {
                            WordIndexForQuery wifq = wordIndexes[i];
                            Entity.DocumentPositionList docList;

                            if (i == 0)
                            {
                                docList = wifqDocBufs[i][j];
                            }
                            else
                            {
                                docList = wifqDocBufs[i][curIndexes[i]];
                            }

                            long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

                            if (score < 0)
                            {
                                //Overflow
                                score = long.MaxValue - 4000000;
                            }

                            totalScore += score;
                        }

                        if (upDict == null)
                        {
                            docIdRank.Add(firstDocId, totalScore);
                        }
                        else
                        {
                            if (!upDict.Not)
                            {
                                if (upDict.ContainsKey(firstDocId))
                                {
                                    docIdRank.Add(firstDocId, totalScore);
                                }
                            }
                            else
                            {
                                if (!upDict.ContainsKey(firstDocId))
                                {
                                    docIdRank.Add(firstDocId, totalScore);
                                }
                            }
                        }

                        for (int i = 1; i < wordIndexesLen; i++)
                        {
                            curIndexes[i]++;
                        }
                    }

                }
            }

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int delCount = delProvider.Filter(docIdRank);

            if (oneWordOptimize && CanLoadPartOfDocs && upDict == null)
            {
                docIdRank.RelTotalCount = wordIndexes[0].RelTotalCount - delCount;
            }
            else
            {
                docIdRank.RelTotalCount = docIdRank.Count;
            }


#if PerformanceTest
            sw.Stop();

            Console.WriteLine(string.Format("Calculate {0} ms", sw.ElapsedMilliseconds));

#endif
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
                return "CONTAINS";
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

                Dictionary<string, WordIndexForQuery> wordIndexDict = new Dictionary<string, WordIndexForQuery>();

                _QueryWords.Clear();
                wordIndexDict.Clear();

                List<WordIndexForQuery> wordIndexList = new List<WordIndexForQuery>(value.Count);

                bool someWordNoRecords = false;

                foreach (Hubble.Core.Entity.WordInfo wordInfo in value)
                {
                    _QueryWords.Add(wordInfo);

                    WordIndexForQuery wifq;

                    if (!wordIndexDict.TryGetValue(wordInfo.Word, out wifq))
                    {
                        Hubble.Core.Index.InvertedIndex.WordIndexReader wordIndex ;

                        if (value.Count == 1)
                        {
                            wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, CanLoadPartOfDocs);
                        }
                        else
                        {
                            wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, false);
                        }
                        

                        if (wordIndex == null)
                        {
                            someWordNoRecords = true;
                            break;
                        }

                        wifq = new WordIndexForQuery(wordIndex,
                            InvertedIndex.DocumentCount, wordInfo.Rank, this.FieldRank);
                        wifq.QueryCount = 1;
                        wifq.FirstPosition = wordInfo.Position;
                        wordIndexList.Add(wifq);
                        wordIndexDict.Add(wordInfo.Word, wifq);
                        _TotalDocuments = InvertedIndex.DocumentCount;
                    }
                    else
                    {
                        wifq.WordRank += wordInfo.Rank;
                        wifq.QueryCount++;
                    }

                    //wordIndexList[wordIndexList.Count - 1].Rank += wordInfo.Rank;
                }

                if (!someWordNoRecords)
                {
                    _Norm_Ranks = 0;
                    foreach (WordIndexForQuery wq in wordIndexList)
                    {
                        _Norm_Ranks += wq.WordRank * wq.WordRank;
                    }

                    _Norm_Ranks = (long)Math.Sqrt(_Norm_Ranks);

                    _WordIndexes = new WordIndexForQuery[wordIndexList.Count];
                    wordIndexList.CopyTo(_WordIndexes, 0);
                    wordIndexList = null;
                }
                else
                {
                    _WordIndexes = new WordIndexForQuery[0];
                }
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

            sw.Reset();
            sw.Start();
#endif

            Core.SFQL.Parse.DocumentResultWhereDictionary result = new Core.SFQL.Parse.DocumentResultWhereDictionary();

            if (_QueryWords.Count <= 0 || _WordIndexes.Length <= 0)
            {
                if (Not && UpDict != null)
                {
                    return UpDict;
                }
                else
                {
                    return result;
                }
            }

            if (this.Not)
            {
                if (_InvertedIndex.IndexMode == Field.IndexMode.Simple)
                {
                    Calculate(null, ref result, _WordIndexes);
                }
                else
                {
                    CalculateWithPosition(null, ref result, _WordIndexes);
                }
            }
            else
            {
                if (_InvertedIndex.IndexMode == Field.IndexMode.Simple)
                {
                    Calculate(this.UpDict, ref result, _WordIndexes);
                }
                else
                {
                    CalculateWithPosition(this.UpDict, ref result, _WordIndexes);
                }
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
