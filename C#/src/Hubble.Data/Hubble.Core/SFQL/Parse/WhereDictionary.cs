using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.Parse
{
    public class WhereDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public bool Not = false;

        public WhereDictionary<TKey, TValue> AndMerge(WhereDictionary<TKey, TValue> fst, WhereDictionary<TKey, TValue> sec)
        {
            if (fst == null)
            {
                return sec;
            }

            if (sec == null)
            {
                return fst;
            }

            if (fst.Count > sec.Count)
            {
                //Swap input dictionaries
                //Let fst count less then sec

                WhereDictionary<TKey, TValue> temp;

                temp = fst;
                fst = sec;
                sec = temp;
            }

            if (fst.Not && sec.Not)
            {
                foreach (TKey key in fst.Keys)
                {
                    if (!sec.ContainsKey(key))
                    {
                        sec.Add(key, fst[key]);
                    }
                }

                return sec;
            }
            else
            {
                WhereDictionary<TKey, TValue> yes;
                WhereDictionary<TKey, TValue> not;

                if (fst.Not)
                {
                    yes = sec;
                    not = fst;
                }
                else
                {
                    yes = fst;
                    not = sec;
                }

                foreach (TKey key in not.Keys)
                {
                    yes.Remove(key);
                }

                return yes;

            }
        }
    }
}
