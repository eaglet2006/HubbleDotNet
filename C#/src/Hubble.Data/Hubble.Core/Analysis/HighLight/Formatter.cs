using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Analysis.HighLight
{
    public interface Formatter
    {
        string HighlightTerm(string originalText);
    }
}
