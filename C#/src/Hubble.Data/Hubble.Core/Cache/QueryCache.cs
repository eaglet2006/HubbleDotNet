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
using Hubble.Core.Query;
using System.IO.Compression;

namespace Hubble.Core.Cache
{
    class QueryCacheInformation
    {
        private bool _All;
        private int _Count;
        private long _CacheTicks;

        public bool All
        {
            get
            {
                return _All;
            }
        }


        public int Count
        {
            get
            {
                return _Count;
            }
        }

        public long CacheTicks
        {
            get
            {
                return _CacheTicks;
            }
        }

        public QueryCacheInformation(int count, long cacheTicks)
        {
            _Count = count;
            _CacheTicks = cacheTicks;
        }

        public QueryCacheInformation(int count, long cacheTicks, bool all)
        {
            _All = all;
            _Count = count;
            _CacheTicks = cacheTicks;
        }
    }


    class QueryCacheDocuments
    {
        public DocumentResultForSort[] Documents;
        public int Count;
        public int ResultLength;

        public QueryCacheDocuments()
        {
            Documents = null;
            Count = 0;
        }

        public QueryCacheDocuments(int count, DocumentResultForSort[] docResults, int relDocCount)
        {
            Documents = docResults;
            Count = count;
            ResultLength = relDocCount;
        }

    }

    class QueryCache : Cache<QueryCacheDocuments>
    {
        const int CompressFrom = 20;

        public QueryCache(QueryCacheManager cacheMgr, string name)
            : base(cacheMgr)
        {
            base.m_Name = name;
        }

        protected override byte[] GetBytes(QueryCacheDocuments data)
        {
            System.IO.MemoryStream m = new System.IO.MemoryStream();
            m.Write(BitConverter.GetBytes(data.Count), 0, sizeof(int));
            m.Write(BitConverter.GetBytes(data.ResultLength), 0, sizeof(int));

            if (data.Count > CompressFrom)
            {
                using (GZipStream g = new GZipStream(m, CompressionMode.Compress, true))
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        data.Documents[i].Serialize(g);
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    data.Documents[i].Serialize(m);
                }
            }


            m.Position = 0;

            byte[] bytes = new byte[m.Length];

            m.Read(bytes, 0, bytes.Length);

            return bytes;

        }

        protected override QueryCacheDocuments GetData(byte[] buf)
        {
            QueryCacheDocuments result = new QueryCacheDocuments();

            System.IO.MemoryStream m = new System.IO.MemoryStream(buf);

            m.Position = 0;

            byte[] bytes = new byte[sizeof(int)];

            m.Read(bytes, 0, bytes.Length);

            result.Count = BitConverter.ToInt32(bytes, 0);

            m.Read(bytes, 0, bytes.Length);

            result.ResultLength = BitConverter.ToInt32(bytes, 0);

            if (result.Count > CompressFrom)
            {
                using (GZipStream g = new GZipStream(m, CompressionMode.Decompress, true))
                {
                    result.Documents = new DocumentResultForSort[result.Count];

                    for (int i = 0; i < result.Count; i++)
                    {
                        result.Documents[i] = DocumentResultForSort.Deserialize(g);
                    }
                }
            }
            else
            {
                result.Documents = new DocumentResultForSort[result.Count];

                for (int i = 0; i < result.Count; i++)
                {
                    result.Documents[i] = DocumentResultForSort.Deserialize(m);
                }
            }

            return result;
        }
    }
}
