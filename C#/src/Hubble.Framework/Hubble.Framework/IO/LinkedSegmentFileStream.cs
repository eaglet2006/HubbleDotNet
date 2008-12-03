using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hubble.Framework.IO
{
    public class LinkedSegmentFileStream : Stream
    {
        private FileStream _FileStream;
        private int _SegmentSize;
        private byte[] _CurrentSegment;
        private int _BeginSegment;
        private int _BeginPositionInSegment;
        private int _CurSegment;
        private int _CurPositionInSegment;
        private int _AutoIncreaseBytes;

        private int _LastUsedReserveSegment;
        private int _LastReserveSegment;
        private int _LastSegment;
        private int _LastUsedSegment;

        #region private properties
        private int NextSegment
        {
            get
            {
                return BitConverter.ToInt32(_CurrentSegment, _CurrentSegment.Length - 4);
            }

            set
            {
                byte[] buf = BitConverter.GetBytes(value);

                Array.Copy(buf, 0, _CurrentSegment, _CurrentSegment.Length - 4, 4);
            }
        }
        #endregion

        #region public properties

        public int SegmentSize
        {
            get
            {
                return _SegmentSize;
            }
        }

        public int BeginSegment
        {
            get
            {
                return _BeginSegment;
            }
        }

        public int BeginPositionInSegment
        {
            get
            {
                return _BeginPositionInSegment;
            }
        }

        public int CurSegment
        {
            get
            {
                return _CurSegment;
            }
        }

        public int CurPositionInSegment
        {
            get
            {
                return _CurPositionInSegment;
            }
        }

        /// <summary>
        /// Increase file size automaticly when accesses this file overflow.
        /// </summary>
        public int AutoIncreaseBytes
        {
            get
            {
                return _AutoIncreaseBytes;
            }
        }

        #endregion

        #region Private methods

        private void ReadOneSegment(int segment)
        {
            _FileStream.Seek(_SegmentSize * segment, System.IO.SeekOrigin.Begin);

            int len = _FileStream.Read(_CurrentSegment, 0, _CurrentSegment.Length);
            int offset = len;

            while (offset < _CurrentSegment.Length)
            {
                len = _FileStream.Read(_CurrentSegment, offset, _CurrentSegment.Length - offset);
                offset += len;
            }
        }
        #endregion

        #region Public methods

        public LinkedSegmentFileStream(FileStream fs, int segmentSize,
            int segment, int positionInSegment, int autoIncreaseBytes, long fileLength,
            int lastUsedReserveSegment, int lastReserveSegment, int lastUsedSegment          
            )
        {
            _FileStream = fs;
            _SegmentSize = segmentSize;
            _CurrentSegment = new byte[segmentSize];
            _CurSegment = _BeginSegment = segment;
            _CurPositionInSegment = _BeginPositionInSegment = positionInSegment;
            _AutoIncreaseBytes = autoIncreaseBytes;

            _LastSegment = (int)(fileLength / SegmentSize) - 1;
            _LastUsedReserveSegment = lastUsedReserveSegment;
            _LastReserveSegment = lastReserveSegment;
            _LastUsedSegment = lastUsedSegment;
        }

        #endregion

        #region Override from Stream

        public override bool CanRead
        {
            get 
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get 
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get 
            {
                return true;
            }
        }

        public override void Flush()
        {
            _FileStream.Flush();
        }

        public override long Length
        {
            get 
            {
                throw new Exception("The method or operation is not supported."); 
            }
        }

        public override long Position
        {
            get
            {
                throw new Exception("The method or operation is not supported.");
            }

            set
            {
                throw new Exception("The method or operation is not supported.");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_CurPositionInSegment > SegmentSize - 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (_CurPositionInSegment >= SegmentSize - 4)
            {
                _CurSegment = NextSegment;
                _CurPositionInSegment = 0;
            }

            if (CurSegment <= 0)
            {
                return 0;
            }

            ReadOneSegment(CurSegment);

            int retCount = Math.Min(count, SegmentSize -4 - CurPositionInSegment);

            Array.Copy(_CurrentSegment, CurPositionInSegment, buffer, offset, retCount);

            _CurPositionInSegment += retCount;

            return retCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("The method or operation is not supported.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not supported.");
        }

        private void SetNextSegment(int relCount)
        {
            if (CurSegment <= _LastReserveSegment)
            {
                _LastUsedReserveSegment++;

                if (_LastUsedReserveSegment > _LastReserveSegment)
                {
                    _LastUsedSegment++;
                    _CurSegment = _LastUsedSegment;
                }
                else
                {
                    _CurSegment = _LastUsedReserveSegment;
                }

                if (CurSegment > _LastSegment)
                {
                    _FileStream.SetLength(_LastSegment * SegmentSize + AutoIncreaseBytes);
                    _LastSegment += AutoIncreaseBytes / SegmentSize;
                }
            }

            if (relCount <= 0)
            {
                _FileStream.Write(BitConverter.GetBytes(CurSegment), 0, 4);
            }
            else
            {
                Array.Copy(BitConverter.GetBytes(CurSegment), 0, _CurrentSegment, relCount, 4);
                _FileStream.Write(_CurrentSegment, 0, relCount + 4);
            }

            _CurPositionInSegment = 0;

            _FileStream.Seek(_SegmentSize * CurSegment + CurPositionInSegment, System.IO.SeekOrigin.Begin);

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_CurPositionInSegment > SegmentSize - 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            _FileStream.Seek(_SegmentSize * CurSegment + CurPositionInSegment, System.IO.SeekOrigin.Begin);

            int remain = count;
            int relCount = 0;

            while (remain > 0)
            {
                if (_CurPositionInSegment == SegmentSize - 4)
                {
                    SetNextSegment(relCount);
                }

                relCount = Math.Min(count, SegmentSize - 4 - CurPositionInSegment);

                Array.Copy(buffer, offset, _CurrentSegment, 0, relCount);

                _CurPositionInSegment += relCount;

                remain -= relCount;
            }

            if (relCount > 0)
            {
                _FileStream.Write(_CurrentSegment, 0, relCount);
            }

        }

        #endregion
    }
}
