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
using Hubble.Core.Service;

namespace Hubble.Core.SFQL.Parse
{
    /// <summary>
    /// This class is used to be a watch dog which can 
    /// terminate long time select request.
    /// </summary>
    class SelectWatchDog
    {
        internal static SelectWatchDog WatchDog = new SelectWatchDog();

        class ThreadInfo
        {
            internal QueryThread QueryThread;
            internal string TableName;
            internal int TimeOut;
            internal int TimeRemain;
            internal Thread Thread;
            internal ThreadInfo(string tableName, int timeout, Thread thread)
            {
                TimeOut = timeout;
                TimeRemain = timeout;
                Thread = thread;
                TableName = tableName;
                QueryThread = null;
            }

            internal ThreadInfo(string tableName, int timeout, QueryThread thread)
            {
                TimeOut = timeout;
                TimeRemain = timeout;
                Thread = null;
                TableName = tableName;
                QueryThread = thread;
            }
        }

        object _LockObj = new object();
        Dictionary<int, ThreadInfo> _ThreadIdToThread = new Dictionary<int, ThreadInfo>();
        Thread _Thread;

        internal SelectWatchDog()
        {
        }


        void ThreadProc()
        {
            List<ThreadInfo> terminateThreads = new List<ThreadInfo>();
            while (true)
            {
                lock (_LockObj)
                {
                    if (_ThreadIdToThread.Count > 0)
                    {
                        foreach (ThreadInfo threadInfo in _ThreadIdToThread.Values)
                        {
                            threadInfo.TimeRemain -= 1000;

                            if (threadInfo.TimeRemain <= 0)
                            {
                                terminateThreads.Add(threadInfo);
                            }
                        }

                        foreach (ThreadInfo threadInfo in terminateThreads)
                        {
                            if (threadInfo.QueryThread != null)
                            {
                                _ThreadIdToThread.Remove(threadInfo.QueryThread.ManagedThreadId);
                            }
                            else
                            {
                                _ThreadIdToThread.Remove(threadInfo.Thread.ManagedThreadId);
                            }

                            try
                            {
                                if (threadInfo.QueryThread == null)
                                {
                                    threadInfo.Thread.Abort();
                                }
                                else
                                {
                                    // For async connection
                                    //Don't worry about abort when return message. because 
                                    //return select watch dog before return message to tcp channel.
                                    threadInfo.QueryThread.AbortAndRestart();
                                }

                                Global.Report.WriteErrorLog(string.Format("Select statement of {0} has been executing more then {1} ms. Abort it",
                                    threadInfo.TableName, threadInfo.TimeOut));
                            }
                            catch (Exception e)
                            {
                                Global.Report.WriteErrorLog("Select Watch dog Abort fail.", e);
                            }
                        }

                        if (terminateThreads.Count > 0)
                        {
                            terminateThreads.Clear();
                        }
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Enter watch dog
        /// </summary>
        /// <param name="timeout">in millisecond</param>
        internal void Enter(int timeout, string tableName)
        {
            if (timeout <= 0)
            {
                return;
            }

            if (System.Threading.Monitor.TryEnter (_LockObj, 200))
            {
                try
                {
                    if (_Thread == null)
                    {
                        _Thread = new Thread(ThreadProc);
                        _Thread.IsBackground = true;
                        _Thread.Start();
                    }

                    Thread thread = Thread.CurrentThread;

                    ThreadInfo threadInfo;
                    if (_ThreadIdToThread.TryGetValue(thread.ManagedThreadId, out threadInfo))
                    {
                        threadInfo.TimeRemain = timeout;
                    }
                    else
                    {
                        if (CurrentConnection.ConnectionInfo != null)
                        {
                            if (CurrentConnection.ConnectionInfo.QueryThread != null)
                            {
                                threadInfo = new ThreadInfo(tableName, timeout,
                                    CurrentConnection.ConnectionInfo.QueryThread);
                            }
                            else
                            {
                                threadInfo = new ThreadInfo(tableName, timeout, thread);
                            }
                        }
                        else
                        {
                            threadInfo = new ThreadInfo(tableName, timeout, thread);
                        }

                        _ThreadIdToThread.Add(thread.ManagedThreadId, threadInfo);
                    }
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog("Select Watch dog Enter.", e);
                }
                finally
                {
                    System.Threading.Monitor.Exit(_LockObj);
                }
            }
        }

        internal void Exit()
        {
            lock (_LockObj)
            {
                try
                {
                    Thread thread = Thread.CurrentThread;
                    _ThreadIdToThread.Remove(thread.ManagedThreadId);
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog("Select Watch dog Exit.", e);
                }
            }
        }


    }
}
