﻿/*
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
        private TcpServer _TcpServer;
        private MessageHead _MsgHead;
        private object _Msg;
        private object _ReturnMsg;
        private IMySerialization _CustomSerializtion;
        private int _ThreadId;
        private System.Net.Sockets.TcpClient _TcpClient;
        private System.Net.Sockets.NetworkStream _TcpStream;
        private int _ClassId;
        private object _ConnectionInfo;
        public object LockObj;
        public DateTime StartTime = DateTime.Now;
        public bool QueryStoreProcedure = false;

        /// <summary>
        /// Tcp server
        /// </summary>
        public TcpServer TcpServer
        {
            get
            {
                return _TcpServer;
            }
        }

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
        /// This property is used for async connection.
        /// Spicify the class id 
        /// </summary>
        public int ClassId
        {
            get
            {
                return _ClassId;
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

        public System.Net.Sockets.TcpClient TcpClient
        {
            get
            {
                return _TcpClient;
            }
        }

        public System.Net.Sockets.NetworkStream TcpStream
        {
            get
            {
                return _TcpStream;
            }
        }

        public object ConnectionInfo
        {
            get
            {
                return _ConnectionInfo;
            }

            set
            {
                _ConnectionInfo = value;
            }
        }

        public MessageReceiveEventArgs(TcpServer tcpServer, MessageHead msgHead, object msg, int threadId, int classId, 
            System.Net.Sockets.TcpClient tcpClient,  System.Net.Sockets.NetworkStream tcpStream, object lockObj)
        {
            LockObj = lockObj;
            _TcpServer = tcpServer;
            _MsgHead = msgHead;
            _Msg = msg;
            _ReturnMsg = null;
            _ClassId = classId;
            _CustomSerializtion = null;
            _ThreadId = threadId;
            _TcpClient = tcpClient;
            _TcpStream = tcpStream;
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
