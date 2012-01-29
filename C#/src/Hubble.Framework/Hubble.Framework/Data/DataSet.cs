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

namespace Hubble.Framework.Data
{
    public class DataSet
    {
        private List<DataTable> _Tables = new List<DataTable>();

        public List<DataTable> Tables
        {
            get
            {
                return _Tables;
            }

            set
            {
                _Tables = value;
            }
        }

        public DataSet()
        {
        }

        public DataSet(System.Data.DataSet ds)
        {
            foreach (System.Data.DataTable table in ds.Tables)
            {
                this.Tables.Add(new DataTable(table));
            }
        }

        public System.Data.DataSet ConvertToSystemDataSet()
        {
            System.Data.DataSet ds = new System.Data.DataSet();

            foreach (DataTable table in this.Tables)
            {
                ds.Tables.Add(table.ConvertToSystemDataTable());
            }

            return ds;
        }
    }
}
