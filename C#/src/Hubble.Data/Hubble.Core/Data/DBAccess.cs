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
                directory = Hubble.Framework.IO.Path.AppendDivision(directory, '\\');

                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                DBProvider.NewDBProvider(table.Name, new DBProvider());
                DBProvider.GetDBProvider(table.Name).Create(table, directory);
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


        #region IDisposable Members

        public void Dispose()
        {
            Collect();
        }

        #endregion
    }
}
