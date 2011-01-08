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

using Hubble.Core.Query;

namespace Hubble.Core.SFQL.Parse
{
    /**/
    /// <summary>
    /// Quick Sort
    /// </summary>
    /// <typeparam name="DocumentResultForSort"></typeparam>
    public class QueryResultHeapSort
    {
        private static void HeapAdjust(DocumentResultForSort[] array, int root, int bottom, bool asc)
        {
            if (asc)
            {
                DocumentResultForSort temp;

                for (int j = root * 2 + 1; j <= bottom; j = j * 2 + 1)
                {
                    if (j < bottom && array[root * 2 + 1].SortValue < array[root * 2 + 2].SortValue)
                    {
                        j++;
                    }

                    if (array[root].SortValue >= array[j].SortValue)
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
            else
            {
                DocumentResultForSort temp;

                for (int j = root * 2 + 1; j <= bottom; j = j * 2 + 1)
                {
                    if (j < bottom && array[root * 2 + 1].SortValue > array[root * 2 + 2].SortValue)
                    {
                        j++;
                    }

                    if (array[root].SortValue <= array[j].SortValue)
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
        }

        public static void Sort(DocumentResultForSort[] array, int len, bool asc)
        {
            int i;
            DocumentResultForSort temp;

            int mid = (len / 2) - 1;

            for (i = mid; i >= 0; i--)
            {
                HeapAdjust(array, i, len - 1, asc);
            }

            for (i = len - 1; i >= 1; i--)
            {
                temp = array[0];
                array[0] = array[i];
                array[i] = temp;
                HeapAdjust(array, 0, i - 1, asc);
            }
        }

        public static void Sort(DocumentResultForSort[] array, bool asc)
        {
            Sort(array, array.Length, asc);
        }

        public static void TopSort(DocumentResultForSort[] array, int top)
        {
            if (array.Length <= 0 || top <= 0)
            {
                return;
            }

            bool asc = array[0].Asc;

            if (top >= array.Length)
            {
                Sort(array, asc);
            }
            else
            {
                //init top heap
                int i;
                DocumentResultForSort temp;

                int mid = (top / 2) - 1;

                for (i = mid; i >= 0; i--)
                {
                    HeapAdjust(array, i, top - 1, asc);
                }

                for (i = top; i < array.Length; i++)
                {
                    if (asc)
                    {
                        if (array[0].SortValue <= array[i].SortValue)
                        {
                            continue;
                        }
                        else
                        {
                            array[0] = array[top - 1];
                            array[top - 1] = array[i];

                            if (array[top - 1].SortValue <= array[0].SortValue)
                            {
                                HeapAdjust(array, 0, top - 1, asc);
                            }
                            else
                            {
                                mid = (top / 2) - 1;

                                for (int j = mid; j >= 0; j--)
                                {
                                    HeapAdjust(array, j, top - 1, asc);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (array[0].SortValue >= array[i].SortValue)
                        {
                            continue;
                        }
                        else
                        {
                            array[0] = array[top - 1];
                            array[top - 1] = array[i];

                            if (array[top - 1].SortValue >= array[0].SortValue)
                            {
                                HeapAdjust(array, 0, top - 1, asc);
                            }
                            else
                            {
                                mid = (top / 2) - 1;

                                for (int j = mid; j >= 0; j--)
                                {
                                    HeapAdjust(array, j, top - 1, asc);
                                }
                            }
                        }
                    }
                }

                for (i = top - 1; i >= 1; i--)
                {
                    temp = array[0];
                    array[0] = array[i];
                    array[i] = temp;
                    HeapAdjust(array, 0, i - 1, asc);
                }
            }
        }

    }
}
