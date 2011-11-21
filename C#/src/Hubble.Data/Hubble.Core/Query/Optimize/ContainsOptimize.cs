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
using System.Linq;
using System.Text;

using Hubble.Core.Data;
using Hubble.Core.Entity;
using Hubble.Core.SFQL.Parse;
using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Query.Optimize
{
    public class ContainsOptimize : IQueryOptimize
    {
        bool _HasRandField = false;

        #region IQueryOptimize Members

        public OptimizeArgument Argument { get; set; }

        private WordIndexForQuery[] _WordIndexes;

        public WordIndexForQuery[] WordIndexes
        {
            get
            {
                return _WordIndexes;
            }

            set
            {
                _WordIndexes = value;
            }
        }

        unsafe private bool GetNext(WordIndexForQuery fstWifq, 
            ref OriginalDocumentPositionList fstODPL, ref Entity.DocumentPositionList fstDPL)
        {
            fstWifq.WordIndex.GetNextOriginal(ref fstODPL);
            fstODPL.ToDocumentPositionList(ref fstDPL);

            return fstODPL.DocumentId >= 0;
#if a
            if (!_HasRandField)
            {
            }
            else
            {
                if (_CurDocidPayloadIndex >= _CurDocidPayloadsCount)
                {
                    _CurDocidPayloadsCount = LoadDocIdPayloads();

                    if (_CurDocidPayloadsCount <= 0)
                    {
                        fstODPL.DocumentId = -1;
                        return;
                        //return new OriginalDocumentPositionList(-1);
                    }
                }

                fstODPL.DocumentId = _DocidPayloads[_CurDocidPayloadIndex].DocumentId;
                fstODPL.CountAndWordCount = _DocidPayloads[_CurDocidPayloadIndex].CountAndWordCount;
                //odpl = _DocidPayloads[_CurDocidPayloadIndex++];
                _CurDocidPayloadIndex++;
                return true;
            }
#endif
        }


        private void Init()
        {
            Data.Field rankField = Argument.DBProvider.GetField("Rank");

            if (rankField != null)
            {
                if (rankField.DataType == Hubble.Core.Data.DataType.Int &&
                    rankField.IndexType == Hubble.Core.Data.Field.Index.Untokenized)
                {
                    _HasRandField = true;
                }
            }
        }

        private unsafe void CalculateOrderByScore(Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary upDict, ref Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank)
        {
            DBProvider dBProvider = Argument.DBProvider;
            bool needGroupBy = Argument.NeedGroupBy;

            bool needFilterUntokenizedConditions = this.Argument.NeedFilterUntokenizedConditions;
            ExpressionTree untokenizedTree = this.Argument.UntokenizedTreeOnRoot;

            Query.DocumentResult documentResult;
            Query.DocumentResult* drp = &documentResult;


            //vars for delete
            bool haveRecordsDeleted = dBProvider.DelProvider.Count > 0;
            int[] delDocs = null;
            int curDelIndex = 0;
            int curDelDocid = 0;

            if (haveRecordsDeleted)
            {
                delDocs = dBProvider.DelProvider.DelDocs;
                curDelDocid = delDocs[curDelIndex];
            }


            double ratio = 1;
            WordIndexForQuery[] wordIndexes = WordIndexes;

            if (wordIndexes.Length > 1)
            {
                ratio = (double)2 / (double)(wordIndexes.Length - 1);
            }

            int wordIndexesLen = wordIndexes.Length;

            WordIndexForQuery fstWifq = wordIndexes[0]; //first word

            OriginalDocumentPositionList fstODPL = new OriginalDocumentPositionList();

            //Entity.DocumentPositionList fstDocList = fstWifq.WordIndex.GetNext();

            Entity.DocumentPositionList[] docListArr = new Hubble.Core.Entity.DocumentPositionList[wordIndexesLen];

            //docListArr[0] = fstDocList;
            //fstWifq.WordIndex.GetNextOriginal(ref fstODPL);
            //fstODPL.ToDocumentPositionList(ref docListArr[0]);

            GetNext(fstWifq, ref fstODPL, ref docListArr[0]);

            OriginalDocumentPositionList odpl = new OriginalDocumentPositionList();

            Entity.DocumentPositionList lastDocList
                = new Hubble.Core.Entity.DocumentPositionList();
            int top;

            //calculate top
            //If less then 100, set to 100
            if (this.Argument.End >= 0)
            {
                top = (1 + this.Argument.End / 100) * 100;

                if (top <= 0)
                {
                    top = 100;
                }

                if (this.Argument.End * 2 > top)
                {
                    top *= 2;
                }
            }
            else
            {
                top = int.MaxValue;
            }

            PriorQueue<Docid2Long> priorQueue = new PriorQueue<Docid2Long>(top, new DocIdLongComparer(false));

            long lastMinScore = 0;
            int rows = 0;

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
                    //Process untokenized conditions.
                    //If is not matched, get the next one.
                    if (needFilterUntokenizedConditions)
                    {
                        int docId = firstDocId;
                        drp->DocId = docId;
                        drp->PayloadData = dBProvider.GetPayloadDataWithShareLock(docId);
                        if (!ParseWhere.GetComparisionExpressionValue(dBProvider, drp,
                            untokenizedTree))
                        {
                            GetNext(fstWifq, ref fstODPL, ref docListArr[0]);
                            continue;
                        }
                    }

                    //Matched
                    //Caculate score

                    long totalScore = 0;
                    lastDocList.Count = 0;
                    lastDocList.FirstPosition = 0;

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

                        lastDocList.Count = docList.Count;
                        lastDocList.FirstPosition = docList.FirstPosition;

                        totalScore += (long)(score * delta);
                    } //End for cycle

                    if (haveRecordsDeleted)
                    {
                        if (curDelIndex < delDocs.Length)
                        {
                            //If docid deleted, get next
                            if (firstDocId == curDelDocid)
                            {
                                GetNext(fstWifq, ref fstODPL, ref docListArr[0]);
                                continue;
                            }
                            else if (firstDocId > curDelDocid)
                            {
                                while (curDelIndex < delDocs.Length && curDelDocid < firstDocId)
                                {
                                    curDelIndex++;

                                    if (curDelIndex >= delDocs.Length)
                                    {
                                        haveRecordsDeleted = false;
                                        break;
                                    }

                                    curDelDocid = delDocs[curDelIndex];
                                }

                                if (curDelIndex < delDocs.Length)
                                {
                                    if (firstDocId == curDelDocid)
                                    {
                                        GetNext(fstWifq, ref fstODPL, ref docListArr[0]);
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    if (needGroupBy)
                    {
                        docIdRank.AddToGroupByCollection(firstDocId);
                    }

                    if (_HasRandField)
                    {
                        int rank = dBProvider.SharedPayloadProvider.GetPayloadRank(firstDocId);
                        totalScore *= rank;
                        if (totalScore < 0)
                        {
                            totalScore = long.MaxValue - 4000000;
                        }
                    }

                    if (rows >= top)
                    {
                        rows++;

                        if (lastMinScore < totalScore)
                        {
                            priorQueue.Add(new Docid2Long(firstDocId, totalScore));
                            lastMinScore = priorQueue.Last.Value1;
                        }
                    }
                    else
                    {
                        priorQueue.Add(new Docid2Long(firstDocId, totalScore));
                        rows++;

                        if (rows == top)
                        {
                            lastMinScore = priorQueue.Last.Value1;
                        }
                    }

                    //docIdRank.Add(firstDocId, totalScore);
                }//if (curWord >= wordIndexesLen)

                GetNext(fstWifq, ref fstODPL, ref docListArr[0]);

                //fstWifq.WordIndex.GetNextOriginal(ref fstODPL);
                //fstODPL.ToDocumentPositionList(ref docListArr[0]);

            }

            docIdRank.RelTotalCount = rows;

            foreach (Docid2Long docid2Long in priorQueue.ToArray())
            {
                long score = docid2Long.Value1;

                if (score < 0)
                {
                    //Overflow
                    score = long.MaxValue - 4000000;
                }

                docIdRank.Add(docid2Long.DocId, new DocumentResult(docid2Long.DocId, score));
            }

            docIdRank.Sorted = true;
        }


        private unsafe void CalculateNormal(Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary upDict, ref Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank)
        {

            DBProvider dBProvider = Argument.DBProvider;
            bool needGroupBy = Argument.NeedGroupBy;

            bool needFilterUntokenizedConditions = this.Argument.NeedFilterUntokenizedConditions;
            ExpressionTree untokenizedTree = this.Argument.UntokenizedTreeOnRoot;

            Query.DocumentResult documentResult;
            Query.DocumentResult* drp = &documentResult;

            //vars for delete
            bool haveRecordsDeleted = dBProvider.DelProvider.Count > 0;
            int[] delDocs = null;
            int curDelIndex = 0;
            int curDelDocid = 0;

            Field[] orderByFields;
            DocId2LongComparer comparer = DocId2LongComparer.Generate(
                dBProvider, Argument.OrderBys, out orderByFields);

            if (haveRecordsDeleted)
            {
                delDocs = dBProvider.DelProvider.DelDocs;
                curDelDocid = delDocs[curDelIndex];
            }


            double ratio = 1;
            WordIndexForQuery[] wordIndexes = WordIndexes;

            if (wordIndexes.Length > 1)
            {
                ratio = (double)2 / (double)(wordIndexes.Length - 1);
            }

            int wordIndexesLen = wordIndexes.Length;

            WordIndexForQuery fstWifq = wordIndexes[0]; //first word

            OriginalDocumentPositionList fstODPL = new OriginalDocumentPositionList();

            //Entity.DocumentPositionList fstDocList = fstWifq.WordIndex.GetNext();

            Entity.DocumentPositionList[] docListArr = new Hubble.Core.Entity.DocumentPositionList[wordIndexesLen];

            //docListArr[0] = fstDocList;
            //fstWifq.WordIndex.GetNextOriginal(ref fstODPL);
            //fstODPL.ToDocumentPositionList(ref docListArr[0]);

            GetNext(fstWifq, ref fstODPL, ref docListArr[0]);

            OriginalDocumentPositionList odpl = new OriginalDocumentPositionList();

            Entity.DocumentPositionList lastDocList
                = new Hubble.Core.Entity.DocumentPositionList();
            int top;

            //calculate top
            //If less then 100, set to 100
            if (this.Argument.End >= 0)
            {
                top = (1 + this.Argument.End / 100) * 100;

                if (top <= 0)
                {
                    top = 100;
                }

                if (this.Argument.End * 2 > top)
                {
                    top *= 2;
                }
            }
            else
            {
                top = int.MaxValue;
            }

            PriorQueue<Docid2Long> priorQueue = new PriorQueue<Docid2Long>(top, new DocIdLongComparer(false));

            int rows = 0;

            Docid2Long last = new Docid2Long();
            last.DocId = -1;

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
                    //Process untokenized conditions.
                    //If is not matched, get the next one.
                    if (needFilterUntokenizedConditions)
                    {
                        int docId = firstDocId;
                        drp->DocId = docId;
                        drp->PayloadData = dBProvider.GetPayloadDataWithShareLock(docId);
                        if (!ParseWhere.GetComparisionExpressionValue(dBProvider, drp,
                            untokenizedTree))
                        {
                            GetNext(fstWifq, ref fstODPL, ref docListArr[0]);
                            continue;
                        }
                    }

                    //Matched
                    //Caculate score

                    long totalScore = 0;
                    lastDocList.Count = 0;
                    lastDocList.FirstPosition = 0;

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

                        lastDocList.Count = docList.Count;
                        lastDocList.FirstPosition = docList.FirstPosition;

                        totalScore += (long)(score * delta);
                    } //End for cycle

                    if (haveRecordsDeleted)
                    {
                        if (curDelIndex < delDocs.Length)
                        {
                            //If docid deleted, get next
                            if (firstDocId == curDelDocid)
                            {
                                GetNext(fstWifq, ref fstODPL, ref docListArr[0]);
                                continue;
                            }
                            else if (firstDocId > curDelDocid)
                            {
                                while (curDelIndex < delDocs.Length && curDelDocid < firstDocId)
                                {
                                    curDelIndex++;

                                    if (curDelIndex >= delDocs.Length)
                                    {
                                        haveRecordsDeleted = false;
                                        break;
                                    }

                                    curDelDocid = delDocs[curDelIndex];
                                }

                                if (curDelIndex < delDocs.Length)
                                {
                                    if (firstDocId == curDelDocid)
                                    {
                                        GetNext(fstWifq, ref fstODPL, ref docListArr[0]);
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    if (needGroupBy)
                    {
                        docIdRank.AddToGroupByCollection(firstDocId);
                    }

                    if (_HasRandField)
                    {
                        int rank = dBProvider.SharedPayloadProvider.GetPayloadRank(firstDocId);
                        totalScore *= rank;
                        if (totalScore < 0)
                        {
                            totalScore = long.MaxValue - 4000000;
                        }
                    }

                    Docid2Long cur = new Docid2Long();

                    if (firstDocId == 23659)
                    {
                        Console.WriteLine(firstDocId);
                    }

                    if (rows >= top)
                    {
                        rows++;

                        cur.DocId = firstDocId;

                        Docid2Long.Generate(ref cur, dBProvider, orderByFields, totalScore);

                        if (comparer.Compare(last, cur) > 0)
                        {
                            priorQueue.Add(cur);
                            last = priorQueue.Last;
                        }
                    }
                    else
                    {
                        cur.DocId = firstDocId;

                        Docid2Long.Generate(ref cur, dBProvider, orderByFields, totalScore);
                        priorQueue.Add(cur);

                        rows++;

                        if (rows == top)
                        {
                            last = priorQueue.Last;
                        }
                    }

                    //docIdRank.Add(firstDocId, totalScore);
                }//if (curWord >= wordIndexesLen)

                GetNext(fstWifq, ref fstODPL, ref docListArr[0]);

                //fstWifq.WordIndex.GetNextOriginal(ref fstODPL);
                //fstODPL.ToDocumentPositionList(ref docListArr[0]);

            }

            docIdRank.RelTotalCount = rows;

            foreach (Docid2Long docid2Long in priorQueue.ToArray())
            {
                long score = comparer.GetScore(docid2Long);

                if (score < 0)
                {
                    //Overflow
                    score = long.MaxValue - 4000000;
                }

                docIdRank.Add(docid2Long.DocId, new DocumentResult(docid2Long.DocId, score));
            }

            docIdRank.Sorted = true;
        }

        public unsafe void CalculateOptimize(Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary upDict, ref Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank)
        {
            Init();
            
            Argument.DBProvider.SharedPayloadProvider.EnterPayloladShareLock();

            try
            {
                if (Argument.IsOrderByScoreDesc())
                {
                    CalculateOrderByScore(upDict, ref docIdRank);
                }
                else
                {
                    CalculateNormal(upDict, ref docIdRank);
                }
            }
            finally
            {
                Argument.DBProvider.SharedPayloadProvider.LeavePayloadShareLock();
            }
        }

        #endregion
    }
}
