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
using System.Security.Cryptography;
using System.IO;

namespace Hubble.Framework.Security
{
    public class DesEncryption
    {
        private static readonly byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        public static String Encrypt(string key, String text)
        {
            return Encrypt(EncryptString.GetBytes(key), text);
        }

        public static String Encrypt(byte[] key, String text)
        {
            byte[] textArray = Encoding.UTF8.GetBytes(text);

            DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(key, IV), CryptoStreamMode.Write);
            cStream.Write(textArray, 0, textArray.Length);
            cStream.FlushFinalBlock();
            return EncryptString.GetString(mStream.ToArray());
        }

        public static String Decrypt(byte[] key, String text)
        {
            return Decrypt(key, EncryptString.GetBytes(text));
        }

        public static String Decrypt(string key, String text)
        {
            return Decrypt(EncryptString.GetBytes(key), EncryptString.GetBytes(text));
        }

        public static String Decrypt(byte[] key, byte[] textArray)
        {
            DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateDecryptor(key, IV), CryptoStreamMode.Write);
            cStream.Write(textArray, 0, textArray.Length);
            cStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(mStream.ToArray());
        }
    }
}
