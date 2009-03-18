using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Hubble.Core.Entity;
using Hubble.Core.Store;
using Hubble.Framework.DataStructure;
using Hubble.Framework.IO;

namespace Hubble.Core.Index
{

    /// <summary>
    /// This class helps to build and query 
    /// a inverted index
    /// </summary>
    public class InvertedIndex
    {
        #region WordIndex

        public class WordIndexReader
        {
            #region Private fields
            string _Word;

            List<DocumentPositionList> _ListForReader = new List<DocumentPositionList>();

            long _WordCount;

            #endregion

            #region Private properties
            private List<DocumentPositionList> ListForReader
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

            #endregion

            #region Public methods

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

            #endregion

            #region internal properties

            #endregion

            #region internal methods

            internal WordIndexReader(string word, List<DocumentPositionList> docList)
            {
                _Word = word;
                _ListForReader = docList;

                _WordCount = 0;

                foreach (DocumentPositionList dList in docList)
                {
                    _WordCount += dList.Count;
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

            internal WordIndexWriter(string word)
            {
                _Word = word;
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

        private System.Threading.Thread _CollectThread; //Collect Thread collect the WordTable that need write to index file,and write the index to file
        private bool _Closed = false;
        private Dictionary<string, WordIndexWriter> _WordTableNeedCollectDict = new Dictionary<string, WordIndexWriter>();

        private Dictionary<string, WordIndexReader> _WordTableReader = new Dictionary<string, WordIndexReader>();
        private Dictionary<string, WordIndexWriter> _WordTableWriter = new Dictionary<string, WordIndexWriter>();

        private Dictionary<long, int> _DocumentWordCountTable = new Dictionary<long, int>();
        private long _DocumentCount;
        private Store.IndexFileProxy _IndexFileProxy;
        private bool _IndexFinished = false;

        private int _WriteCount = 0;

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
                    GC.Collect();
                    Console.WriteLine("GC.Collect");
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
            _CollectThread = new System.Threading.Thread(new System.Threading.ThreadStart(DoCollect));
            _CollectThread.IsBackground = true;
            _CollectThread.Start();
        }

        private void InitFileStore(string path, string fieldName, bool rebuild)
        {
            _IndexFileProxy = new IndexFileProxy(path, fieldName);
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

            _IndexFileProxy.Collect();
        }

        #endregion

        public InvertedIndex(string path, string fieldName, bool rebuild)
        {
            InitFileStore(path, fieldName, rebuild);
            //InitCollectThread();
        }

        public InvertedIndex()
        {
            //InitCollectThread();
        }


        #region Public Metheds

        public void Close()
        {
            Closed = true;
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
            WordIndexReader retVal;

            if (_WordTableReader.TryGetValue(word, out retVal))
            {
                return retVal;
            }

            WordIndexReader wIndex = _IndexFileProxy.GetWordIndex(word);

            if (wIndex != null)
            {
                _WordTableReader.Add(word, wIndex);
                return wIndex;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Index a text for one field
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="documentId">document id</param>
        /// <param name="analyzer">analyzer</param>
        public void Index(string text, long documentId, Analysis.IAnalyzer analyzer)
        {
            List<WordIndexWriter> hitIndexes = new List<WordIndexWriter>(4192);

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
                        wordIndex = new WordIndexWriter(wordInfo.Word);

                        hitIndexes.Add(wordIndex);
                        wordIndex.Hit = true;
                        wordIndex.TempDocumentId = documentId;

                        WordTableWriter.Add(wordInfo.Word, wordIndex);
                    }

                    wordIndex.AddToTempPositionList(wordInfo.Position);
                    count++;

                }

                _DocumentWordCountTable[documentId] = count;
            }


            foreach (WordIndexWriter wordIndex in hitIndexes)
            {
                wordIndex.Index();

                lock (this)
                {
                    if (!_WordTableNeedCollectDict.ContainsKey(wordIndex.Word))
                    {
                        _WordTableNeedCollectDict.Add(wordIndex.Word, wordIndex);
                    }
                }
 
            }

            if (WriteCount >= 5000)
            {
                WriteCount = 0;
                StoreIndexToFile();
            }
            else
            {
                WriteCount++;
            }
        }

        public void FinishIndex()
        {
            WriteCount = 0;
            StoreIndexToFile();
        }

        #endregion
    }
}
