using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Hubble.Framework.Serialization
{
    public class XmlSerialization
    {
        public static Stream Serialize(object obj, Encoding encode, Stream s)
        {
            TextWriter writer = null;
            writer = new StreamWriter(s, encode);

            XmlSerializer ser = new XmlSerializer(obj.GetType());

            ser.Serialize(writer, obj);
            return s;

        }

        public static MemoryStream Serialize(object obj, Encoding encode)
        {
            MemoryStream s = new MemoryStream();
            Serialize(obj, encode, s);
            s.Position = 0;
            return s;
        }

        public static MemoryStream Serialize(object obj, String encode)
        {
            return Serialize(obj, Encoding.GetEncoding(encode));
        }

        public static MemoryStream Serialize(object obj)
        {
            return Serialize(obj, Encoding.UTF8);
        }

        public static object Deserialize(System.IO.Stream In, Type objType)
        {
            In.Position = 0;
            XmlSerializer ser = new XmlSerializer(objType);
            return ser.Deserialize(In);
        }
    }


    public class XmlSerialization<T>
    {
        public static Stream Serialize(T obj, Encoding encode, Stream s)
        {
            TextWriter writer = null;
            writer = new StreamWriter(s, encode);

            XmlSerializer ser = new XmlSerializer(typeof(T));

            ser.Serialize(writer, obj);
            return s;
        }

        public static MemoryStream Serialize(T obj, Encoding encode)
        {
            MemoryStream s = new MemoryStream();
            Serialize(obj, encode, s);
            s.Position = 0;
            return s;
        }

        public static MemoryStream Serialize(T obj, String encode)
        {
            return Serialize(obj, Encoding.GetEncoding(encode));
        }

        public static MemoryStream Serialize(T obj)
        {
            return Serialize(obj, Encoding.UTF8);
        }

        public static T Deserialize(System.IO.Stream In)
        {
            In.Position = 0;
            XmlSerializer ser = new XmlSerializer(typeof(T));
            return (T)ser.Deserialize(In);
        }
    }

}
