using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Framework.Net;

namespace Hubble.SQLClient
{
    public class SqlConnection : IDisposable
    {
        TcpClient _TcpClient;

        System.Data.SqlClient.SqlConnectionStringBuilder _SqlConnBuilder;

        private string _ConnectionString;

        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
        }

        private string _DataSource;

        public string DataSource
        {
            get
            {
                return _DataSource;
            }
        }

        private int _TcpPort = 7523;

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

        /// <summary>
        /// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
        /// </summary>
        public int ConnectionTimeout
        {
            get
            {
                return _TcpClient.SendTimeout / 1000;
            }

            set
            {
                _TcpClient.SendTimeout = value * 1000;
                _TcpClient.ReceiveTimeout = value * 1000;
            }
        }

        private Hubble.Framework.Serialization.IMySerialization RequireCustomSerialization(Int16 evt, object data)
        {
            switch (evt)
            {
                case 1000:
                    return new QueryResultSerialization((QueryResult)data);
            }
            return null;
        }

        public SqlConnection(string connectionString)
        {
            _SqlConnBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);

            string[] strs = _SqlConnBuilder.DataSource.Split(new char[] { ':' });

            if (strs.Length > 1)
            {
                _TcpPort = int.Parse(strs[1]);
            }

            _DataSource = strs[0];

            _TcpClient = new TcpClient();

            System.Net.IPAddress[] addresslist = System.Net.Dns.GetHostAddresses(_DataSource);

            _TcpClient.RemoteAddress = addresslist[0];
            _TcpClient.Port = TcpPort;
            _TcpClient.RequireCustomSerialization = RequireCustomSerialization;

            _ConnectionString = connectionString;
            ConnectionTimeout = 15;

        }

        public void Open()
        {
            _TcpClient.Connect();
        }

        public void Close()
        {
            _TcpClient.Close();
        }



        public QueryResult QuerySql(string sql)
        {
            return _TcpClient.SendSyncMessage(1000, sql) as QueryResult;
        }

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
    }
}
