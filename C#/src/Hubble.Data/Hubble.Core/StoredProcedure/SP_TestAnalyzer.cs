using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    class SP_TestAnalyzer : StoredProcedure, IStoredProc
    {
        #region IStoredProc Members

        public string Name
        {
            get
            {
                return "SP_TestAnalyzer";
            }
        }

        public void Run()
        {
            if (Parameters.Count != 2)
            {
                throw new ArgumentException("the number of parameters must be 2. Parameter 1 is Analyzer name, Parameter 2 is a text for test.");
            }

            Analysis.IAnalyzer analyzer = Data.DBProvider.GetAnalyzer(Parameters[0]);

            if (analyzer == null)
            {
                throw new Data.DataException(string.Format("Can't find analyzer name : {0}", Parameters[0]));
            }

            AddColumn("Word");
            AddColumn("Position");
            AddColumn("Rank");

            foreach (Entity.WordInfo word in analyzer.Tokenize(Parameters[1]))
            {
                NewRow();
                OutputValue("Word", word.Word);
                OutputValue("Position", word.Position.ToString());
                OutputValue("Rank", word.Rank.ToString());
            }
        }

        #endregion
    }
}
