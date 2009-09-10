using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Analysis;
using Hubble.Core.Data;
using PanGu;

namespace Hubble.Analyzer
{
    public class PanGuAnalyzer : IAnalyzer, INamedExternalReference
    {
        #region IAnalyzer Members

        public IEnumerable<Hubble.Core.Entity.WordInfo> Tokenize(string text)
        {
            PanGu.Segment segment = new Segment();
            foreach (PanGu.WordInfo wi in segment.DoSegment(text))
            {
                yield return new Hubble.Core.Entity.WordInfo(wi.Word, wi.Position, wi.Rank);
            }
        }

        #endregion

        #region IAnalyzer Members

        public void Init()
        {
            PanGu.Segment.Init();
        }

        #endregion

        #region INamedExternalReference Members

        public string Name
        {
            get 
            {
                return "PanGuSegment";
            }
        }

        #endregion
    }
}
