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
using System.Linq;
using System.Text;

namespace Hubble.Framework.DataStructure
{
    public class BitSet : IEnumerable<int>
    {
        static readonly uint[] Mask = new uint[]
        {
            0x00000001,   0x00000002,  0x00000004,  0x00000008,
            0x00000010,   0x00000020,  0x00000040,  0x00000080,
            0x00000100,   0x00000200,  0x00000400,  0x00000800,
            0x00001000,   0x00002000,  0x00004000,  0x00008000,
            0x00010000,   0x00020000,  0x00040000,  0x00080000,
            0x00100000,   0x00200000,  0x00400000,  0x00800000,
            0x01000000,   0x02000000,  0x04000000,  0x08000000,
            0x10000000,   0x20000000,  0x40000000,  0x80000000,
        };

        static int[] ByteToBitsCount = null;

        static object _SynObj = new object();

        static void InitByteToBits()
        {
            ByteToBitsCount = new int[256];

            for (int i = 0; i <= 255; i++)
            {
                List<int> byteList = new List<int>();

                byte bit = 0;
                byte data = (byte)i;

                while (data != 0)
                {
                    if ((data & 1) != 0)
                    {
                        byteList.Add(bit);
                    }

                    data >>= 1;
                    bit++;
                }

                ByteToBitsCount[i] = byteList.Count;
            }
        }

        class Bucket
        {
            internal uint[] Data;

            internal Bucket(int size)
            {
                Data = new uint[size / 32];
            }
        }

        int _Count;
        int _BucketSize;
        int _Capability;
        Bucket[] _Index;

        public int Count
        {
            get
            {
                return _Count;
            }
        }

        public BitSet()
            : this(1 * 1024 * 1024, 8 * 1024)
        {

        }

        public BitSet(int capability, int bucketSize)
        {
            lock (_SynObj)
            {
                if (ByteToBitsCount == null)
                {
                    InitByteToBits();
                }
            }

            if (bucketSize < 1024)
            {
                bucketSize = 1024;
            }

            _BucketSize = bucketSize;

            int d = _BucketSize % 32;
            if (d != 0)
            {
                _BucketSize += d;
            }

            if (capability < 1024 * 1024)
            {
                capability = 1024 * 1024;
            }

            if (capability < _BucketSize)
            {
                capability = _BucketSize;
            }

            int detal = capability % bucketSize;

            if (detal != 0)
            {
                capability += detal;
            }

            _Capability = capability;

            _Count = 0;
            _Index = new Bucket[_Capability / _BucketSize];
        }

        private void Resize(int key)
        {
            if (key >= _Capability)
            {
                _Capability = ((key / _Capability) + 1) * _Capability;

                Bucket[] newIndex = new Bucket[_Capability / _BucketSize];

                Array.Copy(_Index, newIndex, _Index.Length);

                _Index = newIndex;
            }
        }

        public bool ForceAdd(int key)
        {
            if (key < 0)
            {
                throw new ArgumentException("key must not be negative");
            }

            if (key >= _Capability)
            {
                Resize(key);
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                _Index[index] = new Bucket(_BucketSize);
            }

            int subIndex = (key % _BucketSize) / 32;

            int subBitIndex = (key % _BucketSize) % 32;

            if ((_Index[index].Data[subIndex] & Mask[subBitIndex]) != 0)
            {
                return false;
            }

            _Count++;

            _Index[index].Data[subIndex] |= Mask[subBitIndex];

            return true;
        }

        public void Add(int key)
        {
            if (key < 0)
            {
                throw new ArgumentException("key must not be negative");
            }

            if (key >= _Capability)
            {
                Resize(key);
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                _Index[index] = new Bucket(_BucketSize);
            }

            int subIndex = (key % _BucketSize) / 32;

            int subBitIndex = (key % _BucketSize) % 32;

            if ((_Index[index].Data[subIndex] & Mask[subBitIndex]) != 0) 
            {
                throw new System.ArgumentException("Adding duplicate key");
            }

            _Count++;

            _Index[index].Data[subIndex] |= Mask[subBitIndex];
        }

        public bool Contains(int key)
        {
            if (key >= _Capability || key < 0)
            {
                return false;
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                return false;
            }

            int subIndex = (key % _BucketSize) / 32;

            int subBitIndex = (key % _BucketSize) % 32;


            return (_Index[index].Data[subIndex] & Mask[subBitIndex]) != 0;
        }

        public bool Remove(int key)
        {
            if (key >= _Capability || key < 0)
            {
                return false;
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                return false;
            }

            int subIndex = (key % _BucketSize) / 32;

            int subBitIndex = (key % _BucketSize) % 32;

            if ((_Index[index].Data[subIndex] & Mask[subBitIndex]) == 0)
            {
                return false;
            }

            _Index[index].Data[subIndex] &= ~Mask[subBitIndex];
            _Count--;
            return true;
        }

        public int CalculateCount()
        {
            int count = 0;

            for (int i = 0; i < _Index.Length; i++)
            {
                if (_Index[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < _Index[i].Data.Length; j++)
                {
                    uint data = _Index[i].Data[j];
                    

                    while (data != 0)
                    {
                        int bData = (int)data & 0xFF;

                        count += ByteToBitsCount[bData];

                        data >>= 8;
                    }

                    //if (data == 0)
                    //{
                    //    continue;
                    //}

                    //int subBitIndex = 0;
                    //while (data > 0)
                    //{
                    //    if ((data & 1) != 0)
                    //    {
                    //        count++;
                    //    }

                    //    data >>= 1;
                    //    subBitIndex++;
                    //}
                }
            }

            return count;
        }

        #region IEnumerable<int> Members

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < _Index.Length; i++)
            {
                if (_Index[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < _Index[i].Data.Length; j++)
                {
                    uint data = _Index[i].Data[j];

                    if (data == 0)
                    {
                        continue;
                    }

                    int subBitIndex = 0;
                    while (data > 0)
                    {
                        if ((data & 1) != 0)
                        {
                            yield return i * _BucketSize + j * 32 + subBitIndex;
                        }

                        data >>= 1;
                        subBitIndex++;
                    }

                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < _Index.Length; i++)
            {
                if (_Index[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < _Index[i].Data.Length; j++)
                {
                    uint data = _Index[i].Data[j];

                    if (data == 0)
                    {
                        continue;
                    }

                    int subBitIndex = 0;
                    while (data > 0)
                    {
                        if ((data & 1) != 0)
                        {
                            yield return i * _BucketSize + j * 32 + subBitIndex;
                        }

                        data >>= 1;
                        subBitIndex++;
                    }

                }
            }
        }

        #endregion
    }
}
