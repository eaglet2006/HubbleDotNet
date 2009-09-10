using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_Version : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get 
            {
                return "SP_Version"; 
            }
        }

        public void Run()
        {
            AddColumn("Version");
            OutputValue("Version", Hubble.Framework.Reflection.Assembly.GetCallingAssemblyVersion().ToString());
        }

        #endregion
    }
}
