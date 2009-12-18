using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Analysis.Framework
{
    public enum WordType
    {
        None = 0,
        English = 1,
        SimplifiedChinese = 2,
        TraditionalChinese = 3,
        Numeric = 4,
        Symbol = 5,
        Space = 6,
    }


    public class WordInfo : WordAttribute, IComparable<WordInfo>
    {
        /// <summary>
        /// Current word type
        /// </summary>
        public WordType WordType;

        /// <summary>
        /// Original word type
        /// </summary>
        public WordType OriginalWordType;

        /// <summary>
        /// Word position
        /// </summary>
        public int Position;

        /// <summary>
        /// Rank for this word
        /// 单词权重
        /// </summary>
        public int Rank;

        public WordInfo()
        {
        }

        public WordInfo(string word, double frequency)
            : base(word, frequency)
        {
        }

        public WordInfo(WordAttribute wordAttr)
        {
            this.Word = wordAttr.Word;
            this.Frequency = wordAttr.Frequency;
        }

        public int GetEndPositon()
        {
            return this.Position + this.Word.Length;
        }

        #region IComparable<WordInfo> Members

        public int CompareTo(WordInfo other)
        {
            if (other == null)
            {
                return -1;
            }

            if (this.Position != other.Position)
            {
                return this.Position.CompareTo(other.Position);
            }

            if (other.Word == null)
            {
                return -1;
            }

            return this.Word.Length.CompareTo(other.Word.Length);
        }

        #endregion
    }

}
