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
    class SP_AddExternalReference : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_AddExternalReference";
            }
        }

        private void AddExternalReference(string refName, string assemblyFile)
        {
            refName = refName.ToLower().Trim();

            if (!System.IO.File.Exists(assemblyFile))
            {
                throw new StoredProcException(string.Format("assembly file : {0} does not exist!", assemblyFile));
            }

            
            switch (refName)
            {
                case "query":
                    Hubble.Core.Global.IQueryConfig iqueryConfig = new Hubble.Core.Global.IQueryConfig(assemblyFile);

                    if (Global.Setting.Config.IQuerys.Contains(iqueryConfig))
                    {
                        throw new StoredProcException(string.Format("assembly file : {0} is already configured!", assemblyFile));
                    }

                    Global.Setting.Config.IQuerys.Add(iqueryConfig);
                    Global.Setting.Save();
                    break;
                case "analyzer":
                    Hubble.Core.Global.IAnalyzerConfig ianalyzerConfig = new Hubble.Core.Global.IAnalyzerConfig(assemblyFile);

                    if (Global.Setting.Config.IAnalyzers.Contains(ianalyzerConfig))
                    {
                        throw new StoredProcException(string.Format("assembly file : {0} is already configured!", assemblyFile));
                    }

                    Global.Setting.Config.IAnalyzers.Add(ianalyzerConfig);
                    Global.Setting.Save();
                    break;

                case "dbadapter":
                    Hubble.Core.Global.IDBAdapterConfig idbadapterConfig = new Hubble.Core.Global.IDBAdapterConfig(assemblyFile);

                    if (Global.Setting.Config.IDBAdapters.Contains(idbadapterConfig))
                    {
                        throw new StoredProcException(string.Format("assembly file : {0} is already configured!", assemblyFile));
                    }

                    Global.Setting.Config.IDBAdapters.Add(idbadapterConfig);
                    Global.Setting.Save();
                    break;

                default:
                    throw new StoredProcException("refName must be 'Query', 'Analyzer' or 'DBAdapter");
            }
        }

        public void Run()
        {
            if (Parameters.Count != 2)
            {
                throw new StoredProcException("Parameter number must be 2. Parameter 1 is refer name and parameter 2 is assembly file path!");
            }

            AddExternalReference(Parameters[0], Parameters[1]);

            OutputMessage(string.Format("Add {0} on {1} successfully! Available after reboot hubble.net!", 
                Parameters[1], Parameters[0]));
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Add a extrenal reference. Parameter number must be 2. Parameter 1 is refer name and parameter 2 is assembly file path!";
            }
        }

        #endregion
    }
}
