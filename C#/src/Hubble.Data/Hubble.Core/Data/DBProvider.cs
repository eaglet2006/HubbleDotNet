using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Index;
using Hubble.Core.Global;
using Hubble.Framework.Reflection;

namespace Hubble.Core.Data
{
    class DBProvider
    {
        #region Static members
        static Dictionary<string, DBProvider> _DBProviderTable = new Dictionary<string, DBProvider>();
        static object _LockObj = new object();

        public static DBProvider GetDBProvider(string tableName)
        {
            return _DBProviderTable[tableName.ToLower().Trim()];
        }

        public static bool DBProviderExists(string tableName)
        {
            return _DBProviderTable.ContainsKey(tableName.ToLower().Trim());
        }

        public static void NewDBProvider(string tableName, DBProvider dbProvider)
        {
            _DBProviderTable.Add(tableName.ToLower().Trim(), dbProvider);
        }
        #endregion

        #region Private fields

        Dictionary<string, InvertedIndex> _FieldInvertedIndex = new Dictionary<string, InvertedIndex>();

        #endregion

        #region Properties

        DBAdapter.IDBAdapter _DBAdapter;

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
            }
        }

        #endregion

        #region Private methods

        private void AddFieldInvertedIndex(string fieldName, InvertedIndex index)
        {
            lock (this)
            {
                _FieldInvertedIndex.Add(fieldName.Trim().ToLower(), index);
            }
        }

        #endregion


        public void Create(Table table, string directory)
        {
            lock (_LockObj)
            {
                if (!string.IsNullOrEmpty(table.DBAdapterTypeName))
                {
                    DBAdapter = (DBAdapter.IDBAdapter)Instance.CreateInstance(table.DBAdapterTypeName);
                }

                if (DBAdapter != null)
                {
                    DBAdapter.Table = table;
                    DBAdapter.Drop();
                    DBAdapter.Create();
                }

                try
                {
                    table.Save(directory);
                }
                catch
                {
                    DBAdapter.Drop();
                    throw;
                }

                Setting.Config.Tables.Add(new TableConfig(directory));
                Setting.Save();

                foreach (Field field in table.Fields)
                {
                    if (field.IndexType == Field.Index.Tokenized)
                    {
                        InvertedIndex invertedIndex = new InvertedIndex(directory, field.Name.Trim(), true);
                        AddFieldInvertedIndex(field.Name, new InvertedIndex());
                    }
                }
            }
        }
    }
}
