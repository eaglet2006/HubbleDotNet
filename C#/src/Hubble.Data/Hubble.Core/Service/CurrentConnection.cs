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

        public static void Disconnect(int managedThreadId)
        {
            lock (_RootSyn)
            {
                int threadId = managedThreadId;

                if (_ConnectionInformationDict.ContainsKey(threadId))
                {
                    _ConnectionInformationDict.Remove(threadId);
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

        public static ConnectionInformation ConnectionInfo
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
