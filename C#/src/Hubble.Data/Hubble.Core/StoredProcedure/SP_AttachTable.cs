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
    class SP_AttachTable : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_AttachTable";
            }
        }

        public void Run()
        {
            if (Parameters.Count < 1)
            {
                throw new ArgumentException("the number of parameters must be large then 0. Parameter 1 is directory");
            }

            string directory = null;
            string tableName = null;
            string connectString = null;
            string dbTableName = null;

            if (Parameters.Count >= 4)
            {
                dbTableName = Parameters[3];
            }

            if (Parameters.Count >= 3)
            {
                connectString = Parameters[2];
            }

            if (Parameters.Count >= 2)
            {
                tableName = Parameters[1];
            }

            if (Parameters.Count >= 1)
            {
                directory = Parameters[0];
            }


            Data.DBProvider dbProvider  = new Hubble.Core.Data.DBProvider();

            dbProvider.Attach(directory, tableName, connectString, dbTableName);

            OutputMessage(string.Format("Attach table {0} successul.", dbProvider.TableName));
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Attach table. Parameter 1 is directory Parameter 2 is table name Parameter 3 is connectString Parameter 4 is DBTableName. The last thress is optional";
            }
        }

        #endregion
    }
}
