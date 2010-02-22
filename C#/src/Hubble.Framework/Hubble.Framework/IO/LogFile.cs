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
using Hubble.Framework.Reflection;

namespace Hubble.Framework.IO
{
    public class LogFile<T>
    {
        public enum LogType
        {
            AppLog = 0,
            Error  = 1,
        }

        static object _LockObj = new object();

        static LogFile<T> _LogFile = null;

        string _LogDir;

        protected string LogDir
        {
            get
            {
                if (string.IsNullOrEmpty(_LogDir))
                {
                    _LogDir = Path.AppendDivision(Path.ProcessDirectory, '\\');
                }

                return _LogDir;
            }

            set
            {
                _LogDir = value;

                try
                {
                    if (!System.IO.Directory.Exists(_LogDir))
                    {
                        System.IO.Directory.CreateDirectory(_LogDir);
                    }
                }
                catch
                {
                    _LogDir = Path.AppendDivision(Path.ProcessDirectory, '\\');
                }
            }
        }

        protected void WriteLog(LogType logType, string message)
        {
            try
            {
                string file = LogDir + DateTime.Now.ToString("yyyy-MM-dd") + "_" +
                       logType.ToString() + ".log";

                StringBuilder line = new StringBuilder();
                line.AppendFormat("LogTime:{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                line.AppendFormat("Process:{0}\r\n", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
                line.AppendFormat("Message:{0}\r\n", message);
                File.WriteLine(file, line.ToString());
            }
            catch
            {
            }
        }

        public static void WriteErrorLog(string message, Exception e)
        {
            WriteErrorLog(string.Format("ErrMsg:{0}\r\nException:{1}\r\nMessage:{2}\r\nStack:{3}",
                message, e.GetType().ToString(), e.Message, e.StackTrace));
        }


        public static void WriteErrorLog(string message)
        {
            lock (_LockObj)
            {
                if (_LogFile == null)
                {
                    _LogFile = (LogFile<T>)Instance.CreateInstance(typeof(T));
                }
            }

            _LogFile.WriteLog(LogType.Error, message);
        }

        public static void WriteAppLog(string message, bool enabled)
        {
            if (!enabled)
            {
                return;
            }

            lock (_LockObj)
            {
                if (_LogFile == null)
                {
                    _LogFile = (LogFile<T>)Instance.CreateInstance(typeof(T));
                }
            }

            _LogFile.WriteLog(LogType.AppLog, message);
        }
    }
}
