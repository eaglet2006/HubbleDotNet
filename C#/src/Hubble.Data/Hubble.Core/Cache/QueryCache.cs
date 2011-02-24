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
using System.Threading;

namespace Hubble.Core.Cache
{
    public class QueryCacheInformation
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

            set
            {
                _All = value;
            }
        }


        public int Count
        {
            get
            {
                return _Count;
            }

            set
            {
                _Count = value;
            }
        }

        public long CacheTicks
        {
            get
            {
                return _CacheTicks;
            }

            set
            {
                _CacheTicks = value;
            }
        }

        public QueryCacheInformation()
            :this(0, 0)
        {
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


    public class QueryCacheDocuments
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
            Documents = new DocumentResultForSort[count];
            Array.Copy(docResults, Documents, count);

            //Documents = docResults;
            Count = count;
            ResultLength = relDocCount;
        }

    }

    public class CacheFile
    {
        public string Key;
        public QueryCacheDocuments Documents;
        public QueryCacheInformation Info;

        public CacheFile()
        {
        }

        public CacheFile(string key, QueryCacheDocuments documents, QueryCacheInformation info)
        {
            Key = key;
            Documents = documents;
            Info = info;
        }
    }

    class QueryCache : Cache<QueryCacheDocuments>
    {
        private object _LockObj = new object();
        private object _WriteLockObj = new object();

        private string _CacheFileFolder = null;
        private Data.Table _Table;

        const int CompressFrom = 20;

        private bool CacheFileEnabled
        {
            get
            {
                return _CacheFileFolder != null;
            }
        }

        internal string CacheFileFolder
        {
            get
            {
                lock (_LockObj)
                {
                    return _CacheFileFolder;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    if (value == null)
                    {
                        _CacheFileFolder = null;
                    }
                    else
                    {
                        _CacheFileFolder = Hubble.Framework.IO.Path.AppendDivision(value, '\\');
                    }

                    if (_CacheFileFolder != null)
                    {
                        if (!System.IO.Directory.Exists(_CacheFileFolder))
                        {
                            System.IO.Directory.CreateDirectory(_CacheFileFolder);
                        }
                    }
                }
            }
        }

        private void SetFileLastWriteTime(string filePath)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SetFileLastWriteTime), filePath);
        }

        private void SetFileLastWriteTime(object filePath)
        {
            lock (_WriteLockObj)
            {
                try
                {
                    try
                    {
                        System.IO.File.SetLastWriteTime(filePath as string, DateTime.Now);
                    }
                    catch (Exception e)
                    {
                        Global.Report.WriteErrorLog(string.Format("Set file last write time fail. file name: {0} err:{1}",
                            filePath as string, e.Message));
                    }
                }
                catch
                {
                }
            }
        }

        private void WriteCacheFile(CacheFile cacheFile)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(WriteCacheFile), cacheFile);
        }

        private void WriteCacheFile(object state)
        {
            lock (_WriteLockObj)
            {
                try
                {
                    string filePath = "";

                    try
                    {
                        CacheFile cacheFile = state as CacheFile;

                        string bit16String = base.GetMD5String(cacheFile.Key);

                        filePath = _CacheFileFolder + bit16String + ".xml";

                        using (System.IO.FileStream fs = new System.IO.FileStream(filePath,
                                     System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                        {
                            Hubble.Framework.Serialization.XmlSerialization<CacheFile>.Serialize(
                                cacheFile, Encoding.UTF8, fs);
                        }
                    }
                    catch (Exception e)
                    {
                        Global.Report.WriteErrorLog(string.Format("Write cahce file fail. file name: {0} err:{1}",
                            filePath as string, e.Message));
                    }
                }
                catch
                {
                }
            }
        }

        public QueryCache(QueryCacheManager cacheMgr, Data.Table table, string name, string cacheFileFolder)
            : base(cacheMgr)
        {
            base.m_Name = name;
            CacheFileFolder = cacheFileFolder;
            _Table = table;
        }

        public QueryCache(QueryCacheManager cacheMgr, Data.Table table, string name)
            : this(cacheMgr, table, name, null)
        {
        }



        new public void Insert(string key, QueryCacheDocuments item, DateTime expireTime)
        {
            Insert(key, item, expireTime, null);
        }

        public void Insert(string key, QueryCacheDocuments item, DateTime expireTime, QueryCacheInformation cacheInfo)
        {
            if (Monitor.TryEnter(_LockObj, Timeout))
            {
                try
                {
                    base.Insert(key, item, expireTime, cacheInfo);

                    if (CacheFileEnabled)
                    {
                        WriteCacheFile(new CacheFile(key, item, cacheInfo));
                    }
                }
                finally
                {
                    Monitor.Exit(_LockObj);
                }
            }
        }

        new public bool TryGetValue(string key, out QueryCacheDocuments value, out DateTime expireTime, out int hitCount)
        {
            object tag;
            return TryGetValue(key, out value, out expireTime, out hitCount, out tag);
        }

        public bool TryGetValue(string key, out QueryCacheDocuments value, out DateTime expireTime, out int hitCount, out QueryCacheInformation cacheInfo)
        {
            if (Monitor.TryEnter(_LockObj, Timeout))
            {
                try
                {
                    if (CacheFileEnabled)
                    {
                        string bit16String = base.GetMD5String(key);

                        object tag;
                        bool result = base.TryGetValue(key, out value, out expireTime, out hitCount, out tag);

                        string filePath = _CacheFileFolder + bit16String + ".xml";

                        if (!result)
                        {
                            if (System.IO.File.Exists(filePath))
                            {
                                using (System.IO.FileStream fs = new System.IO.FileStream(filePath,
                                     System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                                {
                                    CacheFile cacheFile;

                                    try
                                    {
                                        cacheFile = Hubble.Framework.Serialization.XmlSerialization<CacheFile>.Deserialize(fs);
                                    }
                                    catch
                                    {
                                        cacheInfo = null;
                                        return false;

                                    }

                                    value = cacheFile.Documents;
                                    cacheInfo = cacheFile.Info;
                                    expireTime = new DateTime();
                                    hitCount = 1;
                                    base.Insert(key, value, expireTime, cacheInfo);
                                }

                                System.IO.File.SetLastWriteTime(filePath, DateTime.Now);
                                return true;
                            }
                            else
                            {
                                cacheInfo = null;
                                return false;
                            }
                        }
                        else
                        {
                            cacheInfo = tag as QueryCacheInformation;

                            try
                            {
                                if (!System.IO.File.Exists(filePath))
                                {
                                    WriteCacheFile(new CacheFile(key, value, cacheInfo));
                                }
                                else
                                {
                                    SetFileLastWriteTime(filePath);
                                }
                            }
                            catch
                            {
                            }

                            return true;
                        }
                    }
                    else
                    {
                        object tag;
                        bool result = base.TryGetValue(key, out value, out expireTime, out hitCount, out tag);
                        cacheInfo = tag as QueryCacheInformation;
                        return result;
                    }
                }
                finally
                {
                    Monitor.Exit(_LockObj);
                }
            }
            else
            {
                value = null;
                expireTime = default(DateTime);
                hitCount = 0;
                cacheInfo = null;
                return false;
            }
        }

        public override void DeleteExpireCacheFiles()
        {
            lock (_LockObj)
            {
                if (!CacheFileEnabled)
                {
                    return;
                }

                int days = _Table.CleanupQueryCacheFileInDays;

                foreach (string file in System.IO.Directory.GetFiles(_CacheFileFolder, "*.xml"))
                {
                    TimeSpan span = DateTime.Now - System.IO.File.GetLastWriteTime(file);

                    if (span.TotalDays > days)
                    {
                        System.IO.File.Delete(file);
                    }
                }

            }
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
