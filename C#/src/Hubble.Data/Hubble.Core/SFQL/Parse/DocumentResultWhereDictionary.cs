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
        const int DefaultSize = 32768;

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
            base.Add(docId, new DocumentResultPoint(_Cur));
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
