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
using Hubble.Framework;

namespace Hubble.Framework.DataStructure
{
    /// <summary>
    /// Bit16 Int
    /// </summary>
    public struct Bit16Int 
    {
        ulong _Low;
        ulong _High;

        int _StringLength;
        int _Sum;

        public Bit16Int(byte[] data, int stringLength, int sum)
        {
            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(data.Length == 16);

            _StringLength = stringLength;
            _Sum = sum;

            _Low = BitConverter.ToUInt64(data, 0);
            _High = BitConverter.ToUInt64(data, 8);
        }

        public Bit16Int(int[] data, int stringLength, int sum)
        {
            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(data.Length == 4);

            _StringLength = stringLength;
            _Sum = sum;

            _Low = (ulong)data[0] + (ulong)data[1] * 0x100000000;

            _High = (ulong)data[2] + (ulong)data[3] * 0x100000000;

        }

        public override int GetHashCode()
        {
            ulong key = _Low;
            key = (~key) + (key << 18); // key = (key << 18) - key - 1;
            key = key ^ (key >> 31);
            key = key * 21; // key = (key + (key << 2)) + (key << 4);
            key = key ^ (key >> 11);
            key = key + (key << 6);
            key = key ^ (key >> 22);

            ulong lKey = key;

            key = _High;
            key = (~key) + (key << 18); // key = (key << 18) - key - 1;
            key = key ^ (key >> 31);
            key = key * 21; // key = (key + (key << 2)) + (key << 4);
            key = key ^ (key >> 11);
            key = key + (key << 6);
            key = key ^ (key >> 22);

            return (int)(lKey + key);
        }

        public override bool Equals(object obj)
        {
            Bit16Int dest = (Bit16Int)obj;
            return (dest._High == this._High) && (dest._Low == this._Low) && 
                (dest._StringLength == this._StringLength) && (dest._Sum == this._Sum);
        }
    }

    public class FingerPrintDictionary<T> : IDictionary<string, T>, IEqualityComparer<Bit16Int>
    {
        private Dictionary<Bit16Int, T> _Dict;

        System.Security.Cryptography.MD5CryptoServiceProvider _MD5 =
            new System.Security.Cryptography.MD5CryptoServiceProvider();

        public Bit16Int MD5(string key)
        {
            byte[] b = new byte[key.Length * 2];
            int sum = 0;

            for (int i = 0; i < key.Length; i++)
            {
                char c = key[i];
                sum += c;

                b[2 * i] = (byte)(c % 256);
                b[2 * i + 1] = (byte)(c / 256);
            }

            b = _MD5.ComputeHash(b);



            return new Bit16Int(b, key.Length, sum);
        }

        public FingerPrintDictionary()
        {
            _Dict = new Dictionary<Bit16Int, T>((IEqualityComparer<Bit16Int>)this);
        }

        #region IDictionary<string,T> Members

        public void Add(string key, T value, out Bit16Int md5Key)
        {
            md5Key = MD5(key);
            _Dict.Add(md5Key, value);
        }

        public void Add(string key, T value)
        {
            _Dict.Add(MD5(key), value);
        }

        public bool ContainsKey(string key)
        {
            return _Dict.ContainsKey(MD5(key));
        }

        public ICollection<Bit16Int> GetKeys()
        {
            return _Dict.Keys;
        }

        public ICollection<string> Keys
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool Remove(Bit16Int key)
        {
            return _Dict.Remove(key);
        }

        public bool Remove(string key)
        {
            return _Dict.Remove(MD5(key));
        }

        public bool TryGetValue(string key, out T value)
        {
            return _Dict.TryGetValue(MD5(key), out value);
        }

        public ICollection<T> Values
        {
            get
            {
                return _Dict.Values;
            }
        }

        public T this[string key]
        {
            get
            {
                return _Dict[MD5(key)];
            }

            set
            {
                _Dict[MD5(key)] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,T>> Members

        public void Add(KeyValuePair<string, T> item)
        {
            _Dict.Add(MD5(item.Key), item.Value);
        }

        public void Clear()
        {
            _Dict.Clear();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return _Dict.ContainsKey(MD5(item.Key));
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get
            {
                return _Dict.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return _Dict.Remove(MD5(item.Key));
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,T>> Members

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEqualityComparer<Bit16Int> Members

        public bool Equals(Bit16Int x, Bit16Int y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Bit16Int obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}
