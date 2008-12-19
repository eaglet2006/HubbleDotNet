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
            bool _IsRamIndex = true;

            List<DocumentPositionList> _ListForReader = new List<DocumentPositionList>();
            List<DocumentPositionList> _ListForWriter = new List<DocumentPositionList>();

            long _WordCount;

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
                    return _Hit;
                }

                set
                {
                    _Hit = value;
                }
            }

            internal long TempDocumentId
            {
                get
                {
                    return _TempDocumentId;
                }

                set
                {
                    _TempDocumentId = value;
                }
            }

            internal AppendList<int> TempPositionList
            {
                get
                {
                    return _TempPositionList;
                }
            }

            #endregion

            #region internal methods

            internal WordIndex(string word)
            {
                _Word = word;
            }

            /// <summary>
            /// Index one document into the _ListForWriter
            /// </summary>
            internal void Index()
            {
                try
                {
                    //Check sorted

                    int last = -1;
                    bool needSort = false;
                    foreach (int position in TempPositionList)
                    {
                        if (position < last)
                        {
                            needSort = true;
                            break;
                        }
                    }

                    if (needSort)
                    {
                        TempPositionList.Sort();
                    }

                    _ListForWriter.Add(new DocumentPositionList(TempPositionList, TempDocumentId));
                    _WordCount += TempPositionList.Count;
                }
                finally
                {
                    Hit = false;
                    TempPositionList.Clear();
                }
            }

            #endregion
        }

        #endregion

        Dictionary<string, WordIndex> _WordTable = new Dictionary<string, WordIndex>();
        Dictionary<long, int> _DocumentWordCountTable = new Dictionary<long, int>();
        long _DocumentCount;

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
                    if (!wordIndex.Hit)
                    {
                        hitIndexes.Add(wordIndex);
                        wordIndex.Hit = true;
                        wordIndex.TempDocumentId = documentId;
                    }
                }
                else
                {
                    wordIndex = new WordIndex(wordInfo.Word);
                    hitIndexes.Add(wordIndex);
                    wordIndex.Hit = true;
                    wordIndex.TempDocumentId = documentId;

                    _WordTable.Add(wordInfo.Word, wordIndex);
                }

                wordIndex.TempPositionList.Add(wordInfo.Position);
                count++;

            }

            _DocumentWordCountTable[documentId] = count;

            foreach (WordIndex wordIndex in hitIndexes)
            {
                wordIndex.Index();
            }
        }
    }
}
