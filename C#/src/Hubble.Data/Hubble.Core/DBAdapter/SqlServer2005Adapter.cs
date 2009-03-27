using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.DBAdapter
{
    public class SqlServer2005Adapter : IDBAdapter
    {
        private string GetFieldLine(Data.Field field)
        {
            if (!field.Store)
            {
                return null;
            }

            string sqlType = "";

            switch (field.DataType)
            {
                case Hubble.Core.Data.DataType.Int32:
                case Hubble.Core.Data.DataType.Date:
                case Hubble.Core.Data.DataType.SmallDateTime:
                    sqlType = "Int";
                    break;
                case Hubble.Core.Data.DataType.DateTime:
                    sqlType = "DateTime";
                    break;
                case Hubble.Core.Data.DataType.Float:
                    sqlType = "Float";
                    break;
                case Hubble.Core.Data.DataType.Int64:
                    sqlType = "Int64";
                    break;
                case Hubble.Core.Data.DataType.String:
                    sqlType = "nvarchar ({1})";
                    break;
                default:
                    throw new ArgumentException(field.DataType.ToString());
            }

            return string.Format("{0} " + sqlType + ",", field.Name,
                field.DataLength <= 0 ? "max" : field.DataLength.ToString());
        }

        #region IDBAdapter Members

        Data.Table _Table;

        public Hubble.Core.Data.Table Table
        {
            get
            {
                return _Table;
            }
            set
            {
                _Table = value;
            }
        }

        public void Create()
        {
            Debug.Assert(Table != null);

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("create table {0} (", Table.DBTableName);

            foreach (Data.Field field in Table.Fields)
            {
                string fieldSql = GetFieldLine(field);

                if (fieldSql != null)
                {
                    sql.Append(field);
                }
            }

            sql.Append(")");


        }

        public void Insert(List<Hubble.Core.Data.Document> docs)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
