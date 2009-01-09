using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Entity;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Index
{

    /// <summary>
    /// This class helps to build and query 
    /// a inverted index
    /// </summary>
    public class InvertedIndex
    {
        #region WordIndex

        /// <summary>
        /// Index for every word
        /// </summary>
        public class WordIndex
        {
            #region Private fields
            string _Word;
            AppendList<int> _TempPositionList = new AppendList<int>();
            long _TempDocumentId;
            bool _Hit = false;
            bool _IsRamIndex;

            List<DocumentPositionList> _ListForReader = new List<DocumentPositionList>();
            List<DocumentPositionList> _ListForWriter = new List<DocumentPositionList>();

            long _WordCount;

            object _WaitLock = new object();

            #endregion

            #region Private properties
            private List<DocumentPositionList> ListForReader
            {
                get
                {
                    return _ListForReader;
                }
            }

            private List<DocumentPositionList> ListForWriter
            {
                get
                {
                    return _ListForWriter;
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
                    return _Word;
                }
            }

            /// <summary>
            /// Total number of documents
            /// </summary>
            public int Count
            {
                get
                {
                    if (_IsRamIndex)
                    {
                        return ListForWriter.Count;
                    }
                    else
                    {
                        return ListForReader.Count;
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
                    return _WordCount;
                }
            }

            public DocumentPositionList this[int index]
            {
                get
                {
                    if (_IsRamIndex)
                    {
                        return ListForWriter[index];
                    }
                    else
                    {
                        return ListForReader[index];
                    }
                }
            }

            #endregion


            #region Private methods

            #endregion

            #region Public methods

            public long GetDocumentId(int index)
            {
                if (_IsRamIndex)
                {
                    return ListForWriter[index].DocumentId;
                }
                else
                {
                    return ListForReader[index].DocumentId;
                }
            }

            public long GetHitCount(int index)
            {
                if (_IsRamIndex)
                {
                    return ListForWriter[index].Count;
                }
                else
                {
                    return ListForReader[index].Count;
                }
            }

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

            public void Wait(long docId)
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

            internal WordIndex(string word, bool isRamIndex)
            {
                _Word = word;
                _IsRamIndex = isRamIndex;
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
                        _WordCount += _TempPositionList.Count;

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

        private System.Threading.Thread _CollectThread; //Collect Thread collect the WordTable that need write to index file,and write the index to file
        private bool _Closed = false;
        private Dictionary<string, WordIndex> _WordTableNeedCollectDict = new Dictionary<string, WordIndex>();
        private Dictionary<string, WordIndex> _WordTable = new Dictionary<string, WordIndex>();
        private Dictionary<long, int> _DocumentWordCountTable = new Dictionary<long, int>();
        private long _DocumentCount;
        private string _FileName;
        private Store.IndexFileProxy _IndexFileProxy;

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

        #region Public properties

        public int WordTableSize
        {
            get
            {
                return _WordTable.Count;
            }
        }

        public long DocumentCount
        {
            get
            {
                return _DocumentCount;
            }
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }

            set
            {
                _FileName = value;

                _IndexFileProxy = new Hubble.Core.Store.IndexFileProxy(FileName);
            }
        }

        #endregion

        #region Private Metheds

        private Dictionary<string, WordIndex> GetWordTableNeedCollectDict()
        {
            lock (this)
            {
                Dictionary<string, WordIndex> dict = _WordTableNeedCollectDict;
                _WordTableNeedCollectDict = new Dictionary<string,WordIndex>();
                return dict;
            }
        }

        private void DoCollect()
        {
            while (!Closed)
            {
                System.Threading.Thread.Sleep(1000);

                //Do Collect

                foreach (WordIndex wordIndex in GetWordTableNeedCollectDict().Values)
                {

                }
            }
        }

        #endregion

        public InvertedIndex()
        {
            _CollectThread = new System.Threading.Thread(new System.Threading.ThreadStart(DoCollect));
            _CollectThread.IsBackground = true;
            _CollectThread.Start();
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

        public WordIndex GetWordIndex(string word)
        {
            WordIndex retVal;

            if (_WordTable.TryGetValue(word, out retVal))
            {
                return retVal;
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
            List<WordIndex> hitIndexes = new List<WordIndex>(4192);

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

                    WordIndex wordIndex;
                    if (_WordTable.TryGetValue(wordInfo.Word, out wordIndex))
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
                        wordIndex = new WordIndex(wordInfo.Word, FileName == null);
                        hitIndexes.Add(wordIndex);
                        wordIndex.Hit = true;
                        wordIndex.TempDocumentId = documentId;

                        _WordTable.Add(wordInfo.Word, wordIndex);
                    }

                    wordIndex.AddToTempPositionList(wordInfo.Position);
                    count++;

                }

                _DocumentWordCountTable[documentId] = count;
            }


            foreach (WordIndex wordIndex in hitIndexes)
            {
                wordIndex.Index();
            }
        }

        #endregion
    }
}
