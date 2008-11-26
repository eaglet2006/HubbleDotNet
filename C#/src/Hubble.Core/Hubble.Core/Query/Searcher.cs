using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Query
{
    public class Searcher
    {
        List<DocumentRank> _DocRankList = new List<DocumentRank>();

        int _TotalCount;
        int _MinRank = int.MaxValue;
        int _SortedLength = 100;
        bool _HasSorted = false;
        IQuery _Query;

        public int TotalCount
        {
            get
            {
                return _TotalCount;
            }
        }

        private void Sort()
        {
            _TotalCount = 0;
            foreach (DocumentRank docRank in _Query.GetRankEnumerable())
            {
                if (_MinRank > docRank.Rank)
                {
                    if (_TotalCount <= _SortedLength)
                    {
                        _MinRank = docRank.Rank;
                    }
                }
                _DocRankList.Add(docRank);
                _TotalCount++;
            }

            _DocRankList.Sort();
            _HasSorted = true;
        }

        public Searcher(IQuery query)
        {
            _Query = query;
        }

        public void Search()
        {
            if (!_HasSorted)
            {
                _SortedLength = 100;
                Sort();
            }
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
            if (!_HasSorted || length > _SortedLength)
            {
                _SortedLength = length;
                Sort();
            }

            for(int i = first; i < first + length; i++)
            {
                yield return _DocRankList[i];
            }

        }

        public IEnumerable<DocumentRank> Get()
        {
            if (!_HasSorted || int.MaxValue != _SortedLength)
            {
                _SortedLength = int.MaxValue;
                Sort();
            }

            foreach(DocumentRank docRank in _DocRankList)
            {
                yield return docRank;
            }

        }
    }
}
