using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Data
{
    public class DBAccess : IDisposable
    {
        private DBProvider _DBInsertProvider = null;
        private string _LastTableName = null;

        private string _Host = null;

        public string Host
        {
            get
            {
                return _Host;
            }

            set
            {
                _Host = value;
            }
        }

        ~DBAccess()
        {
            Dispose();
        }

        public void CreateTable(Table table, string directory)
        {
            if (string.IsNullOrEmpty(Host))
            {
                if (table.Name == null)
                {
                    throw new System.ArgumentNullException("Null table name");
                }

                if (table.Name.Trim() == "")
                {
                    throw new System.ArgumentException("Empty table name");
                }

                if (DBProvider.DBProviderExists(table.Name))
                {
                    throw new DataException(string.Format("Table {0} exists already!", table.Name));
                }

                directory = Hubble.Framework.IO.Path.AppendDivision(directory, '\\');

                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                DBProvider.NewDBProvider(table.Name, new DBProvider());

                try
                {
                    DBProvider.GetDBProvider(table.Name).Create(table, directory);
                }
                catch 
                {
                    DBProvider.Drop(table.Name);
                    throw;
                }
            }
        }

        public void Insert(string tableName, List<Document> docs)
        {
            if (string.IsNullOrEmpty(Host))
            {
                if (_DBInsertProvider != null && _LastTableName != tableName)
                {
                    _DBInsertProvider.Collect();
                }

                _DBInsertProvider = DBProvider.GetDBProvider(tableName);
                _DBInsertProvider.Insert(docs);
            }

            _LastTableName = tableName;
        }

        public void Collect()
        {
            if (_DBInsertProvider != null)
            {
                _DBInsertProvider.Collect();
                _DBInsertProvider = null;
                _LastTableName = null;
            }
        }

        public void Close()
        {
            Collect();
        }

        #region IDisposable Members

        public void Dispose()
        {
            //Collect();
        }

        #endregion
    }
}
