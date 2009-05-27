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
//using System.IO;
using System.Diagnostics;

namespace Hubble.Framework.IO
{
    /// <summary>
    /// This stream manages the linked file system as a stream.
    /// This stream only can be used for append.
    /// It can't be used for update!
    /// </summary>
    public class LinkedSegmentFileStream : System.IO.Stream, IDisposable
    {
        public struct SegmentPosition
        {
            public int Segment;
            public int PositionInSegment;

            public SegmentPosition(int segment, int positionInSegment)
            {
                Segment = segment;
                PositionInSegment = positionInSegment;
            }

            public SegmentPosition(int segment)
                : this(segment, 0)
            {
            }
        }

        private System.IO.FileStream _FileStream;
        private int _SegmentSize;
        private byte[] _CurrentSegment;
        private int _LastReadSegment; //The segment that be read last time

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

            //set
            //{
            //    byte[] buf = BitConverter.GetBytes(value);

            //    Array.Copy(buf, 0, _CurrentSegment, _CurrentSegment.Length - 4, 4);
            //}
        }
        #endregion

        #region public properties

        public int LastSegment
        {
            get
            {
                return _LastSegment;
            }
        }

        public int SegmentSize
        {
            get
            {
                return _SegmentSize;
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

        /// <summary>
        /// Read file to buf, dest offset is zero.
        /// </summary>
        /// <param name="buf">buffer</param>
        /// <param name="offset">offset of file</param>
        /// <param name="count">count of bytes</param>
        private void ReadFile(byte[] buf, int offset, int count)
        {
            _FileStream.Seek(offset, System.IO.SeekOrigin.Begin);

            int read_offset = 0;

            int len = _FileStream.Read(buf, read_offset, count - read_offset);

            read_offset += len;

            while (read_offset < count && len > 0)
            {
                len = _FileStream.Read(buf, read_offset, count - read_offset);
                read_offset += len;
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

            int curSegment = LastSegment;

            byte[] buf = new byte[4];

            //Get _LastUsedReserveSegment and _LastUsedSegment
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
                        _LastUsedSegment = _LastReserveSegment;
                    }
                    else if (curSegment == 1)
                    {
                        _LastUsedReserveSegment = 1;
                    }
                }

                curSegment--;
            }
        }

        private System.IO.FileStream InitFile(string fileName, int segmentSize, int reserverSegmentLen, bool createNew)
        {
            System.IO.FileStream fs;

            if (!System.IO.File.Exists(fileName))
            {
                createNew = true;
            }

            if (createNew)
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }

                fs = new System.IO.FileStream(fileName, System.IO.FileMode.CreateNew,
                     System.IO.FileAccess.ReadWrite);

                try
                {
                    fs.SetLength(segmentSize * reserverSegmentLen);
                }
                catch (Exception e)
                {
                    Close();
                    throw e;
                }
            }
            else
            {
                fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open,
                     System.IO.FileAccess.ReadWrite);
            }

            return fs;
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
            else if (_LastUsedSegment > 0 && _LastUsedSegment < LastSegment)
            {
                _LastUsedSegment++;
                _CurSegment = _LastUsedSegment;
            }
            else
            {
                if (AutoIncreaseBytes <= 0)
                {
                    throw new System.IO.IOException("File full because AutoIncreaseBytes == 0");
                }

                _LastUsedSegment = LastSegment + 1;
                _CurSegment = _LastUsedSegment;
                _FileStream.SetLength((LastSegment + 1) * SegmentSize + AutoIncreaseBytes);
                _LastSegment += AutoIncreaseBytes / SegmentSize;
            }

            return CurSegment;
        }

        #endregion

        #region Public methods

        public LinkedSegmentFileStream(string fileName, int segmentSize,
            int autoIncreaseBytes, int lastReserveSegment)
            : this(fileName, false, segmentSize, autoIncreaseBytes, lastReserveSegment)
        {

        }

        public LinkedSegmentFileStream(string fileName, bool createNew, int segmentSize,
            int autoIncreaseBytes, int lastReserveSegment)
        {
            _LastReadSegment = -1;

            _FileStream = InitFile(fileName, segmentSize, lastReserveSegment + 1, createNew);

            long fileLength = _FileStream.Length;

            if (fileLength % segmentSize != 0 || segmentSize <= 4 || autoIncreaseBytes < 0)
            {
                Close();
                throw new ArgumentOutOfRangeException("LinkedSegmentFileStream invalid parameter!");
            }

            _SegmentSize = segmentSize;
            _LastSegment = (int)(fileLength / SegmentSize) - 1;

            if (lastReserveSegment > LastSegment)
            {
                Close();
                throw new ArgumentOutOfRangeException("LinkedSegmentFileStream invalid parameter, lastReserveSegment large then LastSegment!");
            }

            _LastReserveSegment = lastReserveSegment;
            
            _CurrentSegment = new byte[segmentSize];
            _CurSegment = 1;
            _CurPositionInSegment = 0;
            _AutoIncreaseBytes = autoIncreaseBytes;

            Init();
        }

        /// <summary>
        /// Seek to segment and positionInSegment = 0
        /// </summary>
        /// <param name="segment"></param>
        public void Seek(int segment)
        {
            Seek(segment, 0);
        }

        /// <summary>
        /// Seek to segment and positionInSegment
        /// </summary>
        /// <param name="segment">segment number. Must large then 0 and less then SegmentSize</param>
        /// <param name="positionInSegment">poistion in segment. Must less then or equal SegmentSize - 4</param>
        public void Seek(int segment, int positionInSegment)
        {
            if (segment <= 0 || positionInSegment > SegmentSize - 4 || segment > LastSegment)
            {
                throw new ArgumentOutOfRangeException();
            }

            _CurSegment = segment;
            _CurPositionInSegment = positionInSegment;
        }

        /// <summary>
        /// Alloc new segment for the new data
        /// Only alloc normal segment
        /// </summary>
        /// <returns></returns>
        public int AllocSegment()
        {
            if (_LastUsedSegment > 0 && _LastUsedSegment < LastSegment)
            {
                _LastUsedSegment++;
                _CurSegment = _LastUsedSegment;
            }
            else
            {
                if (AutoIncreaseBytes <= 0)
                {
                    throw new System.IO.IOException("File full because AutoIncreaseBytes == 0");
                }

                _LastUsedSegment = LastSegment + 1;
                _CurSegment = _LastUsedSegment;
                _FileStream.SetLength((LastSegment + 1) * SegmentSize + AutoIncreaseBytes);
                _LastSegment += AutoIncreaseBytes / SegmentSize;
            }

            return CurSegment;
        }


        /// <summary>
        /// Get last segment number of one data.
        /// There are a lot of data save in this file.
        /// We need know each data's last segment number
        /// </summary>
        /// <param name="segment">from this segment number</param>
        /// <returns></returns>
        public SegmentPosition GetLastSegmentNumberFrom(int segment)
        {
            if (segment <= 0 || segment > LastSegment)
            {
                throw new ArgumentOutOfRangeException();
            }

            int oldSegment;
            int nextSegment = segment;

            byte[] buf = new byte[4];

            do
            {
                _FileStream.Seek(nextSegment * SegmentSize + SegmentSize - 4, System.IO.SeekOrigin.Begin);
                _FileStream.Read(buf, 0, 4);

                oldSegment = nextSegment;
                nextSegment = BitConverter.ToInt32(buf, 0);

                //Only distrustful file can raise this exception
                if (nextSegment > LastSegment)
                {
                    throw new System.IO.IOException(string.Format("Distrustful data, next segment large then LastSegment!, from segment {0} nextSegment {1}",
                        segment, nextSegment));
                }

                //If segment point to a free segment block, raise this exception
                if (nextSegment == 0)
                {
                    throw new System.IO.IOException(string.Format("Distrustful data, next segment is zero!, from segment {0}",
                        segment));
                }
            } while (nextSegment > 0);

            return new SegmentPosition(oldSegment, 0 - nextSegment);
        }

        public override void Close()
        {
            lock (this)
            {
                try
                {
                    if (_FileStream != null)
                    {
                        _FileStream.Close();
                    }
                }
                catch
                {
                }

                _FileStream = null;
            }

            base.Close();
        }

        ~LinkedSegmentFileStream()
        {
            Close();
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
            Debug.Assert(CurSegment > 0);
            Debug.Assert(_CurPositionInSegment <= SegmentSize - 4);

            int nextSegment = 0;

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

                _CurSegment = nextSegment;
                _CurPositionInSegment = 0;
            }

            //if (CurSegment <= 0)
            //{
            //    return 0;
            //}

            ReadOneSegment(CurSegment);

            int packetLength;
            nextSegment = NextSegment;

            if (nextSegment < 0)
            {
                packetLength = 0 - nextSegment - CurPositionInSegment;
            }
            else if (nextSegment == 0)
            {
                return 0;
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

        public override long Seek(long offset, System.IO.SeekOrigin origin)
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
            Debug.Assert(_CurPositionInSegment <= SegmentSize - 4);

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
                _FileStream.Seek(_SegmentSize * (CurSegment + 1) - 4 , System.IO.SeekOrigin.Begin);
                _FileStream.Write(BitConverter.GetBytes(0 - CurPositionInSegment), 0, 4);
            }

        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            Close();
        }

        #endregion
    }
}
