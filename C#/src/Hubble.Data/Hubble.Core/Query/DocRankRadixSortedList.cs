using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Query
{

    public class DocRankRadixSortedList : IEnumerable<DocumentRank>
    {
        class DescComparer : System.Collections.Generic.IComparer<int>
        {
            #region IComparer<int> ≥…‘±

            public int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }

            #endregion
        }

        const int TableSize = 260;
        int _Count = 0;
        int _Top = int.MaxValue;

        //int _MaxRadix = -1;
        int _MinRadix = 0;

        SortedList<int, List<DocumentRank>> _HitRadix;
        static DescComparer descCompare = new DescComparer(); 
            
        List<DocumentRank>[] _RadixTable = new List<DocumentRank>[TableSize];
        bool[] _RadixSortedTable = new bool[TableSize];

        public int Count
        {
            get
            {
                return _Count;
            }
        }

        public int Top
        {
            get
            {
                return _Top;
            }

            set
            {
                if (value <= 0)
                {
                    _Top = 1;
                }
                else
                {
                    _Top = value;
                }
            }
        }

        private void CalculateMinRadix()
        {
            int count = 0;

            foreach (int radix in _HitRadix.Keys)
            {
                count += _RadixTable[radix].Count;

                if (count > Top)
                {
                    _MinRadix = radix;
                    break;
                }
            }


            //int count = 0;
            //for (int i = _HitRadix.Count - 1; i >= 0; i--)
            //{

            //    if (_RadixTable[i] != null)
            //    {
            //        count += _RadixTable[i].Count;
            //    }

            //    if (count > Top)
            //    {
            //        _MinRadix = i;
            //        break;
            //    }
            //}
        }

        public DocRankRadixSortedList(int top)
        {
            Clear();
            _Top = top;
        }

        public DocRankRadixSortedList()
        {
            Clear();
        }

        public void Clear()
        {
            _Count = 0;
            _Top = int.MaxValue;

            //_MaxRadix = -1;
            _MinRadix = 0;

            _HitRadix = new SortedList<int, List<DocumentRank>>(descCompare);
            _RadixTable = new List<DocumentRank>[TableSize];
            _RadixSortedTable = new bool[TableSize];
        }

        public void Add(DocumentRank docRank)
        {
            Debug.Assert(docRank.Rank >= 0);

            int radix;

            if (docRank.Rank < 128 * 16)
            {
                radix = docRank.Rank / 128;
            }
            else if (docRank.Rank < 32768 + 128 * 16)
            {
                radix = (docRank.Rank - (128 * 16)) / 128;
            }
            else if (docRank.Rank < 100000)
            {
                radix = 256;
            }
            else if (docRank.Rank < 1000000)
            {
                radix = 257;
            }
            else if (docRank.Rank < 10000000)
            {
                radix = 258;
            }
            else
            {
                radix = 259;
            }

            if (radix < _MinRadix)
            {
                _Count++;
                return;
            }

            if (_RadixTable[radix] == null)
            {
                _RadixTable[radix] = new List<DocumentRank>();
                _HitRadix.Add(radix, _RadixTable[radix]);

                //if (_MaxRadix < radix)
                //{
                //    _MaxRadix = radix;
                //}
            }

            _RadixTable[radix].Add(docRank);

            _Count++;

            if (_Top < int.MaxValue)
            {
                if (_Count % Top == 0)
                {
                    CalculateMinRadix();
                }
            }
        }


        #region IEnumerable<DocumentRank> Members

        public IEnumerator<DocumentRank> GetEnumerator()
        {
            int radix = 259;
            int curIndex = 0;
            int count = 0;

            while (radix >= 0)
            {
                if (_RadixTable[radix] != null)
                {
                    if (!_RadixSortedTable[radix])
                    {
                        _RadixSortedTable[radix] = true;

                        if (_RadixTable[radix].Count > 1)
                        {
                            _RadixTable[radix].Sort();
                        }
                    }

                    yield return _RadixTable[radix][curIndex];
                    count++;
                    if (count >= Top)
                    {
                        yield break;
                    }

                    curIndex++;

                    if (curIndex >= _RadixTable[radix].Count)
                    {
                        curIndex = 0;
                        radix--;
                    }
                }
                else
                {
                    radix--;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
