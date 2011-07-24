using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hubble.Framework.Net
{
    public class TcpClient
    {
        #region Private fields
        private System.Net.Sockets.TcpClient _Client = null;
        private System.Net.Sockets.NetworkStream _ClientStream;

        private IPAddress _RemoteAddress = IPAddress.Parse("127.0.0.1");
        private int _Port = 8800;

        private bool _Async = false;

        private object _ThreadClosedLock = new object();
        private bool _ThreadClosed = false;

        private bool ThreadClosed
        {
            get
            {
                lock (_ThreadClosedLock)
                {
                    return _ThreadClosed;
                }
            }

            set
            {
                lock (_ThreadClosedLock)
                {
                    _ThreadClosed = value;
                }
            }
        }

        private Thread _Thread = null;

        RequireCustomSerializationDelegate _RequireCustomSerialization;


        #endregion

        #region Public properties



        public AsyncMessageRecievedDelegate AsyncMessageRecieved
        {
            get
            {
                return _AsyncMessageRecieved;
            }

            set
            {
                _AsyncMessageRecieved = value;
            }
        }

        /// <summary>
        /// An IPAddress that request the remote IP address.
        /// </summary>
        public IPAddress RemoteAddress
        {
            get
            {
                return _RemoteAddress;
            }

            set
            {
                _RemoteAddress = value;
            }
        }

        /// <summary>
        /// The port on which to connect to the remote server. 
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
        /// The time-out value of the connection in milliseconds. The default value is 0.
        /// </summary>
        /// <remarks>
        /// The ReceiveTimeout property determines the amount of time that the Read method will block until it is able to receive data. This time is measured in milliseconds. If the time-out expires before Read successfully completes, TcpClient throws a IOException. There is no time-out by default.
        /// </remarks>
        public int ReceiveTimeout
        {
            get
            {
                return _Client.ReceiveTimeout;
            }

            set
            {
                _Client.ReceiveTimeout = value;
            }
        }

        /// <summary>
        /// The send time-out value, in milliseconds. The default is 0.
        /// </summary>
        /// <remarks>
        /// The SendTimeout property determines the amount of time that the Send method will block until it is able to return successfully. This time is measured in milliseconds.
        /// After you call the Write method, the underlying Socket returns the number of bytes actually sent to the host. The SendTimeout property determines the amount of time a TcpClient will wait before receiving the number of bytes returned. If the time-out expires before the Send method successfully completes, TcpClient will throw a SocketException. There is no time-out by default.
        /// </remarks>
        public int SendTimeout
        {
            get
            {
                return _Client.SendTimeout;
            }

            set
            {
                _Client.SendTimeout = value;
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

        public bool Closed
        {
            get
            {
                if (_Client != null)
                {
                    return !_Client.Connected;
                }
                else
                {
                    return true;
                }
            }
        }

        #endregion

        #region Delegates

        public delegate Hubble.Framework.Serialization.IMySerialization RequireCustomSerializationDelegate(Int16 evt, object data);

        AsyncMessageRecievedDelegate _AsyncMessageRecieved;
        public delegate void AsyncMessageRecievedDelegate(ASyncPackage package);

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

        #region Public methods

        public TcpClient(bool async)
        {
            _Client = new System.Net.Sockets.TcpClient();
            _Async = async;
        }

        public TcpClient()
        {
            _Client = new System.Net.Sockets.TcpClient();
        }

        ~TcpClient()
        {
            Dispose();
        }



        public void Connect(IPAddress IPAddress, int port)
        {
            RemoteAddress = IPAddress;
            Port = port;
            Connect();
        }

        /// <summary>
        /// Connect with timeout
        /// </summary>
        /// <param name="connectTimeout">connect timeout. In millisecond</param>
        public void Connect(int connectTimeout)
        {
            if (connectTimeout > 30000)
            {
                Connect();
                return;
            }

            if (_Client == null)
            {
                _Client = new System.Net.Sockets.TcpClient();
            }

            IPEndPoint serverEndPoint =
               new IPEndPoint(RemoteAddress, Port);

            IAsyncResult ar = _Client.BeginConnect(serverEndPoint.Address, 
                serverEndPoint.Port, null, null);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;

            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(connectTimeout, false))
                {
                    _Client.Close();
                    throw new TimeoutException(string.Format("Try to connect {0} timeout", serverEndPoint));
                }

                _Client.EndConnect(ar);
            }
            finally
            {
                wh.Close();
            } 

            _ClientStream = _Client.GetStream();

            if (_Async)
            {
                _Thread = new Thread(AsyncMessageRecv);
                _Thread.IsBackground = true;
                _Thread.Start();
            }
        }

        public void Connect()
        {
            if (_Client == null)
            {
                _Client = new System.Net.Sockets.TcpClient();
            }

            IPEndPoint serverEndPoint =
               new IPEndPoint(RemoteAddress, Port);

            _Client.Connect(serverEndPoint);

            _ClientStream = _Client.GetStream();

            if (_Async)
            {
                _Thread = new Thread(AsyncMessageRecv);
                _Thread.IsBackground = true;
                _Thread.Start();
            }
        }

        /// <summary>
        /// Set connection mode to asynchronous connection
        /// </summary>
        public void SetToAsync()
        {
            if (!_Async)
            {
                _Async = true;

                if (_Thread != null)
                {
                    _Thread.Abort();
                }

                _Thread = new Thread(AsyncMessageRecv);
                _Thread.IsBackground = true;
                _Thread.Start();
            }
        }

        public void Close()
        {
            Dispose();
        }

        private void AsyncMessageRecv()
        {
            TcpCacheStream tcpStream = new TcpCacheStream(_ClientStream);

            while (true)
            {
                try
                {
                    object result;

                    //Recevie data

                    byte[] revBuf = new byte[8];
                    int offset = 0;

                    while (offset < 8)
                    {
                        int len = tcpStream.Read(revBuf, offset, 8 - offset);

                        if (len == 0)
                        {
                            throw new Exception("Tcp stream closed!");
                        }

                        offset += len;
                    }

                    MessageHead head = new MessageHead();

                    head.Event = BitConverter.ToInt16(revBuf, 0);
                    head.Flag = (MessageFlag)BitConverter.ToInt16(revBuf, 2);

                    int classId = BitConverter.ToInt32(revBuf, 4);

                    if ((head.Flag & MessageFlag.NullData) == 0)
                    {
                        if ((head.Flag & MessageFlag.CustomSerialization) != 0)
                        {
                            if (RequireCustomSerialization != null)
                            {
                                Hubble.Framework.Serialization.IMySerialization mySerializer =
                                    RequireCustomSerialization(head.Event, null);

                                if (mySerializer == null)
                                {
                                    throw new Exception(string.Format("RequireCustomSerialization of Event = {0} is null!",
                                        head.Event));
                                }

                                result = mySerializer.Deserialize(tcpStream, mySerializer.Version);
                            }
                            else
                            {
                                throw new Exception("RequireCustomSerialization of TcpClient is null!");
                            }
                        }
                        else if ((head.Flag & MessageFlag.IsString) == 0)
                        {
                            IFormatter formatter = new BinaryFormatter();
                            result = formatter.Deserialize(tcpStream);
                        }
                        else
                        {
                            MemoryStream m = new MemoryStream();

                            byte b = (byte)tcpStream.ReadByte();
                            while (b != 0)
                            {
                                m.WriteByte(b);
                                b = (byte)tcpStream.ReadByte();
                            }

                            m.Position = 0;

                            using (StreamReader sr = new StreamReader(m, Encoding.UTF8))
                            {
                                result = sr.ReadToEnd();
                            }

                            if ((head.Flag & MessageFlag.IsException) != 0)
                            {
                                string[] strs = Hubble.Framework.Text.Regx.Split(result as string, "innerStackTrace:");

                                result = new InnerServerException(strs[0], strs[1]);
                            }
                        }

                        if (AsyncMessageRecieved != null)
                        {
                            AsyncMessageRecieved(new ASyncPackage(classId, result));
                        }
                    }
                }
                catch(Exception e)
                {
                    lock (this)
                    {
                        Close();
                    }

                    ThreadClosed = true;

                    if (AsyncMessageRecieved != null)
                    {
                        AsyncMessageRecieved(new ASyncPackage(-1, new TcpRemoteCloseException(e.Message)));
                    }
                    return;
                }
            }
        }

        public void SendASyncMessage(short evt, ref ASyncPackage package)
        {
            SendASyncMessage(evt, ref package, false);
        }

        public void SendASyncMessage(short evt, ref ASyncPackage package, bool priorMessage)
        {
            //if (_Client == null)
            //{
            //    Connect();
            //}
            //else
            //{
            //    lock (this)
            //    {
            //        if (!_Client.Connected)
            //        {
            //            Close();
            //            Connect();
            //        }
            //    }
            //}

            try
            {
                lock (this)
                {
                    object msg = package.Message;

                    TcpCacheStream tcpStream = new TcpCacheStream(_ClientStream);

                    MessageHead head = new MessageHead(evt);
                    
                    head.Flag |= MessageFlag.ASyncMessage;

                    if (priorMessage)
                    {
                        head.Flag |= MessageFlag.Prior;
                    }

                    if (msg == null)
                    {
                        head.Flag |= MessageFlag.NullData;
                    }

                    //Send event
                    byte[] sendBuf = BitConverter.GetBytes(head.Event);
                    tcpStream.Write(sendBuf, 0, sendBuf.Length);

                    if (msg != null)
                    {
                        //Send Flag

                        if (msg is string)
                        {
                            head.Flag |= MessageFlag.IsString;
                            sendBuf = BitConverter.GetBytes((short)head.Flag);
                            tcpStream.Write(sendBuf, 0, sendBuf.Length);

                            sendBuf = BitConverter.GetBytes(package.ClassId);
                            tcpStream.Write(sendBuf, 0, sendBuf.Length);

                            byte[] buf = Encoding.UTF8.GetBytes((msg as string));

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
                        else
                        {
                            sendBuf = BitConverter.GetBytes((short)head.Flag);
                            tcpStream.Write(sendBuf, 0, sendBuf.Length);
                            sendBuf = BitConverter.GetBytes(package.ClassId);
                            tcpStream.Write(sendBuf, 0, sendBuf.Length);

                            IFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(tcpStream, msg);
                        }
                    }
                    else
                    {
                        //Send Flag
                        sendBuf = BitConverter.GetBytes((short)head.Flag);
                        tcpStream.Write(sendBuf, 0, sendBuf.Length);
                        sendBuf = BitConverter.GetBytes(package.ClassId);
                        tcpStream.Write(sendBuf, 0, sendBuf.Length);
                    }

                    tcpStream.Flush();
                }
            }
            catch (System.IO.IOException ioEx)
            {
                Close();
                throw ioEx;
            }

        }

        /// <summary>
        /// Send synchronous message
        /// </summary>
        /// <param name="evt">Event</param>
        /// <param name="msg">message</param>
        /// <param name="milliseconds">time out</param>     
        /// <returns>response object. If no reponse from server, return null</returns>
        public object SendSyncMessage(short evt, object msg)
        {
            if (_ClientStream == null)
            {
                Connect();
            }

            try
            {

                lock (this)
                {

                    TcpCacheStream tcpStream = new TcpCacheStream(_ClientStream);

                    object result = null;

                    MessageHead head = new MessageHead(evt);

                    if (msg == null)
                    {
                        head.Flag |= MessageFlag.NullData;
                    }

                    //Send event
                    byte[] sendBuf = BitConverter.GetBytes(head.Event);
                    tcpStream.Write(sendBuf, 0, sendBuf.Length);

                    if (msg != null)
                    {
                        //Send Flag

                        if (msg is string)
                        {
                            head.Flag |= MessageFlag.IsString;
                            sendBuf = BitConverter.GetBytes((short)head.Flag);
                            tcpStream.Write(sendBuf, 0, sendBuf.Length);

                            byte[] buf = Encoding.UTF8.GetBytes((msg as string));

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
                        else
                        {
                            sendBuf = BitConverter.GetBytes((short)head.Flag);
                            tcpStream.Write(sendBuf, 0, sendBuf.Length);
                            IFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(tcpStream, msg);
                        }
                    }
                    else
                    {
                        //Send Flag
                        sendBuf = BitConverter.GetBytes((short)head.Flag);
                        tcpStream.Write(sendBuf, 0, sendBuf.Length);
                    }

                    tcpStream.Flush();

                    tcpStream = new TcpCacheStream(_ClientStream);

                    //Recevie data

                    byte[] revBuf = new byte[4];
                    int offset = 0;

                    while (offset < 4)
                    {
                        int len = tcpStream.Read(revBuf, offset, 4 - offset);

                        if (len == 0)
                        {
                            throw new Exception("Tcp stream closed!");
                        }


                        offset += len;
                    }

                    head.Event = BitConverter.ToInt16(revBuf, 0);
                    head.Flag = (MessageFlag)BitConverter.ToInt16(revBuf, 2);

                    if ((head.Flag & MessageFlag.NullData) == 0)
                    {
                        if ((head.Flag & MessageFlag.CustomSerialization) != 0)
                        {
                            if (RequireCustomSerialization != null)
                            {
                                Hubble.Framework.Serialization.IMySerialization mySerializer =
                                    RequireCustomSerialization(head.Event, null);

                                if (mySerializer == null)
                                {
                                    throw new Exception(string.Format("RequireCustomSerialization of Event = {0} is null!",
                                        head.Event));
                                }

                                result = mySerializer.Deserialize(tcpStream, mySerializer.Version);
                            }
                            else
                            {
                                throw new Exception("RequireCustomSerialization of TcpClient is null!");
                            }
                        }
                        else if ((head.Flag & MessageFlag.IsString) == 0)
                        {
                            IFormatter formatter = new BinaryFormatter();
                            result = formatter.Deserialize(tcpStream);
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
                                result = sr.ReadToEnd();
                            }

                            if ((head.Flag & MessageFlag.IsException) != 0)
                            {
                                string[] strs = Hubble.Framework.Text.Regx.Split(result as string, "innerStackTrace:");

                                result = new InnerServerException(strs[0], strs[1]);

                            }
                        }

                        if (result is InnerServerException)
                        {
                            throw new ServerException(result as InnerServerException);
                        }
                        else if (result is Exception)
                        {
                            throw result as Exception;
                        }
                    }

                    return result;
                }
            }
            catch (System.IO.IOException ioEx)
            {
                Close();
                throw ioEx;
            }


        }


        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (_ClientStream != null)
                {
                    _ClientStream.Close();
                }

                if (_Client != null)
                {
                    _Client.Close();
                }

                if (_Async)
                {
                    if (_Thread != null)
                    {
                        if (!ThreadClosed)
                        {
                            _Thread.Abort();
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                _ClientStream = null;
                _Client = null;
                _Thread = null;
            }
        }

        #endregion

    }
}
