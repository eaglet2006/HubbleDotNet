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
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hubble.Framework.Net
{
    [Serializable]
    public class TcpCacheStream : Stream
    {
        #region Private fields
        const int BUF_SIZE = 4096;
        private byte[] _Buf = new byte[BUF_SIZE];

        private MemoryStream _CacheStream = new MemoryStream(BUF_SIZE);
        private NetworkStream _NetworkStream;

        private int _BufLen = 0;

        #endregion

        #region Private properties

        private MemoryStream CacheStream
        {
            get
            {
                return _CacheStream;
            }
        }

        #endregion


        #region Public properties

        /// <summary>
        /// get or set the Network Stream
        /// </summary>
        public NetworkStream NetworkStream
        {
            get
            {
                return _NetworkStream;
            }
        }

        #endregion


        public TcpCacheStream(NetworkStream networkStream)
        {
            _NetworkStream = networkStream;
        }

        #region Implement stream class

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
            NetworkStream.Write(_Buf, 0, _BufLen);
            NetworkStream.Flush();
        }

        public override long Length
        {
            get
            {
                throw new Exception("This stream can not seek!");
            }
        }

        public override long Position
        {
            get
            {
                throw new Exception("This stream can not seek!");
            }

            set
            {
                throw new Exception("This stream can not seek!");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int len = 0;

            //If cache is not empty, read from cache
            if (CacheStream.Length > CacheStream.Position)
            {
                len = CacheStream.Read(buffer, offset, count);
                return len;
            }

            //Fill cache
            len = NetworkStream.Read(_Buf, 0, BUF_SIZE);

            if (len == 0)
            {
                return 0;
            }

            CacheStream.Position = 0;
            CacheStream.Write(_Buf, 0, len);
            CacheStream.SetLength(len);
            CacheStream.Position = 0;

            len = CacheStream.Read(buffer, offset, count);

            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("This stream can not seek!");
        }

        public override void SetLength(long value)
        {
            throw new Exception("This stream can not seek!");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("Count + offset large then buffer.Length");
            }

            int remain = count - (BUF_SIZE - _BufLen);

            if (remain < 0)
            {
                Array.Copy(buffer, offset, _Buf, _BufLen, count);
                _BufLen = BUF_SIZE + remain;
            }
            else
            {
                Array.Copy(buffer, offset, _Buf, _BufLen, BUF_SIZE - _BufLen);
                NetworkStream.Write(_Buf, 0, _Buf.Length);

                offset += BUF_SIZE - _BufLen;

                while (remain >= BUF_SIZE)
                {
                    NetworkStream.Write(buffer, offset, BUF_SIZE);
                    offset += BUF_SIZE;
                    remain -= BUF_SIZE;
                }

                Array.Copy(buffer, offset, _Buf, 0, remain);

                _BufLen = remain;
            }
        }

        #endregion
    }
}
