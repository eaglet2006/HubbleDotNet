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
    class SP_GetTableAttributes : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_GetTableAttributes";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 1)
            {
                throw new StoredProcException("First parameter is table name.");
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist!", Parameters[0]));
            }

            AddColumn("Attribute");
            AddColumn("Value");

            NewRow();
            OutputValue("Attribute", "Directory");
            OutputValue("Value", dbProvider.Directory);

            NewRow();
            OutputValue("Attribute", "IndexOnly");
            OutputValue("Value", dbProvider.IndexOnly.ToString());

            NewRow();
            OutputValue("Attribute", "DocId");
            OutputValue("Value", dbProvider.DocIdReplaceField);

            NewRow();
            OutputValue("Attribute", "DBTableName");
            OutputValue("Value", dbProvider.Table.DBTableName);

            NewRow();
            OutputValue("Attribute", "DBAdapter");
            OutputValue("Value", ((Data.INamedExternalReference)dbProvider.DBAdapter).Name);

            NewRow();
            OutputValue("Attribute", "LastDocId");
            OutputValue("Value", dbProvider.LastDocIdForIndexOnly.ToString());

            NewRow();
            OutputValue("Attribute", "MaxReturnCount");
            OutputValue("Value", dbProvider.MaxReturnCount.ToString());

            NewRow();
            OutputValue("Attribute", "GroupByLimit");
            OutputValue("Value", dbProvider.Table.GroupByLimit.ToString());

            NewRow();
            OutputValue("Attribute", "InitImmediatelyAfterStartup");
            OutputValue("Value", dbProvider.Table.InitImmediatelyAfterStartup.ToString());

            NewRow();
            OutputValue("Attribute", "QueryCacheEnabled");
            OutputValue("Value", dbProvider.QueryCacheEnabled.ToString());

            NewRow();
            OutputValue("Attribute", "QueryCacheTimeout");
            OutputValue("Value", dbProvider.QueryCacheTimeout.ToString());

            NewRow();
            OutputValue("Attribute", "StoreQueryCacheInFile");
            OutputValue("Value", dbProvider.Table.StoreQueryCacheInFile.ToString());

            NewRow();
            OutputValue("Attribute", "CleanupQueryCacheFileInDays");
            OutputValue("Value", dbProvider.Table.CleanupQueryCacheFileInDays.ToString());

            NewRow();
            OutputValue("Attribute", "IndexThread");
            OutputValue("Value", dbProvider.Table.IndexThread.ToString());

            NewRow();
            OutputValue("Attribute", "Debug");
            OutputValue("Value", dbProvider.Table.Debug.ToString());

            NewRow();
            OutputValue("Attribute", "TableSynchronization");
            OutputValue("Value", dbProvider.Table.TableSynchronization.ToString());

            NewRow();
            OutputValue("Attribute", "TriggerTableName");
            OutputValue("Value", dbProvider.Table.TriggerTableName);

            NewRow();
            OutputValue("Attribute", "MirrorTableEnabled");
            OutputValue("Value", dbProvider.Table.MirrorTableEnabled);


            NewRow();
            OutputValue("Attribute", "UsingMirrorTableForNonFulltextQuery");
            OutputValue("Value", dbProvider.Table.UsingMirrorTableForNonFulltextQuery);

        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Get table attribute. First parameter is table name.";
            }
        }

        #endregion
    }
}
