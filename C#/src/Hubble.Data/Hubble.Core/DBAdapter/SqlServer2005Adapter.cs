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
                    case Hubble.Core.Data.DataType.Float:
                        defaultValue = field.DefaultValue;
                        break;
                    case Hubble.Core.Data.DataType.Date:
                    case Hubble.Core.Data.DataType.SmallDateTime:
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
                    sqlType = "BigInt";
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

                    System.Data.DataSet ds;

                    if (DocIdReplaceField == null)
                    {
                        ds = sqlData.QuerySql("select max(DocId) as MaxDocId from " + Table.DBTableName);
                    }
                    else
                    {
                        ds = sqlData.QuerySql(string.Format("select max({0}) as MaxDocId from " + Table.DBTableName,
                            DocIdReplaceField));
                    }

                    if (ds.Tables[0].Rows.Count <= 0)
                    {
                        return -1;
                    }

                    if (ds.Tables[0].Rows[0][0] == System.DBNull.Value)
                    {
                        return -1;
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

        public void Truncate(string tableName)
        {
            Debug.Assert(Table != null);

            string sql = string.Format("truncate table {0}",
                tableName);

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

            sql.Append("DocId Int NOT NULL,");

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

        public void CreateMirrorTable()
        {
            Debug.Assert(Table != null);

            List<string> primaryKeys = new List<string>();
            

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("create table {0} (", Table.DBTableName);

            if (Table.DocIdReplaceField != null)
            {
                primaryKeys.Add(Table.DocIdReplaceField);
            }
            else
            {
                primaryKeys.Add("DocId");
                sql.Append("DocId Int NOT NULL,");
            }

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
            }


            if (primaryKeys.Count > 0)
            {
                i = 0;
                sql.Append(" primary key (");

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

        public void MirrorInsert(IList<Hubble.Core.Data.Document> docs)
        {
            StringBuilder insertString = new StringBuilder();

            foreach (Hubble.Core.Data.Document doc in docs)
            {
                if (Table.DocIdReplaceField == null)
                {
                    insertString.AppendFormat("Insert {0} ([DocId]", _Table.DBTableName);
                }
                else
                {
                    insertString.AppendFormat("Insert {0} (", _Table.DBTableName);
                }

                int i = 0;
                foreach (Data.FieldValue fv in doc.FieldValues)
                {
                    if (fv.Value == null)
                    {
                        continue;
                    }

                    if (i == 0 && Table.DocIdReplaceField != null)
                    {
                        insertString.AppendFormat("[{0}]", fv.FieldName);
                    }
                    else
                    {
                        insertString.AppendFormat(", [{0}]", fv.FieldName);
                    }

                    i++;
                }

                if (Table.DocIdReplaceField == null)
                {
                    insertString.AppendFormat(") Values({0}", doc.DocId);
                }
                else
                {
                    insertString.Append(") Values(");
                }

                i = 0;

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
                            if (i == 0 && Table.DocIdReplaceField != null)
                            {
                                insertString.AppendFormat("'{0}'", fv.Value.Replace("'", "''"));
                            }
                            else
                            {
                                insertString.AppendFormat(",'{0}'", fv.Value.Replace("'", "''"));
                            }
                            break;
                        default:
                            if (i == 0 && Table.DocIdReplaceField != null)
                            {
                                insertString.AppendFormat("{0}", fv.Value);
                            }
                            else
                            {
                                insertString.AppendFormat(",{0}", fv.Value);
                            }
                            break;
                    }

                    i++;
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

        public void MirrorDelete(IList<int> docIds)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("delete ");

            if (Table.DocIdReplaceField != null)
            {
                sql.AppendFormat(" {0} where {1} in (", Table.DBTableName, Table.DocIdReplaceField);
            }
            else
            {
                sql.AppendFormat(" {0} where docId in (", Table.DBTableName);
            }
            int i = 0;
            foreach (int docId in docIds)
            {
                long id;

                if (Table.DocIdReplaceField != null)
                {
                    id = this.DBProvider.GetDocIdReplaceFieldValue(docId);
                    if (id == long.MaxValue)
                    {
                        //does not find this record
                        continue;
                    }
                }
                else
                {
                    id = docId;
                }

                if (i++ == 0)
                {
                    sql.AppendFormat("{0}", id);
                }
                else
                {
                    sql.AppendFormat(",{0}", id);
                }
            }

            if (i == 0)
            {
                //No records need to delete
                return;
            }

            sql.Append(")");

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());
            }

        }

        public void Update(Data.Document doc, IList<Query.DocumentResultForSort> docs)
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
            
            if (DocIdReplaceField == null)
            {
                sql.Append(" where docId in (");
            }
            else
            {
                sql.AppendFormat(" where {0} in (", DocIdReplaceField);
            }


            i = 0;

            foreach (Query.DocumentResultForSort docResult in docs)
            {
                int docId = docResult.DocId;

                if (DocIdReplaceField == null)
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
                else
                {
                    long replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);

                    if (i++ == 0)
                    {
                        sql.AppendFormat("{0}", replaceFieldValue);
                    }
                    else
                    {
                        sql.AppendFormat(",{0}", replaceFieldValue);
                    }
                }

            }

            sql.Append(")");

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());
            }

        }

        public void MirrorUpdate(IList<FieldValue> fieldValues, IList<List<FieldValue>> docValues,
            IList<Query.DocumentResultForSort> docs)
        {
            StringBuilder sql = new StringBuilder();

            for (int index = 0; index < docs.Count; index++)
            {
                sql.AppendFormat("update {0} set ", Table.DBTableName);

                for(int i = 0; i < fieldValues.Count; i++)
                {
                    Data.FieldValue fvFieldName = fieldValues[i];

                    Data.FieldValue fv = docValues[index][i];

                    string value = fv.Type == Data.DataType.Varchar || fv.Type == Data.DataType.NVarchar ||
                        fv.Type == Data.DataType.NChar || fv.Type == Data.DataType.Char ||
                        fv.Type == Data.DataType.Date || fv.Type == Data.DataType.DateTime ||
                        fv.Type == Data.DataType.SmallDateTime || fv.Type == Data.DataType.Data ?
                        string.Format("'{0}'", fv.Value.Replace("'", "''")) : fv.Value;

                    if (i == 0)
                    {
                        sql.AppendFormat("[{0}]={1}", fvFieldName.FieldName, value);
                    }
                    else
                    {
                        sql.AppendFormat(",[{0}]={1}", fvFieldName.FieldName, value);
                    }
                }

                int docid = docs[index].DocId;

                if (DocIdReplaceField == null)
                {
                    sql.AppendFormat(" where docId = {0}; \r\n", docid);
                }
                else
                {
                    long replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docid);
                    sql.AppendFormat(" where {0} = {1}; \r\n", DocIdReplaceField, replaceFieldValue);
                }

            }

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());
            }

        }

        public System.Data.DataTable Query(IList<Hubble.Core.Data.Field> selectFields, IList<Query.DocumentResultForSort> docs)
        {
            return Query(selectFields, docs, 0, docs.Count - 1);
        }

        public System.Data.DataTable Query(IList<Hubble.Core.Data.Field> selectFields, IList<Query.DocumentResultForSort> docs, int begin, int end)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("select ");

            int i = 0;
            foreach (Hubble.Core.Data.Field field in selectFields)
            {
                if (DocIdReplaceField != null)
                {
                    if (field.Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                }

                if (i++ == 0)
                {
                    sql.AppendFormat("[{0}]", field.Name);
                }
                else
                {
                    sql.AppendFormat(",[{0}]", field.Name);
                }
            }

            if (DocIdReplaceField != null)
            {
                sql.AppendFormat(",[{0}]", DocIdReplaceField);
            }

            if (DocIdReplaceField == null)
            {
                sql.AppendFormat(" from {0} where docId in (", Table.DBTableName);
            }
            else
            {
                sql.AppendFormat(" from {0} where {1} in (", Table.DBTableName, DocIdReplaceField);
            }

            i = 0;

            Dictionary<long, int> replaceFieldValueToDocId = null;

            if (DocIdReplaceField != null)
            {
                replaceFieldValueToDocId = new Dictionary<long, int>();
            }

            for (int j = begin; j <= end; j++)
            {
                if (j >= docs.Count)
                {
                    break;
                }

                int docId = docs[j].DocId;

                if (DocIdReplaceField == null)
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
                else
                {
                    long replaceFieldValue = this.DBProvider.GetDocIdReplaceFieldValue(docId);

                    replaceFieldValueToDocId.Add(replaceFieldValue, docId);

                    if (i++ == 0)
                    {
                        sql.AppendFormat("{0}", replaceFieldValue);
                    }
                    else
                    {
                        sql.AppendFormat(",{0}", replaceFieldValue);
                    }
                }
            }

            sql.Append(")");

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);

                System.Data.DataTable table = sqlData.QuerySql(sql.ToString()).Tables[0];

                if (DocIdReplaceField != null)
                {
                    table.Columns.Add(new System.Data.DataColumn("DocId", typeof(int)));

                    for (i = 0; i < table.Rows.Count; i++)
                    {
                        table.Rows[i]["DocId"] =
                            replaceFieldValueToDocId[long.Parse(table.Rows[i][DocIdReplaceField].ToString())];
                    }
                }

                return table;

            }
        }

        unsafe private string GetInSql(Data.DBProvider dbProvider, Query.DocumentResultForSort[] result, int begin, int count)
        {
            if (begin + count > result.Length)
            {
                return null;
            }

            StringBuilder sql = new StringBuilder();

            if (dbProvider.DocIdReplaceField == null)
            {
                sql.Append("docId in (");
            }
            else
            {
                sql.AppendFormat("{0} in (", dbProvider.DocIdReplaceField);
            }

            Dictionary<long, int> replaceFieldValueToDocId = null;

            if (dbProvider.DocIdReplaceField != null)
            {
                replaceFieldValueToDocId = new Dictionary<long, int>();
            }

            int i = 0;

            for(int j = begin ; j < begin + count; j++)
            {
                Query.DocumentResultForSort docResult = result[j];
                int docId = docResult.DocId;

                if (dbProvider.DocIdReplaceField == null)
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
                else
                {
                    long replaceFieldValue = dbProvider.GetDocIdReplaceFieldValue(docId);

                    if (i++ == 0)
                    {
                        sql.AppendFormat("{0}", replaceFieldValue);
                    }
                    else
                    {
                        sql.AppendFormat(",{0}", replaceFieldValue);
                    }
                }
            }

            sql.Append(")");

            return sql.ToString();
        }



        public IList<Core.Query.DocumentResultForSort> GetDocumentResultForSortList(int end, 
            Core.Query.DocumentResultForSort[] docResults, string orderby)
        {
            if (docResults.Length == 0)
            {
                return new List<Core.Query.DocumentResultForSort>();
            }

            int unitLength = Math.Max(10000, (end + 1) * 10);

            List<IList<Core.Query.DocumentResultForSort>> docResultForSortList = new List<IList<Hubble.Core.Query.DocumentResultForSort>>();

            for (int begin = 0; begin < docResults.Length; begin += unitLength)
            {
                int count = Math.Min(unitLength, docResults.Length - begin);
                docResultForSortList.Add(GetDocumentResultForSortList(end,
                    GetInSql(_DBProvder, docResults, begin, count), orderby));
            }

            if (docResultForSortList.Count == 1)
            {
                return docResultForSortList[0];
            }

            List<Core.Query.DocumentResultForSort> temp = new List<Hubble.Core.Query.DocumentResultForSort>();

            foreach(List<Core.Query.DocumentResultForSort> docSort in docResultForSortList)
            {
                temp.AddRange(docSort);
            }

            return GetDocumentResultForSortList(end, temp.ToArray(), orderby);
        }


        public IList<Core.Query.DocumentResultForSort> GetDocumentResultForSortList(int end, string where, string orderby)
        {
            string sql;

            if (end >= 0)
            {
                sql = string.Format("select top {0} ", end + 1);
            }
            else
            {
                sql = "select ";
            }

            if (string.IsNullOrEmpty(where))
            {
                if (DocIdReplaceField == null)
                {
                    sql += string.Format(" docid from {0} ", Table.DBTableName);
                }
                else
                {
                    sql += string.Format(" {0} from {1} ", DocIdReplaceField, Table.DBTableName);
                }
            }
            else
            {
                if (DocIdReplaceField == null)
                {
                    sql += string.Format(" docid from {0} where {1}", Table.DBTableName, where);
                }
                else
                {
                    sql += string.Format(" {0} from {1} where {2}", DocIdReplaceField, Table.DBTableName, where);
                }
            }

            if (!string.IsNullOrEmpty(orderby))
            {
                sql += " order by " + orderby;
            }

            List<Core.Query.DocumentResultForSort> result = new List<Core.Query.DocumentResultForSort>();

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);

                foreach (System.Data.DataRow row in sqlData.QuerySql(sql).Tables[0].Rows)
                {
                    int docId;
                    if (DocIdReplaceField == null)
                    {
                        docId = int.Parse(row[0].ToString());
                    }
                    else
                    {
                        docId = DBProvider.GetDocIdFromDocIdReplaceFieldValue(long.Parse(row[DocIdReplaceField].ToString()));

                        if (docId < 0)
                        {
                            continue;
                        }
                    }

                    result.Add(new Core.Query.DocumentResultForSort(docId));
                }
            }

            return result;

        }

        public Core.SFQL.Parse.DocumentResultWhereDictionary GetDocumentResults(int end, string where, string orderby)
        {
            string sql;

            if (end >= 0)
            {
                sql = string.Format("select top {0} ", end + 1);
            }
            else
            {
                sql = "select ";
            }

            if (string.IsNullOrEmpty(where))
            {
                if (DocIdReplaceField == null)
                {
                    sql += string.Format(" docid from {0} ", Table.DBTableName);
                }
                else
                {
                    sql += string.Format(" {0} from {1} ", DocIdReplaceField, Table.DBTableName);
                }
            }
            else
            {
                if (DocIdReplaceField == null)
                {
                    sql += string.Format(" docid from {0} where {1}", Table.DBTableName, where);
                }
                else
                {
                    sql += string.Format(" {0} from {1} where {2}", DocIdReplaceField, Table.DBTableName, where);
                }
            }

            if (!string.IsNullOrEmpty(orderby))
            {
                sql += " order by " + orderby;
            }

            Core.SFQL.Parse.DocumentResultWhereDictionary result = new Core.SFQL.Parse.DocumentResultWhereDictionary();

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                foreach (System.Data.DataRow row in sqlData.QuerySql(sql).Tables[0].Rows)
                {
                    int docId;
                    if (DocIdReplaceField == null)
                    {
                        docId = int.Parse(row[0].ToString());
                    }
                    else
                    {
                        docId = DBProvider.GetDocIdFromDocIdReplaceFieldValue(long.Parse(row[DocIdReplaceField].ToString()));

                        if (docId < 0)
                        {
                            continue;
                        }
                    }

                    result.Add(docId, new Hubble.Core.Query.DocumentResult(docId));
                }

                System.Data.DataSet ds;

                if (string.IsNullOrEmpty(where))
                {
                    ds = sqlData.QuerySql(string.Format("select count(*) cnt from {0}",
                        Table.DBTableName));
                }
                else
                {
                    ds = sqlData.QuerySql(string.Format("select count(*) cnt from {0} where {1}",
                        Table.DBTableName, where));
                }
                
                result.RelTotalCount = int.Parse(ds.Tables[0].Rows[0][0].ToString());

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

        public System.Data.DataSet GetSchema(string tableName)
        {
            string sql = string.Format("select * from {0}", tableName);

            using (SQLDataProvider sqlData = new SQLDataProvider())
            {
                string connectionString = this.ConnectionString;

                sqlData.Connect(connectionString);

                return sqlData.GetSchema(sql);
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

        public void ConnectionTest()
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
            }
        }

        string _DocIdReplaceField = null;

        public string DocIdReplaceField
        {
            get
            {
                return _DocIdReplaceField;
            }
            set
            {
                _DocIdReplaceField = value;
            }
        }

        DBProvider _DBProvder;
        public DBProvider DBProvider
        {
            get
            {
                return _DBProvder;
            }
            set
            {
                _DBProvder = value;
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
