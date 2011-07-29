using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Hubble.Framework.IO
{
    /// <summary>
    /// This class manages the buffer for 
    /// Cached file stream.
    /// </summary>
    public class CachedFileBufferManager
    {
        static internal CachedFileBufferManager CachedFileBufMgr = new CachedFileBufferManager();

        public const int BufferUnitSize = 8 * 1024 * 1024; //The max size of buffer can be alloc.

        internal enum BufferStatus
        {
            Ready  = 0,
            Loading= 1,
            Running= 2,
            Closed = 3,
        }

        internal class Buffer : IComparable<Buffer>
        {
            private byte[] _Data;

            private DateTime _LastAccessTime;

            private BufferStatus _Status;

            internal BufferStatus Status
            {
                get
                {
                    lock (this)
                    {
                        return _Status;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _Status = value;
                    }
                }
            }

            internal byte[] Data
            {
                get
                {
                    lock (this)
                    {
                        _LastAccessTime = DateTime.Now;
                        return _Data;
                    }
                }
            }

            LinkedListNode<Buffer> _Node;

            internal Buffer(LinkedList<Buffer> bufferList, int size)
            {
                _LastAccessTime = default(DateTime);
                _Node = bufferList.AddLast(this);
                _Data = new byte[size];
                _Status = BufferStatus.Ready;
            }

            internal int Close()
            {
                lock (this)
                {
                    int size = _Data.Length;
                    _Data = null;
                    _Node.List.Remove(_Node);
                    return size;
                }
            }

            #region IComparable<Buffer> Members

            public int CompareTo(Buffer other)
            {
                return this._LastAccessTime.CompareTo(other._LastAccessTime);
            }

            #endregion
        }

        const long DefaultMaxMemorySize = (long)2 * 1024 * 1024 * 1024 * 1024; //In bytes, default is 2TB
        long _MaxMemorySize;
        long _AllocedMemorySize;

        LinkedList<Buffer> _BufferList;

        public CachedFileBufferManager()
            : this(DefaultMaxMemorySize)
        {

        }


        public CachedFileBufferManager(long maxSize)
        {
            _MaxMemorySize = maxSize;
            _BufferList = new LinkedList<Buffer>();
            _AllocedMemorySize = 0;
        }

        /// <summary>
        /// Cleanup memory when memory overflow.
        /// Increased to 50%
        /// </summary>
        private void Cleanup()
        {
            Buffer[] buffers = _BufferList.ToArray<Buffer>();

            Array.Sort(buffers);

            foreach (Buffer buffer in buffers)
            {
                _AllocedMemorySize -= buffer.Close();

                if (_AllocedMemorySize < BufferUnitSize / 2)
                {
                    break;
                }
            }

            GC.Collect();
            GC.Collect();
            GC.Collect();
        }

        internal Buffer Alloc(int size)
        {
            if (size > BufferUnitSize)
            {
                throw new ArgumentException(string.Format("Size must be less than or equal {0}", BufferUnitSize));
            }

            lock (this)
            {
                _AllocedMemorySize += size;

                if (_AllocedMemorySize >= _MaxMemorySize)
                {
                    Cleanup();
                }

                return new Buffer(_BufferList, size);
            }
        }
    }
}
