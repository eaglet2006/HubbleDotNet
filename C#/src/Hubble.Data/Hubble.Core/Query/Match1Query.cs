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
    public class Match1Query : IQuery, INamedExternalReference
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

                Norm_d_t = (int)Math.Sqrt(_WordIndex.WordCount);
                Idf_t = (int)Math.Log10((double)totalDocuments / (double)_WordIndex.RelDocCount + 1) + 1;
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

        long _Norm_Ranks = 0; //sqrt(sum_t(rank)^2))

        #endregion

        unsafe private void CalculateWithPosition08(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            double ratio = 1;
            if (wordIndexes.Length > 1)
            {
                ratio = (double)2 / (double)(wordIndexes.Length - 1);
            }

            //Get max word doc list count
            int maxWordDocListCount = 0;
            int maxDocId = 0;
            int documentSum = 0;

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                maxWordDocListCount += wifq.WordIndex.WordDocList.Count;

                if (maxWordDocListCount > MinResultCount)
                {
                    maxDocId = Math.Max(maxDocId, wifq.WordIndex.WordDocList[wifq.WordIndex.WordDocList.Count - 1].DocumentId);
                    break;
                }

                maxDocId = Math.Max(maxDocId, wifq.WordIndex.WordDocList[wifq.WordIndex.WordDocList.Count - 1].DocumentId);
            }

            maxWordDocListCount += maxWordDocListCount / 2;

            if (docIdRank.Count == 0)
            {
                if (maxWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(maxWordDocListCount);
                }
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            //Merge
            bool beginFilter = false;
            bool oneWordOptimize = this.CanLoadPartOfDocs && this.NoAndExpression && wordIndexes.Length == 1;
            int oneWordMaxCount = 0;

            for (int i = 0; i < wordIndexes.Length; i++)
            {
                WordIndexForQuery wifq = wordIndexes[i];

                if (docIdRank.Count > MinResultCount)
                {
                    beginFilter = true;
                }

                Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                for (int j = 0; j < wifq.WordIndexesLength; j++)
                {
                    //Entity.DocumentPositionList docList = wifq.WordIndex[j];
                    Entity.DocumentPositionList docList = wifqDocBuf[j];

                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (oneWordOptimize)
                    {
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
                    }


                    if (beginFilter)
                    {
                        if (docList.DocumentId > maxDocId)
                        {
                            documentSum += wifq.WordIndexesLength - j;
                            break;
                        }

                        if (!docIdRank.TryGetValue(docList.DocumentId, out drp))
                        {
                            documentSum++;
                            continue;
                        }
                    }


                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }


                    //int score = 0;

                    //if (rank > int.MaxValue - 4000000)
                    //{
                    //    long high = rank % (int.MaxValue - 4000000);

                    //    score = int.MaxValue - 4000000 + (int)(high / 1000);
                    //}
                    //else
                    //{
                    //    score = (int)rank;
                    //}

                    //rank = (long)wifq.FieldRank * (long)wifq.WordRank * (long)score;

                    //Query.DocumentResult docResult;

                    bool exits = drp.pDocumentResult != null;

                    if (!exits && i > 0)
                    {
                        exits = docIdRank.TryGetValue(docList.DocumentId, out drp);
                    }

                    if (exits)
                    {
                        drp.pDocumentResult->Score += score;
                        //docResult.Score += rank;

                        //WordIndexForQuery lastWordIndex = (WordIndexForQuery)docResult.Tag;

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

                        if (i - drp.pDocumentResult->LastIndex > 1)
                        {
                            int sumWordRank = 10;
                            for (int k = drp.pDocumentResult->LastIndex + 1; k < i; k++)
                            {
                                sumWordRank += wordIndexes[k].WordRank;
                            }

                            delta /= (double)sumWordRank;
                        }

                        drp.pDocumentResult->Score = (long)(drp.pDocumentResult->Score * delta);
                        drp.pDocumentResult->LastIndex = (UInt16)i;
                        drp.pDocumentResult->LastPosition = docList.FirstPosition;
                        drp.pDocumentResult->LastCount = (UInt16)docList.Count;
                        drp.pDocumentResult->LastWordIndexFirstPosition = (UInt16)wifq.FirstPosition;
                        //docResult.SortValue = docList.Count * 0x1000000000000 + i * 0x100000000 + docList.FirstPosition;

                        //docIdRank[docList.DocumentId] = docResult;
                    }
                    else
                    {
                        if (i > 0)
                        {
                            int sumWordRank = 10;
                            for (int k =0; k < i; k++)
                            {
                                sumWordRank += wordIndexes[k].WordRank;
                            }

                            double delta = 1 / (double)sumWordRank;
                            score = (long)(score * delta);
                        }

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
            }

            if (wordIndexes.Length > 1)
            {
                List<DocumentResult> reduceDocs = new List<DocumentResult>(docIdRank.Count);
                int lstIndex = wordIndexes.Length - 1;
                foreach (Core.SFQL.Parse.DocumentResultPoint drp in docIdRank.Values)
                {
                    DocumentResult* dr = drp.pDocumentResult;
                    //DocumentResult* dr1 = drp.pDocumentResult;
                    if (dr->LastIndex != lstIndex)
                    {
                        int sumWordRank = 10;
                        for (int k = dr->LastIndex + 1; k <= lstIndex; k++)
                        {
                            sumWordRank += wordIndexes[k].WordRank;
                        }

                        double delta = 1 / (double)sumWordRank;

                        dr->Score = (long)((double)dr->Score * delta);
                        //reduceDocs.Add(dr1);
                    }

                    if (dr->Score < 0)
                    {
                        dr->Score = long.MaxValue / 10;
                        //reduceDocs.Add(dr1);
                    }
                }

                //foreach (DocumentResult dr in reduceDocs)
                //{
                //    docIdRank[dr.DocId] = dr;
                //}
            }

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


        unsafe private void Calculate08(DocumentResultWhereDictionary upDict,
            ref DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            //Get max word doc list count
            int maxWordDocListCount = 0;
            int maxDocId = 0;
            int documentSum = 0;

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                maxWordDocListCount += wifq.WordIndex.WordDocList.Count;
                if (maxWordDocListCount > MinResultCount)
                {
                    maxDocId = Math.Max(maxDocId, wifq.WordIndex.WordDocList[wifq.WordIndex.WordDocList.Count - 1].DocumentId);
                    break;
                }

                maxDocId = Math.Max(maxDocId, wifq.WordIndex.WordDocList[wifq.WordIndex.WordDocList.Count - 1].DocumentId);
            }

            //maxWordDocListCount += maxWordDocListCount / 2;

            if (docIdRank.Count == 0)
            {
                if (maxWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(maxWordDocListCount);
                }
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            //Merge
            bool beginFilter = false;
            bool oneWordOptimize = this.CanLoadPartOfDocs && this.NoAndExpression && wordIndexes.Length == 1;
            int oneWordMaxCount = 0;

            for (int i = 0; i < wordIndexes.Length; i++)
            {
                WordIndexForQuery wifq = wordIndexes[i];

                if (docIdRank.Count > MinResultCount)
                {
                    beginFilter = true;
                }

                Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                for (int j = 0; j < wifq.WordIndexesLength; j++)
                {
                    //Entity.DocumentPositionList docList = wifq.WordIndex[j];
                    Entity.DocumentPositionList docList = wifqDocBuf[j];

                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (oneWordOptimize)
                    {
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
                    }


                    if (beginFilter)
                    {
                        if (docList.DocumentId > maxDocId)
                        {
                            documentSum += wifq.WordIndexesLength - j;
                            break;
                        }

                        if (!docIdRank.TryGetValue(docList.DocumentId, out drp))
                        {
                            documentSum++;
                            continue;
                        }
                    }

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }

                    //long rank = (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

                    //int score = 0;

                    //if (rank > int.MaxValue - 4000000)
                    //{
                    //    long high = rank % (int.MaxValue - 4000000);

                    //    score = int.MaxValue - 4000000 + (int)(high / 1000);
                    //}
                    //else
                    //{
                    //    score = (int)rank;
                    //}

                    //rank = (long)wifq.FieldRank * (long)wifq.WordRank * (long)score;

                    //Query.DocumentResult* pDocResult;

                    bool exits = drp.pDocumentResult != null;

                    if (!exits && i > 0)
                    {
                        exits = docIdRank.TryGetValue(docList.DocumentId, out drp);
                    }

                    if (exits)
                    {
                        drp.pDocumentResult->Score += score;

                        //docIdRank[docList.DocumentId] = docResult;
                    }
                    else
                    {

                        if (upDict == null)
                        {
                            //docResult = new DocumentResult(docList.DocumentId, rank);
                            //docIdRank.Add(docList.DocumentId, docResult);
                            docIdRank.Add(docList.DocumentId, score);
                        }
                        else
                        {
                            if (!upDict.Not)
                            {
                                if (upDict.ContainsKey(docList.DocumentId))
                                {
                                    //docResult = new DocumentResult(docList.DocumentId, rank);
                                    //docIdRank.Add(docList.DocumentId, docResult);
                                    docIdRank.Add(docList.DocumentId, score);
                                }
                            }
                            else
                            {
                                if (!upDict.ContainsKey(docList.DocumentId))
                                {
                                    //docResult = new DocumentResult(docList.DocumentId, rank);
                                    //docIdRank.Add(docList.DocumentId, docResult);
                                    docIdRank.Add(docList.DocumentId, score);
                                }
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

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

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

            if (wordIndexes.Length > 1)
            {
                List<DocumentResult> reduceDocs = new List<DocumentResult>(docIdRank.Count);
                int lstIndex = wordIndexes.Length - 1;
                foreach (Core.SFQL.Parse.DocumentResultPoint drp in docIdRank.Values)
                {
                    DocumentResult* dr = drp.pDocumentResult;
                    //DocumentResult* dr1 = drp.pDocumentResult;
                    if (dr->LastIndex != lstIndex)
                    {
                        int sumWordRank = 10;
                        for (int k = dr->LastIndex + 1; k <= lstIndex; k++)
                        {
                            sumWordRank += wordIndexes[k].WordRank;
                        }

                        double delta = 1 / (double)sumWordRank;

                        dr->Score = (long)((double)dr->Score * delta);
                    }

                    if (dr->Score < 0)
                    {
                        dr->Score = long.MaxValue / 10;
                    }
                }
            }

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


                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Norm_d_t * (long)docList.TotalWordsInThisDocument);

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
                return "Match1";
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

                _Norm_Ranks = 0;
                foreach (WordIndexForQuery wq in wordIndexList)
                {
                    _Norm_Ranks += wq.WordRank * wq.WordRank;
                }

                _Norm_Ranks = (long)Math.Sqrt(_Norm_Ranks);

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
