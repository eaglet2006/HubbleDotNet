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

namespace Hubble.Core.SFQL.Parse
{
    public class WhereDictionary<TKey, TValue> : Dictionary<int, TValue>
    //public class WhereDictionary<TKey, TValue> : Hubble.Framework.DataStructure.SortDictionary<TValue>
    {
        public WhereDictionary()
        {
        }

        public WhereDictionary(int capacity)
            : base(capacity)
        {
        }

        //public WhereDictionary<int, TValue> AndMerge(WhereDictionary<int, TValue> fst, WhereDictionary<int, TValue> sec)
        //{
        //    if (fst == null)
        //    {
        //        return sec;
        //    }

        //    if (sec == null)
        //    {
        //        return fst;
        //    }

        //    if (fst.Count > sec.Count)
        //    {
        //        //Swap input dictionaries
        //        //Let fst count less then sec

        //        WhereDictionary<int, TValue> temp;

        //        temp = fst;
        //        fst = sec;
        //        sec = temp;
        //    }

        //    if (fst.Not && sec.Not)
        //    {
        //        foreach (int key in fst.Keys)
        //        {
        //            if (!sec.ContainsKey(key))
        //            {
        //                sec.Add(key, fst[key]);
        //            }
        //        }

        //        return sec;
        //    }
        //    else
        //    {
        //        WhereDictionary<int, TValue> yes;
        //        WhereDictionary<int, TValue> not;

        //        if (fst.Not)
        //        {
        //            yes = sec;
        //            not = fst;
        //        }
        //        else
        //        {
        //            yes = fst;
        //            not = sec;
        //        }

        //        foreach (int key in not.Keys)
        //        {
        //            yes.Remove(key);
        //        }

        //        return yes;

        //    }
        //}
    }
}
