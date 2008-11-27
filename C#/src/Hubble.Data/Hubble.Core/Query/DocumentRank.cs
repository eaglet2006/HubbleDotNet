using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Query
{
    public struct DocumentRank : IComparable<DocumentRank>
    {
        public long DocumentId;
        public int Rank;

        public DocumentRank(long docId)
            : this(docId, 1)
        {

        }

        public DocumentRank(long docId, int rank)
        {
            DocumentId = docId;
            Rank = rank;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", DocumentId, Rank);
        }

        #region IComparable<DocumentRank> Members

        public int CompareTo(DocumentRank other)
        {
            return 0 - Rank.CompareTo(other.Rank);
        }

        #endregion
    }
}
