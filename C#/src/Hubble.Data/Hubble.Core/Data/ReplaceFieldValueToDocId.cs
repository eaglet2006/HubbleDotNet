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
