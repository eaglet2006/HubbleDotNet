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
using System.Diagnostics;

namespace Hubble.Core.Data
{
    [Serializable]
    public class FieldValue
    {
        #region Private

        private string _FieldName;
        private DataType _Type;
        private bool _FromDB;
        private int _DataLength;
        #endregion

        #region Public properties

        public string FieldName
        {
            get
            {
                return _FieldName;
            }
        }

        public DataType Type
        {
            get
            {
                return _Type;
            }

            set
            {
                _Type = value;
            }
        }


        public int DataLength
        {
            get
            {
                return _DataLength;
            }

            set
            {
                _DataLength = value;
            }
        }

        public string Value;

        public bool FromDB
        {
            get
            {
                return _FromDB;
            }
        }

        #endregion



        #region Constructor

        public FieldValue(string name, string value)
            : this(name, value, DataType.NVarchar, -1, false)
        {
        }

        public FieldValue(string name, string value, DataType type, int length, bool fromDB)
        {
            Debug.Assert(name != null);

            _FieldName = name;
            Value = value;
            _Type = type;
            _FromDB = fromDB;
            _DataLength = length;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return ((FieldValue)obj).FieldName.Equals(this.FieldName, StringComparison.CurrentCultureIgnoreCase) ;
        }

        public override int GetHashCode()
        {
            return this.FieldName.GetHashCode();
        }
    }

    [Serializable]
    public class Document
    {
        List<FieldValue> _FieldValues = new List<FieldValue>();

        private int _DocId = -1;

        public int DocId
        {
            get
            {
                return _DocId;
            }

            set
            {
                _DocId = value;
            }
        }

        public List<FieldValue> FieldValues
        {
            get
            {
                return _FieldValues;
            }
        }


        public void Add(string fieldName, string value, DataType type, int dataLength, bool fromDB)
        {
            _FieldValues.Add(new FieldValue(fieldName, value, type, dataLength, fromDB));
        }

        #region static methods

        public static Hubble.Framework.Data.DataSet ToDataSet(List<Field> schema, List<Document> docs)
        {
            Hubble.Framework.Data.DataTable dt = new Hubble.Framework.Data.DataTable();

            foreach(Field field in  schema)
            {
                Hubble.Framework.Data.DataColumn col = new Hubble.Framework.Data.DataColumn(field.Name,
                    DataTypeConvert.GetClrType(field.DataType));
                dt.Columns.Add(col);
            }

            foreach (Document doc in docs)
            {
                Hubble.Framework.Data.DataRow row = dt.NewRow();

                foreach (FieldValue fv in doc.FieldValues)
                {
                    if (fv.Value == null)
                    {
                        row[fv.FieldName] = System.DBNull.Value;
                    }
                    else
                    {
                        Type type = DataTypeConvert.GetClrType(fv.Type);

                        if (fv.Type == DataType.TinyInt)
                        {
                            bool bitValue;

                            //check the bit data type of database
                            if (bool.TryParse(fv.Value, out bitValue))
                            {
                                if (bitValue)
                                {
                                    row[fv.FieldName] = (byte)1;
                                }
                                else
                                {
                                    row[fv.FieldName] = (byte)0;
                                }

                                continue;
                            }
                        }

                        row[fv.FieldName] =
                            System.ComponentModel.TypeDescriptor.GetConverter(type).ConvertFrom(fv.Value);
                    }
                }

                dt.Rows.Add(row);
            }

            Hubble.Framework.Data.DataSet ds = new Hubble.Framework.Data.DataSet();
            ds.Tables.Add(dt);
            return ds;
        }

        #endregion

    }
}
