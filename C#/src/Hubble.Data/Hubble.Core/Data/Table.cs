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

        string _ConnectionString = "Data Source=(local);Initial Catalog=Test;Integrated Security=True";

        string _DBTableName;

        List<Field> _Fields = new List<Field>();

        string _DBAdapterTypeName; //eg. SqlServer2005Adapter 

        string _SQLForCreate;

        int _ForceCollectCount = 5000;

        #endregion

        #region Public properties

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

            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open,
                 System.IO.FileAccess.Read))
            {
                return XmlSerialization<Table>.Deserialize(fs);
            }
        }

        #endregion

    }
}
