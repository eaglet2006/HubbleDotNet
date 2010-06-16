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

namespace Hubble.Core.SFQL.Parse
{
    public unsafe struct DocumentResultPoint
    {
        public Query.DocumentResult* pDocumentResult;

        public DocumentResultPoint(Query.DocumentResult* pDocResult)
        {
            pDocumentResult = pDocResult;
        }
    }

    unsafe public class DocumentResultWhereDictionary : WhereDictionary<int, DocumentResultPoint>, IDisposable
    {
        public const int DefaultSize = 32768;

        List<IntPtr> _MemList;
        int _UnitSize;
        int _UnitIndex;
        Query.DocumentResult* _Cur;


        public bool Not = false;

        public bool ZeroResult = false;

        private int _RelTotalCount = 0;

        public int RelTotalCount
        {
            get
            {
                if (_RelTotalCount > this.Count)
                {
                    return _RelTotalCount;
                }
                else
                {
                    return this.Count;
                }
            }

            set
            {
                _RelTotalCount = value;
            }
        }


        unsafe public DocumentResultWhereDictionary()
            :this(DefaultSize)
        {
        }

        unsafe public DocumentResultWhereDictionary(int capacity)
            : base(capacity)
        {
            _MemList = new List<IntPtr>();

            _MemList.Add(Marshal.AllocHGlobal(capacity * sizeof(Query.DocumentResult)));

            _UnitSize = capacity;
            _UnitIndex = 0;
            _Cur = (Query.DocumentResult*)_MemList[_MemList.Count-1];
        }

        ~DocumentResultWhereDictionary()
        {
            try
            {
                Dispose();
            }
            catch
            {
            }
        }

        unsafe public void Add(int docId, Query.DocumentResult value)
        {
            try
            {
                base.Add(docId, new DocumentResultPoint(_Cur));
            }
            catch(Exception e)
            {
                throw new ParseException(string.Format("Docid={0} err:{1}",
                    docId, e.Message));
            }

            *_Cur = value;

            _UnitIndex++;

            if (_UnitIndex >= _UnitSize)
            {
                _MemList.Add(Marshal.AllocHGlobal(_UnitSize * sizeof(Query.DocumentResult)));
                _UnitIndex = 0;
                _Cur = (Query.DocumentResult*)_MemList[_MemList.Count - 1];
            }
            else
            {
                _Cur++;
            }
        }

        unsafe public void Update(int docId, long score)
        {
            base[docId].pDocumentResult->Score = score;
        }

        unsafe public void Add(int docId, long rank)
        {
            base.Add(docId, new DocumentResultPoint(_Cur));
            _Cur->DocId = docId;
            _Cur->Score = rank;

            _UnitIndex++;

            if (_UnitIndex >= _UnitSize)
            {
                _MemList.Add(Marshal.AllocHGlobal(_UnitSize * sizeof(Query.DocumentResult)));
                _UnitIndex = 0;
                _Cur = (Query.DocumentResult*)_MemList[_MemList.Count - 1];
            }
            else
            {
                _Cur++;
            }
        }

        unsafe new public Query.DocumentResult this[int docid]
        {
            get
            {
                DocumentResultPoint drp = base[docid];
                return *drp.pDocumentResult;
            }

            set
            {
                DocumentResultPoint drp = base[docid];
                *drp.pDocumentResult = value;

            }
        }

        public bool TryGetValue(int docId, out Query.DocumentResult* value)
        {
            DocumentResultPoint drp;
            if (TryGetValue(docId, out drp))
            {
                value = drp.pDocumentResult;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }


        public bool TryGetValue(int docId, out Query.DocumentResult value)
        {
            DocumentResultPoint drp;
            if (TryGetValue(docId, out drp))
            {
                value = *drp.pDocumentResult;
                return true;
            }
            else
            {
                value = new Hubble.Core.Query.DocumentResult();
                return false;
            }
        }

        unsafe public DocumentResultWhereDictionary OrMerge(DocumentResultWhereDictionary fst, DocumentResultWhereDictionary sec)
        {
            if (fst == null)
            {
                return sec;
            }

            if (sec == null)
            {
                return fst;
            }

            foreach (int docid in sec.Keys)
            {
                if (fst.ContainsKey(docid))
                {
                    fst.Update(docid, fst[docid].Score + sec[docid].Score);
                }
                else
                {
                    fst.Add(docid, sec[docid]);
                }
            }

            return fst;
        }

        public DocumentResultWhereDictionary AndMerge(DocumentResultWhereDictionary fst, DocumentResultWhereDictionary sec)
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

                DocumentResultWhereDictionary temp;

                temp = fst;
                fst = sec;
                sec = temp;
            }

            if (fst.Not && sec.Not)
            {
                foreach (int key in fst.Keys)
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
                DocumentResultWhereDictionary yes;
                DocumentResultWhereDictionary not;

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

                foreach (int key in not.Keys)
                {
                    yes.Remove(key);
                }

                return yes;

            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (_MemList != null)
            {
                foreach (IntPtr p in _MemList)
                {
                    Marshal.FreeHGlobal(p);
                }

                _MemList = null;
            }
        }

        #endregion
    }
}
