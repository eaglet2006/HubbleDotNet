using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.IO;

namespace Hubble.Core.Data
{
    class PayloadMemoryBlock : HGlobalMemoryBlock
    {
        public int UsedCount = 0;

        private int _RealPayloadIntSize;

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
