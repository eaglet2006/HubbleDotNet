using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Data;

namespace Hubble.Core.StoredProcedure
{
    class SP_TableList : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_TableList";
            }
        }

        public void Run()
        {
            AddColumn("TableName");

            string databaseName = null;

            if (Parameters.Count > 0)
            {
                //First parameter is database name

                databaseName = Parameters[0] + ".";
            }

            foreach (string tableName in DBProvider.GetTables())
            {
                if (databaseName != null)
                {
                    if (tableName.IndexOf(databaseName, 0, StringComparison.CurrentCultureIgnoreCase) != 0)
                    {
                        continue;
                    }
                }

                NewRow();
                OutputValue("TableName", tableName);
            }

        }

        #endregion
    }
}
