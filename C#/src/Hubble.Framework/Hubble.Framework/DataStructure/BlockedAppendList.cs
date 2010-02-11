using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.DataStructure
{
    static class BlockedUnitAlloc<T> where T: struct
    {
        static internal void Init()
        { 
        }


        internal class BlockedUnit
        {
            internal bool Static;
            public T[] Data;

            public BlockedUnit(bool stat, T[] data)
            {
                Static = stat;
                Data = data;
            }
        }

        internal const int UnitSize = 8192;

        static object _SynObj = new object();

        static Queue<BlockedUnit> _FreeQueue;

        static BlockedUnitAlloc()
        {
            _FreeQueue = new Queue<BlockedUnit>();

            for (int i = 0; i < 1024; i++)
            {
                T[] unit = new T[UnitSize];
                _FreeQueue.Enqueue(new BlockedUnit(true, unit));
            }
        }

        static internal BlockedUnit GetUnit()
        {
            lock (_SynObj)
            {
                if (_FreeQueue.Count > 0)
                {
                    return _FreeQueue.Dequeue();
                }
                else
                {
                    return new BlockedUnit(false, new T[UnitSize]);
                }
            }
        }

        static internal void RetUnit(BlockedUnit unit)
        {
            lock (_SynObj)
            {
                if (unit.Static)
                {
                    _FreeQueue.Enqueue(unit);
                }
            }
        }
    }


    public class BlockedAppendList<T> : IDisposable where T : struct
    {
        private List<BlockedUnitAlloc<T>.BlockedUnit> _Segment = new List<BlockedUnitAlloc<T>.BlockedUnit>(256);

        private int _Count;
        private int _LastSegmentLength;
        private BlockedUnitAlloc<T>.BlockedUnit _LastSegment;
        private BlockedUnitAlloc<T>.BlockedUnit _LastReadSegment = null;
        private int _LastReadIndex = 0;
        private int _LastSegmentReadIndex = 0;
        private int _UnitSize;

        public BlockedAppendList() 
        {
            BlockedUnitAlloc<T>.Init();
            _Count = 0;
            _LastSegmentLength = 0;
            _UnitSize = BlockedUnitAlloc<T>.UnitSize;
        }

        ~BlockedAppendList()
        {
            try
            {
                if (_Segment != null)
                {
                    Dispose();
                }
            }
            catch
            {
            }

        }

        public int Count
        {
            get
            {
                return _Count;
            }
        }


        public void Add(T value)
        {
            if (_LastSegmentLength == 0)
            {
                _LastSegment = BlockedUnitAlloc<T>.GetUnit();
 
                _Segment.Add(_LastSegment);
            }

            _LastSegment.Data[_LastSegmentLength] = value;

            _Count++;
            if (++_LastSegmentLength == _UnitSize)
            {
                _LastSegmentLength = 0;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index >= _Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (index - _LastReadIndex != 1)
                {
                    _LastReadSegment = _Segment[index / _UnitSize];
                    _LastSegmentReadIndex = index % _UnitSize;
                }
                else
                {
                    _LastSegmentReadIndex++;
                    if (_LastSegmentReadIndex >= _UnitSize)
                    {
                        _LastReadSegment = _Segment[index / _UnitSize];
                        _LastSegmentReadIndex = 0;
                    }
                }

                _LastReadIndex = index;

                return _LastReadSegment.Data[_LastSegmentReadIndex];
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                int count = _Count;

                while (count > 0)
                {
                    for (int i = 0; i < _Segment.Count; i++)
                    {
                        BlockedUnitAlloc<T>.BlockedUnit unit = _Segment[i];

                        int j = 0;
                        while (count > 0 && j < unit.Data.Length)
                        {
                            yield return unit.Data[j];
                            j++;
                            count--;
                        }
                    }
                }
            }
        }

        

        #region IDisposable Members

        public void Dispose()
        {
            foreach(BlockedUnitAlloc<T>.BlockedUnit unit in _Segment)
            {
                if (unit == null)
                {
                    break;
                }

                BlockedUnitAlloc<T>.RetUnit(unit);
            }

            _Segment.Clear();
            _Segment = null;
        }

        #endregion
    }
}
