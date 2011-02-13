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
        internal int HitCount;

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
            HitCount = 1;
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
            HitCount = 1;
        }
    }
}
