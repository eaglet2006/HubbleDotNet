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
using Hubble.Framework.Net;

namespace Hubble.SQLClient
{
    class TcpItem
    {
        Dictionary<int, IAsyncClass> _ClassDict = new Dictionary<int, IAsyncClass>();

        object _ClassIdLock = new object();
        object _ConnectingLock = new object();
        object _LockObj = new object();

        bool _Connecting = false;

        int _ClassId = -1;

        internal int GetClassId(IAsyncClass asyncClass)
        {
            lock (_ClassIdLock)
            {
                _ClassId++;

                if (_ClassId == int.MaxValue)
                {
                    _ClassId = 0;
                }

                _ClassDict.Add(_ClassId, asyncClass);
                return _ClassId;
            }
        }

        internal void RemoveClassId(int classId)
        {
            lock (_ClassIdLock)
            {
                _ClassDict.Remove(classId);
            }
        }

        internal bool TryGetConnecting()
        {
            lock (_ConnectingLock)
            {
                if (!_Connecting)
                {
                    _Connecting = true;
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        internal bool Connecting
        {
            get
            {
                lock (_ConnectingLock)
                {
                    return _Connecting;
                }
            }

            set
            {
                lock (_ConnectingLock)
                {
                    _Connecting = value;
                }
            }
        }


        internal void Enter()
        {
            Monitor.Enter(_LockObj);
        }

        internal void Leave()
        {
            Monitor.Exit(_LockObj);
        }

        TcpClient _TcpClient = null;

        internal TcpClient TcpClient
        {
            get
            {
                lock (_LockObj)
                {
                    return _TcpClient;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _TcpClient = value;
                    _ClassDict = new Dictionary<int, IAsyncClass>();
                    _TcpClient.AsyncMessageRecieved = AsyncMessageRecieved;
                }
            }
        }

        void AsyncMessageRecieved(ASyncPackage package)
        {
            IAsyncClass asyncClass;

            if (package.Message is TcpRemoteCloseException)
            {
                //Tcp connection closed
                //Release all classes
                lock (_ClassIdLock)
                {
                    foreach (IAsyncClass aClass in _ClassDict.Values)
                    {
                        try
                        {
                            aClass.ReceiveMessage(package.Message);
                        }
                        catch
                        {
                        }
                    }

                    _ClassDict.Clear();
                }
                return;
            }

            lock (_ClassIdLock)
            {
                if (!_ClassDict.TryGetValue(package.ClassId, out asyncClass))
                {
                    Console.WriteLine(string.Format("AsyncTcp Manager, class id = {0} is not find", 
                        package.ClassId));
                    return;
                }
            }

            asyncClass.ReceiveMessage(package.Message);
        }
    }

    static class AsyncTcpManager
    {
        static object _LockObj = new object();
        static Dictionary<string, TcpItem> _TcpItemDict = new Dictionary<string, TcpItem>();

        static internal TcpItem Get(string connectionString)
        {
            lock (_LockObj)
            {
                TcpItem item;

                if (_TcpItemDict.TryGetValue(connectionString, out item))
                {
                    return item;
                }
                else
                {
                    return null;
                }
            }
        }

        static internal TcpItem Set(string connectionString, TcpClient tcpClient)
        {
            lock (_LockObj)
            {
                TcpItem item = new TcpItem();
                item.TcpClient = tcpClient;

                if (_TcpItemDict.ContainsKey(connectionString))
                {
                    _TcpItemDict[connectionString] = item;
                }
                else
                {
                    _TcpItemDict.Add(connectionString, item);
                }

                return item;
            }
        }

    }
}
