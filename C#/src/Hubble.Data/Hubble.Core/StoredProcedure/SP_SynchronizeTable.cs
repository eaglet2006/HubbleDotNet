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

namespace Hubble.Core.StoredProcedure
{
    class SP_SynchronizeTable : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get 
            {
                return "SP_SynchronizeTable"; 
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo(Right.RightItem.ManageDB);

            if (Parameters.Count < 1)
            {
                throw new ArgumentException("the number of parameters must large than 0. Parameter 1 is table name, Parameter 2 is step, Parameter 3 is optimize mode");
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist!", Parameters[0]));
            }

            int option = 0;
            int step = 5000;
            bool fastestMode = false;
            string strFlags = "";
            Service.SyncFlags flags = Service.SyncFlags.Insert | Service.SyncFlags.Delete 
                | Service.SyncFlags.Update;

            if (Parameters.Count > 1)
            {
                step = int.Parse(Parameters[1]);
            }

            if (Parameters.Count > 2)
            {
                option = int.Parse(Parameters[2]);
            }

            if (Parameters.Count > 3)
            {
                fastestMode = bool.Parse(Parameters[3]);
            }

            if (Parameters.Count > 4)
            {
                strFlags = Parameters[4];

                flags = (Service.SyncFlags) Hubble.Framework.Reflection.Emun.FromString(typeof(Service.SyncFlags), strFlags);

            }

            Hubble.Core.Data.OptimizationOption optimizeOption ;

            switch (option)
            {
                case 0:
                    optimizeOption = Hubble.Core.Data.OptimizationOption.Idle;
                    break;
                case 1:
                    optimizeOption = Hubble.Core.Data.OptimizationOption.Minimum;
                    break;
                case 2:
                    optimizeOption = Hubble.Core.Data.OptimizationOption.Middle;
                    break;
                case 3:
                    optimizeOption = Hubble.Core.Data.OptimizationOption.Speedy;
                    break;
                default:
                    optimizeOption = Hubble.Core.Data.OptimizationOption.Minimum;
                    break;
            }

            dbProvider.SynchronizeWithDatabase(step, optimizeOption, fastestMode, flags);

            if ((flags & Hubble.Core.Service.SyncFlags.WaitForExit) != 0)
            {
                //Wait for exit
                int count = 0;

                while (true)
                {
                    if (dbProvider.TableSynchronizeProgress >= 100 ||
                        dbProvider.TableSynchronizeProgress < 0)
                    {
                        OutputMessage(string.Format("Table: {0} finished synchronizing!", Parameters[0]));
                        return;
                    }

                    if ((++count % 60) == 0)
                    {
                        Hubble.Core.Global.Report.WriteAppLog(string.Format("TableSynchronizeProgress = {0}% of table:{1}",
                            dbProvider.TableSynchronizeProgress, dbProvider.TableName));
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            }

            OutputMessage(string.Format("Table: {0} is synchronizing now!", Parameters[0]));
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Synchronize from database. Parameter 1 is table name, Parameter 1 is table name, Parameter 2 is step, Parameter 3 is merge mode, Parameter 4 is fastest mode, true or false.";
            }
        }

        #endregion
    }
}
