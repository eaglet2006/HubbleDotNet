using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Data;

namespace Hubble.Core.SFQL.Parse
{
    class ParseWhere
    {
        string _TableName;
        DBProvider _DBProvider;

        public int Begin = 0;
        public int End = 99;

        public ParseWhere(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ParseException("Table name is empty!");
            }

            _TableName = tableName;

            _DBProvider =  DBProvider.GetDBProvider(_TableName);

            if (_DBProvider == null)
            {
                throw new ParseException(string.Format("Table name={0} does not exist!", _TableName));
            }
        }

        private Dictionary<long, Query.DocumentResult> GetResultFromDatabase(
            SyntaxAnalysis.ExpressionTree expressionTree)
        {
            return null;
        }

        private Dictionary<long, Query.DocumentResult> GetResultFromQuery(
            SyntaxAnalysis.ExpressionTree expressionTree, Dictionary<long, Query.DocumentResult> upDict)
        {
            Dictionary<long, Query.DocumentResult> orDict = new Dictionary<long, Hubble.Core.Query.DocumentResult>();
            Dictionary<long, Query.DocumentResult> andDict = new Dictionary<long, Hubble.Core.Query.DocumentResult>();

            if (expressionTree.Expression.NeedReverse)
            {
                SyntaxAnalysis.ExpressionTree cur = expressionTree.Expression 
                    as SyntaxAnalysis.ExpressionTree;

                if (cur.OrChild != null)
                {
                    orDict = InnerParse(cur.OrChild);
                }

                andDict = GetResultFromQuery(cur, upDict);

                return MergeDict(andDict, orDict);
            }

            if (!expressionTree.Expression.NeedTokenize)
            {
                return GetResultFromDatabase(expressionTree);
            }
            else
            {
                SyntaxAnalysis.Expression cur = 
                    expressionTree.Expression as SyntaxAnalysis.Expression;


                Hubble.Core.Query.IQuery query;

                string[] words = cur.Right[0].Text.Split(new string[] { " " }, StringSplitOptions.None);

                if (words.Length <= 0)
                {
                    throw new ParseException("Empty match string");
                }

                string fieldName = cur.Left[0].Text;

                query = DBProvider.GetQuery(cur.Operator.Text);

                if (query == null)
                {
                    throw new ParseException(string.Format("Can't find the command: {0}", cur.Operator.ToString()));
                }

                query.InvertedIndex = _DBProvider.GetInvertedIndex(fieldName);

                if (query.InvertedIndex == null)
                {
                    throw new ParseException(string.Format("Field: {0} is not Tokenized field!", fieldName));
                }


                List<Hubble.Core.Entity.WordInfo> queryWords = new List<Hubble.Core.Entity.WordInfo>();

                int position = 0;

                for (int i = 0; i < words.Length; i++)
                {
                    Hubble.Core.Entity.WordInfo wordInfo = new Hubble.Core.Entity.WordInfo(words[i], position);
                    queryWords.Add(wordInfo);
                    position += words.Length + 1;
                }

                query.QueryWords = queryWords;
                query.DBProvider = _DBProvider;
                query.TabIndex = _DBProvider.GetField(fieldName).TabIndex;

                Hubble.Core.Query.Searcher searcher = new Hubble.Core.Query.Searcher(query);
                return searcher.Search();
            }

        }

        bool GetComparisionExpressionValue(Query.DocumentResult docResult, SyntaxAnalysis.ExpressionTree expressionTree)
        {
            if (docResult.Payload == null)
            {
                Payload payLoad = _DBProvider.GetPayload(docResult.DocId);

                if (payLoad == null)
                {
                    return false;
                }

                docResult.Payload = payLoad.Data;
            }

            if (expressionTree.OrChild != null)
            {
                if (GetComparisionExpressionValue(docResult, expressionTree.OrChild))
                {
                    return true;
                }
            }

            if (expressionTree.Expression.NeedReverse)
            {
                if (!GetComparisionExpressionValue(docResult, expressionTree.Expression as SyntaxAnalysis.ExpressionTree))
                {
                    return false;
                }
            }
            else
            {
                SyntaxAnalysis.Expression cur = expressionTree.Expression as SyntaxAnalysis.Expression;

                switch (cur.DataType)
                {
                    case DataType.Date:
                    case DataType.SmallDateTime:
                    case DataType.Int32:
                        #region Date, SmallDateTime and Int32
                        switch (cur.Operator.SyntaxType)
                        {
                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Lessthan:
                                if (docResult.Payload[cur.FieldTab] >= cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LessthanEqual:
                                if (docResult.Payload[cur.FieldTab] > cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Largethan:
                                if (docResult.Payload[cur.FieldTab] <= cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LargethanEqual:
                                if (docResult.Payload[cur.FieldTab] < cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Equal:
                                if (docResult.Payload[cur.FieldTab] != cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.NotEqual:
                                if (docResult.Payload[cur.FieldTab] == cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;
                        }
                        #endregion
                        break;
                    case DataType.Int64:
                    case DataType.DateTime:
                        #region DateTime, Int64

                        long leftLong = (((long)docResult.Payload[cur.FieldTab]) << 32) + (uint)docResult.Payload[cur.FieldTab + 1];
                        long rightLong = (((long)cur.ComparisionData[0]) << 32) + (uint)cur.ComparisionData[1];

                        switch (cur.Operator.SyntaxType)
                        {
                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Lessthan:
                                if (leftLong >= rightLong)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LessthanEqual:
                                if (leftLong > rightLong)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Largethan:
                                if (leftLong <= rightLong)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LargethanEqual:
                                if (leftLong < rightLong)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Equal:
                                if (leftLong != rightLong)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.NotEqual:
                                if (leftLong == rightLong)
                                {
                                    return false;
                                }
                                break;
                        }
                        #endregion

                        break;
                    case DataType.Float:
                    case DataType.String:
                        #region Float and String
                        switch (cur.Operator.SyntaxType)
                        {
                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Lessthan:
                                {
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)docResult.Payload[i] >= (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LessthanEqual:
                                {
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)docResult.Payload[i] > (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Largethan:
                                {
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)docResult.Payload[i] <= (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LargethanEqual:
                                {
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)docResult.Payload[i] < (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Equal:
                                {
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)docResult.Payload[i] != (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                    }
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.NotEqual:
                                {
                                    bool equl = true;
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)docResult.Payload[i] != (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            equl = false;
                                            break;
                                        }
                                    }

                                    if (equl)
                                    {
                                        return false;
                                    }
                                }
                                break;


                        }
                        #endregion
                        break;
                }

            }

            if (expressionTree.AndChild != null)
            {
                if (!GetComparisionExpressionValue(docResult, expressionTree.AndChild))
                {
                    return false;
                }
            }

            return true;
        }


        private void RemoveByPayload(SyntaxAnalysis.ExpressionTree expressionTree, Dictionary<long, Query.DocumentResult> upDict)
        {
            Preprocess(expressionTree);

            List<long> delDocIdList = new List<long>();

            foreach (Query.DocumentResult docResult in upDict.Values)
            {
                if (!GetComparisionExpressionValue(docResult, expressionTree))
                {
                    delDocIdList.Add(docResult.DocId);
                }
            }

            foreach (long docId in delDocIdList)
            {
                upDict.Remove(docId);
            }
        }

        private void Preprocess(SyntaxAnalysis.ExpressionTree expressionTree)
        {
            if (expressionTree.OrChild != null)
            {
                Preprocess(expressionTree.OrChild);
            }

            if (expressionTree.Expression.NeedReverse)
            {
                Preprocess(expressionTree.Expression as SyntaxAnalysis.ExpressionTree);
            }
            else
            {
                SyntaxAnalysis.Expression cur = expressionTree.Expression as SyntaxAnalysis.Expression; 
                string fieldName = cur.Left[0].Text;
                Field field = _DBProvider.GetField(fieldName);

                if (field == null)
                {
                    throw new ParseException(string.Format("Unknow Field: {0}", fieldName));
                }

                if (field.IndexType != Field.Index.Untokenized)
                {
                    throw new ParseException(string.Format("Field: {0} is not Untokenized field!", fieldName));
                }

                string value = cur.Right[0].Text;

                cur.FieldTab = field.TabIndex;
                cur.PayloadLength = DataTypeConvert.GetDataLength(field.DataType, field.DataLength); 
                cur.ComparisionData = DataTypeConvert.GetData(field.DataType, field.DataLength, value);
            }

            if (expressionTree.AndChild != null)
            {
                Preprocess(expressionTree.AndChild);
            }

        }

        private Dictionary<long, Query.DocumentResult> MergeDict(Dictionary<long, Query.DocumentResult> and, Dictionary<long, Query.DocumentResult> or)
        {
            Dictionary<long, Query.DocumentResult> src;
            Dictionary<long, Query.DocumentResult> dest;

            if (and.Count > or.Count)
            {
                src = or;
                dest = and;
            }
            else
            {
                src = and;
                dest = or;
            }

            foreach (Query.DocumentResult docResult in src.Values)
            {
                Query.DocumentResult dr;
                if (dest.TryGetValue(docResult.DocId, out dr))
                {
                    dr.Score += docResult.Score;
                    if (dr.Payload == null && docResult.Payload != null)
                    {
                        dr.Payload = docResult.Payload;
                    }
                }
                else
                {
                    dest.Add(docResult.DocId, docResult);
                }
            }

            return dest;
        }

        private Dictionary<long, Query.DocumentResult> InnerParse(SyntaxAnalysis.ExpressionTree expressionTree)
        {
            Dictionary<long, Query.DocumentResult> orDict = new Dictionary<long,Hubble.Core.Query.DocumentResult>();
            Dictionary<long, Query.DocumentResult> andDict = new Dictionary<long, Hubble.Core.Query.DocumentResult>();

            if (expressionTree.OrChild != null)
            {
                orDict = InnerParse(expressionTree.OrChild);
            }

            if (!expressionTree.Expression.NeedTokenize)
            {
                return GetResultFromDatabase(expressionTree);
            }
            else
            {
                SyntaxAnalysis.ExpressionTree cur = expressionTree;
                while (cur != null)
                {
                    andDict = GetResultFromQuery(cur, andDict);

                    cur = cur.AndChild;

                    if (cur != null)
                    {
                        if (!cur.Expression.NeedTokenize)
                        {
                            break;
                        }
                    }
                }

                if (cur != null)
                {
                    RemoveByPayload(cur, andDict);
                }

                return MergeDict(andDict, orDict);
            }
        }

        public Query.DocumentResult[] Parse(SyntaxAnalysis.ExpressionTree expressionTree)
        {
            Dictionary<long, Query.DocumentResult> dict;

            if (!expressionTree.NeedTokenize)
            {
                dict = GetResultFromDatabase(expressionTree);
            }
            else
            {
                dict = InnerParse(expressionTree);
            }
            //Sort

            Query.DocumentResult[] result = new Hubble.Core.Query.DocumentResult[dict.Count];

            int i = 0;
            foreach (Query.DocumentResult docResult in dict.Values)
            {
                result[i++] = docResult;
            }

            return result;
        }
    }
}
