using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_TableIndexOnly : StoredProcedure, IStoredProc
    {
        void ShowValue(string tableName)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName)); 
            }

            AddColumn("TableName");
            AddColumn("IndexOnly");

            OutputValue("TableName", tableName);
            OutputValue("IndexOnly", dbProvider.IndexOnly.ToString());
        }

        void SetValue(string tableName, string value)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName));
            }

            bool indexonly;

            if (bool.TryParse(value, out indexonly))
            {
                dbProvider.IndexOnly = indexonly;
                dbProvider.SaveTable();
                OutputMessage(string.Format("Set table {0} index only to {1} sucessful!",
                    tableName, dbProvider.IndexOnly));
            }
            else
            {
                throw new StoredProcException("Parameter 2 must be 'True' or 'False'");
            }
        }

        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_TableIndexOnly";
            }
        }

        public void Run()
        {
            if (Parameters.Count == 1)
            {
                ShowValue(Parameters[0]);
            }
            else if (Parameters.Count == 2)
            {
                SetValue(Parameters[0], Parameters[1]);
            }
            else
            {
                throw new StoredProcException("First parameter is table name and second is 'True' or 'False'.");
            }

        }

        #endregion

    }
}
