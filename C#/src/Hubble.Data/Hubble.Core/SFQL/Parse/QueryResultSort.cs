using System;
using System.Collections.Generic;
using System.Text;

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
                        docResult.SortInfoList = new List<Hubble.Core.Query.SortInfo>();
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
        }
    }
}
