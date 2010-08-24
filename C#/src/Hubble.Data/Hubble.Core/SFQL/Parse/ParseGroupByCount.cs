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
    class ParseGroupByCount : IGroupBy
    {
        struct GroupByPair : IComparable<GroupByPair>
        {
            internal ulong Key;
            internal int Count;

            internal GroupByPair(ulong key, int count)
            {
                this.Key = key;
                this.Count = count;
            }

            #region IComparable<GroupByPair> Members

            public int CompareTo(GroupByPair other)
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

        int _Top = 0;
        DBProvider _DBProvider;
        List<Field> _GroupByFields;
        string _GroupByFieldsString;
        SyntaxAnalysis.ExpressionTree _ExpressionTree;

        /// <summary>
        /// Get key for the values of group by feilds
        /// </summary>
        /// <param name="docid"></param>
        /// <returns></returns>
        unsafe private ulong GetKey(int docid)
        {
            int* payloadData = _DBProvider.GetPayloadData(docid);
            ulong key = 0;

            if (payloadData == null)
            {
                throw new ParseException(string.Format("docid={0} does not exist",
                    docid));
            }

            foreach (Field field in _GroupByFields)
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

        unsafe private System.Data.DataTable GetTable(Dictionary<ulong, int> groupByDict)
        {
            GroupByPair[] groupByPair = new GroupByPair[groupByDict.Count];

            int i = 0;

            foreach(ulong key in groupByDict.Keys)
            {
                groupByPair[i++] = new GroupByPair(key, groupByDict[key]);
            }

            Array.Sort(groupByPair);

            System.Data.DataTable table = new System.Data.DataTable();

            foreach (Field field in _GroupByFields)
            {
                System.Data.DataColumn col = new System.Data.DataColumn(field.Name,
                    DataTypeConvert.GetClrType(field.DataType));

                table.Columns.Add(col);
            }

            table.Columns.Add(new System.Data.DataColumn("Count", typeof(int)));

            foreach (GroupByPair gbp in groupByPair)
            {
                System.Data.DataRow row = table.NewRow();

                int preDataLength = 0;
                int col = _GroupByFields.Count - 1;

                for(; col >= 0; col--)
                {
                    row[col] = GetFieldValue(_GroupByFields[col], ref preDataLength, gbp.Key);
                }

                row[_GroupByFields.Count] = gbp.Count;

                table.Rows.Add(row);

                if (table.Rows.Count >= _Top)
                {
                    break;
                }
            }

            table.TableName = "GroupByCount_" + _GroupByFieldsString;
            table.MinimumCapacity = groupByDict.Count;

            return table;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attribute">GroupBy Attribute</param>
        /// <param name="dbProvider"></param>
        /// <example> 
        /// [GroupBy("Count", "*", "type1, type2", 10)]
        /// equivalent sql:
        /// select top 10 count(*) as count from table group by type1, type2 order by count desc
        /// </example>
        internal ParseGroupByCount(TSFQLAttribute attribute, DBProvider dbProvider,
            SyntaxAnalysis.ExpressionTree expressionTree)
        {
            if (attribute.Parameters.Count < 3)
            {
                throw new ParseException("Invalid parameter count");
            }

            if (!attribute.Parameters[0].Equals("count", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ParseException("first parameter must be count");
            }

            _DBProvider = dbProvider;

            _GroupByFields = new List<Field>();

            _ExpressionTree = expressionTree;

            int fieldsDataLength = 0;

            _GroupByFieldsString = attribute.Parameters[2];

            foreach (string fieldName in _GroupByFieldsString.Split(new char[] { ',' }))
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


                _GroupByFields.Add(field);
            }

            if (fieldsDataLength > 8)
            {
                throw new ParseException("Sum of the data length of all group by fields can't be large then 8.");
            }

            if (_GroupByFields.Count <= 0)
            {
                throw new ParseException("It need at least one group by field");
            }

            _Top = int.MaxValue;

            if (attribute.Parameters.Count > 3)
            {
                _Top = int.Parse(attribute.Parameters[3]);
            }

        }

        private System.Data.DataTable GroupByFromDatabase()
        {
            string whereSql;

            if (_ExpressionTree == null)
            {
                whereSql = "";
            }
            else
            {
                whereSql = "where " + _ExpressionTree.SqlText;
            }

            StringBuilder sb = new StringBuilder();

            string fields = "";
            int i = 0;

            foreach(Field field in _GroupByFields)
            {
                if (i++ == 0)
                {
                    fields += field.Name;
                }
                else
                {
                    fields += "," + field.Name;
                }
            }

            string sql = string.Format("select {0} , count(*) as Count from {1} {2} group by {0} order by count desc",
                fields, _DBProvider.Table.DBTableName, whereSql);

            System.Data.DataTable src = _DBProvider.DBAdapter.QuerySql(sql).Tables[0];
            System.Data.DataTable table = src.Clone();

            table.MinimumCapacity = src.Rows.Count;
            int top = _Top;

            if (top < 0)
            {
                top = int.MaxValue;
            }

            i = 0;
            while (top > 0 && i < src.Rows.Count)
            {
                System.Data.DataRow row = table.NewRow();

                for (int j = 0; j < table.Columns.Count; j++)
                {
                    row[j] = src.Rows[i][j];
                }

                table.Rows.Add(row);

                i++;
                top--;
            }

            table.TableName = "GroupByCount_" + _GroupByFieldsString;
            

            return table;


        }

        /// <summary>
        /// Do Group By
        /// </summary>
        /// <param name="result">result of the query</param>
        /// <param name="limit">limit count to group by. only group by limit number of records</param>
        public System.Data.DataTable GroupBy(Query.DocumentResultForSort[] result, int limit)
        {
            if (_ExpressionTree == null)
            {
                return GroupByFromDatabase();
            }

            if (!_ExpressionTree.NeedTokenize)
            {
                return GroupByFromDatabase();
            }

            limit = Math.Min(result.Length, limit);
            Dictionary<ulong, int>  groupByDict = new Dictionary<ulong, int>(limit);

            for (int i = 0; i < limit; i++)
            {
                ulong key = GetKey(result[i].DocId);

                int count;

                if (groupByDict.TryGetValue(key, out count))
                {
                    groupByDict[key] = count + 1;
                }
                else
                {
                    groupByDict.Add(key, 1);
                }
            }

            return GetTable(groupByDict);
        }
    }
}
