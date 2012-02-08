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
        static long _GlobalItemId = 0;
        static object _GlobalLockObj = new object();

        Dictionary<int, IAsyncClass> _ClassDict = new Dictionary<int, IAsyncClass>();

        object _ClassIdLock = new object();
        object _ConnectingLock = new object();
        object _LockObj = new object();

        bool _ServerBusy = false;

        bool _Connecting = false;

        long _ConnectTimes = 0;

        long _ItemId;

        int _ClassId = -1;

        internal long ItemId
        {
            get
            {
                return _ItemId;
            }
        }

        internal bool ServerBusy
        {
            get
            {
                lock (_ConnectingLock)
                {
                    return _ServerBusy;
                }
            }

            set
            {
                lock (_ConnectingLock)
                {
                    _ServerBusy = value;
                }
            }
        }

        internal long ConnectTimes
        {
            get
            {
                lock (_ConnectingLock)
                {
                    return _ConnectTimes;
                }
            }

            set
            {
                lock (_ConnectingLock)
                {
                    _ConnectTimes = value;
                }
            }
        }

        internal TcpItem()
        {
            lock (_GlobalLockObj)
            {
                _ItemId = _GlobalItemId++;
            }
        }

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
                    IAsyncClass[] iAsyncClass;
                    
                    lock (_LockObj)
                    {
                        iAsyncClass = new IAsyncClass[_ClassDict.Count];
                        _ClassDict.Values.CopyTo(iAsyncClass, 0);
                    }


                    foreach (IAsyncClass aClass in iAsyncClass)
                    {
                        try
                        {
                            aClass.ReceiveMessage(package.Message);
                        }
                        catch
                        {
                        }
                    }

                    lock (_LockObj)
                    {
                        _ClassDict.Clear();
                    }
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
        static Dictionary<string, ConnectionPool> _TcpItemDict = new Dictionary<string, ConnectionPool>();

        static internal void Remove(string connectionString)
        {
            lock (_LockObj)
            {
                ConnectionPool pool;

                if (_TcpItemDict.TryGetValue(connectionString, out pool))
                {
                    pool.Clear();
                    _TcpItemDict.Remove(connectionString);
                }

            }
        }

        static internal ConnectionPool Get(HubbleAsyncConnection asyncConnection, string connectionString)
        {
            lock (_LockObj)
            {
                ConnectionPool pool;

                if (_TcpItemDict.TryGetValue(connectionString, out pool))
                {
                    return pool;
                }
                else
                {
                    pool = new ConnectionPool(asyncConnection, connectionString);
                    _TcpItemDict.Add(connectionString, pool);
                    return pool;
                }
            }
        }

        //static internal ConnectionPool Set(HubbleAsyncConnection asyncConnection, string connectionString)
        //{
        //    lock (_LockObj)
        //    {
        //        ConnectionPool pool = new ConnectionPool(asyncConnection, connectionString);

        //        if (_TcpItemDict.ContainsKey(connectionString))
        //        {
        //            _TcpItemDict[connectionString] = pool;
        //        }
        //        else
        //        {
        //            _TcpItemDict.Add(connectionString, pool);
        //        }

        //        return pool;
        //    }
        //}

    }
}
