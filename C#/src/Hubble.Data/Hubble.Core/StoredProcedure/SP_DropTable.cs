using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_DropTable : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_DropTable";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 1)
            {
                throw new ArgumentException("the number of parameters must be 1. Parameter 1 is table name.");
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

            if (dbProvider == null)
            {
                OutputMessage(string.Format("Table name {0} does not exist!", Parameters[0]));
            }
            else
            {
                Data.DBProvider.Drop(Parameters[0]);
            }

            OutputMessage(string.Format("Drop table {0} successul.", Parameters[0]));
        }

        #endregion
    }
}
