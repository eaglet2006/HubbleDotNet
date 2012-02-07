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
using System.Linq;
using System.Text;

namespace Hubble.SQLClient
{
    class ConnectionPool
    {
        LinkedList<TcpItem> _TcpItemPool = new LinkedList<TcpItem>();
        int _MinPoolSize;
        string _ConnectionString;
        object _LockObj = new object();

        public int Count
        {
            get
            {
                lock (_LockObj)
                {
                    return _TcpItemPool.Count;
                }
            }
        }

        private void InitPool(HubbleAsyncConnection asyncConnection)
        {
            lock (_LockObj)
            {
                for (int i = 0; i < _MinPoolSize; i++)
                {
                    Hubble.Framework.Net.TcpClient tcpClient;

                    asyncConnection.InitTcpClient(out tcpClient);

                    TcpItem item = new TcpItem();
                    item.TcpClient = tcpClient;

                    _TcpItemPool.AddLast(item);
                }
            }
        }

        internal TcpItem TryGetAConnectedItem()
        {
            lock (_LockObj)
            {
                if (_TcpItemPool.Count <= 0)
                {
                    throw new System.IO.IOException("Connection Pool is empty");
                }

                if (_TcpItemPool.Count == 1)
                {
                    _TcpItemPool.First.Value.ConnectTimes++;
                    return _TcpItemPool.First.Value;
                }

                LinkedListNode<TcpItem> node = _TcpItemPool.First;

                while (node.Value.TcpClient.Closed || node.Value.ServerBusy)
                {
                    node = node.Next;

                    if (node == null)
                    {
                        return null;
                    }
                }

                TcpItem item = node.Value;

                item.ConnectTimes++;

                _TcpItemPool.Remove(node);
                
                _TcpItemPool.AddLast(node);

                return item;
            }
        }


        internal TcpItem GetOne()
        {
            lock (_LockObj)
            {
                if (_TcpItemPool.Count <= 0)
                {
                    throw new System.IO.IOException("Connection Pool is empty");
                }

                if (_TcpItemPool.Count == 1)
                {
                    _TcpItemPool.First.Value.ConnectTimes++;
                    return _TcpItemPool.First.Value;
                }

                LinkedListNode<TcpItem> node = _TcpItemPool.First;

                while (node.Value.ServerBusy)
                {
                    node = node.Next;

                    if (node == null)
                    {
                        node = _TcpItemPool.First;
                        break;
                    }
                }

                TcpItem item = node.Value;
                
                item.ConnectTimes++;

                _TcpItemPool.Remove(node);

                _TcpItemPool.AddLast(node);

                return item;
            }
        }

        internal void Remove(TcpItem tcpItem)
        {
            lock (_LockObj)
            {
                try
                {
                    _TcpItemPool.Remove(tcpItem);
                }
                catch
                {
                }
            }
        }

        internal ConnectionPool(HubbleAsyncConnection asyncConnection, string connectionString)
        {
            _ConnectionString = connectionString;

            //Get min pool size from connection string
            System.Data.SqlClient.SqlConnectionStringBuilder buider =
                new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            _MinPoolSize = buider.MinPoolSize;

            if (_MinPoolSize <= 0)
            {
                _MinPoolSize = 1;
            }

            InitPool(asyncConnection);
        }


    }
}
