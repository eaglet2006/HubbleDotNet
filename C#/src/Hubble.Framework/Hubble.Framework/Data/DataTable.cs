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
    public class DataTable
    {
        private DataColumnCollection _Columns;

        public DataColumnCollection Columns
        {
            get
            {
                return _Columns;
            }

            set
            {
                _Columns = value;
            }
        }

        private int _MinimumCapacity;

        public int MinimumCapacity
        {
            get
            {
                return _MinimumCapacity;
            }

            set
            {
                _MinimumCapacity = value;
            }
        }

        private string _TableName = "Table";

        public string TableName
        {
            get
            {
                return _TableName;
            }

            set
            {
                _TableName = value;
            }
        }

        private DataRowCollection _Rows = new DataRowCollection();

        public DataRowCollection Rows
        {
            get
            {
                return _Rows;
            }

            set
            {
                _Rows = value;
            }
        }

        public DataTable(string tableName)
        {
            _TableName = tableName;
            _Columns = new DataColumnCollection(this);
        }

        public DataTable()
        {
            _Columns = new DataColumnCollection(this);
        }

        public DataTable(System.Data.DataTable datatable)
        {
            this.TableName = datatable.TableName;
            this.MinimumCapacity = datatable.MinimumCapacity;

            _Columns = new DataColumnCollection(this);

            foreach (System.Data.DataColumn col in datatable.Columns)
            {
                this.Columns.Add(new DataColumn(col.ColumnName, col.DataType));
            }

            foreach (System.Data.DataRow row in datatable.Rows)
            {
                DataRow hRow = this.NewRow();

                for (int i = 0; i < this.Columns.Count; i++)
                {
                    hRow[i] = row[i];
                }

                this.Rows.Add(hRow);
            }

        }

        public System.Data.DataTable ConvertToSystemDataTable()
        {
            System.Data.DataTable result = new System.Data.DataTable(this.TableName);
            result.MinimumCapacity = this.MinimumCapacity;

            foreach (DataColumn col in this.Columns)
            {
                result.Columns.Add(new System.Data.DataColumn(col.ColumnName, col.DataType));
            }

            foreach (DataRow hRow in this.Rows)
            {
                System.Data.DataRow row = result.NewRow();

                for (int i = 0; i < this.Columns.Count; i++)
                {
                    row[i] = hRow[i];
                }

                result.Rows.Add(row);
            }

            return result;
        }

        public DataRow NewRow()
        {
            return new DataRow(this);
        }


        public DataRow[] Select(string filterExpression, string sort)
        {
            System.Data.DataTable table = this.ConvertToSystemDataTable();
            
            System.Data.DataRow[] rows = table.Select(filterExpression, sort);

            DataRow[] result = new DataRow[rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                result[i] = new DataRow(this, rows[i]);
            }

            return result;
        }

        public DataTable Clone()
        {
            DataTable result = new DataTable(this.TableName);

            result.Columns.CopyFrom(this.Columns);

            return result;
        }
    }
}
