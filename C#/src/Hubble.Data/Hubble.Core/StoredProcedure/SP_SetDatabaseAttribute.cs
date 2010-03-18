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
    class SP_SetDatabaseAttribute : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_SetDatabaseAttribute";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 3)
            {
                throw new ArgumentException("Parameter 1 is Database name. Parameter 2 is Attribute name, Parameter 3 is Attribute value");
            }

            Global.Database database = Global.Setting.GetDatabase(Parameters[0]);

            if (database == null)
            {
                throw new StoredProcException(string.Format("Database name {0} does not exist!", Parameters[0]));
            }

            if (Parameters[1].Equals("DefaultPath", StringComparison.CurrentCultureIgnoreCase))
            {
                string dir = System.IO.Path.GetFullPath( Parameters[2]);

                dir = Hubble.Framework.IO.Path.AppendDivision(dir, '\\');

                dir = System.IO.Path.GetDirectoryName(dir);

                dir = Hubble.Framework.IO.Path.AppendDivision(dir, '\\');

                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                database.DefaultPath = dir;
            }
            else if (Parameters[1].Equals("DefaultDBAdapter", StringComparison.CurrentCultureIgnoreCase))
            {
                database.DefaultDBAdapter = Parameters[2];
            }
            else if (Parameters[1].Equals("DefaultConnectionString", StringComparison.CurrentCultureIgnoreCase))
            {
                database.DefaultConnectionString = Parameters[2];
            }

            Global.Setting.Save();

            OutputMessage(string.Format("Set database {0} attribute {1} to {2} successul.", 
                database.DatabaseName, Parameters[1], Parameters[2]));
        }

        #endregion
    }
}
