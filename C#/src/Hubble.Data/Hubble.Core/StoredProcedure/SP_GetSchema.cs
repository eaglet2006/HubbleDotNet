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
using Hubble.Core.Service;

namespace Hubble.Core.StoredProcedure
{
    class SP_GetSchema : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_GetSchema";
            }
        }

        public void Run()
        {
            AddColumn("SchemaId");
            AddColumn("Name");
            AddColumn("Schema");

            Global.UserRightProvider.CanDo(Right.RightItem.ManageDB);

            if (Parameters.Count != 1)
            {
                throw new ArgumentException("Parameter 1 is SchemaId. ");
            }

            int schemaId = int.Parse(Parameters[0]);

            Task task = ScheduleTaskMgr.ScheduleMgr.GetTask(schemaId);

            NewRow();

            OutputValue("SchemaId", task.SchemaId);
            OutputValue("Name", task.Schema.Name);
            OutputValue("Schema", task.Schema.ConvertToString());

        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Get task schema by SchemaId!";
            }
        }

        #endregion
    }
}
