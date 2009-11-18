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
using Hubble.Framework.DataStructure;

namespace Hubble.SQLClient
{
    public static class DataCacheMgr
    {
        private static object _LockObj = new object();

        private static Dictionary<string, DataCache> _DataCacheDict = new Dictionary<string, DataCache>();

        private static CacheManage _Mgr = new CacheManage();

        public static long MaxMemorySize
        {
            get
            {
                return _Mgr.MaxMemorySize;
            }

            set
            {
                _Mgr.MaxMemorySize = value;
            }
        }

        static internal void OnConnect(SqlConnection conn)
        {
            lock (_LockObj)
            {
                string key = conn.DataSource.ToLower().Trim() + ":" + conn.TcpPort.ToString() + "\\" + conn.Database.ToLower().Trim();
                if (!_DataCacheDict.ContainsKey(key))
                {
                    _DataCacheDict.Add(key, new DataCache(_Mgr));
                }
            }
        }

        static internal void ChangeExpireTime(SqlConnection conn,
            string sql, DateTime expireTime)
        {
            DataCache dataCache;

            lock (_LockObj)
            {
                string key = conn.DataSource.ToLower().Trim() + ":" + conn.TcpPort.ToString() + "\\" + conn.Database.ToLower().Trim();
                if (!_DataCacheDict.TryGetValue(key, out dataCache))
                {
                    return;
                }
            }

            dataCache.ChangeExpireTime(sql.ToLower().Trim(), expireTime);
        }


        static internal void Insert(SqlConnection conn, 
            string sql, QueryResult qResult, DateTime expireTime, string tableTick)
        {
            DataCache dataCache;

            lock (_LockObj)
            {
                string key = conn.DataSource.ToLower().Trim() + ":" + conn.TcpPort.ToString() + "\\" + conn.Database.ToLower().Trim();
                if (!_DataCacheDict.TryGetValue(key, out dataCache))
                {
                    return;
                }
            }

            dataCache.Insert(sql.ToLower().Trim(), qResult, expireTime, tableTick);
        }

        static internal QueryResult Get(SqlConnection conn, string sql,
            out DateTime expireTime, out int hitCount, out string tableTicks)
        {
            DataCache dataCache;
            tableTicks = "";

            lock (_LockObj)
            {
                string key = conn.DataSource.ToLower().Trim() + ":" + conn.TcpPort.ToString() + "\\" + conn.Database.ToLower().Trim();
                if (!_DataCacheDict.TryGetValue(key, out dataCache))
                {
                    expireTime = default(DateTime);
                    hitCount = 0;

                    return null;
                }
            }

            QueryResult result;

            object tag;

            if (dataCache.TryGetValue(sql.Trim().ToLower(), out result, out expireTime, out hitCount, out tag))
            {
                tableTicks = (string)tag;
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
