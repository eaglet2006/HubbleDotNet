using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.SQLClient
{
    public class TableTicks
    {
        private string _TableName;

        public string TableName
        {
            get
            {
                return _TableName;
            }
        }

        private long _Ticks;

        public long Ticks
        {
            get
            {
                return _Ticks;
            }
        }

        public TableTicks(string tableName, long ticks)
        {
            _TableName = tableName;
            _Ticks = ticks;
        }

        public static List<TableTicks> ToTableTicksList(string text)
        {
            List<TableTicks> result = new List<TableTicks>();

            foreach (string tblTicks in Hubble.Framework.Text.Regx.Split(text, ";"))
            {
                if (string.IsNullOrEmpty(tblTicks))
                {
                    continue;
                }

                List<string> strs;

                if (Hubble.Framework.Text.Regx.GetMatchStrings(tblTicks, @"(.+?)\=(\d+)", true,
                    out strs))
                {
                    string tableName = strs[0].Trim();

                    if (tableName != "")
                    {
                        result.Add(new TableTicks(tableName, long.Parse(strs[1])));
                    }
                }
            }

            return result;
        }
    }
}
