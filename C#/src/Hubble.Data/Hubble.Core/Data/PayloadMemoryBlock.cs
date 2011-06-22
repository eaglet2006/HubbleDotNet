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
using Hubble.Framework.IO;

namespace Hubble.Core.Data
{
    class PayloadMemoryBlock : HGlobalMemoryBlock
    {
        public int UsedCount = 0;

        readonly private int _RealPayloadIntSize;

        private unsafe int BinarySearch(int index, int length, int value)
        {
            int* objArray = (int*)Ptr;

            int lo = index;
            int hi = index + length - 1;

            while (lo <= hi)
            {
                // i might overflow if lo and hi are both large positive numbers.
                int i = lo + ((hi - lo) >> 1);

                int c;

                if (objArray[i * _RealPayloadIntSize]  > value)
                {
                    c = 1;
                }
                else if (objArray[i * _RealPayloadIntSize] < value)
                {
                    c = -1;
                }
                else
                {
                    c = 0;
                }

                if (c == 0) return i;
                if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }


        public unsafe int* Find(int docId)
        {
            if (UsedCount <= 0)
            {
                return null;
            }

            int* objArray = (int*)Ptr;
            int firstDocId = objArray[0];
            int lastDocId = objArray[(UsedCount - 1) * _RealPayloadIntSize];

            if (lastDocId - firstDocId == UsedCount - 1)
            {
                if (docId - firstDocId > UsedCount - 1)
                {
                    return null;
                }

                return ((int*)Ptr) + (docId - firstDocId) * _RealPayloadIntSize;
            }

            int index = BinarySearch(0, UsedCount, docId);

            if (index < 0)
            {
                return null;
            }
            else
            {
                return ((int*)Ptr) + index * _RealPayloadIntSize;
            }
        }

        public PayloadMemoryBlock(int size, int realPayloadSize)
            : base(size)
        {
            _RealPayloadIntSize = realPayloadSize / 4;
        }
    }
}
