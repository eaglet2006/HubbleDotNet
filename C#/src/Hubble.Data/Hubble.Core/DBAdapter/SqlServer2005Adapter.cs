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
using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;

namespace Hubble.Core.DBAdapter
{
    public class SqlServer2005Adapter : IDBAdapter, INamedExternalReference
    {
        private string GetFieldLine(Data.Field field)
        {
            if (!field.Store)
            {
                return null;
            }

            string sqlType = "";
            string defaultValue = null;

            if (field.DefaultValue != null)
            {
                switch (field.DataType)
                {
                    case DataType.TinyInt:
                    case DataType.SmallInt:
                    case Hubble.Core.Data.DataType.Int:
                    case Hubble.Core.Data.DataType.BigInt:
                    case Hubble.Core.Data.DataType.Date:
                    case Hubble.Core.Data.DataType.SmallDateTime:
                    case Hubble.Core.Data.DataType.Float:
                        defaultValue = field.DefaultValue;
                        break;
                    case Hubble.Core.Data.DataType.DateTime:
                    case Hubble.Core.Data.DataType.Varchar:
                    case Hubble.Core.Data.DataType.NVarchar:
                    case Hubble.Core.Data.DataType.Char:
                    case Hubble.Core.Data.DataType.NChar:
                        defaultValue = string.Format("'{0}'", field.DefaultValue.Replace("'", "''"));
                        break;
                    default:
                        throw new ArgumentException(field.DataType.ToString());
                }
            }


            switch (field.DataType)
            {
                case DataType.TinyInt:
                    sqlType = "TinyInt";
                    break;
                case DataType.SmallInt:
                    sqlType = "SmallInt";
                    break;
                case Hubble.Core.Data.DataType.Int:
                    sqlType = "Int";
                    break;
                case Hubble.Core.Data.DataType.Date:
                case Hubble.Core.Data.DataType.SmallDateTime:
                case Hubble.Core.Data.DataType.DateTime:
                    sqlType = "DateTime";
                    break;
                case Hubble.Core.Data.DataType.Float:
                    sqlType = "Float";
                    break;
                case Hubble.Core.Data.DataType.BigInt:
                    sqlType = "Int64";
                    break;
                case Hubble.Core.Data.DataType.Varchar:
                    sqlType = "varchar ({1})";
                    break;
                case Hubble.Core.Data.DataType.NVarchar:
                    sqlType = "nvarchar ({1})";
                    break;
                case Hubble.Core.Data.DataType.Char:
                    sqlType = "char ({1})";
                    break;
                case Hubble.Core.Data.DataType.NChar:
                    sqlType = "nchar ({1})";
                    break;
                default:
                    throw new ArgumentException(field.DataType.ToString());
            }

            string sql = string.Format("[{0}] " + sqlType + " " , field.Name,
                field.DataLength <= 0 ? "max" : field.DataLength.ToString());

            if (!field.CanNull)
            {
                sql += "NOT NULL ";
            }

            if (defaultValue != null)
            {
                sql += "DEFAULT " + defaultValue + " ";
            }

            return sql;
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

        string _ConnectionString = null;

        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
            set
            {
                _ConnectionString = value;
            }
        }

        public int MaxDocId
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
                        return int.Parse(ds.Tables[0].Rows[0][0].ToString());
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

        public void Truncate()
        {
            Debug.Assert(Table != null);

            string sql = string.Format("truncate table {0}",
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

            List<string> primaryKeys = new List<string>();

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("create table {0} (", Table.DBTableName);

            sql.Append("DocId BigInt NOT NULL,");

            int i = 0;

            for (i = 0; i < Table.Fields.Count; i++)
            {
                Data.Field field = Table.Fields[i];
                string fieldSql = GetFieldLine(field);

                if (fieldSql != null)
                {
                    sql.Append(fieldSql);

                    if (i < Table.Fields.Count - 1)
                    {
                        sql.Append(",");
                    }
                }

                if (field.PrimaryKey)
                {
                    primaryKeys.Add(field.Name);
                }
            }


            if (primaryKeys.Count > 0)
            {
                i = 0;
                sql.Append(" primary key NONCLUSTERED(");

                foreach (string pkName in primaryKeys)
                {
                    if (i == 0)
                    {
                        sql.Append(pkName);
                    }
                    else
                    {
                        sql.Append("," + pkName);
                    }

                    i++;
                }

                sql.Append(")");
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

        public void Insert(IList<Hubble.Core.Data.Document> docs)
        {
            StringBuilder insertString = new StringBuilder();

            foreach (Hubble.Core.Data.Document doc in docs)
            {
                insertString.AppendFormat("Insert {0} ([DocId]", _Table.DBTableName);

                foreach (Data.FieldValue fv in doc.FieldValues)
                {
                    if (fv.Value == null)
                    {
                        continue;
                    }

                    insertString.AppendFormat(", [{0}]", fv.FieldName);
                }

                insertString.AppendFormat(") Values({0}", doc.DocId);
                
                foreach (Data.FieldValue fv in doc.FieldValues)
                {
                    if (fv.Value == null)
                    {
                        continue;
                    }

                    switch (fv.Type)
                    {
                        case Hubble.Core.Data.DataType.Varchar:
                        case Hubble.Core.Data.DataType.NVarchar:
                        case Hubble.Core.Data.DataType.Char:
                        case Hubble.Core.Data.DataType.NChar:
                        case Hubble.Core.Data.DataType.DateTime:
                        case Hubble.Core.Data.DataType.Date:
                        case Hubble.Core.Data.DataType.SmallDateTime:
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

        public void Delete(IList<int> docIds)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("delete ");

            sql.AppendFormat(" {0} where docId in (", Table.DBTableName);

            int i = 0;
            foreach (int docId in docIds)
            {
                if (i++ == 0)
                {
                    sql.AppendFormat("{0}", docId);
                }
                else
                {
                    sql.AppendFormat(",{0}", docId);
                }
            }

            sql.Append(")");

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());
            }

        }

        public void Update(Data.Document doc, IList<Query.DocumentResult> docs)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("update {0} set ", Table.DBTableName);

            int i = 0;

            foreach (Data.FieldValue fv in doc.FieldValues)
            {
                string value = fv.Type == Data.DataType.Varchar || fv.Type == Data.DataType.NVarchar ||
                    fv.Type == Data.DataType.NChar || fv.Type == Data.DataType.Char ||
                    fv.Type == Data.DataType.Date || fv.Type == Data.DataType.DateTime ||
                    fv.Type == Data.DataType.SmallDateTime || fv.Type == Data.DataType.Data ? 
                    string.Format("'{0}'", fv.Value.Replace("'", "''"))  : fv.Value;

                if (i++ == 0)
                {
                    sql.AppendFormat("[{0}]={1}", fv.FieldName, value);
                }
                else
                {
                    sql.AppendFormat(",[{0}]={1}", fv.FieldName, value);
                }
            }

            sql.Append(" where docId in (");

            i = 0;

            foreach (Query.DocumentResult docResult in docs)
            {
                int docId = docResult.DocId;

                if (i++ == 0)
                {
                    sql.AppendFormat("{0}", docId);
                }
                else
                {
                    sql.AppendFormat(",{0}", docId);
                }
            }

            sql.Append(")");

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());
            }

        }

        public System.Data.DataTable Query(IList<Hubble.Core.Data.Field> selectFields, IList<Query.DocumentResult> docs)
        {
            return Query(selectFields, docs, 0, docs.Count - 1);
        }

        public System.Data.DataTable Query(IList<Hubble.Core.Data.Field> selectFields, IList<Query.DocumentResult> docs, int begin, int end)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("select ");

            int i = 0;
            foreach (Hubble.Core.Data.Field field in selectFields)
            {
                if (i++ == 0)
                {
                    sql.AppendFormat("[{0}]", field.Name);
                }
                else
                {
                    sql.AppendFormat(",[{0}]", field.Name);
                }
            }

            sql.AppendFormat(" from {0} where docId in (", Table.DBTableName);

            i = 0;

            for (int j = begin; j <= end; j++)
            {
                if (j >= docs.Count)
                {
                    break;
                }

                int docId = docs[j].DocId;

                if (i++ == 0)
                {
                    sql.AppendFormat("{0}", docId);
                }
                else
                {
                    sql.AppendFormat(",{0}", docId);
                }
            }

            sql.Append(")");

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                return sqlData.QuerySql(sql.ToString()).Tables[0];
            }
        }

        public WhereDictionary<int, Hubble.Core.Query.DocumentResult> GetDocumentResults(string where)
        {
            string sql;

            if (string.IsNullOrEmpty(where))
            {
                sql = string.Format("select docid from {0} ", Table.DBTableName);
            }
            else
            {
                sql = string.Format("select docid from {0} where {1}", Table.DBTableName, where);
            }

            WhereDictionary<int, Hubble.Core.Query.DocumentResult> result = new WhereDictionary<int, Hubble.Core.Query.DocumentResult>();

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                foreach (System.Data.DataRow row in sqlData.QuerySql(sql).Tables[0].Rows)
                {
                    int docId = int.Parse(row[0].ToString());
                    result.Add(docId, new Hubble.Core.Query.DocumentResult(docId));
                }
            }

            return result;
        }

        public System.Data.DataSet QuerySql(string sql)
        {
            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                string connectionString;
                if (Table == null)
                {
                    connectionString = this.ConnectionString;
                }
                else
                {
                    connectionString = Table.ConnectionString;
                }

                sqlData.Connect(connectionString);

                return sqlData.QuerySql(sql);
            }
        }

        public int ExcuteSql(string sql)
        {
            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                string connectionString;
                if (Table == null)
                {
                    connectionString = this.ConnectionString;
                }
                else
                {
                    connectionString = Table.ConnectionString;
                }

                sqlData.Connect(connectionString);

                return sqlData.ExcuteSql(sql);
            }
        }

        #endregion


        #region INamedExternalReference Members

        public string Name
        {
            get 
            {
                return "SQLSERVER2005";
            }
        }

        #endregion

    }
}
