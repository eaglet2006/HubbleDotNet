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
using Hubble.Framework.DataStructure;
using Hubble.Core.Data;
using Hubble.Core.SFQL.Parse;


namespace Hubble.Core.Query
{
    /// <summary>
    /// This query analyze input words just using
    /// tf/idf. The poisition informations are no useful.
    /// Syntax: MutiStringQuery('xxx','yyy','zzz')
    /// </summary>
    public class LikeQuery : IQuery, INamedExternalReference
    {
        class WordIndexForQueryCompareByPositionAndRank : IComparer<WordIndexForQuery>
        {

            #region IComparer<WordIndexForQuery> Members

            public int Compare(WordIndexForQuery x, WordIndexForQuery y)
            {
                if (x.FirstPosition > y.FirstPosition)
                {
                    return 1;
                }
                else if (x.FirstPosition < y.FirstPosition)
                {
                    return -1;
                }
                else
                {
                    if (x.WordRank > y.WordRank)
                    {
                        return -1;
                    }
                    else if (x.WordRank < y.WordRank)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            #endregion
        }

        class WordIndexForQuery : IComparable<WordIndexForQuery>
        {
            public int CurDocIdIndex;
            public int WordIndexesLength;
            public int Sum_d_t;
            public int Idf_t;
            public int WordRank;
            public int FieldRank;
            public int RelTotalCount;

            public int QueryCount; //How many time is this word in query string.
            public int FirstPosition; //First position in query string.

            private int _CurIndex;

            private Index.WordIndexReader _WordIndex;

            public int CurIndex
            {
                get
                {
                    return _CurIndex;
                }

                set
                {
                    _CurIndex = value;
                }
            }


            public Index.WordIndexReader WordIndex
            {
                get
                {
                    return _WordIndex;
                }
            }

            public WordIndexForQuery(Index.WordIndexReader wordIndex,
                int totalDocuments, int wordRank, int fieldRank)
            {
                FieldRank = fieldRank;
                WordRank = wordRank;
                RelTotalCount = wordIndex.RelDocCount;

                if (FieldRank <= 0)
                {
                    FieldRank = 1;
                }

                if (WordRank <= 0)
                {
                    WordRank = 1;
                }


                if (wordIndex.Count <= 0)
                {
                    _CurIndex = -1;
                }
                else
                {
                    _CurIndex = 0;
                }

                _WordIndex = wordIndex;

                Sum_d_t = (int)Math.Sqrt(_WordIndex.WordCount);
                Idf_t = (int)Math.Log10((double)totalDocuments / (double)_WordIndex.Count + 1) + 1;
                CurDocIdIndex = 0;
                WordIndexesLength = _WordIndex.Count;
            }

            public void IncCurIndex()
            {
                CurIndex++;

                if (CurIndex >= _WordIndex.Count)
                {
                    CurIndex = -1;
                }
            }


            #region IComparable<WordIndexForQuery> Members

            public int CompareTo(WordIndexForQuery other)
            {
                return this.WordIndexesLength.CompareTo(other.WordIndexesLength);
            }

            #endregion
        }

        #region Private fields
        int MinResultCount = 32768;

        string _FieldName;
        Hubble.Core.Index.InvertedIndex _InvertedIndex;
        private int _TabIndex;
        private DBProvider _DBProvider;
        private int _TotalDocuments;
        string _LikeString = null;

        bool _QueryThroughDB = false;

        AppendList<Entity.WordInfo> _QueryWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        WordIndexForQuery[] _WordIndexes;

        #endregion

        unsafe private void CalculateWithPosition(Core.SFQL.Parse.DocumentResultWhereDictionary upDict,
            ref Core.SFQL.Parse.DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            MinResultCount = _DBProvider.Table.GroupByLimit;

            //Get max word doc list count
            int minWordDocListCount = 1 * 1024 * 1024; //1M

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                minWordDocListCount = Math.Min(minWordDocListCount, wifq.WordIndex.WordDocList.Count);
            }

            if (docIdRank.Count == 0)
            {
                if (minWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(minWordDocListCount);
                }
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            //Merge
            bool oneWordOptimize = this._QueryParameter.CanLoadPartOfDocs && this._QueryParameter.NoAndExpression && wordIndexes.Length == 1;
            int oneWordMaxCount = 0;

            if (oneWordOptimize)
            {
                //One word
                WordIndexForQuery wifq = wordIndexes[0]; //first word

                //Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                Entity.DocumentPositionList docList = wifq.WordIndex.GetNext();
                int j = 0;

                while (docList.DocumentId >= 0)
                {
                    //Entity.DocumentPositionList docList = wifq.WordIndex[j];

                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (j > MinResultCount)
                    {
                        if (oneWordMaxCount > docList.Count)
                        {
                            j++;
                            docList = wifq.WordIndex.GetNext();
                            continue;
                        }
                    }
                    else
                    {
                        if (oneWordMaxCount < docList.Count)
                        {
                            oneWordMaxCount = docList.Count;
                        }
                    }

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }


                    if (upDict == null)
                    {
                        docIdRank.Add(docList.DocumentId, score);
                    }
                    else
                    {
                        if (!upDict.Not)
                        {
                            if (upDict.ContainsKey(docList.DocumentId))
                            {
                                docIdRank.Add(docList.DocumentId, score);
                            }
                        }
                        else
                        {
                            if (!upDict.ContainsKey(docList.DocumentId))
                            {
                                docIdRank.Add(docList.DocumentId, score);
                            }
                        }
                    }

                    j++;
                    docList = wifq.WordIndex.GetNext();
                }
            }
            else
            {
                double ratio = 1;

                if (wordIndexes.Length > 1)
                {
                    ratio = (double)2 / (double)(wordIndexes.Length - 1);
                }

                int wordIndexesLen = wordIndexes.Length;

                WordIndexForQuery fstWifq = wordIndexes[0]; //first word

                Entity.DocumentPositionList fstDocList = fstWifq.WordIndex.GetNext();

                Entity.DocumentPositionList[] docListArr = new Hubble.Core.Entity.DocumentPositionList[wordIndexesLen];

                docListArr[0] = fstDocList;

                while (fstDocList.DocumentId >= 0)
                {
                    int curWord = 1;
                    int firstDocId = fstDocList.DocumentId;

                    while (curWord < wordIndexesLen)
                    {
                        docListArr[curWord] = wordIndexes[curWord].WordIndex.Get(firstDocId);

                        if (docListArr[curWord].DocumentId < 0)
                        {
                            break;
                        }

                        curWord++;
                    } //While

                    if (curWord >= wordIndexesLen)
                    {
                        //Matched
                        //Caculate score

                        long totalScore = 0;
                        Entity.DocumentPositionList lastDocList
                            = new Hubble.Core.Entity.DocumentPositionList();

                        for (int i = 0; i < wordIndexesLen; i++)
                        {
                            WordIndexForQuery wifq = wordIndexes[i];
                            Entity.DocumentPositionList docList = docListArr[i];


                            long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                            if (score < 0)
                            {
                                //Overflow
                                score = long.MaxValue - 4000000;
                            }

                            double delta = 1;

                            if (i > 0)
                            {
                                //Calculate with position
                                double queryPositionDelta = wifq.FirstPosition - wordIndexes[i - 1].FirstPosition;
                                double positionDelta = docList.FirstPosition - lastDocList.FirstPosition;

                                delta = Math.Abs(queryPositionDelta - positionDelta);

                                if (delta < 0.031)
                                {
                                    delta = 0.031;
                                }
                                else if (delta <= 1.1)
                                {
                                    delta = 0.5;
                                }
                                else if (delta <= 2.1)
                                {
                                    delta = 1;
                                }

                                delta = Math.Pow((1 / delta), ratio) * docList.Count * lastDocList.Count /
                                    (double)(wifq.QueryCount * wordIndexes[i - 1].QueryCount);
                            }

                            lastDocList = docList;

                            totalScore += (long)(score * delta);
                        }

                        if (upDict == null)
                        {
                            docIdRank.Add(firstDocId, totalScore);
                        }
                        else
                        {
                            if (!upDict.Not)
                            {
                                if (upDict.ContainsKey(firstDocId))
                                {
                                    docIdRank.Add(firstDocId, totalScore);
                                }
                            }
                            else
                            {
                                if (!upDict.ContainsKey(firstDocId))
                                {
                                    docIdRank.Add(firstDocId, totalScore);
                                }
                            }
                        }

                    }//if (curWord >= wordIndexesLen)

                    fstDocList = fstWifq.WordIndex.GetNext();
                    docListArr[0] = fstDocList;
                }

            }

            //Merge score if upDict != null
            if (upDict != null)
            {
                if (!upDict.Not)
                {
                    foreach (int docid in docIdRank.Keys)
                    {
                        DocumentResult* upDrp;

                        if (upDict.TryGetValue(docid, out upDrp))
                        {
                            DocumentResult* drpResult;
                            if (docIdRank.TryGetValue(docid, out drpResult))
                            {
                                drpResult->Score += upDrp->Score;
                            }
                        }
                    }
                }
            }

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int delCount = delProvider.Filter(docIdRank);

            if (oneWordOptimize && _QueryParameter.CanLoadPartOfDocs && upDict == null)
            {
                docIdRank.RelTotalCount = wordIndexes[0].RelTotalCount - delCount;
            }
            else
            {
                docIdRank.RelTotalCount = docIdRank.Count;
            }

            performanceReport.Stop();

        }


        unsafe private void Calculate(DocumentResultWhereDictionary upDict,
            ref DocumentResultWhereDictionary docIdRank, WordIndexForQuery[] wordIndexes)
        {
            Array.Sort(wordIndexes);

            MinResultCount = _DBProvider.Table.GroupByLimit;

            //Get max word doc list count
            int minWordDocListCount = 1 * 1024 * 1024; //1M

            foreach (WordIndexForQuery wifq in wordIndexes)
            {
                minWordDocListCount = Math.Min(minWordDocListCount, wifq.WordIndex.WordDocList.Count);
            }

            if (docIdRank.Count == 0)
            {
                if (minWordDocListCount > DocumentResultWhereDictionary.DefaultSize)
                {
                    docIdRank = new Core.SFQL.Parse.DocumentResultWhereDictionary(minWordDocListCount);
                }
            }

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Calculate");

            //Merge
            bool oneWordOptimize = this._QueryParameter.CanLoadPartOfDocs && this._QueryParameter.NoAndExpression && wordIndexes.Length == 1;
            int oneWordMaxCount = 0;

            if (oneWordOptimize)
            {
                //One word
                WordIndexForQuery wifq = wordIndexes[0]; //first word

                //Entity.DocumentPositionList[] wifqDocBuf = wifq.WordIndex.DocPositionBuf;

                Entity.DocumentPositionList docList = wifq.WordIndex.GetNext();
                int j = 0;

                while (docList.DocumentId >= 0)
                {
                    //Entity.DocumentPositionList docList = wifq.WordIndex[j];

                    Core.SFQL.Parse.DocumentResultPoint drp;
                    drp.pDocumentResult = null;

                    if (j > MinResultCount)
                    {
                        if (oneWordMaxCount > docList.Count)
                        {
                            j++;
                            docList = wifq.WordIndex.GetNext();

                            continue;
                        }
                    }
                    else
                    {
                        if (oneWordMaxCount < docList.Count)
                        {
                            oneWordMaxCount = docList.Count;
                        }
                    }

                    long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                    if (score < 0)
                    {
                        //Overflow
                        score = long.MaxValue - 4000000;
                    }

                    if (upDict == null)
                    {
                        docIdRank.Add(docList.DocumentId, score);
                    }
                    else
                    {
                        if (!upDict.Not)
                        {
                            if (upDict.ContainsKey(docList.DocumentId))
                            {
                                docIdRank.Add(docList.DocumentId, score);
                            }
                        }
                        else
                        {
                            if (!upDict.ContainsKey(docList.DocumentId))
                            {
                                docIdRank.Add(docList.DocumentId, score);
                            }
                        }
                    }

                    j++;
                    docList = wifq.WordIndex.GetNext();
                }
            }
            else
            {
                int wordIndexesLen = wordIndexes.Length;

                WordIndexForQuery fstWifq = wordIndexes[0]; //first word

                Entity.DocumentPositionList fstDocList = fstWifq.WordIndex.GetNext();

                Entity.DocumentPositionList[] docListArr = new Hubble.Core.Entity.DocumentPositionList[wordIndexesLen];

                docListArr[0] = fstDocList;

                while (fstDocList.DocumentId >= 0)
                {
                    int curWord = 1;
                    int firstDocId = fstDocList.DocumentId;

                    while (curWord < wordIndexesLen)
                    {
                        docListArr[curWord] = wordIndexes[curWord].WordIndex.Get(firstDocId);

                        if (docListArr[curWord].DocumentId < 0)
                        {
                            break;
                        }

                        curWord++;
                    } //While

                    if (curWord >= wordIndexesLen)
                    {
                        //Matched

                        long totalScore = 0;
                        for (int i = 0; i < wordIndexesLen; i++)
                        {
                            WordIndexForQuery wifq = wordIndexes[i];
                            Entity.DocumentPositionList docList = docListArr[i];

                            long score = (long)wifq.FieldRank * (long)wifq.WordRank * (long)wifq.Idf_t * (long)docList.Count * (long)1000000 / ((long)wifq.Sum_d_t * (long)docList.TotalWordsInThisDocument);

                            if (score < 0)
                            {
                                //Overflow
                                score = long.MaxValue - 4000000;
                            }

                            totalScore += score;
                        }

                        if (upDict == null)
                        {
                            docIdRank.Add(firstDocId, totalScore);
                        }
                        else
                        {
                            if (!upDict.Not)
                            {
                                if (upDict.ContainsKey(firstDocId))
                                {
                                    docIdRank.Add(firstDocId, totalScore);
                                }
                            }
                            else
                            {
                                if (!upDict.ContainsKey(firstDocId))
                                {
                                    docIdRank.Add(firstDocId, totalScore);
                                }
                            }
                        }
                    }

                    fstDocList = fstWifq.WordIndex.GetNext();
                    docListArr[0] = fstDocList;
                }
            }

            //Merge score if upDict != null
            if (upDict != null)
            {
                if (!upDict.Not)
                {
                    foreach (int docid in docIdRank.Keys)
                    {
                        DocumentResult* upDrp;

                        if (upDict.TryGetValue(docid, out upDrp))
                        {
                            DocumentResult* drpResult;
                            if (docIdRank.TryGetValue(docid, out drpResult))
                            {
                                drpResult->Score += upDrp->Score;
                            }
                        }
                    }
                }
            }

            DeleteProvider delProvider = _DBProvider.DelProvider;
            int delCount = delProvider.Filter(docIdRank);

            if (oneWordOptimize && _QueryParameter.CanLoadPartOfDocs && upDict == null)
            {
                docIdRank.RelTotalCount = wordIndexes[0].RelTotalCount - delCount;
            }
            else
            {
                docIdRank.RelTotalCount = docIdRank.Count;
            }

            performanceReport.Stop();
        }


        unsafe private DocumentResultWhereDictionary GetExternLikeDocResults(DocumentResultWhereDictionary docIdRank)
        {
            if (docIdRank.Count <= 0)
            {
                return docIdRank;
            }

            //Sort

            Query.DocumentResultForSort[] docResultArray =
                new Hubble.Core.Query.DocumentResultForSort[docIdRank.Count]; ;

            int i = 0;

            foreach (Core.SFQL.Parse.DocumentResultPoint drp in docIdRank.Values)
            {
                docResultArray[i++] = new Hubble.Core.Query.DocumentResultForSort(drp.pDocumentResult);
            }

            List<SFQL.SyntaxAnalysis.Select.OrderBy> orderBys = new List<SFQL.SyntaxAnalysis.Select.OrderBy>();

            SFQL.SyntaxAnalysis.Select.OrderBy scoreOrderBy = new SFQL.SyntaxAnalysis.Select.OrderBy();
            scoreOrderBy.Name = "score";
            scoreOrderBy.Order = "desc";

            orderBys.Add(scoreOrderBy);

            QueryResultSort qSort = new QueryResultSort(orderBys, _DBProvider);

            int sortLen = MinResultCount;

            if (_QueryParameter.CanLoadPartOfDocs)
            {
                sortLen = _QueryParameter.End + 1 + 10;
            }

            if (sortLen > 0)
            {
                sortLen = ((sortLen - 1) / 100 + 1) * 100;
            }

            qSort.Sort(docResultArray, sortLen); // using part quick sort can reduce 40% time

            //Build sql 
            int len = Math.Min(docResultArray.Length, sortLen);

            StringBuilder sql = new StringBuilder();

            bool docidReplace = _DBProvider.DocIdReplaceField != null;

            string docIdFieldName = "docid";

            if (docidReplace)
            {
                docIdFieldName = _DBProvider.DocIdReplaceField;
            }

            sql.AppendFormat("select {0} from {1} where {2} like '{3}' and {0} in (",
                docIdFieldName, _DBProvider.Table.DBTableName, InvertedIndex.FieldName, _LikeString);

            for (i = 0; i < len; i++)
            {
                long id;

                if (docidReplace)
                {
                    id = _DBProvider.GetDocIdReplaceFieldValue(docResultArray[i].DocId);
                }
                else
                {
                    id = docResultArray[i].DocId;
                }

                if (i == 0)
                {
                    sql.AppendFormat("{0}", id);
                }
                else
                {
                    sql.AppendFormat(",{0}", id);
                }
            }

            sql.Append(")");

            System.Data.DataSet ds = _DBProvider.DBAdapter.QuerySql(sql.ToString());

            DocumentResultWhereDictionary result = new DocumentResultWhereDictionary(ds.Tables[0].Rows.Count);

            foreach(System.Data.DataRow row in ds.Tables[0].Rows)
            {
                int docid;

                if (docidReplace)
                {
                    docid = _DBProvider.GetDocIdFromDocIdReplaceFieldValue(long.Parse(row[0].ToString()));
                }
                else
                {
                    docid = int.Parse(row[0].ToString());
                }

                result.Add(docid, 1);
            }

            if (result.Count < sortLen)
            {
                result.RelTotalCount = result.Count;
            }
            else
            {
                result.RelTotalCount = docIdRank.RelTotalCount;
            }

            if (result.Count < docIdRank.Count && this._QueryParameter.NeedGroupBy)
            {
                //if result count less than total count
                //and need group by
                //init group by collection
                foreach (int docid in docIdRank.Keys)
                {
                    result.AddToGroupByCollection(docid);
                }
            }

            return result;

        }

        #region IQuery Members

        public string FieldName
        {
            get
            {
                return _FieldName;
            }

            set
            {
                _FieldName = value;
            }
        }

        public int TabIndex
        {
            get
            {
                return _TabIndex;
            }
            set
            {
                _TabIndex = value;
            }
        }

        public string Command
        {
            get
            {
                return "Like";
            }
        }

        QueryParameter _QueryParameter = new QueryParameter();

        public QueryParameter QueryParameter
        {
            get
            {
                return _QueryParameter;
            }
        }
      
        public DBProvider DBProvider
        {
            get
            {
                return _DBProvider;
            }
            set
            {
                _DBProvider = value;
            }
        }

        public Hubble.Core.Index.InvertedIndex InvertedIndex
        {
            get
            {
                return _InvertedIndex;
            }

            set
            {
                _InvertedIndex = value;
            }
        }

        public IList<Hubble.Core.Entity.WordInfo> QueryWords
        {
            get
            {
                return _QueryWords;
            }

            set
            {
                Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("QueryWords");

                Dictionary<string, int> wordIndexDict = new Dictionary<string, int>();

                _QueryWords.Clear();
                wordIndexDict.Clear();

                List<WordIndexForQuery> wordIndexList = new List<WordIndexForQuery>(value.Count);

                if (value.Count <= 0)
                {
                    return;
                }

                _LikeString = value[0].Word;

                if (_LikeString.Contains("%") || !_LikeString.Contains("*"))
                {
                    _QueryThroughDB = true;
                    return;
                }

                if (!_LikeString.StartsWith("*") || !_LikeString.EndsWith("*"))
                {
                    throw new ParseException("like statement must be start and end with *. such as like '*xxx*'!");
                }

                _LikeString = _LikeString.Replace("*", "%");
                string likeString = _LikeString;

                Data.Field field = _DBProvider.GetField(InvertedIndex.FieldName);

                Analysis.IAnalyzer analyzer =
                    Data.DBProvider.GetAnalyzer(field.AnalyzerName);

                Dictionary<string, WordIndexForQuery> wordIndexDictExtern = new Dictionary<string, WordIndexForQuery>();

                foreach (Entity.WordInfo wordInfo in analyzer.TokenizeForSqlClient(
                    likeString.Replace("%", " ").Replace("''", "'").Trim()))
                {
                    _QueryWords.Add(wordInfo);

                    if (!wordIndexDictExtern.ContainsKey(wordInfo.Word))
                    {
                        WordIndexForQuery wifq;

                        if (!wordIndexDictExtern.TryGetValue(wordInfo.Word, out wifq))
                        {
                            Hubble.Core.Index.WordIndexReader wordIndex;

                            if (value.Count == 1)
                            {
                                //wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, CanLoadPartOfDocs);
                                wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, _QueryParameter.CanLoadPartOfDocs, true);

                            }
                            else
                            {
                                //wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, false);
                                wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word, false, true);
                            }

                            if (wordIndex == null)
                            {
                                //No result
                                wordIndex = new Hubble.Core.Index.WordIndexReader(wordInfo.Word,
                                    new Hubble.Core.Store.WordDocumentsList(), 0, _DBProvider);
                            }

                            wifq = new WordIndexForQuery(wordIndex,
                                InvertedIndex.DocumentCount, wordInfo.Rank, this._QueryParameter.FieldRank);
                            wifq.QueryCount = 1;
                            wifq.FirstPosition = wordInfo.Position;
                            wordIndexList.Add(wifq);
                            wordIndexDictExtern.Add(wordInfo.Word, wifq);
                        }
                        else
                        {
                            wifq.WordRank += wordInfo.Rank;
                            wifq.QueryCount++;
                        }
                    }
                }
                
                _TotalDocuments = InvertedIndex.DocumentCount; 

                _WordIndexes = new WordIndexForQuery[wordIndexList.Count];
                wordIndexList.CopyTo(_WordIndexes, 0);

                performanceReport.Stop();
            }
        }

        private Core.SFQL.Parse.DocumentResultWhereDictionary PartSearch(WordIndexForQuery[] wordIndexes)
        {
            Core.SFQL.Parse.DocumentResultWhereDictionary result = new Core.SFQL.Parse.DocumentResultWhereDictionary();

            if (_QueryWords.Count <= 0 || wordIndexes.Length <= 0)
            {
                if (_QueryParameter.Not && UpDict != null)
                {
                    return UpDict;
                }
                else
                {
                    return result;
                }
            }

            if (this._QueryParameter.Not)
            {
                if (_InvertedIndex.IndexMode == Field.IndexMode.Simple)
                {
                    Calculate(null, ref result, wordIndexes);
                }
                else
                {
                    CalculateWithPosition(null, ref result, wordIndexes);
                }
            }
            else
            {
                if (_InvertedIndex.IndexMode == Field.IndexMode.Simple)
                {
                    Calculate(this.UpDict, ref result, wordIndexes);
                }
                else
                {
                    CalculateWithPosition(this.UpDict, ref result, wordIndexes);
                }
            }

            return result;

        }

        private List<WordIndexForQuery[]> GetAllPartOfWordIndexes()
        {
            List<WordIndexForQuery[]> result = new List<WordIndexForQuery[]>();

            Array.Sort(_WordIndexes, new WordIndexForQueryCompareByPositionAndRank());

            List<List<WordIndexForQuery>> groups = new List<List<WordIndexForQuery>>();

            foreach (WordIndexForQuery wifq in _WordIndexes)
            {
                bool addNew = true;

                int i = 0;

                foreach (List<WordIndexForQuery> wifqList in groups)
                {
                    WordIndexForQuery wifq1 = wifqList[wifqList.Count - 1];

                    if (wifq.FirstPosition >= wifq1.FirstPosition + wifq1.WordIndex.Word.Length)
                    {
                        if (i == 0)
                        {
                            for (int j = 1; j < groups.Count; j++)
                            {
                                WordIndexForQuery wifq2 = groups[j][groups[j].Count - 1];
                                if (wifq1.FirstPosition >= wifq2.FirstPosition + wifq2.WordIndex.Word.Length)
                                {
                                    groups[j].Add(wifq1);
                                }
                            }
                        }

                        wifqList.Add(wifq);
                        addNew = false;
                        break;
                    }

                    i++;
                }

                if (addNew)
                {
                    groups.Add(new List<WordIndexForQuery>());

                    if (groups.Count > 1)
                    {
                        //Add pre words 
                        int j = 0;
                        WordIndexForQuery wifq1 = groups[0][j];

                        while (wifq1.FirstPosition + wifq1.WordIndex.Word.Length <= wifq.FirstPosition)
                        {
                            groups[groups.Count - 1].Add(wifq1);
                            j++;

                            if (j >= groups[0].Count)
                            {
                                break;
                            }

                            wifq1 = groups[0][j];
                        }
                    }

                    groups[groups.Count - 1].Add(wifq);
                }
            }

            if (groups.Count > 0)
            {
                WordIndexForQuery wifq1 = groups[0][groups[0].Count - 1];

                for (int j = 1; j < groups.Count; j++)
                {
                    WordIndexForQuery wifq2 = groups[j][groups[j].Count - 1];
                    if (wifq1.FirstPosition >= wifq2.FirstPosition + wifq2.WordIndex.Word.Length)
                    {
                        groups[j].Add(wifq1);
                    }
                }
            }

            foreach (List<WordIndexForQuery> group in groups)
            {
                result.Add(group.ToArray());
            }

            return result;
        }

        public Core.SFQL.Parse.DocumentResultWhereDictionary Search()
        {
            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport("Search of Like");
            Core.SFQL.Parse.DocumentResultWhereDictionary result;

            if (_QueryThroughDB)
            {
                string orderBy = _QueryParameter.OrderBy;

                if (orderBy != null)
                {
                    if (orderBy.ToLower().Contains("score"))
                    {
                        orderBy = null;
                    }
                    else if (_DBProvider.Table.IndexOnly && _DBProvider.Table.DocIdReplaceField != null &&
                        orderBy.ToLower().Contains("docid"))
                    {
                        orderBy = null;
                    }
                }

                if (!_QueryParameter.NoAndExpression || UpDict != null)
                {
                    if (_DBProvider.Table.IndexOnly && _DBProvider.Table.DocIdReplaceField != null)
                    {
                        orderBy = _DBProvider.Table.DocIdReplaceField;
                    }
                    else
                    {
                        orderBy = "docid";
                    }

                    _QueryParameter.End = -1;
                }

                result = _DBProvider.DBAdapter.GetDocumentResults(_QueryParameter.End,
                    string.Format("{0} like '{1}'", _FieldName, _LikeString), orderBy);

                if (UpDict != null)
                {
                    if (!UpDict.Not)
                    {
                        result = result.AndMergeDict(result, UpDict);
                    }
                }
            }
            else
            {

                List<WordIndexForQuery[]> partList = GetAllPartOfWordIndexes();

                if (_QueryWords.Count <= 0 || partList.Count <= 0)
                {
                    return PartSearch(new WordIndexForQuery[0]);
                }


                result = PartSearch(partList[0]);

                for (int i = 1; i < partList.Count; i++)
                {
                    bool someWordNoResult = false;

                    foreach (WordIndexForQuery w in partList[i])
                    {
                        if (w.WordIndex.WordCount == 0)
                        {
                            someWordNoResult = true;
                            break;
                        }

                        w.WordIndex.Reset();
                    }

                    if (!someWordNoResult)
                    {
                        result.OrMerge(result, PartSearch(partList[i]));
                    }
                }
            }

            if (this._QueryParameter.Not)
            {
                result.Not = true;

                if (UpDict != null)
                {
                    result = result.AndMergeForNot(result, UpDict);
                }
            }

            if (!_QueryThroughDB)
            {
                result = GetExternLikeDocResults(result);
            }

            performanceReport.Stop(); 

            return result;
        }

        private Core.SFQL.Parse.DocumentResultWhereDictionary _UpDict;

        public Core.SFQL.Parse.DocumentResultWhereDictionary UpDict
        {
            get
            {
                return _UpDict;
            }
            set
            {
                _UpDict = value;
            }
        }

        private Dictionary<int, int> _NotInDict = null;

        /// <summary>
        /// Key is docid
        /// Value is 0
        /// </summary>
        public Dictionary<int, int> NotInDict
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

        #endregion


        #region INamedExternalReference Members

        public string Name
        {
            get
            {
                return Command;
            }
        }

        #endregion
    }
}
