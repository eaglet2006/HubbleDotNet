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
    class SP_TableIndexOnly : StoredProcedure, IStoredProc, IHelper
    {
        void ShowValue(string tableName)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName)); 
            }

            AddColumn("TableName");
            AddColumn("IndexOnly");

            OutputValue("TableName", tableName);
            OutputValue("IndexOnly", dbProvider.IndexOnly.ToString());
        }

        void SetValue(string tableName, string value)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName));
            }

            bool indexonly;

            if (bool.TryParse(value, out indexonly))
            {
                dbProvider.SetIndexOnly(indexonly);
                dbProvider.SaveTable();
                OutputMessage(string.Format("Set table {0} index only to {1} sucessful!",
                    tableName, dbProvider.IndexOnly));
            }
            else
            {
                throw new StoredProcException("Parameter 2 must be 'True' or 'False'");
            }
        }

        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_TableIndexOnly";
            }
        }

        public void Run()
        {
            if (Parameters.Count == 1)
            {
                ShowValue(Parameters[0]);
            }
            else if (Parameters.Count == 2)
            {
                SetValue(Parameters[0], Parameters[1]);
            }
            else
            {
                throw new StoredProcException("First parameter is table name and second is 'True' or 'False'.");
            }

        }

        #endregion


        #region IHelper Members

        public string Help
        {
            get
            {
                return "Set or get table index only. First parameter is table name. Second parameter is 'True' or 'False' if set";
            }
        }

        #endregion
    }
}
