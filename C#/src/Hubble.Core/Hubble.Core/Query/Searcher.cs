using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Query
{
    public class Searcher
    {
        Hubble.Framework.DataType.AppendList<DocumentRank> _DocRankList;

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

            if (_SortedLength <= 1024)
            {
                _DocRankList = new Hubble.Framework.DataType.AppendList<DocumentRank>(_SortedLength * 2);
            }
            else
            {
                _DocRankList = new Hubble.Framework.DataType.AppendList<DocumentRank>();
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

            for(int i = first; i < Math.Min(_DocRankList.Count, first + length); i++)
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
