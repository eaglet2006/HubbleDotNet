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
using System.Xml;

using Hubble.Core.Global;
using Hubble.Framework.IO;
using Hubble.Framework.Serialization;

namespace Hubble.Core.Data
{
    /// <summary>
    /// File for insert protect
    /// </summary>
    public class InsertProtect
    {
        private static object _LockObj = new object();

        [System.Xml.Serialization.XmlIgnore]
        internal const string FileName = "iprotect.xml";

        static InsertProtect _InsertProtect;

        [System.Xml.Serialization.XmlIgnore]
        static internal InsertProtect InsertProtectInfo
        {
            get
            {
                return _InsertProtect;
            }
             
            set
            {
                _InsertProtect = value;
            }
        }


        private bool _IndexOnly = true;

        public bool IndexOnly
        {
            get
            {
                return _IndexOnly;
            }

            set
            {
                _IndexOnly = value;
            }
        }


        private int _LastDocId = Int16.MaxValue;

        public int LastDocId
        {
            get
            {
                return _LastDocId;
            }

            set
            {
                _LastDocId = value;
            }
        }

        private int _DocumentsCount = 0;

        public int DocumentsCount
        {
            get
            {
                return _DocumentsCount;
            }

            set
            {
                _DocumentsCount = value;
            }
        }

        private List<string> _IndexFiles = new List<string>();

        public List<string> IndexFiles
        {
            get
            {
                return _IndexFiles;
            }

            set
            {
                _IndexFiles = value;
            }
        }

        static internal bool Load(string filePath)
        {
            lock (_LockObj)
            {
                string fileName = Path.AppendDivision(filePath, '\\') + FileName;

                if (System.IO.File.Exists(fileName))
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open,
                         System.IO.FileAccess.Read))
                    {
                        _InsertProtect = XmlSerialization<InsertProtect>.Deserialize(fs);
                    }

                    return true;
                }
                else
                {
                    _InsertProtect = null;
                    return false;
                }
            }
        }

        static internal void Remove(string filePath)
        {
            lock (_LockObj)
            {
                string fileName = Path.AppendDivision(filePath, '\\') + FileName;

                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
            }
        }

        static internal void Save(string filePath)
        {
            lock (_LockObj)
            {
                if (_InsertProtect == null)
                {
                    return;
                }

                string fileName = Path.AppendDivision(filePath, '\\') + FileName;

                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create,
                     System.IO.FileAccess.ReadWrite))
                {
                    XmlSerialization<InsertProtect>.Serialize(_InsertProtect, Encoding.UTF8, fs);
                }
            }
        }
    }
}
