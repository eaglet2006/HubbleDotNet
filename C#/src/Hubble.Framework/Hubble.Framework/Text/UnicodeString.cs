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

using Hubble.Framework.DataStructure;

namespace Hubble.Framework.Text
{
    public class UnicodeString
    {
        /// <summary>
        /// Encode unicode string for head
        /// Encoding format:
        /// /buffer length/match length of pre string/encoding bytes/vlong for position
        /// </summary>
        /// <param name="tempMem">Temp memory stream</param>
        /// <param name="buffer">encode to this buffer</param>
        /// <param name="index">encode from index</param>
        /// <param name="word">word will be encoded</param>
        /// <param name="preString">previous string for match</param>
        /// <param name="position">index data position for this word</param>
        /// <param name="length">index data length for this word</param>
        public static int Encode(MemoryStream tempMem, byte[] buffer, int index, string word, string preString, long position, long length)
        {
            tempMem.Position = 0;

            VLong vPosition = new VLong(position);
            VLong vLength = new VLong(length);

            int i = 0;

            while (i < preString.Length)
            {
                if (i >= word.Length)
                {
                    break;
                }

                if (preString[i] == 0)
                {
                    //PreString end with zero

                    break;
                }

                if (word[i] == preString[i])
                {
                    i++;
                }
                else
                {
                    break;
                }
            }

            int preMatchLen = i;

            if (preMatchLen >= word.Length)
            {
                throw new System.IO.IOException(string.Format("Reduplicate word:{0} in head!", word));
            }

            tempMem.WriteByte((byte)preMatchLen);
            vPosition.WriteToStream(tempMem);
            vLength.WriteToStream(tempMem);
            byte[] utf8Buffer = Encoding.UTF8.GetBytes(word.Substring(preMatchLen, word.Length - preMatchLen));
            tempMem.Write(utf8Buffer, 0, utf8Buffer.Length);

            long tempMemLen = tempMem.Position;

            if (tempMemLen + 1 >= 256)
            {
                throw new System.IO.IOException(string.Format("Word:{0} is too long to insert to head!", word));
            }

            if (tempMemLen + 1 >= buffer.Length - index)
            {
                return -1;
            }

            buffer[index] = (byte)tempMemLen;
            index++;
            tempMem.Position = 0;
            tempMem.Read(buffer, index, (int)tempMemLen);
            return (int)(index + tempMemLen);
        }

        public static int Decode(byte[] buffer, int index, out string word, string preString,
            out long position, out long length)
        {
            return Decode(buffer, 0, index, out word, preString, out position, out length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="start">start position of unit</param>
        /// <param name="index">index position inside unit</param>
        /// <param name="word"></param>
        /// <param name="preString"></param>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int Decode(byte[] buffer, int start, int index, out string word, string preString, 
            out long position, out long length)
        {
            index += start;

            if (index >= buffer.Length)
            {
                word = "";
                position = 0;
                length = 0;

                return -1;
            }

            VLong vPosition = new VLong();
            VLong vLength = new VLong();


            MemoryStream m = new MemoryStream(buffer);
            m.Position = index;

            int bufferLen = m.ReadByte();

            if (bufferLen <= 0)
            {
                word = "";
                position = 0;
                length = 0;

                return -1;
            }

            int preMatchLen = m.ReadByte();

            position = vPosition.ReadFromStream(m);
            length = vPosition.ReadFromStream(m);

            word = preString.Substring(0, preMatchLen) + Encoding.UTF8.GetString(buffer, (int)m.Position, (int)(bufferLen - (m.Position - index -1)));

            return index + bufferLen + 1 - start;
        }


        public static int Comparer(string str1, string str2)
        {
            if (str1 == null && str2 == null)
            {
                return 0;
            }
            else if (str1 == null)
            {
                return -1;
            }
            else if (str2 == null)
            {
                return 1;
            }

            for (int i = 0; i < str1.Length; i++)
            {
                if (i >= str2.Length)
                {
                    return 1;
                }

                if (str1[i] > str2[i])
                {
                    return 1;
                }
                else if (str1[i] < str2[i])
                {
                    return -1;
                }
            }

            if (str1.Length < str2.Length)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
