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

using Hubble.Framework.Threading;
using Hubble.Framework.Net;

namespace Hubble.Core.Service
{
    class QueryThread : MessageQueue
    {
        MessageReceiveEventArgs _Args = null;
        int _ManagedThreadId = -1;

        QueryThreadPool _Pool;

        private void ReceiveError(Exception e)
        {
            if (e is Hubble.Framework.Threading.MQAbortException)
            {
                if (_Args != null)
                {
                    _Pool.ExecuteFinished(this);

                    _Args.ReturnMsg = e;
                    CurrentConnection.Disconnect(_ManagedThreadId);
                    TcpServer.ReturnMessage(_Args);
                }
            }
        }

        object ReceiveMessage(int evt, MessageQueue.MessageFlag flag, object data)
        {
            if (evt == (int)SQLClient.ConnectEvent.ExcuteSql)
            {
                try
                {
                    _Args = data as MessageReceiveEventArgs;
                    CurrentConnection currentConnection = new CurrentConnection(
                        _Args.ConnectionInfo as ConnectionInformation);

                    CurrentConnection.ConnectionInfo.QueryThread = this;
                    _ManagedThreadId = this.ManagedThreadId;

                    if (!_Args.QueryStoreProcedure)
                    {
                        int queryQueueWaitingTimeout = Global.Setting.Config.QueryQueueWaitingTimeout;

                        if (queryQueueWaitingTimeout > 0)
                        {
                            TimeSpan timeSpan = DateTime.Now - _Args.StartTime;
                            if (timeSpan.TotalSeconds > queryQueueWaitingTimeout)
                            {
                                throw new Hubble.Core.Query.QueryException("Timeout during sql was waiting in Query Queue.");
                            }
                        }
                    }

                    HubbleTask.ExcuteSqlMessageProcess(_Args);

                    _Args = null;
                }
                catch(Exception e)
                {
                    _Args.ReturnMsg = e;
                    TcpServer.ReturnMessage(_Args);
                }
                finally
                {
                    _Pool.ExecuteFinished(this);
                    CurrentConnection.Disconnect();
                }
            }

            return null;

        }

        public QueryThread(bool closeWhenEmpty, QueryThreadPool pool)
            : base(closeWhenEmpty, false)
        {
            _Pool = pool;
            OnMessageEvent = ReceiveMessage;
            OnErrorEvent = ReceiveError;
        }
    }
}
