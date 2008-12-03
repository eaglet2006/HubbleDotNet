using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hubble.Framework.Serialization
{
    public class BinSerialization
    {
        public static void SerializeBinary(object Obj, Stream s)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, Obj);
            s.Flush();
        }

        public static Stream SerializeBinary(object Obj)
        {
            MemoryStream s = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, Obj);
            s.Position = 0;
            return s;
        }

        public static void DeserializeBinary(Stream In, out object Obj)
        {
            In.Position = 0;
            IFormatter formatter = new BinaryFormatter();
            Obj = formatter.Deserialize(In);
        }

    }
}
