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
using System.Linq;
using System.Text;

using Hubble.SQLClient.Linq.Entities;

namespace Hubble.SQLClient.Linq.StoredProcedure
{
    public partial class SPDataContext : DataContext
    {
        /// <summary>
        /// Get word info that are analyzed by server.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="fieldName">field name must be tokenized index type</param>
        /// <param name="text">text</param>
        /// <returns> word info List</returns>
        public IList<WordInfo> SP_FieldAnalyze(string tableName,
            string fieldName, string text)
        {
            return SP_FieldAnalyze(tableName, fieldName, text, false);
                
        }

        /// <summary>
        /// Get word info that are analyzed by server.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="fieldName">field name must be tokenized index type</param>
        /// <param name="text">text</param>
        /// <param name="sqlclient">use Analzyer for SqlClient</param>
        /// <returns>word info List</returns>
        public IList<WordInfo> SP_FieldAnalyze(string tableName,
            string fieldName, string text, bool sqlclient)
        {
            SPExecutor<WordInfo> spExecutor = new SPExecutor<WordInfo>(this);

            if (sqlclient)
            {
                return spExecutor.ReturnAsList("exec SP_FieldAnalyze {0}, {1}, {2}", tableName,
                    fieldName, text, "SqlClient");
            }
            else
            {
                return spExecutor.ReturnAsList("exec SP_FieldAnalyze {0}, {1}, {2}", tableName,
                    fieldName, text);
            }
        }
    }
}
