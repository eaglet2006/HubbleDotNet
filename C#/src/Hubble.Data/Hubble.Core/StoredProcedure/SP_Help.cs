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
    class StoreProcedureComparer : IComparer<IStoredProc>
    {
        #region IComparer<IStoredProc> Members

        public int Compare(IStoredProc x, IStoredProc y)
        {
            return x.Name.CompareTo(y.Name);
        }

        #endregion
    }

    class SP_Help : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_Help";
            }
        }

        public void Run()
        {
            AddColumn("Name");
            AddColumn("Note");

            List<IStoredProc> spList = DBProvider.GetAllStoredProcs();

            spList.Sort(new StoreProcedureComparer());

            foreach (IStoredProc storedProc in spList)
            {
                IHelper helper = storedProc as IHelper;

                if (helper == null || storedProc == null)
                {
                    continue;
                }

                NewRow();
                OutputValue("Name", storedProc.Name);
                OutputValue("Note", helper.Help);
            }
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "List all store procedures";
            }
        }

        #endregion
    }
}
