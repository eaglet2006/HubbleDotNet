using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Analysis.Framework
{
    [Serializable]
    public class WordAttribute
    {
        /// <summary>
        /// Word
        /// </summary>
        public String Word;

        /// <summary>
        /// Frequency for this word
        /// </summary>
        public double Frequency;

        public WordAttribute()
        {

        }

        public WordAttribute(string word, double frequency)
        {
            this.Word = word;
            this.Frequency = frequency;
        }

        public override string ToString()
        {
            return Word;
        }

    }
}
