using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Query
{
    public class Searcher
    {
        Hubble.Framework.DataStructure.AppendList<DocumentRank> _DocRankList;
        DocRankRadixSortedList _DocRankRadixSortedList;

        int _TotalCount;
        int _MinRank = int.MaxValue;
        int _SortedLength = 100;
        bool _HasSorted = false;
        IQuery _Query;

        public int TotalCount
        {
            get
            {
                //return _TotalCount;

                if (_DocRankRadixSortedList == null)
                {
                    return 0;
                }
                else
                {
                    return _DocRankRadixSortedList.Count;
                }
            }
        }

        private void Sort1()
        {
            _TotalCount = 0;

            if (_SortedLength <= 1024)
            {
                _DocRankList = new Hubble.Framework.DataStructure.AppendList<DocumentRank>(_SortedLength * 2);
            }
            else
            {
                _DocRankList = new Hubble.Framework.DataStructure.AppendList<DocumentRank>();
            }

            foreach (DocumentRank docRank in _Query.GetRankEnumerable())
            {
                if (_MinRank > docRank.Rank)
                {
                    if (_TotalCount <= _SortedLength)
                    {
                        _MinRank = docRank.Rank;
                    }
                    else
                    {
                        _TotalCount++;
                        continue;
                    }
                }

                _DocRankList.Add(docRank);

                if (_SortedLength <= 1024)
                {
                    if (_DocRankList.Count >= _SortedLength * 2)
                    {
                        _DocRankList.Sort();
                        _DocRankList.ReduceSize(_SortedLength);
                        _MinRank = _DocRankList[_DocRankList.Count - 1].Rank;
                    }
                }

                _TotalCount++;
            }

            _DocRankList.Sort();
            _HasSorted = true;
        }

        public void Sort(int top)
        {
            _DocRankRadixSortedList = new DocRankRadixSortedList(top);
            foreach (DocumentRank docRank in _Query.GetRankEnumerable())
            {
                _DocRankRadixSortedList.Add(docRank);
            }

            _HasSorted = true;

        }

        public Searcher(IQuery query)
        {
            _Query = query;
        }

        public void Search()
        {
            Dictionary<long, DocumentRank> docRankTbl = _Query.Search();

            if (docRankTbl.Count == 0)
            {
                docRankTbl = new Dictionary<long, DocumentRank>();
            }

            //if (!_HasSorted)
            //{
            //    //_SortedLength = 100;
            //    Sort(10);
            //}
        }

        /// <summary>
        /// Search
        /// </summary>
        /// <param name="first">First row number</param>
        /// <param name="length">How many rows you want to get.</param>
        /// <param name="totalCount">Total number of rows in this query</param>
        /// <param name="query">query</param>
        /// <returns></returns>
        public IEnumerable<DocumentRank> Get(int first, int length)
        {
            if (!_HasSorted)
            {
                _HasSorted = false;
                Sort(first + length);
            }

            if (first + length > _DocRankRadixSortedList.Top)
            {
                _HasSorted = false;
                Sort(first + length);
            }

            int i = 0;
            foreach (DocumentRank docRank in _DocRankRadixSortedList)
            {
                if (i < first)
                {
                    i++;
                    continue;
                }

                if (i >= first + length)
                {
                    yield break;
                }

                yield return docRank;
            }
        }

        public IEnumerable<DocumentRank> Get()
        {
            if (!_HasSorted)
            {
                _HasSorted = false;
                Sort(int.MaxValue);
            }

            if (int.MaxValue != _DocRankRadixSortedList.Top)
            {
                _HasSorted = false;
                Sort(int.MaxValue);
            }

            foreach (DocumentRank docRank in _DocRankRadixSortedList)
            {
                yield return docRank;
            }

        }
    }
}
