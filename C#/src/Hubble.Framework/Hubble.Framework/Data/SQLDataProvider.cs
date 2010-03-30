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
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Hubble.Framework.Data
{
    public class SQLDataProvider : IDisposable
    {
        SqlConnection _Conn;

        private string GetSqlValue(object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            if (value is DateTime)
            {
                DateTime d = (DateTime)value;

                return "'" + d.ToString("yyyy-MM-dd HH:mm:ss") + "." + d.Millisecond.ToString() + "'";
            }
            else if (value is string)
            {
                return "'" + value.ToString().Replace("'", "''") + "'";
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Connect dataBase
        /// </summary>
        /// <param name="connectString">Connect string</param>
        public void Connect(string connectString)
        {
            Close();
            if (!connectString.Contains("Connection Timeout"))
            {
                if (connectString.EndsWith(";"))
                {
                    connectString += "Connection Timeout=120;";
                }
                else
                {
                    connectString += ";Connection Timeout=120;";
                }
            }

            _Conn = new SqlConnection(connectString);
            _Conn.Open();
        }

        public int ExcuteSql(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql, _Conn);
            return cmd.ExecuteNonQuery();
        }

        public DataSet QuerySql(string sql)
        {
            SqlDataAdapter dadapter = new SqlDataAdapter();

            dadapter.SelectCommand = new SqlCommand(sql, _Conn);

            DataSet ds = new DataSet();
            dadapter.Fill(ds);

            return ds;
        }


        public DataSet GetSchema(string sql)
        {
            SqlDataAdapter dadapter = new SqlDataAdapter();

            dadapter.SelectCommand = new SqlCommand(sql, _Conn);

            DataSet ds = new DataSet();
            dadapter.FillSchema(ds, SchemaType.Mapped);

            return ds;
        }

        public void InsertEntity(string tableName, object entity)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("Insert {0} (", tableName);

            int i = 0;
            foreach (PropertyInfo pi in entity.GetType().GetProperties())
            {
                if (i++ == 0)
                {
                    sql.AppendFormat("{0}", pi.Name);
                }
                else
                {
                    sql.AppendFormat(",{0}", pi.Name);
                }
            }

            sql.Append(") values(");

            i = 0;
            foreach (PropertyInfo pi in entity.GetType().GetProperties())
            {
                if (i++ == 0)
                {
                    sql.AppendFormat("{0}", GetSqlValue(pi.GetValue(entity, null)));
                }
                else
                {
                    sql.AppendFormat(",{0}", GetSqlValue(pi.GetValue(entity, null)));
                }
            }

            sql.Append(")");

            ExcuteSql(sql.ToString());

        }

        /// <summary>
        /// Close database
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (_Conn != null)
                {
                    if (_Conn.State != ConnectionState.Closed &&
                        _Conn.State != ConnectionState.Broken)
                    {
                        _Conn.Close();
                    }

                    _Conn = null;
                }
            }
            catch
            {
            }
        }

        #endregion

    }
}
