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

using Hubble.Core.Query;

namespace Hubble.Core.SFQL.Parse
{
    /**/
    /// <summary>
    /// Quick Sort
    /// </summary>
    /// <typeparam name="DocumentResultForSort"></typeparam>
    public class QueryResultHeapSort
    {
        readonly bool _Asc1; //for only one field for sort.
        readonly bool _Asc2; //for only one field for sort.
        readonly bool _CanDo;
        readonly int _SortFieldsCount;
        readonly List<SyntaxAnalysis.Select.OrderBy> _OrderBys;

        public bool CanDo
        {
            get
            {
                return _CanDo;
            }
        }

        Data.DBProvider _DBProvider;

        public QueryResultHeapSort(List<SyntaxAnalysis.Select.OrderBy> orderBys, Data.DBProvider dbProvider)
        {
            _DBProvider = dbProvider;
            _OrderBys = orderBys;
            _CanDo = true;

            if (orderBys == null)
            {
                _CanDo = false;
            }
            else if (orderBys.Count <= 0 || orderBys.Count > 2)
            {
                _CanDo = false;
            }
            else
            {
                _SortFieldsCount = orderBys.Count;

                for (int i = 0; i < orderBys.Count; i++)
                {
                    bool asc = orderBys[i].Order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);

                    if (orderBys[i].Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (i == 0)
                        {
                            _Asc1 = asc;
                        }
                        else
                        {
                            _Asc2 = asc;
                        }
                    }
                    else if (orderBys[i].Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (i == 0)
                        {
                            _Asc1 = asc;
                        }
                        else
                        {
                            _Asc2 = asc;
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            _Asc1 = asc;
                        }
                        else
                        {
                            _Asc2 = asc;
                        }

                        Data.Field field = _DBProvider.GetField(orderBys[i].Name);

                        if (field != null)
                        {
                            if (field.IndexType != Hubble.Core.Data.Field.Index.Untokenized)
                            {
                                throw new ParseException(string.Format("Order by field name:{0} is not Untokenized Index!", orderBys[i].Name));
                            }


                            switch (field.DataType)
                            {
                                case Hubble.Core.Data.DataType.Date:
                                case Hubble.Core.Data.DataType.SmallDateTime:
                                case Hubble.Core.Data.DataType.Int:
                                case Hubble.Core.Data.DataType.SmallInt:
                                case Hubble.Core.Data.DataType.TinyInt:
                                case Hubble.Core.Data.DataType.BigInt:
                                case Hubble.Core.Data.DataType.DateTime:
                                case Hubble.Core.Data.DataType.Float:
                                    break;
                                default:
                                    _CanDo = false;
                                    break;
                            }
                        }
                    }

                    if (!_CanDo)
                    {
                        break;
                    }
                }

            }
        }

        unsafe public void Prepare(DocumentResultForSort[] docResults)
        {
            for (int index = 0; index < _SortFieldsCount; index++)
            {
                bool asc;
                if (index == 0)
                {
                    asc = _Asc1;
                }
                else
                {
                    asc = _Asc2;
                }

                if (_OrderBys[index].Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                {
                    for (int i = 0; i < docResults.Length; i++)
                    {
                        if (index == 0)
                        {
                            docResults[i].SortValue = docResults[i].DocId;
                        }
                        else
                        {
                            docResults[i].SortValue1 = docResults[i].DocId;
                        }
                    }
                }
                else if (_OrderBys[index].Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                {
                    for (int i = 0; i < docResults.Length; i++)
                    {
                        if (index == 0)
                        {
                            docResults[i].SortValue = docResults[i].Score;
                        }
                        else
                        {
                            docResults[i].SortValue1 = docResults[i].Score;
                        }
                    }
                }
                else
                {
                    Data.Field field = _DBProvider.GetField(_OrderBys[index].Name);

                    if (field != null)
                    {
                        if (field.IndexType != Hubble.Core.Data.Field.Index.Untokenized)
                        {
                            throw new ParseException(string.Format("Order by field name:{0} is not Untokenized Index!", _OrderBys[index].Name));
                        }


                        switch (field.DataType)
                        {
                            case Hubble.Core.Data.DataType.Date:
                            case Hubble.Core.Data.DataType.SmallDateTime:
                            case Hubble.Core.Data.DataType.Int:
                            case Hubble.Core.Data.DataType.SmallInt:
                            case Hubble.Core.Data.DataType.TinyInt:
                                {
                                    _DBProvider.FillPayloadData(docResults);

                                    for (int i = 0; i < docResults.Length; i++)
                                    {
                                        int* payLoadData = docResults[i].PayloadData;

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(asc, field.DataType,
                                            payLoadData, field.TabIndex, field.SubTabIndex, field.DataLength);

                                        if (index == 0)
                                        {
                                            docResults[i].SortValue = sortInfo.IntValue;
                                        }
                                        else
                                        {
                                            docResults[i].SortValue1 = sortInfo.IntValue;
                                        }
                                    }
                                }
                                break;

                            case Hubble.Core.Data.DataType.BigInt:
                            case Hubble.Core.Data.DataType.DateTime:
                                {
                                    _DBProvider.FillPayloadData(docResults);

                                    for (int i = 0; i < docResults.Length; i++)
                                    {
                                        int* payLoadData = docResults[i].PayloadData;

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(asc, field.DataType,
                                            payLoadData, field.TabIndex, field.SubTabIndex, field.DataLength);

                                        if (index == 0)
                                        {
                                            docResults[i].SortValue = sortInfo.LongValue;
                                        }
                                        else
                                        {
                                            docResults[i].SortValue1 = sortInfo.LongValue;
                                        }
                                    }

                                }
                                break;
                            case Hubble.Core.Data.DataType.Float:
                                {
                                    _DBProvider.FillPayloadData(docResults);

                                    for (int i = 0; i < docResults.Length; i++)
                                    {
                                        int* payLoadData = docResults[i].PayloadData;

                                        Query.SortInfo sortInfo = Data.DataTypeConvert.GetSortInfo(asc, field.DataType,
                                            payLoadData, field.TabIndex, field.SubTabIndex, field.DataLength);

                                        if (index == 0)
                                        {
                                            docResults[i].SortValue = (long)(sortInfo.DoubleValue * 1000);
                                        }
                                        else
                                        {
                                            docResults[i].SortValue1 = (long)(sortInfo.DoubleValue * 1000);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }


        private void HeapAdjust(DocumentResultForSort[] array, int root, int bottom)
        {
            if (_Asc1)
            {
                DocumentResultForSort temp;

                for (int j = root * 2 + 1; j <= bottom; j = j * 2 + 1)
                {
                    bool lt1 = array[root * 2 + 1].SortValue < array[root * 2 + 2].SortValue;

                    if (_SortFieldsCount == 2)
                    {
                        if (array[root * 2 + 1].SortValue == array[root * 2 + 2].SortValue)
                        {
                            if (_Asc2)
                            {
                                lt1 = array[root * 2 + 1].SortValue1 < array[root * 2 + 2].SortValue1;
                            }
                            else
                            {
                                lt1 = array[root * 2 + 1].SortValue1 > array[root * 2 + 2].SortValue1;
                            }
                        }
                    }

                    if (j < bottom && lt1)
                    {
                        j++;
                    }

                    bool lt2 = array[root].SortValue >= array[j].SortValue;

                    if (_SortFieldsCount == 2)
                    {
                        if (array[root].SortValue == array[j].SortValue)
                        {
                            if (_Asc2)
                            {
                                lt2 = array[root].SortValue1 >= array[j].SortValue1;
                            }
                            else
                            {
                                lt2 = array[root].SortValue1 <= array[j].SortValue1;
                            }
                        }
                    }

                    if (lt2)
                    {
                        break;
                    }
                    else
                    {
                        temp = array[root];
                        array[root] = array[j];
                        array[j] = temp;
                        root = j;
                    }
                }
            }
            else
            {
                DocumentResultForSort temp;

                for (int j = root * 2 + 1; j <= bottom; j = j * 2 + 1)
                {
                    bool lt1 = array[root * 2 + 1].SortValue > array[root * 2 + 2].SortValue;

                    if (_SortFieldsCount == 2)
                    {
                        if (array[root * 2 + 1].SortValue == array[root * 2 + 2].SortValue)
                        {
                            if (!_Asc2)
                            {
                                lt1 = array[root * 2 + 1].SortValue1 > array[root * 2 + 2].SortValue1;
                            }
                            else
                            {
                                lt1 = array[root * 2 + 1].SortValue1 < array[root * 2 + 2].SortValue1;
                            }
                        }
                    }

                    if (j < bottom && lt1)
                    {
                        j++;
                    }

                    bool lt2 = array[root].SortValue <= array[j].SortValue;

                    if (_SortFieldsCount == 2)
                    {
                        if (array[root].SortValue == array[j].SortValue)
                        {
                            if (!_Asc2)
                            {
                                lt2 = array[root].SortValue1 <= array[j].SortValue1;
                            }
                            else
                            {
                                lt2 = array[root].SortValue1 >= array[j].SortValue1;
                            }
                        }
                    }

                    if (lt2)
                    {
                        break;
                    }
                    else
                    {
                        temp = array[root];
                        array[root] = array[j];
                        array[j] = temp;
                        root = j;
                    }
                }
            }
        }

        public void Sort(DocumentResultForSort[] array, int len)
        {
            int i;
            DocumentResultForSort temp;

            int mid = (len / 2) - 1;

            for (i = mid; i >= 0; i--)
            {
                HeapAdjust(array, i, len - 1);
            }

            for (i = len - 1; i >= 1; i--)
            {
                temp = array[0];
                array[0] = array[i];
                array[i] = temp;
                HeapAdjust(array, 0, i - 1);
            }
        }

        public void Sort(DocumentResultForSort[] array)
        {
            Sort(array, array.Length);
        }

        public void TopSort(DocumentResultForSort[] array, int top)
        {
            if (array.Length <= 0 || top <= 0)
            {
                return;
            }

            if (top >= array.Length)
            {
                Sort(array);
            }
            else
            {
                //init top heap
                int i;
                DocumentResultForSort temp;

                int mid = (top / 2) - 1;

                for (i = mid; i >= 0; i--)
                {
                    HeapAdjust(array, i, top - 1);
                }

                for (i = top; i < array.Length; i++)
                {
                    if (_Asc1)
                    {
                        bool lt = array[0].SortValue <= array[i].SortValue;

                        if (_SortFieldsCount == 2)
                        {
                            if (array[0].SortValue == array[i].SortValue)
                            {
                                if (_Asc2)
                                {
                                    lt = array[0].SortValue1 <= array[i].SortValue1;
                                }
                                else
                                {
                                    lt = array[0].SortValue1 >= array[i].SortValue1;
                                }
                            }
                        }

                        if (lt)
                        {
                            continue;
                        }
                        else
                        {
                            array[0] = array[top - 1];
                            array[top - 1] = array[i];

                            if (array[top - 1].SortValue <= array[0].SortValue)
                            {
                                HeapAdjust(array, 0, top - 1);
                            }
                            else
                            {
                                mid = (top / 2) - 1;

                                for (int j = mid; j >= 0; j--)
                                {
                                    HeapAdjust(array, j, top - 1);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool lt = array[0].SortValue >= array[i].SortValue;

                        if (_SortFieldsCount == 2)
                        {
                            if (array[0].SortValue == array[i].SortValue)
                            {
                                if (!_Asc2)
                                {
                                    lt = array[0].SortValue1 >= array[i].SortValue1;
                                }
                                else
                                {
                                    lt = array[0].SortValue1 <= array[i].SortValue1;
                                }

                            }
                        }

                        if (lt)
                        {
                            continue;
                        }
                        else
                        {
                            array[0] = array[top - 1];
                            array[top - 1] = array[i];

                            if (array[top - 1].SortValue >= array[0].SortValue)
                            {
                                HeapAdjust(array, 0, top - 1);
                            }
                            else
                            {
                                mid = (top / 2) - 1;

                                for (int j = mid; j >= 0; j--)
                                {
                                    HeapAdjust(array, j, top - 1);
                                }
                            }
                        }
                    }
                }

                for (i = top - 1; i >= 1; i--)
                {
                    temp = array[0];
                    array[0] = array[i];
                    array[i] = temp;
                    HeapAdjust(array, 0, i - 1);
                }
            }
        }

    }
}
