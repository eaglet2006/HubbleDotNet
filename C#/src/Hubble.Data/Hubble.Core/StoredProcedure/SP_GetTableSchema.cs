using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_GetTableSchema : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_GetTableSchema";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 3)
            {
                throw new ArgumentException("Parameter 1 is DBAdapter Name. Parameter 2 is ConnectionString. Parameter 3 is table name.");
            }

            string tableName;

            DBAdapter.IDBAdapter dbAdapter;

            dbAdapter = (DBAdapter.IDBAdapter)Hubble.Framework.Reflection.Instance.CreateInstance(
                Data.DBProvider.GetDBAdapter(Parameters[0]));

            if (dbAdapter == null)
            {
                throw new StoredProcException(string.Format("Invalid dbadapter:{0}",
                    Parameters[0]));
            }

            dbAdapter.ConnectionString = Parameters[1];

            tableName = Parameters[2];

            _QueryResult.DataSet = dbAdapter.GetSchema(tableName);

        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Get table's schema from database. Parameter 1 is DBAdapter Name. Parameter 2 is ConnectionString. Parameter 3 is table name.";
            }
        }

        #endregion 
    }
}
