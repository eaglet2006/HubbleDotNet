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
using System.Data.SQLite;

using Hubble.Framework.Data;
using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;
using Hubble.Core.DBAdapter;
using Data = Hubble.Core.Data;
using Query = Hubble.Core.Query;
using Core = Hubble.Core;

namespace SQLiteDBAdapter
{
    public class SQLiteDataProvider : IDisposable
    {
        int _CommandTimeOut = 300;
        SQLiteConnection _Conn;

        /// <summary>
        /// Connect dataBase
        /// </summary>
        /// <param name="connectString">Connect string</param>
        public void Connect(string connectionString)
        {
            Close();
            if (!connectionString.Contains("Connection Timeout") && !connectionString.Contains("Connect Timeout"))
            {
                if (connectionString.EndsWith(";"))
                    connectionString += "Connection Timeout=300;";
                else
                    connectionString += ";Connection Timeout=300;";
            }

            _Conn = new SQLiteConnection(connectionString);
            _CommandTimeOut = _Conn.ConnectionTimeout;
            _Conn.Open();
        }

        public int ExecuteNonQuery1(string sql)
        {
            SQLiteCommand cmd = new SQLiteCommand(sql, _Conn);
            SQLiteTransaction tran = _Conn.BeginTransaction();
            cmd.Transaction = tran;

            try
            {

                cmd.CommandTimeout = _CommandTimeOut;
                int result = cmd.ExecuteNonQuery();

                tran.Commit();

                return result;
            }
            catch (Exception e)
            {
                tran.Rollback();
                throw e;
            }

        }

        public int SaveDataTable(System.Data.DataTable dt)
        {
            SQLiteTransaction tran = null;

            try
            {
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                string sql = string.Format(@"select * from {0} where 1=2", dt.TableName);
                SQLiteCommand cmd = new SQLiteCommand(_Conn); ;
                SQLiteCommandBuilder comBuilder = new SQLiteCommandBuilder();

                comBuilder.DataAdapter = adapter;
                cmd.CommandText = sql;
                cmd.Connection = _Conn;
                adapter.SelectCommand = cmd;
                adapter.InsertCommand = comBuilder.GetInsertCommand();
                adapter.UpdateCommand = comBuilder.GetUpdateCommand();
                adapter.DeleteCommand = comBuilder.GetDeleteCommand();

                tran = _Conn.BeginTransaction();//transaction begin 传说中sqlite每执行一条insert语句都开启一个事务，死慢
                cmd.Transaction = tran;
                int result = adapter.Update(dt);
                tran.Commit();//transaction end
                return result;
            }
            catch (Exception ex)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }

                throw ex;
            }
        }

        public System.Data.Common.DbTransaction BeginTranscation()
        {
            return _Conn.BeginTransaction();
        }

        public void Commit(System.Data.Common.DbTransaction tran)
        {
            tran.Commit();
        }

        public void Rollback(System.Data.Common.DbTransaction tran)
        {
            tran.Rollback();
        }

        public System.Data.Common.DbCommand GetCommand(out System.Data.Common.DbTransaction tran)
        {
            SQLiteCommand cmd = new SQLiteCommand(_Conn);
            cmd.Transaction =  _Conn.BeginTransaction();
            cmd.CommandTimeout = _CommandTimeOut;
            tran = cmd.Transaction;
            return cmd;
        }

        public int ExecuteNonQuery(string sql, System.Data.Common.DbCommand cmd)
        {
            cmd.CommandText = sql;
            return cmd.ExecuteNonQuery();
        }

        public int ExcuteSql(string sql)
        {
            return ExecuteNonQuery(sql);
        }

        public int ExecuteNonQuery(string sql)
        {
            SQLiteCommand cmd = new SQLiteCommand(sql, _Conn);

            cmd.CommandTimeout = _CommandTimeOut;
            return cmd.ExecuteNonQuery();
        }


        public object ExecuteScalar(string sql)
        {
            SQLiteCommand cmd = new SQLiteCommand(sql, _Conn);
            cmd.CommandTimeout = _CommandTimeOut;
            return cmd.ExecuteScalar();
        }

        public System.Data.DataSet QuerySql(string sql)
        {
            return ExecuteReader(sql);
        }

        public System.Data.DataSet ExecuteReader(string sql)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            SQLiteDataAdapter dadapter = new SQLiteDataAdapter();

            dadapter.SelectCommand = new SQLiteCommand(sql, _Conn);

            dadapter.SelectCommand.CommandTimeout = _CommandTimeOut;
            dadapter.Fill(ds);

            foreach (System.Data.DataTable table in ds.Tables)
            {
                foreach (System.Data.DataColumn col in table.Columns)
                {
                    if (col.ColumnName.StartsWith("[") && col.ColumnName.EndsWith("]"))
                    {
                        col.ColumnName = col.ColumnName.Substring(1, col.ColumnName.Length - 2);
                    }
                }
            }

            return ds;
        }

        public System.Data.DataSet GetSchema(string sql)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            SQLiteDataAdapter dadapter = new SQLiteDataAdapter();
            dadapter.SelectCommand = new SQLiteCommand(sql, _Conn);
            dadapter.SelectCommand.CommandTimeout = _CommandTimeOut;

            dadapter.FillSchema(ds, SchemaType.Mapped);
            return ds;
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
            { }
        }
        #endregion
    }
}
