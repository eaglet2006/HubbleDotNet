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

namespace Hubble.Framework.IO
{
    /// <summary>
    /// use existed buffer establish the logical buffer.
    /// logical buf is part of the original buffer.
    /// This call usually used for don't want to copy data from original buffer
    /// to new buffer.
    /// </summary>
    public class BufferMemory
    {
        /// <summary>
        /// original buffer
        /// </summary>
        public byte[] Buf;

        /// <summary>
        /// logical buf start index
        /// </summary>
        public int Start;

        /// <summary>
        /// current position
        /// </summary>
        public int Position;

        /// <summary>
        /// logical buf length
        /// </summary>
        public int Length;

        public BufferMemory(byte[] buf, int start, int length)
        {
            if (buf == null)
            {
                throw new ArgumentException("Buf can't be null");
            }

            if (start < 0 || start >= buf.Length)
            {
                throw new ArgumentException("Invalid start");
            }

            if (start + length > buf.Length)
            {
                throw new ArgumentOutOfRangeException("Invalid length");
            }

            Buf = buf;
            Start = start;
            Length = length;
            Position = start;
        }
    }
}
