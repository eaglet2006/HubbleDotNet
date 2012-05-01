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

using Hubble.Core.Data;

namespace Hubble.Core.Service
{
    [Flags]
    public enum SyncFlags 
    {
        Insert = 0x01,
        Update = 0x02,
        Delete = 0x04,
        NoLock = 0x08,
        WaitForExit = 0x10,
        Rebuild= 0x20,
    }

    class TableSynchronize
    {
        const int MaxStep = 100000;

        Table _Table;
        DBProvider _DBProvider;

        Thread _Thread;

        int _Step;
        OptimizationOption _OptimizeOption;
        bool _FastestMode;
        SyncFlags _Flags;

        double _Progress = -1;
        int _InsertRows = 0;

        object _ProgressLock = new object();

        Exception _Exception = null;

        bool _Stopping = false;

        public bool Stopping
        {
            get
            {
                lock (this)
                {
                    return _Stopping;
                }
            }
        }

        public bool Synchronizing
        {
            get
            {
                return SyncThread != null;
            }
        }

        public Exception Exception
        {
            get
            {
                lock (_ProgressLock)
                {
                    return _Exception;
                }
            }
        }

        public double Progress
        {
            get
            {
                if (Exception != null)
                {
                    Exception e = this.Exception;
                    SetException(null);

                    throw e;
                }

                lock (_ProgressLock)
                {
                    return _Progress;
                }
            }
        }

        public int InsertRows
        {
            get
            {
                if (Exception != null)
                {
                    Exception e = this.Exception;
                    SetException(null);

                    throw e;
                }

                lock (_ProgressLock)
                {
                    return _InsertRows;
                }
            }
        }

        Thread SyncThread
        {
            get
            {
                lock (this)
                {
                    return _Thread;
                }
            }

            set
            {
                lock (this)
                {
                    _Thread = value;
                }
            }
        }

        internal void SetProgress(double progress)
        {
            SetProgress(progress, -1);
        }

        internal void SetProgress(double progress, int insertRows)
        {
            lock (_ProgressLock)
            {
                _Progress = progress;
                
                if (insertRows >= 0)
                {
                    _InsertRows = insertRows;
                }
            }
        }

        internal static string GetFieldName(string fieldName)
        {
            bool specialChar = false;

            foreach (char c in fieldName)
            {
                if (c < 128)
                {
                    if (!(c == '_' || (c >= '0' && c <= '9') ||
                        (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                    {
                        throw new System.Data.DataException(string.Format("Invalid character:{0} inf field name:{1}",
                            c, fieldName));
                    }
                }
                else
                {
                    specialChar = true;
                }
            }

            if (specialChar)
            {
                return "'" + fieldName + "'";
            }
            else
            {
                return fieldName;
            }
        }


        private void SetException(Exception ex)
        {
            lock (_ProgressLock)
            {
                _Exception = ex;
            }
        }

        private void DoSynchronizeAppendOnly()
        {
            SynchronizeAppendOnly syncAppendOnly = new SynchronizeAppendOnly(this, _DBProvider, _Step, 
                _OptimizeOption, _FastestMode, _Flags);
            syncAppendOnly.Do();
        }

        private void DoSynchronizeCanUpdate()
        {
            SynchronizeCanUpdate syncCanUpdate = new SynchronizeCanUpdate(this, _DBProvider, _Step, 
                _OptimizeOption, _FastestMode, _Flags);
            syncCanUpdate.Do();
        }

        private void DoSynchronize()
        {
            bool notIndexOnly = false;
            bool notTableSynchronization = false;

            try
            {
                if (!_Table.IndexOnly)
                {
                    notIndexOnly = true;
                    _Table.IndexOnly = true;
                }

                if (!_Table.TableSynchronization)
                {
                    notTableSynchronization = true;
                    _Table.TableSynchronization = true;
                }

                if (_Table.DocIdReplaceField != null)
                {
                    DoSynchronizeCanUpdate();
                }
                else
                {
                    DoSynchronizeAppendOnly();
                }

                SetProgress(100);

                SyncThread = null;
            }
            catch (Exception e)
            {
                SetProgress(100);
                Global.Report.WriteErrorLog("Table Synchronize fail", e);
                SetException(e);
                SyncThread = null;
            }
            finally
            {
                if (notIndexOnly)
                {
                    _Table.IndexOnly = false;
                }

                if (notTableSynchronization)
                {
                    _Table.TableSynchronization = false;
                }
            }
        }

        public TableSynchronize(DBProvider dbProvider)
        {
            _Table = dbProvider.Table;
            _DBProvider = dbProvider;
            _Thread = null;
        }

        /// <summary>
        /// Do synchronize
        /// </summary>
        /// <param name="step"></param>
        /// <param name="option">optimize option</param>
        /// <param name="fastestMode">if fastest mode. read data from db without log</param>
        /// <param name="flags">flags</param>
        public void Synchronize(int step, OptimizationOption option, bool fastestMode, SyncFlags flags)
        {
            if (SyncThread != null)
            {
                return;
            }

            if (!_Table.TableSynchronization && (flags & SyncFlags.Rebuild) == 0)
            {
                throw new DataException("Can't do synchronization. You must set the TableSynchronization to true!");
            }

            if (!_Table.IndexOnly && (flags & SyncFlags.Rebuild) == 0)
            {
                throw new DataException("Can't do synchronization in non-indexonly mode");
            }

            if (SyncThread != null)
            {
                Hubble.Core.Global.Report.WriteAppLog(string.Format("Table:{0} is synchronizing now.", _Table.Name));
                return;
            }

            SetException(null);

            _Step = step;

            if (_Step <= 0)
            {
                _Step = 1;
            }
            else if (_Step > MaxStep)
            {
                _Step = MaxStep;
            }

            _OptimizeOption = option;
            _FastestMode = fastestMode;
            _Flags = flags;

            SetProgress(0);
            SyncThread = new Thread(DoSynchronize);

            SyncThread.IsBackground = true;
            SyncThread.Start();
        }

        internal void Stop()
        {
            lock (this)
            {
                _Stopping = true;
            }
        }

        internal void ResetStopping()
        {
            lock (this)
            {
                _Stopping = false;
            }
        }

        internal void Close()
        {
            if (SyncThread == null)
            {
                return;
            }

            Stop();
            int times = 0;

            while (Progress >= 0 && Progress < 100)
            {
                System.Threading.Thread.Sleep(1000);

                times++;

                if (times > 600)
                {
                    SyncThread.Abort();
                    break;
                }
            }
        }
    }
}
