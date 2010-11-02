using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_ExcuteSql : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get 
            {
                return "SP_ExcuteSql"; 
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo(Right.RightItem.ExcuteSql);

            if (Parameters.Count < 1)
            {
                throw new ArgumentException("the number of parameters must be 2. Parameter 1 is table name. Parameter 2 is sql for excute");
            }

            DBAdapter.IDBAdapter dbAdapter;
            string sql;

            if (Parameters.Count == 1)
            {
                string curDatabaseName = Service.CurrentConnection.ConnectionInfo.DatabaseName;
                Global.Database database = Global.Setting.GetDatabase(curDatabaseName);

                if (string.IsNullOrEmpty(database.DefaultDBAdapter))
                {
                    throw new StoredProcException("Current database hasn't default dbadapter");
                }

                if (string.IsNullOrEmpty(database.DefaultConnectionString))
                {
                    throw new StoredProcException("Current database hasn't default connectionstring");
                }

                dbAdapter = (DBAdapter.IDBAdapter)Hubble.Framework.Reflection.Instance.CreateInstance(
                    Data.DBProvider.GetDBAdapter(database.DefaultDBAdapter));

                if (dbAdapter == null)
                {
                    throw new StoredProcException(string.Format("Current database include a invalid default dbadapter:{0}",
                        database.DefaultDBAdapter));
                }

                dbAdapter.ConnectionString = database.DefaultConnectionString;

                sql = Parameters[0];
            }
            else
            {
                Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

                if (dbProvider == null)
                {
                    throw new StoredProcException(string.Format("Table name {0} does not exist!", Parameters[0]));
                }

                dbAdapter = dbProvider.DBAdapter;
                sql = Parameters[1];
            }

            int rows = dbAdapter.ExcuteSql(sql);

            OutputMessage(string.Format("Excute sql successul. Affect {0} rows!", rows));
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Excute sql directly from database. Parameter 1 is table name or sql for excute. Parameter 2 is sql for excute if Parameter 1 is table name";
            }
        }

        #endregion
    }
}
