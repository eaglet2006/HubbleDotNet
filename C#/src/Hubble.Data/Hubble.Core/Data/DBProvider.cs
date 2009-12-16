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
    public class DBProvider
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

        public static DBProvider GetDBProvider(string tableName)
        {
            tableName = Setting.GetTableFullName(tableName);

            lock (_sLockObj)
            {
                DBProvider dbProvider;
                if (_DBProviderTable.TryGetValue(tableName.ToLower().Trim(), out dbProvider))
                {
                    return dbProvider;
                }
                else
                {
                    return null;
                }
            }
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
                    DBProvider.GetDBProvider(table.Name).Create(table, directory);
                }
                catch (Exception e)
                {
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

                DBProvider dbProvider = GetDBProvider(tableName);

                if (dbProvider != null)
                {
                    try
                    {
                        dbProvider._TableLock.Enter(Lock.Mode.Mutex, 30000);

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

        public static void Init(string settingPath)
        {
            Setting.SettingPath = settingPath;

            Init();
        }

        public static void Init()
        {
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
                iquery.Load(_QueryTable);
            }

            //Load IAnalyzer external reference
            foreach (IAnalyzerConfig ianalyzer in Setting.Config.IAnalyzers)
            {
                ianalyzer.Load(_AnalyzerTable);
            }

            //Load IDBAdapter external reference
            foreach (IDBAdapterConfig idbadapter in Setting.Config.IDBAdapters)
            {
                idbadapter.Load(_DBAdapterTable);
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

        #region structure

        struct PayloadSegment
        {
            public int TabIndex;
            public int[] Data;

            public PayloadSegment(int tabIndex, int[] data)
            {
                TabIndex = tabIndex;
                Data = data;
            }
        }

        #endregion

        #region Private fields

        private Table _Table = null;

        private Dictionary<string, InvertedIndex> _FieldInvertedIndex = new Dictionary<string, InvertedIndex>();
        private Dictionary<string, Field> _FieldIndex = new Dictionary<string, Field>();

        private Dictionary<long, Payload> _DocPayload = new Dictionary<long, Payload>(); //DocId to payload

        private string _Directory;

        private int _PayloadLength = 0;
        private string _PayloadFileName;
        private Store.PayloadFile _PayloadFile;

        private int _InsertCount = 0;
        private DeleteProvider _DelProvider;
        private Hubble.Framework.Threading.Lock _TableLock = new Hubble.Framework.Threading.Lock();

        private long _LastModifyTicks = DateTime.Now.Ticks;

        private Cache.QueryCache _QueryCache = null;

        private object _ExitLock = new object();
        private bool _NeedExit = false;
        private int _BusyCount = 0;

        #endregion

        #region Properties

        long _LastDocId = 0;

        DBAdapter.IDBAdapter _DBAdapter;

        public string Directory
        {
            get
            {
                return _Directory;
            }
        }

        /// <summary>
        /// Database adapter
        /// </summary>
        public DBAdapter.IDBAdapter DBAdapter
        {
            get
            {
                return _DBAdapter;
            }

            set
            {
                _DBAdapter = value;
            }
        }

        internal Table Table
        {
            get
            {
                return _Table;
            }
        }

        public string TableName
        {
            get
            {
                return _Table.Name;
            }
        }

        public bool IndexOnly
        {
            get
            {
                return _Table.IndexOnly;
            }
        }

        public int PayloadLength
        {
            get
            {
                return _PayloadLength;
            }
        }

        internal Cache.QueryCache QueryCache
        {
            get
            {
                return _QueryCache;
            }
        }

        internal bool QueryCacheEnabled
        {
            get
            {
                return _Table.QueryCacheEnabled;
            }
        }

        internal int QueryCacheTimeout
        {
            get
            {
                return _Table.QueryCacheTimeout;
            }
        }

        internal DeleteProvider DelProvider
        {
            get
            {
                return _DelProvider;
            }
        }

        internal long LastModifyTicks
        {
            get
            {
                lock (this)
                {
                    return _LastModifyTicks;
                }
            }

            set
            {
                lock (this)
                {
                    _LastModifyTicks = value;
                }
            }

        }

        internal bool Closed
        {
            get
            {
                lock (_ExitLock)
                {
                    if (_BusyCount > 0)
                    {
                        return false;
                    }

                    foreach (InvertedIndex iIndex in _FieldInvertedIndex.Values)
                    {
                        if (!iIndex.OptimizeStoped)
                        {
                            return false;
                        }

                        if (!iIndex.IndexStoped)
                        {
                            return false;
                        }

                    }

                    return true;
                }
            }
        }

        #endregion

        #region Private and internal methods

        private void AddFieldInvertedIndex(string fieldName, InvertedIndex index)
        {
            lock (this)
            {
                index.ForceCollectCount = _Table.ForceCollectCount;
                _FieldInvertedIndex.Add(fieldName.Trim().ToLower(), index);
            }
        }

        internal void SetCacheQuery(bool value, int timeout)
        {
            _Table.QueryCacheEnabled = value;

            if (timeout >= 0)
            {
                _Table.QueryCacheTimeout = timeout;
            }

        }

        internal void SetIndexOnly(bool value)
        {
            try
            {
                _TableLock.Enter(Lock.Mode.Share, 30000);

                _Table.IndexOnly = value;
            }
            finally
            {
                _TableLock.Leave();
            }
        }

        internal InvertedIndex GetInvertedIndex(string fieldName)
        {
            lock (this)
            {
                InvertedIndex index;

                if (_FieldInvertedIndex.TryGetValue(fieldName.Trim().ToLower(), out index))
                {
                    return index;
                }
                else
                {
                    return null;
                }
            }
        }

        private void ClearAll()
        {
            lock (this)
            {
                _FieldIndex.Clear();
                _FieldInvertedIndex.Clear();
                _DocPayload.Clear();
                _PayloadFile = null;
            }
        }

        private void AddFieldIndex(string fieldName, Field field)
        {
            lock (this)
            {
                _FieldIndex.Add(fieldName.Trim().ToLower(), field);
            }
        }

        internal Field GetField(string fieldName)
        {
            lock (this)
            {
                Field field;

                if (_FieldIndex.TryGetValue(fieldName.Trim().ToLower(), out field))
                {
                    return field;
                }
                else
                {
                    return null;
                }
            }
        }


        internal List<Field> GetAllFields()
        {
            lock (this)
            {
                List<Field> selectFields = new List<Field>();

                foreach (Field field in _FieldIndex.Values)
                {
                    selectFields.Add(field);
                }

                return selectFields;
            }

        }

        internal List<Field> GetAllSelectFields()
        {
            lock (this)
            {
                List<Field> selectFields = new List<Field>();

                selectFields.Add(new Data.Field("DocId", Hubble.Core.Data.DataType.BigInt));

                foreach (Field field in _FieldIndex.Values)
                {
                    selectFields.Add(field);
                }

                selectFields.Add(new Data.Field("Score", Hubble.Core.Data.DataType.BigInt));

                return selectFields;
            }

        }

        private void OpenPayloadInformation(Table table)
        {
            _PayloadLength = 0;

            foreach (Field field in table.Fields)
            {
                switch (field.IndexType)
                {
                    case Field.Index.Tokenized:
                        _PayloadLength++;
                        break;
                    case Field.Index.Untokenized:
                        _PayloadLength += DataTypeConvert.GetDataLength(field.DataType, field.DataLength);
                        break;
                    default:
                        continue;
                }
            }
        }



        private void CreatePayloadInformation(Table table)
        {
            _PayloadLength = 0;

            foreach (Field field in table.Fields)
            {
                int tabIndex = -1;

                switch (field.IndexType)
                {
                    case Field.Index.Tokenized:
                        tabIndex = _PayloadLength;
                        _PayloadLength++;
                        break;
                    case Field.Index.Untokenized:
                        tabIndex = _PayloadLength;
                        _PayloadLength += DataTypeConvert.GetDataLength(field.DataType, field.DataLength);
                        break;
                    default:
                        continue;
                }

                field.TabIndex = tabIndex;
            }
        }

        private void Drop()
        {
            try
            {
                if (_Table == null)
                {
                    return;
                }

                foreach (Field field in _Table.Fields)
                {
                    if (field.IndexType == Field.Index.Tokenized)
                    {
                        InvertedIndex invertedIndex = GetInvertedIndex(field.Name);
                        invertedIndex.Close();
                    }
                }

                _PayloadFile.Close(2000);

                string dir = Directory;
                string optimizeDir = Hubble.Framework.IO.Path.AppendDivision(Directory, '\\') + "Optimize";

                //Delete files

                foreach (string file in System.IO.Directory.GetFiles(dir, "*.hdx"))
                {
                    System.IO.File.Delete(file);
                }

                foreach (string file in System.IO.Directory.GetFiles(dir, "*.idx"))
                {
                    System.IO.File.Delete(file);
                }

                foreach (string file in System.IO.Directory.GetFiles(dir, "Payload.db"))
                {
                    System.IO.File.Delete(file);
                }

                foreach (string file in System.IO.Directory.GetFiles(dir, "Delete.db"))
                {
                    System.IO.File.Delete(file);
                }

                foreach (string file in System.IO.Directory.GetFiles(dir, "tableinfo.xml"))
                {
                    System.IO.File.Delete(file);
                }

                if (System.IO.Directory.Exists(optimizeDir))
                {
                    foreach (string file in System.IO.Directory.GetFiles(optimizeDir, "*.hdx"))
                    {
                        System.IO.File.Delete(file);
                    }

                    foreach (string file in System.IO.Directory.GetFiles(optimizeDir, "*.idx"))
                    {
                        System.IO.File.Delete(file);
                    }
                }

                ClearAll();
            }
            finally
            {
                if (_QueryCache != null)
                {
                    try
                    {
                        Cache.QueryCacheManager.Manager.Delete(_QueryCache);
                    }
                    catch
                    {
                    }

                    _QueryCache = null;
                }
            }
        }

        private void Open(string directory)
        {
            _Directory = directory;
            _DelProvider = new DeleteProvider();

            _Table = Table.Load(directory);
            Table table = _Table;
            long dbMaxDocId = 0;

            lock (_LockObj)
            {
                //Get DB adapter
                if (!string.IsNullOrEmpty(table.DBAdapterTypeName))
                {
                    Type dbAdapterType;

                    if (_DBAdapterTable.TryGetValue(table.DBAdapterTypeName.ToLower().Trim(), out dbAdapterType))
                    {
                        DBAdapter = (DBAdapter.IDBAdapter)Instance.CreateInstance(dbAdapterType);
                    }
                }

                //init db table
                if (DBAdapter != null)
                {
                    DBAdapter.Table = table;

                    int TryConnectDBTimes = 5;

                    for (int i = 0; i < TryConnectDBTimes; i++)
                    {
                        try
                        {
                            dbMaxDocId = DBAdapter.MaxDocId;
                            break;
                        }
                        catch (Exception e)
                        {
                            if (i == TryConnectDBTimes - 1)
                            {
                                throw;
                            }

                            Global.Report.WriteErrorLog(string.Format("Get MaxDocId fail, Try again! Try times:{0} Err:{1}, Stack:{2}",
                                i + 1, e.Message, e.StackTrace));

                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                }
                else
                {
                    throw new DataException(string.Format("Can't find DBAdapterTypeName : {0}",
                        table.DBAdapterTypeName));
                }

                //Open delete provider
                _DelProvider.Open(_Directory);

                //Open payload information
                OpenPayloadInformation(table);

                //set payload file name
                _PayloadFileName = Hubble.Framework.IO.Path.AppendDivision(directory, '\\') + "Payload.db";

                lock (this)
                {
                    //Start payload message queue
                    if (_PayloadFile == null)
                    {
                        _PayloadFile = new Hubble.Core.Store.PayloadFile(_PayloadFileName);

                        //Open Payload file
                        _DocPayload = _PayloadFile.Open(table.Fields, PayloadLength, out _LastDocId);
                        _LastDocId++;

                        if (_LastDocId <= dbMaxDocId)
                        {
                            _LastDocId = dbMaxDocId + 1;
                        }
                    }
                }


                //Start payload file
                _PayloadFile.Start();

                //Add field inverted index 
                foreach (Field field in table.Fields)
                {
                    if (field.IndexType == Field.Index.Tokenized)
                    {
                        InvertedIndex invertedIndex = new InvertedIndex(directory, field.Name.Trim(), field.TabIndex, field.Mode, false, this);
                        AddFieldInvertedIndex(field.Name, invertedIndex);
                    }

                    AddFieldIndex(field.Name, field);
                }
            }

            if (_QueryCache == null)
            {
                _QueryCache = new Cache.QueryCache(Cache.QueryCacheManager.Manager, this.TableName);
            }
        }

        public List<Document> Query(List<Field> selectFields, IList<Query.DocumentResult> docs)
        {
            return Query(selectFields, docs, 0, docs.Count - 1);
        }

        public List<Document> Query(List<Field> selectFields, IList<Query.DocumentResult> docs, int begin, int end)
        {
            List<Field> dbFields = new List<Field>();

            Dictionary<long, Document> docIdToDocumnet = null;

            List<Document> result;

            result = new List<Document>(docs.Count);

            if (docs.Count <= 0)
            {
                return result;
            }

            foreach (Field field in selectFields)
            {
                if (field.Store && field.IndexType != Field.Index.Untokenized)
                {
                    if (!field.Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                    {
                        dbFields.Add(field);
                    }
                }
            }

            if (dbFields.Count > 0)
            {
                docIdToDocumnet = new Dictionary<long,Document>();
            }

            if (end < 0)
            {
                end = docs.Count - 1;
            }

            if (begin < 0)
            {
                throw new DataException("Query error, begin less then 0!");
            }

            for(int i = begin; i <= end; i++)
            {
                if (i >= docs.Count)
                {
                    break;
                }
                
                long docId = docs[i].DocId;

                Document doc = new Document();
                result.Add(doc);
                doc.DocId = docId;

                if (docIdToDocumnet != null)
                {
                    if (!docIdToDocumnet.ContainsKey(docId))
                    {
                        docIdToDocumnet.Add(docId, doc);
                    }
                }

                Payload payload;

                lock (this)
                {
                    if (!_DocPayload.TryGetValue(docId, out payload))
                    {
                        payload = null;
                    }
                }

                foreach(Field field in selectFields)
                {
                    string value = null;

                    if (payload != null)
                    {
                        if (field.IndexType == Field.Index.Untokenized)
                        {
                            value = DataTypeConvert.GetString(field.DataType,
                                payload.Data, field.TabIndex, field.DataLength);
                        }
                    }
                    
                    if (field.Name.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                    {
                        value = docId.ToString();

                        doc.Add(field.Name, value, field.DataType, field.DataLength, false);
                    }
                    else if (field.Name.Equals("Score", StringComparison.CurrentCultureIgnoreCase))
                    {
                        value = docs[i].Score.ToString();

                        doc.Add(field.Name, value, field.DataType, field.DataLength, false);
                    }
                    else
                    {
                        doc.Add(field.Name, value, field.DataType, field.DataLength, 
                            field.Store && field.IndexType != Field.Index.Untokenized);
                    }
                    
                }
            }

            if (dbFields.Count > 0)
            {
                dbFields.Add(new Field("DocId", DataType.BigInt));
                System.Data.DataTable dt = _DBAdapter.Query(dbFields, docs, begin, end);

                foreach (System.Data.DataRow row in dt.Rows)
                {
                    long docId = (long)row["DocId"];

                    Document doc ;

                    if (docIdToDocumnet.TryGetValue(docId, out doc))
                    {
                        foreach (FieldValue fv in doc.FieldValues)
                        {
                            if (fv.FromDB)
                            {
                                object obj = row[fv.FieldName];
                                if (obj == System.DBNull.Value)
                                {
                                    fv.Value = null;
                                }
                                else
                                {
                                    fv.Value = obj.ToString();
                                }
                            }
                        }
                    }
                }
            }

            return result; 

        }


        #endregion

        #region internal methods

        internal void SaveTable()
        {
            _Table.Save(_Directory);
        }

        internal int GetDocWordsCount(long docId, int tabIndex)
        {
            int numDocWords = 1000000;

            lock (this)
            {
                Payload payload;
                _DocPayload.TryGetValue(docId, out payload);

                if (payload != null)
                {
                    numDocWords = payload.WordCount(tabIndex);
                }
            }

            return numDocWords;

        }

        internal Payload GetPayload(long docId)
        {
            lock (this)
            {
                Payload payload;
                if (_DocPayload.TryGetValue(docId, out payload))
                {
                    return payload;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Create a table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="directory"></param>
        internal void Create(Table table, string directory)
        {
            lock (_LockObj)
            {
                if (Setting.TableExists(directory))
                {
                    throw new DataException(string.Format("Table dicectory: {0} is already existed!",
                        directory));
                }

                _Directory = directory;
                _DelProvider = new DeleteProvider();

                //Get DB adapter
                if (!string.IsNullOrEmpty(table.DBAdapterTypeName))
                {
                    Type dbAdapterType;

                    if (_DBAdapterTable.TryGetValue(table.DBAdapterTypeName.ToLower().Trim(), out dbAdapterType))
                    {
                        DBAdapter = (DBAdapter.IDBAdapter)Instance.CreateInstance(dbAdapterType);
                    }
                }

                //Create db table
                if (DBAdapter != null)
                {
                    DBAdapter.Table = table;
                    DBAdapter.Drop();
                    DBAdapter.Create();
                }
                else
                {
                    throw new DataException(string.Format("Can't find DBAdapterTypeName : {0}",
                        table.DBAdapterTypeName));
                }

                //Open delete provider
                _DelProvider.Open(_Directory);

                //Create payload information
                CreatePayloadInformation(table);

                //Create table directory and create table index files
                try
                {
                    table.Save(directory);
                }
                catch
                {
                    DBAdapter.Drop();
                    throw;
                }

                //Add to global configuration file
                Setting.AddTableConfig(directory, table.Name);
                Setting.Save();

                //set payload file name
                _PayloadFileName = Hubble.Framework.IO.Path.AppendDivision(directory, '\\') + "Payload.db";

                //Delete payload file
                if (System.IO.File.Exists(_PayloadFileName))
                {
                    System.IO.File.Delete(_PayloadFileName);
                }

                lock (this)
                {
                    //Reset last docid
                    _LastDocId = 0;
                    _Table = table;

                    //Start payload message queue
                    if (_PayloadFile == null)
                    {
                        _PayloadFile = new Hubble.Core.Store.PayloadFile(_PayloadFileName);
                        
                        //Build Payload file head
                        _PayloadFile.Create(table.Fields);
                    }
                }

                //Start payload file
                _PayloadFile.Start();

                //Add field inverted index 
                foreach (Field field in table.Fields)
                {
                    if (field.IndexType == Field.Index.Tokenized)
                    {
                        InvertedIndex invertedIndex = new InvertedIndex(directory, field.Name.Trim(), field.TabIndex, field.Mode, true, this);
                        AddFieldInvertedIndex(field.Name, invertedIndex);
                    }

                    AddFieldIndex(field.Name, field);
                }
            }

            if (_QueryCache == null)
            {
                _QueryCache = new Cache.QueryCache(Cache.QueryCacheManager.Manager, this.TableName);
            }
        }

        internal int GetTabIndex(string fieldName)
        {
            return GetField(fieldName).TabIndex;
        }

        #endregion

        #region public methods
        public void Insert(List<Document> docs)
        {
            try
            {
                _TableLock.Enter(Lock.Mode.Share, 30000);

                lock (_ExitLock)
                {
                    if (_NeedExit)
                    {
                        return;
                    }
                    else
                    {
                        _BusyCount++;
                    }
                }

                lock (this)
                {
                    List<Field> fields = GetAllFields();

                    foreach (Document doc in docs)
                    {
                        if (IndexOnly)
                        {
                            if (_LastDocId < doc.DocId)
                            {
                                throw new DataException("Must insert DocId sort ascending!");
                            }

                            _LastDocId = doc.DocId;
                        }
                        else
                        {
                            doc.DocId = _LastDocId++;
                        }

                        Dictionary<string, FieldValue> notnullFields = new Dictionary<string, FieldValue>();

                        foreach (FieldValue fValue in doc.FieldValues)
                        {
                            Field field = GetField(fValue.FieldName);

                            if (field == null)
                            {
                                throw new DataException(string.Format("Field:{0} is not in the table {1}",
                                    fValue.FieldName, _Table.Name));
                            }

                            fValue.Type = field.DataType;

                            if (fValue.Value != null)
                            {
                                if (notnullFields.ContainsKey(fValue.FieldName.ToLower().Trim()))
                                {
                                    throw new DataException(string.Format("Field:{0} in table {1} repeats in one insert sentence",
                                        fValue.FieldName, _Table.Name));
                                }

                                notnullFields.Add(fValue.FieldName.ToLower().Trim(), fValue);
                            }
                        }

                        foreach (Field field in fields)
                        {
                            if (!notnullFields.ContainsKey(field.Name.ToLower().Trim()))
                            {
                                if (!field.CanNull)
                                {
                                    throw new DataException(string.Format("Field:{0} in table {1}. Can't be null!",
                                        field.Name, _Table.Name));
                                }

                                if (field.DefaultValue != null)
                                {
                                    FieldValue fv = new FieldValue(field.Name, field.DefaultValue);
                                    fv.Type = field.DataType;
                                }
                                else
                                {
                                    if (field.IndexType != Field.Index.None)
                                    {
                                        throw new DataException(string.Format("Field:{0} in table {1}. Can't insert null value on the field that is indexed!",
                                            field.Name, _Table.Name));
                                    }
                                }
                            }
                        }

                    }
                }

                _DBAdapter.Insert(docs);

                foreach (Document doc in docs)
                {
                    int[] data = new int[_PayloadLength];

                    long docId = doc.DocId;

                    foreach (FieldValue fValue in doc.FieldValues)
                    {
                        Field field = GetField(fValue.FieldName);

                        int[] fieldPayload;

                        switch (field.IndexType)
                        {
                            case Field.Index.Untokenized:
                                if (field.DataType == DataType.Varchar ||
                                    field.DataType == DataType.NVarchar ||
                                    field.DataType == DataType.Char ||
                                    field.DataType == DataType.NChar)
                                {
                                    if (fValue.Value.Length > field.DataLength)
                                    {
                                        throw new DataException(string.Format("Field:{0} string length is {1} large then {2}",
                                            fValue.FieldName, fValue.Value.Length, field.DataLength));
                                    }
                                }

                                fieldPayload = DataTypeConvert.GetData(field.DataType, field.DataLength, fValue.Value);

                                Array.Copy(fieldPayload, 0, data, field.TabIndex, fieldPayload.Length);

                                break;
                            case Field.Index.Tokenized:

                                InvertedIndex iIndex = GetInvertedIndex(field.Name);

                                int count = iIndex.Index(fValue.Value, docId, field.GetAnalyzer());

                                data[field.TabIndex] = count;

                                break;
                        }
                    }

                    lock (this)
                    {
                        _InsertCount++;

                        if (_InsertCount > _Table.ForceCollectCount)
                        {
                            _PayloadFile.Collect();
                            _InsertCount = 0;
                        }
                    }

                    Payload payload = new Payload(data);

                    lock (this)
                    {
                        _DocPayload.Add(docId, payload);
                    }

                    _PayloadFile.Add(docId, payload);
                }
                                    
                Cache.CacheManager.InsertCount += docs.Count;
                LastModifyTicks = DateTime.Now.Ticks;
            }
            finally
            {
                lock (_ExitLock)
                {
                    _BusyCount--;
                }

                _TableLock.Leave();
            }

        }

        public void Update(IList<FieldValue> fieldValues, IList<Query.DocumentResult> docs)
        {
            try
            {

                _TableLock.Enter(Lock.Mode.Share, 30000);

                lock (_ExitLock)
                {
                    if (_NeedExit)
                    {
                        return;
                    }
                    else
                    {
                        _BusyCount++;
                    }
                }

                if (IndexOnly)
                {
                    throw new DataException("Can not update table that is index only.");
                }

                Debug.Assert(fieldValues != null && docs != null);


                if (fieldValues.Count <= 0)
                {
                    return;
                }

                bool needDel = false;

                Document doc = new Document();

                List<PayloadSegment> payloadSegment = new List<PayloadSegment>();

                foreach (FieldValue fv in fieldValues)
                {
                    string fieldNameKey = fv.FieldName.ToLower().Trim();
                    if (fieldNameKey == "docid")
                    {
                        throw new DataException("DocId field is fixed field, can't update it manually!");
                    }

                    Field field;

                    if (!_FieldIndex.TryGetValue(fieldNameKey, out field))
                    {
                        throw new DataException(string.Format("Invalid column name '{0}'", fv.FieldName));
                    }

                    if (field.IndexType == Field.Index.Tokenized)
                    {
                        needDel = true;
                    }

                    fv.Type = field.DataType;

                    if (field.Store)
                    {
                        doc.FieldValues.Add(fv);
                    }

                    if (field.IndexType == Field.Index.Untokenized)
                    {
                        int[] data = DataTypeConvert.GetData(field.DataType, field.DataLength, fv.Value);
                        payloadSegment.Add(new PayloadSegment(field.TabIndex, data));
                    }
                }

                if (docs.Count <= 0)
                {
                    return;
                }

                if (needDel)
                {
                    Dictionary<string, string> fieldNameValue = new Dictionary<string, string>();

                    foreach (FieldValue fv in fieldValues)
                    {
                        fieldNameValue.Add(fv.FieldName.ToLower().Trim(), fv.Value);
                    }

                    List<Field> selectFields = new List<Field>();

                    foreach (Field field in _FieldIndex.Values)
                    {
                        selectFields.Add(field);
                    }

                    List<Query.DocumentResult> doDocs = new List<Query.DocumentResult>();

                    int i = 0;
                    foreach (Query.DocumentResult dResult in docs)
                    {
                        //long docId = dResult.DocId;

                        doDocs.Add(dResult);

                        if (++i % 100 == 0)
                        {
                            List<Document> docResult = Query(selectFields, doDocs);

                            foreach (Document updatedoc in docResult)
                            {
                                foreach (FieldValue fv in updatedoc.FieldValues)
                                {
                                    string value;
                                    if (fieldNameValue.TryGetValue(fv.FieldName.ToLower().Trim(), out value))
                                    {
                                        fv.Value = value;
                                    }
                                }
                            }

                            Insert(docResult);
                            Delete(doDocs);
                            doDocs.Clear();
                        }
                    }

                    if (doDocs.Count > 0)
                    {
                        List<Document> docResult = Query(selectFields, doDocs);

                        foreach (Document updatedoc in docResult)
                        {
                            foreach (FieldValue fv in updatedoc.FieldValues)
                            {
                                string value;
                                if (fieldNameValue.TryGetValue(fv.FieldName.ToLower().Trim(), out value))
                                {
                                    fv.Value = value;
                                }
                            }
                        }

                        Insert(docResult);
                        Delete(doDocs);
                    }


                }
                else
                {
                    _DBAdapter.Update(doc, docs);

                    List<long> updateIds = new List<long>();
                    List<Payload> updatePayloads = new List<Payload>();

                    foreach (Query.DocumentResult docResult in docs)
                    {
                        long docId = docResult.DocId;
                        Payload payLoad;

                        lock (this)
                        {
                            if (_DocPayload.TryGetValue(docId, out payLoad))
                            {
                                foreach (PayloadSegment ps in payloadSegment)
                                {
                                    Array.Copy(ps.Data, 0, payLoad.Data, ps.TabIndex, ps.Data.Length);
                                }

                                updateIds.Add(docId);
                                updatePayloads.Add(payLoad.Clone());
                            }
                        }

                        if (updateIds.Count >= 1000)
                        {
                            _PayloadFile.Update(updateIds, updatePayloads);
                            updateIds.Clear();
                            updatePayloads.Clear();
                        }
                    }

                    if (updateIds.Count > 0)
                    {
                        _PayloadFile.Update(updateIds, updatePayloads);
                    }
                }

                Collect();
                LastModifyTicks = DateTime.Now.Ticks;
            }
            finally
            {
                lock (_ExitLock)
                {
                    _BusyCount--;
                }

                _TableLock.Leave();
            }

        }

        public void Delete(IList<long> docs)
        {
            try
            {
                _TableLock.Enter(Lock.Mode.Share, 30000);

                lock (_ExitLock)
                {
                    if (_NeedExit)
                    {
                        return;
                    }
                    else
                    {
                        _BusyCount++;
                    }
                }

                if (docs.Count <= 0)
                {
                    return;
                }

                if (!IndexOnly)
                {
                    _DBAdapter.Delete(docs);
                }

                _DelProvider.Delete(docs);
                LastModifyTicks = DateTime.Now.Ticks;
            }
            finally
            {
                lock (_ExitLock)
                {
                    _BusyCount--;
                }

                _TableLock.Leave();
            }

        }

        public void Delete(IList<Query.DocumentResult> docs)
        {
            if (docs.Count <= 0)
            {
                return;
            }

            List<long> docIds = new List<long>();

            foreach (Query.DocumentResult docResult in docs)
            {
                docIds.Add(docResult.DocId);
            }

            Delete(docIds);
        }

        public void Optimize()
        {
            try
            {
                _TableLock.Enter(Lock.Mode.Share, 30000);

                foreach (InvertedIndex iIndex in _FieldInvertedIndex.Values)
                {
                    iIndex.Optimize();
                }
            }
            finally
            {
                _TableLock.Leave();
            }
        }

        public void Optimize(OptimizationOption option)
        {
            try
            {
                _TableLock.Enter(Lock.Mode.Share, 30000);

                foreach (InvertedIndex iIndex in _FieldInvertedIndex.Values)
                {
                    iIndex.Optimize(option);
                }
            }
            finally
            {
                _TableLock.Leave();
            }
        }

        public void Collect()
        {
            foreach (InvertedIndex iIndex in _FieldInvertedIndex.Values)
            {
                iIndex.FinishIndex();
            }

            if (_PayloadFile != null)
            {
                _PayloadFile.Collect();
            }
        }

        internal void CloseInvertedIndex()
        {
            foreach (InvertedIndex iIndex in _FieldInvertedIndex.Values)
            {
                iIndex.Close();
            }
        }

        internal void Close()
        {
            lock (_ExitLock)
            {
                _NeedExit = true;
            }

            foreach (InvertedIndex iIndex in _FieldInvertedIndex.Values)
            {
                if (!iIndex.OptimizeStoped)
                {
                    iIndex.StopOptimize();
                    iIndex.StopIndexFileProxy();
                }
            }
        }

        public System.Data.DataSet Select(string sql, int first, int length, out int count)
        {
            try
            {
                _TableLock.Enter(Lock.Mode.Share, 30000);

                Hubble.Core.Query.IQuery query;
                count = 0;

                string[] words = sql.Split(new string[] { " " }, StringSplitOptions.None);

                if (words.Length <= 2)
                {
                    return null;
                }

                string fieldName = words[0];

                query = GetQuery(words[1]);

                if (query == null)
                {
                    throw new DataException(string.Format("Can't find the command: {0}", words[1]));
                }

                query.InvertedIndex = GetInvertedIndex(fieldName);

                List<Hubble.Core.Entity.WordInfo> queryWords = new List<Hubble.Core.Entity.WordInfo>();

                int position = 0;

                for (int i = 2; i < words.Length; i++)
                {
                    Hubble.Core.Entity.WordInfo wordInfo = new Hubble.Core.Entity.WordInfo(words[i], position);
                    queryWords.Add(wordInfo);
                    position += words.Length + 1;
                }

                query.QueryWords = queryWords;
                query.DBProvider = this;
                query.TabIndex = GetField(fieldName).TabIndex;

                Hubble.Core.Query.Searcher searcher = new Hubble.Core.Query.Searcher(query);
                SFQL.Parse.WhereDictionary<long, Query.DocumentResult> docRankTbl = searcher.Search();

                List<long> docs = new List<long>();

                if (docRankTbl != null)
                {
                    count = docRankTbl.Count;

                    foreach (Hubble.Core.Query.DocumentResult docResult in docRankTbl.Values)
                    {
                        docs.Add(docResult.DocId);

                        if (docs.Count > 10)
                        {
                            break;
                        }
                    }
                }

                List<Field> selectFields = new List<Field>();

                foreach (Field field in _FieldIndex.Values)
                {
                    selectFields.Add(field);
                }

                return new System.Data.DataSet();
            }
            finally
            {
                _TableLock.Leave();
            }
            //List<Query.DocumentResult> docResult = Query(selectFields, docs);

            //return Document.ToDataSet(selectFields, docResult);

            //foreach (Hubble.Core.Query.DocumentRank docRank in searcher.Get(first, length))
            //{
                
            //}

        }

        #endregion
    }
}
