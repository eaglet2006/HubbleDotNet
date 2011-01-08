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
    /// <summary>
    /// Heap sort
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HeapSort<T>
    {
        private static void HeapAdjust(T[] array, int root, int bottom, IComparer<T> comparer)
        {
            T temp;

            for (int j = root * 2 + 1; j <= bottom; j = j * 2 + 1)
            {
                if (j < bottom && comparer.Compare(array[root * 2 + 1], array[root * 2 + 2]) < 0)
                {
                    j++;
                }

                if (comparer.Compare(array[root], array[j]) >= 0)
                {
                    break;
                }
                else
                {
                    temp = array[root];
                    array[root] = array[j];
                    array[j] = temp;
                    root = j;
                }
            }
        }

        public static void Sort(T[] array, int len, IComparer<T> comparer)
        {
            int i;
            T temp;

            int mid = (len / 2) - 1;

            for (i = mid; i >= 0; i--)
            {
                HeapAdjust(array, i, len - 1, comparer);
            }

            for (i = len - 1; i >= 1; i--)
            {
                temp = array[0];
                array[0] = array[i];
                array[i] = temp;
                HeapAdjust(array, 0, i - 1, comparer);
            }
        }

        public static void Sort(T[] array, IComparer<T> comparer)
        {
            Sort(array, array.Length, comparer);
        }

        public static void TopSort(T[] array, int top, IComparer<T> comparer)
        {
            if (top <= 0)
            {
                return;
            }

            if (top >= array.Length)
            {
                Sort(array, comparer);
            }
            else
            {
                //init top heap
                int i;
                T temp;

                int mid = (top / 2) - 1;
                
                for (i = mid; i >= 0; i--)
                {
                    HeapAdjust(array, i, top - 1, comparer);
                }

                for (i = top; i < array.Length; i++)
                {
                    if (comparer.Compare(array[0], array[i]) <= 0)
                    {
                        continue;
                    }
                    else
                    {
                        array[0] = array[top - 1];
                        array[top - 1] = array[i];

                        if (comparer.Compare(array[top - 1], array[0]) <= 0)
                        {
                            HeapAdjust(array, 0, top - 1, comparer);
                        }
                        else
                        {
                            mid = (top / 2) - 1;

                            for (int j = mid; j >= 0; j--)
                            {
                                HeapAdjust(array, j, top - 1, comparer);
                            }
                        }
                    }
                }

                for (i = top - 1; i >= 1; i--)
                {
                    temp = array[0];
                    array[0] = array[i];
                    array[i] = temp;
                    HeapAdjust(array, 0, i - 1, comparer);
                }
            }
        }
    }
}
