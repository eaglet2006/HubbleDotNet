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
    class SP_QuerySql : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_QuerySql";
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo(Right.RightItem.QuerySql);

            if (Parameters.Count < 1)
            {
                throw new ArgumentException("the number of parameters must be 2. Parameter 1 is table name. Parameter 2 is sql for excute");
            }

            string sql;

            DBAdapter.IDBAdapter dbAdapter;

            if (Parameters.Count == 1)
            {
                string curDatabaseName = Service.CurrentConnection.ConnectionInfo.DatabaseName;
                Global.Database database = Global.Setting.GetDatabase(curDatabaseName);

                if (string.IsNullOrEmpty(database.DefaultDBAdapter))
                {
                    throw new StoredProcException("Current database hasn't default dbadapter");
                }

                if (string.IsNullOrEmpty(database.DefaultConnectionString))
                {
                    throw new StoredProcException("Current database hasn't default connectionstring");
                }

                dbAdapter = (DBAdapter.IDBAdapter)Hubble.Framework.Reflection.Instance.CreateInstance(
                    Data.DBProvider.GetDBAdapter(database.DefaultDBAdapter));

                if (dbAdapter == null)
                {
                    throw new StoredProcException(string.Format("Current database include a invalid default dbadapter:{0}",
                        database.DefaultDBAdapter));
                }

                dbAdapter.ConnectionString = database.DefaultConnectionString;

                sql = Parameters[0];
            }
            else
            {
                Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

                if (dbProvider == null)
                {
                    throw new StoredProcException(string.Format("Table name {0} does not exist!", Parameters[0]));
                }

                dbAdapter = dbProvider.DBAdapter;

                sql = Parameters[1];
            }


            _QueryResult.DataSet = dbAdapter.QuerySql(sql);

        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Query sql directly from database. Parameter 1 is table name or sql for query. Parameter 2 is sql for excute if Parameter 1 is table name";
            }
        }

        #endregion
    }
}
