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
using System.Linq;

namespace Hubble.Core.StoredProcedure
{
    class SP_EnableTablet : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_EnableTablet";
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo("", Right.RightItem.CreateTable);

            if (Parameters.Count > 3)
            {
                throw new ArgumentException("the number of parameters must be 3. Parameter 1 is table name, parameter 2 is xml string of bigtable.");
            }

            string bigTableName = Parameters[0];

            string tabletName = Parameters[1];

            string dbName = null;

            if (Parameters.Count == 3)
            {
                dbName = Parameters[2];
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(bigTableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("BigTable name {0} does not exist!", bigTableName));
            }

            Hubble.Core.BigTable.BigTable bigTable = dbProvider.BigTableCfg;

            Hubble.Core.BigTable.TabletInfo tabletInfo =
                bigTable.Tablets.SingleOrDefault(s => s.TableName.Equals(tabletName, StringComparison.CurrentCultureIgnoreCase));

            if (tabletInfo == null)
            {
                throw new StoredProcException(
                    string.Format("Tablet:{0} of bigtable: {1} doesn't exist.", tabletName, bigTableName));
            }

            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in tabletInfo.BalanceServers)
            {
                if (dbName == null)
                {
                    serverInfo.Enabled = true;
                }
                else if (serverInfo.ServerName.Equals(dbName, StringComparison.CurrentCultureIgnoreCase))
                {
                    serverInfo.Enabled = true;
                }
            }

            foreach (Hubble.Core.BigTable.ServerInfo serverInfo in tabletInfo.FailoverServers)
            {
                if (dbName == null)
                {
                    serverInfo.Enabled = true;
                }
                else if (serverInfo.ServerName.Equals(dbName, StringComparison.CurrentCultureIgnoreCase))
                {
                    serverInfo.Enabled = true;
                }
            }

            bigTable.TimeStamp = DateTime.Now.ToUniversalTime();

            dbProvider.BigTableCfg = bigTable;

            dbProvider.Table.Save(dbProvider.Directory);

            OutputMessage(string.Format("Enable tablet:{0} of bigtable: {1} successul.", 
                tabletName, bigTableName));
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Enable tablet of big table.  Parameter 1 is big table name, parameter 2 is tablet name, parameter 3 is server name (optional)";
            }
        }

        #endregion
    }
}
