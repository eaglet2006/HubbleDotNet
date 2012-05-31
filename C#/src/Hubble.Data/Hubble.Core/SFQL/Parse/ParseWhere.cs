/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Data;

using Hubble.Core.Entity;
using Hubble.Core.SFQL.SyntaxAnalysis.Select;
using Hubble.Core.SFQL.SyntaxAnalysis;

namespace Hubble.Core.SFQL.Parse
{
    class ParseWhere
    {
        enum QueryStringState
        {
            Word = 0,
            Boost = 1,
            Position = 2,
            Flag = 3,
        }

        string _TableName;
        DBProvider _DBProvider;

        public int Begin = 0;
        public int End = 99;

        private bool _OrderByScore;

        public bool OrderByScore
        {
            get
            {
                return _OrderByScore;
            }
        }

        private bool _NeedGroupBy;

        public bool NeedGroupBy
        {
            get
            {
                return _NeedGroupBy;
            }

            set
            {
                _NeedGroupBy = value;
            }
        }

        public List<OrderBy> OrderBys { get; set; }

        private bool _NeedDistinct;

        public bool NeedDistinct
        {
            get
            {
                return _NeedDistinct;
            }

            set
            {
                _NeedDistinct = value;
            }
        }

        public bool ComplexTree = false;

        public ExpressionTree UntokenizedTreeOnRoot = null;

        private string _OrderBy;

        private Dictionary<int, int> _NotInDict = null;

        private SyntaxAnalysis.ExpressionTree _NoneTokenizedAndTree = null; //the first and child is not including full text search operator.
        private bool _ComplexExpression = false; //is it a complex expression.

        /// <summary>
        /// Key is docid
        /// Value is 0
        /// </summary>
        internal Dictionary<int, int> NotInDict
        {
            get
            {
                return _NotInDict;
            }

            set
            {
                _NotInDict = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="merged">
        /// if expression hasn't or child, output the merged expression. It means the expression can be 
        /// merged.
        /// Like (a match 'xxx' and b match 'yyy') can merged to a match 'xxx' and b match 'yyy'
        /// else
        /// output null.
        /// </param>
        private void OptimizeReverse(SyntaxAnalysis.IExpression expression,
            out SyntaxAnalysis.IExpression merged)
        {
            merged = null;

            if (expression == null)
            {
                return;
            }

            if (expression is SyntaxAnalysis.ExpressionTree)
            {
                SyntaxAnalysis.ExpressionTree tree = expression as SyntaxAnalysis.ExpressionTree;
                if (tree.OrChild == null && tree.AndChild == null)
                {
                    merged = tree.Expression;
                }

                SyntaxAnalysis.IExpression myMerged;
                OptimizeReverse(tree.AndChild, out myMerged);

                if (tree.AndChild != null)
                {
                    if (!tree.AndChild.NeedTokenize)
                    {
                        _ComplexExpression = true;
                    }
                }

                OptimizeReverse(tree.Expression, out myMerged);

                if (myMerged != null)
                {
                    tree.Expression = myMerged;
                }

                OptimizeReverse(tree.OrChild, out myMerged);
            }

            return;

            //if (expression
        }

        /// <summary>
        /// This is the entry to optimize the expression.
        /// </summary>
        /// <param name="expressionTree">input the expression tree and optimized it.</param>
        internal void OptimizeExpression(SyntaxAnalysis.ExpressionTree expressionTree)
        {
            if (expressionTree == null)
            {
                return;
            }

            SyntaxAnalysis.IExpression merged;

            OptimizeReverse(expressionTree.AndChild, out merged);

            if (expressionTree.AndChild != null)
            {
                if (!expressionTree.AndChild.NeedTokenize)
                {
                    _NoneTokenizedAndTree = expressionTree.AndChild;

                    //in first and child, have none tokenized tree is not complex expression
                    if (_ComplexExpression)
                    {
                        _ComplexExpression = false;
                    }
                }
            }

            OptimizeReverse(expressionTree.Expression, out merged);

            if (merged != null)
            {
                expressionTree.Expression = merged;
            }

            OptimizeReverse(expressionTree.OrChild, out merged);

        }

        public ParseWhere(string tableName, DBProvider dbProvider, bool orderbyScore, string orderBy)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ParseException("Table name is empty!");
            }

            _TableName = tableName;
            _OrderByScore = orderbyScore;
            _OrderBy = orderBy;

            //_DBProvider =  DBProvider.GetDBProvider(_TableName);
            _DBProvider = dbProvider;

            if (_DBProvider == null)
            {
                throw new ParseException(string.Format("Table name={0} does not exist!", _TableName));
            }
        }

        private Core.SFQL.Parse.DocumentResultWhereDictionary GetResultFromDatabase(
            SyntaxAnalysis.ExpressionTree expressionTree)
        {
            if (NotInDict != null)
            {
                throw new ParseException("Can't do NotIn for sql without fulltext conditions currently.");
            }

            string whereSql;

            if (expressionTree == null)
            {
                whereSql = "";
            }
            else
            {
                whereSql = expressionTree.SqlText;
            }

            string orderBy = _OrderBy;
            int end = End;

            if (!string.IsNullOrEmpty(_OrderBy))
            {
                if (_OrderBy.ToLower().Contains("score"))
                {
                    orderBy = null;
                    end = -1;
                }
                else if (_DBProvider.Table.IndexOnly && _DBProvider.Table.DocIdReplaceField != null &&
                    _OrderBy.ToLower().Contains("docid"))
                {
                    orderBy = null;
                    end = -1;
                }
            }
            else
            {
                if (whereSql == "")
                {
                    if (_DBProvider.Table.DocIdReplaceField != null)
                    {
                        orderBy = _DBProvider.Table.DocIdReplaceField;
                    }
                    else
                    {
                        orderBy = "docid";
                    }
                }
            }

            Query.PerformanceReport performanceReport;
            if (_DBProvider.Table.UsingMirrorTableForNonFulltextQuery && _DBProvider.MirrorDBAdapter != null)
            {
                performanceReport = new Hubble.Core.Query.PerformanceReport(string.Format("Non-fulltext query from Mirror table, where={0}, orderby={1}, end={2}",
                    whereSql, orderBy, end));
            }
            else
            {
                performanceReport = new Hubble.Core.Query.PerformanceReport(string.Format("Non-fulltext query from DB, where={0}, orderby={1}, end={2}",
                    whereSql, orderBy, end));
            }

            Core.SFQL.Parse.DocumentResultWhereDictionary result;

            if (_DBProvider.Table.UsingMirrorTableForNonFulltextQuery && _DBProvider.MirrorDBAdapter != null)
            {
                if (this._NeedDistinct)
                {
                    result = _DBProvider.MirrorDBAdapter.GetDocumentResults(-1, whereSql, orderBy);
                }
                else
                {
                    result = _DBProvider.MirrorDBAdapter.GetDocumentResults(end, whereSql, orderBy);
                }
            }
            else
            {
                if (this._NeedDistinct)
                {
                    result = _DBProvider.DBAdapter.GetDocumentResults(-1, whereSql, orderBy);
                }
                else
                {
                    result = _DBProvider.DBAdapter.GetDocumentResults(end, whereSql, orderBy);
                }
            }

            if (orderBy != null)
            {
                result.Sorted = true;
            }

            performanceReport.Stop();
            return result;
        }

        internal static List<Entity.WordInfo> GetWordInfoList(string queryStr)
        {
            List<Entity.WordInfo> result = new List<Hubble.Core.Entity.WordInfo>();

            string[] words = queryStr.Split(new string[] { " " }, StringSplitOptions.None);

            if (words.Length <= 0)
            {
                throw new ParseException("Empty match string");
            }

            int lastPosition = 0;
            
            foreach (string word in words)
            {
                QueryStringState state = QueryStringState.Word;

                string w = word;
                int boost = 1;
                int position = -1;
                int flag = 0;
                int begin = 0;

                for (int i = 0; i < word.Length; i++)
                {
                    switch (state)
                    {
                        case QueryStringState.Word:
                            if (word[i] == '^')
                            {
                                state = QueryStringState.Boost;
                                w = word.Substring(begin, i - begin);
                                begin = i + 1;
                            }

                            break;
                        case QueryStringState.Boost:
                            if (word[i] == '^')
                            {
                                state = QueryStringState.Position;
                                boost = int.Parse(word.Substring(begin, i - begin));
                                begin = i + 1;
                            }

                            break;
                        case QueryStringState.Position:
                            if (word[i] == '^')
                            {
                                state = QueryStringState.Flag;
                                position = int.Parse(word.Substring(begin, i - begin));
                                begin = i + 1;
                            }
                            break;

                        case QueryStringState.Flag:
                            break;
                    }
                }

                switch (state)
                {
                    case QueryStringState.Word:
                        w = word;
                        break;
                    case QueryStringState.Boost:
                        boost = int.Parse(word.Substring(begin, word.Length - begin));
                        break;
                    case QueryStringState.Position:
                        position = int.Parse(word.Substring(begin, word.Length - begin));
                        break;
                    case QueryStringState.Flag:
                        flag = int.Parse(word.Substring(begin, word.Length - begin));
                        break;
                }

                if (boost < 0 || boost > 65535)
                {
                    throw new ParseException("Boost must be between 0 to 65535!");
                }

                if (position < 0)
                {
                    position = lastPosition;
                }

                result.Add(new Hubble.Core.Entity.WordInfo(w, position, boost, (WordInfo.Flag)flag));

                lastPosition = position + w.Length;
            }

            return result;
        }

        private Core.SFQL.Parse.DocumentResultWhereDictionary GetResultFromQuery(
            SyntaxAnalysis.ExpressionTree expressionTree, Core.SFQL.Parse.DocumentResultWhereDictionary upDict)
        {

            Core.SFQL.Parse.DocumentResultWhereDictionary orDict = null;
            Core.SFQL.Parse.DocumentResultWhereDictionary andDict = null;

            if (expressionTree.Expression.NeedReverse)
            {
                SyntaxAnalysis.ExpressionTree cur = expressionTree.Expression 
                    as SyntaxAnalysis.ExpressionTree;

                if (cur.OrChild != null)
                {
                    if (!cur.OrChild.OrChildCalculated) //Change at 22 July 2010
                    {
                        orDict = InnerParse(cur.OrChild);
                        cur.OrChild.OrChildCalculated = true;
                    }
                }

                //andDict = GetResultFromQuery(cur, upDict);
                andDict = InnerParse(cur, upDict); //Change at 30 oct 2009

                andDict = MergeDict(andDict, orDict); //Change at 22 July 2010

                //Change at 22 July 2010
                if (upDict != null && cur.OrChild != null) 
                {
                    if (upDict.Count > 0)
                    {
                        return AndMergeDict(andDict, upDict);
                    }
                }

                return andDict;
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

                string fieldName = cur.Left[0].Text;

                int fieldRank = 1;

                if (cur.Left.Count == 3)
                {
                    if (cur.Left[1].SyntaxType == Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Up)
                    {
                        fieldRank = int.Parse(cur.Left[2].Text);

                        if (fieldRank < 0 || fieldRank > 65535)
                        {
                            throw new ParseException("Field boost must be from 0 to 65535");
                        }
                    }
                }

                query = DBProvider.GetQuery(cur.Operator.Text);

                if (query == null)
                {
                    throw new ParseException(string.Format("Can't find the command: {0}", cur.Operator.ToString()));
                }

                if (cur.Left.Count == 2)
                {
                    //not match, not like, not contain eg.
                    if (cur.Left[1].SyntaxType == Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.NOT)
                    {
                        query.QueryParameter.Not = true;
                    }
                }

                query.QueryParameter.End = this.End;
                query.QueryParameter.NeedGroupBy = this.NeedGroupBy;
                query.QueryParameter.NeedDistinct = this.NeedDistinct;
                query.QueryParameter.FieldRank = fieldRank;
                query.FieldName = fieldName;
                query.QueryParameter.OrderByCanBeOptimized = Hubble.Core.Query.Optimize.OptimizeArgument.GetOrderByCanBeOptimized(
                    this.OrderBys, _DBProvider);

                query.QueryParameter.CanLoadPartOfDocs = query.QueryParameter.OrderByCanBeOptimized && !_ComplexExpression;
                //query.QueryParameter.CanLoadPartOfDocs = OrderByScore && !_ComplexExpression;
                query.QueryParameter.OrderBy = _OrderBy;
                query.QueryParameter.OrderBys = this.OrderBys;
                query.QueryParameter.NoAndExpression = expressionTree.AndChild == null && _NoneTokenizedAndTree == null;
                query.QueryParameter.ComplexTree = this.ComplexTree;
                query.QueryParameter.UntokenizedTreeOnRoot = this.UntokenizedTreeOnRoot;

                query.InvertedIndex = _DBProvider.GetInvertedIndex(fieldName);


                if (query.InvertedIndex == null)
                {
                    throw new ParseException(string.Format("Field: {0} is not Tokenized field!", fieldName));
                }


                List<Hubble.Core.Entity.WordInfo> queryWords ;

                if (query is Query.LikeQuery)
                {
                    queryWords = new List<Hubble.Core.Entity.WordInfo>();
                    queryWords.Add(new Hubble.Core.Entity.WordInfo(cur.Right[0].Text, 0));
                }
                else
                {
                    queryWords = GetWordInfoList(cur.Right[0].Text);
                }


                query.UpDict = null;

                if (upDict != null)
                {
                    if (upDict.Count > 0 || upDict.ZeroResult)
                    {
                        query.UpDict = upDict;
                    }
                    else
                    {
                        upDict.Dispose();
                    }
                }

                if (query.UpDict != null)
                {
                    query.QueryParameter.CanLoadPartOfDocs = false;
                }

                query.NotInDict = NotInDict;

                query.DBProvider = _DBProvider;
                query.TabIndex = _DBProvider.GetField(fieldName).TabIndex;

                _DBProvider.MergeLock.Enter(Hubble.Framework.Threading.Lock.Mode.Share);

                try
                {
                    query.QueryWords = queryWords;

                    Hubble.Core.Query.Searcher searcher = new Hubble.Core.Query.Searcher(query);


                    return searcher.Search();
                }
                finally
                {
                    _DBProvider.MergeLock.Leave(Hubble.Framework.Threading.Lock.Mode.Share);
                }
            }

        }

        /// <summary>
        /// Get the comparision expressin value.
        /// If matched, return true.
        /// This method is used for mix where conditions that is including both full text query and 
        /// none-fulltext query.
        /// </summary>
        /// <param name="dbProvider">DBProvider</param>
        /// <param name="pDocResult">pointer to docResult</param>
        /// <param name="expressionTree"></param>
        /// <returns></returns>
        internal unsafe static bool GetComparisionExpressionValue(DBProvider dbProvider, Query.DocumentResult* pDocResult, SyntaxAnalysis.ExpressionTree expressionTree)
        {
            if (pDocResult->PayloadData == null)
            {
                int* payloadData = dbProvider.GetPayloadData(pDocResult->DocId);

                if (payloadData == null)
                {
                    return false;
                }

                pDocResult->PayloadData = payloadData;
            }

            if (expressionTree.OrChild != null)
            {
                if (GetComparisionExpressionValue(dbProvider, pDocResult, expressionTree.OrChild))
                {
                    return true;
                }
            }

            if (expressionTree.Expression.NeedReverse)
            {
                if (!GetComparisionExpressionValue(dbProvider, pDocResult, expressionTree.Expression as SyntaxAnalysis.ExpressionTree))
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
                    case DataType.TinyInt:
                    case DataType.SmallInt:
                    case DataType.Int:
                        #region Date, SmallDateTime and Int32

                        int payloadInt = pDocResult->PayloadData[cur.FieldTab];

                        if (cur.DataType == DataType.TinyInt)
                        {
                            sbyte* sbyteP = (sbyte*)&payloadInt + cur.SubTabIndex;
                            payloadInt = *sbyteP;

                        }
                        else if (cur.DataType == DataType.SmallInt)
                        {
                            sbyte* sbyteP = (sbyte*)&payloadInt + cur.SubTabIndex;
                            short al = (byte)(*sbyteP);
                            short ah = *(sbyteP + 1);
                            ah <<= 8;
                            payloadInt = al | ah;
                        }

                        switch (cur.Operator.SyntaxType)
                        {
                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Lessthan:
                                if (payloadInt >= cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LessthanEqual:
                                if (payloadInt > cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Largethan:
                                if (payloadInt <= cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LargethanEqual:
                                if (payloadInt < cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Equal:
                                if (payloadInt != cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.NotEqual:
                                if (payloadInt == cur.ComparisionData[0])
                                {
                                    return false;
                                }
                                break;
                        }
                        #endregion
                        break;
                    case DataType.BigInt:
                    case DataType.DateTime:
                        #region DateTime, Int64

                        long leftLong;

                        if (cur.FieldTab == -1)
                        {
                            //DocId
                            leftLong = pDocResult->DocId;
                        }
                        else
                        {
                            leftLong = (((long)pDocResult->PayloadData[cur.FieldTab]) << 32) + (uint)pDocResult->PayloadData[cur.FieldTab + 1];
                        }


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
                        #region Float
                        float leftFloat;
                        float rightFloat;

                        long l;
                        //Calculate left float
                        l = (((long)pDocResult->PayloadData[cur.FieldTab]) << 32) + pDocResult->PayloadData[cur.FieldTab + 1];

                        leftFloat = l;
                        l = (((long)pDocResult->PayloadData[cur.FieldTab + 2]) << 32) + pDocResult->PayloadData[cur.FieldTab + 3];

                        leftFloat += (float)l / 1000000000000000000;

                        //Calculate right float
                        l = (((long)cur.ComparisionData[cur.FieldTab - cur.FieldTab]) << 32) + cur.ComparisionData[cur.FieldTab - cur.FieldTab + 1];

                        rightFloat = l;
                        l = (((long)cur.ComparisionData[cur.FieldTab - cur.FieldTab + 2]) << 32) + cur.ComparisionData[cur.FieldTab - cur.FieldTab + 3];

                        rightFloat += (float)l / 1000000000000000000;

                        switch (cur.Operator.SyntaxType)
                        {
                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Lessthan:
                                if (leftFloat >= rightFloat)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LessthanEqual:
                                if (leftFloat > rightFloat)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Largethan:
                                if (leftFloat <= rightFloat)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LargethanEqual:
                                if (leftFloat < rightFloat)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Equal:
                                if (leftFloat != rightFloat)
                                {
                                    return false;
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.NotEqual:
                                if (leftFloat == rightFloat)
                                {
                                    return false;
                                }
                                break;
                        }

                        #endregion
                        break;
                    case DataType.Varchar:
                    case DataType.NVarchar:
                    case DataType.Char:
                    case DataType.NChar:
                        #region String
                        switch (cur.Operator.SyntaxType)
                        {
                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Lessthan:
                                {
                                    bool equal = true;

                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)pDocResult->PayloadData[i] > (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                        else if ((uint)pDocResult->PayloadData[i] < (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            equal = false;
                                            break;
                                        }
                                    }

                                    if (equal)
                                    {
                                        return false;
                                    }
                                }

                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LessthanEqual:
                                {
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)pDocResult->PayloadData[i] > (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                        else if ((uint)pDocResult->PayloadData[i] < (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            break;
                                        }
                                    }
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Largethan:
                                {
                                    bool equal = true;
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)pDocResult->PayloadData[i] < (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                        else if ((uint)pDocResult->PayloadData[i] > (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            equal = false;
                                            break;
                                        }
                                    }

                                    if (equal)
                                    {
                                        return false;
                                    }
                                }

                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.LargethanEqual:
                                {
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)pDocResult->PayloadData[i] < (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            return false;
                                        }
                                        else if ((uint)pDocResult->PayloadData[i] > (uint)cur.ComparisionData[i - cur.FieldTab])
                                        {
                                            break;
                                        }
                                    }
                                }
                                break;

                            case Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Equal:
                                {
                                    for (int i = cur.FieldTab; i < cur.FieldTab + cur.PayloadLength; i++)
                                    {
                                        if ((uint)pDocResult->PayloadData[i] != (uint)cur.ComparisionData[i - cur.FieldTab])
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
                                        if ((uint)pDocResult->PayloadData[i] != (uint)cur.ComparisionData[i - cur.FieldTab])
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
                if (!GetComparisionExpressionValue(dbProvider, pDocResult, expressionTree.AndChild))
                {
                    return false;
                }
            }

            return true;
        }

        unsafe internal static string GetInSql(Data.DBProvider dbProvider, Core.SFQL.Parse.DocumentResultWhereDictionary upDict)
        {
            StringBuilder sql = new StringBuilder();

            if (dbProvider.DocIdReplaceField == null)
            {
                sql.Append("docId in (");
            }
            else
            {
                sql.AppendFormat("{0} in (", dbProvider.DocIdReplaceField);
            }

            Dictionary<long, int> replaceFieldValueToDocId = null;

            if (dbProvider.DocIdReplaceField != null)
            {
                replaceFieldValueToDocId = new Dictionary<long, int>();
            }

            int i = 0;

            foreach (Core.SFQL.Parse.DocumentResultPoint drp in upDict.Values)
            {
                int docId = drp.pDocumentResult->DocId;

                if (dbProvider.DocIdReplaceField == null)
                {
                    if (i++ == 0)
                    {
                        sql.AppendFormat("{0}", docId);
                    }
                    else
                    {
                        sql.AppendFormat(",{0}", docId);
                    }
                }
                else
                {
                    long replaceFieldValue = dbProvider.GetDocIdReplaceFieldValue(docId);

                    if (i++ == 0)
                    {
                        sql.AppendFormat("{0}", replaceFieldValue);
                    }
                    else
                    {
                        sql.AppendFormat(",{0}", replaceFieldValue);
                    }
                }
            }

            sql.Append(")");

            return sql.ToString();
        }

        unsafe private void RemoveByDatabaseQuery(SyntaxAnalysis.ExpressionTree expressionTree, Core.SFQL.Parse.DocumentResultWhereDictionary upDict)
        {
            int upDictCount = upDict.Count;

            if (upDictCount == 0)
            {
                return;
            }

            Core.SFQL.Parse.DocumentResultWhereDictionary queryDict;

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport();

            if (upDictCount < 8196)
            {
                queryDict = _DBProvider.DBAdapter.GetDocumentResults(-1,
                    string.Format("({0}) and {1}", expressionTree.ToString(), GetInSql(_DBProvider, upDict)), null);

            }
            else
            {
                queryDict = _DBProvider.DBAdapter.GetDocumentResults(-1,
                    string.Format("({0})", expressionTree.ToString()), null);
            }

            performanceReport.Stop(string.Format("RemoveByDatabaseQuery, query count={0}, del count = {1}, where = {2}",
                upDict.Count, queryDict.Count, expressionTree.ToString()));

            if (queryDict.Count == 0)
            {
                upDict.Clear();
                return;
            }

            List<int> delDocIdList = new List<int>();

            foreach (Core.SFQL.Parse.DocumentResultPoint drp in upDict.Values)
            {
                if (!queryDict.ContainsKey(drp.pDocumentResult->DocId))
                {
                    delDocIdList.Add(drp.pDocumentResult->DocId);
                }
            }

            foreach (int docId in delDocIdList)
            {
                upDict.Remove(docId);
                upDict.RelTotalCount--;
            }
        }

        unsafe private void RemoveByPayload(SyntaxAnalysis.ExpressionTree expressionTree, Core.SFQL.Parse.DocumentResultWhereDictionary upDict)
        {

            bool needQueryFromDatabase = false;
            Preprocess(_DBProvider, expressionTree, ref needQueryFromDatabase);

            if (needQueryFromDatabase)
            {
                RemoveByDatabaseQuery(expressionTree, upDict);
                return;
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport();

            List<int> delDocIdList = new List<int>();

            foreach (Core.SFQL.Parse.DocumentResultPoint drp in upDict.Values)
            {
                if (!GetComparisionExpressionValue(_DBProvider, drp.pDocumentResult, expressionTree))
                {
                    delDocIdList.Add(drp.pDocumentResult->DocId);
                }
            }

            foreach (int docId in delDocIdList)
            {
                upDict.Remove(docId);
                upDict.RelTotalCount--;
            }

            performanceReport.Stop(string.Format("RemoveByPayload result count = {0}", delDocIdList.Count));
        }

        static internal void Preprocess(DBProvider dbProvider, SyntaxAnalysis.ExpressionTree expressionTree, ref bool needQueryFromDatabase)
        {
            if (expressionTree.HasBeenProprocessed)
            {
                if (expressionTree.NeedQueryFromDatabase)
                {
                    needQueryFromDatabase = true;
                }

                return;
            }

            if (expressionTree.OrChild != null)
            {
                Preprocess(dbProvider, expressionTree.OrChild, ref needQueryFromDatabase);
            }

            if (expressionTree.Expression.NeedReverse)
            {
                Preprocess(dbProvider, expressionTree.Expression as SyntaxAnalysis.ExpressionTree, ref needQueryFromDatabase);
            }
            else
            {
                SyntaxAnalysis.Expression cur = expressionTree.Expression as SyntaxAnalysis.Expression; 
                string fieldName = cur.Left[0].Text;
                string value;

                if (fieldName.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    value = cur.Right[0].Text;
                    cur.DataType = DataType.BigInt;
                    cur.FieldTab = -1;
                    cur.PayloadLength = 8;
                    cur.ComparisionData = DataTypeConvert.GetData(DataType.BigInt, 8, -1, value);
                }
                else
                {
                    Field field = dbProvider.GetField(fieldName);

                    if (field == null)
                    {
                        throw new ParseException(string.Format("Unknow Field: {0}", fieldName));
                    }

                    if (field.IndexType != Field.Index.Untokenized)
                    {
                        needQueryFromDatabase = true;
                        expressionTree.HasBeenProprocessed = true;
                        expressionTree.NeedQueryFromDatabase = true;

                        return;
                        //throw new ParseException(string.Format("Field: {0} is not Untokenized field!", fieldName));
                    }

                    value = cur.Right[0].Text;
                    cur.DataType = field.DataType;
                    cur.FieldTab = field.TabIndex;
                    cur.SubTabIndex = field.SubTabIndex;
                    cur.PayloadLength = DataTypeConvert.GetDataLength(field.DataType, field.DataLength);

                    cur.ComparisionData = DataTypeConvert.GetData(field.DataType, field.DataLength, field.SubTabIndex, value);

                    switch (field.DataType)
                    {
                        case DataType.TinyInt:
                        case DataType.SmallInt:
                            cur.ComparisionData[0] = DataTypeConvert.GetSubTabData(field.DataType, field.SubTabIndex,
                                cur.ComparisionData[0]);
                            break;
                    }
                }
            }

            if (expressionTree.AndChild != null)
            {
                Preprocess(dbProvider, expressionTree.AndChild, ref needQueryFromDatabase);
            }

            expressionTree.HasBeenProprocessed = true;
            expressionTree.NeedQueryFromDatabase = needQueryFromDatabase;
        }

        unsafe private Core.SFQL.Parse.DocumentResultWhereDictionary AndMergeDict(Core.SFQL.Parse.DocumentResultWhereDictionary and, Core.SFQL.Parse.DocumentResultWhereDictionary or)
        {
            if (and == or)
            {
                return and;
            }

            if (and == null)
            {
                if (or != null)
                {
                    or.Dispose();
                }

                return new Core.SFQL.Parse.DocumentResultWhereDictionary();
            }

            if (or == null)
            {
                if (and != null)
                {
                    and.Dispose();
                }

                return new DocumentResultWhereDictionary();
            }

            Core.SFQL.Parse.DocumentResultWhereDictionary src;
            Core.SFQL.Parse.DocumentResultWhereDictionary dest;

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


            Core.SFQL.Parse.DocumentResultWhereDictionary result = new DocumentResultWhereDictionary();
            result.Not = dest.Not;

            foreach (Core.SFQL.Parse.DocumentResultPoint drp in src.Values)
            {
                Query.DocumentResult* dr;
                if (dest.TryGetValue(drp.pDocumentResult->DocId, out dr))
                {
                    dr->Score += drp.pDocumentResult->Score;
                    if (dr->PayloadData == null && drp.pDocumentResult->PayloadData != null)
                    {
                        dr->PayloadData = drp.pDocumentResult->PayloadData;
                    }

                    result.Add(drp.pDocumentResult->DocId, *dr);
                }
            }

            src.Dispose();
            dest.Dispose();

            return result;
        }


        unsafe private Core.SFQL.Parse.DocumentResultWhereDictionary MergeDict(Core.SFQL.Parse.DocumentResultWhereDictionary and, Core.SFQL.Parse.DocumentResultWhereDictionary or)
        {
            if (and == or)
            {
                return and;
            }

            if (and == null)
            {
                and = new Core.SFQL.Parse.DocumentResultWhereDictionary();
            }

            if (or == null)
            {
                if (and.Not)
                {
                    and.Dispose();
                    return new DocumentResultWhereDictionary();
                }
                else
                {
                    return and;
                }

                //or = new DocumentResultWhereDictionary();
            }

            if (and.Not)
            {
                and.Dispose();
                and = new Core.SFQL.Parse.DocumentResultWhereDictionary();
            }

            if (or.Not)
            {
                or.Dispose();
                or = new Core.SFQL.Parse.DocumentResultWhereDictionary();
            }

            Core.SFQL.Parse.DocumentResultWhereDictionary src;
            Core.SFQL.Parse.DocumentResultWhereDictionary dest;

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

            if (this.NeedGroupBy)
            {
                //Merge group by collection

                dest.GroupByCollection = DocumentResultWhereDictionary.MergeOr(
                    src.GroupByCollection, dest.GroupByCollection);

                //if (src.GroupByCollection.Count > 0 || dest.GroupByCollection.Count > 0)
                //{
                //    //Init group by collection

                //    if (src.GroupByCollection.Count <= 0)
                //    {
                //        foreach (int docid in src.Keys)
                //        {
                //            src.AddToGroupByCollection(docid);
                //        }
                //    }

                //    if (dest.GroupByCollection.Count <= 0)
                //    {
                //        foreach (int docid in dest.Keys)
                //        {
                //            dest.AddToGroupByCollection(docid);
                //        }
                //    }

                //    foreach (int docid in src.GroupByCollection)
                //    {
                //        if (!dest.GroupByContains(docid))
                //        {
                //            dest.AddToGroupByCollection(docid);
                //        }
                //    }
                //}
            }

            foreach (Core.SFQL.Parse.DocumentResultPoint drp in src.Values)
            {
                Query.DocumentResult* dr;
                if (dest.TryGetValue(drp.pDocumentResult->DocId, out dr))
                {
                    dr->Score += drp.pDocumentResult->Score;
                    if (dr->PayloadData == null && drp.pDocumentResult->PayloadData != null)
                    {
                        dr->PayloadData = drp.pDocumentResult->PayloadData;
                    }
                }
                else
                {
                    dest.Add(drp.pDocumentResult->DocId, *drp.pDocumentResult);
                }
            }

            if (dest.RelTotalCount < and.RelTotalCount)
            {
                dest.RelTotalCount = and.RelTotalCount;
            }

            if (dest.RelTotalCount < or.RelTotalCount)
            {
                dest.RelTotalCount = or.RelTotalCount;
            }

            if (src != null)
            {
                src.Dispose();
            }

            dest.Sorted = false;
            return dest;
        }

        private Core.SFQL.Parse.DocumentResultWhereDictionary InnerParse(SyntaxAnalysis.ExpressionTree expressionTree)
        {
            return InnerParse(expressionTree, null);
        }

        private Core.SFQL.Parse.DocumentResultWhereDictionary InnerParse(SyntaxAnalysis.ExpressionTree expressionTree,
            Core.SFQL.Parse.DocumentResultWhereDictionary upDict)
        {
            Core.SFQL.Parse.DocumentResultWhereDictionary orDict = null;
            Core.SFQL.Parse.DocumentResultWhereDictionary andDict;

            if (upDict == null)
            {
                andDict = new Core.SFQL.Parse.DocumentResultWhereDictionary();
            }
            else
            {
                if (upDict.Count > 0)
                {
                    andDict = upDict;
                }
                else
                {
                    andDict = new Core.SFQL.Parse.DocumentResultWhereDictionary();
                }
            }

            if (expressionTree.OrChild != null)
            {
                //Change at 22 July 2010
                if (!expressionTree.OrChild.OrChildCalculated)
                {
                    orDict = InnerParse(expressionTree.OrChild);
                    expressionTree.OrChild.OrChildCalculated = true;
                }
            }

            if (!expressionTree.Expression.NeedTokenize)
            {
                if (expressionTree.OrChild != null)
                {
                    if (expressionTree.OrChild.OrChildCalculated)
                    {
                        expressionTree.OrChild = null;
                    }
                }

                andDict = GetResultFromDatabase(expressionTree);

                return MergeDict(andDict, orDict);


            }
            else
            {
                SyntaxAnalysis.ExpressionTree cur = expressionTree;
                while (cur != null)
                {
                    bool orMerged = false;

                    if (cur.OrChild != null)
                    {
                        //Change at 22 July 2010
                        if (cur.OrChild.OrChildCalculated)
                        {
                            //All or chilren finished.
                            //Merge first node and orDict
                            Core.SFQL.Parse.DocumentResultWhereDictionary fstDict;
                            fstDict = GetResultFromQuery(cur, null);
                            andDict = MergeDict(fstDict, orDict);
                            orMerged = true;
                            //andDict = fstDict.AndMerge(fstDict, andDict);
                        }
                    }

                    if (!orMerged)
                    {
                        andDict = GetResultFromQuery(cur, andDict);
                    }

                    if (andDict.Count == 0)
                    {
                        andDict.ZeroResult = true;
                    }

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

        public Query.DocumentResultForSort[] Parse(SyntaxAnalysis.ExpressionTree expressionTree, out IList<int> groupByCollection, out bool sorted)
        {
            int relTotalCount;
            return Parse(expressionTree, out relTotalCount, out groupByCollection, out sorted);
        }

        unsafe public Query.DocumentResultForSort[] Parse(SyntaxAnalysis.ExpressionTree expressionTree, 
            out int relTotalCount, out IList<int> groupByCollection, out bool sorted)
        {
            Core.SFQL.Parse.DocumentResultWhereDictionary dict;

            if (expressionTree == null)
            {
                dict = GetResultFromDatabase(expressionTree);
            }
            else if (!expressionTree.NeedTokenize)
            {
                if (expressionTree.OrChild == null && expressionTree.AndChild == null)
                {
                    Core.SFQL.SyntaxAnalysis.Expression expression = 
                        expressionTree.Expression as Core.SFQL.SyntaxAnalysis.Expression;

                    if (_DBProvider.DocIdReplaceField != null)
                    {
                        if (expression != null)
                        {
                            if (expression.Left != null)
                            {
                                if (expression.Left.Count == 1)
                                {
                                    //if the expression like: Docid = 000
                                    if (expression.Left[0].Text.Equals("DocId", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (expression.Operator.SyntaxType == Hubble.Core.SFQL.SyntaxAnalysis.SyntaxType.Equal)
                                        {
                                            int docid = int.Parse(expression.Right[0].Text);

                                            if (_DBProvider.GetDocIdReplaceFieldValue(docid) != long.MaxValue &&
                                                !_DBProvider.DelProvider.DocIdDeleted(docid))
                                            {
                                                Query.DocumentResultForSort[] dresult = new Hubble.Core.Query.DocumentResultForSort[1];
                                                dresult[0] = new Hubble.Core.Query.DocumentResultForSort(docid);
                                                relTotalCount = 1;
                                                groupByCollection = null;
                                                sorted = false;
                                                return dresult;
                                            }
                                            else
                                            {
                                                relTotalCount = 0;
                                                groupByCollection = null;
                                                sorted = false;
                                                return new Query.DocumentResultForSort[0];
                                            }
                                        }
                                    }
                                    else if (expression.Left[0].Text.Equals(_DBProvider.DocIdReplaceField, StringComparison.CurrentCultureIgnoreCase) &&
                                        expression.Operator.SyntaxType == SyntaxType.Equal)
                                    {
                                        //if the expression like: id = 000

                                        long id = long.Parse(expression.Right[0].Text);
                                        int docid = _DBProvider.GetDocIdFromDocIdReplaceFieldValue(id);

                                        if (docid != int.MinValue)
                                        {
                                            if (!_DBProvider.DelProvider.DocIdDeleted(docid))
                                            {
                                                Query.DocumentResultForSort[] dresult = new Hubble.Core.Query.DocumentResultForSort[1];
                                                dresult[0] = new Hubble.Core.Query.DocumentResultForSort(docid);
                                                relTotalCount = 1;
                                                groupByCollection = null;
                                                sorted = false;
                                                return dresult;
                                            }
                                        }

                                        relTotalCount = 0;
                                        groupByCollection = null;
                                        sorted = false;
                                        return new Query.DocumentResultForSort[0];
                                    }
                                }
                            }

                        }
                    }

                }

                //dict = GetResultFromDatabase(expressionTree);
                dict = InnerParse(expressionTree);
            }
            else
            {
                dict = InnerParse(expressionTree);
            }

            relTotalCount = dict.RelTotalCount;

            //Sort

            Query.DocumentResultForSort[] result = new Hubble.Core.Query.DocumentResultForSort[dict.Count];

            int i = 0;
            foreach (Core.SFQL.Parse.DocumentResultPoint drp in dict.Values)
            {
                result[i++] = new Hubble.Core.Query.DocumentResultForSort(drp.pDocumentResult);
            }

            groupByCollection = dict.GroupByCollection;
            sorted = dict.Sorted;

            dict.Dispose();

            return result;
        }
    }
}
