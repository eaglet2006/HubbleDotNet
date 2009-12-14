using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.DataStructure
{
    public class SortDictionary<TValue>
    {
        class Node 
        {
            public long Id;
            public TValue Value;
            public Node Next;

            public Node (long id, TValue value)
            {
                this.Id = id;
                this.Value = value;
                this.Next = null;
            }
        }

        Node _Head;
        Node _Cur;
        int _Count;

        public SortDictionary()
        {
            _Head = null;
            _Cur = null;
            _Count = 0;
        }


        public SortDictionary(int capacity)
        {
            _Head = null;
            _Cur = null;
            _Count = 0;
        }

        public void Reset()
        {
            _Cur = _Head;
        }

        #region IDictionary<long,TValue> Members

        public void Add(long key, TValue value)
        {
            if (_Head == null)
            {
                _Head = new Node(key, value);
                _Cur = _Head;
                _Count++;
            }
            else
            {
                if (_Cur == null)
                {
                    _Cur = _Head;
                }

                if (_Cur.Id > key)
                {
                    if (_Cur == _Head)
                    {
                        Node node = new SortDictionary<TValue>.Node(key, value);
                        node.Next = _Head;
                        _Head = node;
                        _Count++;
                        _Cur = node;
                        return;
                    }

                    _Cur = _Head;
                }

                if (_Cur.Next == null)
                {
                    Node node = new Node(key, value);
                    _Cur.Next = node;
                    _Count++;
                    _Cur = node;
                    return;
                }

                Node last = _Cur;

                while (_Cur != null)
                {
                    if (_Cur.Id == key)
                    {
                        throw new Exception("Key exist!");
                    }

                    if (_Cur.Id > key)
                    {
                        Node node = new SortDictionary<TValue>.Node(key, value);

                        if (last == _Cur)
                        {
                            last.Next = node;
                            node.Next = _Cur.Next;
                        }
                        else
                        {
                            last.Next = node;
                            node.Next = _Cur;
                        }

                        _Cur = node;
                        _Count++;
                        return;
                    }
                    else
                    {
                        last = _Cur;
                        _Cur = _Cur.Next;
                    }
                }

                if (_Cur == null)
                {
                    Node node = new SortDictionary<TValue>.Node(key, value);
                    last.Next = node;
                    _Count++;
                    _Cur = node;
                }
            }


            //if (_SortedList.Count == 0)
            //{
            //    _Current = _SortedList.AddLast(new Node(key, value));
            //}
            //else
            //{
            //    if (_Current == null)
            //    {
            //        _Current = _SortedList.First;
            //    }

            //    if (key > _Current.Value.Id)
            //    {
            //        while (_Current.Value.Id < key)
            //        {
            //            _Current = _Current.Next;

            //            if (_Current == null)
            //            {
            //                break;
            //            }
            //        }

            //        if (_Current == null)
            //        {
            //            _Current = _SortedList.AddLast(new Node(key, value));
            //        }
            //        else
            //        {
            //            if (_Current.Value.Id == key)
            //            {
            //                throw new Exception("Key exist!");
            //            }

            //            _Current = _SortedList.AddBefore(_Current, new Node(key, value));
            //        }
            //    }
            //    else
            //    {
            //        while (_Current.Value.Id > key)
            //        {
            //            _Current = _Current.Previous;

            //            if (_Current == null)
            //            {
            //                break;
            //            }
            //        }

            //        if (_Current == null)
            //        {
            //            _Current = _SortedList.AddFirst(new Node(key, value));
            //        }
            //        else
            //        {
            //            if (_Current.Value.Id == key)
            //            {
            //                throw new Exception("Key exist!");
            //            }

            //            _Current = _SortedList.AddAfter(_Current, new Node(key, value));
            //        }
            //    }

            //}
        }

        public bool ContainsKey(long key)
        {
            if (_Head == null)
            {
                return false;
            }

            if (_Cur == null)
            {
                _Cur = _Head;
            }

            if (_Cur.Id > key)
            {
                if (_Cur == _Head)
                {
                    return false;
                }

                _Cur = _Head;
            }

            Node last = _Cur;

            while (_Cur != null)
            {
                if (_Cur.Id == key)
                {
                    return true;
                }

                if (_Cur.Id > key)
                {
                    return false;
                }
                else
                {
                    last = _Cur;
                    _Cur = _Cur.Next;
                }
            }

            _Cur = last;

            return false;
        }

        public IEnumerable<long> Keys
        {
            get 
            {
                Node cur = _Head;
                while (cur != null)
                {
                    yield return cur.Id;
                    cur = cur.Next;
                }
            }
        }

        public bool Remove(long key)
        {
            if (_Head == null)
            {
                return false;
            }

            if (_Cur == null)
            {
                _Cur = _Head;
            }

            if (_Cur.Id > key)
            {
                if (_Cur == _Head)
                {
                    return false;
                }

                _Cur = _Head;
            }

            Node last = _Cur;

            while (_Cur != null)
            {
                if (_Cur.Id == key)
                {
                    if (last == _Head)
                    {
                        _Cur = _Cur.Next;
                    }
                    else
                    {
                        last.Next = _Cur.Next;
                        _Cur = last;
                    }

                    _Count--;
                    return true;
                }

                if (_Cur.Id > key)
                {
                    return false;
                }
                else
                {
                    last = _Cur;
                    _Cur = _Cur.Next;
                }
            }

            _Cur = last;

            return false;
        }

        public bool TryGetValue(long key, out TValue value)
        {
            value = default(TValue);

            if (_Head == null)
            {
                return false;
            }

            if (_Cur == null)
            {
                _Cur = _Head;
            }

            if (_Cur.Id > key)
            {
                if (_Cur == _Head)
                {
                    return false;
                }

                _Cur = _Head;
            }

            Node last = _Cur;

            while (_Cur != null)
            {
                if (_Cur.Id == key)
                {
                    value = _Cur.Value;
                    return true;
                }

                if (_Cur.Id > key)
                {
                    return false;
                }
                else
                {
                    last = _Cur;
                    _Cur = _Cur.Next;
                }
            }

            _Cur = last;
            return false;
        }

        public IEnumerable<TValue> Values
        {
            get 
            {
                Node cur = _Head;
                while (cur != null)
                {
                    yield return cur.Value;
                    cur = cur.Next;
                }
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
            _Head = null;
            _Cur = null;
            _Count = 0;
        }

        public int Count
        {
            get { return _Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion


    }
}
