using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Entity
{
    public struct WordInfo : IComparable<WordInfo>
    {
        public string Word;
        public int Position;
        public int Rank;

        public WordInfo(string word, int position) : this(word, position, 0)
        {
        }

        public WordInfo(string word, int position, int rank)
        {
            Word = word;
            Position = position;
            Rank = rank <= 0 ? 1 : rank;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Word, Position, Rank) ;
        }

        #region IComparable<WordInfo> Members

        public int CompareTo(WordInfo other)
        {
            return this.Position.CompareTo(other.Position);
        }

        #endregion
    }
}
