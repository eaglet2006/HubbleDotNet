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
using MySql.Data.MySqlClient;
//using MySQLDriverCS;

namespace MySqlDBAdapter
{
    public class MysqlDataProvider : IDisposable
    {
        bool _Opened = false;
        string _ConnectionString;

        MySqlConnection _MySqlConnection = null;

        unsafe public void Connect(string connectionString)
        {
            _ConnectionString = connectionString;
            _MySqlConnection = new MySqlConnection(connectionString);
            _MySqlConnection.Open();
            _Opened = true;
        }

        public void Close()
        {
            if (_Opened)
            {
                _MySqlConnection.Close();
                _Opened = false;
            }
        }

        public void BeginTransaction()
        {
            _MySqlConnection.BeginTransaction();
        }

        public System.Data.Common.DbDataReader ExecuteReader(string sql, out MySqlCommand cmd)
        {
            cmd = new MySqlCommand(sql, _MySqlConnection);
            return cmd.ExecuteReader();
        }


        public int ExcuteSql(string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, _MySqlConnection);
            return cmd.ExecuteNonQuery();
        }

        public DataSet QuerySql(string sql)
        {

            MySqlDataAdapter dadapter = new MySqlDataAdapter();

            MySqlCommand cmd = new MySqlCommand(sql, _MySqlConnection);

            dadapter.SelectCommand = cmd;

            dadapter.AcceptChangesDuringFill = false;

            DataSet ds = new DataSet();
            dadapter.Fill(ds);

            return ds;

        }


        public DataSet GetSchema(string sql)
        {
            MySqlDataAdapter dadapter = new MySqlDataAdapter();

            dadapter.SelectCommand = new MySqlCommand(sql, _MySqlConnection);

            DataSet ds = new DataSet();
            dadapter.FillSchema(ds, SchemaType.Mapped);

            return ds;
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch
            {
            }
        }

        #endregion
    }
}
