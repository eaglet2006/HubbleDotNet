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
