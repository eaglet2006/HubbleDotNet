using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hubble.Framework.DataStructure;
using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;
using Hubble.Core.Query;
using Hubble.Core.SFQL.SyntaxAnalysis;

namespace Hubble.Core.Query.Optimize
{
    class MatchOptimize : IQueryOptimize
    {
        bool _HasRankField = false;

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

        unsafe private void CalculateWithPositionOrderByScoreDesc(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
                 ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            DBProvider dbProvider = Argument.DBProvider;

            bool needFilterUntokenizedConditions = this.Argument.NeedFilterUntokenizedConditions;
            ExpressionTree untokenizedTree = this.Argument.UntokenizedTreeOnRoot;

            if (upDict != null)
            {
                throw new ParseException("UpDict is not null!");
            }

            //Calculate top
            int top;

            if (Argument.End >= 0)
            {
                top = (1 + Argument.End / 100) * 100;

                if (top <= 0)
                {
                    top = 100;
                }
            }
            else
            {
                top = int.MaxValue;
            }

            PriorQueue<Docid2Long> priorQueue = null;
            List<Docid2Long> docid2longList = null;

            if (top == int.MaxValue)
            {
                docid2longList = new List<Docid2Long>();
            }
            else
            {
                priorQueue = new PriorQueue<Docid2Long>(top, new DocIdLongComparer(false));
            }

            long lastMinScore = 0;
            int rows = 0;

            Core.SFQL.Parse.DocumentResultWhereDictionary groupByDict = Argument.NeedGroupBy ? docIdRank : null;


            MultiWordsDocIdEnumerator mwde = new MultiWordsDocIdEnumerator(wordIndexes, dbProvider, groupByDict, -1,
                needFilterUntokenizedConditions); //Changed at 2012-3-18, top optimize will effect search result, disable it.

            //MultiWordsDocIdEnumerator mwde = new MultiWordsDocIdEnumerator(wordIndexes, dbProvider, groupByDict, top, 
            //    needFilterUntokenizedConditions);

            Entity.OriginalDocumentPositionList odpl = new Hubble.Core.Entity.OriginalDocumentPositionList();

            mwde.GetNextOriginal(ref odpl);

            Entity.DocumentPositionList lastDocList
                = new Hubble.Core.Entity.DocumentPositionList();

            double ratio = 1;

            if (wordIndexes.Length > 1)
            {
                ratio = (double)2 / (double)(wordIndexes.Length - 1);
            }

            Query.DocumentResult documentResult;
            Query.DocumentResult* drp = &documentResult;
            int skipCount = 0; //skip by filter untokenized conditions

            while (odpl.DocumentId >= 0)
            {
                //Process untokenized conditions.
                //If is not matched, get the next one.
                if (needFilterUntokenizedConditions)
                {
                    int docId = odpl.DocumentId;
                    drp->DocId = docId;
                    drp->PayloadData = dbProvider.GetPayloadDataWithShareLock(docId);
                    if (!ParseWhere.GetComparisionExpressionValue(dbProvider, drp,
                        untokenizedTree))
                    {
                        mwde.GetNextOriginal(ref odpl);
                        skipCount++;
                        continue;
                    }
                }

                //Matched
                //Caculate score
                #region Caclate score

                long totalScore = 0;
                lastDocList.Count = 0;
                lastDocList.FirstPosition = 0;
                int lastWifqIndex = 0;

                for (int i = 0; i < mwde.SelectedCount; i++)
                {
                    int index = mwde.SelectedIndexes[i];

                    WordIndexForQuery wifq = mwde.WordIndexes[index];

                    Int16 count = (Int16)mwde.SelectedDocLists[i].Count;
                    int firstPosition = mwde.SelectedDocLists[i].FirstPosition;
                    int totalWordsInThisDocument = mwde.SelectedDocLists[i].TotalWordsInThisDocument;

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)count * (long)1000000 / ((long)wifq.Sum_d_t * (long)totalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }

                    double delta = 1;

                    if (i > 0)
                    {
                        //Calculate with position
                        double queryPositionDelta = wifq.FirstPosition - wordIndexes[lastWifqIndex].FirstPosition;
                        double positionDelta = firstPosition - lastDocList.FirstPosition;

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

                        delta = Math.Pow((1 / delta), ratio) * count * lastDocList.Count /
                            (double)(wifq.QueryCount * wordIndexes[lastWifqIndex].QueryCount);
                    }

                    lastDocList.Count = count;
                    lastDocList.FirstPosition = firstPosition;
                    lastWifqIndex = index;

                    totalScore += (long)(score * delta);
                } //End of score calculation

                if (_HasRankField)
                {
                    int rank = dbProvider.SharedPayloadProvider.GetPayloadRank(odpl.DocumentId);
                    totalScore *= rank;
                    if (totalScore < 0)
                    {
                        totalScore = long.MaxValue - 4000000;
                    }
                }

                //all of the words matched
                //10 times
                if (mwde.SelectedCount == wordIndexes.Length)
                {
                    totalScore *= 10;

                    if (totalScore < 0)
                    {
                        totalScore = long.MaxValue - 4000000;
                    }
                }

                #endregion

                //Insert to prior queue
                if (rows >= top)
                {
                    if (lastMinScore < totalScore)
                    {
                        priorQueue.Add(new Docid2Long(odpl.DocumentId, totalScore));
                        lastMinScore = priorQueue.Last.Value1;
                    }
                }
                else
                {
                    if (top == int.MaxValue)
                    {
                        docid2longList.Add(new Docid2Long(odpl.DocumentId, totalScore));
                    }
                    else
                    {
                        priorQueue.Add(new Docid2Long(odpl.DocumentId, totalScore));
                        rows++;

                        if (rows == top)
                        {
                            lastMinScore = priorQueue.Last.Value1;
                        }
                    }
                }

                mwde.GetNextOriginal(ref odpl);
            }

            docIdRank.RelTotalCount = mwde.TotalDocIdCount - skipCount;

            Docid2Long[] docid2longArr;

            if (top == int.MaxValue)
            {
                docid2longList.Sort(new DocIdLongComparer(false));
                docid2longArr = docid2longList.ToArray();
            }
            else
            {
                docid2longArr = priorQueue.ToArray();
            }

            foreach (Docid2Long docid2Long in docid2longArr)
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

        unsafe private void CalculateWithPositionOrderByNormal(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
                 ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            DBProvider dbProvider = Argument.DBProvider;

            bool needFilterUntokenizedConditions = this.Argument.NeedFilterUntokenizedConditions;
            ExpressionTree untokenizedTree = this.Argument.UntokenizedTreeOnRoot;
            bool orderbyIncludingScore = this.Argument.OrderByIncludingScore();

            if (upDict != null)
            {
                throw new ParseException("UpDict is not null!");
            }

            //Calculate top
            int top;

            if (Argument.End >= 0)
            {
                top = (1 + Argument.End / 100) * 100;

                if (top <= 0)
                {
                    top = 100;
                }
            }
            else
            {
                top = int.MaxValue;
            }

            PriorQueue<Docid2Long> priorQueue = null;
            List<Docid2Long> docid2longList = null;

            Field[] orderByFields;
            DocId2LongComparer comparer = DocId2LongComparer.Generate(
                dbProvider, Argument.OrderBys, out orderByFields);

            if (top == int.MaxValue)
            {
                docid2longList = new List<Docid2Long>();
            }
            else
            {
                priorQueue = new PriorQueue<Docid2Long>(top, comparer);
            }

            int rows = 0;

            Core.SFQL.Parse.DocumentResultWhereDictionary groupByDict = Argument.NeedGroupBy ? docIdRank : null;

            //Set top = -1 to get all the records
            MultiWordsDocIdEnumerator mwde = new MultiWordsDocIdEnumerator(wordIndexes, dbProvider, groupByDict, -1,
                needFilterUntokenizedConditions); 

            Entity.OriginalDocumentPositionList odpl = new Hubble.Core.Entity.OriginalDocumentPositionList();

            mwde.GetNextOriginal(ref odpl);

            Entity.DocumentPositionList lastDocList
                = new Hubble.Core.Entity.DocumentPositionList();

            double ratio = 1;

            if (wordIndexes.Length > 1)
            {
                ratio = (double)2 / (double)(wordIndexes.Length - 1);
            }

            Query.DocumentResult documentResult;
            Query.DocumentResult* drp = &documentResult;
            int skipCount = 0; //skip by filter untokenized conditions

            Docid2Long cur = new Docid2Long();
            Docid2Long last = new Docid2Long();
            last.DocId = -1;

            while (odpl.DocumentId >= 0)
            {
                //Process untokenized conditions.
                //If is not matched, get the next one.
                if (needFilterUntokenizedConditions)
                {
                    int docId = odpl.DocumentId;
                    drp->DocId = docId;
                    drp->PayloadData = dbProvider.GetPayloadDataWithShareLock(docId);
                    if (!ParseWhere.GetComparisionExpressionValue(dbProvider, drp,
                        untokenizedTree))
                    {
                        mwde.GetNextOriginal(ref odpl);
                        skipCount++;
                        continue;
                    }
                }

                //Matched
                //Caculate score
                long totalScore = 0;

                if (orderbyIncludingScore)
                {
                    #region Caclate score

                    lastDocList.Count = 0;
                    lastDocList.FirstPosition = 0;
                    int lastWifqIndex = 0;

                    for (int i = 0; i < mwde.SelectedCount; i++)
                    {
                        int index = mwde.SelectedIndexes[i];

                        WordIndexForQuery wifq = mwde.WordIndexes[index];

                        Int16 count = (Int16)mwde.SelectedDocLists[i].Count;
                        int firstPosition = mwde.SelectedDocLists[i].FirstPosition;
                        int totalWordsInThisDocument = mwde.SelectedDocLists[i].TotalWordsInThisDocument;

                        long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)count * (long)1000000 / ((long)wifq.Sum_d_t * (long)totalWordsInThisDocument);

                        if (score < 0)
                        {
                            //Overflow
                            score = long.MaxValue - 4000000;
                        }

                        double delta = 1;

                        if (i > 0)
                        {
                            //Calculate with position
                            double queryPositionDelta = wifq.FirstPosition - wordIndexes[lastWifqIndex].FirstPosition;
                            double positionDelta = firstPosition - lastDocList.FirstPosition;

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

                            delta = Math.Pow((1 / delta), ratio) * count * lastDocList.Count /
                                (double)(wifq.QueryCount * wordIndexes[lastWifqIndex].QueryCount);
                        }

                        lastDocList.Count = count;
                        lastDocList.FirstPosition = firstPosition;
                        lastWifqIndex = index;

                        totalScore += (long)(score * delta);
                    } //End of score calculation

                    if (_HasRankField)
                    {
                        int rank = dbProvider.SharedPayloadProvider.GetPayloadRank(odpl.DocumentId);
                        totalScore *= rank;
                        if (totalScore < 0)
                        {
                            totalScore = long.MaxValue - 4000000;
                        }
                    }

                    //all of the words matched
                    //10 times
                    if (mwde.SelectedCount == wordIndexes.Length)
                    {
                        totalScore *= 10;

                        if (totalScore < 0)
                        {
                            totalScore = long.MaxValue - 4000000;
                        }
                    }

                    #endregion
                }

                //Insert to prior queue
                if (rows >= top)
                {
                    cur.DocId = odpl.DocumentId;

                    Docid2Long.Generate(ref cur, dbProvider, orderByFields, totalScore);

                    if (comparer.Compare(last, cur) > 0)
                    {
                        priorQueue.Add(cur);
                        last = priorQueue.Last;
                    }
                }
                else
                {
                    if (top == int.MaxValue)
                    {
                        cur.DocId = odpl.DocumentId;

                        Docid2Long.Generate(ref cur, dbProvider, orderByFields, totalScore);
                        docid2longList.Add(cur);
                    }
                    else
                    {
                        cur.DocId = odpl.DocumentId;

                        Docid2Long.Generate(ref cur, dbProvider, orderByFields, totalScore);
                        priorQueue.Add(cur);

                        rows++;

                        if (rows == top)
                        {
                            last = priorQueue.Last;
                        }
                    }
                }

                mwde.GetNextOriginal(ref odpl);
            }

            docIdRank.RelTotalCount = mwde.TotalDocIdCount - skipCount;

            Docid2Long[] docid2longArr;

            if (top == int.MaxValue)
            {
                docid2longList.Sort(comparer);
                docid2longArr = docid2longList.ToArray();
            }
            else
            {
                docid2longArr = priorQueue.ToArray();
            }

            foreach (Docid2Long docid2Long in docid2longArr)
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

        
        private void Init()
        {
            Data.Field rankField = Argument.DBProvider.GetField("Rank");

            if (rankField != null)
            {
                if (rankField.DataType == Hubble.Core.Data.DataType.Int &&
                    rankField.IndexType == Hubble.Core.Data.Field.Index.Untokenized)
                {
                    _HasRankField = true;
                }
            }
        }


        public unsafe void CalculateOptimize(Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary upDict, ref Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank)
        {
            Init();

            DBProvider dbProvider = Argument.DBProvider;

            dbProvider.SharedPayloadProvider.EnterPayloladShareLock();

            try
            {
                if (Argument.IsOrderByScoreDesc())
                {
                    CalculateWithPositionOrderByScoreDesc(upDict, ref docIdRank, WordIndexes);
                }
                else
                {
                    CalculateWithPositionOrderByNormal(upDict, ref docIdRank, WordIndexes);
                }
            }
            finally
            {
                dbProvider.SharedPayloadProvider.LeavePayloadShareLock();
            }
        }

        #endregion
    }
}
