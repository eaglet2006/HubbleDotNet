using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_AnalyzerList : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_AnalyzerList";
            }
        }

        public void Run()
        {
            AddColumn("Name");
            AddColumn("ClassName");
            AddColumn("Assembly");
            AddColumn("FileName");

            foreach (Type type in Data.DBProvider.GetAnalyzers())
            {
                NewRow();

                Data.INamedExternalReference nameRef = Hubble.Framework.Reflection.Instance.CreateInstance(type)
                    as Data.INamedExternalReference;

                OutputValue("Name", nameRef.Name);
                OutputValue("ClassName", type.FullName);
                OutputValue("Assembly", type.Assembly.FullName);
                OutputValue("FileName", type.Assembly.Location);
            }

        }

        #endregion
    }
}
