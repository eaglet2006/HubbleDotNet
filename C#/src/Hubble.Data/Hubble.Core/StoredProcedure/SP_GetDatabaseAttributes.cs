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
    class SP_GetDatabaseAttributes : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_GetDatabaseAttributes";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 1)
            {
                throw new StoredProcException("First parameter is database name.");
            }

            Global.Database database = Global.Setting.GetDatabase(Parameters[0]);

            if (database == null)
            {
                throw new StoredProcException(string.Format("Database name {0} does not exist!", Parameters[0]));
            }

            AddColumn("Attribute");
            AddColumn("Value");

            NewRow();
            OutputValue("Attribute", "DefaultPath");
            OutputValue("Value", database.DefaultPath);

            NewRow();
            OutputValue("Attribute", "DefaultDBAdapter");
            OutputValue("Value", database.DefaultDBAdapter);

            NewRow();
            OutputValue("Attribute", "DefaultConnectionString");
            OutputValue("Value", database.DefaultConnectionString);
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get 
            {
                return "Get attributes of the database. First parameter is database name.";
            }
        }

        #endregion
    }
}
