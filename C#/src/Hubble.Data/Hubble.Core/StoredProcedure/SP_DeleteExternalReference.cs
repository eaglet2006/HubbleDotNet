using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_DeleteExternalReference : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        override public string Name
        {
            get
            {
                return "SP_DeleteExternalReference";
            }
        }

        private void DeleteExternalReference(string refName, string assemblyFile)
        {
            refName = refName.ToLower().Trim();

            if (!System.IO.File.Exists(assemblyFile))
            {
                throw new StoredProcException(string.Format("assembly file : {0} does not exist!", assemblyFile));
            }

            switch (refName)
            {
                case "query":
                    Hubble.Core.Global.IQueryConfig iqueryConfig = new Hubble.Core.Global.IQueryConfig(assemblyFile);

                    Global.Setting.Config.IQuerys.Remove(iqueryConfig);
                    Global.Setting.Save();
                    break;
                case "analyzer":
                    Hubble.Core.Global.IAnalyzerConfig ianalyzerConfig = new Hubble.Core.Global.IAnalyzerConfig(assemblyFile);

                    Global.Setting.Config.IAnalyzers.Remove(ianalyzerConfig);
                    Global.Setting.Save();
                    break;

                case "dbadapter":
                    Hubble.Core.Global.IDBAdapterConfig idbadapterConfig = new Hubble.Core.Global.IDBAdapterConfig(assemblyFile);

                    Global.Setting.Config.IDBAdapters.Remove(idbadapterConfig);
                    Global.Setting.Save();
                    break;

                default:
                    throw new StoredProcException("refName must be 'Query', 'Analyzer' or 'DBAdapter");
            }
        }

        public void Run()
        {
            Global.UserRightProvider.CanDo("", Right.RightItem.ManageSystem);

            if (Parameters.Count != 2)
            {
                throw new StoredProcException("Parameter number must be 2. Parameter 1 is refer name and parameter 2 is assembly file path!");
            }

            DeleteExternalReference(Parameters[0], Parameters[1]);

            OutputMessage(string.Format("Delete external reference {0} on {1} successfully! Available after reboot hubble.net!",
                Parameters[1], Parameters[0]));
        }

        #endregion


        #region IHelper Members

        public string Help
        {
            get
            {
                return "Delete external reference. Parameter number must be 2. Parameter 1 is refer name and parameter 2 is assembly file path!";
            }
        }

        #endregion
    }
}
