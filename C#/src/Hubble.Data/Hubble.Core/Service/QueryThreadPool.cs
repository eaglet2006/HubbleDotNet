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

using Hubble.Framework.Net;
using Hubble.Framework.Threading;

namespace Hubble.Core.Service
{
    static class QueryThreadPool
    {
        static object _LockObj = new object();
        static QueryThread[] _PrimaryPool; //Primary pool used to query store procedure
        static QueryThread[] _SecondaryPool;
        static Random _Rand = new Random();

        internal static void Init(int queryThreadNum)
        {
            _PrimaryPool = new QueryThread[16];

            for (int i = 0; i < _PrimaryPool.Length; i++)
            {
                _PrimaryPool[i] = new QueryThread(false);
            }

            int secondaryPoolCount = Math.Max(1, queryThreadNum);

            _SecondaryPool = new QueryThread[secondaryPoolCount];

            for (int i = 0; i < _SecondaryPool.Length; i++)
            {
                _SecondaryPool[i] = new QueryThread(false);
            }
        }

        internal static void ExcuteSql(MessageReceiveEventArgs args)
        {
            QueryThread queryThread = null;

            lock (_LockObj)
            {
                queryThread = _SecondaryPool[_Rand.Next(_SecondaryPool.Length)];

                bool queryStoreProcedure = false;

                if (args.InMessage is string)
                {
                    string sql = args.InMessage as string;

                    if (sql.StartsWith("exec", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (sql.IndexOf("select", 0, StringComparison.CurrentCultureIgnoreCase) < 0)
                        {
                            queryStoreProcedure = true;
                        }
                    }
                }

                if (queryStoreProcedure)
                {
                    queryThread = _PrimaryPool[_Rand.Next(_PrimaryPool.Length)];
                }
                else
                {
                    queryThread = _SecondaryPool[_Rand.Next(_SecondaryPool.Length)];
                }
            }

            queryThread.ASendMessage((int)SQLClient.ConnectEvent.ExcuteSql, args);
        }
    }
}
