using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Entity;

namespace Hubble.Core.Query
{
    public class WordIndexForQuery : IComparable<WordIndexForQuery>
    {
        public int CurDocIdIndex;
        public int WordIndexesLength;
        public int Sum_d_t; //Sqrt of Total number of terms in all documents
        public int Idf_t;
        public int WordRank;
        public int FieldRank;
        public int RelTotalCount;
        public WordInfo.Flag Flags;

        public int QueryCount; //How many time is this word in query string.
        public int FirstPosition; //First position in query string.

        private Index.WordIndexReader _WordIndex;

        public Index.WordIndexReader WordIndex
        {
            get
            {
                return _WordIndex;
            }
        }

        public WordIndexForQuery(Index.WordIndexReader wordIndex,
            int totalDocuments, int wordRank, int fieldRank)
            :this(wordIndex, totalDocuments, wordRank, fieldRank, WordInfo.Flag.None)
        {

        }

        public WordIndexForQuery(Index.WordIndexReader wordIndex,
            int totalDocuments, int wordRank, int fieldRank, WordInfo.Flag flags)
        {
            FieldRank = fieldRank;
            WordRank = wordRank;
            RelTotalCount = wordIndex.RelDocCount;
            this.Flags = flags;

            if (FieldRank <= 0)
            {
                FieldRank = 1;
            }

            if (WordRank <= 0)
            {
                WordRank = 1;
            }

            _WordIndex = wordIndex;

            Sum_d_t = (int)Math.Sqrt(_WordIndex.WordCount);
            Idf_t = (int)Math.Log10((double)totalDocuments / (double)_WordIndex.RelDocCount + 1) + 1;
            //Idf_t = (int)Math.Log10((double)totalDocuments / (double)_WordIndex.Count + 1) + 1; //Old contains use this

            CurDocIdIndex = 0;
            WordIndexesLength = _WordIndex.Count;
        }

        public override string ToString()
        {
            return string.Format("{0}^{1}^{2}", this._WordIndex, this.WordRank, this.FirstPosition);
        }

        #region IComparable<WordIndexForQuery> Members

        public int CompareTo(WordIndexForQuery other)
        {
            return this.RelTotalCount.CompareTo(other.RelTotalCount);
            //return this.WordIndexesLength.CompareTo(other.WordIndexesLength); //old contains use this
        }

        #endregion
    }
}
