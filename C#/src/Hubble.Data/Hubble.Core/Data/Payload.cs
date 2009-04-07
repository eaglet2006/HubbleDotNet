using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Data
{
    /// <summary>
    /// Payload of each document
    /// </summary>
    public class Payload
    {
        int[] _Data;

        public int[] Data
        {
            get
            {
                return _Data;
            }
        }

        public Payload(int[] data)
        {
            _Data = data;
        }

        /// <summary>
        /// Get word count of one document
        /// </summary>
        /// <param name="tabIndex">tab index</param>
        /// <returns>count</returns>
        public int WordCount(int tabIndex)
        {
            return Data[tabIndex];
        }


    }
}
