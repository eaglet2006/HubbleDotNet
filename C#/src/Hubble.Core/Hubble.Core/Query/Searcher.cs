using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Query
{
    public class Searcher
    {
        public IEnumerable<long> Search(int first, int count, IQuery query)
        {
            yield return 0;
        }
    }
}
