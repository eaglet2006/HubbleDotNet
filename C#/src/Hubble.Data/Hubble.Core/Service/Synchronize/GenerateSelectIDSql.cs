using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Core.Service.Synchronize
{
    class GenerateSelectIDSql
    {
        string _IDFieldName;
        string _DBAdapterName;
        string _FieldSql;
        string _DBTableName;
        SyncFlags _Flags;

        StringBuilder GetSqlServerSelectSql()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Select ");
                            
            if ((_Flags & SyncFlags.NoLock) != 0)
            {
                sb.AppendFormat("{0}, {1} from {2} with(nolock) where {0} in ", _IDFieldName,
                    _FieldSql, _DBTableName);
                // Mark here. Will add a option.
            }
            else
            {
                sb.AppendFormat("{0}, {1} from {2} where {0} in ", _IDFieldName,
                    _FieldSql, _DBTableName);
            }

            return sb;
        }

        StringBuilder GetOracleSelectSql()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Select ");

            sb.AppendFormat("{0}, {1} from {2} where {0} in ", _IDFieldName,
                _FieldSql, _DBTableName);

            return sb;
        }

        StringBuilder GetMySqlSelectSql()
        {
            StringBuilder sb = new StringBuilder();

            if ((_Flags & SyncFlags.NoLock) != 0)
            {
                sb.Append("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ;");
            }

            sb.Append("Select ");

            if ((_Flags & SyncFlags.NoLock) != 0)
            {
                sb.AppendFormat("{0}, {1} from {2} with(nolock) where {0} in ", _IDFieldName,
                    _FieldSql, _DBTableName);
            }
            else
            {
                sb.AppendFormat("{0}, {1} from {2} where {0} in ", _IDFieldName,
                    _FieldSql, _DBTableName);
            }

            if ((_Flags & SyncFlags.NoLock) != 0)
            {
                sb.Append(";SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;");
            }

            return sb;
        }

        StringBuilder GetSqliteSelectSql()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Select ");

            sb.AppendFormat("{0}, {1} from {2} where {0} in ", _IDFieldName,
                _FieldSql, _DBTableName);

            return sb;
        }

        internal StringBuilder GetSql()
        {
            if (_DBAdapterName.IndexOf("sqlserver", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetSqlServerSelectSql();
            }
            else if (_DBAdapterName.IndexOf("oracle", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetOracleSelectSql();
            }
            else if (_DBAdapterName.IndexOf("mysql", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetMySqlSelectSql();
            }
            else if (_DBAdapterName.IndexOf("sqlite", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return GetSqliteSelectSql();
            }
            else
            {
                return GetSqlServerSelectSql();
            }
        }

        internal GenerateSelectIDSql(string idFieldName, string dbAdapterName, string fieldsSql, string dbTableName, SyncFlags flags)
        {
            _IDFieldName = idFieldName;
            _DBAdapterName = dbAdapterName;
            _FieldSql = fieldsSql;
            _DBTableName = dbTableName;
            _Flags = flags;
        }
    }
}
