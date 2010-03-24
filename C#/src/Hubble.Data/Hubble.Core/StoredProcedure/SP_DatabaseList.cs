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
using Hubble.Core.Data;

namespace Hubble.Core.StoredProcedure
{
    class SP_DatabaseList : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_DatabaseList";
            }
        }

        public void Run()
        {
            AddColumn("DatabaseName");
            AddColumn("DefaultPath");
            AddColumn("DefaultDBAdapter");
            AddColumn("DefaultConnectionString");

            foreach (string databaseName in Global.Setting.DatabaseList)
            {
                NewRow();
                OutputValue("DatabaseName", databaseName);

                Global.Database database = Global.Setting.GetDatabase(databaseName);

                OutputValue("DefaultPath", database.DefaultPath);
                OutputValue("DefaultDBAdapter", database.DefaultDBAdapter);
                OutputValue("DefaultConnectionString", database.DefaultConnectionString);

            }
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get 
            {
                return "List all databases";
            }
        }

        #endregion
    }
}
