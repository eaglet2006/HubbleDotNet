using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.Parse
{
    interface IDistinct
    {
        /// <summary>
        /// Distinct
        /// </summary>
        /// <param name="result">input result</param>
        /// <param name="table">output distinct informations to datatable</param>
        /// <returns>return result distincted</returns>
        Query.DocumentResultForSort[] Distinct(Query.DocumentResultForSort[] result, out Hubble.Framework.Data.DataTable table);
    }
}
