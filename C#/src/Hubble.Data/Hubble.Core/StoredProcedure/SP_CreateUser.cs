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
using Hubble.Core.Right;
 

namespace Hubble.Core.StoredProcedure
{
    class SP_CreateUser : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_CreateUser";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 3)
            {
                throw new ArgumentException("Parameter 1 is user name. Parameter 2 is password, Parameter 3 is system user right");
            }

            string userName = Parameters[0].Trim();

            if (userName == "")
            {
                throw new UserRightException("User name can't be empty!");
            }

            int rightItem = int.Parse(Parameters[2]);

            Global.UserRightProvider.CreateUser(userName, Parameters[1], (RightItem)rightItem);

            OutputMessage(string.Format("Create user account: {0} successul.", userName));
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Add a user account. Parameter 1 is user name. Parameter 2 is password, Parameter 3 is system user right";
            }
        }

        #endregion
    }
}

