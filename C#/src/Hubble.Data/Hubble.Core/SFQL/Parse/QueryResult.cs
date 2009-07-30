using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.Parse
{
    [Serializable]
    public class QueryResult
    {
        public System.Data.DataSet DataSet = new System.Data.DataSet();

        public List<string> PrintMessages = new List<string>();
        
        public QueryResult()
        { 
        }

        public QueryResult(string printMessage, System.Data.DataSet dataSet)
        {
            PrintMessages.Add(printMessage);
            DataSet = dataSet;
        }

        public QueryResult(System.Data.DataSet dataSet)
        {
            DataSet = dataSet;
        }

        public QueryResult(string printMessage)
        {
            PrintMessages.Add(printMessage);
        }
    }
}
