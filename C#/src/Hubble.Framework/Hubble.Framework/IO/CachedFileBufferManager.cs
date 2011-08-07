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
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics; 

namespace Hubble.Framework.IO
{
    /// <summary>
    /// This class manages the buffer for 
    /// Cached file stream.
    /// </summary>
    public class CachedFileBufferManager
    {
        static internal CachedFileBufferManager CachedFileBufMgr = null;

        static public void Init(int memorySize, DelegateErrorMessage errorMessage)
        {
            CachedFileBufMgr = new CachedFileBufferManager(memorySize, errorMessage); 
        }

        /// <summary>
        /// Set new max memory size. (MB)
        /// </summary>
        /// <param name="memorySize">Memory size. In MB</param>
        /// <returns></returns>
        static public void SetMaxMemorySize(int memorySize)
        {
            //Min size  = 64MB
            if (memorySize <= 64)
            {
                memorySize = 64;
            }

            CachedFileBufMgr.MaxMemorySize = (long)memorySize * 1024 * 1024;
        }

        /// <summary>
        /// Get max memory size. (MB);
        /// </summary>
        /// <returns>in MB</returns>
        static public int GetMaxMemorySize()
        {
            return (int)(CachedFileBufMgr.MaxMemorySize / (1024 * 1024));
        }

        /// <summary>
        /// Get Avaliable physical memory since last alloc. (MB)
        /// </summary>
        /// <returns>Avaliable ram size. In MB</returns>
        static public float GetAvaliableRam()
        {
            return CachedFileBufMgr.AvaliableRam;
        }

        /// <summary>
        /// Get max memory size. (MB);
        /// </summary>
        /// <returns>in MB</returns>
        static public int GetAllocedMemorySize()
        {
            return (int)(CachedFileBufMgr.AllocedMemorySize / (1024 * 1024));
        }

        static public DelegateErrorMessage ErrorMessage = null;

        public const int BufferUnitSize = 8 * 1024 * 1024; //The max size of buffer can be alloc.

        public delegate void DelegateErrorMessage(string message, Exception e);

        internal class Buffer : IComparable<Buffer>
        {
            private byte[] _Data;

            private DateTime _LastAccessTime;

            private int _HeadPosition;

            private Buffer _NextBuffer = null;

            private CachedFileBufferManager _Mgr;


            private DateTime LastAccessTime
            {
                get
                {
                    lock (this)
                    {
                        return _LastAccessTime;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _LastAccessTime = value;
                    }
                }
            }

            /// <summary>
            /// If buffer cross multi-unit,
            /// this property is used to help release the buffer.
            /// </summary>
            internal Buffer Next
            {
                get
                {
                    lock (this)
                    {
                        return _NextBuffer;
                    }
                }

                set
                {
                    lock (this)
                    {
                        _NextBuffer = value;
                    }
                }
            }

            /// <summary>
            /// The position of the data for the first byte of this buffer.
            /// Because of the merge, sometimes the head position is not zero.
            /// </summary>
            internal int HeadPosition
            {
                get
                {
                    return _HeadPosition;
                }
            }

            internal byte[] Data
            {
                get
                {
                    lock (this)
                    {
                        _LastAccessTime = DateTime.Now;

                        if (Next != null && _HeadPosition == 0)
                        {
                            //Close crossed buffers
                            Buffer buffer = Next;

                            while (buffer != null)
                            {
                                buffer.LastAccessTime = DateTime.Now; 
                                buffer = buffer.Next;
                            }
                        }

                        return _Data;
                    }
                }
            }

            LinkedListNode<Buffer> _Node;

            internal Buffer()
            {
                _HeadPosition = 0;
                _LastAccessTime = DateTime.Now;
                _Node = null;
                _Data = null;
            }

            internal Buffer(CachedFileBufferManager mgr, LinkedList<Buffer> bufferList, int size)
            {
                _HeadPosition = 0;
                _LastAccessTime = DateTime.Now;
                _Node = bufferList.AddLast(this);
                _Data = new byte[size];
                _Mgr = mgr;
            }

            private void CrossClose()
            {
                lock (this)
                {
                    if (_HeadPosition > 0)
                    {
                        _Data = null;

                        if (_Node != null)
                        {
                            _Node.List.Remove(_Node);
                        }
                    }
                }
            }

            internal int Close()
            {
                lock (this)
                {
                    if (_HeadPosition > 0)
                    {
                        //Crossed buffer only can be closed by first buffer
                        return 0;
                    }

                    if (_Data == null)
                    {
                        return 0;
                    }

                    int size = _Data.Length;
                    _Data = null;

                    if (Next != null)
                    {
                        //Close crossed buffers
                        Buffer buffer = Next;

                        while (buffer != null)
                        {
                            buffer.CrossClose();
                            buffer = buffer.Next;
                        }
                    }


                    if (_Node != null)
                    {
                        _Node.List.Remove(_Node);
                        _Node = null;
                        _Mgr.ReduceMemory(size);
                        return size;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            internal Buffer Clone(int headPosition)
            {
                Buffer result = new Buffer();
                result._Data = this._Data;
                result._LastAccessTime = this._LastAccessTime;
                result._Node = null;
                result._HeadPosition = headPosition;

                return result;
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
        PerformanceCounter _RamCounter;
        float _AvaliableRam;

        System.Threading.Thread _LoadRamCounterThread;

        private float AvaliableRam
        {
            get
            {
                lock (this)
                {
                    return _AvaliableRam;
                }
            }
        }

        private long MaxMemorySize
        {
            get
            {
                lock (this)
                {
                    return _MaxMemorySize;
                }
            }

            set
            {
                lock (this)
                {
                    _MaxMemorySize = value;

                    if (_AllocedMemorySize >= _MaxMemorySize)
                    {
                        Cleanup();
                    }
                }
            }
        }

        private long AllocedMemorySize
        {
            get
            {
                lock (this)
                {
                    return _AllocedMemorySize;
                }
            }

            set
            {
                lock (this)
                {
                    _AllocedMemorySize = value;
                }
            }
        }


        public CachedFileBufferManager()
            : this(DefaultMaxMemorySize, null)
        {

        }


        public CachedFileBufferManager(long maxSize, DelegateErrorMessage errorMessage)
        {
            _MaxMemorySize = (long)maxSize * 1024 * 1024;
            _BufferList = new LinkedList<Buffer>();
            _AvaliableRam = 2 * 1024 * 1024;

            ErrorMessage = errorMessage;

            _LoadRamCounterThread = new System.Threading.Thread(delegate()
            {
                try
                {
                    PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                    lock (this)
                    {
                        _RamCounter = ramCounter;
                        _AvaliableRam = _RamCounter.NextValue();
                    }
                }
                catch (Exception e)
                {
                    if (ErrorMessage != null)
                    {
                        ErrorMessage("Init PerformanceCounter fail in CachedFileBufferManager", e);
                    }
                }
            });

            _LoadRamCounterThread.IsBackground = true;

            _LoadRamCounterThread.Start();
            
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
                buffer.Close();

                if (AllocedMemorySize <= MaxMemorySize / 2)
                {
                    break;
                }
            }

            GC.Collect(GC.MaxGeneration);
        }

        private void ReduceMemory(int size)
        {
            AllocedMemorySize -= size;
        }

        /// <summary>
        /// Alloc memory
        /// </summary>
        /// <param name="size">alloc size. in bytes</param>
        /// <returns>If it is not enough memory, return null</returns>
        internal Buffer Alloc(int size)
        {
            lock (this)
            {
                try
                {
                    if (_RamCounter == null)
                    {
                        _AvaliableRam = 2 * 1024 * 1024; ;
                    }
                    else
                    {
                        _AvaliableRam = _RamCounter.NextValue();
                    }
                }
                catch
                {
                    _AvaliableRam = 2 * 1024 * 1024; //2TB
                }

                if (_AvaliableRam < 100 && _AllocedMemorySize < 100 * 1024 * 1024)
                {
                    return null;
                }

                if (_AllocedMemorySize >= _MaxMemorySize || _AvaliableRam < 100)
                {
                    Cleanup();
                }

                _AllocedMemorySize += size;
                return new Buffer(this, _BufferList, size);
            }
        }
    }
}
