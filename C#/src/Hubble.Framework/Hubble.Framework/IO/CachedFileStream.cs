using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

        private bool _Closed = false;

        private int _MinCacheLength = 1 * 1024 * 1024; //only used for type = Dynamic.

        private CachedFileBufferManager _CachedFileMgr;
        private CachedFileBufferManager.Buffer[] _CacheBufferIndex;

        private long _FileLength;
        private long _CurPosition;

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

            if (_MinCacheLength > CachedFileBufferManager.BufferUnitSize)
            {
                _MinCacheLength = CachedFileBufferManager.BufferUnitSize;
            }

            if (mode != FileMode.Open)
            {
                _Type = CachedType.NoCache;
            }

            if (_Type != CachedType.NoCache)
            {
                int indexLength = (int)(this.Length / CachedFileBufferManager.BufferUnitSize) + 1;
                _CacheBufferIndex = new CachedFileBufferManager.Buffer[indexLength];
                _CachedFileMgr = CachedFileBufferManager.CachedFileBufMgr;

                _FileLength = this.Length;
                _CurPosition = 0;

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

        //~CachedFileStream()
        //{
        //    try
        //    {
        //        if (!this._Closed)
        //        {
        //            Dispose(false);
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        private void CacheAll()
        {
            base.Seek(0, SeekOrigin.Begin);

            for(int i = 0; i < _CacheBufferIndex.Length; i++)
            {
                CachedFileBufferManager.Buffer buffer;

                if (i < _CacheBufferIndex.Length - 1)
                {
                    buffer = _CachedFileMgr.Alloc(
                        CachedFileBufferManager.BufferUnitSize);
                }
                else
                {
                    buffer = _CachedFileMgr.Alloc((int)(this.Length % CachedFileBufferManager.BufferUnitSize));
                }

                byte[] data = buffer.Data;

                if (data != null)
                {
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

                    _CacheBufferIndex[i] = buffer;
                }
            }
        }

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

        private void LoadToBuffer(int count, long position)
        {
            base.Position = position;

            int index = (int)(base.Position / CachedFileBufferManager.BufferUnitSize);
            int offsetInBuffer = (int)(base.Position % CachedFileBufferManager.BufferUnitSize);

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

            byte[] data = buffer.Data;

            if (data != null)
            {
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

                _CacheBufferIndex[index] = buffer;
            }

            int readbyte = data.Length - offsetInBuffer;
            count -= readbyte;

            if (count > 0)
            {
                LoadToBuffer(count, position + readbyte);
            }
        }

        public override int ReadByte()
        {
            if (_CacheBufferIndex != null)
            {
                int index = (int)(this.Position / CachedFileBufferManager.BufferUnitSize);
                int offsetInBuffer = (int)(this.Position % CachedFileBufferManager.BufferUnitSize);

                if (_CacheBufferIndex[index] != null)
                {
                    byte[] buffer = _CacheBufferIndex[index].Data;

                    if (buffer != null)
                    {
                        Position++;
                        return buffer[offsetInBuffer];
                    }
                }

                base.Position = Position;
            }


            return base.ReadByte();

        }


        public override int Read(byte[] array, int offset, int count)
        {
            if (_CacheBufferIndex != null)
            {
                int index = (int)(this.Position / CachedFileBufferManager.BufferUnitSize);
                int offsetInBuffer = (int)(this.Position % CachedFileBufferManager.BufferUnitSize);

                if (_CacheBufferIndex[index] != null)
                {
                    byte[] buffer = _CacheBufferIndex[index].Data;

                    if (buffer != null)
                    {
                        int len = Math.Min(buffer.Length - offsetInBuffer, count);

                        Array.Copy(buffer, offsetInBuffer, array, offset, len);

                        Position += len;
                        return len;
                    }
                }

                if (_Type == CachedType.Dynamic ||
                    _Type == CachedType.Full)
                {
                    if (count >= _MinCacheLength)
                    {
                        LoadToBuffer(count, Position);

                        return Read(array, offset, count);
                    }
                }

                base.Position = Position;
                return base.Read(array, offset, count);
            }


            return base.Read(array, offset, count);
        }

        public override void Close()
        {
            try
            {
                _Closed = true;

                if (_CacheBufferIndex != null)
                {
                    foreach (CachedFileBufferManager.Buffer buffer in _CacheBufferIndex)
                    {
                        buffer.Close();
                    }
                }
            }
            catch
            {
            }

            base.Close();
        }
    }

}
