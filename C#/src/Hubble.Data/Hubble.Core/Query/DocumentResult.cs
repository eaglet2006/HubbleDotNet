using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Serialization = Hubble.SQLClient.QueryResultSerialization;

namespace Hubble.Core.Query
{
    [StructLayout(LayoutKind.Auto)]
    unsafe public struct DocumentResult 
    {
        public long Score;
        public int* PayloadData;
        public int DocId;

        internal int LastPosition;
        internal UInt16 LastIndex;
        internal UInt16 LastCount;

        internal UInt16 LastWordIndexFirstPosition;
        internal UInt16 LastWordIndexQueryCount;

        public DocumentResult(int docId)
            :this(docId, 1)
        {
        }

        public DocumentResult(int docId, long score, int lastWordIndexFirstPosition,
            int lastWordIndexQueryCount, int lastPostion, int lastCount, int lastIndex)
            : this(docId, score, (int*)null)
        {
            this.LastWordIndexFirstPosition = (UInt16)lastWordIndexFirstPosition;

            if (lastWordIndexQueryCount > UInt16.MaxValue)
            {
                this.LastWordIndexQueryCount = UInt16.MaxValue;
            }
            else
            {
                this.LastWordIndexQueryCount = (UInt16)lastWordIndexQueryCount;
            }

            this.LastCount = (UInt16)lastCount;
            this.LastIndex = (UInt16)lastIndex;
            this.LastPosition = lastPostion;
        }

        public DocumentResult(int docId, long score)
            : this(docId, score, (int*)null)
        {
        }

        public DocumentResult(int docId, long score, int* payload)
        {
            LastWordIndexQueryCount = 0;
            LastWordIndexFirstPosition = 0;

            DocId = docId;
            Score = score;
            PayloadData = payload;
            LastPosition = 0;
            LastCount = 0;
            LastIndex = 0;
        }
    }
}
