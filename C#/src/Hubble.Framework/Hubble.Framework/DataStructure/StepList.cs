using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.DataStructure
{
    /// <summary>
    /// Step List 
    /// is a list can append step by step
    /// </summary>
    public class StepList<T>
    {
        int _InitCapability;
        int _Capability;
        int _StartStep;
        int _Step;
        int _Count;

        T[] _Data;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initCapability">initial capability</param>
        /// <param name="startStep">when capablility large then this value begin to step</param>
        /// <param name="step">Append bytes every step</param>
        public StepList(int initCapability, int startStep, int step)
        {
            _Data = new T[initCapability];
            _Capability = initCapability;
            _InitCapability = initCapability;
            _StartStep = startStep;
            _Step = step;
            _Count = 0;
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
            if (_Count < _Capability)
            {
                _Data[_Count] = value;
                _Count++;
            }
            else
            {
                int destCapability;

                if (_Capability < _StartStep)
                {
                    if (_Capability * 2 < _StartStep)
                    {
                        destCapability = _Capability * 2;
                    }
                    else
                    {
                        destCapability = _StartStep;
                    }
                }
                else
                {
                    destCapability = _Capability + _Step;
                }

                T[] buf = new T[destCapability];

                _Data.CopyTo(buf, 0);

                _Data = null;
                _Data = buf;

                _Capability = destCapability;

                _Data[_Count] = value;
                _Count++;
            }
        }

        public T[] ToArray()
        {
            return _Data;
        }

        public T this[int index]
        {
            get
            {
                if (index >= _Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return _Data[index];
            }

            set
            {
                if (index >= _Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _Data[index] = value;
            }
        }

        public int BinarySearch(T value)
        {
            return Array.BinarySearch(_Data, 0, _Count, value); 
        }

        public bool TryGetValue(T key, out T value)
        {
            int index = Array.BinarySearch(_Data, 0, _Count, key);
            if (index < 0)
            {
                value = default(T);
                return false;
            }
            else
            {
                value = _Data[index];
                return true;
            }
        }

        public void Clear()
        {
            _Data = new T[_InitCapability];
            _Capability = _InitCapability;
            _Count = 0;
        }
    }
}
