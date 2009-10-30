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

namespace Hubble.Core.Data
{
    public class DBAccess : IDisposable
    {
        private DBProvider _DBInsertProvider = null;
        private string _LastTableName = null;

        private string _Host = null;

        public string Host
        {
            get
            {
                return _Host;
            }

            set
            {
                _Host = value;
            }
        }

        ~DBAccess()
        {
            Dispose();
        }

        public QueryResult Query(string sql)
        {
            SFQL.Parse.SFQLParse sfqlParse = new SFQL.Parse.SFQLParse();
            return sfqlParse.Query(sql);
        }

        public void CreateTable(Table table, string directory)
        {
            if (string.IsNullOrEmpty(Host))
            {
                lock (this)
                {
                    if (table.Name == null)
                    {
                        throw new System.ArgumentNullException("Null table name");
                    }

                    if (table.Name.Trim() == "")
                    {
                        throw new System.ArgumentException("Empty table name");
                    }

                    if (DBProvider.DBProviderExists(table.Name))
                    {
                        throw new DataException(string.Format("Table {0} exists already!", table.Name));
                    }

                    directory = Hubble.Framework.IO.Path.AppendDivision(directory, '\\');

                    if (!System.IO.Directory.Exists(directory))
                    {
                        System.IO.Directory.CreateDirectory(directory);
                    }

                    DBProvider.NewDBProvider(table.Name, new DBProvider());

                    try
                    {
                        DBProvider.GetDBProvider(table.Name).Create(table, directory);
                    }
                    catch
                    {
                        DBProvider.Drop(table.Name);
                        throw;
                    }
                }
            }
        }

        public void Insert(string tableName, List<Document> docs)
        {
            if (string.IsNullOrEmpty(Host))
            {
                DBProvider dbProvider;

                lock (this)
                {
                    if (_DBInsertProvider != null && _LastTableName != tableName)
                    {
                        _DBInsertProvider.Collect();
                    }

                    _DBInsertProvider = DBProvider.GetDBProvider(tableName);
                    dbProvider = _DBInsertProvider;
                }

                dbProvider.Insert(docs);
            }

            _LastTableName = tableName;
        }

        public void Delete(string tableName, List<long> docs)
        {
            if (string.IsNullOrEmpty(Host))
            {
                DBProvider dbProvider;

                lock (this)
                {
                    if (_DBInsertProvider != null && _LastTableName != tableName)
                    {
                        _DBInsertProvider.Collect();
                    }

                    _DBInsertProvider = DBProvider.GetDBProvider(tableName);
                    dbProvider = _DBInsertProvider;
                }

                dbProvider.Delete(docs);
            }

            _LastTableName = tableName;
        }

        public void Collect()
        {
            lock (this)
            {
                if (_DBInsertProvider != null)
                {
                    _DBInsertProvider.Collect();
                    _DBInsertProvider = null;
                    _LastTableName = null;
                }
            }
        }

        public void Optimize(string tableName)
        {
            if (string.IsNullOrEmpty(Host))
            {
                DBProvider dbProvider;

                lock (this)
                {
                    if (_DBInsertProvider != null && _LastTableName != tableName)
                    {
                        _DBInsertProvider.Collect();
                    }

                    _DBInsertProvider = DBProvider.GetDBProvider(tableName);
                    dbProvider = _DBInsertProvider;
                }

                dbProvider.Optimize();
            }

            _LastTableName = tableName;
        }

        public void Close()
        {
            Collect();
        }

        #region IDisposable Members

        public void Dispose()
        {
            //Collect();
        }

        #endregion
    }
}
