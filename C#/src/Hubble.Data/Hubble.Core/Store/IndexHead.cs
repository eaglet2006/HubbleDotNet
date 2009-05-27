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

namespace Hubble.Core.Store
{
    [Serializable]
    public class IndexHead
    {
        #region Private fields

        private readonly string _Copyright = "Copy right: Hubble.net Bo Xiao 2008-2009";
        private string _Version;
        private string _FieldName;
        private int _SegmentSize = 2048;
        private int _ReserveSegments = 512;
        private int _AutoIncreaseBytes = 10 * 1024 * 1024;
        #endregion

        #region Public properties

        /// <summary>
        /// Copy right
        /// </summary>
        public string Copyright
        {
            get
            {
                return _Copyright;
            }
        }

        /// <summary>
        /// Hubble.net version
        /// </summary>
        public string Version
        {
            get
            {
                if (_Version == null)
                {
                    _Version = System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString();
                }

                return _Version;
            }

            set
            {
                _Version = value;
            }
        }

        /// <summary>
        /// Field name
        /// </summary>
        public string FieldName
        {
            get
            {
                return _FieldName;
            }

            set
            {
                _FieldName = value;
            }
        }

        /// <summary>
        /// Size of segment
        /// </summary>
        public int SegmentSize
        {
            get
            {
                return _SegmentSize;
            }

            set
            {
                if (value < 2048)
                {
                    _SegmentSize = 2048;
                }
                else
                {
                    _SegmentSize = value;
                }
            }
        }

        /// <summary>
        /// Segment 0 reserves for index head
        /// Segment 1 to ReserveSegments - 1 reserves for word table 
        /// </summary>
        public int ReserveSegments
        {
            get
            {
                return _ReserveSegments;
            }

            set
            {
                if (value <= 1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _ReserveSegments = value;
            }
        }

        /// <summary>
        /// Increase file size automaticly when accesses this file overflow.
        /// </summary>
        public int AutoIncreaseBytes
        {
            get
            {
                return _AutoIncreaseBytes;
            }

            set
            {
                if (value < 1 * 1024 * 1024)
                {
                    value = 1 * 1024 * 1024;
                }

                if (value % SegmentSize != 0)
                {
                    throw new ArgumentException("AutoIncreaseBytes must be times of SegmentSize");
                }
                
                _AutoIncreaseBytes = value;
            }
        }

        #endregion
    }
}
