using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Data
{
    /// <summary>
    /// Payload of each document
    /// </summary>
    public class Payload
    {
        int _FileIndex = -1;

        public int FileIndex
        {
            get
            {
                return _FileIndex;
            }

            set
            {
                _FileIndex = value;
            }
        }

        int[] _Data;

        public int[] Data
        {
            get
            {
                return _Data;
            }
        }

        public Payload(int dataLength)
        {
            _Data = new int[dataLength];
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

        public void CopyTo(byte[] buf)
        {
            Debug.Assert(buf != null);
            Debug.Assert(buf.Length == Data.Length * sizeof(int));

            int start = 0;
            foreach (int d in Data)
            {
                byte[] dBuf = BitConverter.GetBytes(d);
                Array.Copy(dBuf, 0, buf, start, dBuf.Length);
                start += dBuf.Length;
            }
        }

        public void CopyFrom(byte[] buf)
        {
            Debug.Assert(buf != null);
            Debug.Assert(buf.Length == Data.Length * sizeof(int));

            int start = 0;
            while (start < buf.Length)
            {
                int d = BitConverter.ToInt32(buf, start);
                Data[start / sizeof(int)] = d;
                start += sizeof(int);
            }
        }
    }
}
