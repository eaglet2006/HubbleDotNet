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
    public class DataColumnCollection : IEnumerable<DataColumn>
    {
        private DataTable _Table;

        //Dictionary<string, DataColumn> _DictColumn = new Dictionary<string, DataColumn>(32);

        List<DataColumn> _Columns = new List<DataColumn>(32);

        public DataColumn this[int colIndex]
        {
            get
            {
                return _Columns[colIndex];
            }
        }

        public DataColumn this[string columnName]
        {
            get
            {
                DataColumn result = Find(columnName);

                return result;
            }
        }

        public int Count
        {
            get
            {
                return _Columns.Count;
            }
        }

        public DataColumn[] Columns
        {
            get
            {
                return _Columns.ToArray();
            }

            set
            {
                if (value == null)
                {
                    throw new System.Data.DataException("Columns can't be null");
                }

                _Columns = new List<DataColumn>(value);

                int id = 0;
                foreach (DataColumn col in _Columns)
                {
                    if (col == null)
                    {
                        throw new System.Data.DataException("DataColumn can't be null");
                    }

                    if (col.ColumnName == null)
                    {
                        throw new System.Data.DataException("ColumnName can't be null");
                    }

                    col.ColumnId = id++;
                }
            }
        }

        public DataColumnCollection(DataTable table)
        {
            _Table = table;
        }

        internal void CopyFrom(DataColumnCollection columns)
        {
            this._Columns = new List<DataColumn>(columns.Count);

            for (int i = 0; i < columns.Count; i++)
            {
                this._Columns.Add(columns[i]);
            }
        }

        public DataColumn Find(string columnName)
        {
            foreach (DataColumn col in _Columns)
            {
                if (col.ColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return col;
                }
            }

            return null;
        }

        public bool Contains(string columnName)
        {
            foreach (DataColumn col in _Columns)
            {
                if (col.ColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;

        }


        public void Add(DataColumn col)
        {
            if (col == null)
            {
                throw new System.Data.DataException("DataColumn can't be null");
            }

            if (col.ColumnName == null)
            {
                throw new System.Data.DataException("ColumnName can't be null");
            }

            col.ColumnId = _Columns.Count;

            _Columns.Add(col);

            if (_Table.Rows.Count > 0)
            {
                foreach (DataRow row in _Table.Rows)
                {
                    row.Append(col.DefaultValue);
                }
            }
            
        }

        public void Remove(DataColumn col)
        {
            if (col == null)
            {
                throw new System.Data.DataException("DataColumn can't be null");
            }

            if (col.ColumnName == null)
            {
                throw new System.Data.DataException("ColumnName can't be null");
            }

            int index = col.ColumnId;

            _Columns.Remove(col);

            if (_Table.Rows.Count > 0)
            {
                foreach (DataRow row in _Table.Rows)
                {
                    row.RemoveAt(index);
                }
            }
        }

        public void RemoveAt(int index)
        {
            DataColumn col = _Columns[index];
            Remove(col);
        }

        #region IEnumerable<DataColumn> Members

        public IEnumerator<DataColumn> GetEnumerator()
        {
            return _Columns.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Columns.GetEnumerator();
        }

        #endregion
    }

    public class DataColumn
    {
        string _ColumnName;

        public string ColumnName
        {
            get
            {
                return _ColumnName;
            }


            set
            {
                _ColumnName = value;
            }
        }

        Type _DataType;

        internal Type OrginalDataType
        {
            get
            {
                return _DataType;
            }
        }

        public Type DataType
        {
            get
            {
                if (_DataType == null)
                {
                    return typeof(string);
                }

                return _DataType;
            }

            set
            {
                _DataType = value;
            }
        }

        int _ColumnId;

        public int ColumnId
        {
            get
            {
                return _ColumnId;
            }

            set
            {
                _ColumnId = value;
            }
        }

        private object _DefaultValue = null;

        public object DefaultValue
        {
            get
            {
                return _DefaultValue;
            }

            set
            {
                _DefaultValue = value;
            }
        }

        public DataColumn()
            :this("", typeof(string))
        {
        }

        public DataColumn(string columnName)
            :this(columnName, typeof(string))
        {
        }

        public DataColumn(string columnName, Type dataType)
        {
            _DataType = dataType;
            _ColumnName = columnName;
        }

        public override bool Equals(object obj)
        {
            return this.ColumnName.Equals(((DataColumn)obj).ColumnName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.ColumnName.ToLower().GetHashCode();
        }

    }
}
