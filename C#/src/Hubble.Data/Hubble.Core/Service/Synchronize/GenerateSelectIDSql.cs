using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Core.Service.Synchronize
{
    class GenerateSelectIDSql
    {
        internal class IdFields : IComparable<IdFields>
        {
            internal long Id;
            internal string Fields;
            internal bool HasTokenizedFields;

            internal IdFields(long id, bool hasTokenizedFields)
            {
                this.Id = id;
                this.Fields = null;
                HasTokenizedFields = hasTokenizedFields;
            }

            public override bool Equals(object obj)
            {
                IdFields other = (IdFields)obj;

                if (other == null)
                {
                    return false;
                }

                return this.Id == other.Id && this.HasTokenizedFields == other.HasTokenizedFields;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", this.Id, this.HasTokenizedFields, this.Fields);
            }

            #region IComparable<IdFields> Members

            public int CompareTo(IdFields other)
            {
                if (this.HasTokenizedFields && !other.HasTokenizedFields)
                {
                    return 1;
                }
                else if (!this.HasTokenizedFields && other.HasTokenizedFields)
                {
                    return -1;
                }
                else
                {
                    if (this.Id > other.Id)
                    {
                        return 1;
                    }
                    else if (this.Id < other.Id)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }


            #endregion
        }


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

        private static string GetFieldsStringFromHashSet(HashSet<string> fieldsSet, string dbAdapterName)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (string field in fieldsSet)
            {
                if (i++ > 0)
                {
                    sb.Append(",");
                }

                if (dbAdapterName.IndexOf("sqlserver", StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    sb.Append("[" + field + "]");
                }
                else if (dbAdapterName.IndexOf("oracle", StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    sb.Append(Hubble.Core.DBAdapter.Oracle8iAdapter.GetFieldName(field));
                }
                else if (dbAdapterName.IndexOf("mysql", StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    sb.Append("`" + field + "`");
                }
                else if (dbAdapterName.IndexOf("sqlite", StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    sb.Append("[" + field + "]");
                }
                else if (dbAdapterName.IndexOf("mongodb", StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    sb.Append("'" + field + "'");
                }
                else
                {
                    sb.Append("[" + field + "]");
                } 
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get IdFields List from trigger table 
        /// </summary>
        /// <param name="dbProvider">dbProvider that used to get field info</param>
        /// <param name="dbAdapterName">DBAdapter name</param>
        /// <param name="table">table that read from trigger table</param>
        /// <param name="lastSerial">the last serial number of trigger table from which read</param>
        /// <returns></returns>
        internal static List<IdFields> GetIdFieldsList(Data.DBProvider dbProvider, string dbAdapterName, 
            System.Data.DataTable table, out long lastSerial)
        {
            lastSerial = -1;

            HashSet<string> fieldsSetWithTokenizedFields = new HashSet<string>();
            HashSet<string> fieldsSetWithoutTokenizedFields = new HashSet<string>();
            List<string> tempFields = new List<string>(128);
            List<IdFields> result = new List<IdFields>(table.Rows.Count);

            foreach (System.Data.DataRow row in table.Rows)
            {
                long id = long.Parse(row["id"].ToString());
                lastSerial = long.Parse(row["Serial"].ToString());
                string fields = row["Fields"].ToString();
                
                tempFields.Clear();
                bool hasTokenized = false;

                //check fields
                foreach (string field in fields.Split(new char[] { ',' }))
                {
                    string f = field.Trim().ToLower();

                    if (f == "")
                    {
                        continue;
                    }

                    Data.Field dbField = dbProvider.GetField(f);

                    if (dbField == null)
                    {
                        continue;
                    }

                    if (dbField.IndexType == Hubble.Core.Data.Field.Index.Tokenized)
                    {
                        hasTokenized = true;
                    }

                    tempFields.Add(f);
                }

                //Fill hash set
                if (hasTokenized)
                {
                    foreach (string field in tempFields)
                    {
                        if (!fieldsSetWithTokenizedFields.Contains(field))
                        {
                            fieldsSetWithTokenizedFields.Add(field);
                        }
                    }
                }
                else
                {
                    foreach (string field in tempFields)
                    {
                        if (!fieldsSetWithoutTokenizedFields.Contains(field))
                        {
                            fieldsSetWithoutTokenizedFields.Add(field);
                        }
                    }
                }

                result.Add(new IdFields(id, hasTokenized));
            }

            //Get new fields string 
            string fieldsWithTokenized = GetFieldsStringFromHashSet(fieldsSetWithTokenizedFields, dbAdapterName);
            string fieldsWithoutTokenized = GetFieldsStringFromHashSet(fieldsSetWithoutTokenizedFields, dbAdapterName);

            foreach(IdFields idFields in result)
            {
                if (idFields.HasTokenizedFields)
                {
                    idFields.Fields = fieldsWithTokenized;
                }
                else
                {
                    idFields.Fields = fieldsWithoutTokenized;
                }
            }

            //Merge same Id
            result.Sort();

            if (result.Count > 0)
            {
                IdFields last = result[0];

                for (int i = 1; i < result.Count; i++)
                {
                    if (result[i].Equals(last))
                    {
                        result[i] = null;
                        continue;
                    }
                    else
                    {
                        last = result[i];
                    }
                }
            }

            return result;
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
