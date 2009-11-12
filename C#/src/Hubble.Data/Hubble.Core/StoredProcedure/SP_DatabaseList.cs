using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Data;

namespace Hubble.Core.StoredProcedure
{
    class SP_DatabaseList : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_DatabaseList";
            }
        }

        public void Run()
        {
            AddColumn("DatabaseName");
            AddColumn("DefaultPath");
            AddColumn("DefaultDBAdapter");
            AddColumn("DefaultConnectionString");

            foreach (string databaseName in Global.Setting.DatabaseList)
            {
                NewRow();
                OutputValue("DatabaseName", databaseName);

                Global.Database database = Global.Setting.GetDatabase(databaseName);

                OutputValue("DefaultPath", database.DefaultPath);
                OutputValue("DefaultDBAdapter", database.DefaultDBAdapter);
                OutputValue("DefaultConnectionString", database.DefaultConnectionString);

            }
        }

        #endregion
    }
}
