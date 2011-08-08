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
using System.IO;
using System.Threading;

namespace Hubble.Framework.IO
{
    /// <summary>
    /// This class providers a cachable fie stream.
    /// This class has to worked with 
    /// CachedFileBufferManager
    /// </summary>
    public class CachedFileStream : FileStream
    {
        public enum CachedType
        {
            NoCache = 0,
            Full = 1, //Cache all the data
            Dynamic = 2, //Cache dynamicly
            Small = 3, //Only cache small file
        }

        private CachedType _Type;

        private string _FilePath;

        private bool _Closed = false;

        private int _MinCacheLength = 1 * 1024 * 1024; //only used for type = Dynamic.

        private CachedFileBufferManager _CachedFileMgr;
        private CachedFileBufferManager.Buffer[] _CacheBufferIndex;

        private long _FileLength;
        private long _CurPosition;

        private object _CacheLock = new object();
        private Thread _CacheThread = null;
        private List<int> _CacheIndexes = new List<int>();

        private bool _CacheAllFinished = true;

        private bool _Closing = false;

        private bool CacheAllFinished
        {
            get
            {
                lock (this)
                {
                    return _CacheAllFinished;
                }
            }

            set
            {
                lock (this)
                {
                    _CacheAllFinished = value;
                }
            }
        }


        #region Properties

        public int MinCacheLength
        {
            get
            {
                lock (this)
                {
                    return _MinCacheLength;
                }
            }

            set
            {
                lock (this)
                {
                    _MinCacheLength = value;
                }
            }
        }

        #endregion

        #region Contributor
        public CachedFileStream(string path,
              FileMode mode, FileAccess access, FileShare share)
            : this(CachedType.NoCache, 1 * 1024 * 1024, path, mode, access, share)
        {

        }

        public CachedFileStream(string path,
              FileMode mode, FileAccess access)
            : this(CachedType.NoCache, 1 * 1024 * 1024, path, mode, access, FileShare.None)
        {

        }

        public CachedFileStream(CachedType type, string path,
            FileMode mode, FileAccess access)
            : this(type, 1 * 1024 * 1024, path, mode, access, FileShare.None)
        {

        }


        public CachedFileStream(CachedType type, string path,
            FileMode mode, FileAccess access, FileShare share)
            : this(type, 1 * 1024 * 1024, path, mode, access, share)
        {

        }

        public CachedFileStream(CachedType type, int minCacheLength, string path,
                 FileMode mode, FileAccess access, FileShare share)
            : base(path, mode, access, share)
        {
            _Type = type;
            _MinCacheLength = minCacheLength;
            _FilePath = path;
            _FileLength = this.Length;
            _CurPosition = 0;

            if (_MinCacheLength > CachedFileBufferManager.BufferUnitSize)
            {
                _MinCacheLength = CachedFileBufferManager.BufferUnitSize;
            }

            if (mode != FileMode.Open || share != FileShare.Read)
            {
                _Type = CachedType.NoCache;
            }

            if (_Type != CachedType.NoCache)
            {
                int indexLength = InitCacheBufferIndex();

                if (_Type == CachedType.Full ||
                    (_Type == CachedType.Small && indexLength <= 1))
                {
                    CacheAll();
                }
            }
            else
            {
                _CacheBufferIndex = null;
            }
        }

        #endregion

        #region Private methods
        private int InitCacheBufferIndex()
        {
            int indexLength = (int)(this.Length / CachedFileBufferManager.BufferUnitSize);

            if ((this.Length % CachedFileBufferManager.BufferUnitSize) > 0)
            {
                indexLength++;
            }

            _CacheBufferIndex = new CachedFileBufferManager.Buffer[indexLength];
            _CachedFileMgr = CachedFileBufferManager.CachedFileBufMgr;

            _CurPosition = 0;

            return indexLength;
        }

        private void Cleanup()
        {
            bool waittingForCacheThreadFinish = false;

            lock (_CacheLock)
            {
                if (_CacheThread != null)
                {
                    waittingForCacheThreadFinish = true;
                    _Closing = true;
                }
            }

            if (waittingForCacheThreadFinish)
            {
                try
                {
                    if (!_CacheThread.Join(5000))
                    {
                        _CacheThread.Abort();
                    }
                }
                catch
                {
                }
            }

            if (_CacheBufferIndex != null)
            {
                long realseMemSize = 0;

                for (int index = 0; index < _CacheBufferIndex.Length; index++)
                {
                    CachedFileBufferManager.Buffer buffer = _CacheBufferIndex[index];

                    if (buffer != null)
                    {
                        realseMemSize += buffer.Close();
                    }
                }

                if (realseMemSize >= 64 * 1024 * 1024)
                {
                    //Large than 64M
                    //Force collect GC
                    GC.Collect(GC.MaxGeneration);
                }
            }
        }

        private bool InnerCacheUnit(int index)
        {
            CachedFileBufferManager.Buffer buffer;

            if (index < _CacheBufferIndex.Length - 1)
            {
                buffer = _CachedFileMgr.Alloc(
                    CachedFileBufferManager.BufferUnitSize);
            }
            else
            {
                buffer = _CachedFileMgr.Alloc((int)(_FileLength % CachedFileBufferManager.BufferUnitSize));
            }

            if (buffer == null)
            {
                return false;
            }

            byte[] data = buffer.Data;

            if (data != null)
            {
                using (System.IO.FileStream fs = new FileStream(_FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.Position = index * CachedFileBufferManager.BufferUnitSize;
                    int offset = 0;
                    int read = 0;

                    while ((read = fs.Read(data, offset, data.Length - offset)) > 0)
                    {
                        offset += read;
                        if (offset >= data.Length)
                        {
                            break;
                        }
                    }

                    if (offset != data.Length)
                    {
                        throw new System.IO.IOException();
                    }
                }
 
                lock (this)
                {
                    _CacheBufferIndex[index] = buffer;
                }

                return true;
            }
            else
            {
                return false;
            }

        }

        private void CacheProc()
        {
            try
            {
                while (true)
                {
                    int[] indexes = null;
                    lock (_CacheLock)
                    {
                        if (_CacheIndexes.Count == 0)
                        {
                            _CacheThread = null;
                            break;
                        }
                        else
                        {
                            indexes = _CacheIndexes.ToArray();
                            _CacheIndexes.Clear();
                        }
                    }

                    foreach (int index in indexes)
                    {
                        lock (_CacheLock)
                        {
                            if (_Closing)
                            {
                                break;
                            }
                        }

                        if (!InnerCacheUnit(index))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    if (CachedFileBufferManager.ErrorMessage != null)
                    {
                        CachedFileBufferManager.ErrorMessage(string.Format("Cache file fail. FilePath:{0}", _FilePath), e);
                    }
                }
                catch
                {
                }
            }
            finally
            {
                lock (_CacheLock)
                {
                    _CacheThread = null;
                }
            }

            CacheAllFinished = true;
        }

        private void CacheUnit(int index)
        {
            lock (_CacheLock)
            {
                _CacheIndexes.Add(index);

                if (_CacheThread == null)
                {
                    _CacheThread = new Thread(CacheProc);
                    _CacheThread.IsBackground = true;
                    _CacheThread.Start(); 
                }
            }
        }

        private byte[] GetUnitBuffer(int index, out int headPosition)
        {
            lock (this)
            {
                if (_CacheBufferIndex == null)
                {
                    headPosition = 0;
                    return null;
                }

                if (_CacheBufferIndex[index] == null)
                {
                    headPosition = 0;
                    return null;
                }

                headPosition = _CacheBufferIndex[index].HeadPosition;

                return _CacheBufferIndex[index].Data;
            }
        }
   
        private void CacheAll()
        {
            _Closing = false;
            CacheAllFinished = false;
            base.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < _CacheBufferIndex.Length; i++)
            {
                CacheUnit(i);
            }
        }

        private bool LoadToBuffer(int count, long position)
        {
            int index = (int)(position / CachedFileBufferManager.BufferUnitSize);
            int offsetInBuffer = (int)(position % CachedFileBufferManager.BufferUnitSize);

            if (count > CachedFileBufferManager.BufferUnitSize - offsetInBuffer)
            {
                int dataremain = (count - (CachedFileBufferManager.BufferUnitSize - offsetInBuffer));

                int loadIndexes = 1 + dataremain / CachedFileBufferManager.BufferUnitSize;

                if ((dataremain % CachedFileBufferManager.BufferUnitSize) > 0)
                {
                    loadIndexes++;
                }

                int loadLength = loadIndexes * CachedFileBufferManager.BufferUnitSize;

                if (index + loadIndexes >= _CacheBufferIndex.Length)
                {
                    //To last buffer

                    loadLength = (int)(_FileLength - index * CachedFileBufferManager.BufferUnitSize);
                }

                CachedFileBufferManager.Buffer buffer = _CachedFileMgr.Alloc(loadLength);

                if (buffer == null)
                {
                    return false;
                }

                byte[] data = buffer.Data;

                if (data != null)
                {
                    base.Seek(index * CachedFileBufferManager.BufferUnitSize, SeekOrigin.Begin);

                    int offset = 0;
                    int read = 0;

                    while ((read = base.Read(data, offset, data.Length - offset)) > 0)
                    {
                        offset += read;
                        if (offset >= data.Length)
                        {
                            break;
                        }
                    }

                    if (offset != data.Length)
                    {
                        throw new System.IO.IOException();
                    }
                }
                else
                {
                    return false;
                }

                lock (this)
                {
                    CachedFileBufferManager.Buffer preBuffer = null;

                    for (int i = index; i < index + loadIndexes; i++)
                    {
                        if (_CacheBufferIndex[i] != null)
                        {
                            _CacheBufferIndex[i].Close();
                        }

                        if (i == index)
                        {
                            _CacheBufferIndex[i] = buffer;
                            preBuffer = _CacheBufferIndex[i];
                        }
                        else
                        {
                            _CacheBufferIndex[i] = buffer.Clone((i - index) * CachedFileBufferManager.BufferUnitSize);
                            preBuffer.Next = _CacheBufferIndex[i];
                            preBuffer = _CacheBufferIndex[i];
                        }
                    }
                }

                return true;
            }
            else
            {
                return InnerCacheUnit(index);
            }
        }


        #endregion

        #region public properties

        public override long Position
        {
            get
            {
                if (_CacheBufferIndex != null)
                {
                    return _CurPosition;
                }
                else
                {
                    return base.Position;
                }
            }

            set
            {
                if (_CacheBufferIndex != null)
                {
                    _CurPosition = value;
                }
                else
                {
                    base.Position = value;
                }
            }
        }

        #endregion

        #region public methods

        public void ChangeCachedType(CachedType type)
        {
            switch (_Type)
            {
                case CachedType.NoCache:
                    switch (type)
                    {
                        case CachedType.Dynamic:
                        case CachedType.Full:
                        case CachedType.Small:

                            InitCacheBufferIndex();

                            int indexLength = InitCacheBufferIndex();

                            if (type == CachedType.Full ||
                                (type == CachedType.Small && indexLength <= 1))
                            {
                                CacheAll();
                            }

                            break;
                    }
                    break;
                case CachedType.Dynamic:
                    if (type == CachedType.NoCache)
                    {
                        Cleanup();
                    }
                    //else if (type == CachedType.Full ||
                    //    (type == CachedType.Small && _FileLength < )
                    //{
                    //}

                    break;
                case CachedType.Full:
                case CachedType.Small:

                    if (type == CachedType.NoCache)
                    {
                        Cleanup();
                        
                    }
                    break;

            }

            _Type = type;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_CacheBufferIndex != null)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        _CurPosition = offset;
                        break;
                    case SeekOrigin.Current:
                        _CurPosition += offset;
                        break;
                    case SeekOrigin.End:
                        _CurPosition = _FileLength - offset;
                        break;
                }

                return _CurPosition;
            }
            else
            {
                return base.Seek(offset, origin);
            }
        }


        public override int ReadByte()
        {
            if (_CacheBufferIndex != null)
            {
                int index = (int)(this.Position / CachedFileBufferManager.BufferUnitSize);
                int offsetInBuffer = (int)(this.Position % CachedFileBufferManager.BufferUnitSize);

                int headPosition;

                byte[] data = GetUnitBuffer(index, out headPosition);

                if (data != null)
                {
                    Position++;
                    return data[headPosition + offsetInBuffer];
                }
                else
                {
                    if (Position != base.Position)
                    {
                        base.Seek(Position, SeekOrigin.Begin);
                    }

                    Position++;
                    return base.ReadByte();
                }
            }
            else
            {
                return base.ReadByte();
            }
        }

        /// <summary>
        /// Read and output to array 
        /// </summary>
        /// <param name="array">array</param>
        /// <param name="offsetInArray">offset in array</param>
        /// <param name="offset">offset in file</param>
        /// <param name="count">count want to read</param>
        /// <returns>actual read count</returns>
        public int Read(out byte[] array, out int offsetInArray, int count)
        {
            if (_CacheBufferIndex != null)
            {
                int index = (int)(this.Position / CachedFileBufferManager.BufferUnitSize);
                int offsetInBuffer = (int)(this.Position % CachedFileBufferManager.BufferUnitSize);

                if (_CacheBufferIndex[index] != null)
                {
                    CachedFileBufferManager.Buffer buffer = _CacheBufferIndex[index];
                    byte[] data = _CacheBufferIndex[index].Data;

                    if (data != null)
                    {
                        if (data.Length - offsetInBuffer - buffer.HeadPosition >= count)
                        {
                            array = data;
                            offsetInArray = offsetInBuffer + buffer.HeadPosition;
                            Position += count;
                            return count;
                        }
                        else
                        {
                            if (buffer.HeadPosition > 0 || index == _CacheBufferIndex.Length - 1 || count < 10 * 1024)
                            {
                                //Data outside one block. but the index is not the beginning of the block
                                //Or it is the last block
                                //Or small count less than 10K
                                //Return part of the data

                                array = new byte[count];

                                Stream.ReadToBuf(this, array, 0, ref count);
                                offsetInArray = 0;

                                return count;
                            }
                        }
                    }
                }

                if ((_Type == CachedType.Dynamic || _Type == CachedType.Full)
                    && CacheAllFinished)
                {
                    if (count >= MinCacheLength)
                    {
                        //Cached
                        if (LoadToBuffer(count, Position))
                        {
                            return Read(out array, out offsetInArray, count);
                        }
                    }
                }

                //return data from file
                array = new byte[count];

                Stream.ReadToBuf(this, array, 0, ref count);
                offsetInArray = 0;
                return count;
            }

            array = new byte[count];
            offsetInArray = 0;
            Stream.ReadToBuf(this, array, 0, ref count);

            return count;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            if (_CacheBufferIndex != null)
            {
                int index = (int)(this.Position / CachedFileBufferManager.BufferUnitSize);
                int offsetInBuffer = (int)(this.Position % CachedFileBufferManager.BufferUnitSize);

                if (_CacheBufferIndex[index] != null)
                {
                    CachedFileBufferManager.Buffer buffer = _CacheBufferIndex[index];
                    byte[] data = _CacheBufferIndex[index].Data;

                    if (data != null)
                    {
                        if (data.Length - offsetInBuffer - buffer.HeadPosition >= count)
                        {
                            //Data inside one block.
                            Array.Copy(data, offsetInBuffer + buffer.HeadPosition, array, offset, count);

                            Position += count;
                            return count;
                        }
                        else
                        {
                            if (buffer.HeadPosition > 0 || index == _CacheBufferIndex.Length - 1 || count < 10 * 1024)
                            {
                                //Data outside one block. but the index is not the beginning of the block
                                //Or it is the last block
                                //Or small count less than 10K
                                //Return part of the data

                                int len = data.Length - offsetInBuffer - buffer.HeadPosition;

                                Array.Copy(data, offsetInBuffer + buffer.HeadPosition, array, offset, len);

                                Position += len;
                                return len;
                            }
                        }
                    }
                }

                if ((_Type == CachedType.Dynamic || _Type == CachedType.Full)
                    && CacheAllFinished)
                {
                    if (count >= MinCacheLength)
                    {
                        //Cached
                        if (LoadToBuffer(count, Position))
                        {
                            return Read(array, offset, count);
                        }
                    }
                }

                //return data from file
                base.Seek(Position, SeekOrigin.Begin);
                int retLen = base.Read(array, offset, count);
                Position += retLen;
                return retLen;
            }


            return base.Read(array, offset, count);
        }

        public override void Close()
        {
            try
            {
                _Closed = true;

                Cleanup();

            }
            catch(Exception e)
            {
                try
                {
                    if (CachedFileBufferManager.ErrorMessage != null)
                    {
                        CachedFileBufferManager.ErrorMessage(string.Format("Closing CachedFileStream fail. FilePath:{0}", _FilePath), e);
                    }
                }
                catch
                {
                }

            }

            base.Close();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            try
            {
                if (!_Closed)
                {
                    Close();
                }
            }
            catch
            {
            }
        }
    }

}
