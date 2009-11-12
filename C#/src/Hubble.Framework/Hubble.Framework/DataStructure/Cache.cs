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
    /// This class is using to create cache buffer 
    /// </summary>
    /// <typeparam name="T">type of cache item</typeparam>
    public abstract class Cache<T>
    {
        #region Bucket

        private class Bucket : LinkedList<CacheItem>
        {
            private long _MemorySize;

            public long MemorySize
            {
                get
                {
                    return _MemorySize;
                }
            }

            public new LinkedListNode<CacheItem> AddLast(CacheItem node)
            {
                _MemorySize += node.Size;
                return base.AddLast(node);
            }

            public new void Remove(LinkedListNode<CacheItem> node)
            {
                _MemorySize -= node.Value.Size;
                base.Remove(node);
            }
        }

        #endregion


        #region CacheItem

        private class CacheItem
        {
            private Cache<T> _Cache;

            private DateTime _ExpireTime;
            private int _HitCount;
            private byte[] _Buf = null;

            private int _Stair;
            private LinkedListNode<CacheItem> _Node;

            public int Size
            {
                get
                {
                    lock (this)
                    {
                        if (_Buf == null)
                        {
                            return 32; //approximate size exclude buf of CacheItem.
                        }
                        else
                        {
                            return _Buf.Length + 32;
                        }
                    }
                }
            }

            public DateTime ExpireTime
            {
                get
                {
                    lock (this)
                    {
                        return _ExpireTime;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _ExpireTime = value;
                    }
                }
            }

            public int HitCount
            {
                get
                {
                    lock (this)
                    {
                        return _HitCount;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _HitCount = value;
                    }
                }
            }

            public int Stair
            {
                get
                {
                    lock (this)
                    {
                        return _Stair;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _Stair = value;
                    }
                }
            }

            public LinkedListNode<CacheItem> Node
            {
                get
                {
                    lock (this)
                    {
                        return _Node;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _Node = value;
                    }
                }
            }

            public T Value
            {
                get
                {
                    lock (this)
                    {
                        return _Cache.GetData(_Buf);
                    }
                }

                set
                {
                    lock (this)
                    {
                        _Buf = _Cache.GetBytes(value);
                    }
                }

            }

            public CacheItem(T data, DateTime expireTime, Cache<T> cache, int stair)
            {
                _Stair = stair;
                _HitCount = 0;
                _ExpireTime = expireTime;

                _Cache = cache;
                Value = data;
            }

            public void Remove()
            {
                lock (this)
                {
                    _Cache._Buckets[_Stair].Remove(_Node);
                }
            }
        }
        
        #endregion

        #region Private fields

        private object _LockObj = new object();

        private FingerPrintDictionary<LinkedListNode<CacheItem>> _Dict = new FingerPrintDictionary<LinkedListNode<CacheItem>>();

        private int[] _Stairs = {10, 100, 1000, 10000, 100000, 1000000, Int16.MaxValue};

        private Bucket[] _Buckets;

        #endregion

        #region abstract members

        protected abstract byte[] GetBytes(T data);
        protected abstract T GetData(byte[] buf);

        #endregion

        #region Private methods

        private void Init()
        {
            _Buckets = new Bucket[_Stairs.Length];

            for (int i = 0; i < _Buckets.Length; i++)
            {
                _Buckets[i] = new Bucket();
            }
        }


        #endregion

        public Cache()
        {
            Init();
        }


        #region Public methods

        /// <summary>
        /// Insert data by key
        /// If key is already exist, update expire time, else insert it.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="item">data</param>
        /// <param name="expireTime">expire time</param>
        public void Insert(string key, T item, DateTime expireTime)
        {
            lock (_LockObj)
            {
                LinkedListNode<CacheItem> node;

                if (_Dict.TryGetValue(key, out node))
                {
                    node.Value.ExpireTime = expireTime;
                }
                else
                {
                    CacheItem cacheItem = new Cache<T>.CacheItem(item, expireTime, this, 0);
                    
                    LinkedListNode<CacheItem> n = _Buckets[0].AddLast(cacheItem);

                    cacheItem.Node = n;

                    _Dict.Add(key, node);


                }
            }
        }

        public bool TryGetValue(string key, out T value, out DateTime expireTime, out int hitCount)
        {
            lock (_LockObj)
            {
                LinkedListNode<CacheItem> node;
                if (_Dict.TryGetValue(key, out node))
                {
                    value = node.Value.Value;
                    expireTime = node.Value.ExpireTime;
                    hitCount = node.Value.HitCount;

                    return true;
                }
                else
                {
                    value = default(T);
                    expireTime = default(DateTime);
                    hitCount = 0;
                    return false;
                }
            }
        }

        #endregion
    }
}
