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

namespace Hubble.Framework.Threading
{
    public class MessageQueue
    {
        public delegate object OnMessage(int evt, MessageFlag flag, object data);
        public delegate void OnError(Exception e);

        #region data structures
        
        /// <summary>
        /// Message flag
        /// </summary>
        [Flags]
        public enum MessageFlag
        {
            None = 0x0,
            SyncMsg = 0x01,
            Urgent  = 0x02,
        }

        /// <summary>
        /// Message package
        /// </summary>
        private class Message : IDisposable
        {
            #region Private fields

            private int _Event;
            private MessageFlag _Flag;
            private object _Data;
            private object _RetData;
            private System.Threading.ManualResetEvent _SSendEvent;

            #endregion

            #region Public properties

            /// <summary>
            /// Event number for the message
            /// </summary>
            public int Event
            {
                get
                {
                    return _Event;
                }

                set
                {
                    _Event = value;
                }
            }

            /// <summary>
            /// Message flag
            /// </summary>
            public MessageFlag Flag
            {
                get
                {
                    return _Flag;
                }

                set
                {
                    _Flag = value;
                }
            }

            /// <summary>
            /// Data which sender send to
            /// </summary>
            public object Data
            {
                get
                {
                    return _Data;
                }

                set
                {
                    _Data = value;
                }
            }

            /// <summary>
            /// Data which receiver return
            /// </summary>
            public object RetData
            {
                get
                {
                    return _RetData;
                }

                set
                {
                    _RetData = value;
                }
            }

            #endregion

            #region Constructor
            public Message(int evt, MessageFlag flag, object data)
            {
                Event = evt;
                Flag = flag;
                Data = data;
                _SSendEvent = null;
                RetData = null;
            }

            public Message(int evt, MessageFlag flag, object data, System.Threading.ManualResetEvent ssendEvent)
            {
                Event = evt;
                Flag = flag;
                Data = data;
                _SSendEvent = ssendEvent;
                RetData = null;
            }

            ~Message()
            {
                try
                {
                    Dispose();
                }
                catch
                {
                }
            }
            #endregion

            #region Public methods

            /// <summary>
            /// Set event process the message
            /// This method only uses for Sync Send
            /// </summary>
            public void SetSSendEvent()
            {
                lock (this)
                {
                    if (_SSendEvent != null)
                    {
                        _SSendEvent.Set();
                    }
                }
            }

            /// <summary>
            /// After send a Sync message, wait for this event until
            /// receiver have processed the message.
            /// </summary>
            /// <param name="millisecondsTimeout"></param>
            /// <returns></returns>
            public bool WaitOne(int millisecondsTimeout)
            {
                if (_SSendEvent != null)
                {
                    return _SSendEvent.WaitOne(millisecondsTimeout, true);
                }
                else
                {
                    throw new ArgumentNullException("SSendEvent is null!");
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                lock (this)
                {
                    if (_SSendEvent != null)
                    {
                        _SSendEvent.Close();
                        _SSendEvent = null;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Private fields

        private System.Threading.Thread _Thread;

        private Queue<Message> _Queue = new Queue<Message>();
        private Queue<Message> _UrgentQueue = new Queue<Message>();

        private OnMessage _OnMessageEvent;
        private OnError _OnError;
        private System.Threading.Semaphore _Sema;

        private bool _Closing = false;
        private bool Closing
        {
            get
            {
                lock (this)
                {
                    return _Closing;
                }
            }

            set
            {
                lock (this)
                {
                    _Closing = value;
                }
            }
        }

        protected OnMessage OnMessageEvent
        {
            get
            {
                return _OnMessageEvent;
            }

            set
            {
                _OnMessageEvent = value;
            }
        }

        private System.Threading.ManualResetEvent _CloseEvent = new System.Threading.ManualResetEvent(false);

        #endregion

        #region Public properties

        /// <summary>
        /// call back when error raise
        /// </summary>
        public OnError OnErrorEvent
        {
            get
            {
                return _OnError;
            }

            set
            {
                _OnError = value;                    
            }
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Process message
        /// </summary>
        private void MessageProc()
        {
            try
            {
                while (true)
                {
                    _Sema.WaitOne();

                    if (Closing)
                    {
                        try
                        {
                            bool close = true;

                            lock (this)
                            {
                                if (_UrgentQueue.Count > 0 || _Queue.Count > 0)
                                {
                                    close = false;
                                }
                            }

                            if (close)
                            {
                                _CloseEvent.Set();
                                return;
                            }
                        }
                        catch
                        {
                        }
                    }

                    Message msg = null;

                    try
                    {
                        lock (this)
                        {
                            if (_UrgentQueue.Count > 0)
                            {
                                msg = _UrgentQueue.Dequeue();
                            }
                            else
                            {
                                msg = _Queue.Dequeue();
                            }
                        }

                        if (_OnMessageEvent != null)
                        {
                            msg.RetData = _OnMessageEvent(msg.Event, msg.Flag, msg.Data);
                        }

                    }
                    catch (Exception e)
                    {
                        if (OnErrorEvent != null)
                        {
                            try
                            {
                                msg.RetData = e;
                                OnErrorEvent(e);
                            }
                            catch
                            {
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            if (msg != null)
                            {
                                if ((msg.Flag & MessageFlag.SyncMsg) != 0)
                                {
                                    //Sync msg
                                    msg.SetSSendEvent();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                OnErrorEvent(e);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Constructor

        public MessageQueue()
        {
            _Sema = new System.Threading.Semaphore(0, int.MaxValue);
            _Thread = new System.Threading.Thread(new System.Threading.ThreadStart(MessageProc));
            _Thread.IsBackground = true;
        }

        public MessageQueue(OnMessage onMessageEvent)
            :this()
        {
            if (onMessageEvent == null)
            {
                throw new ArgumentNullException();
            }

            _OnMessageEvent = onMessageEvent;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Start to receive message
        /// </summary>
        public void Start()
        {
            _Thread.Start();
        }

        /// <summary>
        /// Close message queue
        /// Exit thread
        /// </summary>
        /// <param name="millisecondsTimeout">wait timeout for close</param>
        public bool Close(int millisecondsTimeout)
        {
            Closing = true;

            try
            {
                _Sema.Release();

                if (!_CloseEvent.WaitOne(millisecondsTimeout, true))
                {
                    _Thread.Abort();
                }
                else
                {
                    return true;
                }
            }
            catch
            {
            }
            finally
            {
                try
                {
                    _CloseEvent.Close();
                }
                catch
                {
                }

                try
                {
                    _Sema.Close();
                }
                catch
                {
                }

                _OnMessageEvent = null;
                _OnError = null;
            }

            return false;
        }

        /// <summary>
        /// Send asynchronous message from another thread
        /// </summary>
        /// <param name="evt">Message event number</param>
        /// <param name="data">Message data</param>
        public void ASendMessage(int evt, object data)
        {
            if (Closing)
            {
                return;
                //throw new Exception("MessageQueue is closing");
            }

            Message msg = new Message(evt, MessageFlag.None, data);

            lock(this)
            {
                _Queue.Enqueue(msg);
                _Sema.Release();
            }
        }

        /// <summary>
        /// Send urgent asynchronous message from another thread
        /// </summary>
        /// <param name="evt">Message event number</param>
        /// <param name="data">Message data</param>
        public void ASendUrgentMessage(int evt, object data)
        {
            if (Closing)
            {
                return;
                //throw new Exception("MessageQueue is closing");
            }


            Message msg = new Message(evt, MessageFlag.Urgent, data);

            lock(this)
            {
                _UrgentQueue.Enqueue(msg);
                _Sema.Release();
            }
        }

        /// <summary>
        /// Send synchronous message from another thread
        /// </summary>
        /// <param name="evt">Message event number</param>
        /// <param name="data">Message data</param>
        /// <param name="millisecondsTimeout">timeout that wait for message process</param>
        /// <returns>Recevers return data</returns>
        public object SSendMessage(int evt, object data, int millisecondsTimeout)
        {
            if (Closing)
            {
                throw new Exception("MessageQueue is closing");
            }

            using (Message msg = new Message(evt, MessageFlag.SyncMsg, data, new System.Threading.ManualResetEvent(false)))
            {
                lock (this)
                {
                    _Queue.Enqueue(msg);
                    _Sema.Release();
                }

                if (msg.WaitOne(millisecondsTimeout))
                {
                    if (msg.RetData is Exception)
                    {
                        throw new Exception((msg.RetData as Exception).Message, msg.RetData as Exception);
                    }

                    return msg.RetData;
                }
                else
                {
                    throw new System.TimeoutException("SSend timeout");
                }
            }
        }

        /// <summary>
        /// Send urgent synchronous message from another thread
        /// </summary>
        /// <param name="evt">Message event number</param>
        /// <param name="data">Message data</param>
        /// <param name="millisecondsTimeout">timeout that wait for message process</param>
        /// <returns>Recevers return data</returns>
        public object SSendUrgentMessage(int evt, object data, int millisecondsTimeout)
        {
            if (Closing)
            {
                throw new Exception("MessageQueue is closing");
            }

            using (Message msg = new Message(evt, MessageFlag.SyncMsg | MessageFlag.Urgent, data, new System.Threading.ManualResetEvent(false)))
            {
                lock (this)
                {
                    _UrgentQueue.Enqueue(msg);
                    _Sema.Release();
                }

                if (msg.WaitOne(millisecondsTimeout))
                {
                    if (msg.RetData is Exception)
                    {
                        throw msg.RetData as Exception;
                    }

                    return msg.RetData;
                }
                else
                {
                    throw new System.TimeoutException("SSend timeout");
                }
            }
        }

        #endregion
    }
}
