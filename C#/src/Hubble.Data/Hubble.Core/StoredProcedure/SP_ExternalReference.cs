using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_ExternalReference : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
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
        }

        public void Run()
        {
            ShowExternalReference();
        }

        #endregion

    }
}
