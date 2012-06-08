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
    class SP_Rebuild : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_Rebuild";
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo(Right.RightItem.ManageDB);

            if (Parameters.Count < 1)
            {
                throw new ArgumentException("the number of parameters must large than 0. Parameter 1 is table name, Parameter 2 is step, Parameter 3 is optimize mode");
            }

            string tableName = Parameters[0];
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist!", Parameters[0]));
            }

            if (dbProvider.TableSynchronizeProgress >= 0 &&
                dbProvider.TableSynchronizeProgress < 100)
            {
                return;
            }


            int option = 0;
            int step = 5000;
            bool fastestMode = true;
            string strFlags = "";
            Service.SyncFlags flags = Service.SyncFlags.Insert | Service.SyncFlags.Rebuild;

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
                strFlags = Parameters[3];

                flags = (Service.SyncFlags)Hubble.Framework.Reflection.Emun.FromString(typeof(Service.SyncFlags), strFlags);

                flags &= ~Hubble.Core.Service.SyncFlags.Delete;
                flags &= ~Hubble.Core.Service.SyncFlags.Update;
            }

            Hubble.Core.Data.OptimizationOption optimizeOption;

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

            bool notIndexOnly = false;

            try
            {
                if (!dbProvider.IndexOnly)
                {
                    notIndexOnly = true;
                    dbProvider.Table.IndexOnly = true;
                }

                Data.DBProvider.Truncate(tableName);

                if (notIndexOnly)
                {
                    dbProvider = Data.DBProvider.GetDBProvider(tableName);
                    dbProvider.Table.IndexOnly = false;
                }
            }
            catch
            {
                if (notIndexOnly)
                {
                    dbProvider = Data.DBProvider.GetDBProvider(tableName);
                    dbProvider.Table.IndexOnly = false;
                }
            }

            dbProvider = Data.DBProvider.GetDBProvider(tableName);

            dbProvider.SynchronizeWithDatabase(step, optimizeOption, fastestMode, flags);

            if ((flags & Hubble.Core.Service.SyncFlags.WaitForExit) != 0)
            {
                //Wait for exit

                while (true)
                {
                    if (dbProvider.TableSynchronizeProgress >= 100 ||
                        dbProvider.TableSynchronizeProgress < 0)
                    {
                        OutputMessage(string.Format("Table: {0} finished synchronizing!", Parameters[0]));
                        return;
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
                return "rebuild the table. Parameter 1 is table name, Parameter 2 is step, Parameter 3 is merge mode, Parameter 4 flag.";
            }
        }

        #endregion
    }
}
