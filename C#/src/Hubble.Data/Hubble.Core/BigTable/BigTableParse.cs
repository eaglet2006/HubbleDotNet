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

using Hubble.Core.SFQL.Parse;
using Hubble.Core.SFQL.SyntaxAnalysis.Select;
using Hubble.Core.BigTable;
using Hubble.SQLClient;

namespace Hubble.Core.BigTable
{
    public class BigTableParse : IBigTableParse
    {
        SFQLParse _SFQLParse;

        #region IBigTableParse Members

        public SFQLParse SFQLParse
        {
            get
            {
                return _SFQLParse;
            }
            set
            {
                _SFQLParse = value;
            }
        }

        public QueryResult Parse(Select select, Hubble.Core.Data.DBProvider dbProvider, 
            Service.ConnectionInformation connInfo)
        {
            BigTable bigTable = dbProvider.BigTableCfg;

            //if (!string.IsNullOrEmpty(tc.ConnectString))
            //{
            //    throw new ParseException("Free version does not provider distributable BigTable. You should use HubblePro for it");
            //}

            if (bigTable.Tablets.Count <= 0)
            {
                throw new ParseException("The Bigtable must have at least one table in table collection");
            }

            List<Select> unionSelect = new List<Select>();

            foreach(TabletInfo tf in bigTable.Tablets)
            {
                string tableName = tf.TableName;
                Select sel = select.Clone();
                sel.SelectFroms[0].Name = tableName;
                sel.SelectFroms[0].Alias = tableName;

                unionSelect.Add(sel);
            }

            string unionSelectTableName;
            string tableTicksReturn = null;
            QueryResult datacacheQuery = null;

            List<QueryResult> unionQueryResult = _SFQLParse.UnionSelectDistribute(connInfo, unionSelect, 
                out unionSelectTableName, out tableTicksReturn, out datacacheQuery);

            if (datacacheQuery != null)
            {
                return datacacheQuery;
            }

            return SFQLParse.ExcuteUnionSelect(unionSelectTableName, tableTicksReturn, 
                unionQueryResult, connInfo);
        }

        #endregion
    }
}
