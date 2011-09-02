using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Core.Query
{
    unsafe struct DocIdPayloadData
    {
        public int* PayloadData;
        public int DocId;

        public DocIdPayloadData(int docId, int* payloadData)
        {
            this.DocId = docId;
            this.PayloadData = payloadData;
        }
    }
}
