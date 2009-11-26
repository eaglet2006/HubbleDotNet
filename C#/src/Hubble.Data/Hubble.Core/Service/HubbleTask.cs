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
using Hubble.Framework.Net;
using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;

using Hubble.SQLClient;

namespace Hubble.Core.Service
{
    public class HubbleTask
    {
        TcpServer _Server;

        DateTime _StartTime;

        static bool  _Closing = false;

        static object _LockObj = new object();  

        private void ConnectEstablishEventHandler(object sender, ConnectEstablishEventArgs args)
        {
            try
            {
                CurrentConnection.Connect();
                //Global.Report.WriteAppLog(string.Format("ThreadId = {0} connected", args.ThreadId));
            }
            catch
            {
            }
        }

        private void DisconnectEventHandler(object sender, DisconnectEventArgs args)
        {
            try
            {
                CurrentConnection.Disconnect();
                //Global.Report.WriteAppLog(string.Format("ThreadId = {0} disconnected", args.ThreadId));
            }
            catch
            {
            }
        }

        private void MessageReceiveEventHandler(object sender, MessageReceiveEventArgs args)
        {
            lock (_LockObj)
            {
                if (_Closing)
                {
                    throw new Exception("Hubble task closing!");
                }
            }

            switch ((SQLClient.ConnectEvent)args.MsgHead.Event)
            {
                case SQLClient.ConnectEvent.Connect : //Connecting 
                    CurrentConnection currentConnection = new CurrentConnection(
                        new ConnectionInformation(args.InMessage as string));
                    break;

                case ConnectEvent.ExcuteSql: //excute sql
                    CurrentConnection.ConnectionInfo.StartCommand();

                    args.ReturnMsg = Excute(args.InMessage as string);

                    if (args.ReturnMsg is QueryResult)
                    {
                        args.CustomSerializtion = new Hubble.SQLClient.QueryResultSerialization(
                            args.ReturnMsg as QueryResult);
                    }

                    break;
                case ConnectEvent.Exit: //quit

                    string startTimeStr = (string)args.InMessage;

                    if (startTimeStr == null)
                    {
                        throw new Exception("StartTime is null!");
                    }

                    DateTime startTime = DateTime.ParseExact(startTimeStr, "yyyy-MM-dd HH:mm:ss", null);

                    if (!startTimeStr.Equals(_StartTime.ToString("yyyy-MM-dd HH:mm:ss"), 
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        throw new Exception("Invalid startTime!");
                    }

                    lock (_LockObj)
                    {
                        if (_Closing)
                        {
                            throw new Exception("Hubble task closing!");
                        }

                        _Closing = true;
                    }

                    System.Threading.ThreadPool.QueueUserWorkItem(Close);

                    break;

            }
        }

        private void MessageReceiveErrorEventHandler(object sender, MessageReceiveErrorEventArgs args)
        {
            try
            {
                Global.Report.WriteErrorLog(string.Format("Error = {0} ", args.InnerException.Message));
            }
            catch
            {
            }
        }

        private Hubble.Framework.Serialization.IMySerialization RequireCustomSerialization(Int16 evt, object data)
        {
            switch ((SQLClient.ConnectEvent)evt)
            {
                case ConnectEvent.ExcuteSql:
                    return new QueryResultSerialization((QueryResult)data);
            }
            return null;
        }

        private void Init(string path)
        {
            string settingPath;

            string settingFile = Hubble.Framework.IO.Path.AppendDivision(path, '\\') +
                Hubble.Core.Global.Setting.FileName;

            if (!System.IO.File.Exists(settingFile))
            {
                throw new Exception(string.Format("{0} does not exist", settingFile));
            }
            else
            {
                settingPath = System.IO.Path.GetDirectoryName(settingFile);
            }

            string currentDirectory = Environment.CurrentDirectory;

            Environment.CurrentDirectory = settingPath;

            DBProvider.Init(settingPath);
        }

        private QueryResult Excute(string sql)
        {
            using (Hubble.Core.Data.DBAccess dbAccess = new DBAccess())
            {
                return dbAccess.Query(sql);
            }
        }

        private void Close(Object stateInfo)
        {
            try
            {
                foreach (DBProvider dbProvider in DBProvider.GetDbProviders())
                {
                    dbProvider.Close();
                }

                DBProvider.StaticClose();
                bool safelyClose = false;

                //Wait 60 seconds for closing
                for (int i = 0; i < 600; i++)
                {
                    bool allTablesClosed = true;

                    foreach (DBProvider dbProvider in DBProvider.GetDbProviders())
                    {
                        if (!dbProvider.Closed)
                        {
                            allTablesClosed = false;
                            break;
                        }
                    }

                    if (allTablesClosed && DBProvider.StaticCanClose)
                    {
                        safelyClose = true;
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }

                if (!safelyClose)
                {
                    Global.Report.WriteErrorLog("Hubble task close unsafely!");
                }

            }
            catch(Exception e)
            {
                Global.Report.WriteErrorLog("Hubble task close fail", e);
            }

            System.Threading.Thread.Sleep(2000);

            foreach (DBProvider dbProvider in DBProvider.GetDbProviders())
            {
                dbProvider.CloseInvertedIndex();
            }

            System.Threading.Thread.Sleep(2000);

            Global.Report.WriteAppLog("Hubble.net close safely");

            Environment.Exit(0);
        }

        public HubbleTask(string path, bool startFromService)
        {
            _StartTime = DateTime.Now;

            path = path.Replace("\"", "");

            if (startFromService)
            {
                string intanceName = TaskInformation.GetCurrentIntanceName(path);
                TaskInformation.SetStartTime(intanceName, _StartTime);
            }    

            Init(path);

            _Server = new TcpServer(Global.Setting.Config.TcpPort);

            if (startFromService)
            {
                string intanceName = TaskInformation.GetCurrentIntanceName(path);

                if (Global.Setting.Config.TcpPort != TaskInformation.GetTcpPort(intanceName))
                {
                    TaskInformation.SetTcpPort(intanceName, Global.Setting.Config.TcpPort);
                }
            }

            _Server.MaxConnectNum = Global.Setting.Config.MaxConnectNum;

            _Server.ConnectEstablishEventHandler += ConnectEstablishEventHandler;
            _Server.DisconnectEventHandler += DisconnectEventHandler;
            _Server.MessageReceiveEventHandler += MessageReceiveEventHandler;
            _Server.MessageReceiveErrorEventHandler += MessageReceiveErrorEventHandler;
            _Server.LocalAddress = null;
            _Server.RequireCustomSerialization = RequireCustomSerialization;

            _Server.Listen();
            Global.Report.WriteAppLog("Hubble.net start successful");
        }
    }
}
