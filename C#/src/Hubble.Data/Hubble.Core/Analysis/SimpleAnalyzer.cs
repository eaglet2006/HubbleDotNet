using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Analysis
{
    public class SimpleAnalyzer : IAnalyzer
    {
        #region IAnalyzer Members

        public IEnumerable<Hubble.Core.Entity.WordInfo> Tokenize(string text)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
