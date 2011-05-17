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
using System.Diagnostics;

using Hubble.Core.Index;
using Hubble.Core.Global;
using Hubble.Core.StoredProcedure;
using Hubble.Framework.Reflection;
using Hubble.Framework.Threading;

namespace Hubble.Core.Data
{
    public partial class DBProvider
    {
        #region Static members
        static Dictionary<string, DBProvider> _DBProviderTable = new Dictionary<string, DBProvider>();
        static object _LockObj = new object();
        static object _sLockObj = new object();

        static Dictionary<string, Type> _QueryTable = new Dictionary<string, Type>(); //name to IQuery Type
        static Dictionary<string, Type> _AnalyzerTable = new Dictionary<string, Type>(); //name to IAnalzer Type
        static Dictionary<string, Type> _DBAdapterTable = new Dictionary<string, Type>(); //name to IDBAdapter Type
        static Dictionary<string, Type> _StoredProcTable = new Dictionary<string, Type>(); //name to StoredProcedure type

        static bool _sNeedClose = false;
        static bool _sCanClose = true;

        static internal Type BigTableParseType = null;

        internal static bool StaticCanClose
        {
            get
            {
                lock (_sLockObj)
                {
                    return _sCanClose;
                }
            }
        }

        internal static void StaticClose()
        {
            lock (_sLockObj)
            {
                _sNeedClose = true;
            }
        }

        private static void DeleteTableName(string tableName)
        {
            lock (_sLockObj)
            {
                if (_DBProviderTable.ContainsKey(tableName.ToLower().Trim()))
                {
                    _DBProviderTable.Remove(tableName.ToLower().Trim());
                }
            }
        }

        private static DBProvider InitTable(TableConfig tableCfg)
        {
            DBProvider dbProvider = new DBProvider();

            dbProvider.Open(tableCfg.Directory);

            return dbProvider;
        }

        internal static Type[] GetAnalyzers()
        {
            Type[] result = new Type[_AnalyzerTable.Count];

            int i = 0;

            //dbadapter interface only load one at begining, so need not lock
            foreach (Type type in _AnalyzerTable.Values)
            {
                result[i++] = type;
            }

            return result;
        }

        internal static Type[] GetDBAdapters()
        {
            Type[] result = new Type[_DBAdapterTable.Count];

            int i = 0;

            //dbadapter interface only load one at begining, so need not lock
            foreach (Type type in _DBAdapterTable.Values)
            {
                result[i++] = type;
            }

            return result;
        }

        internal static Type GetDBAdapter(string name)
        {
            Type type;

            if (_DBAdapterTable.TryGetValue(name.ToLower().Trim(), out type))
            {
                return type;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get Query by command name
        /// </summary>
        /// <param name="command">command name</param>
        /// <returns>The instance of IQuery. If not find, return null</returns>
        internal static Hubble.Core.Query.IQuery GetQuery(string command)
        {
            Type type;

            //query interface only load one at begining, so need not lock
            if (_QueryTable.TryGetValue(command.Trim().ToLower(), out type))
            {
                return Hubble.Framework.Reflection.Instance.CreateInstance(type)
                    as Hubble.Core.Query.IQuery;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Get analyzer by analyzer name
        /// </summary>
        /// <param name="name">analyzer name</param>
        /// <returns>The instance of IQuery. If not find, return null</returns>
        internal static Hubble.Core.Analysis.IAnalyzer GetAnalyzer(string name)
        {
            Type type;

            //analyser interface only load one at begining, so need not lock
            if (_AnalyzerTable.TryGetValue(name.Trim().ToLower(), out type))
            {
                return Hubble.Framework.Reflection.Instance.CreateInstance(type)
                    as Hubble.Core.Analysis.IAnalyzer;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Get storedprocedure by storedprocedure name
        /// </summary>
        /// <param name="name">storedprocedure name</param>
        /// <returns>the instance of IStoredProc. If not find, return null</returns>
        internal static Hubble.Core.StoredProcedure.IStoredProc GetStoredProc(string name)
        {
            Type type;

            //store proc interface only load one at begining, so need not lock
            if (_StoredProcTable.TryGetValue(name.Trim().ToLower(), out type))
            {
                return Hubble.Framework.Reflection.Instance.CreateInstance(type)
                    as Hubble.Core.StoredProcedure.IStoredProc;
            }
            else
            {
                return null;
            }

        }

        internal static List<IStoredProc> GetAllStoredProcs()
        {
            List<IStoredProc> result = new List<IStoredProc>();

            foreach (Type type in _StoredProcTable.Values)
            {
                IStoredProc storedProc =
                    Hubble.Framework.Reflection.Instance.CreateInstance(type)
                    as Hubble.Core.StoredProcedure.IStoredProc;

                if (storedProc != null)
                {
                    result.Add(storedProc);
                }
            }

            return result;
        }

        internal static List<DBProvider> GetDbProviders()
        {
            lock (_sLockObj)
            {
                List<DBProvider> dbProviders = new List<DBProvider>();

                foreach (DBProvider dbProvider in _DBProviderTable.Values)
                {
                    dbProviders.Add(dbProvider);
                }

                return dbProviders;
            }
        }

        public static List<string> GetTables()
        {
            lock (_sLockObj)
            {
                List<string> tables = new List<string>();

                foreach (DBProvider dbProvider in _DBProviderTable.Values)
                {
                    tables.Add(dbProvider.TableName);
                }

                return tables;
            }
        }

        public static DBProvider GetDBProvider(string databaseName, string tableName)
        {
            tableName = Setting.GetTableFullName(databaseName, tableName);

            lock (_sLockObj)
            {
                DBProvider dbProvider;
                if (_DBProviderTable.TryGetValue(tableName.ToLower().Trim(), out dbProvider))
                {
                    if (!dbProvider._Inited)
                    {
                        Exception e = dbProvider.Open();

                        if (e != null)
                        {
                            throw e;
                        }
                    }

                    return dbProvider;
                }
                else
                {
                    return null;
                }
            }
        }

        public static DBProvider GetDBProviderByFullName(string fullTableName)
        {
            return GetDBProviderByFullName(fullTableName, true);
        }

        public static DBProvider GetDBProviderByFullName(string fullTableName, bool init)
        {
            lock (_sLockObj)
            {
                DBProvider dbProvider;
                if (_DBProviderTable.TryGetValue(fullTableName.ToLower().Trim(), out dbProvider))
                {
                    if (!dbProvider._Inited && init)
                    {
                        Exception e = dbProvider.Open();

                        if (e != null)
                        {
                            throw e;
                        }
                    }

                    return dbProvider;
                }
                else
                {
                    return null;
                }
            }
        }

        public static DBProvider GetDBProvider(string tableName)
        {
            return GetDBProvider(tableName, true);
        }

        public static DBProvider GetDBProvider(string tableName, bool init)
        {
            return GetDBProviderByFullName(Setting.GetTableFullName(tableName), init);
        }

        public static bool DBProviderExists(string tableName)
        {
            tableName = Setting.GetTableFullName(tableName);

            lock (_sLockObj)
            {
                return _DBProviderTable.ContainsKey(tableName.ToLower().Trim());
            }
        }

        public static void NewDBProvider(string tableName, DBProvider dbProvider)
        {
            tableName = Setting.GetTableFullName(tableName);

            lock (_sLockObj)
            {
                _DBProviderTable.Add(tableName.ToLower().Trim(), dbProvider);
            }
        }

        public static void CreateTable(Table table, string directory)
        {
            lock (_sLockObj)
            {
                if (_sNeedClose)
                {
                    return;
                }

                _sCanClose = false;
            }

            try
            {
                if (table.Name == null)
                {
                    throw new System.ArgumentNullException("Null table name");
                }

                if (table.Name.Trim() == "")
                {
                    throw new System.ArgumentException("Empty table name");
                }

                table.Name = Setting.GetTableFullName(table.Name.Trim());

                if (DBProvider.DBProviderExists(table.Name))
                {
                    throw new DataException(string.Format("Table {0} exists already!", table.Name));
                }

                directory = Hubble.Framework.IO.Path.AppendDivision(directory, '\\');

                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                DBProvider.NewDBProvider(table.Name, new DBProvider());

                try
                {
                    DBProvider.GetDBProvider(table.Name, false).Create(table, directory);
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog("Create table fail!", e);

                    DBProvider.Drop(table.Name);

                    throw e;
                }
            }
            finally
            {
                lock (_sLockObj)
                {
                    _sCanClose = true;
                }
            }
        }

        public static void Drop(string tableName)
        {
            lock (_sLockObj)
            {
                if (_sNeedClose)
                {
                    return;
                }

                _sCanClose = false;
            }

            try
            {
                tableName = Setting.GetTableFullName(tableName);

                DBProvider dbProvider = GetDBProvider(tableName, false);


                if (dbProvider != null)
                {
                    if (dbProvider.Table != null)
                    {
                        if (!dbProvider.Table.IsBigTable)
                        {
                            if (!dbProvider.Table.IndexOnly)
                            {
                                dbProvider.DBAdapter.Drop();
                            }
                            else
                            {
                                if (dbProvider.MirrorDBAdapter != null)
                                {
                                    dbProvider.MirrorDBAdapter.Drop();
                                }
                            }
                        }
                    }

                    if (!dbProvider._TableLock.Enter(Lock.Mode.Mutex, 30000))
                    {
                        throw new TimeoutException();
                    }

                    try
                    {
                        string dir = dbProvider.Directory;

                        if (dir == null)
                        {
                            DeleteTableName(tableName);
                            return;
                        }

                        dbProvider.Drop();
                        DeleteTableName(tableName);

                        Global.Setting.RemoveTableConfig(dir, tableName);
                        Global.Setting.Save();
                    }
                    finally
                    {
                        dbProvider._TableLock.Leave();
                    }
                }
            }
            finally
            {
                lock (_sLockObj)
                {
                    _sCanClose = true;
                }
            }
        }

        public static void Truncate(string tableName)
        {
            lock (_sLockObj)
            {
                if (_sNeedClose)
                {
                    return;
                }

                _sCanClose = false;
            }

            try
            {
                string fullTableName = Setting.GetTableFullName(tableName);

                DBProvider dbProvider = GetDBProvider(fullTableName, false);

                if (dbProvider != null)
                {
                    Table table = dbProvider.Table;
                    string dir = dbProvider.Directory;
                    bool indexReadOnly = table.IndexOnly;

                    dbProvider.SetIndexOnly(true);

                    if (!indexReadOnly)
                    {
                        dbProvider.DBAdapter.Truncate();
                    }
                    else
                    {
                        if (dbProvider.MirrorDBAdapter != null)
                        {
                            try
                            {
                                dbProvider.MirrorDBAdapter.Truncate();
                            }
                            catch (Exception e)
                            {
                                Global.Report.WriteErrorLog(string.Format("Truncate mirror table fail! tablename={0}, dbadapter={1} connectstring={2}",
                                    dbProvider.Table.MirrorDBTableName, dbProvider.Table.MirrorDBAdapterTypeName,
                                    dbProvider.Table.MirrorConnectionString), e);
                            }
                        }
                    }

                    Drop(tableName);

                    CreateTable(table, dir);

                    fullTableName = Setting.GetTableFullName(tableName);
                    dbProvider = GetDBProvider(fullTableName);
                    dbProvider.SetIndexOnly(indexReadOnly);
                }
                else
                {
                    throw new DataException(string.Format("Table name {0} does not exist!",
                        tableName));
                }
            }
            finally
            {
                lock (_sLockObj)
                {
                    _sCanClose = true;
                }
            }
        }


        public static void Init(string settingPath)
        {
            Setting.SettingPath = settingPath;

            Init();
        }

        public static void Init()
        {

#if HubblePro
            System.Reflection.Assembly bigTableAsm;
            bigTableAsm = System.Reflection.Assembly.LoadFrom("HubblePro.BigTable.dll");
            BigTableParseType = bigTableAsm.GetType("HubblePro.BigTable.BigTableParse");
#else
            BigTableParseType = typeof(BigTable.BigTableParse);
#endif


            //Init XML Cache File Serialization
            Hubble.Framework.Serialization.XmlSerialization<Cache.CacheFile>.Serialize(
                            new Cache.CacheFile("",
                                new Hubble.Core.Cache.QueryCacheDocuments(),
                                new Hubble.Core.Cache.QueryCacheInformation()),
                            Encoding.UTF8, new System.IO.MemoryStream());


            //Load user right from right.db
            try
            {
                Global.UserRightProvider.Load(Global.Setting.UserRigthFilePath);
            }
            catch (Exception e)
            {
                Global.Report.WriteErrorLog("Load user right db fail!", e);
            }

            //Build QueryTable

            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

            foreach (Type type in asm.GetTypes())
            {
                //Init _QueryTable
                if (type.GetInterface("Hubble.Core.Query.IQuery") != null)
                {
                    Hubble.Core.Query.IQuery iQuery = asm.CreateInstance(type.FullName) as
                        Hubble.Core.Query.IQuery;
                    string key = iQuery.Command.ToLower().Trim();
                    if (!_QueryTable.ContainsKey(key))
                    {
                        _QueryTable.Add(key, type);
                    }
                    else
                    {
                        Global.Report.WriteErrorLog(string.Format("Reduplicate query command name = {0}",
                            iQuery.Command));
                    }
                }

                //Init _AnalyzerTable
                if (type.GetInterface("Hubble.Core.Analysis.IAnalyzer") != null)
                {
                    INamedExternalReference refer = asm.CreateInstance(type.FullName) as
                        INamedExternalReference;

                    if (refer == null)
                    {
                        Report.WriteErrorLog(string.Format("External reference {0} does not inherit from INamedExternalReference",
                            type.FullName));
                    }
                    else
                    {
                        string key = refer.Name.ToLower().Trim();


                        if (!_AnalyzerTable.ContainsKey(key))
                        {
                            Analysis.IAnalyzer analyzer = refer as Analysis.IAnalyzer;
                            analyzer.Init();
                            _AnalyzerTable.Add(key, type);
                        }
                        else
                        {
                            Global.Report.WriteErrorLog(string.Format("Reduplicate name = {0} in External reference {1}!",
                                refer.Name, type.FullName));
                        }
                    }
                }

                //Init _DBAdapterTable
                if (type.GetInterface("Hubble.Core.DBAdapter.IDBAdapter") != null)
                {
                    INamedExternalReference refer = asm.CreateInstance(type.FullName) as
                        INamedExternalReference;

                    if (refer == null)
                    {
                        Report.WriteErrorLog(string.Format("External reference {0} does not inherit from INamedExternalReference",
                            type.FullName));
                    }
                    else
                    {
                        string key = refer.Name.ToLower().Trim();

                        if (!_DBAdapterTable.ContainsKey(key))
                        {
                            _DBAdapterTable.Add(key, type);
                        }
                        else
                        {
                            Global.Report.WriteErrorLog(string.Format("Reduplicate name = {0} in External reference {1}!",
                                refer.Name, type.FullName));
                        }
                    }
                }

                if (type.GetInterface("Hubble.Core.StoredProcedure.IStoredProc") != null)
                {
                    Hubble.Core.StoredProcedure.IStoredProc iSP = asm.CreateInstance(type.FullName) as
                        Hubble.Core.StoredProcedure.IStoredProc;
                    string key = iSP.Name.ToLower().Trim();
                    if (!_StoredProcTable.ContainsKey(key))
                    {
                        _StoredProcTable.Add(key, type);
                    }
                    else
                    {
                        Global.Report.WriteErrorLog(string.Format("Reduplicate StoredProcedure name = {0}",
                            iSP.Name));
                    }
                }
            }

   
                //Load from external reference 
                //Load IQuery external reference
                foreach (IQueryConfig iquery in Setting.Config.IQuerys)
                {
                    try
                    {
                        if (iquery != null)
                        {
                            iquery.Load(_QueryTable);
                        }
                    }
                    catch (Exception e)
                    {
                        Global.Report.WriteErrorLog(string.Format("Load IQuery fail. IQuery asm file:{0}", iquery.AssemblyFile),
                            e);
                    }

                }

            //Load IAnalyzer external reference
            foreach (IAnalyzerConfig ianalyzer in Setting.Config.IAnalyzers)
            {
                try
                {
                    if (ianalyzer != null)
                    {
                        ianalyzer.Load(_AnalyzerTable);
                    }
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog(string.Format("Load IAnalyzer fail. IAnalyzer asm file:{0}", ianalyzer.AssemblyFile),
                        e);
                }
            }

            //Load IDBAdapter external reference
            foreach (IDBAdapterConfig idbadapter in Setting.Config.IDBAdapters)
            {
                try
                {
                    if (idbadapter != null)
                    {
                        idbadapter.Load(_DBAdapterTable);
                    }
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog(string.Format("Load IDBAdapter fail. IDBAdapter asm file:{0}", idbadapter.AssemblyFile),
                        e);
                }
            }

            //List<string> removeDirs = new List<string>();

            //Init table
            foreach (TableConfig tc in Setting.Config.Tables)
            {
                try
                {
                    if (tc.Directory == null)
                    {
                        continue;
                    }

                    DBProvider dbProvider = InitTable(tc);

                    if (_DBAdapterTable.ContainsKey(dbProvider.TableName.ToLower().Trim()))
                    {
                        Global.Report.WriteErrorLog(string.Format("Init Table {0} fail, table exists!",
                            dbProvider.TableName));
                    }
                    else
                    {
                        _DBProviderTable.Add(dbProvider.TableName.ToLower().Trim(), dbProvider);
                    }
                }
                catch (Exception e)
                {
                    //removeDirs.Add(tc.Directory);

                    Global.Report.WriteErrorLog(string.Format("Init Table at {0} fail, errmsg:{1} stack {2}",
                        tc.Directory, e.Message, e.StackTrace));
                }
            }

            Cache.QueryCacheManager.Manager.MaxMemorySize = Global.Setting.Config.QueryCacheMemoryLimited;

            //foreach (string removeDir in removeDirs)
            //{
            //    Setting.RemoveTableConfig(removeDir);
            //}
        }



        #endregion

    }
}
