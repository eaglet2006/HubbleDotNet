using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Entities
{
    public struct WordInfo
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
            Rank = rank;
        }
    }
}
