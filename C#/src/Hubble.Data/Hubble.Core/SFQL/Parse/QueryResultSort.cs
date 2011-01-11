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
using System.Diagnostics;

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

        internal void Sort(Query.DocumentResultForSort[] docResults)
        {
            Sort(docResults, -1);
        }

        unsafe internal void Sort(Query.DocumentResultForSort[] docResults, int top)
        {
            if (_OrderBys.Count <= 0)
            {
                return;
            }

            if (_OrderBys.Count == 1)
            {
                if (_OrderBys[0].Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (docResults.Length > 0)
                    {
                        docResults[0].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
                    }

                    for(int i = 0; i < docResults.Length; i++)
                    {
                        docResults[i].SortValue = docResults[i].DocId;
                    }

                    QueryResultHeapSort.TopSort(docResults, top);
                    //QueryResultQuickSort<Query.DocumentResultForSort>.TopSort(docResults, top, new Query.DocumentResultForSortComparer());
                    return;

                }
                else if (_OrderBys[0].Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (docResults.Length > 0)
                    {
                        docResults[0].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
                    }

                    for (int i = 0; i < docResults.Length; i++)
                    {
                        docResults[i].SortValue = docResults[i].Score;
                    }

                    QueryResultHeapSort.TopSort(docResults, top);
                    //QueryResultQuickSort<Query.DocumentResultForSort>.TopSort(docResults, top, new Query.DocumentResultForSortComparer());
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
                                    if (docResults.Length > 0)
                                    {
                                        docResults[0].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
                                    }

                                    _DBProvider.FillPayloadData(docResults);
          
                                    for (int i = 0; i < docResults.Length; i++)
                                    {
                                        int* payLoadData = docResults[i].PayloadData;

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(docResults[i].Asc, field.DataType,
                                            payLoadData, field.TabIndex, field.SubTabIndex, field.DataLength);
                                        docResults[i].SortValue = sortInfo.IntValue;
                                    }

                                    QueryResultHeapSort.TopSort(docResults, top);

                                    //QueryResultQuickSort<Query.DocumentResultForSort>.TopSort(docResults, top, new Query.DocumentResultForSortComparer());
                                    return;

                                }

                            case Hubble.Core.Data.DataType.BigInt:
                            case Hubble.Core.Data.DataType.DateTime:
                                {
                                    if (docResults.Length > 0)
                                    {
                                        docResults[0].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
                                    }

                                    _DBProvider.FillPayloadData(docResults);

                                    for (int i = 0; i < docResults.Length; i++)
                                    {
                                        int* payLoadData = docResults[i].PayloadData;

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(docResults[i].Asc, field.DataType,
                                            payLoadData, field.TabIndex, field.SubTabIndex, field.DataLength);
                                        docResults[i].SortValue = sortInfo.LongValue;
                                    }

                                    QueryResultHeapSort.TopSort(docResults, top);
                                    //QueryResultQuickSort<Query.DocumentResultForSort>.TopSort(docResults, top, new Query.DocumentResultForSortComparer());
                                    return;

                                }

                            case Hubble.Core.Data.DataType.Float:
                                {
                                    if (docResults.Length > 0)
                                    {
                                        docResults[0].Asc = _OrderBys[0].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
                                    }

                                    _DBProvider.FillPayloadData(docResults);

                                    for (int i = 0; i < docResults.Length; i++)
                                    {
                                        int* payLoadData = docResults[i].PayloadData;

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(docResults[i].Asc, field.DataType,
                                            payLoadData, field.TabIndex, field.SubTabIndex, field.DataLength);
                                        docResults[i].SortValue = (long)(sortInfo.DoubleValue * 1000);
                                    }

                                    QueryResultHeapSort.TopSort(docResults, top);
                                    //QueryResultQuickSort<Query.DocumentResultForSort>.TopSort(docResults, top, new Query.DocumentResultForSortComparer());
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
                            field.DataType, docResults[i].PayloadData, field.TabIndex, field.SubTabIndex, field.DataLength));
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
