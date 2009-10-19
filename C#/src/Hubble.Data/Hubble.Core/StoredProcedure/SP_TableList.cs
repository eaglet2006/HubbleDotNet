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

            foreach (string tableName in DBProvider.GetTables())
            {
                NewRow();
                OutputValue("TableName", tableName);
            }

        }

        #endregion
    }
}
