using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hubble.Core.Data;

namespace Hubble.Core.Query.Optimize
{
    public class MultiWordsDocIdEnumerator
    {
        /// <summary>
        /// Doc id entity for specify word
        /// </summary>
        public class WordDocIdEntity : IComparable<WordDocIdEntity>
        {
            internal Entity.OriginalDocumentPositionList DocList;

            internal WordIndexForQuery WordIndex; //WordIndex for this word

            internal int Index; //index of _WordIndexes

            internal WordDocIdEntity(int index, WordIndexForQuery wordIndex)
            {
                this.Index = index;
                this.WordIndex = wordIndex;
            }

            #region IComparable<WordDocIdEntity> Members

            public int CompareTo(WordDocIdEntity other)
            {
                if (this.DocList.DocumentId < other.DocList.DocumentId)
                {
                    return -1;
                }
                else if (this.DocList.DocumentId > other.DocList.DocumentId)
                {
                    //if docid = -1, sort to tail of the list
                    if (other.DocList.DocumentId < 0)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }

            #endregion
        }

        public WordIndexForQuery[] WordIndexes;
        public int SelectedCount;
        public int TotalDocIdCount;
        public int[] SelectedIndexes;
        public Entity.OriginalDocumentPositionList[] SelectedDocLists;

        private WordDocIdEntity[] _WordDocEntities; //this is a priority queue
        int _WordsCount;
        DBProvider _DBProvider;
        Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary _GroupByDict;
        bool _NeedGroupBy;
        int _GroupByLimit;
        int _GroupByStep;

        //vars for delete
        bool _HaveRecordsDeleted = false;
        int[] _DelDocs = null;
        int _CurDelIndex = 0;
        int _CurDelDocid = 0;

        bool _OptimizeByScore = false;
        int _IndexThreshold = 0; //for optimize by score. Only return when index <= _IndexThreshold.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordIndexes">word indexes informations</param>
        /// <param name="dbProvider">current dbprovider</param>
        /// <param name="dictForGroupBy">dict to calculate group by. If is null, don't need output group by</param>
        /// <param name="top">for optimize by score, how many records want to return</param>
        public MultiWordsDocIdEnumerator(WordIndexForQuery[] wordIndexes, DBProvider dbProvider,
            Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary dictForGroupBy, int top)
        {
            _GroupByDict = dictForGroupBy;
            _NeedGroupBy = _GroupByDict != null;

            _DBProvider = dbProvider;

            WordIndexes = wordIndexes;

            Array.Sort(WordIndexes);

            if (_NeedGroupBy)
            {
                _GroupByLimit = _DBProvider.Table.GroupByLimit;
                _GroupByStep = WordIndexes[WordIndexes.Length - 1].RelTotalCount / _GroupByLimit;
                if (_GroupByStep <= 0)
                {
                    _GroupByStep = 1;
                }
            }

            _OptimizeByScore = false;

            if (top >= 0)
            {
                _OptimizeByScore = true;
                int relTop = top + _DBProvider.DelProvider.Count; //should include del records

                _IndexThreshold = -1;

                long sum = 0;
                while (sum < relTop)
                {
                    _IndexThreshold++;

                    if (_IndexThreshold >= WordIndexes.Length)
                    {
                        _OptimizeByScore = false;
                        break;
                    }

                    sum += WordIndexes[_IndexThreshold].RelTotalCount;
                }

            }

            Reset();


        }

        public MultiWordsDocIdEnumerator(WordIndexForQuery[] wordIndexes, DBProvider dbProvider, 
            Hubble.Core.SFQL.Parse.DocumentResultWhereDictionary dictForGroupBy)
            : this(wordIndexes, dbProvider, dictForGroupBy, - 1)
        {
    
        }

        public void Reset()
        {
            _WordDocEntities = new WordDocIdEntity[WordIndexes.Length];

            for (int index = 0; index < WordIndexes.Length; index++)
            {
                _WordDocEntities[index] = new WordDocIdEntity(index, WordIndexes[index]);

                _WordDocEntities[index].WordIndex.WordIndex.GetNextOriginal(ref _WordDocEntities[index].DocList);
            }

            Array.Sort(_WordDocEntities);

            _WordsCount = _WordDocEntities.Length;
            SelectedIndexes = new int[_WordsCount];
            SelectedDocLists = new  Hubble.Core.Entity.OriginalDocumentPositionList[_WordsCount];
            TotalDocIdCount = 0;

            DBProvider dBProvider = _DBProvider;

            //vars for delete
            _HaveRecordsDeleted = dBProvider.DelProvider.Count > 0;
            _DelDocs = null;
            _CurDelIndex = 0;

            if (_HaveRecordsDeleted)
            {
                _DelDocs = dBProvider.DelProvider.DelDocs;
                _CurDelDocid = _DelDocs[_CurDelIndex];
            }
        }

        public void GetNextOriginal(ref Entity.OriginalDocumentPositionList odpl)
        {
            do
            {
                int fstDocId = _WordDocEntities[0].DocList.DocumentId;

                if (fstDocId < 0)
                {
                    odpl.DocumentId = fstDocId;
                    return;
                }

                //Find same docid 
                SelectedIndexes[0] = _WordDocEntities[0].Index;
                SelectedDocLists[0] = _WordDocEntities[0].DocList;
                SelectedCount = 1;
                TotalDocIdCount++;
                int minIndex = SelectedIndexes[0];

                for (int i = 1; i < _WordDocEntities.Length; i++)
                {
                    if (fstDocId == _WordDocEntities[i].DocList.DocumentId)
                    {
                        SelectedIndexes[i] = _WordDocEntities[i].Index;
                        SelectedDocLists[i] = _WordDocEntities[i].DocList;

                        if (minIndex > SelectedIndexes[i])
                        {
                            minIndex = SelectedIndexes[i];
                        }

                        SelectedCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                odpl = _WordDocEntities[0].DocList;

                for (int i = 0; i < SelectedCount; i++)
                {
                    //Get the next docid from first entity of the word
                    _WordDocEntities[0].WordIndex.WordIndex.GetNextOriginal(ref _WordDocEntities[0].DocList);

                    //Re sort the priority queue
                    int cur = 0;
                    int nextOne = cur + 1;

                    while (nextOne < _WordsCount)
                    {
                        if (_WordDocEntities[nextOne].DocList.DocumentId < 0)
                        {
                            break;
                        }

                        if (_WordDocEntities[cur].DocList.DocumentId > _WordDocEntities[nextOne].DocList.DocumentId)
                        {
                            //Swap when cur docid > next docid
                            WordDocIdEntity temp = _WordDocEntities[cur];

                            _WordDocEntities[cur] = _WordDocEntities[nextOne];

                            _WordDocEntities[nextOne] = temp;
                        }
                        else if (_WordDocEntities[cur].DocList.DocumentId < 0)
                        {
                            if (_WordDocEntities[cur].DocList.DocumentId < _WordDocEntities[nextOne].DocList.DocumentId)
                            {
                                //Swap when the docid of this word is empty from full text index
                                WordDocIdEntity temp = _WordDocEntities[cur];

                                _WordDocEntities[cur] = _WordDocEntities[nextOne];

                                _WordDocEntities[nextOne] = temp;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                        cur++;
                        nextOne++;
                    }
                }

                if (_HaveRecordsDeleted)
                {
                    if (_CurDelIndex < _DelDocs.Length)
                    {
                        int firstDocId = odpl.DocumentId;

                        //If docid deleted, get next
                        if (firstDocId == _CurDelDocid)
                        {
                            TotalDocIdCount--;
                            continue;
                        }
                        else if (firstDocId > _CurDelDocid)
                        {
                            //find the next deleted docid
                            while (_CurDelIndex < _DelDocs.Length && _CurDelDocid < firstDocId)
                            {
                                _CurDelIndex++;

                                if (_CurDelIndex >= _DelDocs.Length)
                                {
                                    _HaveRecordsDeleted = false;
                                    break;
                                }

                                _CurDelDocid = _DelDocs[_CurDelIndex];
                            }

                            if (_CurDelIndex < _DelDocs.Length)
                            {
                                if (firstDocId == _CurDelDocid)
                                {
                                    TotalDocIdCount--;
                                    continue;
                                }
                            }
                        }
                    }
                }

                if (_NeedGroupBy)
                {
                    if (TotalDocIdCount % _GroupByStep == 0 ||
                        (_OptimizeByScore && minIndex <= _IndexThreshold))
                    {
                        _GroupByDict.AddToGroupByCollection(odpl.DocumentId);
                    }

                    if (_GroupByDict.GroupByCollection.Count >= _GroupByLimit)
                    {
                        //more than group by limit, don't insert.
                        _NeedGroupBy = false;
                    }
                }

                if (_OptimizeByScore)
                {
                    if (minIndex > _IndexThreshold)
                    {
                        continue;
                    }
                }

                return;
            }
            while (true);
        }
            
    }
    
}
