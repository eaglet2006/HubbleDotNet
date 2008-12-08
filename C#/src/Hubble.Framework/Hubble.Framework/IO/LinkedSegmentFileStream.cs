using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hubble.Framework.IO
{
    /// <summary>
    /// This stream manages the linked file system as a stream.
    /// This stream only can be used for append.
    /// It can't be used for update!
    /// </summary>
    public class LinkedSegmentFileStream : Stream
    {
        private FileStream _FileStream;
        private int _SegmentSize;
        private byte[] _CurrentSegment;
        private int _LastReadSegment = -1; //The segment that be read last time

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

        private void ReadFile(byte[] buf, int offset, int count)
        {
            _FileStream.Seek(offset, System.IO.SeekOrigin.Begin);

            int len = _FileStream.Read(buf, 0, count);

            while (len < count)
            {
                len += _FileStream.Read(_CurrentSegment, len, count - len);
            }
        }

        private void ReadOneSegment(int segment)
        {
            ReadFile(_CurrentSegment, _SegmentSize * segment, _CurrentSegment.Length);
            _LastReadSegment = segment;
        }

        private void Init()
        {
            _LastUsedSegment = -1;
            _LastUsedReserveSegment = -1;

            int curSegment = _LastSegment;

            byte[] buf = new byte[4];

            while (curSegment > 0)
            {
                ReadFile(buf, curSegment * SegmentSize + SegmentSize - 4, 4);
                int nextSegment = BitConverter.ToInt32(buf, 0);

                if (nextSegment != 0)
                {
                    if (curSegment <= _LastReserveSegment)
                    {
                        _LastUsedReserveSegment = curSegment;
                        break;
                    }
                    else
                    {
                        _LastUsedSegment = curSegment;
                        curSegment = _LastReserveSegment;
                        continue;
                    }
                }
                else if (nextSegment == 0)
                {
                    if (curSegment == _LastReserveSegment + 1)
                    {
                        _LastUsedSegment = _LastReserveSegment + 1;
                    }
                    else if (curSegment == 1)
                    {
                        _LastUsedReserveSegment = 1;
                    }
                }

                curSegment--;
            }
        }

        /// <summary>
        /// Alloc new segment 
        /// Include reserve segment and normal segment
        /// </summary>
        /// <returns></returns>
        private int AllocNewSegment()
        {
            if (CurSegment < _LastReserveSegment &&
                _LastUsedReserveSegment > 0 && _LastUsedReserveSegment < _LastReserveSegment)
            {
                //Alloc reserve segment
                _LastUsedReserveSegment++;
                _CurSegment = _LastUsedReserveSegment;
            }
            else if (_LastUsedSegment > 0 && _LastUsedSegment < _LastSegment)
            {
                _LastUsedSegment++;
                _CurSegment = _LastUsedSegment;
            }
            else
            {
                _LastUsedSegment = _LastSegment + 1;
                _CurSegment = _LastUsedSegment;
                _FileStream.SetLength((_LastSegment + 1) * SegmentSize + AutoIncreaseBytes);
                _LastSegment += AutoIncreaseBytes / SegmentSize;
            }

            return CurSegment;
        }

        #endregion

        #region Public methods

        public LinkedSegmentFileStream(FileStream fs, int segmentSize,
            int autoIncreaseBytes, int lastReserveSegment          
            )
        {
            long fileLength = fs.Length;

            if (fileLength % segmentSize != 0 || segmentSize <= 0 || autoIncreaseBytes < 0)
            {
                throw new ArgumentException("LinkedSegmentFileStream invalid parameter!");
            }


            _SegmentSize = segmentSize;
            _LastSegment = (int)(fileLength / SegmentSize) - 1;

            if (lastReserveSegment > _LastSegment)
            {
                throw new ArgumentException("LinkedSegmentFileStream invalid parameter, lastReserveSegment large then LastSegment!");
            }

            _LastReserveSegment = lastReserveSegment;
            
            _FileStream = fs;
            _CurrentSegment = new byte[segmentSize];
            _CurSegment = _BeginSegment = 1;
            _CurPositionInSegment = _BeginPositionInSegment = 0;
            _AutoIncreaseBytes = autoIncreaseBytes;


            Init();
        }

        public void Seek(int segment, int positionInSegment)
        {
            if (segment <= 0 || positionInSegment > SegmentSize - 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            _CurSegment = _BeginSegment = segment;
            _CurPositionInSegment = _BeginPositionInSegment = positionInSegment;
        }

        /// <summary>
        /// Alloc new segment for the new data
        /// Only alloc normal segment
        /// </summary>
        /// <returns></returns>
        public int AllocSegment()
        {
            if (_LastUsedSegment > 0 && _LastUsedSegment < _LastSegment)
            {
                _LastUsedSegment++;
                _CurSegment = _LastUsedSegment;
            }
            else
            {
                _LastUsedSegment = _LastSegment + 1;
                _CurSegment = _LastUsedSegment;
                _FileStream.SetLength((_LastSegment + 1) * SegmentSize + AutoIncreaseBytes);
                _LastSegment += AutoIncreaseBytes / SegmentSize;
            }

            return CurSegment;
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
            int nextSegment = 0;

            if (_CurPositionInSegment > SegmentSize - 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (_CurPositionInSegment >= SegmentSize - 4)
            {
                if (_LastReadSegment != CurSegment)
                {
                    ReadOneSegment(CurSegment);
                }

                nextSegment = NextSegment;

                if (nextSegment <= 0)
                {
                    return 0;
                }

                _CurSegment = NextSegment;
                _CurPositionInSegment = 0;
            }

            if (CurSegment <= 0)
            {
                return 0;
            }

            ReadOneSegment(CurSegment);

            int packetLength;
            nextSegment = NextSegment;

            if (nextSegment < 0)
            {
                packetLength = 0 - nextSegment - CurPositionInSegment;
            }
            else
            {
                packetLength = SegmentSize - 4 - CurPositionInSegment;
            }

            int retCount = Math.Min(count, packetLength);

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
            AllocNewSegment();

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

                relCount = Math.Min(remain, SegmentSize - 4 - CurPositionInSegment);

                Array.Copy(buffer, offset, _CurrentSegment, 0, relCount);
                
                offset += relCount;
                _CurPositionInSegment += relCount;

                remain -= relCount;
            }

            if (relCount > 0)
            {
                _FileStream.Write(_CurrentSegment, 0, relCount);
                _FileStream.Seek(SegmentSize - 4 - relCount, SeekOrigin.Current);
                _FileStream.Write(BitConverter.GetBytes(0 - relCount), 0, 4);
            }

        }

        #endregion
    }
}
