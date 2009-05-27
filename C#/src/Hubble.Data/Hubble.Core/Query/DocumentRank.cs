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

namespace Hubble.Core.Query
{
    public struct DocumentRank : IComparable<DocumentRank>
    {
        public long DocumentId;
        public int Rank;

        public DocumentRank(long docId)
            : this(docId, 1)
        {

        }

        public DocumentRank(long docId, int rank)
        {
            DocumentId = docId;
            Rank = rank;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", DocumentId, Rank);
        }

        #region IComparable<DocumentRank> Members

        public int CompareTo(DocumentRank other)
        {
            return 0 - Rank.CompareTo(other.Rank);
        }

        #endregion
    }
}
