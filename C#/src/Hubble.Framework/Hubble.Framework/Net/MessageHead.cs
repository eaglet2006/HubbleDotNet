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

namespace Hubble.Framework.Net
{
    [Flags]
    public enum MessageFlag : short
    {
        /// <summary>
        /// This bit descript the asynchronous message
        /// </summary>
        ASyncMessage = 0x0001,

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

        /// <summary>
        /// available as ASyncMessage 
        /// Send to prior queue
        /// </summary>
        Prior = 0x0040,
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
