using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Core.Query.Optimize
{
    class DocIdLongComparer : IComparer<Docid2Long>
    {
        bool _Asc;
        internal DocIdLongComparer(bool asc)
        {
            _Asc = asc;
        }

        #region IComparer<DocidCount> Members

        public int Compare(Docid2Long x, Docid2Long y)
        {
            if (_Asc)
            {
                if (y.Value1 > x.Value1)
                {
                    return -1;
                }
                else if (y.Value1 < x.Value1)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (y.Value1 > x.Value1)
                {
                    return 1;
                }
                else if (y.Value1 < x.Value1)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion
    }

    struct Docid2Long
    {
        internal int DocId;
        internal int Rank;
        internal long Value1;
        internal long Value2;

        public Docid2Long(int docid, long value1)
        {
            DocId = docid;
            Value1 = value1;
            Value2 = 0;
            Rank = 0;
        }

        public Docid2Long(int docid, long value1, long value2)
        {
            DocId = docid;
            Value1 = value1;
            Value2 = value2;
            Rank = 0;
        }




    }
}
