using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.SQLClient
{
    public class SqlCommand
    {
        private SqlConnection _SqlConnection;

        private string _Sql;

        public string Sql
        {
            get
            {
                return _Sql;
            }
        }

        private QueryResult _QueryResult;

        public QueryResult Result
        {
            get
            {
                return _QueryResult;
            }
        }

        public System.Data.DataSet Query()
        {
            _QueryResult = _SqlConnection.QuerySql(_Sql);

            return _QueryResult.DataSet;
        }

        public SqlCommand(string sql, SqlConnection sqlConn)
        {
            _Sql = sql;
            _SqlConnection = sqlConn;
        }

        public int ExecuteNonQuery()
        {
            Query();

            if (_QueryResult.DataSet != null)
            {
                if (_QueryResult.DataSet.Tables.Count > 0)
                {
                    return _QueryResult.DataSet.Tables[0].MinimumCapacity;
                }
            }

            return 0;
        }
    }
}
