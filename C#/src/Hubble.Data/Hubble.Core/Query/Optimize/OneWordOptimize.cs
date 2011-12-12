using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.Data;
using Hubble.Core.Entity;
using Hubble.Core.SFQL.Parse;
using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Query.Optimize
{

    public class OneWordOptimize : IQueryOptimize
    {
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

        private Store.IndexReader _IndexReader;
        private bool _HasRandField = false;
        private int _RankTab;

        private OriginalDocumentPositionList[] _DocidPayloads = null;
        private int _CurDocidPayloadIndex = 0;
        private int _CurDocidPayloadsCount = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>load count</returns>
        private int LoadDocIdPayloads()
        {
            DBProvider dBProvider = Argument.DBProvider;


            int count = 0;
            for (; count < _DocidPayloads.Length; count++)
            {
                if (!_IndexReader.GetNextOriginal(ref _DocidPayloads[count]))
                {
                    break;
                }

                //if (docList.DocumentId < 0)
                //{
                //    break;
                //}
                //else
                //{
                //    _DocidPayloads[count] = docList;
                //}
            }

            if (count > 0)
            {
                dBProvider.FillPayloadRank(_RankTab, count, _DocidPayloads);
            }

            _CurDocidPayloadIndex = 0;

            return count;
        }

        unsafe private bool GetNext(ref Entity.OriginalDocumentPositionList odpl)
        {
            if (!_HasRandField)
            {
                return _IndexReader.GetNextOriginal(ref odpl);
            }
            else
            {
                if (_CurDocidPayloadIndex >= _CurDocidPayloadsCount)
                {
                    _CurDocidPayloadsCount = LoadDocIdPayloads();

                    if (_CurDocidPayloadsCount <= 0)
                    {
                        odpl.DocumentId = -1;
                        return false;
                        //return new OriginalDocumentPositionList(-1);
                    }
                }

                odpl.DocumentId = _DocidPayloads[_CurDocidPayloadIndex].DocumentId;
                odpl.CountAndWordCount = _DocidPayloads[_CurDocidPayloadIndex].CountAndWordCount;
                //odpl = _DocidPayloads[_CurDocidPayloadIndex++];
                _CurDocidPayloadIndex++;
                return true;
            }
        }

        unsafe public void CalculateOptimizeOrderByScoreDesc(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank)
        {
            Argument.DBProvider.SharedPayloadProvider.EnterPayloladShareLock();
            bool needFilterUntokenizedConditions = this.Argument.NeedFilterUntokenizedConditions;
            ExpressionTree untokenizedTree = this.Argument.UntokenizedTreeOnRoot;

            Query.DocumentResult documentResult;
            Query.DocumentResult* drp = &documentResult;

            try
            {

                DBProvider dBProvider = Argument.DBProvider;
                bool needGroupBy = Argument.NeedGroupBy;

                WordIndexForQuery wifq = WordIndexes[0];
                _IndexReader = wifq.WordIndex.IndexReader;

                Data.Field rankField = Argument.DBProvider.GetField("Rank");

                if (rankField != null)
                {
                    if (rankField.DataType == Hubble.Core.Data.DataType.Int &&
                        rankField.IndexType == Hubble.Core.Data.Field.Index.Untokenized)
                    {
                        _HasRandField = true;
                        _RankTab = rankField.TabIndex;
                        _DocidPayloads = new OriginalDocumentPositionList[2 * 1024];
                        _CurDocidPayloadIndex = _DocidPayloads.Length;
                    }
                }

                if (_IndexReader != null)
                {
                    int top;

                    //vars for delete
                    bool haveRecordsDeleted = dBProvider.DelProvider.Count > 0;
                    int[] delDocs = null;
                    int curDelIndex = 0;
                    int curDelDocid = 0;
                    int groupByCount = 0;
                    int groupByLen = dBProvider.Table.GroupByLimit;
                    int groupByStep = 1;
                    int groupByIndex = 0;

                    if (needGroupBy)
                    {
                        groupByStep = wifq.RelTotalCount / groupByLen;

                        if (groupByStep <= 0)
                        {
                            groupByStep = 1;
                        }
                    }

                    if (haveRecordsDeleted)
                    {
                        delDocs = dBProvider.DelProvider.DelDocs;
                        curDelDocid = delDocs[curDelIndex];
                    }

                    try
                    {
                        //calculate top
                        //If less then 100, set to 100
                        if (this.Argument.End >= 0)
                        {
                            top = (1 + this.Argument.End / 100) * 100;

                            if (top <= 0)
                            {
                                top = 100;
                            }

                            //if (this.Argument.End * 2 > top)
                            //{
                            //    top *= 2;
                            //}
                        }
                        else
                        {
                            top = int.MaxValue;
                        }

                        PriorQueue<DocidCount> priorQueue = new PriorQueue<DocidCount>(top, new DocIdCountComparer());
                        int rows = 0;

                        Entity.OriginalDocumentPositionList docList = new OriginalDocumentPositionList();

                        bool notEOF = GetNext(ref docList);

                        Index.WordIndexReader wordIndexReader = wifq.WordIndex;

                        int lastCount = 0;
                        int relCount = 0;

                        while (notEOF)
                        {
                            //Process untokenized conditions.
                            //If is not matched, get the next one.
                            if (needFilterUntokenizedConditions)
                            {
                                int docId = docList.DocumentId;
                                drp->DocId = docId;
                                drp->PayloadData = dBProvider.GetPayloadDataWithShareLock(docId);
                                if (!ParseWhere.GetComparisionExpressionValue(dBProvider, drp,
                                    untokenizedTree))
                                {
                                    notEOF = GetNext(ref docList);
                                    continue;
                                }
                            }


                            //Process deleted records
                            if (haveRecordsDeleted)
                            {
                                if (curDelIndex < delDocs.Length)
                                {
                                    //If docid deleted, get next
                                    if (docList.DocumentId == curDelDocid)
                                    {
                                        notEOF = GetNext(ref docList);
                                        continue;
                                    }
                                    else if (docList.DocumentId > curDelDocid)
                                    {
                                        while (curDelIndex < delDocs.Length && curDelDocid < docList.DocumentId)
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
                                            if (docList.DocumentId == curDelDocid)
                                            {
                                                notEOF = GetNext(ref docList);
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }

                            if (needGroupBy)
                            {
                                if (groupByCount < groupByLen)
                                {
                                    if (groupByIndex >= groupByStep)
                                    {
                                        groupByIndex = 0;
                                    }

                                    if (groupByIndex == 0)
                                    {
                                        docIdRank.AddToGroupByCollection(docList.DocumentId);
                                        groupByCount++;
                                    }

                                    groupByIndex++;
                                }
                            }

                            relCount++;

                            if (rows >= top)
                            {
                                int count = docList.CountAndWordCount / 8;

                                if (lastCount < count)
                                {
                                    priorQueue.Add(new DocidCount(docList.DocumentId, count, docList.TotalWordsInThisDocument));
                                    lastCount = priorQueue.Last.Count;
                                    rows++;
                                }
                            }
                            else
                            {
                                priorQueue.Add(new DocidCount(docList.DocumentId,
                                    docList.CountAndWordCount / 8, docList.TotalWordsInThisDocument));
                                rows++;

                                if (rows == top)
                                {
                                    lastCount = priorQueue.Last.Count;
                                }
                            }

                            notEOF = GetNext(ref docList);
                        }

                        docIdRank.RelTotalCount = relCount;

                        foreach (DocidCount docidCount in priorQueue.ToArray())
                        {
                            long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docidCount.Count * (long)1000000 /
                                ((long)wifq.Sum_d_t * (long)docidCount.TotalWordsInThisDocument);

                            if (score < 0)
                            {
                                //Overflow
                                score = long.MaxValue - 4000000;
                            }

                            docIdRank.Add(docidCount.DocId, new DocumentResult(docidCount.DocId, score));
                        }
                    }
                    finally
                    {

                    }

                    docIdRank.Sorted = true;
                }
            }
            finally
            {
                Argument.DBProvider.SharedPayloadProvider.LeavePayloadShareLock();
            }
        }

        /// <summary>
        /// order by except only order by score desc.
        /// </summary>
        /// <param name="upDict"></param>
        /// <param name="docIdRank"></param>
        unsafe public void CalculateOptimizeNormalOrderBy(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank)
        {
            DBProvider dBProvider = Argument.DBProvider;
            Argument.DBProvider.SharedPayloadProvider.EnterPayloladShareLock();

            bool needFilterUntokenizedConditions = this.Argument.NeedFilterUntokenizedConditions;
            ExpressionTree untokenizedTree = this.Argument.UntokenizedTreeOnRoot;

            Query.DocumentResult documentResult;
            Query.DocumentResult* drp = &documentResult;

            try
            {
                Field[] orderByFields;

                DocId2LongComparer comparer = DocId2LongComparer.Generate(
                    dBProvider, Argument.OrderBys, out orderByFields);

                bool needGroupBy = Argument.NeedGroupBy;

                WordIndexForQuery wifq = WordIndexes[0];
                _IndexReader = wifq.WordIndex.IndexReader;

                Data.Field rankField = Argument.DBProvider.GetField("Rank");

                if (rankField != null)
                {
                    if (rankField.DataType == Hubble.Core.Data.DataType.Int &&
                        rankField.IndexType == Hubble.Core.Data.Field.Index.Untokenized)
                    {
                        _HasRandField = true;
                        _RankTab = rankField.TabIndex;
                        _DocidPayloads = new OriginalDocumentPositionList[2 * 1024];
                        _CurDocidPayloadIndex = _DocidPayloads.Length;
                    }
                }

                if (_IndexReader != null)
                {
                    int top;

                    //vars for delete
                    bool haveRecordsDeleted = dBProvider.DelProvider.Count > 0;
                    int[] delDocs = null;
                    int curDelIndex = 0;
                    int curDelDocid = 0;
                    int groupByCount = 0;
                    int groupByLen = dBProvider.Table.GroupByLimit;
                    int groupByStep = 1;
                    int groupByIndex = 0;

                    if (needGroupBy)
                    {
                        groupByStep = wifq.RelTotalCount / groupByLen;

                        if (groupByStep <= 0)
                        {
                            groupByStep = 1;
                        }
                    }

                    if (haveRecordsDeleted)
                    {
                        delDocs = dBProvider.DelProvider.DelDocs;
                        curDelDocid = delDocs[curDelIndex];
                    }

                    try
                    {
                        //calculate top
                        //If less then 100, set to 100
                        if (this.Argument.End >= 0)
                        {
                            top = (1 + this.Argument.End / 100) * 100;

                            if (top <= 0)
                            {
                                top = 100;
                            }

                            //if (this.Argument.End * 2 > top)
                            //{
                            //    top *= 2;
                            //}
                        }
                        else
                        {
                            top = int.MaxValue;
                        }

                        PriorQueue<Docid2Long> priorQueue = new PriorQueue<Docid2Long>(top, comparer);
                        int rows = 0;

                        Entity.OriginalDocumentPositionList docList = new OriginalDocumentPositionList();

                        bool notEOF = GetNext(ref docList);

                        Index.WordIndexReader wordIndexReader = wifq.WordIndex;

                        Docid2Long last = new Docid2Long();
                        last.DocId = -1;

                        int relCount = 0;

                        while (notEOF)
                        {
                            //Process untokenized conditions.
                            //If is not matched, get the next one.
                            if (needFilterUntokenizedConditions)
                            {
                                int docId = docList.DocumentId;
                                drp->DocId = docId;
                                drp->PayloadData = dBProvider.GetPayloadDataWithShareLock(docId);
                                if (!ParseWhere.GetComparisionExpressionValue(dBProvider, drp,
                                    untokenizedTree))
                                {
                                    notEOF = GetNext(ref docList);
                                    continue;
                                }
                            }

                            //Process deleted records
                            if (haveRecordsDeleted)
                            {
                                if (curDelIndex < delDocs.Length)
                                {
                                    //If docid deleted, get next
                                    if (docList.DocumentId == curDelDocid)
                                    {
                                        notEOF = GetNext(ref docList);
                                        continue;
                                    }
                                    else if (docList.DocumentId > curDelDocid)
                                    {
                                        while (curDelIndex < delDocs.Length && curDelDocid < docList.DocumentId)
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
                                            if (docList.DocumentId == curDelDocid)
                                            {
                                                notEOF = GetNext(ref docList);
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }

                            if (needGroupBy)
                            {
                                if (groupByCount < groupByLen)
                                {
                                    if (groupByIndex >= groupByStep)
                                    {
                                        groupByIndex = 0;
                                    }

                                    if (groupByIndex == 0)
                                    {
                                        docIdRank.AddToGroupByCollection(docList.DocumentId);
                                        groupByCount++;
                                    }

                                    groupByIndex++;
                                }
                            }

                            relCount++;

                            Docid2Long cur = new Docid2Long();

                            if (rows >= top)
                            {
                                int score = docList.CountAndWordCount / 8; //one word, score = count

                                cur.DocId = docList.DocumentId;
                                cur.Rank = docList.TotalWordsInThisDocument;

                                Docid2Long.Generate(ref cur, dBProvider, orderByFields, score);

                                if (comparer.Compare(last, cur) > 0)
                                {
                                    priorQueue.Add(cur);
                                    last = priorQueue.Last;
                                }
                            }
                            else
                            {
                                int score = docList.CountAndWordCount / 8; //one word, score = count

                                cur.DocId = docList.DocumentId;
                                cur.Rank = docList.TotalWordsInThisDocument;

                                Docid2Long.Generate(ref cur, dBProvider, orderByFields, score);

                                priorQueue.Add(cur);
                                rows++;

                                if (rows == top)
                                {
                                    last = priorQueue.Last;
                                }
                            }

                            notEOF = GetNext(ref docList);
                        }

                        docIdRank.RelTotalCount = relCount;

                        foreach (Docid2Long docid2Long in priorQueue.ToArray())
                        {
                            //long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * comparer.GetScore(docid2Long) * (long)1000000 /
                            //    ((long)wifq.Sum_d_t * (long)docid2Long.Rank); //use Rank store TotalWordsInThisDocument

                            long score = (long)wifq.FieldRank * (long)wifq.WordRank * comparer.GetScore(docid2Long); //use Rank store TotalWordsInThisDocument

                            if (score < 0)
                            {
                                //Overflow
                                score = long.MaxValue - 4000000;
                            }

                            docIdRank.Add(docid2Long.DocId, new DocumentResult(docid2Long.DocId, score));
                        }
                    }
                    finally
                    {

                    }

                    docIdRank.Sorted = true;
                }
            }
            finally
            {
                Argument.DBProvider.SharedPayloadProvider.LeavePayloadShareLock();
            }
        }

        unsafe public void CalculateOptimize(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank)
        {
            if (Argument.IsOrderByScoreDesc())
            {
                CalculateOptimizeOrderByScoreDesc(upDict, ref docIdRank);
            }
            else
            {
                CalculateOptimizeNormalOrderBy(upDict, ref docIdRank);
            }
        }
    }

}
