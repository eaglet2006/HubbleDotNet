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
    class SP_SetBigTable : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_SetBigTable";
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo("", Right.RightItem.CreateTable);

            if (Parameters.Count != 2)
            {
                throw new ArgumentException("the number of parameters must be 3. Parameter 1 is table name, parameter 2 is xml string of bigtable.");
            }

            string tableName = Parameters[0];

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist!", tableName));
            }

            System.IO.Stream stream = Hubble.Framework.IO.Stream.WriteStringToStream(Parameters[1], Encoding.UTF8);

            BigTable.BigTable bigtable = Hubble.Framework.Serialization.XmlSerialization<BigTable.BigTable>.Deserialize(stream);
            bigtable.TimeStamp = DateTime.Now.ToUniversalTime();
            dbProvider.BigTableCfg = bigtable;

            dbProvider.Table.Save(dbProvider.Directory);

            OutputMessage(string.Format("Set bigtable {0} successul.", tableName));
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Set bigtable.  Parameter 1 is table name, parameter 2 is xml string of bigtable.";
            }
        }

        #endregion
    }
}
