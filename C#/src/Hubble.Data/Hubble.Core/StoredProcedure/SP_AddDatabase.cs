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
    class SP_AddDatabase : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_AddDatabase";
            }
        }

        public void Run()
        {
            if (Parameters.Count < 1)
            {
                throw new ArgumentException("Parameter 1 is Database name. Parameter 2 is DefaultPath, Parameter 3 is DefaultDBAdapter, Parameter 4 is DefaultConnectionString");
            }

            Global.Database database = new Hubble.Core.Global.Database();

            if (Parameters.Count > 3)
            {
                database.DefaultConnectionString = Parameters[3];
            }

            if (Parameters.Count > 2)
            {
                database.DefaultDBAdapter = Parameters[2];
            }

            if (Parameters.Count > 1)
            {
                string dir = System.IO.Path.GetFullPath(Parameters[1]);

                dir = Hubble.Framework.IO.Path.AppendDivision(dir, '\\');

                dir = System.IO.Path.GetDirectoryName(dir);

                dir = Hubble.Framework.IO.Path.AppendDivision(dir, '\\');

                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                database.DefaultPath = dir;
            }

            if (Parameters.Count > 0)
            {
                database.DatabaseName = Parameters[0].Trim();
            }

            Global.Setting.AddDatabase(database);
            Global.Setting.Save();

            OutputMessage(string.Format("Create database {0} successul.", database.DatabaseName));
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get 
            {
                return "Add a database. Parameter 1 is Database name. Parameter 2 is DefaultPath, Parameter 3 is DefaultDBAdapter, Parameter 4 is DefaultConnectionString";
            }
        }

        #endregion
    }
}
