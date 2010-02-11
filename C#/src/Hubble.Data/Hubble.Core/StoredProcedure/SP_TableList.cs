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
    class SP_TableList : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_TableList";
            }
        }

        public void Run()
        {
            AddColumn("TableName");
            AddColumn("InitError");

            string databaseName = null;

            if (Parameters.Count > 0)
            {
                //First parameter is database name

                databaseName = Parameters[0] + ".";
            }

            foreach (string tableName in DBProvider.GetTables())
            {
                DBProvider dbProvider = DBProvider.GetDBProviderByFullName(tableName);

                if (dbProvider == null)
                {
                    continue;
                }


                if (databaseName != null)
                {
                    if (tableName.IndexOf(databaseName, 0, StringComparison.CurrentCultureIgnoreCase) != 0)
                    {
                        continue;
                    }
                }


                NewRow();
                OutputValue("TableName", tableName);
                OutputValue("InitError", dbProvider.InitError);
            }

        }

        #endregion
    }
}
