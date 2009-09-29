using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_ExcuteSql : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get 
            {
                return "SP_ExcuteSql"; 
            }
        }

        public void Run()
        {
            if (Parameters.Count != 2)
            {
                throw new ArgumentException("the number of parameters must be 2. Parameter 1 is table name. Parameter 2 is sql for excute");
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist!", Parameters[0]));
            }

            int rows = dbProvider.DBAdapter.ExcuteSql(Parameters[1]);

            OutputMessage(string.Format("Excute sql successul. Affect {0} rows!", rows));
        }

        #endregion
    }
}
