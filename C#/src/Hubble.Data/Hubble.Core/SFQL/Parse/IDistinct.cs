using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Core.Data;

namespace Hubble.Core.SFQL.Parse
{
    public interface IDistinct
    {
        /// <summary>
        /// the start row number of select.
        /// It is n in following sql:
        /// select between n to m * from table
        /// </summary>
        int StartRow { get; set; }

        /// <summary>
        /// the end row number of select.
        /// It is m in following sql:
        /// select between n to m * from table
        /// </summary>
        int EndRow { get; set; }
        
        /// <summary>
        /// TSFQL attribute
        /// </summary>
        TSFQLAttribute Attribute { get; set; }
        
        /// <summary>
        /// Database Provider
        /// </summary>
        DBProvider DBProvider { get; set; }
        
        /// <summary>
        /// Expression Tree
        /// </summary>
        SyntaxAnalysis.ExpressionTree ExpressionTree { get; set; }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <param name="result">input result</param>
        /// <param name="table">output distinct informations to datatable</param>
        /// <returns>return result distincted</returns>
        Query.DocumentResultForSort[] Distinct(Query.DocumentResultForSort[] result, out Hubble.Framework.Data.DataTable table);
    }
}
