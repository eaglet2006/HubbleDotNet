using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_AddDatabase : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_AddDatabase";
            }
        }

        public void Run()
        {
            if (Parameters.Count < 1)
            {
                throw new ArgumentException("Parameter 1 is Database name. Parameter 2 is DefaultPath, Parameter 3 is DefaultDBAdapter, Parameter 4 is DefaultConnectionString");
            }

            Global.Database database = new Hubble.Core.Global.Database();

            if (Parameters.Count > 3)
            {
                database.DefaultConnectionString = Parameters[3];
            }

            if (Parameters.Count > 2)
            {
                database.DefaultDBAdapter = Parameters[2];
            }

            if (Parameters.Count > 1)
            {
                string dir = System.IO.Path.GetFullPath(Parameters[1]);

                dir = Hubble.Framework.IO.Path.AppendDivision(dir, '\\');

                dir = System.IO.Path.GetDirectoryName(dir);

                dir = Hubble.Framework.IO.Path.AppendDivision(dir, '\\');

                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                database.DefaultPath = dir;
            }

            if (Parameters.Count > 0)
            {
                database.DatabaseName = Parameters[0].Trim();
            }

            Global.Setting.AddDatabase(database);
            Global.Setting.Save();

            OutputMessage(string.Format("Create database {0} successul.", database.DatabaseName));
        }

        #endregion
    }
}
