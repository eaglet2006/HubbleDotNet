using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.Net
{
    [Serializable]
    public class InnerServerException 
    {
        string _InnerStackTrace = "";

        public string InnerStackTrace
        {
            get
            {
                return _InnerStackTrace;
            }
        }

        string _Message = "";

        public string Message
        {
            get
            {
                return _Message;
            }
        }

        public InnerServerException()
        {
        }

        public InnerServerException(string message, string stack)
        {
            _Message = message;
            _InnerStackTrace = stack;
        }

        public InnerServerException(Exception e)
        {
            _Message = e.Message;
            _InnerStackTrace = e.StackTrace;
        }
    }

    [Serializable]
    public class ServerException : Exception
    {
        string _InnerStackTrace = "";

        public string InnerStackTrace
        {
            get
            {
                return _InnerStackTrace;
            }
        }

        public ServerException()
        {
        }

        public ServerException(InnerServerException e)
            :base(e.Message)
        {
            _InnerStackTrace = e.InnerStackTrace;
        }

        public ServerException(string message, string stack)
            : base(message)
        {
            _InnerStackTrace = stack;
        }
    }
}
