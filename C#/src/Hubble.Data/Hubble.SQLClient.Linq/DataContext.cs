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
using System.Linq;
using System.Text;
using Hubble.SQLClient;

namespace Hubble.SQLClient.Linq
{
    public abstract class DataContext : IDisposable
    {
        #region Private fields
        private HubbleConnection _Conn;

        private int _CacheTimeout = -1;

        private bool _Disposed = false;

        #endregion

        #region Protected properties
     

        #endregion 

        #region Public properties

        /// <summary>
        /// Get hubble Conection
        /// </summary>
        public HubbleConnection HBConnection
        {
            get
            {
                return _Conn;
            }
        }

        #endregion

        #region Constractor
        public DataContext(HubbleConnection conn)
        {
            if (conn == null)
            {
                throw new ArgumentException("Conn can't be NULL.");
            }

            _Conn = conn;

            if (!_Conn.Connected)
            {
                _Conn.Open();
            }
        }

        ~DataContext()
        {
            Dispose();
        }


        #endregion

        #region Protected methods

        #endregion

        #region Public methods

        /// <summary>
        /// Close connection.
        /// </summary>
        public void Close()
        {
            _Conn.Close();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_Disposed)
            {
                _Disposed = true;

                try
                {
                    Close();
                }
                catch
                {
                }

                try
                {
                    _Conn.Dispose();
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
