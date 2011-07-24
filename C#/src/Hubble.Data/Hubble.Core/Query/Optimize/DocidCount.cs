using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Query.Optimize
{
    class DocIdCountComparer : IComparer<DocidCount>
    {
        #region IComparer<DocidCount> Members

        public int Compare(DocidCount x, DocidCount y)
        {
            if (y.Count > x.Count)
            {
                return 1;
            }
            else if (y.Count < x.Count)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        #endregion
    }

    struct DocidCount
    {
        internal int DocId;
        internal int Count;
        internal int TotalWordsInThisDocument;

        internal DocidCount(int docid, int count)
        {
            this.DocId = docid;
            this.Count = count;
            this.TotalWordsInThisDocument = 0;
        }

        internal DocidCount(int docid, int count, int totalWordsInThisDoc)
        {
            this.DocId = docid;
            this.Count = count;
            this.TotalWordsInThisDocument = totalWordsInThisDoc;
        }
    }

}
