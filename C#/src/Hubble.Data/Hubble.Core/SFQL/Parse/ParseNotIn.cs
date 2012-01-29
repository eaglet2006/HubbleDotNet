using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.SFQL.SyntaxAnalysis;

namespace Hubble.Core.SFQL.Parse
{
    class ParseNotIn
    {
        Dictionary<int, int> _NotInDict = null;

        internal Dictionary<int, int> NotInDict
        {
            get
            {
                return _NotInDict;
            }
        }

        internal ParseNotIn(TSFQLSentence sentence)
        {
            foreach (TSFQLAttribute attribute in sentence.Attributes)
            {
                if (attribute.Name.Equals("NotIn", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (attribute.Parameters.Count > 0)
                    {
                        ParseSQLToDict(attribute.Parameters[0]);
                    }
                }
            }
        }

        private void ParseSQLToDict(string sql)
        {
            if (_NotInDict == null)
            {
                _NotInDict = new Dictionary<int, int>();
            }

            SFQLParse sfqlParse = new SFQLParse();
            Hubble.SQLClient.QueryResult qResult = sfqlParse.Query(sql);

            if (qResult.DataSet != null)
            {
                if (qResult.DataSet.Tables != null)
                {
                    if (qResult.DataSet.Tables.Count > 0)
                    {
                        foreach (Hubble.Framework.Data.DataRow row in qResult.DataSet.Tables[0].Rows)
                        {
                            int docid = int.Parse(row["docid"].ToString());
                            if (!_NotInDict.ContainsKey(docid))
                            {
                                _NotInDict.Add(docid, 0);
                            }
                        }
                    }
                }
            }

        }

    }
}
