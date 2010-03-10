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

    public delegate void DelegateWordUpdate(string word, List<DocumentPositionList> docList);

    /// <summary>
    /// This class helps to build and query 
    /// a inverted index
    /// </summary>
    public class InvertedIndex //: Cache.IManagedCache
    {
        #region WordIndex

        public class WordIndexReader
        {
            #region Private fields
            string _Word;

            WordDocumentsList _ListForReader = new WordDocumentsList();
            long _WordCountSum;
            int _RelDocCount;

            Data.DBProvider _DBProvider;
            
            //int _TabIndex;
            int _TotalDocs;

            //delete
            DeleteProvider _DelProvider = null;

            #endregion

            #region Private properties


            internal Store.WordDocumentsList WordDocList
            {
                get
                {
                    lock (this)
                    {
                        return _ListForReader;
                    }
                }
            }


            #endregion

            #region Public Properties

            /// <summary>
            /// Word
            /// </summary>
            public string Word
            {
                get
                {
                    lock (this)
                    {
                        return _Word;
                    }
                }
            }

            /// <summary>
            /// Total number of documents
            /// </summary>
            public int Count
            {
                get
                {
                    lock (this)
                    {
                        return _ListForReader.Count;
                    }
                }
            }

            /// <summary>
            /// if doc count > max return count
            /// this field return rel doc count
            /// </summary>
            public int RelDocCount
            {
                get
                {
                    return _RelDocCount;
                }
            }


            /// <summary>
            /// Total number of words 
            /// </summary>
            public long WordCount
            {
                get
                {
                    lock (this)
                    {
                        return _WordCountSum;
                    }
                }
            }

            public DocumentPositionList this[int index]
            {
                get
                {
                    lock (this)
                    {
                        return _ListForReader[index];
                    }
                }
            }

            public DocumentPositionList[] DocPositionBuf
            {
                get
                {
                    return _ListForReader.Buf;
                }
            }

            #endregion

            #region internal methods

            //internal void WordUpdate(string word, List<DocumentPositionList> docList)
            //{
            //    lock (this)
            //    {
            //        _ListForReader.AddRange(docList);
            //        UpdateDocScoreList();
            //    }
            //}

            internal WordIndexReader(string word, WordDocumentsList docList, int totalDocs, 
                Data.DBProvider dbProvider)
            {
                _Word = word;
                _ListForReader = docList;
                _DBProvider = dbProvider;
                _DelProvider = _DBProvider.DelProvider;
                //_TabIndex = tabIndex;
                _TotalDocs = totalDocs;
                _WordCountSum = _ListForReader.WordCountSum;
                _RelDocCount = _ListForReader.RelDocCount;

                //UpdateDocScoreList();
            }

            #endregion

            public override string ToString()
            {
                return Word;
            }

        }

        /// <summary>
        /// Index for every word for writer
        /// </summary>
        public class WordIndexWriter
        {
            #region Private fields
            string _Word;
            AppendList<int> _TempPositionList = new AppendList<int>();
            int _TempDocumentId;
            bool _Hit = false;

            List<DocumentPositionList> _ListForWriter = new List<DocumentPositionList>();

            object _WaitLock = new object();

            Data.Field.IndexMode _IndexMode;

            #endregion

            #region Private properties

            private List<DocumentPositionList> ListForWriter
            {
                get
                {
                    lock (this)
                    {
                        return _ListForWriter;
                    }
                }
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Word
            /// </summary>
            public string Word
            {
                get
                {
                    lock (_WaitLock)
                    {
                        return _Word;
                    }
                }
            }

            /// <summary>
            /// Total number of documents
            /// </summary>
            public int Count
            {
                get
                {
                    lock (this)
                    {
                        return ListForWriter.Count;
                    }
                }
            }

            public int ListForWirterCount
            {
                get
                {
                    lock (this)
                    {
                        return ListForWriter.Count;
                    }
                }
            }

            #endregion


            #region Private methods

            #endregion

            #region Public methods

            #endregion

            #region internal properties

            internal bool Hit
            {
                get
                {
                    lock (_WaitLock)
                    {
                        return _Hit;
                    }
                }

                set
                {
                    lock (_WaitLock)
                    {
                        _Hit = value;
                    }
                }
            }

            private int _TempRank;
            internal int TempRank
            {
                get
                {
                    lock (_WaitLock)
                    {
                        return _TempRank;
                    }
                }

                set
                {
                    lock (_WaitLock)
                    {
                        _TempRank = value;
                    }
                }
            }

            private int _TempTotalWordsInDoc;
            internal int TempTotalWordsInDoc
            {
                get
                {
                    lock (_WaitLock)
                    {
                        return _TempTotalWordsInDoc;
                    }
                }

                set
                {
                    lock (_WaitLock)
                    {
                        _TempTotalWordsInDoc = value;
                    }
                }
            }

            internal int TempDocumentId
            {
                get
                {
                    lock (_WaitLock)
                    {
                        return _TempDocumentId;
                    }
                }

                set
                {
                    lock (_WaitLock)
                    {
                        _TempDocumentId = value;
                    }
                }
            }

            #endregion

            #region internal methods
 
            internal List<DocumentPositionList> GetDocListForWriter()
            {
                lock (this)
                {
                    List<DocumentPositionList> listForWriter = _ListForWriter;
                    _ListForWriter = new List<DocumentPositionList>();
                    return listForWriter;
                }
            }

            internal void Wait(int docId)
            {
                while (Hit && TempDocumentId != docId)
                {
                    System.Threading.Thread.Sleep(1);
                }
            }

            internal void AddToTempPositionList(int position)
            {
                lock (_WaitLock)
                {
                    _TempPositionList.Add(position);
                }
            }

            internal WordIndexWriter(string word, Data.Field.IndexMode mode)
            {
                _Word = word;
                _IndexMode = mode;
            }

            /// <summary>
            /// Index one document into the _ListForWriter
            /// </summary>
            internal void Index()
            {
                lock (this)
                {
                    try
                    {
                        if (_IndexMode == Hubble.Core.Data.Field.IndexMode.Simple)
                        {
                            _ListForWriter.Add(new DocumentPositionList(TempDocumentId, _TempPositionList.Count,
                                DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc)));
                        }
                        else
                        {

                            //Check sorted
                            int last = -1;
                            bool needSort = false;
                            foreach (int position in _TempPositionList)
                            {
                                if (position < last)
                                {
                                    needSort = true;
                                    break;
                                }
                            }

                            if (needSort)
                            {
                                _TempPositionList.Sort();
                            }

                            if (_TempPositionList.Count > 0)
                            {
                                _ListForWriter.Add(new DocumentPositionList(TempDocumentId, 
                                    _TempPositionList.Count,
                                    DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc), 
                                    _TempPositionList[0]));
                            }
                            else
                            {
                                _ListForWriter.Add(new DocumentPositionList(TempDocumentId, _TempPositionList.Count, 
                                    DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc)));
                            }
                        }
                    }
                    finally
                    {
                        _TempPositionList.Clear();
                        Hit = false;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Private fields
        private string _FieldName;
        private Data.Field.IndexMode _IndexMode;
        //private int _TabIndex;
        private bool _Closed = false;
        //private Dictionary<string, WordIndexWriter> _WordTableNeedCollectDict = new Dictionary<string, WordIndexWriter>();
        //private List<IndexFile.DocInfo> _DocInfosNeedCollect = new List<IndexFile.DocInfo>();

        private Dictionary<string, WordIndexWriter> _WordTableWriter = new Dictionary<string, WordIndexWriter>();

        //private Dictionary<int, int> _DocumentWordCountTable = new Dictionary<int, int>();
        private int _DocumentCount; //Index documents count

        private Store.IndexFileProxy _IndexFileProxy;
        private bool _IndexFinished = false;

        private int _WriteCount = 0;

        private int _ForceCollectCount = 5000;

        private Data.DBProvider _DBProvider;

        private IndexMerge _IndexMerge = null;

        #endregion

        #region Private Properties

        private int WriteCount
        {
            get
            {
                lock (this)
                {
                    return _WriteCount;
                }
            }

            set
            {
                lock (this)
                {
                    _WriteCount = value;
                }
            }
        }

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

        private Dictionary<string, WordIndexWriter> WordTableWriter
        {
            get
            {
                lock (this)
                {
                    return _WordTableWriter;
                }
            }
        }

        #endregion

        #region Internal properties

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

        #endregion

        #region Private Metheds

        //private List<IndexFile.DocInfo> GetDocInfosNeedCollect()
        //{
        //    lock (this)
        //    {
        //        List<IndexFile.DocInfo> docInfos = _DocInfosNeedCollect;

        //        _DocInfosNeedCollect = new List<IndexFile.DocInfo>();

        //        return docInfos;
        //    }
        //}

        private void InitFileStore(string path, string fieldName, bool rebuild)
        {
            _IndexFileProxy = new IndexFileProxy(path, fieldName, rebuild, _IndexMode);
            //_IndexFileProxy.WordUpdateDelegate = WordUpdateDelegate;
        }

        private void StoreIndexToFile()
        {
            Dictionary<string, WordIndexWriter> tempWordTableWriter;

            lock (this)
            {
                if (_DBProvider.DelProvider != null)
                {
                    _DBProvider.DelProvider.IncDeleteStamp();
                }

                tempWordTableWriter = _WordTableWriter;
                _WordTableWriter = new Dictionary<string, WordIndexWriter>();
            }

            foreach (WordIndexWriter wordIndex in tempWordTableWriter.Values)
            {
                List<DocumentPositionList> docList = wordIndex.GetDocListForWriter();

                if (docList != null)
                {
                    _IndexFileProxy.AddWordPositionAndDocumentPositionList(wordIndex.Word, docList);
                }
            }

            tempWordTableWriter = null;

            //_IndexFileProxy.AddDocInfos(GetDocInfosNeedCollect());

            _IndexFileProxy.Collect();

            _IndexMerge.Optimize(OptimizationOption.Speedy);
        }

        //void WordUpdateDelegate(string word, List<DocumentPositionList> docList)
        //{
        //    WordIndexReader wr;

        //    lock (this)
        //    {
        //        if (!_WordTableReader.TryGetValue(word, out wr))
        //        {
        //            return;
        //        }
        //    }

        //    wr.WordUpdate(word, docList);
        //}

        #endregion

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

        #region Public Metheds

        internal void Close()
        {
            //Cache.CacheManager.UnRegister(this);

            Closed = true;

            StopOptimize();

            _IndexFileProxy.Close(2000);

            GC.Collect();

        }

        //public int GetDocumentWordCount(int docId)
        //{
        //    int count;

        //    if (_DocumentWordCountTable.TryGetValue(docId, out count))
        //    {
        //        return count;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}

        public WordIndexReader GetWordIndex(string word, bool orderByScore)
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
                    return _IndexFileProxy.GetWordIndex(new IndexFileProxy.GetInfo(word, DocumentCount, _DBProvider, _DBProvider.MaxReturnCount));
                }
                else
                {
                    return _IndexFileProxy.GetWordIndex(new IndexFileProxy.GetInfo(word, DocumentCount, _DBProvider, -1));
                }
            }
        }

        /// <summary>
        /// Index a text for one field
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="documentId">document id</param>
        /// <param name="analyzer">analyzer</param>
        internal int Index(string text, int documentId, Analysis.IAnalyzer analyzer)
        {
            List<WordIndexWriter> hitIndexes = new List<WordIndexWriter>(4192);
            int retCount = 0;

            lock (this)
            {
                int count = 0;
                _DocumentCount++;

                foreach (Entity.WordInfo wordInfo in analyzer.Tokenize(text))
                {
                    if (wordInfo.Position < 0)
                    {
                        continue;
                    }

                    WordIndexWriter wordIndex;

                    string internedWord = string.IsInterned(wordInfo.Word);

                    if (internedWord == null)
                    {
                        internedWord = wordInfo.Word;
                    }

                    if (WordTableWriter.TryGetValue(internedWord, out wordIndex))
                    {
                        //Wait Hit == false 
                        wordIndex.Wait(documentId);

                        if (!wordIndex.Hit)
                        {
                            hitIndexes.Add(wordIndex);
                            wordIndex.Hit = true;
                            wordIndex.TempDocumentId = documentId;
                            wordIndex.TempRank = wordInfo.Rank;
                            wordIndex.TempTotalWordsInDoc = analyzer.Count;
                        }
                    }
                    else
                    {
                        //wordIndex = new WordIndexWriter(wordInfo.Word, FileName == null);
                        wordIndex = new WordIndexWriter(wordInfo.Word, _IndexMode);

                        hitIndexes.Add(wordIndex);
                        wordIndex.Hit = true;
                        wordIndex.TempDocumentId = documentId;
                        wordIndex.TempRank = wordInfo.Rank;
                        wordIndex.TempTotalWordsInDoc = analyzer.Count;

                        WordTableWriter.Add(wordInfo.Word, wordIndex);
                    }

                    wordIndex.AddToTempPositionList(wordInfo.Position);
                    count++;

                }

                //_DocumentWordCountTable[documentId] = count;
                retCount = count;
                //_DocInfosNeedCollect.Add(new IndexFile.DocInfo(

                foreach (WordIndexWriter wordIndex in hitIndexes)
                {
                    wordIndex.Index();
                    wordIndex.Hit = false;

                    //if (!_WordTableNeedCollectDict.ContainsKey(wordIndex.Word))
                    //{
                    //    _WordTableNeedCollectDict.Add(wordIndex.Word, wordIndex);
                    //}
                }


                if (WriteCount >= ForceCollectCount)
                {
                    WriteCount = 0;
                    StoreIndexToFile();
                }
                else
                {
                    WriteCount++;
                }

                return retCount;
            }
        }

        public void FinishIndex()
        {
            WriteCount = 0;
            StoreIndexToFile();
        }

        public void Optimize()
        {
            if (_IndexMerge != null)
            {
                _IndexMerge.Optimize();
            }
        }

        public void Optimize(OptimizationOption option)
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
