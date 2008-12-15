using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hubble.Framework.Serialization
{
    public interface IMySerialization<T>
    {
        Int16 Version { get; }
        void Serialize(Stream s);
        T Deserialize(Stream s, Int16 version);
    }

    public interface IMySerialization
    {
        Int16 Version { get; }
        void Serialize(Stream s);
        object Deserialize(Stream s, Int16 version);
    }

}
