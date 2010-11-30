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

using Hubble.Framework.Net;

namespace Hubble.SQLClient
{
    public enum ConnectEvent : short
    {
        Connect = 999,
        ExcuteSql = 1000,
        Exit = 1001,
    }

    public class HubbleConnection : System.Data.Common.DbConnection,IDisposable , ICloneable
    {
        static readonly byte[] _DefaultKey = {0x87, 0x45, 0xA0, 0xE8, 0x39, 0xC3, 0x99, 0x56 };

        TcpClient _TcpClient;

        System.Data.SqlClient.SqlConnectionStringBuilder _SqlConnBuilder;

        private string _ConnectionString;

        private string _DataSource;

        private string _Database;

        private string _UserId;

        private string _Password;

        private int _TcpPort = 7523;

        private int _ConnectionTimeout = 300;

        private System.Data.ConnectionState _State = System.Data.ConnectionState.Closed;

        private byte[] _DesKey = null;

        public byte[] DesKey
        {
            get
            {
                return _DesKey;
            }

            set
            {
                _DesKey = value;
            }
        
        }

        /// <summary>
        /// Tcp port of data source
        /// </summary>
        public int TcpPort
        {
            get
            {
                return _TcpPort;
            }
        }

        private bool _Connected = false;

        public bool Connected
        {
            get
            {
                if (_TcpClient == null)
                {
                    return false;
                }

                if (_TcpClient.Closed)
                {
                    return false;
                }

                return _Connected;
            }
        }

        public override string DataSource
        {
            get
            {
                return _DataSource;
            }
        }

        public override string Database
        {
            get
            {
                return _Database;
            }
        }

        public string UserId
        {
            get
            {
                return _UserId;
            }
        }

        public string Password
        {
            get
            {
                return _Password;
            }
        }


        /// <summary>
        /// The time (in seconds) to wait for a connection to open. The default value is 300 seconds.
        /// </summary>
        public override int ConnectionTimeout
        {
            get
            {
                return _ConnectionTimeout;
            }
        }

        private int _TryConnectTimeout = 0;

        /// <summary>
        /// in milliseconds.
        /// When TryConnectTimeout > 0, if connect time large then this setting
        /// try to find data cache of the sql, if on data cache, connect again else
        /// return the data of cache.
        /// </summary>
        public int TryConnectTimeout
        {
            get
            {
                return _TryConnectTimeout;
            }

            set
            {
                _TryConnectTimeout = value;
            }
        }

        private bool _ServerBusy = false;

        internal bool ServerBusy
        {
            get
            {
                return _ServerBusy;
            }
        }

        private bool _CommandReportNoCache = false;

        internal bool CommandReportNoCache
        {
            get
            {
                return _CommandReportNoCache;
            }

            set
            {
                _CommandReportNoCache = true;
            }
        }

        #region Private methods

        private Hubble.Framework.Serialization.IMySerialization RequireCustomSerialization(Int16 evt, object data)
        {
            switch ((ConnectEvent)evt)
            {
                case ConnectEvent.ExcuteSql:
                    return new QueryResultSerialization((QueryResult)data);
            }
            return null;
        }


        private string BuildConnectionMessage()
        {
            System.Data.SqlClient.SqlConnectionStringBuilder sqlConnBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            sqlConnBuilder.InitialCatalog = Database;

            byte[] key = _DefaultKey;
            if (_DesKey != null)
            {
                key = _DesKey;
            }

            sqlConnBuilder.UserID = Hubble.Framework.Security.DesEncryption.Encrypt(key, UserId);
            sqlConnBuilder.Password = Hubble.Framework.Security.DesEncryption.Encrypt(key, Password);

            return sqlConnBuilder.ConnectionString;
        }

        #endregion

        #region internal methods

        internal void SetState(System.Data.ConnectionState state)
        {
            _State = state;
        }

        /// <summary>
        /// The time (in seconds) to wait for a connection to open. The default value is determined by the specific type of connection that you are using.
        /// </summary>
        /// <param name="timeout"></param>
        internal void SetConnectionTimeout(int timeout)
        {
            _ConnectionTimeout = timeout;
            _TcpClient.SendTimeout = timeout * 1000;
            _TcpClient.ReceiveTimeout = timeout * 1000;
        }


        internal QueryResult QuerySql(string sql)
        {
            return _TcpClient.SendSyncMessage((short)ConnectEvent.ExcuteSql, sql) as QueryResult;
        }

        #endregion
        
        /// <summary>
        /// Initializes a new instance of the SqlConnection  class.
        /// </summary>
        public HubbleConnection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlConnection  class when given a string that contains the connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public HubbleConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        #region Override from DbConnection

        public override string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }

            set
            {
                if (Connected)
                {
                    Close();
                }

                string connectionString = value;

                _SqlConnBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);

                string[] strs = _SqlConnBuilder.DataSource.Split(new char[] { ':' });

                if (strs.Length > 1)
                {
                    _TcpPort = int.Parse(strs[1]);
                }

                _DataSource = strs[0];
                _Database = _SqlConnBuilder.InitialCatalog;
                _UserId = _SqlConnBuilder.UserID;

                if (_UserId == null)
                {
                    _UserId = "";
                }

                _Password = _SqlConnBuilder.Password;

                if (_Password == null)
                {
                    _Password = "";
                }

                _TcpClient = new TcpClient();

                System.Net.IPAddress[] addresslist = System.Net.Dns.GetHostAddresses(_DataSource);

                _TcpClient.RemoteAddress = addresslist[0];
                _TcpClient.Port = TcpPort;
                _TcpClient.RequireCustomSerialization = RequireCustomSerialization;

                _ConnectionString = connectionString;

                if (connectionString.IndexOf("Connection Timeout", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    SetConnectionTimeout(_SqlConnBuilder.ConnectTimeout);
                }
                else
                {
                    SetConnectionTimeout(300);
                }
                //ConnectionTimeout = 300;
            }
        }

        public override void Open()
        {
            int elapseConnectTime = 0;
            Random rand = new Random();
            _ServerBusy = false;

            while (elapseConnectTime < this.ConnectionTimeout * 1000)
            {
                _TcpClient.Connect();

                try
                {
                    _TcpClient.SendSyncMessage((short)ConnectEvent.Connect, BuildConnectionMessage());
                    break;
                }
                catch (Exception e)
                {
                    if (e.Message.Trim() == "Too many connects on server")
                    {
                        _TcpClient.Close();
                        _Connected = false;
                        ConnectionString = ConnectionString;

                        int sleepMillisecond = rand.Next(200, 800);
                        System.Threading.Thread.Sleep(sleepMillisecond);

                        if (!CommandReportNoCache && TryConnectTimeout > 0)
                        {
                            if (elapseConnectTime > TryConnectTimeout)
                            {
                                _ServerBusy = true;
                                return;
                            }
                        }

                        elapseConnectTime += sleepMillisecond;
                        continue;
                    }

                    _TcpClient.Close();

                    throw e;
                }
            }

            if (elapseConnectTime > this.ConnectionTimeout * 1000)
            {
                throw new System.Data.DataException("Connect timeout.The hubbledotnet server is too busy right now.");
            }

            _Connected = true;
            _State = System.Data.ConnectionState.Open;
            DataCacheMgr.OnConnect(this);
        }

        public override void Close()
        {
            _Connected = false;
            _State = System.Data.ConnectionState.Closed;
            _TcpClient.Close();
        }

        protected override System.Data.Common.DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public override void ChangeDatabase(string databaseName)
        {
            if (Connected)
            {
                Close();
            }

            _Database = databaseName;
            
            Open();
        }

        protected override System.Data.Common.DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }

        public override string ServerVersion
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public override System.Data.ConnectionState State
        {
            get 
            {
                return _State;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                _TcpClient.Dispose();
            }
            catch
            {
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new HubbleConnection(this.ConnectionString);
        }

        #endregion
                
    }
}
