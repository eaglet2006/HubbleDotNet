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
using Hubble.Core.Right;

namespace Hubble.Core.StoredProcedure
{
    class SP_SetUserRight : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_SetUserRight";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 3)
            {
                throw new StoredProcException("First parameter is user name, second parameter is database name, third is right.");
            }

            string userName = Parameters[0].Trim();
            string databaseName = Parameters[1].Trim();
            int right = Int16.Parse(Parameters[2].Trim());

            Global.UserRightProvider.UpdateDBRight(databaseName, userName, (RightItem)right);
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get 
            {
                return "Set user's right for specifical database. First parameter is user name, second parameter is database name, third is right";
            }
        }

        #endregion
    }
}
