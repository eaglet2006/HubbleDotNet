using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Framework.IO
{
    class FileLastAccessManager
    {
        class FileLastAccess : IComparable<FileLastAccess>
        {
            internal string FilePath;
            internal DateTime LastAccessTime;

            internal FileLastAccess(string filePath, DateTime lastWriteTime)
            {
                this.FilePath = filePath;
                this.LastAccessTime = lastWriteTime;
            }

            public override bool Equals(object obj)
            {
                FileLastAccess dest = (FileLastAccess)obj;

                return this.FilePath == dest.FilePath;
            }

            public override int GetHashCode()
            {
                return this.FilePath.GetHashCode();
            }

            #region IComparable<FileLastAccess> Members

            public int CompareTo(FileLastAccess other)
            {
                return this.LastAccessTime.CompareTo(other.LastAccessTime);
            }

            #endregion
        }

        private object _LockObj = new object();
        private readonly int _Capacity;
        private readonly int _Timeout;

        private LinkedList<FileLastAccess> _FileLastWriteList = new LinkedList<FileLastAccess>();

        /// <summary>
        /// Remove the oldest file in the list.
        /// Or if one of the file timeout, remove it.
        /// Or if one of the file does not exist, remove it.
        /// </summary>
        private void RemoveOldestOne()
        {
            DateTime oldestTime = DateTime.Now;
            
            LinkedListNode<FileLastAccess> node = _FileLastWriteList.Last;
            LinkedListNode<FileLastAccess> oldestNode = node;

            while (node != null)
            {
                if (node.Value.LastAccessTime < oldestTime)
                {
                    oldestTime = node.Value.LastAccessTime;
                    oldestNode = node;
                }

                if (!IO.File.Exists(node.Value.FilePath))
                {
                    LinkedListNode<FileLastAccess> last = node;
                    node = node.Previous;
                    _FileLastWriteList.Remove(last);
                    return;
                }

                TimeSpan span = DateTime.Now - node.Value.LastAccessTime;

                if (span.TotalMilliseconds > _Timeout)
                {
                    LinkedListNode<FileLastAccess> last = node;
                    node = node.Previous;
                    _FileLastWriteList.Remove(last);
                    return;
                }

                node = node.Previous;
            }

            if (oldestNode != null)
            {
                _FileLastWriteList.Remove(oldestNode);
            }

        }

        private LinkedListNode<FileLastAccess> Get(string filePath)
        {
            filePath = filePath.ToLower();

            LinkedListNode<FileLastAccess> node = _FileLastWriteList.First;

            while (node != null)
            {
                TimeSpan span = DateTime.Now - node.Value.LastAccessTime;

                if (span.TotalMilliseconds > _Timeout)
                {
                    LinkedListNode<FileLastAccess> last = node;
                    node = node.Next;
                    _FileLastWriteList.Remove(last);
                    continue;
                }

                if (node.Value.FilePath == filePath)
                {
                    return node;
                }

                node = node.Next;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity">the capacity of files need to manager. If more then this capacity, it will remove the older files</param>
        /// <param name="timeout">timeout to remove the file from list. In ms</param>
        internal FileLastAccessManager(int capacity, int timeout)
        {
            _Capacity = capacity;
            _Timeout = timeout;
        }

        #region internal methods

        internal void Add(string filePath)
        {
            lock (_LockObj)
            {
                LinkedListNode<FileLastAccess> node = Get(filePath);
                if (node != null)
                {
                    node.Value.LastAccessTime = DateTime.Now;
                }
                else
                {
                    if (_FileLastWriteList.Count > _Capacity)
                    {
                        RemoveOldestOne();
                    }

                    _FileLastWriteList.AddLast(new FileLastAccess(filePath.ToLower(), DateTime.Now));
                }
            }
        }

        /// <summary>
        /// Is the filepath write access recently.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal bool AccessRecently(string filePath)
        {
            lock (_LockObj)
            {
                LinkedListNode<FileLastAccess> node = Get(filePath);

                if (node == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        #endregion
    }
}
