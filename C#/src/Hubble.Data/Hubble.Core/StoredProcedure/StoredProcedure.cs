using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    public abstract class StoredProcedure
    {
        protected SFQL.Parse.QueryResult _QueryResult = new Hubble.Core.SFQL.Parse.QueryResult();

        protected void AddColumn(string columnName)
        {
            if (_QueryResult.DataSet.Tables.Count == 0)
            {
                _QueryResult.DataSet.Tables.Add(new System.Data.DataTable());
            }

            System.Data.DataTable table = _QueryResult.DataSet.Tables[0];

            System.Data.DataColumn col = new System.Data.DataColumn(columnName);

            table.Columns.Add(col);
        }

        protected void NewRow()
        {
            System.Data.DataTable table = _QueryResult.DataSet.Tables[0];

            table.Rows.Add(table.NewRow());
        }

        protected void OutputValue(string columnName, object value)
        {
            System.Data.DataTable table = _QueryResult.DataSet.Tables[0];

            if (table.Rows.Count == 0)
            {
                table.Rows.Add(table.NewRow());
            }

            table.Rows[table.Rows.Count - 1][columnName] = value; 
        }

        protected void OutputMessage(string message)
        {
            _QueryResult.PrintMessages.Add(message);
        }

        protected void RemoveTable()
        {
            if (_QueryResult.DataSet.Tables.Count > 0)
            {
                _QueryResult.DataSet.Tables.Remove(_QueryResult.DataSet.Tables[0]);
            }
        }

        List<string> _Parameters = new List<string>();

        public List<string> Parameters
        {
            get
            {
                return _Parameters;
            }
        }

        public Hubble.Core.SFQL.Parse.QueryResult Result
        {
            get
            {
                return _QueryResult;
            }
        }
    }
}
