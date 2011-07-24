using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.Data;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Query.Optimize
{
    unsafe interface IQueryOptimize
    {
        DBProvider DBProvider { get; set; }

        int End { get; set; }

        string OrderBy { get; set; }

        bool NeedGroupBy { get; set; }

        unsafe void CalculateOneWordOptimize(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery wifq);
    }
}
