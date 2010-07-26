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
    class ParseGroupBy
    {
        DBProvider _DBProvider;
        List<Field> _GroupByFields;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attribute">GroupBy Attribute</param>
        /// <param name="dbProvider"></param>
        /// <example> 
        /// [GroupBy("Count", "*", "type1, type2", 10)]
        /// equivalent sql:
        /// select top 10 count(*) as count from table group by type1, type2
        /// </example>
        internal ParseGroupBy(TSFQLAttribute attribute, DBProvider dbProvider)
        {
            if (attribute.Parameters.Count != 4)
            {
                throw new ParseException("Invalid parameter count");
            }

            if (!attribute.Parameters[0].Equals("count", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ParseException("first parameter must be count");
            }

            _DBProvider = dbProvider;

            _GroupByFields = new List<Field>();

            int fieldsDataLength = 0;

            foreach (string fieldName in attribute.Parameters[2].Split(new char[] { ',' }))
            {
                Field field = dbProvider.GetField(fieldName.Trim());

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
                        throw new ParseException(string.Format("field:{0}'s data type is not integer data type", 
                            field.Name));
                }


                _GroupByFields.Add(field);
            }

            if (fieldsDataLength > 8)
            {
                throw new ParseException("Sum of the data length of all group by fields can't be large then 8.");
            }

        }
    }
}
