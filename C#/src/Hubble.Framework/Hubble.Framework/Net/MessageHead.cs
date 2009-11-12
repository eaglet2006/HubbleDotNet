using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.Net
{
    [Flags]
    public enum MessageFlag : short
    {
        /// <summary>
        /// This bit descript the synchronous message
        /// </summary>
        SyncMessage = 0x0001,

        /// <summary>
        /// If this flag set, the message need wait a response
        /// </summary>
        NeedResponse = 0x0002,

        /// <summary>
        /// Data is null
        /// </summary>
        NullData = 0x0004,

        /// <summary>
        /// Data is string
        /// </summary>
        IsString = 0x0008,

        /// <summary>
        /// Custom serialization
        /// </summary>
        CustomSerialization = 0x0010,

        /// <summary>
        /// Is Excetpion
        /// </summary>
        IsException = 0x0020,
    }

    public struct MessageHead
    {
        public short Event;
        public MessageFlag Flag;

        public MessageHead(short evt)
        {
            Event = evt;
            Flag = 0;
        }
    }
}
