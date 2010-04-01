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
using System.Data.OleDb;

namespace Hubble.Framework.Data
{
    public class OLEDataProvider : IDisposable
    {
        bool _Opened = false;

        OleDbConnection _OleDbConnection = null;

        public void Connect(string connectionString)
        {
            _OleDbConnection = new OleDbConnection(connectionString);
            _OleDbConnection.Open();
            _Opened = true;
        }

        public void Close()
        {
            if (_Opened)
            {
                _OleDbConnection.Close();
                _Opened = false;
            }
        }

        public int ExcuteSql(string sql)
        {
            OleDbCommand cmd = new OleDbCommand(sql, _OleDbConnection);
            return cmd.ExecuteNonQuery();
        }

        public DataSet QuerySql(string sql)
        {
            OleDbDataAdapter dadapter = new OleDbDataAdapter();

            dadapter.SelectCommand = new OleDbCommand(sql, _OleDbConnection);

            dadapter.AcceptChangesDuringFill = false;

            DataSet ds = new DataSet();
            dadapter.Fill(ds);

            return ds;
        }


        public DataSet GetSchema(string sql)
        {
            OleDbDataAdapter dadapter = new OleDbDataAdapter();

            dadapter.SelectCommand = new OleDbCommand(sql, _OleDbConnection);

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
