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
    class SP_DBAdapterList : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_DBAdapterList";
            }
        }

        public void Run()
        {
            AddColumn("Name");
            AddColumn("ClassName");
            AddColumn("Assembly");
            AddColumn("FileName");

            Global.IDBAdapterConfig[] adapters = Global.Setting.Config.IDBAdapters.ToArray();

            foreach (Type type in Data.DBProvider.GetDBAdapters())
            {
                NewRow();

                Data.INamedExternalReference nameRef = Hubble.Framework.Reflection.Instance.CreateInstance(type)
                    as Data.INamedExternalReference;

                OutputValue("Name", nameRef.Name);
                OutputValue("ClassName", type.FullName);
                OutputValue("Assembly", type.Assembly.FullName);
                OutputValue("FileName", type.Assembly.Location);
            }

        }

        #endregion


        #region IHelper Members

        public string Help
        {
            get
            {
                return "List all db adapters";
            }
        }

        #endregion
    }
}
