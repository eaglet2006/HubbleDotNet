using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    public class SP_GCCollect : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_GCCollect";
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo("", Right.RightItem.ExcuteStoreProcedure);

            GC.Collect(GC.MaxGeneration);
            GC.Collect(GC.MaxGeneration);

            OutputMessage("Force collect GC successfully.");
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Force collect GC";
            }
        }

        #endregion
    }
}
