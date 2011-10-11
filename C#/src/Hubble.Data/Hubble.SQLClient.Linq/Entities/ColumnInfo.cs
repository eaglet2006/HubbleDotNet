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
    /// Column of the hubble table
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// Column name
        /// </summary>
        [Column("FieldName", true)]
        public string ColumnName { get; set; }

        /// <summary>
        /// Data type
        /// </summary>
        [Column("DataType")]
        public string DataType { get; set; }

        /// <summary>
        /// Data length
        /// </summary>
        [Column("DataLength")]
        public int DataLength { get; set; }

        /// <summary>
        /// Index type. 
        /// There are 3 index types now.
        /// Tokenized: full text index
        /// Untokenized : key-value index
        /// None: none index;
        /// </summary>
        [Column("IndexType")]
        public string IndexType { get; set; }

        /// <summary>
        /// Analyzer name
        /// This property is only avaliable when the Index Type is tokenized.
        /// </summary>
        [Column("Analyzer")]
        public string Analyzer { get; set; }

        /// <summary>
        /// Can NULL
        /// </summary>
        [Column("IsNull")]
        public bool CanNull { get; set; }

        /// <summary>
        /// Is Primary key
        /// </summary>
        [Column("IsPrimaryKey")]
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Default value
        /// </summary>
        [Column("Default")]
        public string Default { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ColumnInfo dest = (ColumnInfo)obj;

            if (this.ColumnName == null || dest.ColumnName == null)
            {
                return (this.ColumnName == null && dest.ColumnName == null);
            }

            return ColumnName.Equals(dest.ColumnName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            if (ColumnName == null)
            {
                return 0;
            }

            return ColumnName.ToLower().GetHashCode();
        }

        public override string ToString()
        {
            return ColumnName;
        }
    }
}
