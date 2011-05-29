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

        private void ReceiveError(Exception e)
        {
            if (e is Hubble.Framework.Threading.MQAbortException)
            {
                if (_Args != null)
                {
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
                _Args = data as MessageReceiveEventArgs;
                CurrentConnection currentConnection = new CurrentConnection(
                    _Args.ConnectionInfo as ConnectionInformation);

                CurrentConnection.ConnectionInfo.QueryThread = this;
                _ManagedThreadId = this.ManagedThreadId;

                HubbleTask.ExcuteSqlMessageProcess(_Args);

                _Args = null;

                CurrentConnection.Disconnect();
            }

            return null;
        }

        public QueryThread(bool closeWhenEmpty)
            : base(closeWhenEmpty, false)
        {
            OnMessageEvent = ReceiveMessage;
            OnErrorEvent = ReceiveError;
        }
    }
}
