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
    /// Talbe info for hubble table
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// Table name
        /// </summary>
        [Column("TableName", true)]
        public string TableName { get; set; }

        /// <summary>
        /// Error when initial. If this property is not empty
        /// The table maybe does not initial successfully.
        /// </summary>
        [Column("InitError")]
        public string InitError { get; set; }

        /// <summary>
        /// Is this table bigtable
        /// </summary>
        [Column("IsBigTable")]
        public bool? IsBigTable { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            TableInfo dest = (TableInfo)obj;

            if (this.TableName == null || dest.TableName == null)
            {
                return (this.TableName == null && dest.TableName == null);
            }

            return TableName.Equals(dest.TableName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            if (TableName == null)
            {
                return 0;
            }

            return TableName.ToLower().GetHashCode();
        }

        public override string ToString()
        {
            return TableName;
        }
    }
}
