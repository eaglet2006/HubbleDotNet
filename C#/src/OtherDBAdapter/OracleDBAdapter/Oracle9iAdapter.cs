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
using System.Data.OracleClient;
using System.IO;

using Hubble.Core;
using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;
using Hubble.Core.DBAdapter;
using Data = Hubble.Core.Data;
using Core = Hubble.Core;
using Query = Hubble.Core.Query;

namespace OracleDBAdapter
{
    public class Oracle9iAdapter : IDBAdapter, INamedExternalReference
    {

        #region Keywords

        static readonly string[] _KeyWords = {
            "ACCESS","ELSE","MODIFY","START","ADD","EXCLUSIVE","NOAUDIT","SELECT",
            "ALL","EXISTS","NOCOMPRESS","SESSION","ALTER","FILE","NOT","SET",
            "AND","FLOAT","NOTFOUND","SHARE","ANY","FOR","NOWAIT","SIZE",
            "ARRAYLEN","FROM","NULL","SMALLINT","AS","GRANT","NUMBER","SQLBUF",
            "ASC","GROUP","OF","SUCCESSFUL","AUDIT","HAVING","OFFLINE","SYNONYM",
            "BETWEEN","IDENTIFIED","ON","SYSDATE","BY","IMMEDIATE","ONLINE","TABLE",
            "CHAR","IN","OPTION","THEN","CHECK","INCREMENT","OR","TO",
            "CLUSTER","INDEX","ORDER","TRIGGER","COLUMN","INITIAL","PCTFREE","UID",
            "COMMENT","INSERT","PRIOR","UNION","COMPRESS","INTEGER","PRIVILEGES","UNIQUE",
            "CONNECT","INTERSECT","PUBLIC","UPDATE","CREATE","INTO","RAW","USER",
            "CURRENT","IS","RENAME","VALIDATE","DATE","LEVEL","RESOURCE","VALUES",
            "DECIMAL","LIKE","REVOKE","VARCHAR","DEFAULT","LOCK","ROW","VARCHAR2",
            "DELETE","LONG","ROWID","VIEW","DESC","MAXEXTENTS","ROWLABEL","WHENEVER",
            "DISTINCT","MINUS","ROWNUM","WHERE","DROP","MODE","ROWS","WITH",
            "ADMIN","CURSOR","FOUND","MOUNT","AFTER","CYCLE","FUNCTION","NEXT",
            "ALLOCATE","DATABASE","GO","NEW","ANALYZE","DATAFILE","GOTO","NOARCHIVELOG",
            "ARCHIVE","DBA","GROUPS","NOCACHE","ARCHIVELOG","DEC","INCLUDING","NOCYCLE",
            "AUTHORIZATION","DECLARE","INDICATOR","NOMAXVALUE","AVG","DISABLE","INITRANS","NOMINVALUE",
            "BACKUP","DISMOUNT","INSTANCE","NONE","BEGIN","DOUBLE","INT","NOORDER",
            "BECOME","DUMP","KEY","NORESETLOGS","BEFORE","EACH","LANGUAGE","NORMAL",
            "BLOCK","ENABLE","LAYER","NOSORT","BODY","END","LINK","NUMERIC",
            "CACHE","ESCAPE","LISTS","OFF","CANCEL","EVENTS","LOGFILE","OLD",
            "CASCADE","EXCEPT","MANAGE","ONLY","CHANGE","EXCEPTIONS","MANUAL","OPEN",
            "CHARACTER","EXEC","MAX","OPTIMAL","CHECKPOINT","EXPLAIN","MAXDATAFILES","OWN",
            "CLOSE","EXECUTE","MAXINSTANCES","PACKAGE","COBOL","EXTENT","MAXLOGFILES","PARALLEL",
            "COMMIT","EXTERNALLY","MAXLOGHISTORY","PCTINCREASE","COMPILE","FETCH","MAXLOGMEMBERS","PCTUSED",
            "CONSTRAINT","FLUSH","MAXTRANS","PLAN","CONSTRAINTS","FREELIST","MAXVALUE","PLI",
            "CONTENTS","FREELISTS","MIN","PRECISION","CONTINUE","FORCE","MINEXTENTS","PRIMARY",
            "CONTROLFILE","FOREIGN","MINVALUE","PRIVATE","COUNT","FORTRAN","MODULE","PROCEDURE",
            "PROFILE","SAVEPOINT","SQLSTATE","TRACING","QUOTA","SCHEMA","STATEMENT_ID","TRANSACTION",
            "READ","SCN","STATISTICS","TRIGGERS","REAL","SECTION","STOP","TRUNCATE",
            "RECOVER","SEGMENT","STORAGE","UNDER","REFERENCES","SEQUENCE","SUM","UNLIMITED",
            "REFERENCING","SHARED","SWITCH","UNTIL","RESETLOGS","SNAPSHOT","SYSTEM","USE",
            "RESTRICTED","SOME","TABLES","USING","REUSE","SORT","TABLESPACE","WHEN",
            "ROLE","SQL","TEMPORARY","WRITE","ROLES","SQLCODE","THREAD","WORK",
            "ROLLBACK","SQLERROR","TIME"
            };

        static Dictionary<string, int> _KeywordsDict = null;
        static object _LockObj = new object();

        static private string GetFieldName(string fieldName)
        {
            lock (_LockObj)
            {
                if (_KeywordsDict == null)
                {
                    _KeywordsDict = new Dictionary<string, int>();

                    foreach (string keywords in _KeyWords)
                    {
                        _KeywordsDict.Add(keywords, 0);
                    }
                }

                if (_KeywordsDict.ContainsKey(fieldName.ToUpper()))
                {
                    return "\"" + fieldName + "\"";
                }
                else
                {
                    return fieldName;
                }
            }
        }

        #endregion

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
                        DateTime dateTime;
                        if (!DateTime.TryParseExact(field.DefaultValue, "yyyy-MM-dd HH:mm:ss", null,
                            System.Globalization.DateTimeStyles.None, out dateTime))
                        {
                            dateTime = DateTime.Parse(field.DefaultValue);
                        }

                        defaultValue = string.Format("to_date('{0}','yyyy-mm-dd HH24:MI:SS')", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));

                        break;
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
                    sqlType = "SmallInt";
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
                    sqlType = "Date";
                    break;
                case Hubble.Core.Data.DataType.Float:
                    sqlType = "Double";
                    break;
                case Hubble.Core.Data.DataType.BigInt:
                    sqlType = "Double";
                    break;
                case Hubble.Core.Data.DataType.Varchar:
                    if (field.DataLength <= 0)
                    {
                        sqlType = "clob";
                    }
                    else
                    {
                        if (field.DataLength > 4000)
                        {
                            throw new ArgumentException(string.Format("Invalid data length is set in field:{0}",
                                field.Name));
                        }

                        sqlType = "varchar2 ({1})";
                    }
                    break;
                case Hubble.Core.Data.DataType.NVarchar:
                    if (field.DataLength <= 0)
                    {
                        sqlType = "nclob";
                    }
                    else
                    {

                        if (field.DataLength > 2000)
                        {
                            throw new ArgumentException(string.Format("Invalid data length is set in field:{0}",
                                field.Name));
                        }
                        sqlType = "nvarchar2 ({1})";
                    }
                    break;
                case Hubble.Core.Data.DataType.Char:
                    if (field.DataLength <= 0)
                    {
                        sqlType = "clob";
                    }
                    else
                    {
                        if (field.DataLength > 2000)
                        {
                            throw new ArgumentException(string.Format("Invalid data length is set in field:{0}",
                                field.Name));
                        }
                        sqlType = "char ({1})";
                    }
                    break;
                case Hubble.Core.Data.DataType.NChar:
                    if (field.DataLength <= 0)
                    {
                        sqlType = "nclob";
                    }
                    else
                    {
                        if (field.DataLength > 1000)
                        {
                            throw new ArgumentException(string.Format("Invalid data length is set in field:{0}",
                                field.Name));
                        }
                        sqlType = "nchar ({1})";
                    }
                    break;
                default:
                    throw new ArgumentException(field.DataType.ToString());
            }

            string sql = string.Format("{0} " + sqlType + " ", GetFieldName(field.Name), field.DataLength.ToString());

            if (defaultValue != null)
            {
                sql += "DEFAULT " + defaultValue + " ";
            }

            if (!field.CanNull)
            {
                sql += "NOT NULL ";
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

        public void Drop()
        {
            Debug.Assert(Table != null);

            string sql = string.Format("drop table {0}", Table.DBTableName);

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                string testExistSql = string.Format("select * from {0} where rownum=1", Table.DBTableName);

                sqlData.Connect(Table.ConnectionString);

                try
                {
                    sqlData.QuerySql(testExistSql);
                    sqlData.ExcuteSql(sql);
                }
                catch
                {
                }
            }
        }

        public void Create()
        {
            Debug.Assert(Table != null);

            //List<string> primaryKeys = new List<string>();

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("create table {0} (", Table.DBTableName);

            sql.Append("DocId Int NOT NULL Primary Key,");

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

                //if (field.PrimaryKey)
                //{
                //    primaryKeys.Add(field.Name);
                //}
            }


            //if (primaryKeys.Count > 0)
            //{
            //    i = 0;
            //    sql.Append(" primary key NONCLUSTERED(");

            //    foreach (string pkName in primaryKeys)
            //    {
            //        if (i == 0)
            //        {
            //            sql.Append(pkName);
            //        }
            //        else
            //        {
            //            sql.Append("," + pkName);
            //        }

            //        i++;
            //    }

            //    sql.Append(")");
            //}

            sql.Append(")");

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());

                //sqlData.ExcuteSql(string.Format("create UNIQUE CLUSTERED Index I_{0}_DocId on {0}(DocId)",
                //    Table.DBTableName));

                if (!string.IsNullOrEmpty(Table.SQLForCreate))
                {
                    sqlData.ExcuteSql(Table.SQLForCreate);
                }

            }
        }

        public void CreateMirrorTable()
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

        public void Truncate(string tableName)
        {
            Debug.Assert(Table != null);

            string sql = string.Format("truncate table {0}",
                tableName);

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql);
            }
        }

        private string ReplaceTextValue(string text)
        {
            return Hubble.Framework.Text.Regx.Replace(text, @"\r|\n", "", true);
        }

        public void Insert(IList<Document> docs)
        {
            if (docs.Count <= 0)
            {
                return;
            }

            StringBuilder insertString = new StringBuilder();
            //insertString.AppendLine("DECLARE");
            insertString.Append("BEGIN ");

            foreach (Hubble.Core.Data.Document doc in docs)
            {
                insertString.AppendFormat("Insert into {0} (DocId", _Table.DBTableName);

                foreach (Data.FieldValue fv in doc.FieldValues)
                {
                    if (fv.Value == null)
                    {
                        continue;
                    }

                    insertString.AppendFormat(", {0}", GetFieldName(fv.FieldName));
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
                        case Hubble.Core.Data.DataType.NVarchar:
                        case Hubble.Core.Data.DataType.NChar:
                            if (fv.DataLength < 0)
                            {
                                insertString.Append(",N'A'");
                            }
                            else
                            {
                                insertString.AppendFormat(",N'{0}'", ReplaceTextValue(fv.Value.Replace("'", "''")));
                            }
                            break;
                        case Hubble.Core.Data.DataType.Varchar:
                        case Hubble.Core.Data.DataType.Char:
                        case Hubble.Core.Data.DataType.Data:
                            if (fv.DataLength < 0)
                            {
                                insertString.Append(",N'A'");
                            }
                            else
                            {
                                insertString.AppendFormat(",'{0}'", ReplaceTextValue(fv.Value.Replace("'", "''")));
                            }
                            break;
                        case Hubble.Core.Data.DataType.DateTime:
                        case Hubble.Core.Data.DataType.Date:
                        case Hubble.Core.Data.DataType.SmallDateTime:
                            DateTime dateTime;
                            if (!DateTime.TryParseExact(fv.Value, "yyyy-MM-dd HH:mm:ss", null,
                                System.Globalization.DateTimeStyles.None, out dateTime))
                            {
                                dateTime = DateTime.Parse(fv.Value);
                            }

                            insertString.AppendFormat(",to_date('{0}','yyyy-mm-dd HH24:MI:SS')", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            break;
                        default:
                            insertString.AppendFormat(",{0}", fv.Value);
                            break;
                    }
                }

                insertString.Append("); ");
            }

            insertString.Append(" END;");

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(insertString.ToString());

                string sql = string.Format("select * from {0} where docid >= {1} and docid <= {2} order by docid FOR UPDATE",
                    _Table.DBTableName, docs[0].DocId, docs[docs.Count - 1].DocId);

                OracleCommand cmd;

                using (OracleDataReader reader = sqlData.ExecuteReader(sql, out cmd))
                {
                    cmd.Transaction = cmd.Connection.BeginTransaction();

                    for (int i = 0; i < docs.Count; i++)
                    {
                        reader.Read();

                        for (int j = 0; j < docs[i].FieldValues.Count; j++)
                        {

                            switch (docs[i].FieldValues[j].Type)
                            {
                                case Hubble.Core.Data.DataType.NVarchar:
                                case Hubble.Core.Data.DataType.NChar:
                                case Hubble.Core.Data.DataType.Varchar:
                                case Hubble.Core.Data.DataType.Char:
                                    if (docs[i].FieldValues[j].DataLength < 0)
                                    {
                                        OracleLob clob = reader.GetOracleLob(reader.GetOrdinal(docs[i].FieldValues[j].FieldName));
                                        byte[] head = new byte[2];
                                        //head[0] = 0xCC;
                                        //head[1] = 0xDD;
                                        //clob.Write(head, 0, 2);
                                        //clob.Position = 0;
                                        byte[] buffer = Encoding.Unicode.GetBytes(docs[i].FieldValues[j].Value);
                                        clob.Write(buffer, 0, buffer.Length);

                                        //Console.WriteLine(clob.LobType + ".Write(" + buffer + ", 0, 0) => " + clob.Value);

                                        //OracleLob templob = sqlData.CreateTempLob(clob.LobType);

                                        //long actual = clob.CopyTo(templob);

                                        //Console.WriteLine(clob.LobType + ".CopyTo(" + templob.Value + ") => " + actual);


                                    }

                                    break;
                            }

                        }
                    }

                    cmd.Transaction.Commit();
                }
            }
        }


        public void MirrorInsert(IList<Document> docs)
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

        public void MirrorDelete(IList<int> docIds)
        {
            throw new NotImplementedException();
        }

        public void Update(Document doc, IList<Hubble.Core.Query.DocumentResultForSort> docs)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("update {0} set ", Table.DBTableName);

            int i = 0;

            foreach (Data.FieldValue fv in doc.FieldValues)
            {
                string value;

                switch (fv.Type)
                {
                    case Hubble.Core.Data.DataType.NVarchar:
                    case Hubble.Core.Data.DataType.NChar:
                        value = string.Format("N'{0}'", fv.Value.Replace("'", "''"));
                        break;
                    case Hubble.Core.Data.DataType.Varchar:
                    case Hubble.Core.Data.DataType.Char:
                    case Hubble.Core.Data.DataType.Data:
                        value = string.Format("'{0}'", fv.Value.Replace("'", "''"));
                        break;
                    case Hubble.Core.Data.DataType.DateTime:
                    case Hubble.Core.Data.DataType.Date:
                    case Hubble.Core.Data.DataType.SmallDateTime:
                        DateTime dateTime;
                        if (!DateTime.TryParseExact(fv.Value, "yyyy-MM-dd HH:mm:ss", null,
                            System.Globalization.DateTimeStyles.None, out dateTime))
                        {
                            dateTime = DateTime.Parse(fv.Value);
                        }

                        value = string.Format("to_date('{0}','yyyy-mm-dd HH24:MI:SS')", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        break;
                    default:
                        value = string.Format("{0}", fv.Value);
                        break;
                }

                if (i++ == 0)
                {
                    sql.AppendFormat("{0}={1}", GetFieldName(fv.FieldName), value);
                }
                else
                {
                    sql.AppendFormat(",{0}={1}", GetFieldName(fv.FieldName), value);
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

            using (OracleDataProvider sqlData = new OracleDataProvider())
            {
                sqlData.Connect(Table.ConnectionString);
                sqlData.ExcuteSql(sql.ToString());
            }
        }

        public void MirrorUpdate(IList<FieldValue> fieldValues, IList<List<FieldValue>> docValues, IList<Hubble.Core.Query.DocumentResultForSort> docs)
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
                    sql.AppendFormat("{0}", GetFieldName(field.Name));
                }
                else
                {
                    sql.AppendFormat(",{0}", GetFieldName(field.Name));
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

            for (int j = begin; j < begin + count; j++)
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

            foreach (List<Core.Query.DocumentResultForSort> docSort in docResultForSortList)
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

            List<Core.Query.DocumentResultForSort> result = new List<Core.Query.DocumentResultForSort>();

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

                    result.Add(new Core.Query.DocumentResultForSort(docId));
                }
            }

            return result;

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

            Core.SFQL.Parse.DocumentResultWhereDictionary result = new Core.SFQL.Parse.DocumentResultWhereDictionary();

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
