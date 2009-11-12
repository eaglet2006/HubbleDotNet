using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Service
{
    public class CurrentConnection
    {
        private static object _RootSyn = new object();
        private static Dictionary<int, ConnectionInformation> _ConnectionInformationDict 
            = new Dictionary<int,ConnectionInformation>(); //Key is current thread id, Value is connection information

        public CurrentConnection(ConnectionInformation info)
        {
            lock (_RootSyn)
            {
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                //info.DatabaseName

                if (_ConnectionInformationDict.ContainsKey(threadId))
                {
                    _ConnectionInformationDict[threadId] = info;
                }
                else
                {
                    _ConnectionInformationDict.Add(threadId, info);
                }
            }
        }

        public static void Connect()
        {
            lock (_RootSyn)
            {
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                if (_ConnectionInformationDict.ContainsKey(threadId))
                {
                    _ConnectionInformationDict[threadId] = null;
                }
                else
                {
                    _ConnectionInformationDict.Add(threadId, null);
                }
            }
        }

        public static void Disconnect()
        {
            lock (_RootSyn)
            {
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                if (_ConnectionInformationDict.ContainsKey(threadId))
                {
                    _ConnectionInformationDict.Remove(threadId);
                }
            }
        }

        internal static ConnectionInformation ConnectionInfo
        {
            get
            {
                lock (_RootSyn)
                {
                    ConnectionInformation result;

                    int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                    if (_ConnectionInformationDict.TryGetValue(threadId, out result))
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
