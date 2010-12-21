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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Hubble.Core.Service
{
    public class ThreadMonitor
    {
        #region Static private methods
        /// <summary>
        /// Get Get Total Processor Time.
        /// MilliSeconds
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public static double GetTotalProcessorTime()
        {
            TimeSpan s = Process.GetCurrentProcess().TotalProcessorTime;
            return s.TotalMilliseconds;
        }

        #endregion

        #region enum

        [Flags]
        public enum MonitorFlag
        {
            None = 0x00,
            MonitorHang = 0x01,
            MonitorDeadCycle = 0x02,
        }

        [Flags]
        public enum ThreadStatus
        {
            Normal = 0x00,
            Hang = 0x01,
            DeadCycle = 0x02,
        }

        #endregion

        #region classes

        private class TCB
        {
            private DateTime _LastHeartbeat;

            public DateTime LastHeartbeat
            {
                get
                {
                    lock (this)
                    {
                        return _LastHeartbeat;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _LastHeartbeat = value;
                    }
                }
            }

            private double _LastTotalProcessorTime;

            public double LastTotalProcessorTime
            {
                get
                {
                    lock (this)
                    {
                        return _LastTotalProcessorTime;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _LastTotalProcessorTime = value;
                    }
                }
            }

            private int _HitTimes;

            public int HitTimes
            {
                get 
                {
                    lock (this)
                    {
                        return _HitTimes;
                    }
                }

                set 
                {
                    lock (this)
                    {
                        _HitTimes = value;
                    }
                }
            }
	

            private DateTime _LastGotCpu;

            public DateTime LastGotCpu
            {
                get
                {
                    lock (this)
                    {
                        return _LastGotCpu;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _LastGotCpu = value;
                    }
                }
            }

            private MonitorParameter _Para;

            public MonitorParameter Para
            {
                get
                {
                    return _Para;
                }

                set
                {
                    _Para = value;
                }
            }

            private ThreadStatus _Status;

            public ThreadStatus Status
            {
                get 
                {
                    lock (this)
                    {
                        return _Status;
                    }
                }

                set 
                {
                    lock (this)
                    {
                        _Status = value;
                    }
                }
            }
	

            public TCB(MonitorParameter para)
            {
                _LastHeartbeat = DateTime.Now;
                _LastGotCpu = DateTime.Now;
                _Para = para;
                _HitTimes = 0;

                LastTotalProcessorTime = GetTotalProcessorTime();
            }
        }

        public class MonitorParameter
        {
            private Thread _Thread;

            /// <summary>
            /// Thread which need be monitored
            /// </summary>
            public Thread Thread 
            {
                get
                {
                    return _Thread;
                }
            }

            private string _Name;

            /// <summary>
            /// Thread name
            /// </summary>
            public string Name 
            {
                get
                {
                    return _Name;
                }
            }

            private int _HangTimeout = 10 * 1000; //defaut is 10 seconds

            /// <summary>
            /// Timeout for monitor thread hang. Millisecond
            /// </summary>
            public int HangTimeout 
            {
                get
                {
                    return _HangTimeout;
                }

                set
                {
                    _HangTimeout = value;
                }
            }

            private int _DeadCycleTimeout = 5000; //defaut is 5 seconds

            /// <summary>
            /// Timeout for monitor dead cycle. Millisecond
            /// </summary>
            public int DeadCycleTimeout
            {
                get
                {
                    return _DeadCycleTimeout;
                }

                set
                {
                    _DeadCycleTimeout = value;
                }
            }

            private MonitorFlag _Flag = MonitorFlag.None;

            public MonitorFlag Flag
            {
                get
                {
                    return _Flag;
                }

                set
                {
                    _Flag = value;
                }
            }

            public MonitorParameter(Thread t, string name, MonitorFlag flag)
            {
                _Thread = t;
                _Name = name;
                _Flag = flag;
            }

            public MonitorParameter(Thread t, string name, int hangTimeout, 
                int deadCycleTimeout, MonitorFlag flag)
                : this(t, name, flag)
            {
                HangTimeout = hangTimeout;
                DeadCycleTimeout = deadCycleTimeout;
            }

        }

        #endregion


        #region Events

        public event EventHandler<ThreadMonitorEvent> ThradMonitorEventHandler;

        protected void OnThreadMonitorEvent(ThreadMonitorEvent args)
        {
            EventHandler<ThreadMonitorEvent> handler = ThradMonitorEventHandler;

            if (handler != null)
            {
                handler(this, args);
            }
        }

        public class ThreadMonitorEvent : EventArgs
        {
            private Thread _Thread;

            /// <summary>
            /// Thread which need be monitored
            /// </summary>
            public Thread Thread
            {
                get
                {
                    return _Thread;
                }
            }

            private string _Name;

            /// <summary>
            /// Thread name
            /// </summary>
            public string Name
            {
                get
                {
                    return _Name;
                }
            }

            private ThreadStatus _Status;

            public ThreadStatus Status
            {
                get
                {
                    return _Status;
                }
            }

            private string _StackTrace;

            public string StackTrace
            {
                get 
                { 
                    return _StackTrace; 
                }
            }
	

            public ThreadMonitorEvent(Thread t, string name, 
                ThreadStatus status, string stackTrace)
            {
                _Thread = t;
                _Name = name;
                _Status = status;
                _StackTrace = stackTrace;
            }
        }

        #endregion

        #region Private fields

        private Thread _MonitorThread;
        private Dictionary<int, TCB> _TCBTable = new Dictionary<int, TCB>();
        private object _RegisterLock = new object();

        #endregion

        #region Private methods

        private string GetThreadStackTrace(Thread t)
        {
            bool needFileInfo = NeedFileInfo;

            t.Suspend();
            StackTrace stack = new StackTrace(t, needFileInfo);
            t.Resume();

            return stack.ToString();
        }

        private TCB GetTCB(Thread t)
        {
            lock (_RegisterLock)
            {
                TCB tcb;
                if (_TCBTable.TryGetValue(t.ManagedThreadId, out tcb))
                {
                    return tcb;
                }
                else
                {
                    return null;
                }
            }
        }


        private void MonitorTask()
        {
            List<TCB> _ActionTCBList = new List<TCB>(4);

            int _DeadCycleTimes = 0;

            while (true)
            {
                try
                {
                    _ActionTCBList.Clear();

                    lock (_RegisterLock)
                    {
                        foreach (TCB tcb in _TCBTable.Values)
                        {
                            if (tcb.Para.Flag != MonitorFlag.None)
                            {
                                _ActionTCBList.Add(tcb);
                            }
                        }
                    }

                    foreach (TCB tcb in _ActionTCBList)
                    {
                        ThreadStatus status = tcb.Status;

                        //Monitor Hang
                        if ((tcb.Para.Flag & MonitorFlag.MonitorHang) != 0 &&
                            (status & ThreadStatus.Hang) == 0)
                        {
                            TimeSpan s = DateTime.Now - tcb.LastHeartbeat;

                            if (s.TotalMilliseconds > tcb.Para.HangTimeout)
                            {
                                status |= ThreadStatus.Hang;
                                tcb.Status = status;

                                Thread t = tcb.Para.Thread;
                                string name = tcb.Para.Name;

                                string stacktrace = GetThreadStackTrace(t);

                                OnThreadMonitorEvent(new ThreadMonitorEvent(t, name,
                                    status, stacktrace));
                            }
                        }

                        //Monitor dead cycle
                        if ((tcb.Para.Flag & MonitorFlag.MonitorDeadCycle) != 0 &&
                            (status & ThreadStatus.DeadCycle) == 0)
                        {
                            TimeSpan s = DateTime.Now - tcb.LastGotCpu;

                            switch (tcb.Para.Thread.ThreadState)
                            {
                                case System.Threading.ThreadState.Running:
                                case System.Threading.ThreadState.Background:
                                    tcb.HitTimes++;
                                    break;

                                default:
                                    tcb.HitTimes = 0;
                                    break;
                            }

                            if (s.TotalMilliseconds > tcb.Para.DeadCycleTimeout)
                            {
                                double lastTrheadTime = tcb.LastTotalProcessorTime;

                                double totalMilliseconds = GetTotalProcessorTime();

                                if ((totalMilliseconds - lastTrheadTime) > (s.TotalMilliseconds * 9 / 10))
                                {
                                    if (tcb.HitTimes > (int)s.TotalSeconds ||
                                        _DeadCycleTimes > tcb.Para.DeadCycleTimeout / 1000)
                                    {
                                        status |= ThreadStatus.DeadCycle;
                                        tcb.Status = status;

                                        Thread t = tcb.Para.Thread;
                                        string name = tcb.Para.Name;

                                        string stacktrace = GetThreadStackTrace(t);

                                        OnThreadMonitorEvent(new ThreadMonitorEvent(t, name,
                                            status, stacktrace));
                                    }
                                    else
                                    {
                                        tcb.Status &= ~ThreadStatus.DeadCycle;
                                        _DeadCycleTimes++;
                                    }
                                }
                                else
                                {
                                    _DeadCycleTimes = 0;
                                    tcb.Status &= ~ThreadStatus.DeadCycle;
                                }

                                tcb.LastTotalProcessorTime = totalMilliseconds;
                                tcb.LastGotCpu = DateTime.Now;
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog("Thread Monitor Fail.", e);
                }
                Thread.Sleep(1000);
            }
        }

        #endregion

        #region Public properties

        private bool _NeedFileInfo = false;

        /// <summary>
        /// Report include file info
        /// </summary>
        public bool NeedFileInfo
        {
            get 
            {
                lock (_RegisterLock)
                {
                    return _NeedFileInfo;
                }
            }

            set 
            {
                lock (_RegisterLock)
                {
                    _NeedFileInfo = value;
                }
            }
        }
	

        #endregion


        #region Public methods

        public ThreadMonitor()
        {
            _MonitorThread = new Thread(new ThreadStart(MonitorTask));
            _MonitorThread.Priority = ThreadPriority.Highest;
            _MonitorThread.IsBackground = true;

        }

        /// <summary>
        /// Start monitor
        /// </summary>
        public void Start()
        {
            _MonitorThread.Start();
        }

        public void UnRegister(System.Threading.Thread thread)
        {
            lock (_RegisterLock)
            {
                _TCBTable.Remove(thread.ManagedThreadId);
            }
        }

        /// <summary>
        /// Monitor register
        /// </summary>
        /// <param name="monitorPara">Monitor parameter</param>
        public void Register(MonitorParameter monitorPara)
        {
            Debug.Assert(monitorPara != null);
            Debug.Assert(monitorPara.Thread != null);

            //if (GetTCB(monitorPara.Thread) != null)
            //{
            //    throw new System.ArgumentException("Register repeatedly!");
            //}

            lock (_RegisterLock)
            {
                if (_TCBTable.ContainsKey(monitorPara.Thread.ManagedThreadId))
                {
                    _TCBTable.Remove(monitorPara.Thread.ManagedThreadId);
                }

                _TCBTable.Add(monitorPara.Thread.ManagedThreadId, new TCB(monitorPara));
            }
        }

        public void Heartbeat(Thread t)
        {
            TCB tcb = GetTCB(t);
            if (tcb == null)
            {
                throw new System.ArgumentException("This thread was not registered!");
            }

            tcb.LastHeartbeat = DateTime.Now;
            tcb.HitTimes = 0;
            tcb.Status &= ~ThreadStatus.Hang;
        }

        #endregion

    }
}
