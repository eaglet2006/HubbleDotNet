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
            : this(name, value, DataType.NVarchar, false)
        {
        }

        public FieldValue(string name, string value, DataType type, bool fromDB)
        {
            Debug.Assert(name != null);

            _FieldName = name;
            Value = value;
            _Type = type;
            _FromDB = fromDB;
        }

        #endregion
    }

    [Serializable]
    public class Document
    {
        List<FieldValue> _FieldValues = new List<FieldValue>();

        private long _DocId;

        public long DocId
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


        public void Add(string fieldName, string value, DataType type, bool fromDB)
        {
            _FieldValues.Add(new FieldValue(fieldName, value, type, fromDB));
        }

        #region static methods

        public static System.Data.DataSet ToDataSet(List<Field> schema, List<Document> docs)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            foreach(Field field in  schema)
            {
                System.Data.DataColumn col = new System.Data.DataColumn(field.Name,
                    DataTypeConvert.GetClrType(field.DataType));
                dt.Columns.Add(col);
            }

            foreach (Document doc in docs)
            {
                System.Data.DataRow row = dt.NewRow();

                foreach (FieldValue fv in doc.FieldValues)
                {
                    if (fv.Value == null)
                    {
                        row[fv.FieldName] = System.DBNull.Value;
                    }
                    else
                    {
                        Type type = DataTypeConvert.GetClrType(fv.Type);

                        row[fv.FieldName] =
                            System.ComponentModel.TypeDescriptor.GetConverter(type).ConvertFrom(fv.Value);
                    }
                }

                dt.Rows.Add(row);
            }

            System.Data.DataSet ds = new System.Data.DataSet();
            ds.Tables.Add(dt);
            return ds;
        }

        #endregion

    }
}
