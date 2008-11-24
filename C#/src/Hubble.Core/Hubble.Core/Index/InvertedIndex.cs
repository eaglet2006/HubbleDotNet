using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataTypes;
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

        class WordIndex 
        {
            string _Word;
            List<int> _TempPositionList = new List<int>();
            long _TempDocumentId;
            bool _Hit = false;
            List<CompressIntList> _List = new List<CompressIntList>();

            public string Word
            {
                get
                {
                    return _Word;
                }
            }

            public bool Hit
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

            public long TempDocumentId
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

            public List<int> TempPositionList
            {
                get
                {
                    return _TempPositionList;
                }
            }

            public WordIndex(string word)
            {
                _Word = word;
            }

            public void Index()
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
                    _List.Add(list);
                    //Add(TempDocumentId, list);
                }
                finally
                {
                    Hit = false;
                    TempPositionList.Clear();
                }
            }

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

        public void Index(string text, int documentId, Analyze.IAnalyzer analyzer)
        {
            List<WordIndex> hitIndexes = new List<WordIndex>(4192);

            foreach (Entities.WordInfo wordInfo in analyzer.Tokenize(text))
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
