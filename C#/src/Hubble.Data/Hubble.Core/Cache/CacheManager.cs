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

namespace Hubble.Core.Cache
{
    internal class CacheManager
    {
        private static Dictionary<IManagedCache, int> _CacheToReduceTimes = new Dictionary<IManagedCache, int>();
        private static object _LockObj = new object();
        private static System.Threading.Thread _CacheManagerThread;

        private static object _InserCountLockObj = new object();
        private static int _InsertCount = 0; //Insert count before last GC.Collect;

        public static int InsertCount
        {
            get
            {
                lock (_InserCountLockObj)
                {
                    return _InsertCount;
                }
            }

            set
            {
                lock (_InserCountLockObj)
                {
                    _InsertCount = value;
                }
            }
        }

        private static void CacheManageProc()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10000);

                if (InsertCount > 1000)
                {
                    InsertCount = 0;
                    GC.Collect();
                }

                continue;
                
                //version 0.8, cancel the index cache,so following code is no usefull.
                //I will modify index cache in version 0.9 
                long pageSize = System.Diagnostics.Process.GetCurrentProcess().PagedMemorySize64 ;
                if (pageSize < Global.Setting.Config.MemoryLimited)
                {
                    continue;
                }

                lock (_LockObj)
                {
                    foreach (IManagedCache cache in _CacheToReduceTimes.Keys)
                    {
                        cache.ReduceMemory(50);
                    }
                }

                GC.Collect();
            }
        }

        static CacheManager()
        {
            _CacheManagerThread = new System.Threading.Thread(new System.Threading.ThreadStart(CacheManageProc));
            _CacheManagerThread.Start();
        }

        public static void Register(IManagedCache managedCache)
        {
            lock (_LockObj)
            {
                if (!_CacheToReduceTimes.ContainsKey(managedCache))
                {
                    _CacheToReduceTimes.Add(managedCache, 0);
                }
            }
        }

        public static void UnRegister(IManagedCache managedCache)
        {
            lock (_LockObj)
            {
                if (_CacheToReduceTimes.ContainsKey(managedCache))
                {
                    _CacheToReduceTimes.Remove(managedCache);
                }
            }
        }
    }
}
