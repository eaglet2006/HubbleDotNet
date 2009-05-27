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
using Hubble.Framework.Data;

namespace Hubble.Core.DBAdapter
{
    public class SqlServer2005Adapter : IDBAdapter
    {
        private string GetFieldLine(Data.Field field)
        {
            if (!field.Store)
            {
                return null;
            }

            string sqlType = "";

            switch (field.DataType)
            {
                case Hubble.Core.Data.DataType.Int32:
                case Hubble.Core.Data.DataType.Date:
                case Hubble.Core.Data.DataType.SmallDateTime:
                    sqlType = "Int";
                    break;
                case Hubble.Core.Data.DataType.DateTime:
                    sqlType = "DateTime";
                    break;
                case Hubble.Core.Data.DataType.Float:
                    sqlType = "Float";
                    break;
                case Hubble.Core.Data.DataType.Int64:
                    sqlType = "Int64";
                    break;
                case Hubble.Core.Data.DataType.String:
                    sqlType = "nvarchar ({1})";
                    break;
                default:
                    throw new ArgumentException(field.DataType.ToString());
            }

            return string.Format("[{0}] " + sqlType + ",", field.Name,
                field.DataLength <= 0 ? "max" : field.DataLength.ToString());
        }

        #region IDBAdapter Members

        Data.Table _Table;

        public Hubble.Core.Data.Table Table
        {
            get
            {
                return _Table;
            }
            set
            {
                _Table = value;
            }
        }


        public long MaxDocId
        {
            get 
            {
                using (SQLDataProvider sqlData = new SQLDataProvider())
                {
                    sqlData.Connect(Table.ConnectionString);
                    System.Data.DataSet ds = sqlData.QuerySql("select max(DocId) as MaxDocId from " + Table.DBTableName);

                    if (ds.Tables[0].Rows.Count <= 0)
                    {
                        return 0;
                    }

                    if (ds.Tables[0].Rows[0][0] == System.DBNull.Value)
                    {
                        return 0;
                    }
                    else
                    {
                        return (long)ds.Tables[0].Rows[0][0];
                    }

                } 
            }
        }


        public void Drop()
        {
            Debug.Assert(Table != null);

            string sql = string.Format("if exists (select * from sysobjects where id = object_id('{0}') and type = 'u')	drop table {0}",
                Table.DBTableName);

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql);
            }
        }

        public void Create()
        {
            Debug.Assert(Table != null);

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("create table {0} (", Table.DBTableName);

            sql.Append("DocId BigInt NOT NULL,");

            foreach (Data.Field field in Table.Fields)
            {
                string fieldSql = GetFieldLine(field);

                if (fieldSql != null)
                {
                    sql.Append(fieldSql);
                }
            }

            sql.Append(")");

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());

                sqlData.ExcuteSql(string.Format("create UNIQUE CLUSTERED Index I_{0}_DocId on {0}(DocId)",
                    Table.DBTableName));

                if (!string.IsNullOrEmpty(Table.SQLForCreate))
                {
                    sqlData.ExcuteSql(Table.SQLForCreate);
                }

            }

        }

        public void Insert(List<Hubble.Core.Data.Document> docs)
        {
            StringBuilder insertString = new StringBuilder();

            foreach (Hubble.Core.Data.Document doc in docs)
            {
                insertString.AppendFormat("Insert {0} ([DocId]", _Table.DBTableName);

                foreach (Data.FieldValue fv in doc.FieldValues)
                {
                    insertString.AppendFormat(", [{0}]", fv.FieldName);
                }

                insertString.AppendFormat(") Values({0}", doc.DocId);
                
                foreach (Data.FieldValue fv in doc.FieldValues)
                {
                    switch (fv.Type)
                    {
                        case Hubble.Core.Data.DataType.String:
                        case Hubble.Core.Data.DataType.DateTime:
                        case Hubble.Core.Data.DataType.Data:
                            insertString.AppendFormat(",'{0}'", fv.Value.Replace("'", "''"));
                            break;
                        default:
                            insertString.AppendFormat(",{0}", fv.Value);
                            break;
                    }
                }

                insertString.Append(")\r\n");

            }

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(insertString.ToString());
            }

        }

        #endregion
    }
}
