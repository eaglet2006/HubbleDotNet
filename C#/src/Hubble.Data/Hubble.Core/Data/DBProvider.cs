﻿/*
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

        internal DeleteProvider DelProvider
        {
            get
            {
                return _DelProvider;
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

            foreach (string file in System.IO.Directory.GetFiles(dir, "Delete.db"))
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
            _DelProvider = new DeleteProvider();

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

        }

        private List<Document> Query(List<Field> selectFields, IList<long> docs)
        {
            List<Field> dbFields = new List<Field>();
            Dictionary<long, Document> docIdToDocumnet = null;

            List<Document> result;

            result = new List<Document>(docs.Count);

            foreach (Field field in selectFields)
            {
                if (field.Store && field.IndexType != Field.Index.Untokenized)
                {
                    dbFields.Add(field);
                }
            }

            if (dbFields.Count > 0)
            {
                docIdToDocumnet = new Dictionary<long,Document>();
            }

            foreach (long docId in docs)
            {
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
                    }

                    doc.Add(field.Name, value, field.DataType, 
                        field.Store && field.IndexType != Field.Index.Untokenized);

                    
                }
            }

            if (dbFields.Count > 0)
            {
                dbFields.Add(new Field("DocId", DataType.Int64));
                System.Data.DataTable dt = _DBAdapter.Query(dbFields, docs);

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
                _DelProvider = new DeleteProvider();

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
                    _InsertCount ++;

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
        }

        public void Update(IList<FieldValue> fieldValues, IList<long> docs)
        {
            Debug.Assert(fieldValues != null && docs != null);

            if (docs.Count <= 0)
            {
                return;
            }

            if (fieldValues.Count <= 0)
            {
                return;
            }

            bool needDel = false;

            Document doc = new Document();

            List<PayloadSegment> payloadSegment = new List<PayloadSegment>();

            foreach (FieldValue fv in fieldValues)
            {
                Field field = _FieldIndex[fv.FieldName.ToLower().Trim()];

                if (field.IndexType == Field.Index.Tokenized)
                {
                    needDel = true;
                }

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

            if (needDel)
            {
                List<Field> selectFields = new List<Field>();

                foreach (Field field in _FieldIndex.Values)
                {
                    selectFields.Add(field);
                }

                List<long> doDocs = new List<long>();

                int i = 0;
                foreach (long docId in docs)
                {
                    doDocs.Add(docId);

                    if (++i % 100 == 0)
                    {
                        List<Document> docResult = Query(selectFields, doDocs);
                        Insert(docResult);
                        Delete(doDocs);
                        doDocs.Clear();
                    }
                }

                if (doDocs.Count > 0)
                {
                    List<Document> docResult = Query(selectFields, doDocs);
                    Insert(docResult);
                    Delete(doDocs);
                }

            }
            else
            {
                _DBAdapter.Update(doc, docs);
                
                List<long> updateIds = new List<long>();
                List<Payload> updatePayloads = new List<Payload>(); 

                foreach (long docId in docs)
                {
                    Payload payLoad;

                    lock (this)
                    {
                        if (_DocPayload.TryGetValue(docId, out payLoad))
                        {
                            foreach (PayloadSegment ps in payloadSegment)
                            {
                                Array.Copy(ps.Data, ps.TabIndex, payLoad.Data, ps.TabIndex, ps.Data.Length);
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
        }


        public void Delete(IList<long> docs)
        {
            _DBAdapter.Delete(docs);

            _DelProvider.Delete(docs);
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
            Dictionary<long, Query.DocumentRank> docRankTbl = searcher.Search();

            List<long> docs = new List<long>();

            if (docRankTbl != null)
            {
                foreach (Hubble.Core.Query.DocumentRank docRank in docRankTbl.Values)
                {
                    docs.Add(docRank.DocumentId);

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

            List<Document> docResult = Query(selectFields, docs);

            return Document.ToDataSet(selectFields, docResult);

            //foreach (Hubble.Core.Query.DocumentRank docRank in searcher.Get(first, length))
            //{
                
            //}

        }

        #endregion
    }
}