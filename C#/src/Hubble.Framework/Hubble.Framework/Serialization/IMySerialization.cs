using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hubble.Framework.Serialization
{
    public interface IMySerialization<T>
    {
        byte Version { get; }
        void Serialize(Stream s);
        T Deserialize(Stream s, Int16 version);
    }

    public interface IMySerialization
    {
        byte Version { get; }
        void Serialize(Stream s);
        object Deserialize(Stream s, Int16 version);
    }

}
