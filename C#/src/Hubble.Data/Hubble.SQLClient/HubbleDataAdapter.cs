using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Hubble.SQLClient
{
    public class HubbleDataAdapter: System.Data.Common.DbDataAdapter,
        System.Data.IDbDataAdapter, System.Data.IDataAdapter, ICloneable

    {
        new public HubbleCommand SelectCommand
        {
            get
            {
                return base.SelectCommand as HubbleCommand;
            }

            set
            {
                base.SelectCommand = value;
            }
        }

        new public void Fill(DataSet dataset)
        {
            dataset.Clear();

            List<DataTable> tables = new List<DataTable>();
            DataSet ds = SelectCommand.Query();

            foreach(DataTable table in ds.Tables)
            {
                tables.Add(table);
            }

            foreach (DataTable table in tables)
            {
                ds.Tables.Remove(table);
                dataset.Tables.Add(table);
            }

        }
    }
}
