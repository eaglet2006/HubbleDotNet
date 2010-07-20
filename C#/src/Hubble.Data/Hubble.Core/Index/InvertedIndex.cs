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
using Hubble.Core.Entity;
using Hubble.Core.Store;
using Hubble.Core.Data;
using Hubble.Framework.DataStructure;
using Hubble.Framework.IO;
using Hubble.Core.SFQL.Parse;

namespace Hubble.Core.Index
{

    /// <summary>
    /// This class helps to build and query 
    /// a inverted index
    /// </summary>
    public class InvertedIndex //: Cache.IManagedCache
    {
        #region WordIndex


        #endregion

        #region Private fields

        private string _FieldName;
        private Data.Field.IndexMode _IndexMode;
        private bool _Closed = false;

        private Dictionary<string, int> _WordTableWriter = null;
        private AppendList<int> _TempWordIndexWriter = null;
        private WordIndexWriter[] _WordIndexWriterPool = null;
        private int _IndexWriterPoolId = 0;
        private DocumentPositionAlloc _DocPositionAlloc = null;

        private int _DocumentCount; //Index documents count

        private Store.IndexFileProxy _IndexFileProxy;
        private bool _IndexFinished = false;

        private int _ForceCollectCount = 5000;

        private Data.DBProvider _DBProvider;

        private IndexMerge _IndexMerge = null;

        #endregion

        #region Private Properties

        private bool IndexFinished
        {
            get
            {
                lock (this)
                {
                    bool retVal = _IndexFinished;
                    _IndexFinished = false;
                    return retVal;
                }
            }
        }

        private bool Closed
        {
            get
            {
                lock (this)
                {
                    return _Closed;
                }
            }

            set
            {
                lock (this)
                {
                    _Closed = value;
                }
            }
        }

        #endregion

        #region Internal properties

        internal bool TooManyIndexFiles
        {
            get
            {
                return _IndexFileProxy.TooManyIndexFiles();
            }
        }

        internal double MergeRate
        {
            get
            {
                return _IndexFileProxy.MergeProgress;
            }
        }

        internal bool OptimizeStoped
        {
            get
            {
                if (_IndexMerge != null)
                {
                    return _IndexMerge.CanClose;
                }

                return true;
            }
        }

        internal bool IndexStoped
        {
            get
            {
                return _IndexFileProxy.CanClose;
            }
        }

        #endregion

        #region Public properties

        public string FieldName
        {
            get
            {
                return _FieldName;
            }
        }

        public Data.Field.IndexMode IndexMode
        {
            get
            {
                return _IndexMode;
            }
        }

        public int ForceCollectCount
        {
            get
            {
                return _ForceCollectCount;
            }

            set
            {
                if (value <= 0)
                {
                    _ForceCollectCount = 1;
                }
                else
                {
                    _ForceCollectCount = value;
                }
            }
        }

        public int WordTableSize
        {
            get
            {
                return _IndexFileProxy.WordTableSize; 
            }
        }

        /// <summary>
        /// Index documents count
        /// </summary>
        public int DocumentCount
        {
            get
            {
                return _DocumentCount;
            }
        }

        internal string LastDDXFilePath
        {
            get
            {
                return _IndexFileProxy.LastDDXFilePath;
            }
        }


        //internal string LastHeadFilePath
        //{
        //    get
        //    {
        //        return _IndexFileProxy.LastHeadFilePath;
        //    }
        //}

        internal string LastIndexFilePath
        {
            get
            {
                return _IndexFileProxy.LastIndexFilePath;
            }
        }

        internal bool CanMerge
        {
            get
            {
                return _IndexFileProxy.CanMerge;
            }

            set
            {
                _IndexFileProxy.CanMerge = value;
            }
        }

        #endregion

        #region Private Metheds

        private void InitFileStore(string path, string fieldName, bool rebuild)
        {
            _IndexFileProxy = new IndexFileProxy(path, fieldName, rebuild, _IndexMode, _DBProvider);
        }

        private void StoreIndexToFile()
        {
            lock (this)
            {
                if (_DBProvider.DelProvider != null)
                {
                    _DBProvider.DelProvider.IncDeleteStamp();
                }

            }

            if (_WordTableWriter == null)
            {
                return;
            }

            int wordIndexWriterCount = _WordTableWriter.Count;

            if (wordIndexWriterCount <= 0)
            {
                return;
            }

            Array.Sort(_WordIndexWriterPool, 0, wordIndexWriterCount);

            for (int index = 0; index < wordIndexWriterCount; index++)
            {
                IEnumerable<DocumentPositionList> docList = _WordIndexWriterPool[index].GetDocListForWriter();

                if (docList != null)
                {
                    _IndexFileProxy.AddWordPositionAndDocumentPositionList(
                        _WordIndexWriterPool[index].Word, _WordIndexWriterPool[index].GetFirstDocList(), 
                        _WordIndexWriterPool[index].Count, docList);
                }
            }

            _WordTableWriter = null;
            _WordIndexWriterPool = null;
            _IndexWriterPoolId = 0;
            _TempWordIndexWriter = null;
            _DocPositionAlloc = null;

            _IndexFileProxy.Collect();

            _IndexMerge.Optimize(OptimizationOption.Speedy);
        }

        #endregion

        #region Constructor

        internal InvertedIndex(string path, string fieldName, int tabIndex,
            Data.Field.IndexMode mode, bool rebuild, Data.DBProvider dbProvider, int documnetCount)
        {
            _FieldName = fieldName;
            //_TabIndex = tabIndex;
            _IndexMode = mode;
            _DBProvider = dbProvider;
            InitFileStore(path, fieldName, rebuild);
            //Cache.CacheManager.Register(this);
            _IndexMerge = new IndexMerge(path, _IndexFileProxy);
            _DocumentCount = documnetCount;

            //InitCollectThread();
        }

        #endregion

        #region internal Metheds


        internal void Close()
        {
            //Cache.CacheManager.UnRegister(this);

            Closed = true;

            StopOptimize();

            _IndexFileProxy.Close(2000);

            GC.Collect();

        }

        internal void Index(IList<Document> docs, int fieldIndex)
        {
            int orginalFieldIndex = fieldIndex;
            Field field = _DBProvider.GetField(this.FieldName);

            foreach (Document doc in docs)
            {
                if (doc.FieldValues[fieldIndex].FieldName.Trim().ToLower() != FieldName.Trim().ToLower())
                {
                    //Field index does not match, find new field index again.
                    //Happen as input different field order of rows
                    fieldIndex = 0;

                    foreach (FieldValue fValue in doc.FieldValues)
                    {
                        if (doc.FieldValues[fieldIndex].FieldName.Trim().ToLower() == FieldName.Trim().ToLower())
                        {
                            break;
                        }

                        fieldIndex++;
                    }

                    if (fieldIndex >= doc.FieldValues.Count)
                    {
                        //no data of this field in this row
                        fieldIndex = orginalFieldIndex;
                        continue;
                    }
                }

                this.Index(doc.FieldValues[fieldIndex].Value, doc.DocId, field.GetAnalyzer());
            }
        }

        /// <summary>
        /// Index a text for one field
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="documentId">document id</param>
        /// <param name="analyzer">analyzer</param>
        private void Index(string text, int documentId, Analysis.IAnalyzer analyzer)
        {
            lock (this)
            {
                if (_WordTableWriter == null)
                {
                    _WordTableWriter = new Dictionary<string, int>(65536);
                }

                if (_DocPositionAlloc == null)
                {
                    _DocPositionAlloc = new DocumentPositionAlloc();
                }

                _DocumentCount++;
                if (_TempWordIndexWriter == null)
                {
                    _TempWordIndexWriter = new AppendList<int>(65536);
                }

                _TempWordIndexWriter.Clear();

                foreach (Entity.WordInfo wordInfo in analyzer.Tokenize(text))
                {
                    if (wordInfo.Position < 0)
                    {
                        continue;
                    }

                    string internedWord = string.IsInterned(wordInfo.Word);

                    if (internedWord == null)
                    {
                        internedWord = wordInfo.Word;
                    }

                    int index;

                    if (!_WordTableWriter.TryGetValue(internedWord, out index))
                    {
                        if (_WordIndexWriterPool == null)
                        {
                            _WordIndexWriterPool = new WordIndexWriter[65536]; 
                        }

                        if (_IndexWriterPoolId >= _WordIndexWriterPool.Length)
                        {
                            int nextLength = _WordIndexWriterPool.Length * 2;

                            WordIndexWriter[] tempPool = new WordIndexWriter[nextLength];
                            Array.Copy(_WordIndexWriterPool, tempPool, _WordIndexWriterPool.Length);
                            _WordIndexWriterPool = tempPool;
                        }

                        _WordIndexWriterPool[_IndexWriterPoolId] = new WordIndexWriter(wordInfo.Word, _IndexMode, _DocPositionAlloc);
                        _WordIndexWriterPool[_IndexWriterPoolId].TempDocId = documentId;
                        _WordIndexWriterPool[_IndexWriterPoolId].TempWordCountInThisDoc = 0;
                        _WordIndexWriterPool[_IndexWriterPoolId].TempFirstPosition = wordInfo.Position;
                        _WordIndexWriterPool[_IndexWriterPoolId].TempTotalWordsInDoc = analyzer.Count;

                        _WordTableWriter.Add(wordInfo.Word, _IndexWriterPoolId);

                        _TempWordIndexWriter.Add(_IndexWriterPoolId);
                        index = _IndexWriterPoolId;
                        _IndexWriterPoolId++;
                    }

                    if (_WordIndexWriterPool[index].TempDocId != documentId)
                    {
                        _WordIndexWriterPool[index].TempDocId = documentId;
                        _WordIndexWriterPool[index].TempWordCountInThisDoc = 1;
                        _WordIndexWriterPool[index].TempFirstPosition = wordInfo.Position;
                        _WordIndexWriterPool[index].TempTotalWordsInDoc = analyzer.Count;
                        _TempWordIndexWriter.Add(index);
                    }
                    else
                    {
                        if (_WordIndexWriterPool[index].TempFirstPosition > wordInfo.Position)
                        {
                            _WordIndexWriterPool[index].TempFirstPosition = wordInfo.Position;
                        }

                        _WordIndexWriterPool[index].TempWordCountInThisDoc++;
                    }
                }

                foreach (int writeId in _TempWordIndexWriter)
                {
                    _WordIndexWriterPool[writeId].Index();
                }
            }
        }

        internal void FinishIndex()
        {
            //WriteCount = 0;
            StoreIndexToFile();
        }

        internal void Optimize()
        {
            if (_IndexMerge != null)
            {
                _IndexMerge.Optimize();
            }
        }

        internal void Optimize(OptimizationOption option)
        {
            if (_IndexMerge != null)
            {
                _IndexMerge.Optimize(option);
            }
        }


        internal void StopOptimize()
        {
            if (_IndexMerge != null)
            {
                _IndexMerge.Close();
            }
        }

        internal void StopIndexFileProxy()
        {
            _IndexFileProxy.SafelyClose();
        }

        #endregion


        #region Public Metheds

        public WordIndexReader GetWordIndex(string word, bool orderByScore)
        {
            return GetWordIndex(word, orderByScore, false);
        }

        public WordIndexReader GetWordIndex(string word, bool orderByScore, bool onlyStepDocIndex)
        {
            lock (this)
            {
                //WordIndexReader retVal;

                //if (_WordTableReader.TryGetValue(word, out retVal))
                //{
                //    retVal.Hit();
                //    return retVal;
                //}

                if (orderByScore)
                {
                    return _IndexFileProxy.GetWordIndex(new IndexFileProxy.GetInfo(word, DocumentCount, _DBProvider, _DBProvider.MaxReturnCount), onlyStepDocIndex);
                }
                else
                {
                    return _IndexFileProxy.GetWordIndex(new IndexFileProxy.GetInfo(word, DocumentCount, _DBProvider, -1), onlyStepDocIndex);
                }
            }
        }

        public List<string> InnerLike(string str, InnerLikeType type)
        {
            return _IndexFileProxy.InnerLike(str, type);
        }


        #endregion

        //#region IManagedCache Members

        //public long CacheSize
        //{
        //    get 
        //    {
        //        lock (this)
        //        {
        //            long size = 0;

        //            foreach (WordIndexReader ir in _WordTableReader.Values)
        //            {
        //                size += ir.MemorySize;
        //            }

        //            return size;
        //        }
        //     }
        //}

        //public void ReduceMemory(int percentage)
        //{
        //    lock (this)
        //    {
        //        if (_WordTableReader.Count <= 0)
        //        {
        //            return;
        //        }

        //        int count = _WordTableReader.Count * percentage / 100;

        //        if (count == 0 && _WordTableReader.Count < 10)
        //        {
        //            _WordTableReader.Clear();
        //            return;
        //        }
                
        //        List<WordIndexReader> indexReaderList = new List<WordIndexReader>();


        //        foreach (WordIndexReader ir in _WordTableReader.Values)
        //        {
        //            indexReaderList.Add(ir);
        //        }

        //        indexReaderList.Sort();

        //        for (int i = 0; i < count; i++)
        //        {
        //            _WordTableReader.Remove(indexReaderList[i].Word);
        //        }
        //    }
        //}

        //#endregion
    }
}
