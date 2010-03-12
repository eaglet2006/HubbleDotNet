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

namespace Hubble.Core.Data
{
    class ReplaceFieldValueToDocId
    {
        Dictionary<int, int> _ReplaceFieldValueIntToDocId = null;
        Dictionary<long, int> _ReplaceFieldValueLongToDocId = null;

        internal ReplaceFieldValueToDocId(bool isLong)
        {
            if (isLong)
            {
                _ReplaceFieldValueLongToDocId = new Dictionary<long, int>();
            }
            else
            {
                _ReplaceFieldValueIntToDocId = new Dictionary<int, int>();
            }
        }

        internal void Add(int value, int docId)
        {
            _ReplaceFieldValueIntToDocId.Add(value, docId);
        }

        internal void Add(long value, int docId)
        {
            _ReplaceFieldValueLongToDocId.Add(value, docId);
        }

        internal void Remove(int value)
        {
            _ReplaceFieldValueIntToDocId.Remove(value);
        }

        internal void Remove(long value)
        {
            _ReplaceFieldValueLongToDocId.Remove(value);
        }

        internal bool TryGetValue(int key, out int docId)
        {
            return _ReplaceFieldValueIntToDocId.TryGetValue(key, out docId);
        }

        internal bool TryGetValue(long key, out int docId)
        {
            return _ReplaceFieldValueLongToDocId.TryGetValue(key, out docId);
        }

        internal bool ContainsKey(int key)
        {
            return _ReplaceFieldValueIntToDocId.ContainsKey(key);
        }

        internal bool ContainsKey(long key)
        {
            return _ReplaceFieldValueLongToDocId.ContainsKey(key);
        }

        internal int this[int key]
        {
            get
            {
                return _ReplaceFieldValueIntToDocId[key];
            }

            set
            {
                _ReplaceFieldValueIntToDocId[key] = value;
            }
        }

        internal int this[long key]
        {
            get
            {
                return _ReplaceFieldValueLongToDocId[key];
            }

            set
            {
                _ReplaceFieldValueLongToDocId[key] = value;
            }
        }
    }
}
