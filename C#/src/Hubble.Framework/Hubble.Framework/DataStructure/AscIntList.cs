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
    public class AscIntList : List<int>
    {
        int _LastValue = int.MinValue;

        BitSet _DelBitSet = null;

        public AscIntList()
            :base(1024)
        {
        }

        public AscIntList(int capacity)
            :base(capacity)
        {
        }

        public new bool Contains(int value)
        {
            return this.BinarySearch(value) >= 0;
        }

        public new int Count
        {
            get
            {
                if (_DelBitSet == null)
                {
                    return base.Count;
                }
                else
                {
                    return base.Count - _DelBitSet.Count;
                }
            }
        }

        public void Compress()
        {
            if (_DelBitSet != null)
            {
                List<int> list = new List<int>(this.Count);

                for (int i = 0; i < base.Count; i++)
                {
                    int value = this[i];
                    if (!_DelBitSet.Contains(value))
                    {
                        list.Add(value);
                    }
                }

                this.Clear();
                this.AddRange(list);
                _DelBitSet = null;
            }
        }

        public new bool Remove(int value)
        {
            if (this.Contains(value))
            {
                if (_DelBitSet == null)
                {
                    _DelBitSet = new BitSet();
                }

                return _DelBitSet.ForceAdd(value);
            }
            else
            {
                return false;
            }
        }

        public new void Add(int value)
        {
            if (value <= _LastValue)
            {
                throw new ArgumentException(string.Format("value={0} must large than last value={1}",
                    value, _LastValue));
            }

            _LastValue = value;

            base.Add(value);
        }



        /// <summary>
        /// Merge two lists as or
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static AscIntList MergeOr(AscIntList src, AscIntList dest)
        {
            if (src == null)
            {
                return dest;
            }

            if (dest == null)
            {
                return src;
            }

            src.Compress();
            dest.Compress();

            if (src.Count == 0)
            {
                return dest;
            }

            if (dest.Count == 0)
            {
                return src;
            }

            AscIntList result = new AscIntList(src.Count + dest.Count);

            int srcIndex = 0;
            int destIndex = 0;
            int srcLen = src.Count;
            int destLen = dest.Count;

            while (srcIndex < srcLen && destIndex < destLen)
            {
                int sValue = src[srcIndex] ;
                int dValue = dest[destIndex];
                if (sValue < dValue)
                {
                    result.Add(sValue);
                    srcIndex++;
                }
                else if (sValue > dValue)
                {
                    result.Add(dValue);
                    destIndex++;
                }
                else
                {
                    result.Add(sValue);
                    srcIndex++;
                    destIndex++;
                }
            }

            if (srcIndex < srcLen)
            {
                for (int i = srcIndex; i < srcLen; i++)
                {
                    result.Add(src[i]);
                }
            }

            if (destIndex < destLen)
            {
                for (int i = destIndex; i < destLen; i++)
                {
                    result.Add(dest[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Merge two lists as and
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static AscIntList MergeAnd(AscIntList src, AscIntList dest)
        {
            if (src == null)
            {
                return new AscIntList();
            }

            if (dest == null)
            {
                return new AscIntList();
            }

            src.Compress();
            dest.Compress();

            if (src.Count == 0)
            {
                return src;
            }

            if (dest.Count == 0)
            {
                return dest;
            }

            AscIntList result = new AscIntList(Math.Min(src.Count, dest.Count));

            int srcIndex = 0;
            int destIndex = 0;
            int srcLen = src.Count;
            int destLen = dest.Count;

            while (srcIndex < srcLen && destIndex < destLen)
            {
                int sValue = src[srcIndex];
                int dValue = dest[destIndex];
                if (sValue < dValue)
                {
                    srcIndex++;
                }
                else if (sValue > dValue)
                {
                    destIndex++;
                }
                else
                {
                    result.Add(sValue);
                    srcIndex++;
                    destIndex++;
                }
            }

            return result;
        }
    }
}
