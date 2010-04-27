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
using System.Data.OracleClient;

namespace OracleDBAdapter
{
    /// <summary>
    /// Oracle Data Provider
    /// </summary>
    /// <remarks>
    /// We have to install .NET Managed Provider for Oracle in order to be referenced by 
    /// System.Data.OracleCient;
    /// Download URL:
    /// http://www.microsoft.com/downloads/details.aspx?displaylang=en&FamilyID=4F55D429-17DC-45EA-BFB3-076D1C052524
    /// </remarks>
    public class OracleDataProvider : IDisposable
    {
        bool _Opened = false;
        string _ConnectionString;

        OracleConnection _OracleConnection = null;
        
        public void Connect(string connectionString)
        {
            _ConnectionString = connectionString;
            _OracleConnection = new OracleConnection(connectionString);
            _OracleConnection.Open();
            _Opened = true;
        }

        public void Close()
        {
            if (_Opened)
            {
                _OracleConnection.Close();
                _Opened = false;
            }
        }

        public void BeginTransaction()
        {
            _OracleConnection.BeginTransaction();
        }

        public OracleDataReader ExecuteReader(string sql, out OracleCommand cmd)
        {
            cmd = new OracleCommand(sql, _OracleConnection);
            return cmd.ExecuteReader();
        }

        public OracleLob CreateTempLob(OracleType lobtype)
        {
            //Oracle server syntax to obtain a temporary LOB.
            string sql =  "DECLARE A " + lobtype + "; " +
                           "BEGIN " +
                              "DBMS_LOB.CREATETEMPORARY(A, FALSE); " +
                              ":LOC := A; " +
                           "END;";

            OracleCommand cmd = new OracleCommand(sql, _OracleConnection);

            //Bind the LOB as an output parameter.
            OracleParameter p = cmd.Parameters.Add("LOC", lobtype);
            p.Direction = ParameterDirection.Output;

            //Execute (to receive the output temporary LOB).
            cmd.ExecuteNonQuery();

            //Return the temporary LOB.
            return (OracleLob)p.Value;
        }


        public int ExcuteSql(string sql)
        {
            OracleCommand cmd = new OracleCommand(sql, _OracleConnection);
            return cmd.ExecuteNonQuery();
        }

        public DataSet QuerySql(string sql)
        {

            OracleDataAdapter dadapter = new OracleDataAdapter();

            OracleCommand cmd = new OracleCommand(sql, _OracleConnection);

            dadapter.SelectCommand = cmd;

            dadapter.AcceptChangesDuringFill = false;

            DataSet ds = new DataSet();
            dadapter.Fill(ds);

            return ds;

        }


        public DataSet GetSchema(string sql)
        {
            OracleDataAdapter dadapter = new OracleDataAdapter();

            dadapter.SelectCommand = new OracleCommand(sql, _OracleConnection);

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
