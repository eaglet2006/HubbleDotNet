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

        private object[] _Parameters = null;

        private QueryResult _QueryResult;

        public QueryResult Result
        {
            get
            {
                return _QueryResult;
            }
        }

        private string BuildSql()
        {
            return BuildSql(_Sql, _Parameters);
        }

        public static string BuildSql(string sql, object[] paras)
        {
            if (paras == null)
            {
                return sql;
            }
            else
            {
                object[] parameters = new object[paras.Length];
                paras.CopyTo(parameters, 0);

                for (int i = 0; i < parameters.Length; i++)
                {
                    object obj = parameters[i];

                    if (obj == null)
                    {
                        parameters[i] = "NULL";
                    }
                    else if (obj is string)
                    {
                        parameters[i] = string.Format("'{0}'",
                            ((string)obj).Replace("'", "''"));
                    }
                    else if (obj is DateTime)
                    {
                        parameters[i] = string.Format("'{0}'",
                            ((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else if (obj is byte[])
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.Append("0x");

                        foreach (byte b in (byte[])obj)
                        {
                            sb.AppendFormat("{0:x1}", b);
                        }

                        parameters[i] = sb.ToString();
                    }
                    else
                    {
                        parameters[i] = obj.ToString();
                    }

                }

                return string.Format(sql, parameters);
            }
        }

        public SqlCommand(string sql, SqlConnection sqlConn)
        {
            _Sql = sql;
            _SqlConnection = sqlConn;
            _Parameters = null;
        }

        public SqlCommand(string sql, SqlConnection sqlConn, params object[] parameters)
        {
            _Sql = sql;
            _SqlConnection = sqlConn;
            _Parameters = parameters;
        }

        public System.Data.DataSet Query()
        {
            _QueryResult = _SqlConnection.QuerySql(BuildSql());

            return _QueryResult.DataSet;
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
