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
    /// This query analyze documents with position
    /// belong to the input words.
    /// Syntax: FullTextQuery('xxxyyyzzz')
    /// </summary>
    public class FullTextQuery : IQuery, INamedExternalReference
    {
        class SessionRank
        {
            public long Session;
            public int Rank;
        }

        class WordIndexForQuery
        {
            private int _CurPosition;
            private int _CurIndex;
            private Index.InvertedIndex.WordIndexReader _WordIndex;
            private int _OldIndexForDoc = -1;
            private long _CurDocmentId;

            public IEnumerator<int> _CurPositionValues;

            public IEnumerator<int> CurPositionValues
            {
                get
                {
                    return _CurPositionValues;
                }
            }

            /// <summary>
            /// Cur Position
            /// -1 means end
            /// </summary>
            public int CurPosition
            {
                get
                {
                    return _CurPosition;
                }

                set
                {
                    _CurPosition = value;
                }
            }

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

            public long CurDocumentId
            {
                get
                {
                    if (CurIndex < 0)
                    {
                        return -1;
                    }
                    else
                    {
                        //Because WordIndex.GetDocumentId excute slowly.
                        if (CurIndex == _OldIndexForDoc)
                        {
                            return _CurDocmentId;
                        }
                        else
                        {
                            _OldIndexForDoc = CurIndex;
                        }

                        _CurDocmentId = WordIndex.GetDocumentId(CurIndex);
                        return _CurDocmentId;
                    }
                }
            }

            public Index.InvertedIndex.WordIndexReader WordIndex
            {
                get
                {
                    return _WordIndex;
                }
            }

            public WordIndexForQuery(Index.InvertedIndex.WordIndexReader wordIndex)
            {
                _CurPositionValues = null;

                _OldIndexForDoc = -1;

                if (wordIndex.Count <= 0)
                {
                    _CurIndex = -1;
                    _CurPosition = -1;
                }
                else
                {
                    _CurIndex = 0;
                    _CurPosition = wordIndex[0].Values.Current;
                }

                _WordIndex = wordIndex;
            }

            public void SetCurPositionValues(IEnumerator<int> curPositionValues)
            {
                _CurPositionValues = curPositionValues;

                if (!_CurPositionValues.MoveNext())
                {
                    _CurPosition = -1;
                }
                else
                {
                    _CurPosition = _CurPositionValues.Current;
                }
            }

            public void IncCurIndex()
            {
                CurIndex++;

                if (CurIndex >= _WordIndex.Count)
                {
                    CurIndex = -1;
                }
            }
        }

        string _FieldName;
        Hubble.Core.Index.InvertedIndex _InvertedIndex;
        AppendList<Entity.WordInfo> _QueryWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        Dictionary<string, List<Entity.WordInfo>> _WordQueryWordTable = new Dictionary<string, List<Entity.WordInfo>>();

        AppendList<Entity.WordInfo> _HitWords = new AppendList<Hubble.Core.Entity.WordInfo>();
        Dictionary<string, int> _WordIndexDict = new Dictionary<string, int>();
        AppendList<WordIndexForQuery> _WordIndexList = new AppendList<WordIndexForQuery>();

        AppendList<WordIndexForQuery> _TempSelect = new AppendList<WordIndexForQuery>();
        AppendList<int> _TempPositionList = new AppendList<int>();

        Dictionary<string, SessionRank> _WordRank = new Dictionary<string, SessionRank>();
        long _Session = 0;

        private int _TabIndex;
        private DBProvider _DBProvider;

        private int _QueryStringLength = 0;
        private int _HoleNumber = 0; //If query string does not continue, the hole number will be large then 0

        private IList<Entity.WordInfo> GetNextHitWords(out long docId)
        {
            _HitWords.Clear();

            if (_QueryWords.Count <= 0)
            {
                docId = -1;
                return _HitWords;
            }

            long minDocId = long.MaxValue;

            //Get min document id
            for (int i = 0; i < _WordIndexList.Count; i++)
            {
                long curDocId = _WordIndexList[i].CurDocumentId;

                if (curDocId < 0)
                {
                    continue;
                }

                if (minDocId > curDocId)
                {
                    minDocId = curDocId;
                }
            }

            if (minDocId == long.MaxValue)
            {
                docId = -1;
                return _HitWords;
            }

            //Put min doc id into WordIndexes
            _TempSelect.Clear();
            for (int i = 0; i < _WordIndexList.Count; i++)
            {
                long curDocId = _WordIndexList[i].CurDocumentId;

                if (curDocId == minDocId)
                {
                    _TempSelect.Add(_WordIndexList[i]);
                    _WordIndexList[i].SetCurPositionValues(
                        _WordIndexList[i].WordIndex[_WordIndexList[i].CurIndex].Values);
                }
            }

            docId = minDocId;

            while (true)
            {
                int minPos = int.MaxValue;
                int repeatTimes = 0;
                int minIndex = -1;

                for (int i = 0; i < _TempSelect.Count; i++)
                {
                    int curPos = _TempSelect[i].CurPosition;
                    if (curPos >= 0)
                    {
                        if (minPos > curPos)
                        {
                            minPos = curPos;
                            repeatTimes = 1;
                            minIndex = i;
                        }
                        else if (minPos == curPos)
                        {
                            repeatTimes++;
                        }
                    }
                }

                //Get min position list
                _TempPositionList.Clear();

                if (repeatTimes == 1)
                {
                    _TempPositionList.Add(minIndex);
                }
                else if (repeatTimes > 1)
                {
                    for (int i = 0; i < _TempSelect.Count; i++)
                    {
                        int curPos = _TempSelect[i].CurPosition;

                        if (curPos == minPos)
                        {
                            _TempPositionList.Add(i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _WordIndexList.Count; i++)
                    {
                        long curDocId = _WordIndexList[i].CurDocumentId;

                        if (curDocId == minDocId)
                        {
                            _WordIndexList[i].IncCurIndex();
                        }
                    }

                    return _HitWords;
                }


                foreach (int index in _TempPositionList)
                {
                    WordIndexForQuery indexForQuery = _TempSelect[index];
                    int curIndex = indexForQuery.CurIndex;

                    _HitWords.Add(new Hubble.Core.Entity.WordInfo(indexForQuery.WordIndex.Word,
                        minPos));

                    if (_TempSelect[index].CurPositionValues.MoveNext())
                    {
                        _TempSelect[index].CurPosition = _TempSelect[index].CurPositionValues.Current;
                    }
                    else
                    {
                        _TempSelect[index].CurPosition = -1;
                    }
                }

            }

        }

        readonly static int[] RadixRank = {
        100, 98, 90,  80, 70, 60, 50, 45,
        40, 38, 36, 34, 32, 30, 28, 26,
        20, 18, 16, 14, 12, 11, 10, 9,
        8, 7, 6, 5, 4, 3, 2, 1,
        };

        readonly static int[] ContinueTimesBase = {
        1, 1, 10, 30, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900
        };

        /// <summary>
        /// Find same position wordinfos and 
        /// cacluate their rank sum.
        /// </summary>
        /// <param name="wordInfoList">wordinfo list</param>
        /// <param name="start">start index in wordInfoList</param>
        /// <param name="end">end index in wordInfoList</param>
        /// <param name="lastMinPosition">min position in query words that last time this function has been called.</param>
        /// <returns>if no result, return -1</returns>
        private int CaculateRankSamePosition(IList<Entity.WordInfo> wordInfoList,
            int start, int end, ref int lastMinPosition, out int distance, out int maxEndPosition)
        {
            int minPosition = int.MaxValue;
            int minWordInfoRank = 1;
            int rank = 0;
            distance = 31;
            maxEndPosition = 0;

            if (end < wordInfoList.Count && start >= 0)
            {
                for (int i = start; i <= end; i++)
                {
                    List<Entity.WordInfo> queryWords;

                    maxEndPosition = Math.Max(maxEndPosition, wordInfoList[i].Position + wordInfoList[i].Word.Length);

                    if (!_WordQueryWordTable.TryGetValue(wordInfoList[i].Word, out queryWords))
                    {
                        continue;
                    }

                    foreach (Entity.WordInfo queryWordInfo in queryWords)
                    {
                        if (queryWordInfo.Position > lastMinPosition)
                        {
                            int curPosition = queryWordInfo.Position;

                            if (curPosition < minPosition)
                            {
                                minPosition = queryWordInfo.Position;
                                minWordInfoRank = queryWordInfo.Rank;
                            }

                            rank += queryWordInfo.Rank;
                        }
                    }
                }
            }

            if (minPosition == int.MaxValue)
            {
                return -1;
            }

            if (lastMinPosition < 0 || start == 0)
            {
                //First word when scan
                lastMinPosition = minPosition;

                return rank;
            }
            else
            {
                distance = Math.Abs((minPosition - lastMinPosition) -
                    (wordInfoList[start].Position - wordInfoList[start - 1].Position));

                if (distance > 31)
                {
                    distance = 31;
                }

                //rank += minWordInfoRank <= 0 ? RadixRank[distance] : RadixRank[distance] * minWordInfoRank;
                rank += RadixRank[distance] * (end + 1 - start);
                lastMinPosition = minPosition;

                return rank;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordInfoList"></param>
        /// <returns></returns>
        private int CaculateRank(IList<Entity.WordInfo> wordInfoList)
        {
            if (wordInfoList.Count <= 0)
            {
                return 0;
            }

            _Session++;

            long rank = 0;

            int lastMinPosition = -1; //Last min position

            int start = 0;
            int end = 1;
            int lastScanPostition = wordInfoList[0].Position;
            int continueTimes = 0;
            int distance;
            int fstStart = 0;
            int maxEndPosition = wordInfoList[0].Position + wordInfoList[0].Word.Length;
            int lastMaxEndPosition = 0;
            int holeNumber = 0;

            while (end < wordInfoList.Count)
            {
                SessionRank sRank;
                if (_WordRank.TryGetValue(wordInfoList[end].Word, out sRank))
                {
                    sRank.Session = _Session;
                }

                if (wordInfoList[end].Position != lastScanPostition)
                {
                    int r = CaculateRankSamePosition(wordInfoList, start, end - 1, ref lastMinPosition, out distance, out maxEndPosition);

                    if (r < 0)
                    {
                        //Scan to end of query words
                        //backdate to the begining
                        continueTimes = 1;
                        lastMinPosition = -1;
                        fstStart = start;
                        holeNumber = 0;
                        r = CaculateRankSamePosition(wordInfoList, start, end - 1, ref lastMinPosition, out distance, out maxEndPosition);
                    }
                    else
                    {
                        if (distance < 8 && lastScanPostition >= lastMaxEndPosition)
                        {
                            continueTimes++;
                        }
                    }

                    if (r > 0)
                    {
                        if (continueTimes >= ContinueTimesBase.Length)
                        {
                            continueTimes = ContinueTimesBase.Length - 1;
                        }

                        if ((wordInfoList[start].Position + wordInfoList[start].Word.Length - 
                            wordInfoList[fstStart].Position ==  _QueryStringLength) &&
                            holeNumber == _HoleNumber)
                        {
                            //Whole match
                            rank += 1000 * r;
                        }
                        else
                        {
                            rank += ContinueTimesBase[continueTimes] * r;
                        }
                    }

                    start = end;
                }

                lastScanPostition = wordInfoList[end].Position;
                lastMaxEndPosition = maxEndPosition;
                if (lastScanPostition > lastMaxEndPosition)
                {
                    holeNumber = lastScanPostition - lastMaxEndPosition;
                }

                end++;
            }


            SessionRank sEndRank;
            if (_WordRank.TryGetValue(wordInfoList[start].Word, out sEndRank))
            {
                sEndRank.Session = _Session;
            }

            //deal with end word
            int r_End = CaculateRankSamePosition(wordInfoList, start, end - 1, ref lastMinPosition, out distance, out maxEndPosition);

            if (r_End < 0)
            {
                //Scan to end of query words
                //backdate to the begining
                lastMinPosition = -1;
                r_End = CaculateRankSamePosition(wordInfoList, start, end - 1, ref lastMinPosition, out distance, out maxEndPosition);
            }

            if (r_End > 0)
            {
                if (continueTimes >= ContinueTimesBase.Length)
                {
                    continueTimes = ContinueTimesBase.Length - 1;
                }

                if ((wordInfoList[start].Position + wordInfoList[start].Word.Length -
                    wordInfoList[fstStart].Position == _QueryStringLength) &&
                    holeNumber == _HoleNumber)
                {
                    //Whole match
                    rank += 1000 * r_End;
                }
                else
                {
                    rank += ContinueTimesBase[continueTimes] * r_End;
                }

            }
            //end deal with end word

            int queryRankSum = 0;
            foreach (SessionRank sRank in _WordRank.Values)
            {
                if (sRank.Session == _Session)
                {
                    queryRankSum += sRank.Rank;
                }
            }

            rank *= queryRankSum;

            if (rank > int.MaxValue - 4000000)
            {
                long high = rank % (int.MaxValue - 4000000);

                return int.MaxValue - 4000000 + (int)(high / 1000);
            }
            else
            {
                return (int)rank;
            }

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

        private int _FieldRank = 1;
        public int FieldRank
        {
            get
            {
                return _FieldRank;
            }
            set
            {
                _FieldRank = value;
                if (_FieldRank <= 0)
                {
                    _FieldRank = 1;
                }
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
                _QueryWords.Clear();
                _WordIndexList.Clear();
                _WordIndexDict.Clear();

                foreach (Hubble.Core.Entity.WordInfo wordInfo in value)
                {
                    _QueryWords.Add(wordInfo);

                    if (!_WordIndexDict.ContainsKey(wordInfo.Word))
                    {
                        Hubble.Core.Index.InvertedIndex.WordIndexReader wordIndex = InvertedIndex.GetWordIndex(wordInfo.Word);

                        if (wordIndex == null)
                        {
                            continue;
                        }

                        _WordIndexList.Add(new WordIndexForQuery(wordIndex));
                        _WordIndexDict.Add(wordInfo.Word, _WordIndexList.Count - 1);
                    }
                }

                _QueryWords.Sort();
                bool fst = true;
                int fstPosition = 0;
                int maxEndPosition = 0;

                //Init _PositionQueryWordTable
                //This table use to caculate the document rank
                foreach (Entity.WordInfo wordInfo in _QueryWords)
                {
                    SessionRank sRank;
                    if (!_WordRank.TryGetValue(wordInfo.Word, out sRank))
                    {
                        sRank = new SessionRank();
                        sRank.Rank = wordInfo.Rank;
                        sRank.Session = 0;
                        _WordRank.Add(wordInfo.Word, sRank);
                    }

                    if (wordInfo.Rank > sRank.Rank)
                    {
                        sRank.Rank = wordInfo.Rank;
                    }


                    if (fst)
                    {
                        fstPosition = wordInfo.Position;
                        fst = false;
                    }

                    _QueryStringLength = Math.Max(_QueryStringLength, wordInfo.Word.Length + wordInfo.Position - fstPosition);

                    if (wordInfo.Position - maxEndPosition > 0)
                    {
                        _HoleNumber += wordInfo.Position - maxEndPosition;
                    }

                    maxEndPosition = Math.Max(maxEndPosition, wordInfo.Word.Length + wordInfo.Position);


                    if (!_WordQueryWordTable.ContainsKey(wordInfo.Word))
                    {
                        _WordQueryWordTable.Add(wordInfo.Word,
                            new List<Hubble.Core.Entity.WordInfo>());
                    }

                    _WordQueryWordTable[wordInfo.Word].Add(wordInfo);
                }
            }
        }

        WhereDictionary<long, DocumentResult> _UpDict;

        public WhereDictionary<long, DocumentResult> UpDict
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

        bool _Not;

        public bool Not
        {
            get
            {
                return _Not;
            }
            set
            {
                _Not = value;
            }
        }



        public WhereDictionary<long, DocumentResult> Search()
        {
            WhereDictionary<long, DocumentResult> result = new WhereDictionary<long, DocumentResult>();
            long docId;

            IList<Entity.WordInfo> wordInfoList = GetNextHitWords(out docId);

            while (docId >= 0)
            {
                bool canInsert = false;

                if (this.Not)
                {
                    canInsert = true;
                }
                else
                {
                    if (UpDict != null)
                    {
                        if (!UpDict.Not)
                        {
                            if (UpDict.ContainsKey(docId))
                            {
                                canInsert = true;
                            }
                        }
                        else
                        {
                            if (!UpDict.ContainsKey(docId))
                            {
                                canInsert = true;
                            }
                        }
                    }
                    else
                    {
                        canInsert = true;
                    }
                }

                if (canInsert)
                {
                    result.Add(docId, new DocumentResult(docId, FieldRank * CaculateRank(wordInfoList)));
                }

                wordInfoList = GetNextHitWords(out docId);

            }

            if (this.Not)
            {
                result.Not = true;

                if (UpDict != null)
                {
                    result = result.AndMerge(result, UpDict);
                }
            }

            return result;
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

        #region IQuery Members



        #endregion
    }
}
