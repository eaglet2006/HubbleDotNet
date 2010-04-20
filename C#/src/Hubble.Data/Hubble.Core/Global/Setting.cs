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
using Hubble.Framework.IO;
using Hubble.Framework.Serialization;

namespace Hubble.Core.Global
{
    /// <summary>
    /// Global setting
    /// </summary>
    [Serializable, System.Xml.Serialization.XmlRoot(Namespace = "http://hubbledotnet.codeplex.com")] 
    public class Setting
    {
        #region static members

        static private object _LockObj = new object();

        static private Setting _Config = null;

        static private string _Path = Path.ProcessDirectory;

        [System.Xml.Serialization.XmlIgnore]
        static public string SettingPath
        {
            get
            {
                lock (_LockObj)
                {
                    return _Path;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _Path = value;
                }
            }
        }

        static internal Setting Config
        {
            get
            {
                lock (_LockObj)
                {
                    if (_Config == null)
                    {
                        Load(_Path);
                    }
                }

                return _Config;
            }

            set
            {
                _Config = value;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public const string FileName = "setting.xml";

        static internal void Save()
        {
            string fileName = Path.AppendDivision(SettingPath, '\\') + FileName;

            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create,
                 System.IO.FileAccess.ReadWrite))
            {
                XmlSerialization<Setting>.Serialize(Config, Encoding.UTF8, fs);
            }
        }

        internal static string GetTableFullName(string tableName)
        {
            string curDatabaseName = Service.CurrentConnection.ConnectionInfo.DatabaseName;

            return GetTableFullName(curDatabaseName, tableName);
        }


        internal static string GetTableFullName(string databaseName, string tableName)
        {
            tableName = tableName.Trim();

            string curDatabaseName = databaseName;

            string prefix = curDatabaseName + ".";

            if (tableName.IndexOf(prefix, 0, StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                //already full name
                return tableName;
            }
            else
            {
                return prefix + tableName;
            }
        }


        static internal bool DatabaseExists(string databaseName)
        {
            Setting cfg = Config;

            lock (_LockObj)
            {
                foreach (Database db in cfg.Databases)
                {
                    if (databaseName.Equals(db.DatabaseName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        static internal List<string> DatabaseList
        {
            get
            {
                Setting cfg = Config;

                List<string> result = new List<string>();

                lock (_LockObj)
                {
                    foreach (Database db in cfg.Databases)
                    {
                        result.Add(db.DatabaseName);
                    }

                    return result;
                }
            }
        }

        static internal Database GetDatabase(string databaseName)
        {
            Setting cfg = Config;

            lock (_LockObj)
            {
                foreach (Database db in cfg.Databases)
                {
                    if (databaseName.Equals(db.DatabaseName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return db;
                    }
                }

                return null;
            }
        }

        static internal void UpdateDatabase(Database database)
        {
            Setting cfg = Config;

            Database dest = GetDatabase(database.DatabaseName);

            if (dest == null)
            {
                throw new Data.DataException(string.Format("Database {0} does not exist!",
                    database.DatabaseName));
            }

            lock (_LockObj)
            {
                if (database.DefaultPath != null)
                {
                    dest.DefaultPath = database.DefaultPath;
                }

                if (database.DefaultConnectionString != null)
                {
                    dest.DefaultConnectionString = database.DefaultConnectionString;
                }

                if (database.DefaultDBAdapter != null)
                {
                    dest.DefaultDBAdapter = database.DefaultDBAdapter;
                }

                if (database.DefaultConnectionString != null)
                {
                    dest.DefaultConnectionString = database.DefaultConnectionString;
                }
            }
        }

        static internal void AddDatabase(Database database)
        {
            Setting cfg = Config;

            lock (_LockObj)
            {
                if (cfg.Databases.Contains(database))
                {
                    throw new Data.DataException(string.Format("Database {0} exist!",
                        database.DatabaseName));
                }

                cfg.Databases.Add(database);
            }
        }

        static internal void RemoveDatabase(string databaseName)
        {
            Setting cfg = Config;

            Database database = GetDatabase(databaseName);

            if (database == null)
            {
                return;
            }

            lock (_LockObj)
            {
                if (database.Tables.Count > 0)
                {
                    throw new Data.DataException(string.Format("Database {0} can't be droped! There are more then one tables contain in this database",
                        database.DatabaseName));
                }

                cfg.Databases.Remove(database);
            }
        }

        static internal void RemoveTableConfig(string tableDir, string tableName)
        {
            Setting cfg = Config;

            string curDatabaseName = Service.CurrentConnection.ConnectionInfo.DatabaseName;

            Database database = GetDatabase(curDatabaseName);

            string fullTableName = GetTableFullName(tableName);

            lock (_LockObj)
            {
                string tableNameInDatabase = null;

                foreach (string tName in database.Tables)
                {
                    if (tName.Equals(fullTableName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        tableNameInDatabase = tName;
                        break;
                    }
                }

                if (tableNameInDatabase != null)
                {
                    database.Tables.Remove(tableNameInDatabase);
                }

                cfg.Tables.Remove(new TableConfig(tableDir));
            }
        }

        static internal bool TableExists(string tableDir)
        {
            Setting cfg = Config;

            lock (_LockObj)
            {
                return cfg.Tables.Contains(new TableConfig(tableDir));
            }
        }

        static internal void AddTableConfig(string tableDir, string tableName)
        {
            Setting cfg = Config;

            string curDatabaseName = Service.CurrentConnection.ConnectionInfo.DatabaseName;

            Database database = GetDatabase(curDatabaseName);

            if (database == null)
            {
                throw new Data.DataException(string.Format("Current database {0} does not exist!",
                    curDatabaseName));
            }

            string fullTableName = GetTableFullName(tableName);

            lock (_LockObj)
            {
                foreach(string tName in database.Tables)
                {
                    if (tName.Equals(fullTableName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        throw new Data.DataException(string.Format("Table {0} in database {1} exist!",
                            fullTableName, curDatabaseName));
                    }

                }

                database.Tables.Add(fullTableName);
                

                cfg.Tables.Add(new TableConfig(tableDir));
            }
        }

        private static string GetMD5FileName(string fileName)
        {
            string fullName = System.IO.Path.GetFullPath(fileName);

            System.Security.Cryptography.MD5CryptoServiceProvider _MD5 =
                new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] buf = new byte[fullName.Length * 2];

            for (int i = 0; i < fullName.Length; i++)
            {
                char c = fullName[i];
                buf[2 * i] = (byte)(c % 256);
                buf[2 * i + 1] = (byte)(c / 256);
            }

            buf = _MD5.ComputeHash(buf);

            StringBuilder sb = new StringBuilder();

            foreach (byte b in buf)
            {
                sb.AppendFormat("{0:000}", b);
            }

            return sb.ToString();
        }

        static System.Threading.Mutex _Mutex;

        static internal void Load(string path)
        {
            string fileName = Path.AppendDivision(path, '\\') + FileName;

            string md5FileName = GetMD5FileName(fileName);

            try
            {
                _Mutex = new System.Threading.Mutex(false, md5FileName);
                if (!_Mutex.WaitOne(1000))
                {
                    throw new ReEntryException("Can't entry database on same path!");
                }
            }
            catch(Exception e)
            {
                throw new ReEntryException(e.Message);
            }


            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open,
                     System.IO.FileAccess.Read))
                {
                    Config = XmlSerialization<Setting>.Deserialize(fs);
                }
            }
            else
            {
                Config = new Setting();
            }
        }

        #endregion

        #region Constructor

        public Setting()
        {
            Directories = new Directories();
        }

        #endregion

        #region Public properties

        bool _AppLogEnabled = false;

        public bool AppLogEnabled
        {
            get
            {
                return _AppLogEnabled;
            }

            set
            {
                _AppLogEnabled = value;
            }
        }

        int _TcpPort = 7523;

        public int TcpPort
        {
            get
            {
                return _TcpPort;
            }

            set
            {
                _TcpPort = value;
            }
        }

        int _MaxConnectNum = 32;

        public int MaxConnectNum
        {
            get
            {
                return _MaxConnectNum;
            }

            set
            {
                _MaxConnectNum = value;

                if (_MaxConnectNum < 1)
                {
                    _MaxConnectNum = 1;
                }
            }
        }


        long _MemoryLimited = 800 * 1024 * 1024;

        public long MemoryLimited
        {
            get
            {
                lock (_LockObj)
                {
                    return _MemoryLimited;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _MemoryLimited = value;

                    if (_MemoryLimited < 1 * 1024 * 1024)
                    {
                        _MemoryLimited = 1 * 1024 * 1024;
                    }
                }
            }
        }

        long _QueryCacheMemoryLimited = 16 * 1024 * 1024;

        public long QueryCacheMemoryLimited
        {
            get
            {
                lock (_LockObj)
                {
                    return _QueryCacheMemoryLimited;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _QueryCacheMemoryLimited = value;

                    if (_QueryCacheMemoryLimited < 64 * 1024 )
                    {
                        _MemoryLimited = 1 * 1024 * 1024;
                    }
                }
            }

        }

        int _MaxUnionSelectThread = 8;

        public int MaxUnionSelectThread
        {
            get
            {
                lock (_LockObj)
                {
                    return _MaxUnionSelectThread;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _MaxUnionSelectThread = value;

                    if (_MaxUnionSelectThread < 1)
                    {
                        _MaxUnionSelectThread = 1;
                    }
                }
            }

        }

        bool _InitTablesStartup = false;

        /// <summary>
        /// Initialize all the tables after startup
        /// If false, the table will be initialized when it is accessed firstly.
        /// </summary>
        public bool InitTablesStartup
        {
            get
            {
                return _InitTablesStartup;
            }

            set
            {
                _InitTablesStartup = value;
            }
        }


        Directories _Directories = new Directories();

        public Directories Directories
        {
            get
            {
                return _Directories;
            }

            set
            {
                _Directories = value;
            }
        }

        List<Database> _Databases = new List<Database>();

        public List<Database> Databases
        {
            get
            {
                return _Databases;
            }

            set
            {
                _Databases = value;
            }
        }

        List<TableConfig> _Tables = new List<TableConfig>();

        public List<TableConfig> Tables
        {
            get
            {
                return _Tables;
            }

            set
            {
                _Tables = value;
            }
        }

        List<IQueryConfig> _IQuerys = new List<IQueryConfig>();

        /// <summary>
        /// IQuery exernal reference configuration 
        /// </summary>
        public List<IQueryConfig> IQuerys
        {
            get
            {
                return _IQuerys;
            }

            set
            {
                _IQuerys = value;
            }
        }

        List<IAnalyzerConfig> _IAnalyzers = new List<IAnalyzerConfig>();

        /// <summary>
        /// IAnalyzer exernal reference configuration 
        /// </summary>
        public List<IAnalyzerConfig> IAnalyzers
        {
            get
            {
                return _IAnalyzers;
            }

            set
            {
                _IAnalyzers = value;
            }
        }

        List<IDBAdapterConfig> _IDBAdapters = new List<IDBAdapterConfig>();

        /// <summary>
        /// IDBAdapter exernal reference configuration 
        /// </summary>
        public List<IDBAdapterConfig> IDBAdapters
        {
            get
            {
                return _IDBAdapters;
            }

            set
            {
                _IDBAdapters = value;
            }
        }


        #endregion
    }

    [Serializable]
    public class Directories
    {
        private string _LogDirectory;

        public string LogDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_LogDirectory))
                {
                    return Path.AppendDivision(Setting.SettingPath, '\\') + @"Log\";
                }

                return Path.AppendDivision(System.IO.Path.GetFullPath(_LogDirectory), '\\');
            }

            set
            {
                _LogDirectory = value;
            }
        }

        public Directories()
        {
            
        }
    }

    [Serializable]
    public class Database
    {
        private string _DatabaseName = "";

        [System.Xml.Serialization.XmlAttribute]
        public string DatabaseName
        {
            get
            {
                return _DatabaseName.Trim();
            }

            set
            {
                _DatabaseName = value;
            }
        }

        private string _DefaultPath;

        [System.Xml.Serialization.XmlAttribute]
        public string DefaultPath
        {
            get
            {
                return _DefaultPath;
            }

            set
            {
                _DefaultPath = value;
            }
        }


        private string _DefaultDBAdapter;

        [System.Xml.Serialization.XmlAttribute]
        public string DefaultDBAdapter
        {
            get
            {
                return _DefaultDBAdapter;
            }

            set
            {
                _DefaultDBAdapter = value;
            }
        }

        private string _DefaultConnectionString;

        [System.Xml.Serialization.XmlAttribute]
        public string DefaultConnectionString
        {
            get
            {
                return _DefaultConnectionString;
            }

            set
            {
                _DefaultConnectionString = value;
            }
        }


        private List<string> _Tables = new List<string>();

        public List<string> Tables
        {
            get
            {
                return _Tables;
            }

            set
            {
                _Tables = value;
            }
        }

        public override bool Equals(object obj)
        {
            return this.DatabaseName.Equals(((Database)obj).DatabaseName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.DatabaseName.GetHashCode();
        }
    }


    [Serializable]
    public class TableConfig
    {
        private string _Directory;

        public string Directory
        {
            get
            {
                return Path.AppendDivision(System.IO.Path.GetFullPath(_Directory), '\\');
            }

            set
            {
                _Directory = value;
            }
        }

        public TableConfig()
        {
        }

        public TableConfig(string dir)
        {
            Directory = dir;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return this.Directory.Trim().ToLower() == ((TableConfig)obj).Directory.Trim().ToLower();
        }

        public override int GetHashCode()
        {
            return this.Directory.GetHashCode();
        }
    }

    [Serializable]
    public class IQueryConfig : ExternalReference
    {
        [System.Xml.Serialization.XmlIgnore]
        protected override Type Interface
        {
            get 
            {
                return typeof(Query.IQuery);
            }
        }

        public IQueryConfig()
        {
        }

        public IQueryConfig(string assemblyFile)
        {
            base.AssemblyFile = assemblyFile;
        }
    }

    [Serializable]
    public class IAnalyzerConfig : ExternalReference
    {
        [System.Xml.Serialization.XmlIgnore]
        protected override Type Interface
        {
            get
            {
                return typeof(Analysis.IAnalyzer);
            }
        }

        public IAnalyzerConfig()
        {
        }

        public IAnalyzerConfig(string assemblyFile)
        {
            base.AssemblyFile = assemblyFile;
        }
    }

    [Serializable]
    public class IDBAdapterConfig : ExternalReference
    {
        [System.Xml.Serialization.XmlIgnore]
        protected override Type Interface
        {
            get
            {
                return typeof(DBAdapter.IDBAdapter);
            }
        }

        public IDBAdapterConfig()
        {
        }

        public IDBAdapterConfig(string assemblyFile)
        {
            base.AssemblyFile = assemblyFile;
        }
    }


}
