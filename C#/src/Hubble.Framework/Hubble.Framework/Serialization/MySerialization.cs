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
            //s.Write(BitConverter.GetBytes(iMySerialization.Version), 0, sizeof(byte));
            s.WriteByte(iMySerialization.Version);
            iMySerialization.Serialize(s);
        }

        static public T Deserialize(Stream s, IMySerialization<T> iMySerialization)
        {
            if (s.Length - s.Position < sizeof(byte))
            {
                return default(T);
            }

            byte version = (byte)s.ReadByte();

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
            //s.Write(BitConverter.GetBytes(iMySerialization.Version), 0, sizeof(byte));
            s.WriteByte(iMySerialization.Version);

            iMySerialization.Serialize(s);
        }

        static public object Deserialize(Stream s, IMySerialization iMySerialization)
        {
            byte version = (byte)s.ReadByte();

            return iMySerialization.Deserialize(s, version);
        }
    }
}
