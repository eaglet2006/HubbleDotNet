using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_TableQueryCache : StoredProcedure, IStoredProc
    {
        void ShowValue(string tableName)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName)); 
            }

            AddColumn("TableName");
            AddColumn("QueryCacheEnabled");
            AddColumn("QueryCacheTimeout");

            OutputValue("TableName", tableName);
            OutputValue("QueryCacheEnabled", dbProvider.QueryCacheEnabled.ToString());
            OutputValue("QueryCacheTimeout", dbProvider.QueryCacheTimeout.ToString());
        }

        void SetValue(string tableName, string value, string timeout)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName));
            }

            bool cacheEnabled;
            int cacheTimeout = -1;

            if (!bool.TryParse(value, out cacheEnabled))
            {
                throw new StoredProcException("Parameter 2 must be 'True' or 'False'");
            }

            if (timeout != null)
            {
                if (!int.TryParse(timeout, out cacheTimeout))
                {
                    throw new StoredProcException("Parameter 3 must be none-negative integer");
                }
            }

            dbProvider.SetCacheQuery(cacheEnabled, cacheTimeout);
            dbProvider.SaveTable();

            OutputMessage(string.Format("Table:{0} QueryCacheEnabled = {1} QueryCatchTimeout = {2}",
                tableName, dbProvider.QueryCacheEnabled, dbProvider.QueryCacheTimeout));
        }

        void SetValue(string tableName, string value)
        {
            SetValue(tableName, value, null);
        }

        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_TableQueryCache";
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
            else if (Parameters.Count == 3)
            {
                SetValue(Parameters[0], Parameters[1], Parameters[2]);
            }
            else
            {
                throw new StoredProcException("First parameter is table name and second is 'True' or 'False', third is QueryCacheTimeout.");
            }

        }

        #endregion

    }
}
