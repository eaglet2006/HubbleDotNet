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
using Hubble.Framework.IO;
using Hubble.Framework.Serialization;

namespace Hubble.Core.Data
{
    
    [Serializable, System.Xml.Serialization.XmlRoot(Namespace = "http://www.hubble.net")]
    public class Table
    {
        #region Private field
        
        string _Name;

        string _ConnectionString = null;

        string _DBTableName;

        List<Field> _Fields = new List<Field>();

        string _DBAdapterTypeName = null; //eg. SqlServer2005Adapter 

        string _SQLForCreate;

        int _ForceCollectCount = 5000;

        int _MaxReturnCount = 2 * 1024 * 1024;

        bool _IndexOnly = false;

        private string _DocIdReplaceField = null;

        bool _QueryCacheEnabled = true;

        int _QueryCacheTimeout = 0; //In seconds

        bool _StoreQueryCacheInFile = true;

        int _CleanupQueryCacheFileInDays = 7;

        bool _InitImmediatelyAfterStartup = Global.Setting.Config.InitTablesStartup;

        int _IndexThread = 1;

        #endregion

        #region Public properties

        /// <summary>
        /// This table is index only or not
        /// </summary>
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

        /// <summary>
        /// This field replace docid field connecting with database. 
        /// It is only actively in index only mode.
        /// </summary>
        public string DocIdReplaceField
        {
            get
            {
                return _DocIdReplaceField;
            }

            set
            {
                _DocIdReplaceField = value;
            }
        }

        /// <summary>
        /// Table name
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }

        /// <summary>
        /// ConnectionString of database (eg. SQLSERVER)
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }

            set
            {
                _ConnectionString = value;
            }
        }

        /// <summary>
        /// Table name of database (eg. SQLSERVER)
        /// </summary>
        public string DBTableName
        {
            get
            {
                return _DBTableName;
            }

            set
            {
                _DBTableName = value;
            }
        }

        /// <summary>
        /// Fields of this table
        /// </summary>
        public List<Field> Fields
        {
            get
            {
                return _Fields;
            }
        }

        public string DBAdapterTypeName
        {
            get
            {
                return _DBAdapterTypeName;
            }

            set
            {
                _DBAdapterTypeName = value;
            }
        }

        public string SQLForCreate
        {
            get
            {
                return _SQLForCreate;
            }

            set
            {
                _SQLForCreate = value;
            }
        }



        public int ForceCollectCount
        {
            get
            {
                return _ForceCollectCount;
            }

            set
            {
                if (value <= 0)
                {
                    _ForceCollectCount = 1;
                }
                else
                {
                    _ForceCollectCount = value;
                }
            }
        }

        /// <summary>
        /// max count when the doc id list is too many to list.
        /// </summary>
        public int MaxReturnCount
        {
            get
            {
                return _MaxReturnCount;
            }

            set
            {
                if (value <= 0)
                {
                    _MaxReturnCount = 2 * 1024 * 1024;
                }
                else
                {
                    _MaxReturnCount = value;
                }
            }
        }


        public bool QueryCacheEnabled
        {
            get
            {
                return _QueryCacheEnabled;
            }

            set
            {
                _QueryCacheEnabled = value;
            }
        }

        public int QueryCacheTimeout
        {
            get
            {
                return _QueryCacheTimeout;
            }

            set
            {
                _QueryCacheTimeout = value;
            }
        }


        public bool StoreQueryCacheInFile
        {
            get
            {
                return _StoreQueryCacheInFile;
            }

            set
            {
                _StoreQueryCacheInFile = value;
            }
        }

        public int CleanupQueryCacheFileInDays
        {
            get
            {
                return _CleanupQueryCacheFileInDays;
            }

            set
            {
                if (value < 0)
                {
                    _CleanupQueryCacheFileInDays = 0;
                }
                else
                {
                    _CleanupQueryCacheFileInDays = value;
                }
            }
        }

        /// <summary>
        /// Initinalize immediately after hubble.net startup.
        /// </summary>
        public bool InitImmediatelyAfterStartup
        {
            get
            {
                return _InitImmediatelyAfterStartup;
            }

            set
            {
                _InitImmediatelyAfterStartup = value;
            } 
        }

        /// <summary>
        /// How many threads used for index
        /// </summary>
        public int IndexThread
        {
            get
            {
                return _IndexThread;
            }

            set
            {
                _IndexThread = value;
            } 
        }

        private static void TableCheck(Table table)
        {
            foreach (Field field in table.Fields)
            {
                if (field.IndexType == Field.Index.Untokenized)
                {
                    switch (field.DataType)
                    {
                        case DataType.TinyInt:
                        case DataType.SmallInt:
                            if (field.SubTabIndex < 0)
                            {
                                throw new DataException(string.Format("SubTabIndex of field:{0} less then zero, the payload file is for old version, please truncate table and rebuild it!",
                                    field.Name));
                            }
                            break;
                    }
                }
            }
        }
        

        public void Save(string dir)
        {
            dir = Path.AppendDivision(dir, '\\');

            string fileName = dir + "tableinfo.xml";

            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create,
                 System.IO.FileAccess.ReadWrite))
            {
                XmlSerialization<Table>.Serialize(this, Encoding.UTF8, fs);
            }            
        }

        public static Table Load(string dir)
        {
            dir = Path.AppendDivision(dir, '\\');

            string fileName = dir + "tableinfo.xml";

            Table table;

            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open,
                 System.IO.FileAccess.Read))
            {
                table = XmlSerialization<Table>.Deserialize(fs);
            }

            TableCheck(table);

            return table;
        }

        #endregion

    }
}
