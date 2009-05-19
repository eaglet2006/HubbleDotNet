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
using Hubble.Framework.Serialization;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Entity
{
    /// <summary>
    /// This class record all the position for one word in one document.
    /// 
    /// first bit of first byte is 1
    /// first bit of remain bytes is 0
    /// |11111111 01111111 00111111| 11111111|
    /// means two int
    /// first int is 0x5f7f7f
    /// second int is 0x7f
    /// Input int must not be less then zero!
    /// </summary>
    //[StructLayout(LayoutKind.Sequential,Pack=4)]
    public class DocumentPositionList : IEnumerable<int>, IMySerialization<DocumentPositionList>, IMySerialization 
    {
        //[FieldOffset(0)]
        public long DocumentId;

        //[FieldOffset(8)]
        /// <summary>
        /// Count of items
        /// </summary>
        public int Count;

        //[FieldOffset(12)]
        /// <summary>
        /// Document rank
        /// </summary>
        public int Rank;

        //[FieldOffset(16)]
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst=16)]
        private byte[] Data;

        public int Size
        {
            get
            {
                return sizeof(long) + sizeof(int) * 2 + Data.Length;
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

        public DocumentPositionList()
        {
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
        public DocumentPositionList(IList<int> input, long documentId)
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

        public DocumentPositionList(int count, long documentId)
        {
            DocumentId = documentId;
            Count = count;
            Data = new byte[0];
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

        #region IMySerialization<CompressIntList> Members

        public byte Version
        {
            get 
            {
                return 1;
            }
        }

        public void Serialize(System.IO.Stream s)
        {
            //s.Write(BitConverter.GetBytes(Size), 0, sizeof(int));
            //s.Write(BitConverter.GetBytes(DocumentId), 0, sizeof(long));
            //s.Write(BitConverter.GetBytes(Count), 0, sizeof(int));
            //s.Write(BitConverter.GetBytes(Rank), 0, sizeof(int));

            VInt size = Data.Length;
            size.WriteToStream(s);
            VLong docId = DocumentId;
            docId.WriteToStream(s);
            VInt count = Count;
            count.WriteToStream(s);
            VInt rank = Rank;
            rank.WriteToStream(s);

            if (Data.Length > 0)
            {
                s.Write(Data, 0, Data.Length);
            }
        }

        public DocumentPositionList Deserialize(System.IO.Stream s, short version)
        {
            switch (version)
            {
                case 0:
                    return null;

                case 1:
                    //if (s.Length - s.Position < sizeof(long) + sizeof(int) * 2 + sizeof(int))
                    //{
                    //    return null;
                    //}

                    VInt vsize = new VInt();
                    vsize.ReadFromStream(s);

                    int size = (int)vsize;

                    VLong docId = new VLong();
                    docId.ReadFromStream(s);
                    VInt count = new VInt();
                    count.ReadFromStream(s);
                    VInt rank = new VInt();
                    rank.ReadFromStream(s);

                    //byte[] buf = new byte[sizeof(long)];
                    //Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(int));
                    //int size = BitConverter.ToInt32(buf, 0);

                    //if (size == 0)
                    //{
                    //    return null;
                    //}

                    //Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(long));
                    //DocumentId = BitConverter.ToInt64(buf, 0);
                    
                    //Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(int));
                    //Count = BitConverter.ToInt32(buf, 0);
                    
                    //Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(int));
                    //Rank = BitConverter.ToInt32(buf, 0);

                    //buf = new byte[size - (sizeof(long) + sizeof(int) * 2)];

                    DocumentId = (long)docId;
                    Count = (int)count;
                    Rank = (int)rank;

                    byte[] buf = new byte[size];

                    if (size > 0)
                    {
                        Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, buf.Length);
                    }

                    Data = buf;

                    return this;
                default:
                    throw new System.Runtime.Serialization.SerializationException(
                        string.Format("Invalid version:{0}", version));
            }
        }

        #endregion

        #region IMySerialization Members


        object IMySerialization.Deserialize(System.IO.Stream s, short version)
        {
            return Deserialize(s, version);
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
                    DocumentPositionList.Add(data, tempBuf);
                }
                else
                {
                    DocumentPositionList.Add(data - lastData, tempBuf);
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
