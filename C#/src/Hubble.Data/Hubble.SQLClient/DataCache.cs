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
using System.IO;

namespace Hubble.SQLClient
{
    class DataCache : Cache <QueryResult>
    {
        public DataCache(CacheManage cacheMgr)
            : base(cacheMgr)
        {
        }

        protected override byte[] GetBytes(QueryResult data)
        {
            MemoryStream m = new MemoryStream();

            QueryResultSerialization.Serialize(m, data, false, 1);

            m.Position = 0;

            byte[] bytes = new byte[m.Length];

            m.Read(bytes, 0, bytes.Length);

            return bytes;
        }

        protected override QueryResult GetData(byte[] buf)
        {
            MemoryStream m = new MemoryStream(buf);
            m.Position = 0;

            return QueryResultSerialization.Deserialize(m, false, 1);
        }

        public override void DeleteExpireCacheFiles()
        {
        }
    }
}
