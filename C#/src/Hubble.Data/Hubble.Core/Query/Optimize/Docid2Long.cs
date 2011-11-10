using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hubble.Core.SFQL.SyntaxAnalysis.Select;

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

    public class DocId2LongComparer : IComparer<Docid2Long>
    {
        bool[] _Ascs;

        private int _ScoreFieldIndex = -1;

        public DocId2LongComparer(bool[] ascs, int scoreFieldIndex)
        {
            _ScoreFieldIndex = scoreFieldIndex;
            _Ascs = ascs;
        }

        public long GetScore(Docid2Long docid2Long)
        {
            switch (_ScoreFieldIndex)
            {
                case 0:
                    return docid2Long.Value1;
                case 1:
                    return docid2Long.Value2;
                case 2:
                    return docid2Long.Value3;
                default:
                    return 1;
            }
        }


        public static DocId2LongComparer Generate(Data.DBProvider dbProvider,
            OrderBy[] orderBys, out Data.Field[] orderByFields)
        {
            bool[] ascs = new bool[orderBys.Length];
            orderByFields = new Hubble.Core.Data.Field[orderBys.Length];

            int scoreFieldIndex = -1;

            for (int i = 0; i < ascs.Length; i++)
            {
                if (orderBys[i].Order == null)
                {
                    ascs[i] = true;
                }
                else
                {
                    ascs[i] = !orderBys[i].Order.Equals("desc", StringComparison.CurrentCultureIgnoreCase);
                }

                if (orderBys[i].Name.Equals("docid", StringComparison.CurrentCultureIgnoreCase))
                {
                    orderByFields[i] = new Hubble.Core.Data.Field("docid", Hubble.Core.Data.DataType.Int);
                }
                else if (orderBys[i].Name.Equals("score", StringComparison.CurrentCultureIgnoreCase))
                {
                    scoreFieldIndex = i;
                    orderByFields[i] = new Hubble.Core.Data.Field("score", Hubble.Core.Data.DataType.BigInt);
                }
                else
                {
                    orderByFields[i] = dbProvider.GetField(orderBys[i].Name);
                }
            }

            return new DocId2LongComparer(ascs, scoreFieldIndex);
        }


        #region IComparer<DocidCount> Members

        public int Compare(Docid2Long x, Docid2Long y)
        {
            int len = _Ascs.Length;
            for (int i = 0; i < _Ascs.Length; i++)
            {
                bool asc = _Ascs[i];
                long valueY;
                long valueX;

                switch (i)
                {
                    case 0:
                        valueX = x.Value1;
                        valueY = y.Value1;
                        break;
                    case 1:
                        valueX = x.Value2;
                        valueY = y.Value2;
                        break;
                    case 2:
                        valueX = x.Value3;
                        valueY = y.Value3;
                        break;
                    default:
                        valueX = 0;
                        valueY = 0;
                        break;

                }

                if (asc)
                {
                    if (valueY > valueX)
                    {
                        return -1;
                    }
                    else if (valueY < valueX)
                    {
                        return 1;
                    }
                    else
                    {
                        if (i < len - 1)
                        {
                            continue;
                        }

                        return 0;
                    }
                }
                else
                {
                    if (valueY > valueX)
                    {
                        return 1;
                    }
                    else if (valueY < valueX)
                    {
                        return -1;
                    }
                    else
                    {
                        if (i < len - 1)
                        {
                            continue;
                        }

                        return 0;
                    }
                }
            }

            return 0;
        }

        #endregion
    }


    public struct Docid2Long
    {
        internal int DocId;
        internal int Rank;
        internal long Value1;
        internal long Value2;
        internal long Value3;

        unsafe public static void Generate(ref Docid2Long docid2Long,
            Data.DBProvider dbProvider, Data.Field[] orderByFields, long score)
        {
            int* p = dbProvider.GetPayloadDataWithShareLock(docid2Long.DocId);

            if (p == null)
            {
                throw new Data.DataException(string.Format("DocId={0} does not exist in Payload",
                    docid2Long.DocId));
            }

            for (int i = 0; i < orderByFields.Length; i++)
            {
                Data.Field field = orderByFields[i];
                
                long value;

                if (field.Name == "score")
                {
                    value = score;
                }
                else if (field.Name == "docid")
                {
                    value = docid2Long.DocId;
                }
                else
                {
                    value = Data.DataTypeConvert.GetLongAnyWay(field.DataType, p,
                            field.TabIndex, field.SubTabIndex, field.DataLength);
                }

                switch (i)
                {
                    case 0:
                        docid2Long.Value1 = value;
                        break;
                    case 1:
                        docid2Long.Value2 = value;
                        break;
                    case 2:
                        docid2Long.Value3 = value;
                        break;
                    default:
                        throw new Data.DataException("Order by fields length for optimization must be <= 3");
                }
            }
        }

        public Docid2Long(int docid, long value1)
        {
            DocId = docid;
            Value1 = value1;
            Value2 = 0;
            Value3 = 0;
            Rank = 0;
        }

        public Docid2Long(int docid, long value1, long value2)
        {
            DocId = docid;
            Value1 = value1;
            Value2 = value2;
            Value3 = 0;
            Rank = 0;
        }

        public Docid2Long(int docid, long value1, long value2, long value3)
        {
            DocId = docid;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Rank = 0;
        }



    }
}
