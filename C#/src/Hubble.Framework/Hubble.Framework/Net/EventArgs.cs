using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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


    /// <summary>
    /// Message as a stream
    /// </summary>
    public class MessageReceiveEventArgs : EventArgs
    {
        private MessageHead _MsgHead;
        private object _Msg;
        private object _ReturnMsg;

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

        public MessageReceiveEventArgs(MessageHead msgHead, object msg)
        {
            _MsgHead = msgHead;
            _Msg = msg;
            _ReturnMsg = null;
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
