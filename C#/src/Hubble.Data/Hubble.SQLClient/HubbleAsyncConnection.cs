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
            TcpItem tcpItem;
            DateTime start = DateTime.Now;
            DateTime end = start;

            lock (this)
            {
                tcpItem = AsyncTcpManager.Get(base.ConnectionString);
                if (tcpItem == null)
                {
                    Hubble.Framework.Net.TcpClient tcpClient;
                    InitTcpClient(out tcpClient);
                    tcpItem = AsyncTcpManager.Set(base.ConnectionString, tcpClient);
                }
            }

            while ((end - start).TotalMilliseconds <= timeout)
            {
                if (tcpItem.TcpClient.Closed)
                {
                    if (!tcpItem.TryGetConnecting())
                    {
                        try
                        {
                            tcpItem.Enter();
                            Hubble.Framework.Net.TcpClient tcpClient;
                            InitTcpClient(out tcpClient);
                            tcpClient.ReceiveTimeout = 0;

                            base.Open(tcpClient);
                            tcpClient.SetToAsync();
                            tcpItem.TcpClient = tcpClient;
                            return;
                        }
                        finally
                        {
                            tcpItem.Connecting = false;
                            tcpItem.Leave();
                        }
                    }
                    else
                    {
                        end = DateTime.Now;
                        System.Threading.Thread.Sleep(20);
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
                TcpItem tcpItem = AsyncTcpManager.Get(base.ConnectionString);
                if (tcpItem != null)
                {
                    tcpItem.RemoveClassId(classId);
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
            TcpItem tcpItem;

            lock (this)
            {
                tcpItem = AsyncTcpManager.Get(base.ConnectionString);
                if (tcpItem == null)
                {
                    throw new System.IO.IOException("Hasn't connection");
                }
            }

            if (classId < 0)
            {
                classId = tcpItem.GetClassId(command);
            }

            lock (_AsyncQueryLock)
            {
                Hubble.Framework.Net.ASyncPackage package = new Hubble.Framework.Net.ASyncPackage(
                    classId, sql);

                tcpItem.TcpClient.SendASyncMessage((short)ConnectEvent.ExcuteSql,
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
                TcpItem tcpItem = AsyncTcpManager.Get(base.ConnectionString);

                if (tcpItem == null)
                {
                    return false;
                }
                else
                {
                    if (tcpItem.Connecting)
                    {
                        return false;
                    }
                    else
                    {
                        return !tcpItem.TcpClient.Closed;
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
