using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_Columns : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_Columns";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 1)
            {
                throw new ArgumentException("the number of parameters must be 1. Parameter 1 is table name.");
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

            if (dbProvider == null)
            {
                OutputMessage(string.Format("Table name {0} does not exist!", Parameters[0]));
            }
            else
            {
                AddColumn("FieldName");
                AddColumn("DataType");
                AddColumn("DataLength");
                AddColumn("IndexType");
                AddColumn("Analyzer");
                AddColumn("IsNull");
                AddColumn("IsPrimaryKey");
                AddColumn("Default");

                foreach (Data.Field field in dbProvider.GetAllFields())
                {
                    NewRow();
                    OutputValue("FieldName", field.Name);
                    OutputValue("DataType", field.DataType.ToString());
                    OutputValue("DataLength", field.DataLength);
                    OutputValue("IndexType", field.IndexType.ToString());
                    OutputValue("Analyzer", field.AnalyzerName);
                    OutputValue("IsNull", field.CanNull);
                    OutputValue("IsPrimaryKey", field.PrimaryKey);
                    OutputValue("Default", field.DefaultValue);

                }


            }

        }

        #endregion
    }
}
