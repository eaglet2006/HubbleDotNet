using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Framework.DataStructure;

namespace Hubble.Core.SFQL.Parse
{
    class QueryResultSort
    {
        List<SyntaxAnalysis.Select.OrderBy> _OrderBys;
        Data.DBProvider _DBProvider;

        public QueryResultSort(List<SyntaxAnalysis.Select.OrderBy> orderBys, Data.DBProvider dbProvider)
        {
            _OrderBys = orderBys;
            _DBProvider = dbProvider;
        }

        public void Sort(Query.DocumentResult[] docResults)
        {
            Sort(docResults, -1);
        }

        public void Sort(Query.DocumentResult[] docResults, int top)
        {
            if (_OrderBys.Count <= 0)
            {
                return;
            }

            if (_OrderBys.Count == 1)
            {
                if (_OrderBys[0].Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (Query.DocumentResult docResult in docResults)
                    {
                        docResult.SortValue = docResult.DocId;
                        docResult.Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
                    }

                    QueryResultQuickSort<Query.DocumentResult>.TopSort(docResults, top, new Query.DocumentResultComparer());
                    return;

                }
                else if (_OrderBys[0].Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (Query.DocumentResult docResult in docResults)
                    {
                        docResult.SortValue = docResult.Score;
                        docResult.Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
                    }

                    QueryResultQuickSort<Query.DocumentResult>.TopSort(docResults, top, new Query.DocumentResultComparer());
                    return;
                }
                else
                {
                    Data.Field field = _DBProvider.GetField(_OrderBys[0].Name);

                    if (field != null)
                    {
                        if (field.IndexType != Hubble.Core.Data.Field.Index.Untokenized)
                        {
                            throw new ParseException(string.Format("Order by field name:{0} is not Untokenized Index!", _OrderBys[0].Name));
                        }


                        switch (field.DataType)
                        {
                            case Hubble.Core.Data.DataType.Date:
                            case Hubble.Core.Data.DataType.SmallDateTime:
                            case Hubble.Core.Data.DataType.Int:
                            case Hubble.Core.Data.DataType.SmallInt:
                            case Hubble.Core.Data.DataType.TinyInt:
                                {
                                    foreach (Query.DocumentResult docResult in docResults)
                                    {
                                        Data.Payload payLoad = _DBProvider.GetPayload(docResult.DocId);
                                        docResult.Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(docResult.Asc, field.DataType,
                                            payLoad.Data, field.TabIndex, field.DataLength);
                                        docResult.SortValue = sortInfo.IntValue;
                                    }

                                    QueryResultQuickSort<Query.DocumentResult>.TopSort(docResults, top, new Query.DocumentResultComparer());
                                    return;

                                }

                            case Hubble.Core.Data.DataType.BigInt:
                            case Hubble.Core.Data.DataType.DateTime:
                                {
                                    foreach (Query.DocumentResult docResult in docResults)
                                    {
                                        Data.Payload payLoad = _DBProvider.GetPayload(docResult.DocId);
                                        docResult.Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(docResult.Asc, field.DataType,
                                            payLoad.Data, field.TabIndex, field.DataLength);
                                        docResult.SortValue = sortInfo.LongValue;
                                    }

                                    QueryResultQuickSort<Query.DocumentResult>.TopSort(docResults, top, new Query.DocumentResultComparer());
                                    return;

                                }
                        }

 

                    }
                }


            }


            foreach (SyntaxAnalysis.Select.OrderBy orderBy in _OrderBys)
            {
                Data.Field field = _DBProvider.GetField(orderBy.Name);

                Query.SortType sortType = Hubble.Core.Query.SortType.None;
                bool isDocId = false;
                bool isScore = false;
                bool isAsc = orderBy.Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);

                if (field == null)
                {
                    if (orderBy.Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortType = Hubble.Core.Query.SortType.Long;
                        isDocId = true;
                    }
                    else if (orderBy.Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortType = Hubble.Core.Query.SortType.Long;
                        isScore = true;
                    }
                    else
                    {
                        throw new ParseException(string.Format("Unknown field name:{0}", orderBy.Name));
                    }
                }
                else
                {
                    if (field.IndexType != Hubble.Core.Data.Field.Index.Untokenized)
                    {
                        throw new ParseException(string.Format("Order by field name:{0} is not Untokenized Index!", orderBy.Name));
                    }
                }

                foreach (Query.DocumentResult docResult in docResults)
                {
                    if (docResult.SortInfoList == null)
                    {
                        docResult.SortInfoList = new List<Hubble.Core.Query.SortInfo>(2);
                    }

                    if (isDocId)
                    {
                        docResult.SortInfoList.Add(new Hubble.Core.Query.SortInfo(isAsc, sortType, docResult.DocId));
                    }
                    else if (isScore)
                    {
                        docResult.SortInfoList.Add(new Hubble.Core.Query.SortInfo(isAsc, sortType, docResult.Score));
                    }
                    else
                    {
                        if (docResult.Payload == null)
                        {
                            Data.Payload payLoad = _DBProvider.GetPayload(docResult.DocId);

                            if (payLoad == null)
                            {
                                throw new ParseException(string.Format("DocId={0} has not payload!", docResult.DocId));
                            }
                            
                            docResult.Payload = payLoad.Data;
                        }

                        docResult.SortInfoList.Add(Data.DataTypeConvert.GetSortInfo (isAsc,
                            field.DataType, docResult.Payload, field.TabIndex, field.DataLength));
                    }
                }
            }


            Array.Sort(docResults);

            //Has a bug of partial sort, make comments on following codes until it fixed. 
            //if (top <= 0 || top >= docResults.Length/2)
            //{
            //    Array.Sort(docResults);
            //}
            //else
            //{
            //    QuickSort<Query.DocumentResult>.TopSort(docResults, top, new Query.DocumentResultComparer());
            //}
        }
    }
}
