using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hubble.Core.SFQL.Parse;
using Hubble.Core.Data;

namespace QueryAnalyzer
{
    class DbAccess
    {
        string _SettingPath = null;

        public string ServerName { get; set; }

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
                throw new Exception("Remote server connection function has not finished!");
            }
        }

        public QueryResult Excute(string sql)
        {
            if (_SettingPath != null)
            {
                string currentDirectory = Environment.CurrentDirectory;

                Environment.CurrentDirectory = _SettingPath;

                try
                {
                    using (Hubble.Core.Data.DBAccess dbAccess = new DBAccess())
                    {
                        return dbAccess.Query(sql);
                    }
                }
                finally
                {
                    Environment.CurrentDirectory = currentDirectory;
                }
            }

            return null;
        }
    }
}
