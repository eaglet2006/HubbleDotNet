using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.DataStructure
{
    /// <summary>
    /// Represents a node in a LinkedList<(Of <(T>)>). This class cannot be inherited.
    /// </summary>
    /// <typeparam name="T">Specifies the element type of the linked list.</typeparam>
    public sealed class LinkedNode<T>
    {
        internal LinkedNode<T> _Previous;

        /// <summary>
        /// Gets the previous node in the LinkedList<(Of <(T>)>)
        /// </summary>
        public LinkedNode<T> Previous
        {
            get
            {
                return _Previous;
            }
        }

        internal LinkedNode<T> _Next;

        /// <summary>
        /// Gets the next node in the LinkedList<(Of <(T>)>)
        /// </summary>
        public LinkedNode<T> Next
        {
            get
            {
                return _Next;
            }
        }

        internal T _Value;

        /// <summary>
        /// Gets the value contained in the node
        /// </summary>
        public T Value
        {
            get
            {
                return _Value;
            }
        }

        public LinkedNode()
        {
            _Previous = null;
            _Next = null;
            _Value = default(T);
        }


        public LinkedNode(LinkedNode<T> previous, LinkedNode<T> next, T value)
        {
            _Previous = previous;
            _Next = next;
            _Value = value;
        }

        public LinkedNode(T value)
        {
            _Previous = null;
            _Next = null;
            _Value = value;
        }
    }

    public class LinkedTable<T> : ICollection<T>, IEnumerable<T>
    {
        private LinkedNode<T> _Head;
        private LinkedNode<T> _Rear;
        private int _Count;

        LinkedTable()
        {
            _Head = null;
            _Rear = null;
            _Count = 0;
        }


        public void AddAfter(LinkedNode<T> node, LinkedNode<T> newNode)
        {
            if (node == null)
            {
                throw new ArgumentException("node is null");
            }

            newNode._Next = node.Next;
            node._Next = newNode;
            newNode._Previous = node;

            if (newNode.Next != null)
            {
                newNode.Next._Previous = node;
            }
            else
            {
                _Rear = newNode;
            }

            _Count++;
        }

        public void AddAfter(LinkedNode<T> node, T item)
        {
            AddAfter(node, new LinkedNode<T>(item));
        }

        public void AddBefore(LinkedNode<T> node, LinkedNode<T> newNode)
        {
            if (node == null)
            {
                throw new ArgumentException("node is null");
            }

            newNode._Previous = node.Previous;
            node._Previous = newNode;
            newNode._Next = node;

            if (newNode.Previous != null)
            {
                newNode.Previous._Next = node;
            }
            else
            {
                _Head = newNode;
            }

            _Count++;
        }

        public void AddBefore(LinkedNode<T> node, T item)
        {
            AddBefore(node, new LinkedNode<T>(item));
        }

        public void AddFirst(LinkedNode<T> newNode)
        {
            if (Count == 0)
            {
                _Head = newNode;
                _Rear = newNode;

                _Count++;
            }
            else
            {
                AddBefore(_Head, newNode);
            }
         }

        public void AddFirst(T item)
        {
            AddFirst(new LinkedNode<T>(item));
        }

        public void AddLast(LinkedNode<T> newNode)
        {
            if (Count == 0)
            {
                _Head = newNode;
                _Rear = newNode;

                _Count++;
            }
            else
            {
                AddAfter(_Rear, newNode);
            }
        }

        public void AddLast(T item)
        {
            AddLast(new LinkedNode<T>(item));
        }

        public LinkedListNode<T> Remove(LinkedListNode<T> item)
        {
            if (item == null)
            {
                return null;
            }


            return null;
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
            get { return _Count; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
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

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
