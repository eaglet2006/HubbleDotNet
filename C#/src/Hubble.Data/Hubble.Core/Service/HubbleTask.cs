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

        private void ConnectEstablishEventHandler(object sender, ConnectEstablishEventArgs args)
        {
            try
            {
                CurrentConnection.Connect();
                Global.Report.WriteAppLog(string.Format("ThreadId = {0} connected", args.ThreadId));
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
                Global.Report.WriteAppLog(string.Format("ThreadId = {0} disconnected", args.ThreadId));
            }
            catch
            {
            }
        }

        private void MessageReceiveEventHandler(object sender, MessageReceiveEventArgs args)
        {
            switch ((SQLClient.ConnectEvent)args.MsgHead.Event)
            {
                case SQLClient.ConnectEvent.Connect : //Connecting 
                    CurrentConnection currentConnection = new CurrentConnection(
                        new ConnectionInformation(args.InMessage as string));
                    break;

                case ConnectEvent.ExcuteSql: //excute sql
                    args.ReturnMsg = Excute(args.InMessage as string);

                    if (args.ReturnMsg is QueryResult)
                    {
                        args.CustomSerializtion = new Hubble.SQLClient.QueryResultSerialization(
                            args.ReturnMsg as QueryResult);
                    }

                    break;
                case ConnectEvent.Exit: //quit
                    Environment.Exit(0);
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

        public HubbleTask(string path)
        {
            Init(path);

            _Server = new TcpServer(Global.Setting.Config.TcpPort);

            _Server.MaxConnectNum = Global.Setting.Config.MaxConnectNum;

            _Server.ConnectEstablishEventHandler += ConnectEstablishEventHandler;
            _Server.DisconnectEventHandler += DisconnectEventHandler;
            _Server.MessageReceiveEventHandler += MessageReceiveEventHandler;
            _Server.MessageReceiveErrorEventHandler += MessageReceiveErrorEventHandler;
            _Server.LocalAddress = null;
            _Server.RequireCustomSerialization = RequireCustomSerialization;

            _Server.Listen();
        }
    }
}
