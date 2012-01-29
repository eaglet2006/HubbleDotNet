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

using Hubble.Core.SFQL.Parse;
using Hubble.Core.Data;
using Hubble.SQLClient;

namespace QueryAnalyzer
{
    class DbAccess
    {
        HubbleConnection _Conn = null;

        string _SettingPath = null;

        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public bool AscyncConnection { get; set; }

        private string _DatabaseName = "Master";

        private int _CommandTimeout = -1;
        /// <summary>
        /// Set command timeout.
        /// In second
        /// </summary>
        /// <param name="timeout"></param>
        public void SetCommandTimeout(int timeout)
        {
            _CommandTimeout = timeout;
        }

        public bool IsNoneAuthentication
        {
            get
            {
                return string.IsNullOrEmpty(UserName);
            }
        }

        public HubbleConnection Conn
        {
            get
            {
                return _Conn;
            }
        }

        public string DatabaseName
        {
            get
            {
                return _DatabaseName;
            }

            set
            {
                _DatabaseName = value;
            }
        }


        private bool _ResetDataCacheAfterTimeout = false;

        public bool ResetDataCacheAfterTimeout
        {
            get
            {
                return _ResetDataCacheAfterTimeout;
            }

            set
            {
                _ResetDataCacheAfterTimeout = value;
            }
        }

        public void ReConnect()
        {
            Close();
            Connect(ServerName, UserName, Password);
        }

        public void Connect(string serverName)
        {
            Connect(serverName, "", "");
        }

        public void Connect(string serverName, string userName, string password)
        {
            ServerName = serverName;
            UserName = userName;
            Password = password;

            try
            {
                string settingFile = Hubble.Framework.IO.Path.AppendDivision(serverName, '\\') +
                    Hubble.Core.Global.Setting.FileName;

                if (!System.IO.File.Exists(settingFile))
                {
                    _SettingPath = null;
                }
                else
                {
                    _SettingPath = System.IO.Path.GetDirectoryName(settingFile);
                }
            }
            catch
            {
                _SettingPath = null;
            }

            if (_SettingPath != null)
            {
                Hubble.Core.Global.Setting.SettingPath = _SettingPath;
                Hubble.Core.Service.CurrentConnection.Connect();
                Hubble.Core.Service.CurrentConnection curConnection = new Hubble.Core.Service.CurrentConnection(
                    new Hubble.Core.Service.ConnectionInformation(DatabaseName));
                    
                string currentDirectory = Environment.CurrentDirectory;

                Environment.CurrentDirectory = _SettingPath;

                try
                {
                    DBProvider.Init(_SettingPath);
                }
                finally
                {
                    Environment.CurrentDirectory = currentDirectory;
                }
            }
            else
            {
                System.Data.SqlClient.SqlConnectionStringBuilder sqlConnBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
                sqlConnBuilder.DataSource = serverName;
                sqlConnBuilder.UserID = userName;
                sqlConnBuilder.Password = password;
                sqlConnBuilder.InitialCatalog = DatabaseName;
                if (AscyncConnection)
                {
                    _Conn = new HubbleAsyncConnection(sqlConnBuilder.ConnectionString);
                }
                else
                {
                    _Conn = new HubbleConnection(sqlConnBuilder.ConnectionString);
                }

                _Conn.TryConnectTimeout = 0;
                _Conn.Open();
            }
        }

        public QueryResult Excute(string sql)
        {
            return Excute(sql, null);
        }

        public QueryResult Excute(string sql, int cacheTimeout)
        {
            return Excute(sql, cacheTimeout, null);
        }

        public QueryResult Excute(string sql, params object[] parameters)
        {
            return Excute(sql, -1, parameters);
        }

        public QueryResult Excute(string sql, int cacheTimeout, params object[] parameters)
        {
            if (_SettingPath != null)
            {
                string currentDirectory = Environment.CurrentDirectory;

                Environment.CurrentDirectory = _SettingPath;

                try
                {
                    using (Hubble.Core.Data.DBAccess dbAccess = new DBAccess())
                    {
                        return dbAccess.Query(HubbleCommand.BuildSql(sql, parameters));
                    }
                }
                finally
                {
                    Environment.CurrentDirectory = currentDirectory;
                }
            }
            else
            {
                HubbleCommand cmd = new HubbleCommand(sql, _Conn, parameters);

                if (_CommandTimeout > 0)
                {
                    cmd.CommandTimeout = _CommandTimeout;
                }

                cmd.ResetDataCacheAfterTimeout = this.ResetDataCacheAfterTimeout;
                cmd.ExecuteQuery(cacheTimeout);
                return cmd.Result;
            }
        }

        public void ChangeDatabase(string databaseName)
        {
            if (databaseName.Equals(DatabaseName, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            Close();

            _DatabaseName = databaseName;

            Connect(ServerName, UserName, Password);
        }

        public void Close()
        {
            if (_SettingPath != null)
            {
                Hubble.Core.Service.CurrentConnection.Disconnect();
            }
            else
            {
                if (_Conn != null)
                {
                    _Conn.Close();
                }
            }
        }
    }
}
