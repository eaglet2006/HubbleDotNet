using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hubble.Core.Data;
using Hubble.Core.SFQL.SyntaxAnalysis.Select;

namespace Hubble.Core.Query.Optimize
{
    class OptimizeArgumentGenerator
    {
        private OptimizeArgument _Argumnet;
        internal OptimizeArgument Argument 
        {
            get
            {
                return _Argumnet;
            }
        }

        internal OptimizeArgumentGenerator(DBProvider dbProvider, int end, 
            string orderBy, List<OrderBy> orderBys, bool needGroupBy, bool orderByCanBeOptimized)
        {
            _Argumnet = new OptimizeArgument();
            _Argumnet.DBProvider = dbProvider;
            _Argumnet.End = end;
            _Argumnet.OrderBy = orderBy;
            _Argumnet.NeedGroupBy = needGroupBy;
            _Argumnet.OrderByCanBeOptimized = orderByCanBeOptimized;

            if (orderBy != null)
            {
                _Argumnet.OrderBys = orderBys.ToArray();
            }
            else
            {
                _Argumnet.OrderBys = null;
            }

            _Argumnet.Prepare();
        }
    }
}
