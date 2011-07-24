using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.Data;
using Hubble.Core.Entity;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Query.Optimize
{

    public class OneWordOptimize : IQueryOptimize
    {
        private DBProvider _DBProvider;

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
                _DBProvider.FillPayloadRank(_RankTab, count, _DocidPayloads);
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

        unsafe public void CalculateOneWordOptimize(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery wifq)
        {
            _IndexReader = wifq.WordIndex.IndexReader;

            Data.Field rankField = DBProvider.GetField("Rank");

            if (rankField != null)
            {
                if (rankField.DataType == Hubble.Core.Data.DataType.Int &&
                    rankField.IndexType == Hubble.Core.Data.Field.Index.Untokenized)
                {
                    _HasRandField = true;
                    _RankTab = rankField.TabIndex;
                    _DocidPayloads = new OriginalDocumentPositionList[2*1024];
                    _CurDocidPayloadIndex = _DocidPayloads.Length;
                }
            }

            if (_IndexReader != null)
            {
                int top;

                //vars for delete
                bool haveRecordsDeleted = _DBProvider.DelProvider.Count > 0;
                int[] delDocs = null;
                int curDelIndex = 0;
                int curDelDocid = 0;

                if (haveRecordsDeleted)
                {
                    delDocs = _DBProvider.DelProvider.DelDocs;
                    curDelDocid = delDocs[curDelIndex];
                }

                try
                {
                    //calculate top
                    //If less then 100, set to 100
                    if (this.End >= 0)
                    {
                        top = (1 + this.End / 100) * 100;

                        if (top <= 0)
                        {
                            top = 100;
                        }

                        if (this.End * 2 > top)
                        {
                            top *= 2;
                        }
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

                        if (_NeedGroupBy)
                        {
                            docIdRank.AddToGroupByCollection(docList.DocumentId);
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

    }

}
