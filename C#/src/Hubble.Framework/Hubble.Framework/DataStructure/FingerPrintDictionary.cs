using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework;

namespace Hubble.Framework.DataStructure
{
    /// <summary>
    /// Bit16 Int
    /// </summary>
    public struct Bit16Int : IComparable<Bit16Int>
    {
        ulong _Low;
        ulong _High;

        public ulong Low
        {
            get
            {
                return _Low;
            }
        }

        public ulong High
        {
            get
            {
                return _High;
            }
        }

        public Bit16Int(ulong high, ulong low)
        {
            this._High = high;
            this._Low = low;
        }

        public Bit16Int(byte[] data)
        {
            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(data.Length == 16);

            _Low = BitConverter.ToUInt64(data, 0);
            _High = BitConverter.ToUInt64(data, 8);
        }

        public Bit16Int(int[] data)
        {
            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(data.Length == 4);

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
            return (dest.High == this.High) && (dest.Low == this.Low);
        }

        #region IComparable<Bit16Int> Members

        public int CompareTo(Bit16Int other)
        {
            if ((other.High == this.High) && (other.Low == this.Low))
            {
                return 0;
            }

            if (other.High == this.High)
            {
                return this.High.CompareTo(other.High);
            }
            else
            {
                return this.Low.CompareTo(other.Low);
            }
        }

        #endregion
    }

    public class FingerPrintDictionary<T> : IDictionary<string, T>, IEqualityComparer<Bit16Int>
    {
        private Dictionary<Bit16Int, T> _Dict;

        static System.Security.Cryptography.MD5CryptoServiceProvider _MD5 =
            new System.Security.Cryptography.MD5CryptoServiceProvider();

        public Bit16Int MD5(string key)
        {
            byte[] b = new byte[key.Length * 2];

            for (int i = 0; i < key.Length; i++)
            {
                char c = key[i];
                b[2 * i] = (byte)(c % 256);
                b[2 * i + 1] = (byte)(c / 256);
            }

            b = _MD5.ComputeHash(b);

            return new Bit16Int(b);
        }

        public FingerPrintDictionary()
        {
            _Dict = new Dictionary<Bit16Int, T>((IEqualityComparer<Bit16Int>)this);
        }

        #region IDictionary<string,T> Members

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
            return x.High == y.High && x.Low == y.Low;
        }

        public int GetHashCode(Bit16Int obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}
