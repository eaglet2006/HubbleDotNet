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

namespace Hubble.SQLClient.Linq.Entities
{
    /// <summary>
    /// Entity for Database info
    /// </summary>
    public class DatabaseInfo
    {
        /// <summary>
        /// Get or set the database name
        /// </summary>
        [Column("DatabaseName", true)]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Get or set the Default path of the database
        /// </summary>
        [Column("DefaultPath")]
        public string DefaultPath { get; set; }

        /// <summary>
        /// Get or set the Default DBAdapter of the database
        /// </summary>
        [Column("DefaultDBAdapter")]
        public string DefaultDBAdapter { get; set; }

        /// <summary>
        /// Get or set the Default connection string of the database
        /// </summary>
        [Column("DefaultConnectionString")]
        public string DefaultConnectionString { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            DatabaseInfo dest = (DatabaseInfo)obj;

            if (this.DatabaseName == null || dest.DatabaseName == null)
            {
                return (this.DatabaseName == null && dest.DatabaseName == null);
            }

            return DatabaseName.Equals(dest.DatabaseName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            if (DatabaseName == null)
            {
                return 0;
            }

            return DatabaseName.ToLower().GetHashCode();
        }

        public override string ToString()
        {
            return DatabaseName;
        }
    }
}
