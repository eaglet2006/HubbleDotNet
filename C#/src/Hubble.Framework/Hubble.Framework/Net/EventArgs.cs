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
using System.IO;
using Hubble.Framework.Serialization;

namespace Hubble.Framework.Net
{
    /// <summary>
    /// Occur when tcp connection establish
    /// </summary>
    public class ConnectEstablishEventArgs : EventArgs
    {
        int _ThreadId;

        /// <summary>
        /// The thread id that deal with current conenction
        /// </summary>
        public int ThreadId
        {
            get
            {
                return _ThreadId;
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="threadId">The thread id that deal with current conenction</param>
        public ConnectEstablishEventArgs(int threadId)
        {
            _ThreadId = threadId;
        }
    }

    /// <summary>
    /// Occur when tcp disconnect
    /// </summary>
    public class DisconnectEventArgs : EventArgs
    {
        int _ThreadId;

        /// <summary>
        /// The thread id that deal with current conenction
        /// </summary>
        public int ThreadId
        {
            get
            {
                return _ThreadId;
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="threadId">The thread id that deal with current conenction</param>
        public DisconnectEventArgs(int threadId)
        {
            _ThreadId = threadId;
        }
    }

    /// <summary>
    /// Message as a object
    /// </summary>
    public class ObjectMessageReceiveEventArgs : EventArgs
    {
        private MessageHead _MsgHead;
        private object _DataIn;
        private object _Response = null;

        /// <summary>
        /// Message Head
        /// </summary>
        public MessageHead MsgHead
        {
            get
            {
                return _MsgHead;
            }
        }

        /// <summary>
        /// Input data from client
        /// </summary>
        public object DataIn
        {
            get
            {
                return _DataIn;
            }
        }

        /// <summary>
        /// Response data to client
        /// </summary>
        public object Response
        {
            get
            {
                return _Response;
            }

            set
            {
                _Response = value;
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="msgHead">message head</param>
        /// <param name="dataIn">Input data</param>
        public ObjectMessageReceiveEventArgs(MessageHead msgHead, object dataIn)
        {
            _MsgHead = msgHead;
            _DataIn = dataIn;
        }
    }


    public class RequireCustomSerializationEventArgs : EventArgs
    {
        private Int16 _Event;
        private IMySerialization _CustomSerializtion;

        public Int16 Event
        {
            get
            {
                return _Event;
            }
        }

        /// <summary>
        /// Custom serialization
        /// </summary>
        public IMySerialization CustomSerializtion
        {
            get
            {
                return _CustomSerializtion;
            }

            set
            {
                _CustomSerializtion = value;
            }
        }

        public RequireCustomSerializationEventArgs(Int16 evt)
        {
            _Event = evt;
        }
    }

    /// <summary>
    /// Message as a stream
    /// </summary>
    public class MessageReceiveEventArgs : EventArgs
    {
        private MessageHead _MsgHead;
        private object _Msg;
        private object _ReturnMsg;
        private IMySerialization _CustomSerializtion;
        private int _ThreadId;

        /// <summary>
        /// Message Head
        /// </summary>
        public MessageHead MsgHead
        {
            get
            {
                return _MsgHead;
            }
        }


        /// <summary>
        /// Message in.
        /// </summary>
        public object InMessage
        {
            get
            {
                return _Msg;
            }
        }

        /// <summary>
        /// Return message
        /// </summary>
        public object ReturnMsg
        {
            get
            {
                return _ReturnMsg;
            }

            set
            {
                _ReturnMsg = value;
            }
        }

        /// <summary>
        /// Custom serialization
        /// </summary>
        public IMySerialization CustomSerializtion
        {
            get
            {
                return _CustomSerializtion;
            }

            set
            {
                _CustomSerializtion = value;
            }
        }

        public int ThreadId
        {
            get
            {
                return _ThreadId;
            }

            set
            {
                _ThreadId = value;
            }
        }

        public MessageReceiveEventArgs(MessageHead msgHead, object msg, int threadId)
        {
            _MsgHead = msgHead;
            _Msg = msg;
            _ReturnMsg = null;
            _CustomSerializtion = null;
            _ThreadId = threadId;
        }

    }

    /// <summary>
    /// Message Error
    /// </summary>
    public class MessageReceiveErrorEventArgs : EventArgs
    {
        private MessageHead _MsgHead;
        private Exception _InnerException;

        /// <summary>
        /// Message Head
        /// </summary>
        public MessageHead MsgHead
        {
            get
            {
                return _MsgHead;
            }
        }


        /// <summary>
        /// Inner exception.
        /// </summary>
        public Exception InnerException
        {
            get
            {
                return _InnerException;
            }
        }

        public MessageReceiveErrorEventArgs(Exception e)
        {
            _MsgHead = new MessageHead();
            _InnerException = e;
        }

        public MessageReceiveErrorEventArgs(MessageHead msgHead, Exception e)
        {
            _MsgHead = msgHead;
            _InnerException = e;
        }
    }

}
