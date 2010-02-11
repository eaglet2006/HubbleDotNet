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

        internal void Sort(Query.DocumentResult[] docResults)
        {
            Sort(docResults, -1);
        }

        unsafe internal void Sort(Query.DocumentResult[] docResults, int top)
        {
            if (_OrderBys.Count <= 0)
            {
                return;
            }

            if (_OrderBys.Count == 1)
            {
                if (_OrderBys[0].Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                {
                    for(int i = 0; i < docResults.Length; i++)
                    {
                        docResults[i].SortValue = docResults[i].DocId;
                        docResults[i].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
                    }

                    QueryResultQuickSort<Query.DocumentResult>.TopSort(docResults, top, new Query.DocumentResultComparer());
                    return;

                }
                else if (_OrderBys[0].Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                {
                    for (int i = 0; i < docResults.Length; i++)
                    {
                        docResults[i].SortValue = docResults[i].Score;
                        docResults[i].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
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
                                    for (int i = 0; i < docResults.Length; i++)
                                    {
                                        int* payLoadData = _DBProvider.GetPayloadData(docResults[i].DocId);
                                        docResults[i].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(docResults[i].Asc, field.DataType,
                                            payLoadData, field.TabIndex, field.DataLength);
                                        docResults[i].SortValue = sortInfo.IntValue;
                                    }

                                    QueryResultQuickSort<Query.DocumentResult>.TopSort(docResults, top, new Query.DocumentResultComparer());
                                    return;

                                }

                            case Hubble.Core.Data.DataType.BigInt:
                            case Hubble.Core.Data.DataType.DateTime:
                                {
                                    for (int i = 0; i < docResults.Length; i++)
                                    {
                                        int* payLoadData = _DBProvider.GetPayloadData(docResults[i].DocId);
                                        docResults[i].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(docResults[i].Asc, field.DataType,
                                            payLoadData, field.TabIndex, field.DataLength);
                                        docResults[i].SortValue = sortInfo.LongValue;
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

                for(int i = 0; i < docResults.Length; i++)
                {
                    if (docResults[i].SortInfoList == null)
                    {
                        docResults[i].SortInfoList = new List<Hubble.Core.Query.SortInfo>(2);
                    }

                    if (isDocId)
                    {
                        docResults[i].SortInfoList.Add(new Hubble.Core.Query.SortInfo(isAsc, sortType, docResults[i].DocId));
                    }
                    else if (isScore)
                    {
                        docResults[i].SortInfoList.Add(new Hubble.Core.Query.SortInfo(isAsc, sortType, docResults[i].Score));
                    }
                    else
                    {
                        if (docResults[i].PayloadData == null)
                        {
                            int* payloadData = _DBProvider.GetPayloadData(docResults[i].DocId);

                            if (payloadData == null)
                            {
                                throw new ParseException(string.Format("DocId={0} has not payload!", docResults[i].DocId));
                            }

                            docResults[i].PayloadData = payloadData;
                        }

                        docResults[i].SortInfoList.Add(Data.DataTypeConvert.GetSortInfo(isAsc,
                            field.DataType, docResults[i].PayloadData, field.TabIndex, field.DataLength));
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
