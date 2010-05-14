using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_GetTableSchema : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_GetTableSchema";
            }
        }

        private void FixQueryResult(Hubble.SQLClient.QueryResult result)
        {
            List<System.Data.DataColumn> invaildColList = new List<System.Data.DataColumn>(); 

            if (result.DataSet != null)
            {
                if (result.DataSet.Tables != null)
                {
                    foreach (System.Data.DataColumn col in result.DataSet.Tables[0].Columns)
                    {
                        try
                        {
                            Hubble.SQLClient.QueryResultSerialization.GetDataType(col.DataType);
                        }
                        catch
                        {
                            invaildColList.Add(col);
                        }
                    }

                    foreach (System.Data.DataColumn col in invaildColList)
                    {
                        Global.Report.WriteErrorLog(string.Format("Invalid data type = {0} in column:{1}",
                            col.DataType.ToString(), col.ColumnName));
                        result.DataSet.Tables[0].Columns.Remove(col);
                    }
                }
            }
        }

        public void Run()
        {
            if (Parameters.Count != 3)
            {
                throw new ArgumentException("Parameter 1 is DBAdapter Name. Parameter 2 is ConnectionString. Parameter 3 is table name.");
            }

            string tableName;

            DBAdapter.IDBAdapter dbAdapter;

            dbAdapter = (DBAdapter.IDBAdapter)Hubble.Framework.Reflection.Instance.CreateInstance(
                Data.DBProvider.GetDBAdapter(Parameters[0]));

            if (dbAdapter == null)
            {
                throw new StoredProcException(string.Format("Invalid dbadapter:{0}",
                    Parameters[0]));
            }

            dbAdapter.ConnectionString = Parameters[1];

            tableName = Parameters[2];

            _QueryResult.DataSet = dbAdapter.GetSchema(tableName);

            FixQueryResult(_QueryResult);

        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Get table's schema from database. Parameter 1 is DBAdapter Name. Parameter 2 is ConnectionString. Parameter 3 is table name.";
            }
        }

        #endregion 
    }
}
