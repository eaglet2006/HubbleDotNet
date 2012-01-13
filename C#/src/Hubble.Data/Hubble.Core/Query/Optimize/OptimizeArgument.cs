using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hubble.Core.Data;
using Hubble.Core.SFQL.SyntaxAnalysis.Select;
using Hubble.Core.SFQL.SyntaxAnalysis;

namespace Hubble.Core.Query.Optimize
{
    public class OptimizeArgument
    {
        public DBProvider DBProvider { get; set; }

        public int End { get; set; }

        public string OrderBy { get; set; }

        public OrderBy[] OrderBys { get; set; }

        public bool NeedGroupBy { get; set; }

        public bool NeedFilterUntokenizedConditions {get; set;}

        public ExpressionTree UntokenizedTreeOnRoot { get; set; }

        private bool _OrderByCanBeOptimized = false;

        /// <summary>
        ///Can be the order by optimized 
        /// </summary>
        public bool OrderByCanBeOptimized
        {
            get
            {
                return _OrderByCanBeOptimized;
            }

            set
            {
                _OrderByCanBeOptimized = value;
            }
        }

        /// <summary>
        /// Can the order by be optimized.
        /// </summary>
        static public bool GetOrderByCanBeOptimized(List<OrderBy> orderBys, DBProvider dbProvider)
        {
            if (orderBys == null)
            {
                return false;
            }

            return GetOrderByCanBeOptimized(orderBys.ToArray(), dbProvider);
        }

        /// <summary>
        /// Can the order by be optimized.
        /// </summary>
        /// <returns></returns>
        static public bool GetOrderByCanBeOptimized(OrderBy[] orderBys, DBProvider dbProvider)
        {
            if (orderBys.Length > 3)
            {
                return false;
            }

            if (orderBys.Length == 0)
            {
                return false;
            }

            foreach (OrderBy orderBy in orderBys)
            {
                string fieldName = orderBy.Name;

                if (fieldName.Equals("docid", StringComparison.CurrentCultureIgnoreCase) ||
                    fieldName.Equals("score", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                Data.Field field = dbProvider.GetField(fieldName);

                if (field.IndexType != Field.Index.Untokenized)
                {
                    return false;
                }

                if (field.DataType == DataType.Varchar ||
                    field.DataType == DataType.NChar ||
                    field.DataType == DataType.Data ||
                    field.DataType == DataType.Char ||
                    field.DataType == DataType.NChar)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsOrderByScoreDesc()
        {
            if (OrderBys.Length == 1)
            {
                if (OrderBys[0].Name.Equals("score", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (OrderBys[0].Order.Equals("desc", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsOrderByScoreDesc(IList<OrderBy> orderbys)
        {
            if (orderbys == null)
            {
                return false;
            }

            if (orderbys.Count == 1)
            {
                if (orderbys[0].Name.Equals("score", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (orderbys[0].Order.Equals("desc", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Prepare()
        {
            //if (OrderBys == null)
            //{
            //    _OrderByCanBeOptimized = false;
            //}
            //else
            //{
            //    _OrderByCanBeOptimized = GetOrderByCanBeOptimized(OrderBys, DBProvider);
            //}
        }
    }
}
