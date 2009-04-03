using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Data
{
    public class DBAccess
    {
        public void CreateTable(Table table, string directory)
        {
            directory = Hubble.Framework.IO.Path.AppendDivision(directory, '\\');

            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            DBProvider.NewDBProvider(table.Name, new DBProvider());
            DBProvider.GetDBProvider(table.Name).Create(table, directory);
        }
    }
}
