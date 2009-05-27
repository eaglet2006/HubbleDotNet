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


        private static void CacheManageProc()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10000);

                if (System.Diagnostics.Process.GetCurrentProcess().PagedMemorySize64 < Global.Setting.Config.MemoryLimited)
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
