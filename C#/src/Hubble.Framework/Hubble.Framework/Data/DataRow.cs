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
    public class DataRowCollection : IEnumerable<DataRow>
    {
        public DataRowCollection()
        {

        }

        private List<DataRow> _Rows = new List<DataRow>(128);

        public DataRow[] Rows
        {
            get
            {
                return _Rows.ToArray();
            }

            set
            {
                if (value == null)
                {
                    throw new System.Data.DataException("Rows can't be null");
                }

                _Rows = new List<DataRow>(value);
            }
        }

        public DataRow this[int rowIndex]
        {
            get
            {
                return _Rows[rowIndex];
            }
        }

        public object this[int rowIndex, int columnIndex]
        {
            get
            {
                return _Rows[rowIndex][columnIndex];
            }

            set
            {
                _Rows[rowIndex][columnIndex] = value;
            }
        }

        public object this[int rowIndex, string columnName]
        {
            get
            {
                return _Rows[rowIndex][columnName];
            }

            set
            {
                _Rows[rowIndex][columnName] = value;
            }
        }

        public int Count
        {
            get
            {
                return _Rows.Count;
            }
        }

        public void Add(DataRow row)
        {
            if (row == null)
            {
                throw new System.Data.DataException("DataRow can't be null");
            }

            _Rows.Add(row);
        }

        public void Remove(DataRow row)
        {
            if (row == null)
            {
                throw new System.Data.DataException("DataRow can't be null");
            }

            _Rows.Remove(row);
        }

        public void RemoveAt(int index)
        {
            _Rows.RemoveAt(index);
        }


        #region IEnumerable<DataRow> Members

        public IEnumerator<DataRow> GetEnumerator()
        {
            return _Rows.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Rows.GetEnumerator();
        }

        #endregion
    }


    public class DataRow
    {
        private DataColumnCollection _Columns;

        object[] _Values;

        public object[] Values
        {
            get
            {
                return _Values;
            }

            set
            {
                if (value == null)
                {
                    throw new System.Data.DataException("Values can't be null");
                }

                _Values = value;
            }
        }

        public object this[string columnName]
        {
            get
            {
                return _Values[_Columns[columnName].ColumnId];
            }

            set
            {
                if (value == null)
                {
                    _Values[_Columns[columnName].ColumnId] = System.DBNull.Value;
                }
                else
                {
                    if (_Columns[columnName].OrginalDataType == typeof(string))
                    {
                        _Values[_Columns[columnName].ColumnId] = value.ToString();
                    }
                    else
                    {
                        _Values[_Columns[columnName].ColumnId] = value;
                    }
                }
            }
        }

        public object this[int columnIndex]
        {
            get
            {
                return _Values[columnIndex];
            }

            set
            {
                if (value == null)
                {
                    _Values[columnIndex] = System.DBNull.Value;
                }
                else
                {
                    if (_Columns[columnIndex].OrginalDataType == typeof(string))
                    {
                        _Values[columnIndex] = value.ToString();
                    }
                    else
                    {
                        _Values[columnIndex] = value;
                    }
                }
            }
        }


        public DataRow()
        {
        }

        public DataRow(DataTable table, System.Data.DataRow row)
        {
            _Columns = table.Columns;
            _Values = new object[_Columns.Count];

            for(int i = 0; i < _Columns.Count; i++)
            {
                _Values[i] = row[i];
            }
        }

        internal DataRow(DataTable table)
        {
            _Columns = table.Columns;
            _Values = new object[_Columns.Count];
            for (int i = 0; i < _Values.Length; i++)
            {
                _Values[i] = System.DBNull.Value;
            }
        }

        internal void Append(object defaultValue)
        {
            object[] newValues = new object[_Values.Length + 1];

            Array.Copy(_Values, newValues, _Values.Length);

            _Values = newValues;

            _Values[_Values.Length - 1] = defaultValue;
        }


        internal void RemoveAt(int index)
        {
            List<object> temp = new List<object>(_Values);
            temp.RemoveAt(index);
            _Values = temp.ToArray();
        }
    }
}
