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

using Hubble.Framework.Net;
using Hubble.Framework.Threading;

namespace Hubble.Core.Service
{
    class QueryThreadPool
    {
        Stack<QueryThread> _Pool;

        Queue<MessageReceiveEventArgs> _Queue;

        object _LockObj = new object();

        internal QueryThreadPool(int capability)
        {
            _Pool = new Stack<QueryThread>(capability);

            for (int i = 0; i < capability; i++)
            {
                _Pool.Push(new QueryThread(false, this));
            }

            _Queue = new Queue<MessageReceiveEventArgs>();
        }

        internal void ExecuteSql(MessageReceiveEventArgs args)
        {
            lock (_LockObj)
            {
                if (_Pool.Count > 0)
                {
                    QueryThread qThread = _Pool.Pop();
                    qThread.ASendMessage((int)SQLClient.ConnectEvent.ExcuteSql, args);
                }
                else
                {
                    _Queue.Enqueue(args);
                }
            }
        }

        internal void ExecuteFinished(QueryThread qThread)
        {
            lock (_LockObj)
            {
                if (_Queue.Count > 0)
                {
                    MessageReceiveEventArgs args = _Queue.Dequeue();
                    qThread.ASendMessage((int)SQLClient.ConnectEvent.ExcuteSql, args);
                }
                else
                {
                    _Pool.Push(qThread);
                }
            }
        }
    }
}
