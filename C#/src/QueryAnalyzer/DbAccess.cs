using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hubble.Core.SFQL.Parse;
using Hubble.Core.Data;
using Hubble.SQLClient;

namespace QueryAnalyzer
{
    class DbAccess
    {
        SqlConnection _Conn = null;

        string _SettingPath = null;

        public string ServerName { get; set; }

        private string _DatabaseName = "Master";

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


        public void Connect(string serverName)
        {
            ServerName = serverName;

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
                sqlConnBuilder.InitialCatalog = DatabaseName;
                _Conn = new SqlConnection(sqlConnBuilder.ConnectionString);
                _Conn.Open();
            }
        }

        public QueryResult Excute(string sql)
        {
            return Excute(sql, null);
        }

        public QueryResult Excute(string sql, params object[] parameters)
        {
            if (_SettingPath != null)
            {
                string currentDirectory = Environment.CurrentDirectory;

                Environment.CurrentDirectory = _SettingPath;

                try
                {
                    using (Hubble.Core.Data.DBAccess dbAccess = new DBAccess())
                    {
                        return dbAccess.Query(SqlCommand.BuildSql(sql, parameters));
                    }
                }
                finally
                {
                    Environment.CurrentDirectory = currentDirectory;
                }
            }
            else
            {
                SqlCommand cmd = new SqlCommand(sql, _Conn, parameters);
                cmd.Query();
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

            Connect(ServerName);
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
