using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;

namespace Hubble.Framework.IO
{
    /// <summary>
    /// Memory stream that append by memory block.
    /// The stream is a list of blocks;
    /// </summary>
    public class BlockedMemoryStream : System.IO.Stream
    {
        int _Position = 0;
        int _BlockSize = 0; //In bytes
        List<byte[]> _BlockList;
        long _Length = 0;

        public BlockedMemoryStream(int blockSize)
        {
            _BlockSize = blockSize;
            _BlockList = new List<byte[]>();
        }


        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            
        }

        public int BlockSize
        {
            get
            {
                return _BlockSize;
            }
        }

        public override long Length
        {
            get { return _Length; }
        }

        public override long Position
        {
            get
            {
                return _Position;
            }
            set
            {
                if (value > _Length)
                {
                    throw new System.ArgumentOutOfRangeException();
                }

                _Position = (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
            if (buffer.Length - offset < count)
                throw new ArgumentException("Argument_InvalidOffLen"); 


            int remain = count;

            while (remain > 0)
            {
                if (_Position >= _Length)
                {
                    return count - remain;
                }

                int blockIndex = _Position / _BlockSize;

                int destOffset = _Position % _BlockSize;

                int destCount = Math.Min(_BlockSize - destOffset, remain);

                destCount = Math.Min((int)(_Length - _Position), destCount);

                byte[] buf = _BlockList[blockIndex];

                Array.Copy(buf, destOffset, buffer, offset, destCount);
                offset += destCount;

                _Position += destCount;

                remain -= destCount;
            }

            return count;
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            switch (origin)
            {
                case System.IO.SeekOrigin.Begin:
                    Position = offset;
                    break;
                case System.IO.SeekOrigin.Current:
                    Position = Position + offset;
                    break;
                case System.IO.SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }

            return Position;
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
            if (buffer.Length - offset < count)
                throw new ArgumentException("Argument_InvalidOffLen"); 


            int remain = count;

            while (remain > 0)
            {
                int blockIndex = _Position / _BlockSize;

                if (blockIndex >= _BlockList.Count)
                {
                    _BlockList.Add(new byte[_BlockSize]);
                }

                int destOffset = _Position % _BlockSize;

                int destCount = Math.Min(_BlockSize - destOffset, remain);

                byte[] buf = _BlockList[blockIndex];

                Array.Copy(buffer, offset, buf, destOffset, destCount);

                _Position += destCount;
                offset += destCount;

                remain -= destCount;
            }

            if (_Position > _Length)
            {
                _Length = _Position;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _Length = 0;
            _Position = 0;
            _BlockList = new List<byte[]>();
        }
    }
}
