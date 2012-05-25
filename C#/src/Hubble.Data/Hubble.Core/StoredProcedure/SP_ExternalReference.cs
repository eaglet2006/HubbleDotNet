using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_ExternalReference : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_ExternalReference";
            }
        }

        private void ShowExternalReference()
        {
            AddColumn("RefName");
            AddColumn("AssemblyFile");

            foreach (Hubble.Core.Global.ExternalReference externRef in Global.Setting.Config.IQuerys)
            {
                NewRow();
                OutputValue("RefName", "Query");
                OutputValue("AssemblyFile", externRef.AssemblyFile);
            }

            foreach (Hubble.Core.Global.ExternalReference externRef in Global.Setting.Config.IAnalyzers)
            {
                NewRow();
                OutputValue("RefName", "Analyzer");
                OutputValue("AssemblyFile", externRef.AssemblyFile);
            }

            foreach (Hubble.Core.Global.ExternalReference externRef in Global.Setting.Config.IDBAdapters)
            {
                NewRow();
                OutputValue("RefName", "DBAdapter");
                OutputValue("AssemblyFile", externRef.AssemblyFile);
            }

            foreach (Hubble.Core.Global.ExternalReference externRef in Global.Setting.Config.IDistincts)
            {
                NewRow();
                OutputValue("RefName", "Distinct");
                OutputValue("AssemblyFile", externRef.AssemblyFile);
            }
        }

        public void Run()
        {
            ShowExternalReference();
        }

        #endregion


        #region IHelper Members

        public string Help
        {
            get 
            {
                return "List external reference";
            }
        }

        #endregion
    }
}
