using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Data
{
    public class Table
    {
        #region Private field
        
        string _Name;

        string _ConnectionString = "Data Source=(local);Initial Catalog=Test;Integrated Security=True";

        string _DBTableName;

        List<Field> _Fields = new List<Field>();

        DBAdapter.IDBAdapter _DBAdapter;

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

        /// <summary>
        /// Database adapter
        /// </summary>
        public DBAdapter.IDBAdapter DBAdapter
        {
            get
            {
                return _DBAdapter;
            }

            set
            {
                _DBAdapter = value;

                if (_DBAdapter != null)
                {
                    _DBAdapter.Table = this;
                }
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

        public void Create()
        {
            if (DBAdapter != null)
            {
                DBAdapter.Create();
            }
        }

        #endregion

    }
}
