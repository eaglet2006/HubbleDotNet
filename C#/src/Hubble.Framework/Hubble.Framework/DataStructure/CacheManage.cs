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
using System.Threading;

namespace Hubble.Framework.DataStructure
{
    public class CacheManage
    {
        object _LockObj = new object();

        private int _MaxStair = 0;

        private long _TotalMemorySize = 0;

        private int _Interval = 30000; //In ms

        private long _MaxMemorySize = 64 * 1024;

        private List<IManagedCache> _ManagedCacheList = new List<IManagedCache>();

        private double _Ratio = 0.75; //Clean up ratio

        private long _TotalMemoryNotCollect = 0;

        private Thread _Thread;

        private bool _CacheAdded = false;
        private int _Tick = 0;

        private long TotalMemoryNotCollect
        {
            get
            {
                lock (_LockObj)
                {
                    return _TotalMemoryNotCollect;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _TotalMemoryNotCollect = value;
                }
            }
        }



        public long TotalMemorySize
        {
            get
            {
                lock (_LockObj)
                {
                    return _TotalMemorySize;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _TotalMemoryNotCollect += Math.Abs(value - _TotalMemorySize);
                    _TotalMemorySize = value;

                    if (_TotalMemoryNotCollect > _MaxMemorySize)
                    {
                        _TotalMemoryNotCollect = 0;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(GCCollect));
                    }
                }
            }
        }

        public long MaxMemorySize
        {
            get
            {
                lock (_LockObj)
                {
                    return _MaxMemorySize;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    if (value < 0)
                    {
                        _MaxMemorySize = 1;
                    }
                    else
                    {
                        _MaxMemorySize = value;
                    }

                    Collect();
                }
            }
        }

        public double Ratio
        {
            get
            {
                lock (_LockObj)
                {
                    return _Ratio;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _Ratio = value;

                    if (_Ratio < 0)
                    {
                        _Ratio = 0;
                    }
                }
            }
        }

        static void GCCollect(Object stateInfo)
        {
            // No state object was passed to QueueUserWorkItem, so 
            // stateInfo is null.
            Thread.Sleep(2000);
            GC.Collect();
        }

        void ThreadProc()
        {
            while (true)
            {
                Thread.Sleep(1000 * 60 * 60);
                DeleteExpireCacheFiles();
            }
        }

        internal void Add(IManagedCache cache)
        {
            lock (_LockObj)
            {
                _CacheAdded = true;
                _ManagedCacheList.Add(cache);
                _MaxStair = Math.Max(_MaxStair, cache.MaxStair);
            }
        }

        private void DeleteExpireCacheFiles()
        {
            try
            {
                lock (_LockObj)
                {
                    if (_CacheAdded || _Tick >= 24)
                    {
                        _CacheAdded = false;
                        _Tick = 0;

                        foreach (IManagedCache cache in _ManagedCacheList)
                        {
                            try
                            {
                                cache.DeleteExpireCacheFiles();
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine(string.Format("Delete expire Cache file fail, err:{0} stack:{1}",
                                    e.Message, e.StackTrace));
                            }
                        }
                    }
                    else
                    {
                        _Tick++;
                    }

                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Collect cache
        /// </summary>
        /// <remarks>
        /// This function will be called by 
        /// Cache.Insert
        /// So Only as Cache insert, it will collect.
        /// </remarks>
        internal void Collect()
        {
            try
            {
                if (TotalMemorySize > MaxMemorySize)
                {
                    lock (_LockObj)
                    {
                        long targetMemorySize = (long)(_MaxMemorySize * (_Ratio));

                        if (targetMemorySize > _MaxMemorySize)
                        {
                            targetMemorySize = _MaxMemorySize;
                        }

                        if (targetMemorySize < 0)
                        {
                            targetMemorySize = 0;
                        }


                        for (int i = 0; i < _MaxStair; i++)
                        {
                            long delta = _TotalMemorySize - targetMemorySize;

                            if (delta <= 0)
                            {
                                break;
                            }

                            long stairMemory = 0;

                            foreach (IManagedCache cache in _ManagedCacheList)
                            {
                                stairMemory += cache.GetBucketMemorySize(i);
                            }

                            double ratio = 0;

                            if (stairMemory <= delta)
                            {
                                ratio = 0;
                            }
                            else
                            {
                                ratio = (double)(stairMemory - delta) / (double)stairMemory;
                            }

                            foreach (IManagedCache cache in _ManagedCacheList)
                            {
                                if (i >= cache.MaxStair)
                                {
                                    continue;
                                }

                                cache.Clear(i, ratio);
                            }
                        }

                        GC.Collect();

                        _TotalMemoryNotCollect = 0;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(GCCollect));
                    }
                }
            }
            catch
            {
            }
        }

        public CacheManage()
            : this(30 * 1000, 64 * 1024)
        {

        }

        public CacheManage(int interval)
            : this(interval, 4 * 1024)
        {

        }

        public CacheManage(int interval, long maxMemorySize)
        {
            if (interval < 200)
            {
                _Interval = 200;
            }
            else
            {
                _Interval = interval;
            }

            MaxMemorySize = maxMemorySize;

            _Thread = new Thread(ThreadProc);

            _Thread.IsBackground = true;

            _Thread.Start();

        }

        public IManagedCache[] GetCaches()
        {
            lock (_LockObj)
            {
                return _ManagedCacheList.ToArray();
            }
        }

        public void Delete(IManagedCache cache)
        {
            for (int i = 0; i < cache.MaxStair; i++)
            {
                cache.Clear(i, 0);
            }

            _ManagedCacheList.Remove(cache);
        }
    }
}
