using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hubble.Framework.Serialization
{
    /// <summary>
    /// My serialization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MySerialization<T>
    {
        static public void Serialize(Stream s, IMySerialization<T> iMySerialization)
        {
            s.Write(BitConverter.GetBytes(iMySerialization.Version), 0, sizeof(Int16));
            iMySerialization.Serialize(s);
        }

        static public T Deserialize(Stream s, IMySerialization<T> iMySerialization)
        {
            if (s.Length - s.Position < sizeof(Int16))
            {
                return default(T);
            }

            byte[] buf = new byte[sizeof(Int16)];

            s.Read(buf, 0, sizeof(Int16));

            Int16 version = BitConverter.ToInt16(buf, 0);

            return iMySerialization.Deserialize(s, version);
        }

    }
     
    /// <summary>
    /// My serialization
    /// </summary>
    class MySerialization
    {
        static public void Serialize(Stream s, IMySerialization iMySerialization)
        {
            s.Write(BitConverter.GetBytes(iMySerialization.Version), 0, sizeof(Int16));
            iMySerialization.Serialize(s);
        }

        static public object Deserialize(Stream s, IMySerialization iMySerialization)
        {
            byte[] buf = new byte[sizeof(Int16)];

            s.Read(buf, 0, sizeof(Int16));

            Int16 version = BitConverter.ToInt16(buf, 0);

            return iMySerialization.Deserialize(s, version);
        }
    }
}
