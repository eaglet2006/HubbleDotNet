using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hubble.Core.SFQL.SyntaxAnalysis.Select;

namespace Hubble.Core.SFQL.Parse
{
    class ParseWhereGenerator
    {
        internal ParseWhereGenerator(ParseWhere parseWhere, ParseOptimize parseOptimizor, 
            int begin, int end)
            :this(parseWhere, parseOptimizor, begin, end, false, false, null, null) 
        {

        }

        internal ParseWhereGenerator(ParseWhere parseWhere, ParseOptimize parseOptimizor, 
            int begin, int end, bool needOrderBy, bool needDistinct,
            Dictionary<int, int> notInDict, List<OrderBy> orderBys)
        {
            parseWhere.Begin = begin;
            parseWhere.End = end;

            if (parseWhere.Begin < 0)
            {
                //Means only return count
                parseWhere.Begin = 0;
                parseWhere.End = 0;
            }

            parseWhere.NeedGroupBy = needOrderBy;
            parseWhere.NeedDistinct = needDistinct;
            parseWhere.NotInDict = notInDict;
            parseWhere.OrderBys = orderBys;
            parseWhere.ComplexTree = parseOptimizor.ComplexTree;
            parseWhere.UntokenizedTreeOnRoot = parseOptimizor.UntokenizedTreeOnRoot;
        }
    }
}
