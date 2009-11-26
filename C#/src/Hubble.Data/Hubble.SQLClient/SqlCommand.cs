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
                return _Sql;
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
                qResult.PrintMessages.Remove(message);
            }

            return sb.ToString();
        }

        /// <summary>
        /// SQL query
        /// </summary>
        /// <param name="cacheTimeout">Time out for data cache. In second</param>
        /// <returns></returns>
        public System.Data.DataSet Query(int cacheTimeout)
        {
            if (!_SqlConnection.Connected)
            {
                throw new System.Data.DataException("Sql Connection does not connect!");
            }

            string tableTicks = "";
            string orginalSql = BuildSql();

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
            }

            string sql = "";

            if (cacheTimeout >= 0)
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
            if (cacheTimeout >= 0)
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
    }
}
