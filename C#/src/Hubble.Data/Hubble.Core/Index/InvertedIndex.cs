using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Hubble.Core.Entity;
using Hubble.Core.Store;
using Hubble.Core.Data;
using Hubble.Framework.DataStructure;
using Hubble.Framework.IO;

namespace Hubble.Core.Index
{

    public delegate void DelegateWordUpdate(string word, List<DocumentPositionList> docList);

    /// <summary>
    /// This class helps to build and query 
    /// a inverted index
    /// </summary>
    public class InvertedIndex : Cache.IManagedCache
    {
        #region WordIndex

        public class WordIndexReader : IComparable<WordIndexReader>
        {
            public struct DocScore
            {
                public long DocId;
                public long Score;

                public DocScore(long docId, long score)
                {
                    DocId = docId;
                    Score = score;
                }
            }

            #region Private fields
            string _Word;

            DocScore[] _DocScoreList;

            List<DocumentPositionList> _ListForReader = new List<DocumentPositionList>();
            long _WordCount;

            Data.DBProvider _DBProvider;
            int _TabIndex;
            long _TotalDocs;

            long _HitCount = 0;

            //delete
            DeleteProvider _DelProvider = null;
            int _DeleteStamp = -1;

            #endregion

            #region Private properties

            private long HitCount
            {
                get
                {
                    lock (this)
                    {
                        return _HitCount;
                    }
                }
            }

            private List<DocumentPositionList> ListForReader
            {
                get
                {
                    lock (this)
                    {
                        int deleStamp = _DelProvider.DeleteStamp;
                        if (deleStamp != _DeleteStamp)
                        {
                            _ListForReader = _DelProvider.GetDocumentPositionList(_ListForReader);
                            _DeleteStamp = deleStamp;
                        }

                        return _ListForReader;
                    }
                }
            }

            #endregion

            #region Public Properties

            public long MemorySize
            {
                get
                {
                    lock (this)
                    {
                        long size = 0;
                        foreach (DocumentPositionList docPList in _ListForReader)
                        {
                            size += docPList.Size;
                        }

                        return _DocScoreList.Length * (sizeof(long)*2) + size;
                    }
                }
            }

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
            /// Total number of words 
            /// </summary>
            public long WordCount
            {
                get
                {
                    lock (this)
                    {
                        return _WordCount;
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


            #endregion


            #region Private methods

            private void UpdateDocScoreList()
            {
                _WordCount = 0;
                _DocScoreList = new DocScore[_ListForReader.Count];

                int _Norm_d_t;
                int _Idf_t;

                foreach (DocumentPositionList dList in _ListForReader)
                {
                    _WordCount += dList.Count;
                }

                _Norm_d_t = (int)Math.Sqrt(_WordCount);
                _Idf_t = (int)Math.Log10((double)_TotalDocs / (double)_WordCount + 1) + 1;

                int i = 0;

                foreach (DocumentPositionList dList in _ListForReader)
                {
                    int numDocWords = _DBProvider.GetDocWordsCount(dList.DocumentId, _TabIndex);

                    int docRank = 1;
                    if (dList.Rank > 0)
                    {
                        docRank = dList.Rank;
                    }

                    long score = docRank * _Idf_t * dList.Count * 1000000 / (_Norm_d_t * numDocWords);

                    _DocScoreList[i] = new DocScore(dList.DocumentId, score);

                    i++;
                }
            }


            #endregion

            #region Public methods

            public void Hit()
            {
                lock (this)
                {
                    _HitCount++;
                }
            }

            public long GetDocumentId(int index)
            {
                lock (this)
                {
                    return _ListForReader[index].DocumentId;
                }
            }

            public long GetHitCount(int index)
            {
                lock (this)
                {
                    return _ListForReader[index].Count;
                }
            }

            public void Calculate(Dictionary<long, Query.DocumentRank> docIdRank, int wordRank, long Norm_Ranks)
            {
                lock (this)
                {
                    int deleStamp = _DelProvider.DeleteStamp;
                    if (deleStamp != _DeleteStamp)
                    {
                        _DelProvider.FilterDocScore(_DocScoreList);
                        _DeleteStamp = deleStamp;
                    }
                    
                    for(int i = 0; i < _DocScoreList.Length; i++)
                    {
                        DocScore docScore = _DocScoreList[i];
                        if (docScore.DocId < 0)
                        {
                            continue;
                        }

                        long rank = docScore.Score * wordRank / Norm_Ranks;
                        int score = 0;
                        if (rank > int.MaxValue - 4000000)
                        {
                            long high = rank % (int.MaxValue - 4000000);

                            score = int.MaxValue - 4000000 + (int)(high / 1000);
                        }
                        else
                        {
                            score = (int)rank;
                        }

                        Query.DocumentRank docRank;

                        if (docIdRank.TryGetValue(docScore.DocId, out docRank))
                        {
                            docRank.Rank += score;
                        }
                        else
                        {
                            docRank = new Query.DocumentRank(docScore.DocId, score);
                            docIdRank.Add(docScore.DocId, docRank);
                        }
                    }
                }
            }

            #endregion

            #region internal properties

            #endregion

            #region internal methods

            internal void WordUpdate(string word, List<DocumentPositionList> docList)
            {
                lock (this)
                {
                    _ListForReader.AddRange(docList);
                    UpdateDocScoreList();
                }
            }

            internal WordIndexReader(string word, List<DocumentPositionList> docList, long totalDocs, 
                Data.DBProvider dbProvider, int tabIndex)
            {
                _Word = word;
                _ListForReader = docList;
                _DBProvider = dbProvider;
                _DelProvider = _DBProvider.DelProvider;
                _TabIndex = tabIndex;
                _TotalDocs = totalDocs;
                UpdateDocScoreList();
            }

            #endregion

            public override string ToString()
            {
                return Word + "," + HitCount.ToString();
            }

            #region IComparable<WordIndexReader> Members

            public int CompareTo(WordIndexReader other)
            {
                if (other == null)
                {
                    return 1;
                }

                if (this.HitCount > other.HitCount)
                {
                    return 1;
                }
                else if (this.HitCount == other.HitCount)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }

            #endregion
        }

        /// <summary>
        /// Index for every word for writer
        /// </summary>
        public class WordIndexWriter
        {
            #region Private fields
            string _Word;
            AppendList<int> _TempPositionList = new AppendList<int>();
            long _TempDocumentId;
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

            internal long TempDocumentId
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

            internal void Wait(long docId)
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
                            _ListForWriter.Add(new DocumentPositionList(_TempPositionList.Count, TempDocumentId));
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

                            _ListForWriter.Add(new DocumentPositionList(_TempPositionList, TempDocumentId));
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
        private int _TabIndex;
        //private System.Threading.Thread _CollectThread; //Collect Thread collect the WordTable that need write to index file,and write the index to file
        private bool _Closed = false;
        private Dictionary<string, WordIndexWriter> _WordTableNeedCollectDict = new Dictionary<string, WordIndexWriter>();
        private List<IndexFile.DocInfo> _DocInfosNeedCollect = new List<IndexFile.DocInfo>();

        private Dictionary<string, WordIndexReader> _WordTableReader = new Dictionary<string, WordIndexReader>();
        private Dictionary<string, WordIndexWriter> _WordTableWriter = new Dictionary<string, WordIndexWriter>();

        private Dictionary<long, int> _DocumentWordCountTable = new Dictionary<long, int>();
        private long _DocumentCount;
        private Store.IndexFileProxy _IndexFileProxy;
        private bool _IndexFinished = false;

        private int _WriteCount = 0;

        private int _ForceCollectCount = 5000;

        private Data.DBProvider _DBProvider;
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

        #region Public properties

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

        public long DocumentCount
        {
            get
            {
                return _DocumentCount;
            }
        }

        #endregion

        #region Private Metheds

        private List<IndexFile.DocInfo> GetDocInfosNeedCollect()
        {
            lock (this)
            {
                List<IndexFile.DocInfo> docInfos = _DocInfosNeedCollect;

                _DocInfosNeedCollect = new List<IndexFile.DocInfo>();

                return docInfos;
            }
        }

        private List<WordIndexWriter> GetWordTableNeedCollectDict(bool forceCollect)
        {
            lock (this)
            {
                List<WordIndexWriter> retVal = new List<WordIndexWriter>();

                Dictionary<string, WordIndexWriter> dict = _WordTableNeedCollectDict;

                List<string> removedwords = new List<string>();

                foreach (WordIndexWriter wordIndex in dict.Values)
                {
                    if (wordIndex.ListForWirterCount > 512 || forceCollect)
                    {
                        retVal.Add(wordIndex);
                        removedwords.Add(wordIndex.Word);
                    }
                }

                foreach (string word in removedwords)
                {
                    dict.Remove(word);
                }

                //_WordTableNeedCollectDict = new Dictionary<string,WordIndex>();
                return retVal;
            }
        }


        private void DoCollect()
        {
            Stopwatch stopwatch = new Stopwatch();

            int DoCollectTimes = 0;

            while (!Closed)
            {
                if (stopwatch.ElapsedMilliseconds < 100)
                {
                    System.Threading.Thread.Sleep(100);
                }

                if (DoCollectTimes++ > 100)
                {
                    DoCollectTimes = 0;
                }

                stopwatch.Reset();
                stopwatch.Start();

                //Do Collect
                bool indexFinished = IndexFinished;

                List<WordIndexWriter> wordIndexList = GetWordTableNeedCollectDict(indexFinished);

                foreach (WordIndexWriter wordIndex in wordIndexList)
                {
                    List<DocumentPositionList> docList = wordIndex.GetDocListForWriter();

                    if (docList != null)
                    {
                        //Sync call
                        _IndexFileProxy.AddWordPositionAndDocumentPositionList(wordIndex.Word, docList);
                    }
                    
                }

                wordIndexList = null;

                stopwatch.Stop();
            }
        }

        private void InitCollectThread()
        {
            //_CollectThread = new System.Threading.Thread(new System.Threading.ThreadStart(DoCollect));
            //_CollectThread.IsBackground = true;
            //_CollectThread.Start();
        }

        private void InitFileStore(string path, string fieldName, bool rebuild)
        {
            _IndexFileProxy = new IndexFileProxy(path, fieldName, rebuild);
            _IndexFileProxy.WordUpdateDelegate = WordUpdateDelegate;
        }

        private void StoreIndexToFile()
        {
            Dictionary<string, WordIndexWriter> tempWordTableWriter;

            lock (this)
            {
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

            _IndexFileProxy.AddDocInfos(GetDocInfosNeedCollect());

            _IndexFileProxy.Collect();
        }

        void WordUpdateDelegate(string word, List<DocumentPositionList> docList)
        {
            WordIndexReader wr;

            lock (this)
            {
                if (!_WordTableReader.TryGetValue(word, out wr))
                {
                    return;
                }
            }

            wr.WordUpdate(word, docList);
        }

        #endregion

        public InvertedIndex(string path, string fieldName, int tabIndex, Data.Field.IndexMode mode, bool rebuild, Data.DBProvider dbProvider)
        {
            _FieldName = fieldName;
            _TabIndex = tabIndex;
            _IndexMode = mode;
            _DBProvider = dbProvider;
            InitFileStore(path, fieldName, rebuild);
            Cache.CacheManager.Register(this);
            //InitCollectThread();
        }

        public InvertedIndex(Data.DBProvider dbProvider)
        {
            _DBProvider = dbProvider;
            Cache.CacheManager.Register(this);
            //InitCollectThread();
        }


        #region Public Metheds

        public void Close()
        {
            Cache.CacheManager.UnRegister(this);

            Closed = true;

            _IndexFileProxy.Close(2000);

        }

        public int GetDocumentWordCount(long docId)
        {
            int count;

            if (_DocumentWordCountTable.TryGetValue(docId, out count))
            {
                return count;
            }
            else
            {
                return 0;
            }
        }

        public WordIndexReader GetWordIndex(string word)
        {
            lock (this)
            {
                WordIndexReader retVal;

                if (_WordTableReader.TryGetValue(word, out retVal))
                {
                    retVal.Hit();
                    return retVal;
                }

                WordIndexReader wIndex = _IndexFileProxy.GetWordIndex(new IndexFileProxy.GetInfo(word, DocumentCount, _DBProvider,
                    _TabIndex));

                if (wIndex != null)
                {
                    _WordTableReader.Add(word, wIndex);
                    wIndex.Hit();
                    return wIndex;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Index a text for one field
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="documentId">document id</param>
        /// <param name="analyzer">analyzer</param>
        public int Index(string text, long documentId, Analysis.IAnalyzer analyzer)
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
                    if (WordTableWriter.TryGetValue(wordInfo.Word, out wordIndex))
                    {
                        //Wait Hit == false 
                        wordIndex.Wait(documentId);

                        if (!wordIndex.Hit)
                        {
                            hitIndexes.Add(wordIndex);
                            wordIndex.Hit = true;
                            wordIndex.TempDocumentId = documentId;
                        }
                    }
                    else
                    {
                        //wordIndex = new WordIndexWriter(wordInfo.Word, FileName == null);
                        wordIndex = new WordIndexWriter(wordInfo.Word, _IndexMode);

                        hitIndexes.Add(wordIndex);
                        wordIndex.Hit = true;
                        wordIndex.TempDocumentId = documentId;

                        WordTableWriter.Add(wordInfo.Word, wordIndex);
                    }

                    wordIndex.AddToTempPositionList(wordInfo.Position);
                    count++;

                }

                _DocumentWordCountTable[documentId] = count;
                retCount = count;
                //_DocInfosNeedCollect.Add(new IndexFile.DocInfo(

                foreach (WordIndexWriter wordIndex in hitIndexes)
                {
                    wordIndex.Index();
                    wordIndex.Hit = false;

                    if (!_WordTableNeedCollectDict.ContainsKey(wordIndex.Word))
                    {
                        _WordTableNeedCollectDict.Add(wordIndex.Word, wordIndex);
                    }
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

        #endregion

        #region IManagedCache Members

        public long CacheSize
        {
            get 
            {
                lock (this)
                {
                    long size = 0;

                    foreach (WordIndexReader ir in _WordTableReader.Values)
                    {
                        size += ir.MemorySize;
                    }

                    return size;
                }
             }
        }

        public void ReduceMemory(int percentage)
        {
            lock (this)
            {
                if (_WordTableReader.Count <= 0)
                {
                    return;
                }

                int count = _WordTableReader.Count * percentage / 100;

                if (count == 0 && _WordTableReader.Count < 10)
                {
                    _WordTableReader.Clear();
                    return;
                }
                
                List<WordIndexReader> indexReaderList = new List<WordIndexReader>();


                foreach (WordIndexReader ir in _WordTableReader.Values)
                {
                    indexReaderList.Add(ir);
                }

                indexReaderList.Sort();

                for (int i = 0; i < count; i++)
                {
                    _WordTableReader.Remove(indexReaderList[i].Word);
                }
            }
        }

        #endregion
    }
}
