using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataType;
using Hubble.Framework.Arithmetic;

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
            List<int> _TempPositionList = new List<int>();
            long _TempDocumentId;
            bool _Hit = false;
            bool _IsRamIndex = true;


            List<CompressIntList> _ListForReader = new List<CompressIntList>();
            List<CompressIntList> _ListForWriter = new List<CompressIntList>();
            #endregion

            #region Private properties
            private List<CompressIntList> ListForReader
            {
                get
                {
                    return _ListForReader;
                }
            }

            private List<CompressIntList> ListForWriter
            {
                get
                {
                    return _ListForWriter;
                }
            }

            #endregion

            #region Public Properties

            public string Word
            {
                get
                {
                    return _Word;
                }
            }

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


            public CompressIntList this[int index]
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

            internal List<int> TempPositionList
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

                    CompressIntList list = new CompressIntList(TempPositionList, TempDocumentId);
                    _ListForWriter.Add(list);
                    //Add(TempDocumentId, list);
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

        public int WordTableSize
        {
            get
            {
                return _WordTable.Count;
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

        public void Index(string text, long documentId, Analysis.IAnalyzer analyzer)
        {
            List<WordIndex> hitIndexes = new List<WordIndex>(4192);

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
            }

            foreach (WordIndex wordIndex in hitIndexes)
            {
                wordIndex.Index();
            }
        }
    }
}
