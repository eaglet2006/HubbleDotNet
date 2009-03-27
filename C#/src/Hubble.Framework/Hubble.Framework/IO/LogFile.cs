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

        public static void WriteAppLog(string message)
        {
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
