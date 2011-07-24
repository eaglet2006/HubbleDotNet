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

namespace Hubble.Framework.DataStructure
{
    /// <summary>
    /// This class is the dictionary which the key is type of int.
    /// This dictioinary uses two level index to store and index data.
    /// Usually used in sequential keys or approximately sequential.
    /// </summary>
    public class IntDictionary<T> : IDictionary<int, T>
    {
        sealed class Bucket
        {
            internal byte[] Used;
            internal T[] Data;

            internal Bucket(int bucketSize)
            {
                Used = new byte[bucketSize];
                Data = new T[bucketSize];
            }
        }

        sealed class KeyCollection : ICollection<int>
        {
            private IntDictionary<T> _Dictionary;

            internal KeyCollection(IntDictionary<T> dictionary)
            {
                _Dictionary = dictionary;
            }

            #region ICollection<int> Members

            public void Add(int item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(int item)
            {
                return _Dictionary.ContainsKey(item);
            }

            public void CopyTo(int[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { return _Dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(int item)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable<int> Members

            public IEnumerator<int> GetEnumerator()
            {
                if (_Dictionary._Negative != null)
                {
                    foreach (int key in _Dictionary._Negative.Keys)
                    {
                        yield return 0 - key;
                    }
                }

                for (int i = 0; i < _Dictionary._Index.Length; i++)
                {
                    if (_Dictionary._Index[i] == null)
                    {
                        continue;
                    }

                    int baseNum = _Dictionary._BucketSize * i;

                    for (int j = 0; j < _Dictionary._Index[i].Used.Length; j++)
                    {
                        if (_Dictionary._Index[i].Used[j] != 0)
                        {
                            yield return baseNum + j;
                        }
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion
        }


        sealed class ValueCollection : ICollection<T>
        {
            private IntDictionary<T> _Dictionary;

            internal ValueCollection(IntDictionary<T> dictionary)
            {
                _Dictionary = dictionary;
            }

            #region ICollection<T> Members

            public void Add(T item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(T item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get
                {
                    return _Dictionary.Count;
                }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(T item)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator()
            {
                if (_Dictionary._Negative != null)
                {
                    foreach (T value in _Dictionary._Negative.Values)
                    {
                        yield return value;
                    }
                }

                for (int i = 0; i < _Dictionary._Index.Length; i++)
                {
                    if (_Dictionary._Index[i] == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < _Dictionary._Index[i].Used.Length; j++)
                    {
                        if (_Dictionary._Index[i].Used[j] != 0)
                        {
                            yield return _Dictionary._Index[i].Data[j];
                        }
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        int _Count;
        int _BucketSize;
        int _Capability;
        Bucket[] _Index;

        private IntDictionary<T> _Negative = null;

        private void Resize(int key)
        {
            if (key >= _Capability)
            {
                _Capability = ((key / _Capability) + 1) * _Capability;

                Bucket[] newIndex = new Bucket[_Capability / _BucketSize];

                Array.Copy(_Index, newIndex, _Index.Length);

                _Index = newIndex;
            }
        }

        public IntDictionary()
            : this(10 * 1024 * 1024, 64 * 1024)
        {

        }

        public IntDictionary(int capability, int bucketSize)
        {
            if (bucketSize < 1024)
            {
                bucketSize = 1024;
            }

            _BucketSize = bucketSize;

            if (capability < 1024 * 1024)
            {
                capability = 1024 * 1024;
            }

            if (capability < _BucketSize)
            {
                capability = _BucketSize;
            }

            int detal = capability % bucketSize;

            if (detal != 0)
            {
                capability += detal;
            }

            _Capability = capability;

            _Count = 0;
            _Index = new Bucket[_Capability / _BucketSize];
        }

        /// <summary>
        /// if key doesn't exist, add it else update it
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdate(int key, T value)
        {

            if (key < 0)
            {
                if (_Negative == null)
                {
                    _Negative = new IntDictionary<T>(_Capability, _BucketSize);
                }

                _Negative.Add(0 - key, value);
                return;
            }

            if (key >= _Capability)
            {
                Resize(key);
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                _Index[index] = new Bucket(_BucketSize);
            }

            int subIndex = key % _BucketSize;

            if (_Index[index].Used[subIndex] != 0)
            {
                _Index[index].Data[subIndex] = value;
                return;
            }

            _Count++;
            _Index[index].Used[subIndex] = 1;
            _Index[index].Data[subIndex] = value;
        }

        #region IDictionary<int,T> Members

        public void Add(int key, T value)
        {
            if (key < 0)
            {
                if (_Negative == null)
                {
                    _Negative = new IntDictionary<T>(_Capability, _BucketSize);
                }

                _Negative.Add(0 - key, value);
                return;
            }

            if (key >= _Capability)
            {
                Resize(key);
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                _Index[index] = new Bucket(_BucketSize);
            }

            int subIndex = key % _BucketSize;

            if (_Index[index].Used[subIndex] != 0)
            {
                throw new System.ArgumentException("Adding duplicate key");
            }

            _Count++;
            _Index[index].Used[subIndex] = 1;
            _Index[index].Data[subIndex] = value;
        }

        public bool ContainsKey(int key)
        {
            if (key < 0)
            {
                if (_Negative == null)
                {
                    return false;
                }
                else
                {
                    return _Negative.ContainsKey(0 - key);
                }
            }

            if (key >= _Capability)
            {
                return false;
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                return false;
            }

            int subIndex = key % _BucketSize;

            return _Index[index].Used[subIndex] != 0;
        }

        public ICollection<int> Keys
        {
            get
            {
                return new KeyCollection(this);
            }
        }

        public bool Remove(int key)
        {
            if (key < 0)
            {
                if (_Negative == null)
                {
                    return false;
                }
                else
                {
                    return _Negative.Remove(0 - key);
                }
            }

            if (key >= _Capability)
            {
                return false;
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                return false;
            }

            int subIndex = key % _BucketSize;

            if (_Index[index].Used[subIndex] == 0)
            {
                return false;
            }

            _Index[index].Used[subIndex] = 0;
            _Count--;
            return true;
        }

        public bool TryGetValue(int key, out T value)
        {
            if (key < 0)
            {
                if (_Negative == null)
                {
                    value = default(T);
                    return false;
                }
                else
                {
                    return _Negative.TryGetValue(0 - key, out value);
                }
            }

            if (key >= _Capability)
            {
                value = default(T);
                return false;
            }

            int index = key / _BucketSize;

            if (_Index[index] == null)
            {
                value = default(T);
                return false;
            }

            int subIndex = key % _BucketSize;

            if (_Index[index].Used[subIndex] == 0)
            {
                value = default(T);
                return false;
            }

            value = _Index[index].Data[subIndex];

            return true;
        }

        public ICollection<T> Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }

        public T this[int key]
        {
            get
            {
                if (key < 0)
                {
                    if (_Negative == null)
                    {
                        throw new System.Collections.Generic.KeyNotFoundException(string.Format("Key = {0} does not find", key));
                    }
                    else
                    {
                        return _Negative[0 - key];
                    }
                }

                if (key >= _Capability)
                {
                    throw new System.Collections.Generic.KeyNotFoundException(string.Format("Key = {0} does not find", key));
                }

                int index = key / _BucketSize;

                if (_Index[index] == null)
                {
                    throw new System.Collections.Generic.KeyNotFoundException(string.Format("Key = {0} does not find", key));
                }

                int subIndex = key % _BucketSize;

                if (_Index[index].Used[subIndex] == 0)
                {
                    throw new System.Collections.Generic.KeyNotFoundException(string.Format("Key = {0} does not find", key));
                }

                return _Index[index].Data[subIndex];
            }

            set
            {
                if (key < 0)
                {
                    if (_Negative == null)
                    {
                        throw new System.Collections.Generic.KeyNotFoundException(string.Format("Key = {0} does not find", key));
                    }
                    else
                    {
                        _Negative[0 - key] = value;
                    }

                    return;
                }

                if (key >= _Capability)
                {
                    throw new System.Collections.Generic.KeyNotFoundException(string.Format("Key = {0} does not find", key));
                }

                int index = key / _BucketSize;

                if (_Index[index] == null)
                {
                    throw new System.Collections.Generic.KeyNotFoundException(string.Format("Key = {0} does not find", key));
                }

                int subIndex = key % _BucketSize;

                if (_Index[index].Used[subIndex] == 0)
                {
                    throw new System.Collections.Generic.KeyNotFoundException(string.Format("Key = {0} does not find", key));
                }

                _Count++;
                _Index[index].Data[subIndex] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<int,T>> Members

        public void Add(KeyValuePair<int, T> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _Count = 0;
            _Index = new Bucket[_Capability / _BucketSize];
            _Negative = null;
        }

        public bool Contains(KeyValuePair<int, T> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                if (_Negative != null)
                {
                    return _Count + _Negative.Count;
                }

                return _Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<int, T> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<int,T>> Members

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
