/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Hubble.Framework.DataStructure
{
    /// <summary>
    /// first bit of first byte is 1
    /// first bit of remain bytes is 0
    /// |11111111 01111111 00111111| 11111111|
    /// means two int
    /// first int is 0x5f7f7f
    /// second int is 0x7f
    /// Input int must not be less then zero!
    /// </summary>
    [StructLayout(LayoutKind.Sequential,Pack=4)]
    public struct CompressIntList : IEnumerable<int>
    {
        //[FieldOffset(0)]
        public long DocumentId;

        //[FieldOffset(8)]
        public int Count;

        //[FieldOffset(12)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=16)]
        private byte[] Data;

        public int Size
        {
            get
            {
                return Data.Length;
            }
        }

        public static void Add(int data, List<byte> input)
        {
            Debug.Assert(data >= 0);

            if (data == 0)
            {
                input.Add(0x80);
                return;
            }

            input.Add((byte)((data & 0x0000007f) | 0x80));

            data >>= 7;

            while (data > 0)
            {
                input.Add((byte)(data & 0x0000007f));
                data >>= 7;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">int list</param>
        /// <remarks>
        /// Input int must not be less then zero!
        /// And the list must be sorted!
        /// The first element must be smallest!
        /// </remarks>
        public CompressIntList(IList<int> input, long documentId)
        {
            List<byte> tempBuf = new List<byte>(32);
            DocumentId = documentId;
            Count = input.Count;

            int lastData = -1;

            foreach (int data in input)
            {
                if (lastData == -1)
                {
                    Add(data, tempBuf);
                }
                else
                {
                    Add(data - lastData, tempBuf);
                }

                lastData = data;
            }


            Data = new byte[tempBuf.Count];

            tempBuf.CopyTo(Data);
        }

        public IEnumerator<int> Values
        {
            get
            {
                return this.GetEnumerator();
            }
        }


        #region IEnumerable<int> Members

        public IEnumerator<int> GetEnumerator()
        {
            int retVal = -1;
            int shift = 7;
            int last = -1;
            foreach (byte data in Data)
            {
                if ((data & 0x80) != 0)
                {
                    if (retVal >= 0)
                    {
                        if (last == -1)
                        {
                            last = retVal;
                        }
                        else
                        {
                            last = last + retVal;
                        }

                        yield return last;
                    }

                    shift = 7;
                    retVal = data & 0x7f;
                }
                else
                {
                    retVal += (int)data << shift;
                    shift += 7;
                }
            }

            if (retVal >= 0)
            {
                if (last == -1)
                {
                    last = retVal;
                }
                else
                {
                    last = last + retVal;
                }

                yield return last;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }

    public class CCompressIntList
    {
        private byte[] Data;

        public CCompressIntList(List<int> input):this(input, 8)
        {
        }

        public CCompressIntList(List<int> input, int capacity)
        {
            List<byte> tempBuf = new List<byte>(capacity);

            int lastData = -1;

            foreach (int data in input)
            {
                if (lastData == -1)
                {
                    CompressIntList.Add(data, tempBuf);
                }
                else
                {
                    CompressIntList.Add(data - lastData, tempBuf);
                }

                lastData = data;
            }


            Data = new byte[tempBuf.Count];

            tempBuf.CopyTo(Data);
        }

        public IEnumerator<int> Values
        {
            get
            {
                int retVal = -1;
                int shift = 7;
                int last = -1;
                foreach (byte data in Data)
                {
                    if ((data & 0x80) != 0)
                    {
                        if (retVal >= 0)
                        {
                            if (last == -1)
                            {
                                last = retVal;
                            }
                            else
                            {
                                last = last + retVal;
                            }

                            yield return last;
                        }

                        shift = 7;
                        retVal = data & 0x7f;
                    }
                    else
                    {
                        retVal += (int)data << shift;
                        shift += 7;
                    }
                }
            }
        }
    }

}
