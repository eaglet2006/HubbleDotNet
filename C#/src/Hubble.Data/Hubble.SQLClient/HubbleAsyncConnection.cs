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

namespace Hubble.SQLClient
{
    /// <summary>
    /// asynchronous connection
    /// </summary>
    public class HubbleAsyncConnection : HubbleConnection
    {
        object _AsyncQueryLock = new object();
        TcpItem _TcpItem;

        /// <summary>
        /// Initializes a new instance of the SqlConnection  class.
        /// </summary>
        public HubbleAsyncConnection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlConnection  class when given a string that contains the connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public HubbleAsyncConnection(string connectionString)
            :base(connectionString)
        {
        }

        /// <summary>
        /// Connection string 
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }

            set
            {
                _ConnectionString = value;
                _SqlConnBuilder = null;
                //ConnectionTimeout = 300;
            }
        }

        /// <summary>
        /// Id of tcp item.
        /// </summary>
        public long TcpItemId
        {
            get
            {
                if (_TcpItem != null)
                {
                    return _TcpItem.ItemId;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Connection times of tcp item
        /// </summary>
        public long TcpItemConnectTimes
        {
            get
            {
                if (_TcpItem != null)
                {
                    return _TcpItem.ConnectTimes;
                }
                else
                {
                    return -1;
                }
            }
        }

        public override void Open()
        {
            Open(base.ConnectionTimeout * 1000);
        }

        /// <summary>
        /// open connection with timeout
        /// </summary>
        /// <param name="timeout">in milliseconds</param>
        public void Open(int timeout)
        {
            DateTime start = DateTime.Now;
            DateTime end = start;

            lock (this)
            {
                ConnectionPool pool = AsyncTcpManager.Get(this, base.ConnectionString);
                
                _TcpItem = pool.GetOne();
            }

            while ((end - start).TotalMilliseconds <= timeout)
            {
                if (end != start)
                {
                    //not first time

                    lock (this)
                    {
                        //Try to find a connected tcp item to use
                        ConnectionPool pool = AsyncTcpManager.Get(this, base.ConnectionString);

                        try
                        {
                            TcpItem tcpItem = pool.TryGetAConnectedItem();
                            if (tcpItem != null)
                            {
                                _TcpItem = tcpItem;
                            }

                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e);
                        }

                    }
                }

                if (_TcpItem.TcpClient.Closed)
                {
                    if (!_TcpItem.TryGetConnecting())
                    {
                        try
                        {
                            _TcpItem.Enter();
                            _TcpItem.ServerBusy = false;
                            Hubble.Framework.Net.TcpClient tcpClient;
                            InitTcpClient(out tcpClient);
                            tcpClient.ReceiveTimeout = 0;
                            TryConnectTimeout = 1;
                            CommandReportNoCache = false;

                            base.Open(tcpClient);

                            if (base.ServerBusy)
                            {
                                _TcpItem.ServerBusy = true;
                                System.Threading.Thread.Sleep(20);
                                end = DateTime.Now;
                            }
                            else
                            {
                                tcpClient.SetToAsync();
                                _TcpItem.TcpClient = tcpClient;
                                return;
                            }
                        }
                        finally
                        {
                            _TcpItem.Connecting = false;
                            _TcpItem.Leave();
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(20);
                        end = DateTime.Now;
                    }
                }
                else
                {
                    return;
                }

            }

            throw new System.IO.IOException("Connection timeout");
        }

        internal void EndAsyncQuery(int classId)
        {
            lock (this)
            {
                if (_TcpItem != null)
                {
                    _TcpItem.RemoveClassId(classId);
                }
            }
        }

        internal void BeginAsyncQuerySql(string sql, HubbleCommand command, ref int classId)
        {
            BeginAsyncQuerySql(sql, command, ref classId, false);
        }

        internal void BeginAsyncQuerySql(string sql, HubbleCommand command, ref int classId, 
            bool priorMessage)
        {
            lock (this)
            {
                if (_TcpItem == null)
                {
                    throw new System.IO.IOException("Hasn't connection");
                }
            }

            if (classId < 0)
            {
                classId = _TcpItem.GetClassId(command);
            }

            lock (_AsyncQueryLock)
            {
                Hubble.Framework.Net.ASyncPackage package = new Hubble.Framework.Net.ASyncPackage(
                    classId, sql);

                _TcpItem.TcpClient.SendASyncMessage((short)ConnectEvent.ExcuteSql,
                    ref package, priorMessage);
            }
        }

        internal override QueryResult QuerySql(string sql)
        {
            return null;
        }

        internal override bool ServerBusy
        {
            get
            {
                return base.ServerBusy;
            }
        }

        internal override int GetConnectionTimeout()
        {
            return base.GetConnectionTimeout();
        }

        //internal override void SetConnectionTimeout(int timeout)
        //{
        //    base.SetConnectionTimeout(timeout);
        //}

        public override bool Connected
        {
            get
            {
                if (_TcpItem == null)
                {
                    return false;
                }
                else
                {
                    if (_TcpItem.Connecting)
                    {
                        return false;
                    }
                    else
                    {
                        return !_TcpItem.TcpClient.Closed;
                    }
                }
            }
        }

        internal override bool CommandReportNoCache
        {
            get
            {
                return base.CommandReportNoCache;
            }
            set
            {
                base.CommandReportNoCache = value;
            }
        }
    }
}
