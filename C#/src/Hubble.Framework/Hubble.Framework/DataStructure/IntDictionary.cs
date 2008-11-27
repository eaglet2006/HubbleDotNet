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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Framework.DataStructure
{
    /// <summary>
    /// UIntDictinary is a special dictinary for UInt 
    /// Base arithmetic: Hash
    /// </summary>
    /// <typeparam name="TValue">Value</typeparam>
    public class IntDictionary<TValue> : IDictionary<int, TValue>
    {
        enum Operator
        {
            Query = 0,
            Add = 1,
            Get = 2,
            Set = 3,
            Remove = 4,
        }

        #region DataEntity

        [Serializable]
        public struct DataEntity
        {
            public int Used;
            public TValue Value;
        }


        #endregion

        #region Node class

        /// <summary>
        /// Node
        /// </summary>
        [Serializable]
        class Node 
        {
            int _Count;
            int _FirstKey;
            int _ChildrenNumber;
            Node _Parent;
            int _ParentIndex;
            Node[] _Children;
            DataEntity[] _DataList;
            int _MaxChildren;
            Node _Prev;
            Node _Next;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="isLeafNode">is leaf node or not</param>
            /// <param name="maxChildren">max children number</param>
            public Node(bool isLeafNode, int maxChildren)
            {
                _MaxChildren = maxChildren;

                if (isLeafNode)
                {
                    _DataList = new DataEntity[_MaxChildren];

                    _Children = null;
                }
                else
                {
                    _DataList = null;
                    _Children = new Node[_MaxChildren];
                }
            }

            #region Public properties

            /// <summary>
            /// Get the node is leaf node or not
            /// </summary>
            public bool IsLeafNode
            {
                get
                {
                    return DataList != null;
                }
            }

            /// <summary>
            /// Count of the data entities below this node
            /// </summary>
            public int FirstKey
            {
                get
                {
                    return _FirstKey;
                }

                set
                {
                    _FirstKey = value;
                }
            }



            /// <summary>
            /// Count of the data entities below this node
            /// </summary>
            public int Count
            {
                get
                {
                    return _Count;
                }

                set
                {
                    _Count = value;
                }
            }

            /// <summary>
            /// The children number of this node
            /// If this is leaf node, it is the number of data
            /// </summary>
            public int ChildrenNumber
            {
                get
                {
                    return _ChildrenNumber;
                }

                set
                {
                    _ChildrenNumber = value;
                }
            }

            /// <summary>
            /// The parent node of this node.
            /// If parent is null, this is the root node
            /// </summary>
            public Node Parent
            {
                get
                {
                    return _Parent;
                }

                set
                {
                    _Parent = value;
                }
            }

            /// <summary>
            /// Index of parent node
            /// </summary>
            public int ParentIndex
            {
                get
                {
                    return _ParentIndex;
                }

                set
                {
                    _ParentIndex = value;
                }
            }

            
            /// <summary>
            /// Children of this node.
            /// If children is null, this is leaf node
            /// </summary>
            public Node[] Children
            {
                get
                {
                    return _Children;
                }
  
            }

            /// <summary>
            /// Data list 
            /// only for leaf nodes
            /// </summary>
            public DataEntity[] DataList
            {
                get
                {
                    return _DataList;
                }
            }


            /// <summary>
            /// Only leaf node use it.
            /// Prev leaf node
            /// </summary>
            public Node Prev
            {
                get
                {
                    return _Prev;
                }

                set
                {
                    _Prev = value;
                }
            }

            /// <summary>
            /// Only leaf node use it.
            /// Next leaf node
            /// </summary>
            public Node Next
            {
                get
                {
                    return _Next;
                }

                set
                {
                    _Next = value;
                }
            }

            #endregion
        }

        #endregion

        #region Collection classes

        [Serializable()]
        public sealed class KeyCollection : ICollection<int>, ICollection
        {
            private IntDictionary<TValue> _Dictionary;

            public KeyCollection(IntDictionary<TValue> dictionary)
            {
                _Dictionary = dictionary;
            }

            #region ICollection members

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array is null!");
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException("Arg_NonZeroLowerBound");
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(string.Format("index={0} out of range", index));
                }

                if (array.Length - index < _Dictionary.Count)
                {
                    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
                }

                int i = 0;
                int[] keys = array as int[];
                foreach (int key in _Dictionary.GetKeys())
                {
                    keys[i] = key;
                    i++;
                }
            }

            public int Count
            {
                get 
                {
                    return _Dictionary.Count;
                }
            }

            public bool IsSynchronized
            {
                get 
                {
                    return false;
                }
            }

            public object SyncRoot
            {
                get 
                {
                    return false;
                }
            }

            #endregion

            #region IEnumerable members

            public IEnumerator GetEnumerator()
            {
                foreach (int key in _Dictionary.GetKeys())
                {
                    yield return key;
                }
            }

            #endregion

            #region ICollection<int> Members

            public void Add(int item)
            {
                throw new Exception("Not supported KeyCollectionSet");
            }

            public void Clear()
            {
                throw new Exception("Not supported KeyCollectionSet");
            }

            public bool Contains(int item)
            {
                return _Dictionary.ContainsKey(item);
            }

            public void CopyTo(int[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array is null!");
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException("Arg_NonZeroLowerBound");
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(string.Format("index={0} out of range", index));
                }

                if (array.Length - index < _Dictionary.Count)
                {
                    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
                }

                int i = 0;
                foreach (int key in _Dictionary.GetKeys())
                {
                    array[i] = key;
                    i++;
                }
            }

            public bool IsReadOnly
            {
                get 
                {
                    return true;    
                }
            }

            public bool Remove(int item)
            {
                throw new Exception("Not supported KeyCollectionSet");
            }

            #endregion

            #region IEnumerable<int> Members

            IEnumerator<int> IEnumerable<int>.GetEnumerator()
            {
                foreach (int key in _Dictionary.GetKeys())
                {
                    yield return key;
                }
            }

            #endregion
        }

        [Serializable()]
        public sealed class ValueCollection : ICollection<TValue>, ICollection
        {
            private IntDictionary<TValue> _Dictionary;

            public ValueCollection(IntDictionary<TValue> dictionary)
            {
                _Dictionary = dictionary;
            }

            #region ICollection members

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array is null!");
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException("Arg_NonZeroLowerBound");
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(string.Format("index={0} out of range", index));
                }

                if (array.Length - index < _Dictionary.Count)
                {
                    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
                }

                int i = 0;
                TValue[] values = array as TValue[];
                foreach (TValue value in _Dictionary.GetValues())
                {
                    values[i] = value;
                    i++;
                }
            }

            public int Count
            {
                get
                {
                    return _Dictionary.Count;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return false;
                }
            }

            #endregion

            #region IEnumerable members

            public IEnumerator GetEnumerator()
            {
                foreach (TValue value in _Dictionary.GetValues())
                {
                    yield return value;
                }
            }

            #endregion

            #region ICollection<TValue> Members

            public void Add(TValue item)
            {
                throw new Exception("Not supported KeyCollectionSet");
            }

            public void Clear()
            {
                throw new Exception("Not supported KeyCollectionSet");
            }

            public bool Contains(TValue item)
            {
                return _Dictionary.ContainsValue(item);
            }

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array is null!");
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException("Arg_NonZeroLowerBound");
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(string.Format("index={0} out of range", index));
                }

                if (array.Length - index < _Dictionary.Count)
                {
                    throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
                }

                int i = 0;
                foreach (TValue value in _Dictionary.GetValues())
                {
                    array[i] = value;
                    i++;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public bool Remove(TValue item)
            {
                throw new Exception("Not supported KeyCollectionSet");
            }

            #endregion

            #region IEnumerable<TValue> Members

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                foreach (TValue value in _Dictionary.GetValues())
                {
                    yield return value;
                }
            }

            #endregion
        }


        #endregion

        #region Private fields

        private int _Height = 0;  //Height of the tree
        private int _MaxChildren; //Max children number of each node.
        private List<int> _PowPool; //Caculate Pow and Log
        private int _MinFreeKey = 0; //Used by getting free key automaticly.

        /// <summary>
        /// The root node
        /// </summary>
        Node _Root = null;

        #endregion

        #region Public properties

        /// <summary>
        /// The maximum children number of every nodes
        /// </summary>
        public int MaxChildren
        {
            get
            {
                return _MaxChildren;
            }

            set
            {
                _MaxChildren = value;
                _PowPool = new List<int>();

                long pow = 0;
                int times = 0;
                _PowPool.Add((int)pow);
                pow = (long)Math.Pow(_MaxChildren, ++times);

                while (pow <= int.MaxValue)
                {
                    _PowPool.Add((int)pow);
                    pow = (long)Math.Pow(_MaxChildren, ++times);
                }

            }
        }

        /// <summary>
        /// The height of the tree
        /// </summary>
        public int Height
        {
            get
            {
                return _Height;
            }
        }

        #endregion


        #region Constructor

        public IntDictionary(int maxChildren)
        {
            MaxChildren = maxChildren;
        }

        public IntDictionary()
            : this(2048)
        {
        }

        #endregion

        #region Private methods

        private Node GetFirstLeafNode()
        {
            if (_Root == null)
            {
                return null;
            }

            Node cur = _Root;

            while (!cur.IsLeafNode)
            {
                foreach (Node node in cur.Children)
                {
                    if (node != null)
                    {
                        cur = node;
                        break;
                    }
                }
            }

            return cur;
        }

        private Node GetLastLeafNode()
        {
            if (_Root == null)
            {
                return null;
            }

            Node cur = _Root;

            while (!cur.IsLeafNode)
            {
                for (int i = MaxChildren - 1; i >= 0; i--)
                {
                    Node node = cur.Children[i];
                    
                    if (node != null)
                    {
                        cur = node;
                        break;
                    }
                }
            }

            return cur;
        }


        private Node GetPrevLeafNode(Node curLeafNode)
        {
            Node cur = curLeafNode;
            int index = cur.ParentIndex;
            cur = cur.Parent;

            while (cur != null)
            {
                while (!cur.IsLeafNode)
                {
                    while (--index >= 0)
                    {
                        if (cur.Children[index] != null)
                        {
                            cur = cur.Children[index];
                            index = (int)MaxChildren;
                            break;
                        }
                    }

                    if (index < 0)
                    {
                        break;
                    }
                }

                if (cur.IsLeafNode)
                {
                    return cur;
                }

                index = cur.ParentIndex;
                cur = cur.Parent;
            }

            return null;

        }


        private Node GetNextLeafNode(Node curLeafNode)
        {
            Node cur = curLeafNode;
            int index = cur.ParentIndex;
            cur = cur.Parent;

            while (cur != null)
            {
                while (!cur.IsLeafNode)
                {
                    while (++index < (int)MaxChildren)
                    {
                        if (cur.Children[index] != null)
                        {
                            cur = cur.Children[index];
                            index = -1;
                            break;
                        }
                    }

                    if (index >= (int)MaxChildren)
                    {
                        break;
                    }
                }

                if (cur.IsLeafNode)
                {
                    return cur;
                }

                index = cur.ParentIndex;
                cur = cur.Parent;

            }

            return null;

        }

        private IEnumerable<KeyValuePair<int, TValue>> GetKeyValuePairs()
        {
            Node cur = GetFirstLeafNode();

            while (cur != null)
            {
                int i = 0;
                foreach (DataEntity entity in cur.DataList)
                {
                    if (entity.Used != 0)
                    {
                        yield return new KeyValuePair<int, TValue>(i + cur.FirstKey, entity.Value);
                    }

                    i++;
                }

                cur = cur.Next;
            }
        }

        public IEnumerable<TValue> GetValuesDesc()
        {
            Node cur = GetLastLeafNode();

            while (cur != null)
            {
                for (int i = MaxChildren - 1; i >= 0; i--)
                {
                    DataEntity entity = cur.DataList[i];
                    if (entity.Used != 0)
                    {
                        yield return entity.Value;
                    }
                }

                cur = cur.Prev;
            }
        }

        private IEnumerable<TValue> GetValues()
        {
            Node cur = GetFirstLeafNode();

            while (cur != null)
            {
                foreach (DataEntity entity in cur.DataList)
                {
                    if (entity.Used != 0)
                    {
                        yield return entity.Value;
                    }
                }

                cur = cur.Next;
            }
        }


        private IEnumerable<int> GetKeys()
        {
            Node cur = GetFirstLeafNode();

            while (cur != null)
            {
                int i = 0;
                foreach (DataEntity entity in cur.DataList)
                {
                    if (entity.Used != 0)
                    {
                        yield return i + cur.FirstKey;
                    }

                    i++;
                }

                cur = cur.Next;
            }
        }

        /// <summary>
        /// Construct the tree
        /// </summary>
        /// <param name="key"></param>
        private void Construct(int key)
        {
            #region Math.Log(key, MaxChildren) + 1

            int height = 0;
            foreach (int pow in _PowPool)
            {
                if (key >= pow)
                {
                    height++;
                }
                else
                {
                    break;
                }
            }

            #endregion

            if (height <= Height)
            {
                return;
            }

            if (Height == 0)
            {
                Node node = new Node(true, MaxChildren);
                Node leafNode = node;
                node.FirstKey = (key / MaxChildren) * MaxChildren;

                if (height == 1)
                {
                    _Root = node;
                    _Height = height;
                    return;
                }
                else
                {
                    _Height = 1;

                    while (--height > 0)
                    {
                        Node pNode = new Node(false, MaxChildren);

                        node.Parent = pNode;

                        _Height++;

                        #region Caculate pow and index
                        //long mPowh = (long)Math.Pow(MaxChildren, Height);
                        //int index = (int)((int)(key % mPowh) / (int)(mPowh / MaxChildren));

                        int index;

                        if (height >= _PowPool.Count)
                        {
                            index = (int)(key / _PowPool[_PowPool.Count - 1]);
                        }
                        else
                        {
                            index = (int)((key % _PowPool[height]) / (_PowPool[height] / MaxChildren));
                        }
                        #endregion


                        pNode.Children[index] = node;
                        node.ParentIndex = index;
                        node = pNode;
                    }

                    _Root = node;


                    //Add friend 
                    Node cur = leafNode;

                    if (cur.IsLeafNode)
                    {
                        Node prev = GetPrevLeafNode(cur);
                        Node next = GetNextLeafNode(cur);

                        if (prev != null)
                        {
                            cur.Prev = prev;
                            cur.Next = prev.Next;
                            prev.Next = cur;

                            if (cur.Next != null)
                            {
                                cur.Next.Prev = cur;
                            }
                        }
                        else if (next != null)
                        {
                            cur.Next = next;
                            cur.Prev = next.Prev;
                            next.Prev = cur;

                            if (cur.Prev != null)
                            {
                                cur.Prev.Next = cur;
                            }
                        }
                    }


                }
            }
            else
            {
                while (height > Height)
                {
                    Node pNode = new Node(false, MaxChildren);

                    pNode.Count = _Root.Count;
                    _Root.Parent = pNode;

                    _Height++;

                    pNode.Children[0] = _Root;
                    _Root.ParentIndex = 0;

                    _Root = pNode;
                }
            }
        }

        private bool Get(int key, out Node leafNode, out int index, Operator opr)
        {
            System.Diagnostics.Debug.Assert(opr == Operator.Get || opr == Operator.Query || opr == Operator.Remove);

            int height = Height;
            Node cur = _Root;
            leafNode = null;
            index = 0;

            while (height > 0)
            {
                #region Caculate pow and index
                //long mPowh = (long)Math.Pow(MaxChildren, Height);
                //int index = (int)((int)(key % mPowh) / (int)(mPowh / MaxChildren));

                if (height >= _PowPool.Count)
                {
                    index = (int)(key / _PowPool[_PowPool.Count - 1]);
                }
                else
                {
                    index = (int)((key % _PowPool[height]) / (_PowPool[height] / MaxChildren));
                }

                #endregion

                if (height > 1)
                {
                    //Branch node
                    if (cur.Children[index] == null)
                    {
                        return false;
                    }
                    else
                    {
                        cur = cur.Children[index];
                    }
                }
                else
                {
                    //Leaf node
                    leafNode = cur;
                    return cur.DataList[index].Used != 0;
                }

                height--;
            }

            return false;
        }

        /// <summary>
        /// Do something with the tree
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="opr">operator type</param>
        /// <returns>if find key return true</returns>
        private bool Set(int key, TValue value, Operator opr)
        {
            System.Diagnostics.Debug.Assert(opr == Operator.Add || opr == Operator.Set);

            if (key < 0)
            {
                throw new System.ArgumentException("Key must not be less than zero!");
            }

            int height = Height;
            Node cur = _Root;

            while (height > 0)
            {
                #region Caculate pow and index
                //long mPowh = (long)Math.Pow(MaxChildren, Height);
                //int index = (int)((int)(key % mPowh) / (int)(mPowh / MaxChildren));

                int index;

                if (height >= _PowPool.Count)
                {
                    index = (int)(key / _PowPool[_PowPool.Count - 1]);
                }
                else
                {
                    index = (int)((key % _PowPool[height]) / (_PowPool[height] / MaxChildren));
                }

                #endregion

                if (height > 1)
                {
                    //Branch node
                    if (cur.Children[index] == null)
                    {
                        Node node = new Node(height == 2, MaxChildren);

                        if (node.IsLeafNode)
                        {
                            node.FirstKey = (key / MaxChildren) * MaxChildren;
                        }

                        cur.Children[index] = node;
                        node.ParentIndex = index;
                        node.Parent = cur;
                        cur = node;

                        //Add friend 
                        if (cur.IsLeafNode)
                        {
                            Node prev = GetPrevLeafNode(cur);
                            Node next = GetNextLeafNode(cur);

                            if (prev != null)
                            {
                                cur.Prev = prev;
                                cur.Next = prev.Next;
                                prev.Next = cur;

                                if (cur.Next != null)
                                {
                                    cur.Next.Prev = cur;
                                }
                            }
                            else if (next != null)
                            {
                                cur.Next = next;
                                cur.Prev = next.Prev;
                                next.Prev = cur;

                                if (cur.Prev != null)
                                {
                                    cur.Prev.Next = cur;
                                }
                            }
                        }



                    }
                    else
                    {
                        cur = cur.Children[index];
                    }
                }
                else
                {
                    //Leaf node

                    if (opr == Operator.Set || (cur.DataList[index].Used == 0 && opr == Operator.Add))
                    {
                        //Set value
                        int used = cur.DataList[index].Used;
                        cur.DataList[index].Used = 1;
                        cur.DataList[index].Value = value;

                        //Inc count
                        if (used == 0)
                        {
                            while (cur != null)
                            {
                                cur.Count++;
                                cur = cur.Parent;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        throw new System.ArgumentException("An item with the same key has already been added.");
                    }
                }

                height--;
            }

            return false;

        }

        #endregion

        #region Public methods

        public int SortInsert(int key, TValue value)
        {
            _MinFreeKey = key;

            return Add(value);
        }

        /// <summary>
        /// Add value by automatic key
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>The key that allocates automaticly</returns>
        public int Add(TValue value)
        {
            Node leafNode;
            int index;
            int key = 0;
            int existsKey;

            if (Get(_MinFreeKey, out leafNode, out index, Operator.Query))
            {
                existsKey = _MinFreeKey;

                Node cur = leafNode;

                bool find = false;
                int lastKey = cur.FirstKey;

                while (cur != null)
                {
                    if (lastKey != cur.FirstKey)
                    {
                        //hole of leaf nodes
                        key = lastKey;
                        find = true;
                        break;
                    }

                    if (cur.Count < MaxChildren)
                    {
                        int i = 0;

                        foreach (DataEntity entity in cur.DataList)
                        {
                            if (entity.Used == 0 && cur.FirstKey + i > existsKey)
                            {
                                key = cur.FirstKey + i;
                                find = true;
                                break;
                            }

                            i++;
                        }

                        if (find)
                        {
                            break;
                        }
                    }

                    lastKey = cur.FirstKey + MaxChildren;

                    cur = cur.Next;
                }

                if (!find)
                {
                    key = this.Count;
                }
            }
            else
            {
                key = _MinFreeKey;
            }

            Add(key, value);

            _MinFreeKey = key + 1;

            return key;
        }

        #endregion

        #region IDictionary<int,TValue> Members

        public void Add(int key, TValue value)
        {
            Construct(key);
            Set(key, value, Operator.Add);
        }

        public bool ContainsKey(int key)
        {
            Node leafNode;
            int index;

            return Get(key, out leafNode, out index, Operator.Query); 
        }

        public bool ContainsValue(TValue value)
        {
            foreach (TValue v in GetValues())
            {
                if (typeof(TValue).IsClass)
                {
                    if (v == null && value == null)
                    {
                        return true;
                    }
                    else if (v == null || value == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (v.Equals(value))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (v.Equals(value))
                    {
                        return true;
                    }
                }
            }

            return false;
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
            Node leafNode;
            int index;

            if (!Get(key, out leafNode, out index, Operator.Remove))
            {
                return false;
            }
            else
            {
                if (leafNode != null)
                {
                    leafNode.DataList[index].Used = 0;

                    Node cur = leafNode;
                    
                    //Dec node count including parent nodes
                    while (cur != null)
                    {
                        cur.Count--;
                        cur = cur.Parent;
                    }

                    cur = leafNode;

                    int i = 0;

                    //Remove empty nodes
                    while (cur != null)
                    {
                        i = cur.ParentIndex;

                        if (cur.Count <= 0)
                        {
                            if (cur.IsLeafNode)
                            {
                                //If cur node is leaf node, remove prev and next

                                if (cur.Prev != null)
                                {
                                    cur.Prev.Next = cur.Next;
                                }

                                if (cur.Next != null)
                                {
                                    cur.Next.Prev = cur.Prev;
                                }
                            }

                            Node parent = cur.Parent;

                            if (parent == null)
                            {
                                //Root node empty
                                _Root = null;
                                _Height = 0;
                            }
                            else
                            {
                                parent.Children[i] = null;
                            }
                        }
                        else
                        {
                            break;
                        }

                        cur = cur.Parent;
                    }

                    if (_MinFreeKey > key)
                    {
                        _MinFreeKey = key;
                    }

                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public bool TryGetValue(int key, out TValue value)
        {
            Node leafNode;
            int index;

            if (!Get(key, out leafNode, out index, Operator.Get))
            {
                value = default(TValue);
                return false;
            }
            else
            {
                value = leafNode.DataList[index].Value;
                return true;
            }
        }

        public ICollection<TValue> Values
        {
            get 
            {
                return new ValueCollection(this);
            }
        }

        public TValue this[int key]
        {
            get
            {
                Node leafNode;
                int index;

                if (!Get(key, out leafNode, out index, Operator.Get))
                {
                    throw new System.Collections.Generic.KeyNotFoundException("The given key was not present in the dictionary.");
                }
                else
                {
                    return leafNode.DataList[index].Value;
                }
            }

            set
            {
                Construct(key);

                Set(key, value, Operator.Set);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<int,TValue>> Members

        public void Add(KeyValuePair<int, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _Root = null;
            _Height = 0;
        }

        public bool Contains(KeyValuePair<int, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<int, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array is null!");
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("Arg_NonZeroLowerBound");
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(string.Format("index={0} out of range", arrayIndex));
            }

            if (array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
            }

            int i = 0;
            foreach (KeyValuePair<int, TValue> kv in this.GetKeyValuePairs())
            {
                array[i] = kv;
                i++;
            }
        }

        public int Count
        {
            get 
            {
                if (_Root == null)
                {
                    return 0;
                }
                else
                {
                    return _Root.Count;
                }

            }
        }

        public bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<int, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<int,TValue>> Members

        public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
        {
            foreach (KeyValuePair<int, TValue> kv in this.GetKeyValuePairs())
            {
                yield return kv;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<int, TValue> kv in this.GetKeyValuePairs())
            {
                yield return kv;
            }
        }

        #endregion
    }
}
