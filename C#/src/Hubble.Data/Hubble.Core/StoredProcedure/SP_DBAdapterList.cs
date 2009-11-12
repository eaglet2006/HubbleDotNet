using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Data;

namespace Hubble.Core.StoredProcedure
{
    class SP_DBAdapterList : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_DBAdapterList";
            }
        }

        public void Run()
        {
            AddColumn("Name");
            AddColumn("ClassName");
            AddColumn("Assembly");
            AddColumn("FileName");

            Global.IDBAdapterConfig[] adapters = Global.Setting.Config.IDBAdapters.ToArray();

            foreach (Type type in Data.DBProvider.GetDBAdapters())
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
