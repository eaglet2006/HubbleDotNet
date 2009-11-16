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

        private double _Ratio = 0.5; //Clean up ratio

        private long _TotalMemoryNotCollect = 0;

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
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadProc));
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

        static void ThreadProc(Object stateInfo)
        {
            // No state object was passed to QueueUserWorkItem, so 
            // stateInfo is null.
            Thread.Sleep(2000);
            GC.Collect();
        }


        public void Collect()
        {
            try
            {
                if (TotalMemorySize > MaxMemorySize)
                {
                    lock (_LockObj)
                    {
                        for (int i = 0; i < _MaxStair; i++)
                        {
                            foreach (IManagedCache cache in _ManagedCacheList)
                            {
                                if (i >= cache.MaxStair)
                                {
                                    continue;
                                }

                                cache.Clear(i);
                            }

                            if ((double)_TotalMemorySize <= (double)_MaxMemorySize * Ratio)
                            {
                                break;
                            }
                        }

                        GC.Collect();

                        _TotalMemoryNotCollect = 0;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadProc));
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

        }

        public void Add(IManagedCache cache)
        {
            lock (_LockObj)
            {
                _ManagedCacheList.Add(cache);
                _MaxStair = Math.Max(_MaxStair, cache.MaxStair);
            }
        }

        public IManagedCache[] GetCaches()
        {
            lock (_LockObj)
            {
                return _ManagedCacheList.ToArray();
            }
        }


    }
}
