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
using Hubble.Core.Entity;
using Hubble.Core.Query.Optimize;

namespace Hubble.Core.Query
{
    /// <summary>
    /// This query analyze input words just using
    /// tf/idf. The poisition informations are no useful.
    /// Syntax: MutiStringQuery('xxx','yyy','zzz')
    /// </summary>
    public class ContainsQuery : IQuery, INamedExternalReference
    {
        class WordIndexForQueryCompareByPositionAndRank : IComparer<WordIndexForQuery>
        {

            #region IComparer<WordIndexForQuery> Members

            public int Compare(WordIndexForQuery x, WordIndexForQuery y)
            {
                if (x.FirstPosition > y.FirstPosition)
                {
                    return 1;
                }
                else if (x.FirstPosition < y.FirstPosition)
                {
                    return -1;
                }
                else
                {
                    if (x.WordRank > y.WordRank)
                    {
                        return -1;
                    }
                    else if (x.WordRank < y.WordRank)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
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
        /// if first word's flag is or,
        /// adjust the result of sort.
        /// </summary>
        /// <param name="wordIndexes">result of sort</param>
        private void AdjustSort(WordIndexForQuery[] wordIndexes)
        {
            if (wordIndexes.Length > 0)
            {
                if ((wordIndexes[0].Flags & WordInfo.Flag.Or) != 0)
                {
                    for (int i = 1; i < wordIndexes.Length; i++)
                    {
                        if ((wordIndexes[i].Flags & WordInfo.Flag.Or) == 0)
                        {
                            WordIndexForQuery temp = wordIndexes[i];
                            wordIndexes[i] = wordIndexes[0];
                            wordIndexes[0] = temp;
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        unsafe private void CalculateWithPositionMatch(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
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
            bool oneWordOptimize = this._QueryParameter.CanLoadPartOfDocs && this._QueryParameter.NoAndExpression && wordIndexes.Length == 1;

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

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

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

            if (_QueryParameter.CanLoadPartOfDocs && upDict == null)
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

        private bool UseMatch(WordIndexForQuery[] wordIndexes)
        {
            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                if ((wifq.Flags & WordInfo.Flag.Or) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        unsafe private void CalculateWithPosition(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            if (UseMatch(wordIndexes))
            {
                CalculateWithPositionMatch(upDict, ref docIdRank, wordIndexes);
                return;
            }

            Array.Sort(wordIndexes);

            AdjustSort(wordIndexes);

            MinResultCount = _DBProvider.Table.GroupByLimit;

            //Get max word doc list count
            int minWordDocListCount = 1 * 1024 * 1024; //1M

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                minWordDocListCount = Math.Min(minWordDocListCount, wifq.WordIndex.Count);
            }

  
            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            //Merge
            bool oneWordOptimize = this._QueryParameter.CanLoadPartOfDocs && this._QueryParameter.NoAndExpression
                && wordIndexes.Length == 1 && _NotInDict == null && _QueryParameter.End >= 0 && !_QueryParameter.NeedDistinct;

            if (oneWordOptimize)
            {
                IQueryOptimize qOptimize = QueryOptimizeBuilder.Build(typeof(OneWordOptimize),
                    DBProvider, _QueryParameter.End, _QueryParameter.OrderBy,
                    _QueryParameter.OrderBys, _QueryParameter.NeedGroupBy, _QueryParameter.OrderByCanBeOptimized, wordIndexes);

                try
                {
                    qOptimize.CalculateOptimize(upDict, ref docIdRank);
                    return;
                }
                finally
                {
                    performanceReport.Stop();
                }
            }

            if (this._QueryParameter.CanLoadPartOfDocs && this._QueryParameter.NoAndExpression
                && _NotInDict == null && _QueryParameter.End >= 0 && !_QueryParameter.NeedDistinct)
            {
                IQueryOptimize qOptimize = QueryOptimizeBuilder.Build(typeof(ContainsOptimize),
                    DBProvider, _QueryParameter.End, _QueryParameter.OrderBy,
                    _QueryParameter.OrderBys, _QueryParameter.NeedGroupBy, _QueryParameter.OrderByCanBeOptimized, wordIndexes);

                try
                {
                    qOptimize.CalculateOptimize(upDict, ref docIdRank);
                    return;
                }
                finally
                {
                    performanceReport.Stop();
                }

                //if (qOptimize.Argument.IsOrderByScoreDesc())
                //{
          
                //}
            }

            if (docIdRank.Count == 0)
            {
                if (minWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(minWordDocListCount);
                }
            }

            {
                double ratio = 1;

                if (wordIndexes.Length > 1)
                {
                    ratio = (double)2 / (double)(wordIndexes.Length - 1);
                }

                int wordIndexesLen = wordIndexes.Length;

                WordIndexForQuery fstWifq = wordIndexes[0]; //first word

                OriginalDocumentPositionList fstODPL = new OriginalDocumentPositionList();
                fstWifq.WordIndex.GetNextOriginal(ref fstODPL);

                //Entity.DocumentPositionList fstDocList = fstWifq.WordIndex.GetNext();

                Entity.DocumentPositionList[] docListArr = new Hubble.Core.Entity.DocumentPositionList[wordIndexesLen];

                //docListArr[0] = fstDocList;
                fstODPL.ToDocumentPositionList(ref docListArr[0]);

                OriginalDocumentPositionList odpl = new OriginalDocumentPositionList();

                while (fstODPL.DocumentId >= 0)
                {
                    int curWord = 1;
                    int firstDocId = fstODPL.DocumentId;

                    while (curWord < wordIndexesLen)
                    {
                        //docListArr[curWord] = wordIndexes[curWord].WordIndex.Get(firstDocId);

                        wordIndexes[curWord].WordIndex.GetNextOriginalWithDocId(ref odpl, firstDocId);
                        odpl.ToDocumentPositionList(ref docListArr[curWord]);

                        if (docListArr[curWord].DocumentId < 0)
                        {
                            if ((wordIndexes[curWord].Flags & WordInfo.Flag.Or) != 0)
                            {
                                curWord++;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        curWord++;
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

                            if (wifq.WordIndex.Count == 0)
                            {
                                //a^5000^0 b^5000^2^1
                                //if has a and hasn't b but b can be or
                                //2010-09-30 eaglet
                                continue;
                            }

                            Entity.DocumentPositionList docList = docListArr[i];


                            long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                            if (score < 0)
                            {
                                //Overflow
                                score = long.MaxValue - 4000000;
                            }

                            double delta = 1;

                            if (i > 0)
                            {
                                //Calculate with position
                                double queryPositionDelta = wifq.FirstPosition - wordIndexes[i - 1].FirstPosition;
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
                                    (double)(wifq.QueryCount * wordIndexes[i - 1].QueryCount);
                            }

                            lastDocList = docList;

                            totalScore += (long)(score * delta);
                        }

                        bool notInDict = false;

                        if (_NotInDict != null)
                        {
                            if (_NotInDict.ContainsKey(firstDocId))
                            {
                                notInDict = true;
                            }
                        }

                        if (!notInDict)
                        {
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
                        }

                    }//if (curWord >= wordIndexesLen)

                    //fstDocList = fstWifq.WordIndex.GetNext();
                    //docListArr[0] = fstDocList;

                    fstWifq.WordIndex.GetNextOriginal(ref fstODPL);
                    fstODPL.ToDocumentPositionList(ref docListArr[0]);

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

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int delCount = delProvider.Filter(docIdRank);

            if (oneWordOptimize && _QueryParameter.CanLoadPartOfDocs && upDict == null)
            {
                docIdRank.RelTotalCount = wordIndexes[0].RelTotalCount - delCount;
            }
            else
            {
                docIdRank.RelTotalCount = docIdRank.Count;
            }

            performanceReport.Stop();

        }


        unsafe private void Calculate(DocumentResultWhereDictionary upDict,
            ref DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            AdjustSort(wordIndexes);

            MinResultCount = _DBProvider.Table.GroupByLimit;

            //Get max word doc list count
            int minWordDocListCount = 1 * 1024 * 1024; //1M

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                minWordDocListCount = Math.Min(minWordDocListCount, wifq.WordIndex.Count);
            }

            if (docIdRank.Count == 0)
            {
                if (minWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(minWordDocListCount);
                }
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            //Merge
            bool oneWordOptimize = this._QueryParameter.CanLoadPartOfDocs && this._QueryParameter.NoAndExpression && wordIndexes.Length == 1;
            int oneWordMaxCount = 0;

            if (oneWordOptimize)
            {
                //One word
                WordIndexForQuery wifq = wordIndexes[0]; //first word

                //Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                Entity.DocumentPositionList docList = wifq.WordIndex.GetNext();
                int j = 0;

                while (docList.DocumentId >= 0)
                {
                    //Entity.DocumentPositionList docList = wifq.WordIndex[j];

                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (j > MinResultCount)
                    {
                        if (oneWordMaxCount > docList.Count)
                        {
                            j++;
                            docList = wifq.WordIndex.GetNext();

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

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }

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

                    j++;
                    docList = wifq.WordIndex.GetNext();
                }
            }
            else
            {
                int wordIndexesLen = wordIndexes.Length;

                WordIndexForQuery fstWifq = wordIndexes[0]; //first word

                Entity.DocumentPositionList fstDocList = fstWifq.WordIndex.GetNext();

                Entity.DocumentPositionList[] docListArr = new Hubble.Core.Entity.DocumentPositionList[wordIndexesLen];

                docListArr[0] = fstDocList;

                while (fstDocList.DocumentId >= 0)
                {
                    int curWord = 1;
                    int firstDocId = fstDocList.DocumentId;

                    while (curWord < wordIndexesLen)
                    {
                        docListArr[curWord] = wordIndexes[curWord].WordIndex.Get(firstDocId);
                        
                        if (docListArr[curWord].DocumentId < 0)
                        {
                            if ((wordIndexes[curWord].Flags & WordInfo.Flag.Or) != 0)
                            {
                                curWord++;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        curWord++;
                    } //While

                    if (curWord >= wordIndexesLen)
                    {
                        //Matched

                        long totalScore = 0;
                        for (int i = 0; i < wordIndexesLen; i++)
                        {
                            WordIndexForQuery wifq = wordIndexes[i];
                            Entity.DocumentPositionList docList = docListArr[i];

                            long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                            if (score < 0)
                            {
                                //Overflow
                                score = long.MaxValue - 4000000;
                            }

                            totalScore += score;
                        }

                        bool notInDict = false;

                        if (_NotInDict != null)
                        {
                            if (_NotInDict.ContainsKey(firstDocId))
                            {
                                notInDict = true;
                            }
                        }

                        if (!notInDict)
                        {
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
                        }
                    }

                    fstDocList = fstWifq.WordIndex.GetNext();
                    docListArr[0] = fstDocList;
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

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int delCount = delProvider.Filter(docIdRank);

            if (oneWordOptimize && _QueryParameter.CanLoadPartOfDocs && upDict == null)
            {
                docIdRank.RelTotalCount = wordIndexes[0].RelTotalCount - delCount;
            }
            else
            {
                docIdRank.RelTotalCount = docIdRank.Count;
            }

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
                return "CONTAINS";
            }
        }

        QueryParameter _QueryParameter = new QueryParameter();

        public QueryParameter QueryParameter
        {
            get
            {
                return _QueryParameter;
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
                Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("QueryWords");

                Dictionary<string, WordIndexForQuery> wordIndexDict = new Dictionary<string, WordIndexForQuery>();

                _QueryWords.Clear();
                wordIndexDict.Clear();

                List<WordIndexForQuery> wordIndexList = new List<WordIndexForQuery>(value.Count);

                for(int i =0; i < value.Count; i++)
                {
                    Hubble.Core.Entity.WordInfo wordInfo = value[i];
                    _QueryWords.Add(wordInfo);

                    WordIndexForQuery wifq;

                    if (!wordIndexDict.TryGetValue(wordInfo.Word, out wifq))
                    {
                        Hubble.Core.Index.WordIndexReader wordIndex ;

                        if (value.Count == 1)
                        {
                            //wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, CanLoadPartOfDocs);
                            wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, _QueryParameter.CanLoadPartOfDocs, true);
                        }
                        else
                        {
                            //wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, false);
                            wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, false, true);
                        }
                        

                        if (wordIndex == null)
                        {
                            //No result
                            wordIndex = new Hubble.Core.Index.WordIndexReader(wordInfo.Word,
                                new Hubble.Core.Store.WordDocumentsList(), 0, _DBProvider);
                        }

                        wifq = new WordIndexForQuery(wordIndex,
                            InvertedIndex.DocumentCount, wordInfo.Rank, this._QueryParameter.FieldRank, wordInfo.Flags);
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

        private Core.SFQL.Parse.DocumentResultWhereDictionary PartSearch(WordIndexForQuery[] wordIndexes)
        {
            Core.SFQL.Parse.DocumentResultWhereDictionary result = new Core.SFQL.Parse.DocumentResultWhereDictionary();

            if (_QueryWords.Count <= 0 || wordIndexes.Length <= 0)
            {
                if (_QueryParameter.Not && UpDict != null)
                {
                    return UpDict;
                }
                else
                {
                    return result;
                }
            }

            if (this._QueryParameter.Not)
            {
                if (_InvertedIndex.IndexMode == Field.IndexMode.Simple)
                {
                    Calculate(null, ref result, wordIndexes);
                }
                else
                {
                    CalculateWithPosition(null, ref result, wordIndexes);
                }
            }
            else
            {
                if (_InvertedIndex.IndexMode == Field.IndexMode.Simple)
                {
                    Calculate(this.UpDict, ref result, wordIndexes);
                }
                else
                {
                    CalculateWithPosition(this.UpDict, ref result, wordIndexes);
                }
            }

            return result;

        }

        private List<WordIndexForQuery[]> GetAllPartOfWordIndexes()
        {
            List<WordIndexForQuery[]> result = new List<WordIndexForQuery[]>();

            Array.Sort(_WordIndexes, new WordIndexForQueryCompareByPositionAndRank());

            List<List<WordIndexForQuery>> groups = new List<List<WordIndexForQuery>>();

            foreach(WordIndexForQuery wifq in _WordIndexes)
            {
                bool addNew = true;

                int i = 0;

                foreach (List<WordIndexForQuery> wifqList in groups)
                {
                    WordIndexForQuery wifq1 = wifqList[wifqList.Count - 1];

                    if (wifq.FirstPosition >= wifq1.FirstPosition + wifq1.WordIndex.Word.Length)
                    {
                        if (i == 0)
                        {
                            for(int j = 1; j < groups.Count; j++)
                            {
                                WordIndexForQuery wifq2 = groups[j][groups[j].Count - 1];
                                if (wifq1.FirstPosition >= wifq2.FirstPosition + wifq2.WordIndex.Word.Length)
                                {
                                    groups[j].Add(wifq1);
                                }
                            }
                        }

                        wifqList.Add(wifq);
                        addNew = false;
                        break;
                    }

                    i++;
                }

                if (addNew)
                {
                    groups.Add(new List<WordIndexForQuery>());
                    groups[groups.Count - 1].Add(wifq);
                }
            }

            if (groups.Count > 0)
            {
                WordIndexForQuery wifq1 = groups[0][groups[0].Count-1];

                for (int j = 1; j < groups.Count; j++)
                {
                    WordIndexForQuery wifq2 = groups[j][groups[j].Count - 1];
                    if (wifq1.FirstPosition >= wifq2.FirstPosition + wifq2.WordIndex.Word.Length)
                    {
                        groups[j].Add(wifq1);
                    }
                }
            }

            foreach (List<WordIndexForQuery> group in groups)
            {
                result.Add(group.ToArray());
            }

            return result;
        }

        public Core.SFQL.Parse.DocumentResultWhereDictionary Search()
        {
            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Search of Contains");

            List<WordIndexForQuery[]> partList = GetAllPartOfWordIndexes();

            if (_QueryWords.Count <= 0 || partList.Count <= 0)
            {
                return PartSearch(new WordIndexForQuery[0]);
            }


            Core.SFQL.Parse.DocumentResultWhereDictionary result = PartSearch(partList[0]);

            for (int i = 1; i < partList.Count; i++)
            {
                bool someWordNoResult = false;

                foreach (WordIndexForQuery w in partList[i])
                {
                    if (w.WordIndex.WordCount == 0)
                    {
                        someWordNoResult = true;
                        break;
                    }

                    w.WordIndex.Reset();
                }

                if (!someWordNoResult)
                {
                    result.OrMerge(result, PartSearch(partList[i]));
                }
            }

            if (this._QueryParameter.Not)
            {
                result.Not = true;

                if (UpDict != null)
                {
                    result = result.AndMergeForNot(result, UpDict);
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
