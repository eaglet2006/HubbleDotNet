using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Query
{
    public enum SortType
    {
        None = 0,
        Int = 1,
        Long= 2,
        Double= 3,
        String= 4,
    }

    public class SortInfo
    {
        public bool Asc = true;
        public SortType SortType;
        public int IntValue;
        public long LongValue;
        public Double DoubleValue;
        public string StringValue;

        public SortInfo(bool asc, SortType sortType, int value)
        {
            Asc = asc;
            SortType = sortType;
            IntValue = value;
        }

        public SortInfo(bool asc, SortType sortType, long value)
        {
            Asc = asc;
            SortType = sortType;
            LongValue = value;
        }

        public SortInfo(bool asc, SortType sortType, double value)
        {
            Asc = asc;
            SortType = sortType;
            DoubleValue = value;
        }

        public SortInfo(bool asc, SortType sortType, string value)
        {
            Asc = asc;
            SortType = sortType;
            StringValue = value;
        }

    }

    public class DocumentResult : IComparable<DocumentResult>
    {
        public long DocId;
        public long Score;
        public int[] Payload;
        public List<SortInfo> SortInfoList = null;

        public DocumentResult(long docId)
            :this(docId, 0)
        {
        }

        public DocumentResult(long docId, long score)
            : this(docId, score, null)
        {
        }

        public DocumentResult(long docId, long score, int[] payload)
        {
            DocId = docId;
            Score = score;
            Payload = payload;
        }

        #region IComparable<DocumentResult> Members

        public int CompareTo(DocumentResult other)
        {
            for (int i = 0; i< other.SortInfoList.Count; i++)
            {
                SortInfo src = this.SortInfoList[i];
                SortInfo dest = other.SortInfoList[i];

                if (src.Asc)
                {
                    switch (src.SortType)
                    {
                        case SortType.Int:
                            if (src.IntValue > dest.IntValue)
                            {
                                return 1;
                            }
                            else if (src.IntValue < dest.IntValue)
                            {
                                return -1;
                            }
                            break;
                        case SortType.Long:
                            if (src.LongValue > dest.LongValue)
                            {
                                return 1;
                            }
                            else if (src.LongValue < dest.LongValue)
                            {
                                return -1;
                            }
                            break;
                        case SortType.Double:
                            if (src.DoubleValue > dest.DoubleValue)
                            {
                                return 1;
                            }
                            else if (src.DoubleValue < dest.DoubleValue)
                            {
                                return -1;
                            }
                            break;
                        case SortType.String:
                            int result = src.StringValue.CompareTo(dest.StringValue);

                            if (result != 0)
                            {
                                return result;
                            }
                            break;
                    }
                }
                else
                {
                    switch (src.SortType)
                    {
                        case SortType.Int:
                            if (src.IntValue > dest.IntValue)
                            {
                                return -1;
                            }
                            else if (src.IntValue < dest.IntValue)
                            {
                                return 1;
                            }
                            break;
                        case SortType.Long:
                            if (src.LongValue > dest.LongValue)
                            {
                                return -1;
                            }
                            else if (src.LongValue < dest.LongValue)
                            {
                                return 1;
                            }
                            break;
                        case SortType.Double:
                            if (src.DoubleValue > dest.DoubleValue)
                            {
                                return -1;
                            }
                            else if (src.DoubleValue < dest.DoubleValue)
                            {
                                return 1;
                            }
                            break;
                        case SortType.String:

                            int result = dest.StringValue.CompareTo(src.StringValue);

                            if (result != 0)
                            {
                                return result;
                            }
                            break;
                    }
                }
            }

            return 0;
        }

        #endregion
    }
}
