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
