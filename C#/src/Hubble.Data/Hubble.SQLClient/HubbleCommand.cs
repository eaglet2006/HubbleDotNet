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

namespace Hubble.SQLClient
{
    public class HubbleCommand : System.Data.Common.DbCommand, ICloneable
    {
        class DbParameterForSort : IComparer<System.Data.Common.DbParameter>
        {

            #region IComparer<DbParameter> Members

            public int Compare(System.Data.Common.DbParameter x, System.Data.Common.DbParameter y)
            {
                return y.ParameterName.CompareTo(x.ParameterName);
            }

            #endregion
        }


        private HubbleConnection _SqlConnection;

        private string _Sql;

        public string Sql
        {
            get
            {
                if (_ObjParameters != null)
                {
                    return BuildSql(_Sql, _ObjParameters);
                }
                else if (base.Parameters.Count > 0)
                {
                    return BuildSqlWithSqlParameter(_Sql, base.Parameters);
                }
                else
                {
                    return _Sql;
                }
            }
        }

        private object[] _ObjParameters = null;

        private HubbleParameterCollection _Parameters = null;

        new public HubbleParameterCollection Parameters
        {
            get
            {
                if (_Parameters == null)
                {
                    _Parameters = new HubbleParameterCollection();
                }

                return _Parameters;
            }
        }

        private QueryResult _QueryResult;

        public QueryResult Result
        {
            get
            {
                return _QueryResult;
            }
        }

        private int _CacheTimeout = -1;

        public int CacheTimeout
        {
            get
            {
                return _CacheTimeout;
            }

            set
            {
                _CacheTimeout = value;
            }
        }

        private bool _ResetDataCacheAfterTimeout = false;

        public bool ResetDataCacheAfterTimeout
        {
            get
            {
                return _ResetDataCacheAfterTimeout;
            }

            set
            {
                _ResetDataCacheAfterTimeout = value;
            }
        }


        private static string BuildSqlWithSqlParameter(string sql, System.Data.Common.DbParameterCollection paras)
        {
            System.Data.Common.DbParameter[] arrParas = new System.Data.Common.DbParameter[paras.Count];

            for (int i = 0; i < paras.Count; i++)
            {
                arrParas[i] = paras[i];
            }

            Array.Sort(arrParas, new DbParameterForSort());

            object[] objParas = new object[arrParas.Length]; 

            for (int i = 0; i < arrParas.Length; i++)
            {
                sql = sql.Replace(arrParas[i].ParameterName, "{" + i.ToString() + "}");

                switch (arrParas[i].DbType)
                {
                    case System.Data.DbType.AnsiString:
                    case System.Data.DbType.AnsiStringFixedLength:
                    case System.Data.DbType.String:
                    case System.Data.DbType.StringFixedLength:
                    case System.Data.DbType.Xml:
                        objParas[i] = arrParas[i].Value as string;
                        break;

                    case System.Data.DbType.Boolean:
                        if (arrParas[i].Value == null)
                        {
                            objParas[i] = null;
                        }
                        else
                        {
                            objParas[i] = arrParas[i].Value.ToString();
                        }
                        break;

                    case System.Data.DbType.Date:
                        if (arrParas[i].Value == null)
                        {
                            objParas[i] = null;
                        }
                        else
                        {
                            objParas[i] = ((DateTime)arrParas[i].Value).ToString("yyyy-MM-dd");
                        }
                        break;

                    case System.Data.DbType.DateTime:
                    case System.Data.DbType.DateTime2:
                        if (arrParas[i].Value == null)
                        {
                            objParas[i] = null;
                        }
                        else
                        {
                            objParas[i] = ((DateTime)arrParas[i].Value).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        break;
                    case System.Data.DbType.Time:
                        if (arrParas[i].Value == null)
                        {
                            objParas[i] = null;
                        }
                        else
                        {
                            objParas[i] = ((DateTime)arrParas[i].Value).ToString("HH:mm:ss");
                        }
                        break;
                    case System.Data.DbType.Byte:
                    case System.Data.DbType.UInt16:
                    case System.Data.DbType.UInt32:
                    case System.Data.DbType.UInt64:
                        if (arrParas[i].Value == null)
                        {
                            objParas[i] = null;
                        }
                        else
                        {
                            objParas[i] = ulong.Parse(arrParas[i].Value.ToString());
                        }
                        break;

                    case System.Data.DbType.Decimal:
                    case System.Data.DbType.Double:
                    case System.Data.DbType.Single:
                        if (arrParas[i].Value == null)
                        {
                            objParas[i] = null;
                        }
                        else
                        {
                            objParas[i] = double.Parse(arrParas[i].Value.ToString());
                        }
                        break;

                    case System.Data.DbType.Int16:
                    case System.Data.DbType.Int32:
                    case System.Data.DbType.Int64:
                    case System.Data.DbType.SByte:

                        if (arrParas[i].Value == null)
                        {
                            objParas[i] = null;
                        }
                        else
                        {
                            objParas[i] = long.Parse(arrParas[i].Value.ToString());
                        }
                        break;

                    default:
                        throw new System.Data.DataException(string.Format("Invalid parameter DataType: {0}", arrParas[i].DbType));
                }
            }

            return BuildSql(sql, objParas);
        }

        public static string BuildSql(string sql, object[] paras)
        {
            if (paras == null)
            {
                return sql;
            }
            else
            {
                object[] parameters = new object[paras.Length];
                paras.CopyTo(parameters, 0);

                for (int i = 0; i < parameters.Length; i++)
                {
                    object obj = parameters[i];

                    if (obj == null)
                    {
                        parameters[i] = "NULL";
                    }
                    else if (obj is string)
                    {
                        parameters[i] = string.Format("'{0}'",
                            ((string)obj).Replace("'", "''"));
                    }
                    else if (obj is DateTime)
                    {
                        parameters[i] = string.Format("'{0}'",
                            ((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else if (obj is byte[])
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.Append("0x");

                        foreach (byte b in (byte[])obj)
                        {
                            sb.AppendFormat("{0:x1}", b);
                        }

                        parameters[i] = sb.ToString();
                    }
                    else
                    {
                        parameters[i] = obj.ToString();
                    }

                }

                return string.Format(sql, parameters);
            }
        }

        public HubbleCommand(HubbleConnection sqlConn)
            :this("", sqlConn)
        {

        }

        public HubbleCommand(string sql, HubbleConnection sqlConn)
        {
            _Sql = sql;
            _SqlConnection = sqlConn;
            _ObjParameters = null;
        }

        public HubbleCommand(string sql, HubbleConnection sqlConn, params object[] parameters)
        {
            _Sql = sql;
            _SqlConnection = sqlConn;
            _ObjParameters = parameters;
        }

        private string GetTableTicks(QueryResult qResult)
        {
            StringBuilder sb = new StringBuilder();

            List<string> removeMessages = new List<string>();

            foreach (string message in qResult.PrintMessages)
            {
                string tblTick = Hubble.Framework.Text.Regx.GetMatch(message, @"<TableTicks>(.+?)</TableTicks>", true);

                if (string.IsNullOrEmpty(tblTick))
                {
                    continue;
                }

                removeMessages.Add(message);
                sb.Append(tblTick);
            }

            foreach (string message in removeMessages)
            {
                qResult.RemovePrintMessage(message);
            }

            return sb.ToString();
        }

        private bool IsSelectOrSpTalbe(System.Data.DataTable table)
        {
            if (table.TableName.IndexOf("Select_", 0, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return true;
            }
            else if (table.TableName.IndexOf("StoreProc_", 0, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Merge data cache and table to the result
        /// </summary>
        /// <param name="qrDataCache">data cache</param>
        /// <param name="table">table from server</param>
        /// <param name="result">add the merge table to result</param>
        private void MergeDataCache(QueryResult qrDataCache, System.Data.DataTable table, QueryResult result)
        {
            int i = 0;
            bool find = false;

            for (; i < qrDataCache.DataSet.Tables.Count; i++)
            {
                if (qrDataCache.DataSet.Tables[i].TableName.Equals(table.TableName, StringComparison.CurrentCultureIgnoreCase))
                {
                    find = true;
                    break;
                }
            }

            if (find)
            {
                System.Data.DataTable[] tables = new System.Data.DataTable[qrDataCache.DataSet.Tables.Count];
                qrDataCache.DataSet.Tables.CopyTo(tables, 0);

                do
                {
                    qrDataCache.DataSet.Tables.Remove(tables[i]);
                    result.AddDataTable(tables[i]);

                    if (++i >= tables.Length)
                    {
                        break;
                    }
                }
                while (!IsSelectOrSpTalbe(tables[i]));
            }
        }

        private System.Data.DataSet InnerQuery(string orginalSql, int cacheTimeout)
        {
            if (!_SqlConnection.Connected)
            {
                if (_SqlConnection.ServerBusy)
                {
                    DateTime expireTime;
                    int hitCount;
                    string tableTicks1;

                    _QueryResult = DataCacheMgr.Get(_SqlConnection,
                        orginalSql, out expireTime, out hitCount, out tableTicks1);
                    if (_QueryResult != null)
                    {
                        return _QueryResult.DataSet;
                    }
                    else
                    {
                        _SqlConnection.CommandReportNoCache = true;
                    }
                }

                try
                {
                    _SqlConnection.Open();
                }
                catch
                {
                    throw new System.Data.DataException("Sql Connection does not connect!");
                }
            }

            string tableTicks = "";

            bool needCache = cacheTimeout >= 0;

            if (cacheTimeout >= 0)
            {
                DateTime expireTime;
                int hitCount;

                _QueryResult = DataCacheMgr.Get(_SqlConnection,
                    orginalSql, out expireTime, out hitCount, out tableTicks);

                if (_QueryResult == null)
                {
                    tableTicks = "";
                }

                if (DateTime.Now < expireTime && _QueryResult != null)
                {
                    //not expire

                    return _QueryResult.DataSet;
                }
                else
                {
                    if (orginalSql.Contains(";"))
                    {
                        //if multi select and not unionselect, disable data cache

                        if (orginalSql.Trim().IndexOf("[UnionSelect]", 0, StringComparison.CurrentCultureIgnoreCase) != 0)
                        {
                            needCache = false;
                        }
                    }
                }
            }

            string sql = "";

            if (needCache)
            {
                if (ResetDataCacheAfterTimeout)
                {
                    tableTicks = "";
                }

                if (tableTicks == "")
                {
                    sql = "exec SP_InnerDataCache;";
                }
                else
                {
                    sql = "exec SP_InnerDataCache '" + tableTicks.Replace("'", "''") + "';";
                }
            }

            sql += orginalSql;

            _QueryResult = _SqlConnection.QuerySql(sql);

            //Data cache
            if (cacheTimeout > 0 || needCache)
            {
                if (_QueryResult.DataSet.Tables.Count >= 0)
                {
                    QueryResult qr = new QueryResult();

                    tableTicks = GetTableTicks(_QueryResult);

                    DateTime expireTime;
                    int hitCount;

                    QueryResult qResult = null;

                    int noChangedTables = 0;

                    List<System.Data.DataTable> sourceTables = new List<System.Data.DataTable>();

                    for (int i = 0; i < _QueryResult.DataSet.Tables.Count; i++)
                    {
                        sourceTables.Add(_QueryResult.DataSet.Tables[i]);
                    }

                    for (int i = 0; i < sourceTables.Count; i++)
                    {
                        System.Data.DataTable table;

                        if (sourceTables[i].MinimumCapacity == int.MaxValue)
                        {
                            if (qResult == null)
                            {
                                qResult = DataCacheMgr.Get(_SqlConnection,
                                    orginalSql, out expireTime, out hitCount, out tableTicks);
                            }

                            if (qResult != null)
                            {
                                noChangedTables++;

                                table = sourceTables[i];
                                MergeDataCache(qResult, table, qr);
                                //table = qResult.DataSet.Tables[i];
                                //qResult.DataSet.Tables.Remove(table);
                                //qr.AddDataTable(table);
                            }
                            else
                            {
                                table = sourceTables[i];
                                _QueryResult.DataSet.Tables.Remove(table);

                                qr.AddDataTable(table);
                            }
                        }
                        else
                        {
                            table = sourceTables[i];
                            _QueryResult.DataSet.Tables.Remove(table);

                            qr.AddDataTable(table);
                        }
                    }

                    _QueryResult.DataSet = qr.DataSet;

                    if (noChangedTables != _QueryResult.DataSet.Tables.Count)
                    {
                        DataCacheMgr.Insert(_SqlConnection, orginalSql, _QueryResult, DateTime.Now.AddSeconds(cacheTimeout), tableTicks);
                    }
                    else
                    {
                        DataCacheMgr.ChangeExpireTime(_SqlConnection, orginalSql, DateTime.Now.AddSeconds(cacheTimeout));
                    }
                }
            }

            return _QueryResult.DataSet;
        }

        private System.Data.DataSet Query(string orginalSql, int cacheTimeout)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            try
            {
                sw.Start();
                return InnerQuery(orginalSql, cacheTimeout);
            }
            catch (System.IO.IOException ex)
            {
                sw.Stop();

                if (sw.ElapsedMilliseconds < 1000)
                {
                    return InnerQuery(orginalSql, cacheTimeout);
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                if (sw.IsRunning)
                {
                    sw.Stop();
                }
            }
        }

        /// <summary>
        /// SQL query
        /// </summary>
        /// <param name="cacheTimeout">Time out for data cache. In second</param>
        /// <returns></returns>
        public System.Data.DataSet Query(int cacheTimeout)
        {
            return Query(Sql, cacheTimeout);
        }

        public System.Data.DataSet Query()
        {
            return Query(CacheTimeout);
        }

        public string GetKeywordAnalyzerStringFromServer(string tableName, string fieldName, string keywords, int cacheTimeout, out string bySpace)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("exec SP_FieldAnalyze '{0}', '{1}', '{2}', 'sqlclient' ",
                tableName.Replace("'", "''"), fieldName.Replace("'", "''"), keywords.Replace("'", "''"));

            System.Data.DataTable table = Query(sb.ToString(), cacheTimeout).Tables[0];

            StringBuilder result = new StringBuilder();
            StringBuilder bySpaceSb = new StringBuilder();

            foreach (System.Data.DataRow row in table.Rows)
            {
                string word = row["Word"].ToString().Replace("'", "''");
                bySpaceSb.AppendFormat("{0} ", word);
                result.AppendFormat("{0}^{1}^{2} ", word, row["Rank"], row["Position"]);
            }

            bySpace = bySpaceSb.ToString().Trim();
            return result.ToString().Trim();
        }

        /// <summary>
        /// Get words positions
        /// Usually using for highlight
        /// </summary>
        /// <param name="words">words split by space</param>
        /// <param name="tableName">table name</param>
        /// <param name="fieldName">field name</param>
        /// <param name="docids">docid request</param>
        /// <returns></returns>
        public System.Data.DataTable GetWordsPositions(string words, string tableName, 
            string fieldName, long[] docids, int cacheTimeout)
        {
            if (docids.Length == 0)
            {
                return new System.Data.DataTable();
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("exec SP_GetWordsPositions '{0}', '{1}', '{2}' ",
                words.Replace("'", "''"), tableName.Replace("'", "''"), fieldName.Replace("'", "''"));

            foreach (long docid in docids)
            {
                sb.AppendFormat(",{0}", docid);
            }


            return Query(sb.ToString(), cacheTimeout).Tables[0];
        }


        #region ICloneable Members

        public object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override string CommandText
        {
            get
            {
                return _Sql;
            }
            set
            {
                _Sql = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error. 
        /// The time in seconds to wait for the command to execute. The default is 300 seconds.
        /// </summary>
        public override int CommandTimeout
        {
            get
            {
                return _SqlConnection.ConnectionTimeout;
            }
            set
            {
                _SqlConnection.SetConnectionTimeout(value);
            }
        }


        public override System.Data.CommandType CommandType
        {
            get
            {
                return System.Data.CommandType.Text;
            }

            set
            {
            }
        }

        protected override System.Data.Common.DbParameter CreateDbParameter()
        {
            return new System.Data.SqlClient.SqlParameter();
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return _SqlConnection;
            }

            set
            {
                _SqlConnection = value as HubbleConnection;
            }
        }

        protected override System.Data.Common.DbParameterCollection DbParameterCollection
        {
            get
            {
                return this.Parameters;
            }
        }

        protected override System.Data.Common.DbTransaction DbTransaction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool DesignTimeVisible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int ExecuteNonQuery()
        {
            Query();

            if (_QueryResult.DataSet != null)
            {
                if (_QueryResult.DataSet.Tables.Count > 0)
                {
                    return _QueryResult.DataSet.Tables[0].MinimumCapacity;
                }
            }

            return 0;
        }

        protected override System.Data.Common.DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override System.Data.UpdateRowSource UpdatedRowSource
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
