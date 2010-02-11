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

namespace Hubble.Framework.DataStructure
{
    public class VLong
    {
        long _Value;

        public VLong()
        {
            _Value = 0;
        }

        public VLong(long value)
        {
            _Value = value;
        }

        public override string ToString()
        {
            return _Value.ToString();
        }

        public static implicit operator VLong(long value)
        {
            return new VLong(value);
        }

        public static explicit operator long(VLong value)
        {
            return value._Value;
        }

        /// <summary>
        /// 7 bits every byte and only last byte's highest bit is 0
        /// </summary>
        /// <param name="stream"></param>
        public void WriteToStream(System.IO.Stream stream)
        {
            System.Diagnostics.Debug.Assert(stream != null);

            long data = _Value;

            if (data < 128)
            {
                stream.WriteByte((byte)data);
                return;
            }

            stream.WriteByte((byte)((data & 0x0000007f) | 0x80));
            data >>= 7;

            while (data > 0)
            {
                if (data < 128)
                {
                    stream.WriteByte((byte)data);
                    return;
                }

                stream.WriteByte((byte)((data & 0x0000007f) | 0x80));
                data >>= 7;
            }
        }

        static public void sWriteToStream(long data, System.IO.Stream stream)
        {
            System.Diagnostics.Debug.Assert(stream != null);

            if (data < 128)
            {
                stream.WriteByte((byte)data);
                return;
            }

            stream.WriteByte((byte)((data & 0x0000007f) | 0x80));
            data >>= 7;

            while (data > 0)
            {
                if (data < 128)
                {
                    stream.WriteByte((byte)data);
                    return;
                }

                stream.WriteByte((byte)((data & 0x0000007f) | 0x80));
                data >>= 7;
            }
        }

        public long ReadFromStream(System.IO.Stream stream)
        {
            System.Diagnostics.Debug.Assert(stream != null);

            _Value = 0;

            long b;
            int zeroBits = 0;

            do
            {
                b = (long)stream.ReadByte();

                _Value |= (long)((b & 0x7f) << zeroBits);

                zeroBits += 7;
            }
            while (b >= 128);

            return _Value;
        }

        static public long sReadFromStream(System.IO.Stream stream)
        {
            System.Diagnostics.Debug.Assert(stream != null);

            long value = 0;

            long b;
            int zeroBits = 0;

            do
            {
                b = (long)stream.ReadByte();

                value |= (long)((b & 0x7f) << zeroBits);

                zeroBits += 7;
            }
            while (b >= 128);

            return value;
        }

    }
}
