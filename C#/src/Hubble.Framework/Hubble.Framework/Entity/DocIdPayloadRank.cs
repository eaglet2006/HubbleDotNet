using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Core.Entity
{
    unsafe public struct DocIdPayloadRank
    {
        public int DocId;
        public int Count;

        public DocIdPayloadRank(int docId, int count)
        {
            DocId = docId;
            Count = count;
        }
    }
}
