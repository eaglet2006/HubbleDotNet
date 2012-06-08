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

namespace Hubble.Framework.DataStructure
{
    public class PriorQueue<T>
    {
        IComparer<T> _Comparer;

        T[] _Queue;

        int _QueueLength;
        int _Count;
        int _Start;

        public int QueueLength
        {
            get
            {
                return _QueueLength;
            }
        }

        public int Count
        {
            get
            {
                return _Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len">Queue Length</param>
        public PriorQueue(int len, IComparer<T> comparer)
        {
            if (len < 0)
            {
                throw new System.ArgumentException("Queue length less than zero");
            }

            _Start = 0;
            _QueueLength = len;
            _Comparer = comparer;
            _Queue = new T[_QueueLength];
            _Count = 0;
        }

        public void Add(T value)
        {
            if (_QueueLength == 0)
            {
                return;
            }

            if (_Count >= _QueueLength)
            {
                //Judge last element

                int index = _Start + _Count - 1;

                if (index >= _Count)
                {
                    index = index - _Count;
                }

                if (_Comparer.Compare(value, _Queue[index]) >= 0)
                {
                    return;
                }

                //Judge first element

                index = _Start;
                if (_Comparer.Compare(value, _Queue[index]) <= 0)
                {
                    _Start--;
                    if (_Start < 0)
                    {
                        _Start = _Count - 1;
                    }

                    _Queue[_Start] = value;
                }
                else
                {
                    int pos = BinarySearch<T>.FindClosedIndex(_Queue, _Start, value, _Comparer);

                    if (_Start > 0)
                    {
                        if (_Start > pos)
                        {
                            Array.Copy(_Queue, pos, _Queue, pos + 1, _Start - pos - 1);
                        }
                        else
                        {
                            T lst = _Queue[_QueueLength - 1];
                            Array.Copy(_Queue, 0, _Queue, 1, _Start - 1);
                            _Queue[0] = lst;
                            Array.Copy(_Queue, pos, _Queue, pos + 1, _QueueLength - pos - 1);
                        }

                        _Queue[pos] = value;
                    }
                    else
                    {
                        Array.Copy(_Queue, pos, _Queue, pos + 1, _QueueLength - pos - 1);
                        _Queue[pos] = value;
                    }
                }
            }
            else
            {
                int pos = BinarySearch<T>.FindClosedIndex(_Queue, 0, _Count, value, _Comparer);

                if (pos >= _QueueLength)
                {
                    return;
                }

                if (pos == _QueueLength - 1)
                {
                    _Queue[pos] = value;
                }
                else
                {
                    Array.Copy(_Queue, pos, _Queue, pos + 1, _QueueLength - pos - 1);
                    _Queue[pos] = value;
                }

                _Count++;
            }
        }

        public T[] ToArray()
        {
            if (_Count < _QueueLength)
            {
                T[] result = new T[_Count];
                Array.Copy(_Queue, result, _Count);
                return result;
            }
            else
            {
                T[] result = new T[_Count];
                Array.Copy(_Queue, _Start, result, 0, _Count - _Start);
                Array.Copy(_Queue, 0, result, _Count - _Start, _Start);
                return result;
            }
        }

        public T First
        {
            get
            {
                return _Queue[_Start];
            }
        }

        public T Last
        {
            get
            {
                int last = _Start + _Count - 1;

                if (last >= _Count)
                {
                    last = last - _Count;
                }
                return _Queue[last];
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                for (int i = _Start; i < _Count; i++)
                {
                    yield return _Queue[i];
                }

                for (int i = 0; i < _Start; i++)
                {
                    yield return _Queue[i];
                }
            }
        }
    }



    public class PriorQueueOld<T> 
    {
        IComparer<T> _Comparer;

        T[] _Queue;

        int _QueueLength;
        int _Count;

        public int QueueLength
        {
            get
            {
                return _QueueLength;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len">Queue Length</param>
        public PriorQueueOld(int len, IComparer<T> comparer)
        {
            if (len < 0)
            {
                throw new System.ArgumentException("Queue length less than zero");
            }

            _QueueLength = len;
            _Comparer = comparer;
            _Queue = new T[_QueueLength];
            _Count = 0;
        }

        public void Add(T value)
        {
            if (_QueueLength == 0)
            {
                return;
            }

            if (_Count >= _QueueLength)
            {
                if (_Comparer.Compare(value, _Queue[_Count - 1]) >= 0)
                {
                    return;
                }
            }

            int pos = BinarySearch<T>.FindClosedIndex(_Queue, 0, _Count, value, _Comparer);

            if (pos >= _QueueLength)
            {
                return;
            }

            if (pos == _QueueLength - 1)
            {
                _Queue[pos] = value;
            }
            else
            {
                Array.Copy(_Queue, pos, _Queue, pos + 1, _QueueLength - pos - 1);
                _Queue[pos] = value;
            }

            if (_Count < _QueueLength)
            {
                _Count++;
            }
        }

        public T[] ToArray()
        {
            if (_Count < _QueueLength)
            {
                T[] result = new T[_Count];
                Array.Copy(_Queue, result, _Count);
                return result;
            }
            else
            {
                return _Queue;
            }
        }

        public T First
        {
            get
            {
                return _Queue[0];
            }
        }

        public T Last
        {
            get
            {
                return _Queue[_Count - 1];
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                for (int i = 0; i < _Count; i++)
                {
                    yield return _Queue[i];
                }
            }
        }
    }
}
