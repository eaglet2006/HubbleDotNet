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
    public class MatchQuery : IQuery, INamedExternalReference
    {
        class WordIndexForQuery : IComparable<WordIndexForQuery>
        {
            public int CurDocIdIndex;
            public int WordIndexesLength;
            public int Sum_d_t; //Sqrt of Total number of terms in all documents
            public int Idf; 
            public int WordRank;
            public int FieldRank;
            public int RelTotalCount;

            public int QueryCount; //How many time is this word in query string.
            public int FirstPosition; //First position in query string.

            private Index.WordIndexReader _WordIndex;

            public Index.WordIndexReader WordIndex
            {
                get
                {
                    return _WordIndex;
                }
            }

            public WordIndexForQuery(Index.WordIndexReader wordIndex, 
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

                _WordIndex = wordIndex;

                Sum_d_t = (int)Math.Sqrt(_WordIndex.WordCount);
                Idf = (int)Math.Log10((double)totalDocuments / (double)_WordIndex.RelDocCount + 1) + 1;
                CurDocIdIndex = 0;
                WordIndexesLength = _WordIndex.Count;
            }

            #region IComparable<WordIndexForQuery> Members

            public int CompareTo(WordIndexForQuery other)
            {
                return this.RelTotalCount.CompareTo(other.RelTotalCount);
            }

            #endregion
        }

        #region Private fields
        int MinResultCount = 32768;

        string _FieldName;
        Hubble.Core.Index.InvertedIndex _InvertedIndex;
        private int _TabIndex;
        private DBProvider _DBProvider;
        private int _TotalDocuments;

        AppendList<Entity.WordInfo> _QueryWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        WordIndexForQuery[] _WordIndexes;

        #endregion


        /// <summary>
        /// Order by score desc
        /// and only one expression in the banch of expression tree.
        /// and more then two words
        /// </summary>
        /// <param name="upDict"></param>
        /// <param name="docIdRank"></param>
        /// <param name="wordIndexes"></param>
        unsafe private void CalculateWithPositionOrderByScoreDesc(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            if (upDict != null)
            {
                throw new ParseException("UpDict is not null!");
            }

            Array.Sort(wordIndexes);

            //Calculate top
            int top = (1 + this.End / 100) * 100;
            
            if (top <= 0)
            {
                top = 100;
            }

            if (this.End * 2 > top)
            {
                top *= 2;
            }

            double ratio = 1;
            if (wordIndexes.Length > 1)
            {
                ratio = (double)2 / (double)(wordIndexes.Length - 1);
            }

            //Get max word doc list count
            int maxWordDocListCount = 0;
            int documentSum = 0;

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                maxWordDocListCount += wifq.WordIndex.Count;
            }

            maxWordDocListCount += maxWordDocListCount / 2;

            if (maxWordDocListCount > 1024 * 1024)
            {
                maxWordDocListCount = 1024 * 1024;
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            bool groupbyScanAll = false;

            //Match for group by
            if (this.NeedGroupBy)
            {
                groupbyScanAll = true;

                int groupbyContainsCount = 0;
                int groupbyLimit = _DBProvider.Table.GroupByLimit;

                for (int i = 0; i < wordIndexes.Length; i++)
                {
                    WordIndexForQuery wifq = wordIndexes[i];
                    Entity.DocumentPositionList docList = wifq.WordIndex.GetNext();

                    while (docList.DocumentId >= 0)
                    {
                        if (!docIdRank.GroupByContains(docList.DocumentId))
                        {
                            docIdRank.AddToGroupByCollection(docList.DocumentId);
                            
                            groupbyContainsCount++;

                            if (groupbyContainsCount >= groupbyLimit)
                            {
                                groupbyScanAll = false;
                                break;
                            }
                        }

                        docList = wifq.WordIndex.GetNext();
                    }

                    wifq.WordIndex.Reset();

                    if (!groupbyScanAll)
                    {
                        break;
                    }
                }
            }

            //Merge
            int indexInTop = 0;

            for (int i = 0; i < wordIndexes.Length; i++)
            {
                if (docIdRank.Count >= top)
                {
                    break;
                }

                indexInTop = i;

                WordIndexForQuery wifq = wordIndexes[i];

                //Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                Entity.DocumentPositionList docList = wifq.WordIndex.GetNext();
                int j = 0;

                while (docList.DocumentId >= 0)
                {
                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }
                    else
                    {
                        switch (i)
                        {
                            case 0:
                                score *= 20;
                                break;
                            case 1:
                                score *= 4;
                                break;
                            case 2:
                                score *= 1;
                                break;
                            case 3:
                                score /= 2;
                                break;
                            default:
                                score /= i;
                                break;
                        }
                    }

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }

                    bool exits = drp.pDocumentResult != null;

                    if (!exits && i > 0)
                    {
                        exits = docIdRank.TryGetValue(docList.DocumentId, out drp);
                    }

                    if (exits)
                    {
                        drp.pDocumentResult->Score += score;

                        double queryPositionDelta = wifq.FirstPosition - drp.pDocumentResult->LastWordIndexFirstPosition;
                        double positionDelta = docList.FirstPosition - drp.pDocumentResult->LastPosition;

                        double delta = Math.Abs(queryPositionDelta - positionDelta);

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

                        delta = Math.Pow((1 / delta), ratio) * docList.Count * drp.pDocumentResult->LastCount /
                            (double)(wifq.QueryCount * drp.pDocumentResult->LastWordIndexQueryCount);

                        drp.pDocumentResult->Score = (long)(drp.pDocumentResult->Score * delta);
                        drp.pDocumentResult->LastIndex = (UInt16)i;
                        drp.pDocumentResult->LastPosition = docList.FirstPosition;
                        drp.pDocumentResult->LastCount = (UInt16)docList.Count;
                        drp.pDocumentResult->LastWordIndexFirstPosition = (UInt16)wifq.FirstPosition;
                    }
                    else
                    {
                        bool notInDict = false;

                        if (_NotInDict != null)
                        {
                            if (_NotInDict.ContainsKey(docList.DocumentId))
                            {
                                notInDict = true;
                            }
                        }

                        if (!notInDict)
                        {
                            //upDict is null in this function
                            DocumentResult docResult = new DocumentResult(docList.DocumentId, score, wifq.FirstPosition, wifq.QueryCount, docList.FirstPosition, docList.Count, i);
                            docIdRank.Add(docList.DocumentId, docResult);
                        }
                    }

                    docList = wifq.WordIndex.GetNext();
                    j++;

                    if (j > wifq.WordIndex.Count)
                    {
                        break;
                    }
                }
            }

            double hitRate = 0;

            if (indexInTop < wordIndexes.Length - 1)
            {
                int[] docidlist = new int[docIdRank.Count];

                int i = 0;
                foreach(int docid in docIdRank.Keys)
                {
                    docidlist[i] = docid;
                    i++;
                }

                Array.Sort(docidlist);

                int wordIndexesLen = wordIndexes.Length;
                
                int lastWordHitCount = 0;

                foreach (int firstDocId in docidlist)
                {
                    int curWord = indexInTop + 1;

                    Core.SFQL.Parse.DocumentResultPoint drp;

                    if (docIdRank.TryGetValue(firstDocId, out drp))
                    {
                        while (curWord < wordIndexesLen)
                        {
                            Entity.DocumentPositionList docList = wordIndexes[curWord].WordIndex.Get(firstDocId);
                            int curDocId = docList.DocumentId;

                            if (curDocId >= 0)
                            {
                                if (curWord == wordIndexesLen - 1)
                                {
                                    lastWordHitCount++;
                                }

                                WordIndexForQuery wifq = wordIndexes[curWord];

                                long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                                if (score < 0)
                                {
                                    //Overflow
                                    score = long.MaxValue - 4000000;
                                }
                                else
                                {
                                    switch (curWord)
                                    {
                                        case 0:
                                            score *= 20;
                                            break;
                                        case 1:
                                            score *= 4;
                                            break;
                                        case 2:
                                            score *= 1;
                                            break;
                                        case 3:
                                            score /= 2;
                                            break;
                                        default:
                                            score /= curWord;
                                            break;
                                    }
                                }

                                if (score < 0)
                                {
                                    //Overflow
                                    score = long.MaxValue - 4000000;
                                }

                                drp.pDocumentResult->Score += score;

                                double queryPositionDelta = wifq.FirstPosition - drp.pDocumentResult->LastWordIndexFirstPosition;
                                double positionDelta = docList.FirstPosition - drp.pDocumentResult->LastPosition;

                                double delta = Math.Abs(queryPositionDelta - positionDelta);

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

                                delta = Math.Pow((1 / delta), ratio) * docList.Count * drp.pDocumentResult->LastCount /
                                    (double)(wifq.QueryCount * drp.pDocumentResult->LastWordIndexQueryCount);

                                drp.pDocumentResult->Score = (long)(drp.pDocumentResult->Score * delta);
                                drp.pDocumentResult->LastIndex = (UInt16)curWord;
                                drp.pDocumentResult->LastPosition = docList.FirstPosition;
                                drp.pDocumentResult->LastCount = (UInt16)docList.Count;
                                drp.pDocumentResult->LastWordIndexFirstPosition = (UInt16)wifq.FirstPosition;

                            }
                            curWord++;
                        } //While
                    }
                }

                if (docidlist.Length > 0)
                {
                    hitRate = (double)lastWordHitCount / (double)docidlist.Length;
                }
            }

            performanceReport.Stop();

            documentSum += docIdRank.Count;

            if (indexInTop < wordIndexes.Length - 1)
            {
                documentSum += wordIndexes[wordIndexes.Length - 1].RelTotalCount;

                if (hitRate > 0)
                {
                    int predictCount = 0;

                    for (int i = indexInTop + 1; i < wordIndexes.Length - 1; i++)
                    {
                        predictCount += (int)(wordIndexes[i].RelTotalCount * (1 - hitRate));
                    }

                    documentSum += predictCount;
                }
            }

            if (documentSum > _TotalDocuments)
            {
                documentSum = _TotalDocuments;
            }

            docIdRank.RelTotalCount = documentSum;

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int deleteCount = delProvider.Filter(docIdRank);

            docIdRank.RelTotalCount -= deleteCount;

            if (groupbyScanAll)
            {
                docIdRank.RelTotalCount = docIdRank.GroupByCollection.Count;
            }
            else if (docIdRank.GroupByCollection.Count > docIdRank.RelTotalCount)
            {
                docIdRank.RelTotalCount = docIdRank.GroupByCollection.Count;
            }
        }


        unsafe private void CalculateWithPosition(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            MinResultCount = _DBProvider.Table.GroupByLimit;

            double ratio = 1;
            if (wordIndexes.Length > 1)
            {
                ratio = (double)2 / (double)(wordIndexes.Length - 1);
            }

            //Get max word doc list count
            int maxWordDocListCount = 0;
            int documentSum = 0;

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                maxWordDocListCount += wifq.WordIndex.Count;
            }

            maxWordDocListCount += maxWordDocListCount / 2;

            if (maxWordDocListCount > 1024 * 1024)
            {
                maxWordDocListCount = 1024 * 1024;
            }

            if (docIdRank.Count == 0)
            {
                if (maxWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(maxWordDocListCount);
                }
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            //Merge
            bool oneWordOptimize = this.CanLoadPartOfDocs && this.NoAndExpression && wordIndexes.Length == 1;

            for (int i = 0; i < wordIndexes.Length; i++)
            {
                WordIndexForQuery wifq = wordIndexes[i];

                //Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                Entity.DocumentPositionList docList = wifq.WordIndex.GetNext();
                int j = 0;
                int oneWordMaxCount = 0;

                while (docList.DocumentId >= 0)
                {
                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (oneWordOptimize)
                    {
                        if (j > MinResultCount)
                        {
                            if (oneWordMaxCount > docList.Count)
                            {
                                docList = wifq.WordIndex.GetNext();
                                j++;

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
                    }

                    if (j > wifq.RelTotalCount)
                    {
                        break;
                    }

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }

                    bool exits = drp.pDocumentResult != null;

                    if (!exits && i > 0)
                    {
                        exits = docIdRank.TryGetValue(docList.DocumentId, out drp);
                    }

                    if (exits)
                    {
                        drp.pDocumentResult->Score += score;

                        double queryPositionDelta = wifq.FirstPosition - drp.pDocumentResult->LastWordIndexFirstPosition;
                        double positionDelta = docList.FirstPosition - drp.pDocumentResult->LastPosition;

                        double delta = Math.Abs(queryPositionDelta - positionDelta);

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

                        delta = Math.Pow((1 / delta), ratio) * docList.Count * drp.pDocumentResult->LastCount /
                            (double)(wifq.QueryCount * drp.pDocumentResult->LastWordIndexQueryCount);

                        //some words missed
                        //if (i - drp.pDocumentResult->LastIndex > 1)
                        //{
                        //    int sumWordRank = 10;
                        //    for (int k = drp.pDocumentResult->LastIndex + 1; k < i; k++)
                        //    {
                        //        sumWordRank += wordIndexes[k].WordRank;
                        //    }

                        //    delta /= (double)sumWordRank;
                        //}

                        drp.pDocumentResult->Score = (long)(drp.pDocumentResult->Score * delta);
                        drp.pDocumentResult->LastIndex = (UInt16)i;
                        drp.pDocumentResult->LastPosition = docList.FirstPosition;
                        drp.pDocumentResult->LastCount = (UInt16)docList.Count;
                        drp.pDocumentResult->LastWordIndexFirstPosition = (UInt16)wifq.FirstPosition;
                    }
                    else
                    {
                        //some words missed
                        //if (i > 0)
                        //{
                        //    int sumWordRank = 10;
                        //    for (int k = 0; k < i; k++)
                        //    {
                        //        sumWordRank += wordIndexes[k].WordRank;
                        //    }

                        //    double delta = 1 / (double)sumWordRank;
                        //    score = (long)(score * delta);
                        //}

                        bool notInDict = false;

                        if (_NotInDict != null)
                        {
                            if (_NotInDict.ContainsKey(docList.DocumentId))
                            {
                                notInDict = true;
                            }
                        }

                        if (!notInDict)
                        {
                            if (upDict == null)
                            {
                                DocumentResult docResult = new DocumentResult(docList.DocumentId, score, wifq.FirstPosition, wifq.QueryCount, docList.FirstPosition, docList.Count, i);
                                docIdRank.Add(docList.DocumentId, docResult);
                            }
                            else
                            {
                                if (!upDict.Not)
                                {
                                    if (upDict.ContainsKey(docList.DocumentId))
                                    {
                                        DocumentResult docResult = new DocumentResult(docList.DocumentId, score, wifq.FirstPosition, wifq.QueryCount, docList.FirstPosition, docList.Count, i);
                                        docIdRank.Add(docList.DocumentId, docResult);
                                    }
                                }
                                else
                                {
                                    if (!upDict.ContainsKey(docList.DocumentId))
                                    {
                                        DocumentResult docResult = new DocumentResult(docList.DocumentId, score, wifq.FirstPosition, wifq.QueryCount, docList.FirstPosition, docList.Count, i);
                                        docIdRank.Add(docList.DocumentId, docResult);
                                    }
                                }
                            }
                        }
                    }

                    docList = wifq.WordIndex.GetNext();
                    j++;

                    if (j > wifq.WordIndex.Count)
                    {
                        break;
                    }
                }
            }

            //Merge score if upDict != null
            if (upDict != null)
            {
                if (!upDict.Not)
                {
                    foreach (int docid in docIdRank.Keys)
                    {
                        DocumentResult* upDrp;

                        if (upDict.TryGetValue(docid, out upDrp))
                        {
                            DocumentResult* drpResult;
                            if (docIdRank.TryGetValue(docid, out drpResult))
                            {
                                drpResult->Score += upDrp->Score;
                            }
                        }
                    }
                }
            }

            //some words missed
            //if (wordIndexes.Length > 1)
            //{
            //    List<DocumentResult> reduceDocs = new List<DocumentResult>(docIdRank.Count);
            //    int lstIndex = wordIndexes.Length - 1;
            //    foreach (Core.SFQL.Parse.DocumentResultPoint drp in docIdRank.Values)
            //    {
            //        DocumentResult* dr = drp.pDocumentResult;
            //        //DocumentResult* dr1 = drp.pDocumentResult;
            //        if (dr->LastIndex != lstIndex)
            //        {
            //            int sumWordRank = 10;
            //            for (int k = dr->LastIndex + 1; k <= lstIndex; k++)
            //            {
            //                sumWordRank += wordIndexes[k].WordRank;
            //            }

            //            double delta = 1 / (double)sumWordRank;

            //            dr->Score = (long)((double)dr->Score * delta);
            //        }

            //        if (dr->Score < 0)
            //        {
            //            dr->Score = long.MaxValue / 10;
            //        }
            //    }
            //}

            performanceReport.Stop();

            documentSum += docIdRank.Count;

            if (documentSum > _TotalDocuments)
            {
                documentSum = _TotalDocuments;
            }

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int deleteCount = delProvider.Filter(docIdRank);

            if (CanLoadPartOfDocs && upDict == null)
            {
                if (docIdRank.Count < wordIndexes[wordIndexes.Length - 1].RelTotalCount)
                {
                    if (wordIndexes.Length > 1)
                    {
                        if (wordIndexes[wordIndexes.Length - 1].RelTotalCount > _DBProvider.MaxReturnCount)
                        {
                            documentSum += wordIndexes[wordIndexes.Length - 1].RelTotalCount - _DBProvider.MaxReturnCount;
                        }

                        if (documentSum > _TotalDocuments)
                        {
                            documentSum = _TotalDocuments;
                        }

                        docIdRank.RelTotalCount = documentSum;
                    }
                    else
                    {
                        docIdRank.RelTotalCount = wordIndexes[wordIndexes.Length - 1].RelTotalCount;
                    }
                }
            }

            docIdRank.RelTotalCount -= deleteCount;
        }

        unsafe private void Calculate(DocumentResultWhereDictionary upDict,
            ref DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            MinResultCount = _DBProvider.Table.GroupByLimit;

            //Get max word doc list count
            int maxWordDocListCount = 0;
            int documentSum = 0;

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                maxWordDocListCount += wifq.WordIndex.RelDocCount;
            }

            if (docIdRank.Count == 0)
            {
                if (maxWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(maxWordDocListCount);
                }
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            //Merge
            bool oneWordOptimize = this.CanLoadPartOfDocs && this.NoAndExpression && wordIndexes.Length == 1;

            for (int i = 0; i < wordIndexes.Length; i++)
            {
                WordIndexForQuery wifq = wordIndexes[i];

                //Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                Entity.DocumentPositionList docList = wifq.WordIndex.GetNext();
                int j = 0;
                int oneWordMaxCount = 0;

                while (docList.DocumentId >= 0)
                {
                    //Entity.DocumentPositionList docList = wifq.WordIndex[j];

                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (oneWordOptimize)
                    {
                        if (j > MinResultCount)
                        {
                            if (j > MinResultCount)
                            {
                                if (oneWordMaxCount > docList.Count)
                                {
                                    docList = wifq.WordIndex.GetNext();
                                    j++;

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
                        }
                    }


                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }

                    bool exits = drp.pDocumentResult != null;

                    if (!exits && i > 0)
                    {
                        exits = docIdRank.TryGetValue(docList.DocumentId, out drp);
                    }

                    if (exits)
                    {
                        drp.pDocumentResult->Score += score;
                    }
                    else
                    {
                        bool notInDict = false;

                        if (_NotInDict != null)
                        {
                            if (_NotInDict.ContainsKey(docList.DocumentId))
                            {
                                notInDict = true;
                            }
                        }

                        if (!notInDict)
                        {
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

                    docList = wifq.WordIndex.GetNext();
                    j++;
                }
            }

            //Merge score if upDict != null
            if (upDict != null)
            {
                if (!upDict.Not)
                {
                    foreach (int docid in docIdRank.Keys)
                    {
                        DocumentResult* upDrp;

                        if (upDict.TryGetValue(docid, out upDrp))
                        {
                            DocumentResult* drpResult;
                            if (docIdRank.TryGetValue(docid, out drpResult))
                            {
                                drpResult->Score += upDrp->Score;
                            }
                        }
                    }
                }
            }

            documentSum += docIdRank.Count;

            if (documentSum > _TotalDocuments)
            {
                documentSum = _TotalDocuments;
            }

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int deleteCount = delProvider.Filter(docIdRank);

            if (CanLoadPartOfDocs && upDict == null)
            {
                if (docIdRank.Count < wordIndexes[wordIndexes.Length - 1].RelTotalCount)
                {
                    if (wordIndexes.Length > 1)
                    {
                        if (wordIndexes[wordIndexes.Length - 1].RelTotalCount > _DBProvider.MaxReturnCount)
                        {
                            documentSum += wordIndexes[wordIndexes.Length - 1].RelTotalCount - _DBProvider.MaxReturnCount;
                        }

                        if (documentSum > _TotalDocuments)
                        {
                            documentSum = _TotalDocuments;
                        }

                        docIdRank.RelTotalCount = documentSum;
                    }
                    else
                    {
                        docIdRank.RelTotalCount = wordIndexes[wordIndexes.Length - 1].RelTotalCount;
                    }
                }
            }

            docIdRank.RelTotalCount -= deleteCount;

            performanceReport.Stop();
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
                return "Match";
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

        private int _End = -1;

        public int End
        {
            get
            {
                return _End;
            }
            set
            {
                _End = value;
            }
        }

        private string _OrderBy = null;

        public string OrderBy
        {
            get
            {
                return _OrderBy;
            }
            set
            {
                _OrderBy = value;
            }
        }

        private bool _NeedGroupBy;

        public bool NeedGroupBy
        {
            get
            {
                return _NeedGroupBy;
            }
            set
            {
                _NeedGroupBy = value;
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
                Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("QueryWords");

                Dictionary<string, WordIndexForQuery> wordIndexDict = new Dictionary<string, WordIndexForQuery>();

                _QueryWords.Clear();
                wordIndexDict.Clear();

                List<WordIndexForQuery> wordIndexList = new List<WordIndexForQuery>(value.Count);


                foreach (Hubble.Core.Entity.WordInfo wordInfo in value)
                {
                    _QueryWords.Add(wordInfo);

                    WordIndexForQuery wifq;

                    if (!wordIndexDict.TryGetValue(wordInfo.Word, out wifq))
                    {

                        //Hubble.Core.Index.WordIndexReader wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, CanLoadPartOfDocs); //Get whole index

                        Hubble.Core.Index.WordIndexReader wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, CanLoadPartOfDocs, true); //Only get step doc index

                        if (wordIndex == null)
                        {
                            continue;
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

                _WordIndexes = new WordIndexForQuery[wordIndexList.Count];
                wordIndexList.CopyTo(_WordIndexes, 0);
                wordIndexList = null;

                performanceReport.Stop();
            }
        }

        public Core.SFQL.Parse.DocumentResultWhereDictionary Search()
        {
            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Search");

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
                    if (!this.NeedGroupBy && this.CanLoadPartOfDocs && this.NoAndExpression && _WordIndexes.Length > 1 && this.UpDict == null)
                    {
                        CalculateWithPositionOrderByScoreDesc(null, ref result, _WordIndexes);
                    }
                    else
                    {
                        CalculateWithPosition(null, ref result, _WordIndexes);
                    }

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
                    if (!this.NeedGroupBy && this.CanLoadPartOfDocs && this.NoAndExpression && _WordIndexes.Length > 1 && this.UpDict == null)
                    {
                        CalculateWithPositionOrderByScoreDesc(this.UpDict, ref result, _WordIndexes);
                    }
                    else
                    {
                        CalculateWithPosition(this.UpDict, ref result, _WordIndexes);
                    }
                }
            }

            if (this.Not)
            {
                result.Not = true;

                if (UpDict != null)
                {
                    result = result.AndMerge(result, UpDict);
                }
            }

            performanceReport.Stop();

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

        private Dictionary<int, int> _NotInDict = null;

        /// <summary>
        /// Key is docid
        /// Value is 0
        /// </summary>
        public Dictionary<int, int> NotInDict
        {
            get
            {
                return _NotInDict;
            }

            set
            {
                _NotInDict = value;
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
