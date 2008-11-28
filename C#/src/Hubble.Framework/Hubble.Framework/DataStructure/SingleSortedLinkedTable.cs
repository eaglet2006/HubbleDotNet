using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.DataStructure
{
    /// <summary>
    /// Sorted single linked table
    /// </summary>
    public class SingleSortedLinkedTable<T, TValue> 
    {
        public class Node
        {
            public T Key;
            public TValue Value;
            public Node Next;

            public Node(T key, TValue value)
            {
                Key = key;
                Value = value;
                Next = null;
            }
        }

        private Node _Head = null;
        private System.Collections.Generic.IComparer<T> _Comparer;

        public System.Collections.Generic.IComparer<T> Comparer
        {
            get
            {
                return _Comparer;
            }

            set
            {
                _Comparer = value;
            }

        }

        public bool IsEmpty
        {
            get
            {
                return _Head == null;
            }
        }

        public Node First
        {
            get
            {
                return _Head;
            }
        }

        public SingleSortedLinkedTable(System.Collections.Generic.IComparer<T> comparer)
        {
            System.Diagnostics.Debug.Assert(comparer != null);
            _Comparer = comparer;
        }

        #region Public methods

        public void Clear()
        {
            _Head = null;
        }

        public void Add(T key, TValue value)
        {
            if (_Head == null)
            {
                _Head = new Node(key, value);
            }
            else
            {
                Node cur = _Head;
                Node last = null;

                while (cur != null)
                {
                    if (Comparer.Compare(key, cur.Key) <= 0)
                    {
                        //Insert
                        Node node = new Node(key, value);

                        if (last == null)
                        {
                            _Head = node;
                        }
                        else
                        {
                            last.Next = node;
                        }

                        node.Next = cur;
                        return;
                    }
                    else if (cur.Next == null)
                    {
                        //Last node
                        cur.Next = new Node(key, value);
                        return;
                    }

                    last = cur;
                    cur = cur.Next;
                }
            }
        }

        public IEnumerable<Node> GetAll()
        {
            Node cur = _Head;

            while (cur != null)
            {
                yield return cur;
                cur = cur.Next;
            }
        }

        /// <summary>
        /// Get min value keys
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node> GetFirstKeys()
        {
            Node cur = _Head;
            bool fst = true;
            T lastKey = default(T);

            while (cur != null)
            {
                if (fst)
                {
                    fst = false;
                    lastKey = cur.Key;
                    yield return cur;
                }
                else
                {
                    if (Comparer.Compare(lastKey, cur.Key) == 0)
                    {
                        lastKey = cur.Key;
                        yield return cur;
                    }
                    else
                    {
                        yield break;
                    }
                }

                cur = cur.Next;
            }
        }

        public void RemoveFirstKeys()
        {
            Node cur = _Head;
            bool fst = true;
            T lastKey = default(T);

            while (cur != null)
            {
                if (fst)
                {
                    fst = false;
                    lastKey = cur.Key;
                }
                else
                {
                    if (Comparer.Compare(lastKey, cur.Key) == 0)
                    {
                        lastKey = cur.Key;
                    }
                    else
                    {
                        _Head = cur;
                        return;
                    }
                }

                cur = cur.Next;
            }

            _Head = null;
        }

        #endregion
    }
}
