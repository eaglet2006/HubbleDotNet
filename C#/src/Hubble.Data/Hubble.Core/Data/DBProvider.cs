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
using Hubble.Core.Index;
using Hubble.Core.Global;
using Hubble.Framework.Reflection;

namespace Hubble.Core.Data
{
    public class DBProvider
    {
        #region Static members
        static Dictionary<string, DBProvider> _DBProviderTable = new Dictionary<string, DBProvider>();
        static object _LockObj = new object();
        static object _sLockObj = new object();

        static Dictionary<string, Type> _QueryTable = new Dictionary<string, Type>();

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

        private static Hubble.Core.Query.IQuery GetQuery(string command)
        {
            Type type;

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

        public static DBProvider GetDBProvider(string tableName)
        {
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
            lock (_sLockObj)
            {
                return _DBProviderTable.ContainsKey(tableName.ToLower().Trim());
            }
        }

        public static void NewDBProvider(string tableName, DBProvider dbProvider)
        {
            lock (_sLockObj)
            {
                _DBProviderTable.Add(tableName.ToLower().Trim(), dbProvider);
            }
        }

        public static void Drop(string tableName)
        {
            DBProvider dbProvider = GetDBProvider(tableName);

            if (dbProvider != null)
            {
                string dir = dbProvider.Directory;
                dbProvider.Drop();
                DeleteTableName(tableName);

                Global.Setting.RemoveTableConfig(dir);
                Global.Setting.Save();
            }
        }

        public static void Init()
        {
            //Build QueryTable

            foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.GetInterface("Hubble.Core.Query.IQuery") != null)
                    {
                        Hubble.Core.Query.IQuery iQuery = asm.CreateInstance(type.FullName) as 
                            Hubble.Core.Query.IQuery;

                        _QueryTable.Add(iQuery.Command.ToLower().Trim(), type);
                    }
                }
            }

            List<string> removeDirs = new List<string>();

            foreach (TableConfig tc in Setting.Config.Tables)
            {
                try
                {
                    DBProvider dbProvider = InitTable(tc);
                    _DBProviderTable.Add(dbProvider.TableName.ToLower().Trim(), dbProvider);
                }
                catch (Exception e)
                {
                    removeDirs.Add(tc.Directory);

                    Global.Report.WriteErrorLog(string.Format("Init Table at {0} fail, errmsg:{1} stack {2}",
                        tc.Directory, e.Message, e.StackTrace));
                }
            }

            foreach (string removeDir in removeDirs)
            {
                Setting.RemoveTableConfig(removeDir);
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

        public string TableName
        {
            get
            {
                return _Table.Name;
            }
        }

        public int PayloadLength
        {
            get
            {
                return _PayloadLength;
            }
        }

        #endregion

        #region Private methods

        private void AddFieldInvertedIndex(string fieldName, InvertedIndex index)
        {
            lock (this)
            {
                index.ForceCollectCount = _Table.ForceCollectCount;
                _FieldInvertedIndex.Add(fieldName.Trim().ToLower(), index);
            }
        }

        private InvertedIndex GetInvertedIndex(string fieldName)
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

        private Field GetField(string fieldName)
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

            foreach (string file in System.IO.Directory.GetFiles(dir, "tableinfo.xml"))
            {
                System.IO.File.Delete(file);
            }

            ClearAll();

        }

        private void Open(string directory)
        {
            _Directory = directory;

            _Table = Table.Load(directory);
            Table table = _Table;
            long dbMaxDocId = 0;

            lock (_LockObj)
            {
                //Get DB adapter
                if (!string.IsNullOrEmpty(table.DBAdapterTypeName))
                {
                    DBAdapter = (DBAdapter.IDBAdapter)Instance.CreateInstance(table.DBAdapterTypeName);
                }

                //init db table
                if (DBAdapter != null)
                {
                    DBAdapter.Table = table;
                    dbMaxDocId = DBAdapter.MaxDocId;
                }
                else
                {
                    throw new DataException(string.Format("Can't find DBAdapterTypeName : {0}",
                        table.DBAdapterTypeName));
                }

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

        }

        #endregion

        #region internal methods

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


        /// <summary>
        /// Create a table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="directory"></param>
        internal void Create(Table table, string directory)
        {
            lock (_LockObj)
            {
                _Directory = directory;

                //Get DB adapter
                if (!string.IsNullOrEmpty(table.DBAdapterTypeName))
                {
                    DBAdapter = (DBAdapter.IDBAdapter)Instance.CreateInstance(table.DBAdapterTypeName);
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
                Setting.Config.Tables.Add(new TableConfig(directory));
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
        }

        internal int GetTabIndex(string fieldName)
        {
            return GetField(fieldName).TabIndex;
        }

        #endregion

        #region public methods
        public void Insert(List<Document> docs)
        {
            lock (this)
            {
                foreach (Document doc in docs)
                {
                    doc.DocId = _LastDocId++;
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

                    if (field == null)
                    {
                        throw new DataException(string.Format("Field:{0} is not in the table {1}",
                            fValue.FieldName, _Table.Name));
                    }

                    int[] fieldPayload;

                    switch (field.IndexType)
                    {
                        case Field.Index.Untokenized:
                            if (field.DataType == DataType.String)
                            {
                                if (fValue.Value.Length > field.DataLength)
                                {
                                    throw new DataException(string.Format("Field:{0} string length is {1} large then {2}",
                                        fValue.FieldName, fValue.Value.Length, field.DataLength));
                                }
                            }

                            fieldPayload = DataTypeConvert.GetData(field.DataType, fValue.Value);

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
                    _InsertCount ++;

                    if (_InsertCount > _Table.ForceCollectCount)
                    {
                        _PayloadFile.Collect();
                        _InsertCount = 0;
                    }
                }

                Payload payload = new Payload(data);
                _DocPayload.Add(docId, payload);
                _PayloadFile.Add(docId, payload);
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

        public System.Data.DataSet Select(string sql, int first, int length, out int count)
        {
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

            for(int i = 2; i < words.Length; i++)
            {
                Hubble.Core.Entity.WordInfo wordInfo = new Hubble.Core.Entity.WordInfo(words[i], position);
                queryWords.Add(wordInfo);
                position += words.Length + 1;
            }

            query.QueryWords = queryWords;
            query.DBProvider = this;
            query.TabIndex = GetField(fieldName).TabIndex;

            Hubble.Core.Query.Searcher searcher = new Hubble.Core.Query.Searcher(query);
            count = searcher.Search();

            //foreach (Hubble.Core.Query.DocumentRank docRank in searcher.Get(first, length))
            //{
                
            //}

            return null;
        }

        #endregion
    }
}
