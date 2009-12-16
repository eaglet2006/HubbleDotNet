using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.DataStructure
{
    public class SortDictionary<TValue> 
    {
        struct Node : IComparable<Node> 
        {
            public long Id;
            public TValue Value;

            public Node (long id, TValue value)
            {
                this.Id = id;
                this.Value = value;
            }

            public override bool Equals(object obj)
            {
                return ((Node)obj).Id == this.Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            #region IComparable<Node> Members

            public int CompareTo(Node other)
            {
                if (this.Id > other.Id)
                {
                    return 1;
                }
                else if (this.Id < other.Id)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            #endregion
        }

        List<Node> _Nodes;

        public SortDictionary()
        {
            _Nodes = new List<SortDictionary<TValue>.Node>();
        }


        public SortDictionary(int capacity)
        {
            _Nodes = new List<Node>(capacity);
        }

        public void Sort()
        {
            _Nodes.Sort();
        }

        #region IDictionary<long,TValue> Members

        public void Add(long key, TValue value)
        {
            _Nodes.Add(new Node(key, value));
        }

        public bool ContainsKey(long key)
        {
            int index = _Nodes.BinarySearch(new Node(key, default(TValue)));

            if (index < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public IEnumerable<long> Keys
        {
            get 
            {
                foreach (Node node in _Nodes)
                {
                    if (node.Value == null)
                    {
                        continue;
                    }

                    yield return node.Id;
                }
            }
        }

        public bool Remove(long key)
        {
            int index = _Nodes.BinarySearch(new Node(key, default(TValue)));

            if (index < 0)
            {
                return false;
            }
            else
            {
                Node node = _Nodes[index];
                node.Value = default(TValue);
                return true;
            }
        }

        public bool TryGetValue(long key, out TValue value)
        {
            int index = _Nodes.BinarySearch(new Node(key, default(TValue)));

            if (index < 0)
            {
                value = default(TValue);
                return false;
            }
            else
            {
                value = _Nodes[index].Value;
                return true;
            }
        }

        public IEnumerable<TValue> Values
        {
            get 
            {
                foreach (Node node in _Nodes)
                {
                    if (node.Value == null)
                    {
                        continue;
                    }

                    yield return node.Value;
                }

                //Node cur = _Head;
                //while (cur != null)
                //{
                //    yield return cur.Value;
                //    cur = cur.Next;
                //}
            }
        }

        public TValue this[long key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                {
                    throw new Exception("Key does not exist!");
                }
                else
                {
                    return value;
                }
            }

            set
            {
                if (ContainsKey(key))
                {
                    throw new Exception("Key does not exist!");
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<long,TValue>> Members

        public void Add(KeyValuePair<long, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _Nodes.Clear();
        }

        public int Count
        {
            get 
            {
                return _Nodes.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion


    }
}
