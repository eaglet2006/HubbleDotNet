using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Serialization = Hubble.SQLClient.QueryResultSerialization;

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

    public class DocumentResultComparer : IComparer<DocumentResult>
    {
        #region IComparer<DocumentResult> Members

        public int Compare(DocumentResult x, DocumentResult y)
        {
            return x.CompareTo(y);
        }

        #endregion
    }

    [StructLayout(LayoutKind.Auto)]
    unsafe public struct DocumentResult : IComparable<DocumentResult>
    {
        public long Score;
        public long SortValue; //if SortInfoList == null, use SortValue to sort
        public int* PayloadData;
        public object Tag;
        public List<SortInfo> SortInfoList;
        public int DocId;
        public bool Asc; // Contain with SortValue

        public int LastPosition
        {
            get
            {
                return (int)(SortValue & 0xFFFFFFFF);
            }
        }

        public int lastIndex
        {
            get
            {
                return (int)((SortValue / 0x100000000) & 0xFFFF);
            }
        }

        public int LastCount
        {
            get
            {
                return (int)(SortValue / 0x1000000000000);
            }
        }

        public DocumentResult(int docId)
            :this(docId, 1)
        {
        }

        public DocumentResult(int docId, long score, object tag, int lastPostion, int lastCount, int lastIndex)
            : this(docId, score, (int*)null)
        {
            Tag = tag;
            SortValue = lastCount * 0x1000000000000 + lastIndex * 0x100000000 + lastPostion;
        }

        public DocumentResult(int docId, long score)
            : this(docId, score, (int*)null)
        {
        }

        public DocumentResult(int docId, long score, int* payload)
        {
            DocId = docId;
            Score = score;
            PayloadData = payload;
            SortValue = 0;
            Asc = true;
            SortInfoList = null;
            Tag = null;
        }

        public void Serialize(System.IO.Stream stream)
        {
            Serialization.Write(stream, typeof(int), this.DocId);
            Serialization.Write(stream, typeof(long), this.Score);
        }

        public static DocumentResult Deserialize(System.IO.Stream stream)
        {
            int docId = (int)Serialization.Read(stream,
                 Serialization.DataType.Int);

            long score = (long)Serialization.Read(stream,
                 Serialization.DataType.Long);

            return new DocumentResult(docId, score);
        }

        #region IComparable<DocumentResult> Members

        public int CompareTo(DocumentResult other)
        {
            if (other.SortInfoList == null)
            {
                if (Asc)
                {
                    if (this.SortValue > other.SortValue)
                    {
                        return 1;
                    }
                    else if (this.SortValue < other.SortValue)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (this.SortValue < other.SortValue)
                    {
                        return 1;
                    }
                    else if (this.SortValue > other.SortValue)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            else
            {
                for (int i = 0; i < other.SortInfoList.Count; i++)
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
        }

        #endregion
    }
}
