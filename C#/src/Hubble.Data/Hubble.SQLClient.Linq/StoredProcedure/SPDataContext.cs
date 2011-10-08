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
using System.Data;
using System.Reflection;
using Hubble.SQLClient;

namespace Hubble.SQLClient.Linq.StoredProcedure
{
    public partial class SPDataContext : DataContext
    {
        protected class SPExecutor<T>
        {
            DataContext _Context;

            public SPExecutor(DataContext context)
            {
                _Context = context;
            }

            private DataTable InnerExecute(string sqlFormat, object[] args)
            {
                using (HubbleCommand command = new HubbleCommand(sqlFormat, _Context.HBConnection,
                    args))
                {
                    return command.Query().Tables[0];
                }
            }


            private T GetEntity(DataRow row)
            {
                string pk;

                return GetEntity(row, out pk);
            }

            /// <summary>
            /// Get entity
            /// </summary>
            /// <param name="row">input row</param>
            /// <param name="pk">if has pk, output pk</param>
            /// <returns>entity instance</returns>
            private T GetEntity(DataRow row, out string pk)
            {
                Type tType = typeof(T);

                pk = null;

                T result = (T)Hubble.Framework.Reflection.Instance.CreateInstance(tType);

                foreach (PropertyInfo pi in tType.GetProperties())
                {
                    object[] attrs = pi.GetCustomAttributes(typeof(ColumnAttribute), false);

                    if (attrs.Length > 0)
                    {
                        Type pType = pi.PropertyType;

                        ColumnAttribute columnAttr = ((ColumnAttribute)attrs[0]);

                        string columnName = columnAttr.ColumnName;

                        object value = row[columnName];

                        if (value == DBNull.Value)
                        {
                            if (pType.IsClass)
                            {
                                pi.SetValue(result, null, null);
                            }
                            else
                            {
                                throw new NullValueException(string.Format("ColumnName={0} can't write to non-nullable type", columnName));
                            }
                        }

                        string strValue = value.ToString();

                        object setValue = Hubble.Framework.TypeConverter.FromString(
                            pType, strValue);

                        if (columnAttr.PrimaryKey)
                        {
                            pk = strValue;
                        }

                        pi.SetValue(result, setValue, null);
                    }

                }

                return result;
            }

            public IList<T> ReturnAsList(string sqlFormat, params object[] args)
            {
                List<T> result = new List<T>();

                foreach (DataRow row in InnerExecute(sqlFormat, args).Rows)
                {
                    result.Add(GetEntity(row));
                }

                return result;
            }

            public HashSet<T> ReturnAsHashSet(string sqlFormat, params object[] args)
            {
                HashSet<T> result = new HashSet<T>();

                foreach (DataRow row in InnerExecute(sqlFormat, args).Rows)
                {
                    result.Add(GetEntity(row));
                }

                return result;
            }

        }



        public SPDataContext(HubbleConnection conn)
            :base(conn)
        {


        }
    }
}
