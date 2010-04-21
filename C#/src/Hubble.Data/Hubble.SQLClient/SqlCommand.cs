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
    public class SqlCommand
    {
        private SqlConnection _SqlConnection;

        private string _Sql;

        public string Sql
        {
            get
            {
                return BuildSql(_Sql, _Parameters); ;
            }
        }

        private object[] _Parameters = null;

        private QueryResult _QueryResult;

        public QueryResult Result
        {
            get
            {
                return _QueryResult;
            }
        }

        private string BuildSql()
        {
            return BuildSql(_Sql, _Parameters);
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

        public SqlCommand(SqlConnection sqlConn)
            :this("", sqlConn)
        {

        }

        public SqlCommand(string sql, SqlConnection sqlConn)
        {
            _Sql = sql;
            _SqlConnection = sqlConn;
            _Parameters = null;
        }

        public SqlCommand(string sql, SqlConnection sqlConn, params object[] parameters)
        {
            _Sql = sql;
            _SqlConnection = sqlConn;
            _Parameters = parameters;
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

        private System.Data.DataSet InnerQuery(string orginalSql, int cacheTimeout)
        {
            if (!_SqlConnection.Connected)
            {
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
                        //if multi select, disable data cache
                        needCache = false;
                    }
                }
            }

            string sql = "";

            if (needCache)
            {
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
                    System.Data.DataSet ds = new System.Data.DataSet();

                    tableTicks = GetTableTicks(_QueryResult);

                    DateTime expireTime;
                    int hitCount;

                    QueryResult qResult = null;

                    int noChangedTables = 0;

                    for (int i = 0; i < _QueryResult.DataSet.Tables.Count; i++)
                    {
                        System.Data.DataTable table;

                        if (_QueryResult.DataSet.Tables[i].MinimumCapacity == int.MaxValue)
                        {
                            if (qResult == null)
                            {
                                qResult = DataCacheMgr.Get(_SqlConnection,
                                    orginalSql, out expireTime, out hitCount, out tableTicks);
                            }

                            if (qResult != null)
                            {
                                noChangedTables++;
                                table = qResult.DataSet.Tables[i];
                                qResult.DataSet.Tables.Remove(table);
                                ds.Tables.Add(table);
                            }
                            else
                            {
                                table = _QueryResult.DataSet.Tables[i];
                                _QueryResult.DataSet.Tables.Remove(table);

                                ds.Tables.Add(table);
                            }
                        }
                        else
                        {
                            table = _QueryResult.DataSet.Tables[i];
                            _QueryResult.DataSet.Tables.Remove(table);

                            ds.Tables.Add(table);
                        }
                    }

                    _QueryResult.DataSet = ds;

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
            return Query(BuildSql(), cacheTimeout);
        }

        public System.Data.DataSet Query()
        {
            return Query(-1);
        }

        public int ExecuteNonQuery()
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
 
    }
}
