using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_Version : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
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

        #region IHelper Members

        public string Help
        {
            get 
            {
                return "Get hubble.net service version";
            }
        }

        #endregion
    }
}
