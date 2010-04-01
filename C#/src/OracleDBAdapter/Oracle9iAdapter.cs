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
using Hubble.Core;
using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;
using Hubble.Core.DBAdapter;

namespace OracleDBAdapter
{
    public class Oracle9iAdapter : IDBAdapter, INamedExternalReference
    {
        #region IDBAdapter Members

        Hubble.Core.Data.Table _Table;

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

        public void Drop()
        {
            Debug.Assert(Table != null);

            string sql = string.Format("drop table {0}", Table.DBTableName);

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql);
            }
        }

        public void Create()
        {
            throw new NotImplementedException();
        }

        public void Truncate()
        {
            Debug.Assert(Table != null);

            string sql = string.Format("truncate table {0}",
                Table.DBTableName);

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql);
            }
        }

        public void Insert(IList<Document> docs)
        {
            throw new NotImplementedException();
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

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());
            }
        }

        public void Update(Document doc, IList<Hubble.Core.Query.DocumentResultForSort> docs)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable Query(IList<Field> selectFields, IList<Hubble.Core.Query.DocumentResultForSort> docs)
        {
            return Query(selectFields, docs, 0, docs.Count - 1);
        }

        public System.Data.DataTable Query(IList<Field> selectFields, IList<Hubble.Core.Query.DocumentResultForSort> docs, int begin, int end)
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
                    sql.AppendFormat("{0}", field.Name);
                }
                else
                {
                    sql.AppendFormat(",{0}", field.Name);
                }
            }

            if (DocIdReplaceField != null)
            {
                sql.AppendFormat(",{0}", DocIdReplaceField);
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

            using (OracleDataProvider sqlData = new OracleDataProvider())
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

        public DocumentResultWhereDictionary GetDocumentResults(int end, string where, string orderby)
        {
            string sql;

            if (end >= 0)
            {
                if (DocIdReplaceField == null)
                {
                    sql = "select docid from (select ";
                }
                else
                {
                    sql = string.Format("select {0} from (select ", DocIdReplaceField);
                }
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

            if (end >= 0)
            {
                sql += string.Format(") where rownum <= {0}", end + 1);
            }

            Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary result = new Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary();

            using (OracleDataProvider sqlData = new OracleDataProvider())
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
            }

            return result;
        }

        public int MaxDocId
        {
            get
            {
                using (OracleDataProvider sqlData = new OracleDataProvider())
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

        public System.Data.DataSet QuerySql(string sql)
        {
            using (OracleDataProvider sqlData = new OracleDataProvider())
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

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                string connectionString = this.ConnectionString;

                sqlData.Connect(connectionString);

                return sqlData.GetSchema(sql);
            }
        }

        public int ExcuteSql(string sql)
        {
            using (OracleDataProvider sqlData = new OracleDataProvider())
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
            using (OracleDataProvider sqlData = new OracleDataProvider())
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
                return "Oracle9i"; 
            }
        }

        #endregion
    }
}
