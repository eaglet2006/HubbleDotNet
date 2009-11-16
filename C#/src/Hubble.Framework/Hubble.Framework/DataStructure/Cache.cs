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
    public interface IManagedCache
    {
        string Name { get; }

        int MaxStair { get; }

        bool Clear(int stair);

        List<string> BucketInfoList { get; }
    }

    /// <summary>
    /// This class is using to create cache buffer 
    /// </summary>
    /// <typeparam name="T">type of cache item</typeparam>
    public abstract class Cache<T> : IManagedCache
    {
        #region Bucket

        private class Bucket : LinkedList<CacheItem>
        {
            private long _MemorySize;
            private FingerPrintDictionary<CacheItem> _Dict;
            private CacheManage _CacheManage;

            public long MemorySize
            {
                get
                {
                    return _MemorySize;
                }
            }


            private int _MaxHitCount;

            public int MaxHitCount
            {
                get
                {
                    return _MaxHitCount;
                }

            }

            public Bucket(FingerPrintDictionary<CacheItem> dict,
                int maxHitCount, CacheManage cacheMgr)
            {
                _Dict = dict;
                _MaxHitCount = maxHitCount;
                _CacheManage = cacheMgr;
            }

            public void IncSize(long size)
            {
                _MemorySize += size;
                _CacheManage.TotalMemorySize += size;
            }


            public new LinkedListNode<CacheItem> AddLast(CacheItem node)
            {
                _MemorySize += node.Size;
                _CacheManage.TotalMemorySize += node.Size;
                return base.AddLast(node);
            }

            public new void Remove(CacheItem node)
            {
                Remove(node, true);
            }

            public void Remove(CacheItem node, bool removeDict)
            {
                _MemorySize -= node.Size;
                _CacheManage.TotalMemorySize -= node.Size;

                if (removeDict)
                {
                    _Dict.Remove(node.Key);
                }

                base.Remove(node.Node);
            }

            public new void Clear()
            {
                LinkedListNode<CacheItem> node = this.First;

                while (this.Count > 0)
                {
                    Remove(node.Value);

                    node = this.First;
                }

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

            private Bit16Int _Key;

            internal Bit16Int Key
            {
                get
                {
                    return _Key;
                }

                set
                {
                    _Key = value;
                }
            }


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
        }
        
        #endregion

        #region Private fields

        private object _LockObj = new object();

        private FingerPrintDictionary<CacheItem> _Dict = new FingerPrintDictionary<CacheItem>();

        private int[] _Stairs = { 10, 100, 1000, 10000, 100000, 1000000, 10000000};

        private Bucket[] _Buckets;
        private CacheManage _CacheManage;

        #endregion

        #region Protected fields

        protected string m_Name = "";

        #endregion

        #region abstract members

        protected abstract byte[] GetBytes(T data);
        protected abstract T GetData(byte[] buf);

        #endregion

        #region Private methods

        private void Init()
        {
            Array.Sort(_Stairs);

            _Buckets = new Bucket[_Stairs.Length];

            for (int i = 0; i < _Buckets.Length; i++)
            {
                _Buckets[i] = new Bucket(_Dict, _Stairs[i], _CacheManage);
            }
        }


        #endregion

        public Cache(CacheManage cacheMgr, int[] stairs)
        {
            if (stairs != null)
            {
                _Stairs = stairs;
            }

            _CacheManage = cacheMgr;
            _CacheManage.Add(this);
            Init();
        }

        public Cache(CacheManage cacheMgr) 
            : this (cacheMgr, null)
        {
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
                CacheItem node;

                if (_Dict.TryGetValue(key, out node))
                {
                    node.ExpireTime = expireTime;

                    long oldSize = node.Size;

                    node.Value = item;

                    _Buckets[node.Stair].IncSize(node.Size - oldSize);
                }
                else
                {
                    CacheItem cacheItem = new Cache<T>.CacheItem(item, expireTime, this, 0);

                    LinkedListNode<CacheItem> n = _Buckets[0].AddLast(cacheItem);

                    cacheItem.Node = n;

                    node = n.Value;

                    Bit16Int md5Key;
                    _Dict.Add(key, node, out md5Key);

                    node.Key = md5Key;

                }
            }

            _CacheManage.Collect();
        }

        public bool TryGetValue(string key, out T value, out DateTime expireTime, out int hitCount)
        {
            lock (_LockObj)
            {
                CacheItem node;
                if (_Dict.TryGetValue(key, out node))
                {
                    value = node.Value;
                    expireTime = node.ExpireTime;
                    
                    if (node.HitCount <= int.MaxValue)
                    {
                        node.HitCount++;

                        if (node.HitCount > _Buckets[node.Stair].MaxHitCount)
                        {
                            if (node.Stair < _Buckets.Length - 1)
                            {
                                _Buckets[node.Stair].Remove(node, false);
                                node.Stair++;
                                node.Node = _Buckets[node.Stair].AddLast(node);
                            }
                        }
                    }

                    hitCount = node.HitCount;

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

        public void Remove(string key)
        {
            lock (_LockObj)
            {
                CacheItem node;
                if (_Dict.TryGetValue(key, out node))
                {
                    _Buckets[node.Stair].Remove(node);
                }
            }
        }

        #endregion

        #region IManagedCache Members

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public int MaxStair
        {
            get 
            {
                return _Stairs.Length;    
            }
        }

        public bool Clear(int stair)
        {
            lock (_LockObj)
            {
                if (stair >= _Buckets.Length)
                {
                    return false;
                }

                _Buckets[stair].Clear();

                return true;
            }

        }

        public List<string> BucketInfoList
        {
            get
            {
                List<string> result = new List<string>();

                lock (_LockObj)
                {
                    for (int i = 0; i < _Buckets.Length; i++)
                    {
                        result.Add(string.Format("MaxHitCount:{0} MemorySize:{1} Items:{2}",
                            _Buckets[i].MaxHitCount, _Buckets[i].MemorySize, _Buckets[i].Count));
                    }
                }

                return result;

            }
        }

        #endregion
    }
}
