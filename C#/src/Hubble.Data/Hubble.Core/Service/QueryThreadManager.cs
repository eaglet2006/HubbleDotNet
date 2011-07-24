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
    static class QueryThreadManager
    {
        static object _LockObj = new object();
        static QueryThreadPool _SPPool; //Primary pool used to query store procedure
        static QueryThreadPool _PrimaryPool;
        static QueryThreadPool _PriorPool; //Prior pool used for the query from bigtable 


        internal static void Init(int queryThreadNum)
        {
            _SPPool = new QueryThreadPool(16);

            int primaryPoolCount = Math.Max(1, queryThreadNum);

            _PrimaryPool = new QueryThreadPool(primaryPoolCount);

            _PriorPool = new QueryThreadPool(primaryPoolCount);
        }

        internal static void ExcuteSql(MessageReceiveEventArgs args)
        {
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
                _SPPool.ExecuteSql(args);
            }
            else
            {
                if ((args.MsgHead.Flag & MessageFlag.Prior) == 0)
                {
                    _PrimaryPool.ExecuteSql(args);
                }
                else
                {
                    _PriorPool.ExecuteSql(args);
                }
            }
        }
    }
}
