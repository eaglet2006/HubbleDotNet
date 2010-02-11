using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Data.Old
{
    class PayloadProvider
    {
        struct DocPayload : IComparable<DocPayload>
        {
            public int DocId;
            public Payload Payload;
            public DocPayload(int docid, Payload payload)
            {
                DocId = docid;
                Payload = payload;
            }

            public DocPayload(int docid)
            {
                DocId = docid;
                Payload = new Payload();
            }

            #region IComparable<DocPayload> Members

            public int CompareTo(DocPayload other)
            {
                if (this.DocId > other.DocId)
                {
                    return 1;
                }
                else if (this.DocId < other.DocId)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }

            }

            #endregion
        }

        //private Dictionary<int, Payload> _DocPayload = null; //DocId to payload
        StepList<DocPayload> _DocPayload = null;

        object _LockObj = new object();

        static int BinarySearch(DocPayload[] array, int index, int length, DocPayload value)
        {
            int lo = index;
            int hi = index + length - 1;
            DocPayload[] objArray = array;

            while (lo <= hi)
            {
                // i might overflow if lo and hi are both large positive numbers.
                int i = lo + ((hi - lo) >> 1);

                int c;

                if (objArray[i].DocId > value.DocId)
                {
                    c = 1;
                }
                else if (objArray[i].DocId < value.DocId)
                {
                    c = -1;
                }
                else
                {
                    c = 0;
                }

                if (c == 0) return i;
                if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        private bool TryGetValue(int docId, out Payload payload)
        {
            //return _DocPayload.TryGetValue(docId, out payload);

            DocPayload[] array = _DocPayload.ToArray();

            int index = BinarySearch(array, 0, _DocPayload.Count, new DocPayload(docId));

            if (index < 0)
            {
                payload = new Payload();
                return false;
            }
            else
            {
                payload = array[index].Payload;
                return true;
            }

            //DocPayload docPayload;
            //bool result = _DocPayload.TryGetValue(new DocPayload(docId), out docPayload);

            //payload = docPayload.Payload;

            //return result;

        }

        public PayloadProvider()
        {
            _DocPayload = new StepList<DocPayload>(1024, 1024 * 256, 1024 * 256);
            //_DocPayload = new Dictionary<int, Payload>(); //DocId to payload
        }

        public void SetFileIndex(int docId, int fileIndex)
        {
            lock (_LockObj)
            {
                int index = _DocPayload.BinarySearch(new DocPayload(docId));
                if (index < 0)
                {
                    return;
                }
                else
                {
                    _DocPayload.ToArray()[index].Payload.FileIndex = fileIndex;
                }
            }
        }

        public bool TryGetWordCount(int docId, int tabIndex, ref int count)
        {
            lock (_LockObj)
            {
                Payload payload;

                if (TryGetValue(docId, out payload))
                {
                    count = payload.GetWordCount(tabIndex);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool TryGetFileIndex(int docId, out int fileIndex)
        {
            lock (_LockObj)
            {
                Payload payload;
                if (!TryGetValue(docId, out payload))
                {
                    fileIndex = -1;
                    return false;
                }
                else
                {
                    fileIndex = payload.FileIndex;
                    return true;
                }
            }
        }

        public bool TryGetDataAndFileIndex(int docId, out int fileIndex, out int[] data)
        {
            lock (_LockObj)
            {
                Payload payload;
                if (!TryGetValue(docId, out payload))
                {
                    data = null;
                    fileIndex = -1;
                    return false;
                }
                else
                {
                    fileIndex = payload.FileIndex;
                    data = payload.Data;
                    return true;
                }
            }
        }

        unsafe public bool TryGetData(int docId, out int* data)
        {
            lock (_LockObj)
            {
                Payload payload;
                if (!TryGetValue(docId, out payload))
                {
                    data = null;
                    return false;
                }
                else
                {
                    fixed (int* result = &payload.Data[0])
                    {
                        data = result;
                    }
                    //data = payload.Data;
                    return true;
                }
            }
        }


        public void Add(int docId, Payload payload)
        {
            lock (_LockObj)
            {
                _DocPayload.Add(new DocPayload(docId, payload));
            }
            //_DocPayload.Add(docId, payload);
        }

        public void Clear()
        {
            lock (_LockObj)
            {
                _DocPayload.Clear();
            }
        }
    }
}
