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
using System.Runtime.InteropServices;

using Serialization = Hubble.SQLClient.QueryResultSerialization;

namespace Hubble.Core.Query
{
    public enum SortType
    {
        None = 0,
        Int = 1,
        Long = 2,
        Double = 3,
        String = 4,
    }

    public struct SortInfo
    {
        public bool Asc;
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
            LongValue = 0;
            DoubleValue = 0;
            StringValue = null;
        }

        public SortInfo(bool asc, SortType sortType, long value)
        {
            Asc = asc;
            SortType = sortType;
            IntValue = 0;
            LongValue = value;
            DoubleValue = 0;
            StringValue = null;
        }

        public SortInfo(bool asc, SortType sortType, double value)
        {
            Asc = asc;
            SortType = sortType;
            IntValue = 0;
            LongValue = 0;
            DoubleValue = value;
            StringValue = null;

        }

        public SortInfo(bool asc, SortType sortType, string value)
        {
            Asc = asc;
            SortType = sortType;
            IntValue = 0;
            LongValue = 0;
            DoubleValue = 0;
            StringValue = value;
        }
    }

    public class DocumentResultForSortComparer : IComparer<DocumentResultForSort>
    {
        #region IComparer<DocumentResultForSort> Members

        public int Compare(DocumentResultForSort x, DocumentResultForSort y)
        {
            return x.CompareTo(y);
        }

        #endregion
    }

    [StructLayout(LayoutKind.Auto)]
    unsafe public struct DocumentResultForSort : IComparable<DocumentResultForSort>
    {
        public long Score;
        public long SortValue; //if SortInfoList == null, use SortValue to sort
        
        [System.Xml.Serialization.XmlIgnore]
        public int* PayloadData;

        //[System.Xml.Serialization.XmlIgnore]
        //public object Tag;

        public List<SortInfo> SortInfoList;
        public int DocId;
        public bool Asc; // Contain with SortValue

        unsafe public DocumentResultForSort(DocumentResult* pDocResult)
            : this(pDocResult->DocId, pDocResult->Score)
        {

        }

        public DocumentResultForSort(int docId)
            : this(docId, 1)
        {
        }

        public DocumentResultForSort(int docId, long score, object tag, int lastPostion, int lastCount, int lastIndex)
            : this(docId, score, (int*)null)
        {
            //Tag = tag;
            SortValue = lastCount * 0x1000000000000 + lastIndex * 0x100000000 + lastPostion;
        }

        public DocumentResultForSort(int docId, long score)
            : this(docId, score, (int*)null)
        {
        }

        public DocumentResultForSort(int docId, long score, int* payload)
        {
            DocId = docId;
            Score = score;
            PayloadData = payload;
            SortValue = 0;
            Asc = true;
            SortInfoList = null;
            //Tag = null;
        }

        public void Serialize(System.IO.Stream stream)
        {
            Serialization.Write(stream, typeof(int), this.DocId);
            Serialization.Write(stream, typeof(long), this.Score);
        }

        public static DocumentResultForSort Deserialize(System.IO.Stream stream)
        {
            int docId = (int)Serialization.Read(stream,
                 Serialization.DataType.Int);

            long score = (long)Serialization.Read(stream,
                 Serialization.DataType.Long);

            return new DocumentResultForSort(docId, score);
        }

        #region IComparable<DocumentResultForSort> Members

        public int CompareTo(DocumentResultForSort other)
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
