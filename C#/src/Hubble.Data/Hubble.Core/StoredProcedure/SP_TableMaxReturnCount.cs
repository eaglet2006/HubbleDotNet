using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_TableMaxReturnCount : StoredProcedure, IStoredProc
    {
        void ShowValue(string tableName)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName));
            }

            AddColumn("TableName");
            AddColumn("MaxReturnCount");

            OutputValue("TableName", tableName);
            OutputValue("MaxReturnCount", dbProvider.MaxReturnCount.ToString());
        }

        void SetValue(string tableName, string value)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName));
            }

            int count;

            if (int.TryParse(value, out count))
            {
                dbProvider.SetMaxReturnCount(count);
                dbProvider.SaveTable();
                OutputMessage(string.Format("Set table {0} max return count to {1} sucessful!",
                    tableName, dbProvider.MaxReturnCount));
            }
            else
            {
                throw new StoredProcException("Parameter 2 must be number of bytes");
            }
        }

        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_TableMaxReturnCount";
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
                throw new StoredProcException("First parameter is table name and second is number of bytes.");
            }

        }

        #endregion

    }
}
