using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_DropDatabase : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_DropDatabase";
            }
        }

        public void Run()
        {
            if (Parameters.Count < 1)
            {
                throw new ArgumentException("Parameter 1 is Database name.");
            }

            Global.Setting.RemoveDatabase(Parameters[0].Trim());
            Global.Setting.Save();

            OutputMessage(string.Format("Drop database {0} successul.", Parameters[0].Trim()));
        }

        #endregion
    }
}
