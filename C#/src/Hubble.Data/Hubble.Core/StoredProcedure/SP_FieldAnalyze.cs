using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    public class SP_FieldAnalyze : StoredProcedure, IStoredProc, IHelper
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_FieldAnalyze";
            }
        }

        public void Run()
        {
            if (Parameters.Count < 3)
            {
                throw new ArgumentException("Parameter 1 is table name, Parameter 2 is field name, Parameter 3 is a text for test, Parameter 4 is analyzer type(optional)");
            }

            Data.DBProvider dbProvider = Data.DBProvider.GetDBProvider(Parameters[0]);

            if (dbProvider == null)
            {
                throw new Data.DataException(string.Format("Can't find table name : {0}", Parameters[0]));
            }

            Data.Field field = dbProvider.GetField(Parameters[1]);

            if (field == null)
            {
                throw new Data.DataException(string.Format("Can't find field name : {0}", Parameters[2]));
            }

            Analysis.IAnalyzer analyzer = Data.DBProvider.GetAnalyzer(field.AnalyzerName);

            if (analyzer == null)
            {
                throw new Data.DataException(string.Format("Can't find analyzer name : {0}", field.AnalyzerName));
            }

            bool clientAnalyzer = false;

            if (Parameters.Count == 4)
            {
                if (Parameters[3].Equals("sqlclient", StringComparison.CurrentCultureIgnoreCase))
                {
                    clientAnalyzer = true;
                }
            }

            AddColumn("Word");
            AddColumn("Position");
            AddColumn("Rank");

            if (clientAnalyzer)
            {
                foreach (Entity.WordInfo word in analyzer.TokenizeForSqlClient(Parameters[2]))
                {
                    NewRow();
                    OutputValue("Word", word.Word);
                    OutputValue("Position", word.Position.ToString());
                    OutputValue("Rank", word.Rank.ToString());
                }
            }
            else
            {
                foreach (Entity.WordInfo word in analyzer.Tokenize(Parameters[2]))
                {
                    NewRow();
                    OutputValue("Word", word.Word);
                    OutputValue("Position", word.Position.ToString());
                    OutputValue("Rank", word.Rank.ToString());
                }
            }
        }

        #endregion

        #region IHelper Members

        public string Help
        {
            get
            {
                return "Analyze text using the analyzer of the field that input. Parameter 1 is table name, Parameter 2 is field name, Parameter 3 is a text for test, Parameter 4 is analyzer type(optional)";
            }
        }

        #endregion
    }
}
