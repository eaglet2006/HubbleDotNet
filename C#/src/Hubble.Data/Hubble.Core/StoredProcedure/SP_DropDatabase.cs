using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_DropDatabase : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
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

        #region IHelper Members

        public string Help
        {
            get 
            {
                return "Drop database. Parameter 1 is Database name.";
            }
        }

        #endregion
    }
}
