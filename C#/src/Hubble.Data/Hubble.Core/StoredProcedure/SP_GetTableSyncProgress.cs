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
    class SP_GetTableSyncProgress : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_GetTableSyncProgress";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 1)
            {
                throw new StoredProcException("First parameter is table name.");
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist!", Parameters[0]));
            }

            AddColumn("Progress");
            AddColumn("InsertRows");

            NewRow();

            OutputValue("Progress", dbProvider.TableSynchronizeProgress);
            OutputValue("InsertRows", dbProvider.TableSynchronizeInsertRows);
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Get synchronize progress of table. First parameter is table name.";
            }
        }

        #endregion
    }
}
