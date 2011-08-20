using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.Data;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Query.Optimize
{
    class QueryOptimizeBuilder
    {
        static internal IQueryOptimize Build(Type optimizeType, 
            DBProvider dbProvider, int end, string orderBy, bool needGroupBy,  
            WordIndexForQuery[] wordIndexes)
        {
            IQueryOptimize result = Hubble.Framework.Reflection.Instance.CreateInstance(optimizeType) as IQueryOptimize;

            if (result == null)
            {
                throw new Hubble.Core.SFQL.Parse.ParseException(string.Format("Optimize Type:{0} is not implemented by IQueryOptimize",
                    optimizeType.FullName));
            }

            result.DBProvider = dbProvider;
            result.End = end;
            result.OrderBy = orderBy;
            result.WordIndexes = wordIndexes;
            result.NeedGroupBy = needGroupBy;

            return result;
        }
    }
}
