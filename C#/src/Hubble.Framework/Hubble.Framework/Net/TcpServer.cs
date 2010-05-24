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
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hubble.Framework.Net
{
    public class TcpServer
    {
        class PCB
        {
            public int ThreadId;
            public System.Net.Sockets.TcpClient Client;
        }

        #region Private fields
        private System.Net.Sockets.TcpListener _TcpListener;
        private Thread _ListenThread;
        private IPAddress _LocalAddress = IPAddress.Parse("127.0.0.1");
        private int _Port = 8800;
        private int _MaxConnectNum = 32;
        //private Thread[] _ThreadPool = null;
        private Queue<int> _IdleThreadQueue = new Queue<int>();

        RequireCustomSerializationDelegate _RequireCustomSerialization;

        #endregion

        #region public properties

        /// <summary>
        /// An IPAddress that represents the local IP address.
        /// </summary>
        public IPAddress LocalAddress
        {
            get
            {
                return _LocalAddress;
            }

            set
            {
                _LocalAddress = value;
            }
        }

        /// <summary>
        /// The port on which to listen for incoming connection attempts. 
        /// </summary>
        public int Port
        {
            get
            {
                return _Port;
            }

            set
            {
                _Port = value;
            }
        }

        /// <summary>
        /// The max connect number of tcp client 
        /// </summary>
        public int MaxConnectNum
        {
            get
            {
                return _MaxConnectNum;
            }

            set
            {
                _MaxConnectNum = value;
            }
        }

        public int[] IdleThreadIdList
        {
            get
            {
                lock (this)
                {
                    return _IdleThreadQueue.ToArray();
                }
            }
        }

        public RequireCustomSerializationDelegate RequireCustomSerialization
        {
            get
            {
                return _RequireCustomSerialization;
            }

            set
            {
                _RequireCustomSerialization = value;

            }
        }

        #endregion

        #region Delegates

        public delegate Hubble.Framework.Serialization.IMySerialization RequireCustomSerializationDelegate(Int16 evt, object data);

        #endregion

        #region Events

        /// <summary>
        /// The event occur when tcp connection establish
        /// </summary>
        public event EventHandler<ConnectEstablishEventArgs> ConnectEstablishEventHandler;
        private void OnConnectEstablishEvent(ConnectEstablishEventArgs args)
        {
            EventHandler<ConnectEstablishEventArgs> handler = ConnectEstablishEventHandler;

            if (handler != null)
            {
                handler(this, args);
            }
        }

        ///
        /// The event occur when tcp disconnect
        /// </summary>
        public event EventHandler<DisconnectEventArgs> DisconnectEventHandler;
        private void OnDisconnectEvent(DisconnectEventArgs args)
        {
            EventHandler<DisconnectEventArgs> handler = DisconnectEventHandler;

            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// The event of receive message as object
        /// </summary>
        public event EventHandler<ObjectMessageReceiveEventArgs> ObjectMessageReceiveEventHandler;
        private void OnObjectMessageReceiveEvent(ObjectMessageReceiveEventArgs args)
        {
            EventHandler<ObjectMessageReceiveEventArgs> handler = ObjectMessageReceiveEventHandler;

            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// The event of receive message as stream
        /// </summary>
        public event EventHandler<MessageReceiveEventArgs> MessageReceiveEventHandler;

        private void OnMessageReceiveEvent(MessageReceiveEventArgs args)
        {
            EventHandler<MessageReceiveEventArgs> handler = MessageReceiveEventHandler;

            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// The event of receive message error
        /// </summary>
        public event EventHandler<MessageReceiveErrorEventArgs> MessageReceiveErrorEventHandler;

        private void OnMessageReceiveErrorEvent(MessageReceiveErrorEventArgs args)
        {
            EventHandler<MessageReceiveErrorEventArgs> handler = MessageReceiveErrorEventHandler;

            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion

        #region private methods

        private void InitThreadPool()
        {
            //_ThreadPool = new Thread[MaxConnectNum];

            for (int id = 0; id < MaxConnectNum; id++)
            //for (int id = 0; id < _ThreadPool.Length; id++)
            {
                //Thread t = _ThreadPool[id];
                //t = new Thread(HandleClientComm);
                _IdleThreadQueue.Enqueue(id);
            }
        }

        private PCB GetPCB(System.Net.Sockets.TcpClient client)
        {
            lock (this)
            {
                if (_IdleThreadQueue.Count == 0)
                {
                    return null;
                }

                int id = _IdleThreadQueue.Dequeue();
                PCB pcb = new PCB();
                pcb.ThreadId = id;
                pcb.Client = client;
                return pcb;
            }
        }

        private void RetPCB(PCB pcb)
        {
            lock (this)
            {
                if (pcb == null)
                {
                    return;
                }

                _IdleThreadQueue.Enqueue(pcb.ThreadId);
            }
        }

        private void ListenForClients()
        {
            try
            {
                this._TcpListener.Start();

                while (true)
                {
                    //blocks until a client has connected to the server
                    System.Net.Sockets.TcpClient client = this._TcpListener.AcceptTcpClient();

                    try
                    {
                        //create a thread to handle communication
                        //with connected client
                        Thread clientThread =
                           new Thread(new
                           ParameterizedThreadStart(HandleClientComm));

                        PCB pcb = GetPCB(client);

                        if (pcb == null)
                        {
                            //Over max connect number
                            ReturnMessage(client.GetStream(), new MessageHead(), new Exception("Too many connects on server"), null);

                            System.Threading.Thread.Sleep(200);

                            client.Close();
                            throw new Exception("Too many connects on server");
                        }
                        else
                        {
                            clientThread.Start(pcb);
                        }
                    }
                    catch (Exception e)
                    {
                        OnMessageReceiveErrorEvent(new MessageReceiveErrorEventArgs(e));
                    }
                }
            }
            catch (Exception e)
            {
                OnMessageReceiveErrorEvent(new MessageReceiveErrorEventArgs(e));
            }
        }

        private void ReturnMessage(System.Net.Sockets.NetworkStream clientStream, MessageHead msgHead, 
            object returnMsg, Hubble.Framework.Serialization.IMySerialization customSerializer)
        {
            Hubble.Framework.Net.TcpCacheStream tcpStream = new TcpCacheStream(clientStream);

            byte[] sendBuf = BitConverter.GetBytes(msgHead.Event);
            tcpStream.Write(sendBuf, 0, sendBuf.Length);

            if (returnMsg != null)
            {
                //Send Flag

                msgHead.Flag = 0;

                if (customSerializer != null)
                {
                    msgHead.Flag |= MessageFlag.CustomSerialization;
                    sendBuf = BitConverter.GetBytes((short)msgHead.Flag);
                    tcpStream.Write(sendBuf, 0, sendBuf.Length);

                    //MemoryStream m = new MemoryStream();

                    //customSerializer.Serialize(m);
                    //m.Position = 0;

                    //while (m.Position < m.Length)
                    //{
                    //    byte[] buf = new byte[8192];

                    //    int len = m.Read(buf, 0, buf.Length);

                    //    tcpStream.Write(buf, 0, len);
                    //}

                    //customSerializer.Serialize(tcpStream);
                    customSerializer.Serialize(tcpStream);
                    
                }
                else if (returnMsg is string)
                {
                    msgHead.Flag |= MessageFlag.IsString;
                    sendBuf = BitConverter.GetBytes((short)msgHead.Flag);
                    tcpStream.Write(sendBuf, 0, sendBuf.Length);

                    byte[] buf = Encoding.UTF8.GetBytes((returnMsg as string));

                    for (int i = 0; i < buf.Length; i++)
                    {
                        if (buf[i] == 0)
                        {
                            buf[i] = 0x20;
                        }
                    }

                    tcpStream.Write(buf, 0, buf.Length);
                    tcpStream.WriteByte(0);
                    
                }
                else if (returnMsg is Exception)
                {
                    msgHead.Flag |= MessageFlag.IsException;
                    msgHead.Flag |= MessageFlag.IsString;

                    sendBuf = BitConverter.GetBytes((short)msgHead.Flag);
                    tcpStream.Write(sendBuf, 0, sendBuf.Length);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} innerStackTrace:{1}",
                        (returnMsg as Exception).Message, (returnMsg as Exception).StackTrace);

                    byte[] buf = Encoding.UTF8.GetBytes(sb.ToString());

                    tcpStream.Write(buf, 0, buf.Length);
                    tcpStream.WriteByte(0);
                }
                else
                {
                    sendBuf = BitConverter.GetBytes((short)msgHead.Flag);
                    tcpStream.Write(sendBuf, 0, sendBuf.Length);

                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(tcpStream, returnMsg);
                }
            }
            else
            {
                msgHead.Flag |= MessageFlag.NullData;

                //Send Flag
                sendBuf = BitConverter.GetBytes((short)msgHead.Flag);
                tcpStream.Write(sendBuf, 0, sendBuf.Length);
            }

            tcpStream.Flush();
        }

        private void HandleClientComm(object pcb)
        {
            PCB p = (PCB)pcb;

            System.Net.Sockets.TcpClient tcpClient = p.Client;

            try
            {
                try
                {
                    OnConnectEstablishEvent(new ConnectEstablishEventArgs(p.ThreadId));
                }
                catch (Exception e)
                {
                    OnMessageReceiveErrorEvent(new MessageReceiveErrorEventArgs(e));
                }

                System.Net.Sockets.NetworkStream clientStream = tcpClient.GetStream();

                while (true)
                {
                    MessageHead msgHead = new MessageHead();

                    try
                    {
                        bool disconnected = false;
                        object msg = null;

                        //Recevie data
                        TcpCacheStream tcpStream = new TcpCacheStream(clientStream);

                        byte[] revBuf = new byte[4];
                        int offset = 0;

                        while (offset < 4)
                        {
                            int len;

                            try
                            {
                                len = tcpStream.Read(revBuf, offset, 4 - offset);
                            }
                            catch
                            {
                                disconnected = true;
                                break;
                            }

                            if (len == 0)
                            {
                                //Disconnect
                                disconnected = true;
                                break;
                            }

                            offset += len;
                        }

                        if (disconnected)
                        {
                            break;
                        }

                        msgHead.Event = BitConverter.ToInt16(revBuf, 0);
                        msgHead.Flag = (MessageFlag)BitConverter.ToInt16(revBuf, 2);

                        if ((msgHead.Flag & MessageFlag.NullData) == 0)
                        {
                            if ((msgHead.Flag & MessageFlag.CustomSerialization) != 0)
                            {
                                if (RequireCustomSerialization != null)
                                {
                                    Hubble.Framework.Serialization.IMySerialization mySerializer =
                                        RequireCustomSerialization(msgHead.Event, null);

                                    if (mySerializer == null)
                                    {
                                        throw new Exception(string.Format("RequireCustomSerialization of Event = {0} is null!",
                                            msgHead.Event));
                                    }

                                    msg = mySerializer.Deserialize(tcpStream, mySerializer.Version);
                                }
                                else
                                {
                                    throw new Exception("RequireCustomSerialization of TcpClient is null!");
                                }
                            }
                            else if ((msgHead.Flag & MessageFlag.IsString) == 0)
                            {
                                IFormatter formatter = new BinaryFormatter();
                                msg = formatter.Deserialize(tcpStream);
                            }
                            else
                            {
                                MemoryStream m = new MemoryStream();

                                byte[] buf = new byte[1024];

                                int len = 0;

                                do
                                {
                                    len = tcpStream.Read(buf, 0, buf.Length);
                                    if (buf[len - 1] == 0)
                                    {
                                        m.Write(buf, 0, len - 1);
                                        break;
                                    }
                                    else
                                    {
                                        m.Write(buf, 0, len);
                                    }
                                } while (true);

                                m.Position = 0;

                                using (StreamReader sr = new StreamReader(m, Encoding.UTF8))
                                {
                                    msg = sr.ReadToEnd();
                                }
                            }

                        }
                           
                        MessageReceiveEventArgs receiveEvent = new MessageReceiveEventArgs(msgHead, msg, p.ThreadId);

                        Hubble.Framework.Serialization.IMySerialization customSerializer = null;

                        try
                        {
                            OnMessageReceiveEvent(receiveEvent);
                            customSerializer = receiveEvent.CustomSerializtion;
                        }
                        catch (Exception e)
                        {
                            receiveEvent.ReturnMsg = e;
                        }

                        ReturnMessage(clientStream, msgHead, receiveEvent.ReturnMsg, customSerializer);

                    }
                    catch (Exception innerException)
                    {
                        try
                        {
                            OnMessageReceiveErrorEvent(new MessageReceiveErrorEventArgs(msgHead, innerException));
                        }
                        catch
                        {
                        }

                        if (tcpClient.Connected)
                        {
                            tcpClient.Close();
                        }

                        throw innerException;
                    }
                }

                tcpClient.Close();
            }
            catch (Exception e)
            {
                OnMessageReceiveErrorEvent(new MessageReceiveErrorEventArgs(e));
            }
            finally
            {
                try
                {
                    OnDisconnectEvent(new DisconnectEventArgs(p.ThreadId));
                }
                catch (Exception e)
                {
                    OnMessageReceiveErrorEvent(new MessageReceiveErrorEventArgs(e));
                }

                RetPCB(p);
            }
        }


        #endregion

        #region Public Methods

        public TcpServer()
        {
        }

        public TcpServer(int port)
        {
            _Port = port;
        }

        public TcpServer(IPAddress localAddress, int port)
        {
            _Port = port;
            _LocalAddress = localAddress;
        }

        public void Listen()
        {
            InitThreadPool();

            if (LocalAddress != null)
            {
                IPEndPoint serverEndPoint = new IPEndPoint(LocalAddress, Port);

                this._TcpListener = new System.Net.Sockets.TcpListener(serverEndPoint);
            }
            else
            {
                this._TcpListener = new System.Net.Sockets.TcpListener(IPAddress.Any, Port);
            }
            
            this._ListenThread = new Thread(new ThreadStart(ListenForClients));

            this._ListenThread.Start();
        }

        #endregion


    }
}
