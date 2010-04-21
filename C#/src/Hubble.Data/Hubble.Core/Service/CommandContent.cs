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

namespace Hubble.Core.Service
{

    internal class CommandContent
    {
        internal class DataCacheInfo
        {
            Dictionary<string, long> _TblToTicks = new Dictionary<string, long>();

            internal void Add(string tableName, long ticks)
            {
                string key = tableName.Trim().ToLower();

                if (_TblToTicks.ContainsKey(key))
                {
                    _TblToTicks[key] = ticks;
                }
                else
                {
                    _TblToTicks.Add(key, ticks);
                }
            }

            internal long GetTicks(string tableName)
            {
                string key = tableName.Trim().ToLower();
                long ticks = -1;

                if (_TblToTicks.TryGetValue(key, out ticks))
                {
                    return ticks;
                }
                else
                {
                    return -1;
                }

            }


        }

        private object _LockObj = new object();

        private bool _PerformanceReport = false;

        internal bool PerformanceReport
        {
            get
            {
                return _PerformanceReport;
            }

            set
            {
                _PerformanceReport = value;
            }
        }

        private StringBuilder _PerformanceReportText = null;

        internal string PerformanceReportText
        {
            get
            {
                lock (_LockObj)
                {
                    if (_PerformanceReportText == null)
                    {
                        return "";
                    }

                    return _PerformanceReportText.ToString();
                }
            }
        }

        internal void WritePerformanceReportText(string text)
        {
            lock (_LockObj)
            {
                if (_PerformanceReportText == null)
                {
                    _PerformanceReportText = new StringBuilder();
                }

                _PerformanceReportText.AppendLine(text);
            }
        }


        private DataCacheInfo _DataCache = null;

        internal bool NeedDataCache
        {
            get
            {
                return _DataCache != null;
            }

            set
            {
                if (value)
                {
                    if (_DataCache == null)
                    {
                        _DataCache = new DataCacheInfo();
                    }
                }
                else
                {
                    _DataCache = null;
                }
            }
        }

        internal DataCacheInfo DataCache
        {
            get
            {
                return _DataCache;
            }
        }

    }
}
