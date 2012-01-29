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
using Hubble.SQLClient;

namespace Hubble.Core.StoredProcedure
{
    public abstract class StoredProcedure 
    {
        abstract public string Name { get; }

        protected QueryResult _QueryResult = new QueryResult();

        protected void AddColumn(string columnName)
        {
            if (_QueryResult.DataSet.Tables.Count == 0)
            {
                _QueryResult.DataSet.Tables.Add(new Hubble.Framework.Data.DataTable());
            }

            Hubble.Framework.Data.DataTable table = _QueryResult.DataSet.Tables[0];

            table.TableName = "StoreProc_" + Name;

            Hubble.Framework.Data.DataColumn col = new Hubble.Framework.Data.DataColumn(columnName);

            table.Columns.Add(col);
        }

        protected void NewRow()
        {
            Hubble.Framework.Data.DataTable table = _QueryResult.DataSet.Tables[0];

            table.Rows.Add(table.NewRow());
        }

        protected void OutputValue(string columnName, object value)
        {
            Hubble.Framework.Data.DataTable table = _QueryResult.DataSet.Tables[0];

            if (table.Rows.Count == 0)
            {
                table.Rows.Add(table.NewRow());
            }

            if (value == null)
            {
                table.Rows[table.Rows.Count - 1][columnName] = DBNull.Value;
            }
            else
            {
                table.Rows[table.Rows.Count - 1][columnName] = value;
            }
        }

        protected void OutputMessage(string message)
        {
            _QueryResult.AddPrintMessage(message);
        }

        protected void RemoveTable()
        {
            if (_QueryResult.DataSet.Tables.Count > 0)
            {
                _QueryResult.DataSet.Tables.Remove(_QueryResult.DataSet.Tables[0]);
            }
        }

        List<string> _Parameters = new List<string>();

        public List<string> Parameters
        {
            get
            {
                return _Parameters;
            }
        }

        public QueryResult Result
        {
            get
            {
                if (_QueryResult != null)
                {
                    if (_QueryResult.DataSet != null)
                    {
                        if (_QueryResult.DataSet.Tables != null)
                        {
                            for (int i = 0; i < _QueryResult.DataSet.Tables.Count; i++)
                            {
                                Hubble.Framework.Data.DataTable table = _QueryResult.DataSet.Tables[i];
                                table.MinimumCapacity = table.Rows.Count;
                            }
                        }
                    }
                }
                return _QueryResult;
            }
        }
    }
}
