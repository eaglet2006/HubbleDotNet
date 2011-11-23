using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Analysis;

namespace Hubble.Analyzer
{
    /// <summary>
    /// NoneSegment output whole text of input.
    /// </summary>
    public class NoneSegment : IAnalyzer, Hubble.Core.Data.INamedExternalReference
    {
        int _Count;
        #region IAnalyzer Members

        /// <summary>
        /// Count of words
        /// </summary>
        public int Count
        {
            get
            {
                return _Count;
            }
        }

        /// <summary>
        /// Initialize the segment
        /// </summary>
        public void Init()
        {
            //Write init code here
        }

        /// <summary>
        /// Tokenize for index
        /// </summary>
        /// <param name="text">text which tokenized to</param>
        /// <returns>word info list</returns>
        public IEnumerable<Hubble.Core.Entity.WordInfo> Tokenize(string text)
        {
            _Count = 1;
            text = text.Trim();

            if (text == "")
            {
                yield break;
            }
            else
            {
                yield return new Hubble.Core.Entity.WordInfo(text, 0);
            }
        }

        /// <summary>
        /// Tokenize for search keywords
        /// </summary>
        /// <param name="text">text which tokenized to</param>
        /// <returns>word info list</returns>
        public IEnumerable<Hubble.Core.Entity.WordInfo> TokenizeForSqlClient(string text)
        {
            text = text.Trim();

            if (text == "")
            {
                yield break;
            }
            else
            {
                yield return new Hubble.Core.Entity.WordInfo(text, 0);
            }
        }

        #endregion

        #region INamedExternalReference Members

        public string Name
        {
            get
            {
                return "NoneSegment";
            }
        }

        #endregion
    }
}
