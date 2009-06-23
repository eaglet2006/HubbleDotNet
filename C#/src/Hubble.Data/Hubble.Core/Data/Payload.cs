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
using System.Diagnostics;

namespace Hubble.Core.Data
{
    /// <summary>
    /// Payload of each document
    /// </summary>
    public class Payload
    {
        public int FileIndex = -1;
        public int[] Data;

        public Payload(int dataLength)
        {
            Data = new int[dataLength];
        }

        public Payload(int[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Get word count of one document
        /// </summary>
        /// <param name="tabIndex">tab index</param>
        /// <returns>count</returns>
        public int WordCount(int tabIndex)
        {
            return Data[tabIndex];
        }

        public void CopyTo(byte[] buf)
        {
            Debug.Assert(buf != null);
            Debug.Assert(buf.Length == Data.Length * sizeof(int));

            int start = 0;
            foreach (int d in Data)
            {
                byte[] dBuf = BitConverter.GetBytes(d);
                Array.Copy(dBuf, 0, buf, start, dBuf.Length);
                start += dBuf.Length;
            }
        }

        public void CopyFrom(byte[] buf)
        {
            Debug.Assert(buf != null);
            Debug.Assert(buf.Length == Data.Length * sizeof(int));

            int start = 0;
            while (start < buf.Length)
            {
                int d = BitConverter.ToInt32(buf, start);
                Data[start / sizeof(int)] = d;
                start += sizeof(int);
            }
        }

        public Payload Clone()
        {
            Payload payload = new Payload(Data.Length);

            this.Data.CopyTo(payload.Data, 0);

            payload.FileIndex = this.FileIndex;

            return payload;
        }
    }
}
