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

namespace Hubble.Core.Cache
{
    class QueryCacheManager : CacheManage
    {
        internal static QueryCacheManager Manager = new QueryCacheManager();

        protected override void BeginCollect()
        {
            Global.Report.WriteAppLog(string.Format("QueryCacheManager is Collecting!, TotalMemorySize = {0}", TotalMemorySize));
        }

        protected override void AfterCollect()
        {
            Global.Report.WriteAppLog(string.Format("QueryCacheManager is Collected!, TotalMemorySize = {0}", TotalMemorySize));
        }


        internal string GetQueryCacheReport()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("MaxMemorySize : {0} Total Memory Size:{1}\r\n",
                this.MaxMemorySize, this.TotalMemorySize);

            foreach (Hubble.Framework.DataStructure.IManagedCache cache in this.GetCaches())
            {
                sb.AppendLine();

                sb.AppendFormat("Cache Name:{0}\r\n", cache.Name);

                sb.AppendLine("Bucket infomations:");

                foreach (string bucketInfo in cache.BucketInfoList)
                {
                    sb.AppendFormat("{0}\r\n", bucketInfo);
                }
            }

            return sb.ToString();
        }
    }
}
