using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.Data;
using Hubble.Framework.DataStructure;
using Hubble.Core.SFQL.SyntaxAnalysis.Select;

namespace Hubble.Core.Query.Optimize
{
    class QueryOptimizeBuilder
    {
        static internal IQueryOptimize Build(Type optimizeType, 
            DBProvider dbProvider, int end, string orderBy, List<OrderBy> orderBys,
            bool needGroupBy, bool orderByCanBeOptimized, WordIndexForQuery[] wordIndexes)
        {
            IQueryOptimize result = Hubble.Framework.Reflection.Instance.CreateInstance(optimizeType) as IQueryOptimize;

            if (result == null)
            {
                throw new Hubble.Core.SFQL.Parse.ParseException(string.Format("Optimize Type:{0} is not implemented by IQueryOptimize",
                    optimizeType.FullName));
            }

            OptimizeArgumentGenerator generator = new OptimizeArgumentGenerator(dbProvider, end,
                orderBy, orderBys, needGroupBy, orderByCanBeOptimized);

            result.Argument = generator.Argument;

            result.WordIndexes = wordIndexes;

            return result;
        }
    }
}
