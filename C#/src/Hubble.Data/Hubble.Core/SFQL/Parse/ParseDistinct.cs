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

using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Core.Data;

namespace Hubble.Core.SFQL.Parse
{
    class ParseDistinct : IDistinct, INamedExternalReference
    {
        struct DistinctPair : IComparable<DistinctPair>
        {
            internal ulong Key;
            internal int Count;

            internal DistinctPair(ulong key, int count)
            {
                this.Key = key;
                this.Count = count;
            }

            #region IComparable<DistinctByPair> Members

            public int CompareTo(DistinctPair other)
            {
                if (other.Count > this.Count)
                {
                    return 1;
                }
                else if (other.Count < this.Count)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            #endregion
        }

        bool _Inited = false;

        DBProvider _DBProvider;
        List<Field> _DistinctFields;
        string _DistinctFieldsString;
        int _DistinctCount = 1;
        SyntaxAnalysis.ExpressionTree _ExpressionTree;

        /// <summary>
        /// Get key for the values of group by feilds
        /// </summary>
        /// <param name="docid"></param>
        /// <returns></returns>
        unsafe private ulong GetKey(ref Query.DocumentResultForSort result)
        {
            int* payloadData = result.PayloadData;
            int docid = result.DocId;

            if (payloadData == null)
            {
                payloadData = _DBProvider.GetPayloadData(docid);
                result.PayloadData = payloadData;
            }

            ulong key = 0;

            if (payloadData == null)
            {
                throw new ParseException(string.Format("docid={0} does not exist",
                    docid));
            }

            foreach (Field field in _DistinctFields)
            {
                switch (field.DataType)
                {
                    case DataType.TinyInt:
                        {
                            sbyte data = DataTypeConvert.GetSByte(field.DataType,
                                payloadData, field.TabIndex, field.SubTabIndex, field.DataLength);
                            key <<= 8;
                            key += (byte)data;
                            break;
                        }
                    case DataType.SmallInt:
                        {
                            short data = DataTypeConvert.GetShort(field.DataType,
                                  payloadData, field.TabIndex, field.SubTabIndex, field.DataLength);
                            key <<= 16;
                            key += (ushort)data;
                            break;
                        }
                    case DataType.Int:
                    case DataType.Date:
                    case DataType.SmallDateTime:
                        {
                            int data = DataTypeConvert.GetInt(field.DataType,
                                payloadData, field.TabIndex, field.SubTabIndex, field.DataLength);

                            key <<= 32;
                            key += (uint)data;
                            break;
                        }
                    case DataType.BigInt:
                    case DataType.DateTime:
                        {
                            long data = DataTypeConvert.GetLong(field.DataType,
                                payloadData, field.TabIndex, field.SubTabIndex, field.DataLength);

                            key = (ulong)data;
                            break;
                        }
                }
            }

            return key;
        }

        private object GetFieldValue(Field field, ref int preDataLength, ulong key)
        {
            key >>= preDataLength * 8;

            switch (field.DataType)
            {
                case DataType.TinyInt:
                    {
                        key &= 0x00000000000000FF;
                        preDataLength++;
                        return (sbyte)key;
                    }
                case DataType.SmallInt:
                    {
                        key &= 0x000000000000FFFF;
                        preDataLength += 2;
                        return (short)key;
                    }
                case DataType.Int:
                    key &= 0x00000000FFFFFFFF;
                    preDataLength += 4;
                    return (int)key;

                case DataType.Date:
                    {
                        key &= 0x00000000FFFFFFFF;
                        int data = (int)key;
                        preDataLength += 4;
                        return DataTypeConvert.IntToDate(data);
                    }
                case DataType.SmallDateTime:
                    {
                        key &= 0x00000000FFFFFFFF;
                        int data = (int)key;
                        preDataLength += 4;
                        return DataTypeConvert.IntToSmallDatetime(data);
                    }
                case DataType.BigInt:
                    preDataLength += 8;
                    return (long)key;

                case DataType.DateTime:
                    {
                        preDataLength += 8;
                        return DataTypeConvert.LongToDateTime((long)key);
                    }
                default:
                    throw new ParseException(string.Format("Invalid data type:{0}", field.DataType));
            }
        }

        private void Init()
        {
            if (_Inited)
            {
                return;
            }

            TSFQLAttribute attribute = this.Attribute;
            DBProvider dbProvider = this.DBProvider;
            SyntaxAnalysis.ExpressionTree expressionTree = this.ExpressionTree;

            if (attribute.Parameters.Count < 1)
            {
                throw new ParseException("Invalid parameter count");
            }

            _DBProvider = dbProvider;

            _DistinctFields = new List<Field>();

            _ExpressionTree = expressionTree;

            int fieldsDataLength = 0;

            _DistinctFieldsString = attribute.Parameters[0];

            _DistinctCount = 1;

            if (attribute.Parameters.Count > 1)
            {
                //Second paramter is distinct count. Default is 1.
                _DistinctCount = int.Parse(attribute.Parameters[1]);
            }

            foreach (string fieldName in _DistinctFieldsString.Split(new char[] { ',' }))
            {
                Field field = dbProvider.GetField(fieldName.Trim());

                if (field == null)
                {
                    throw new ParseException(string.Format("field:{0} does not exist", fieldName));
                }


                if (field.IndexType != Field.Index.Untokenized)
                {
                    throw new ParseException(string.Format("field:{0} is not Untokenized index", field.Name));
                }

                switch (field.DataType)
                {
                    case DataType.TinyInt:
                        fieldsDataLength++;
                        break;
                    case DataType.SmallInt:
                        fieldsDataLength += 2;
                        break;
                    case DataType.Int:
                    case DataType.Date:
                    case DataType.SmallDateTime:
                        fieldsDataLength += 4;
                        break;
                    case DataType.DateTime:
                    case DataType.BigInt:
                        fieldsDataLength += 8;
                        break;

                    default:
                        throw new ParseException(string.Format("field:{0}'s data type is not integer or time data type",
                            field.Name));
                }


                _DistinctFields.Add(field);
            }

            if (fieldsDataLength > 8)
            {
                throw new ParseException("Sum of the data length of all group by fields can't be large then 8.");
            }

            if (_DistinctFields.Count <= 0)
            {
                throw new ParseException("It need at least one group by field");
            }

            _Inited = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attribute">DistinctBy Attribute</param>
        /// <param name="dbProvider"></param>
        /// <example> 
        /// [DistinctBy("type1, type2")]
        /// equivalent sql:
        /// select top 10 count(*) as count from table group by type1, type2 order by count desc
        /// </example>
        public ParseDistinct()
        {
        }


        #region IDistinct Members

        public int StartRow
        {
            get;
            set;
        }

        public int EndRow
        {
            get;
            set;
        }

        public TSFQLAttribute Attribute
        {
            get;
            set;
        }

        public DBProvider DBProvider
        {
            get;
            set;
        }

        public ExpressionTree ExpressionTree
        {
            get;
            set;
        }

        public Hubble.Core.Query.DocumentResultForSort[] Distinct(
            Hubble.Core.Query.DocumentResultForSort[] result, out Hubble.Framework.Data.DataTable table)
        {
            Init();

            Dictionary<ulong, int> distinctByDict = new Dictionary<ulong, int>(); //key is the value of the distinct fields, value is distinct count

            table = null;
            List<Hubble.Core.Query.DocumentResultForSort> distinctResult =
                new List<Hubble.Core.Query.DocumentResultForSort>();

            for (int i = 0; i < result.Length; i++)
            {
                ulong key = GetKey(ref result[i]);

                int count;

                if (distinctByDict.TryGetValue(key, out count))
                {
                    distinctByDict[key] = count + 1;

                    if (count + 1 <= _DistinctCount)
                    {
                        distinctResult.Add(result[i]);
                    }
                }
                else
                {
                    distinctByDict.Add(key, 1);
                    distinctResult.Add(result[i]);
                }
            }

            return distinctResult.ToArray();

        }

        #endregion



        #region INamedExternalReference Members

        public string Name
        {
            get { return "Default"; }
        }

        #endregion
    }
}
