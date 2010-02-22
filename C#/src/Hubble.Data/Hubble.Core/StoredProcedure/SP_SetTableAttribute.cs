using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_SetTableAttribute : StoredProcedure, IStoredProc
    {
        void SetValue(string tableName, string attrName, string value)
        {
            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(tableName);

            if (dbProvider == null)
            {
                throw new StoredProcException(string.Format("Table name {0} does not exist.", tableName));
            }

            switch (attrName.ToLower())
            {
                case "indexonly":
                    {
                        bool indexonly;

                        if (bool.TryParse(value, out indexonly))
                        {
                            dbProvider.SetIndexOnly(indexonly);
                            dbProvider.SaveTable();
                            OutputMessage(string.Format("Set table {0} index only to {1} sucessful!",
                                tableName, dbProvider.IndexOnly));
                        }
                        else
                        {
                            throw new StoredProcException("Parameter 3 must be 'True' or 'False'");
                        }
                    }
                    break;
                case "maxreturncount":
                    {
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
                            throw new StoredProcException("Parameter 3 must be number of bytes");
                        }
                    }
                    break;
                default:
                    throw new StoredProcException("Can't set attribute:{0}, it is only can set at create statement");
            }

        }

        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_SetTableAttribute";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 3)
            {
                throw new StoredProcException("First parameter is table name. Second parameter is attribute name. Third is value");
            }

            string tableName = Parameters[0];
            string attrName = Parameters[1];
            string value = Parameters[2];

            SetValue(tableName, attrName, value);
        }

        #endregion

    }
}
