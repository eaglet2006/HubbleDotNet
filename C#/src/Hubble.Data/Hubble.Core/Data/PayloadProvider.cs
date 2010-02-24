using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;
using Hubble.Framework.IO;

namespace Hubble.Core.Data
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
        const int BufSize = 128 * 1024;

        class IndexElement
        {
            public int StartDocId;
            public PayloadMemoryBlock MB;

            public IndexElement(int startDocId, int realPayloadSize)
            {
                StartDocId = startDocId;
                MB = new PayloadMemoryBlock(BufSize * realPayloadSize, realPayloadSize);
            }
        }

        IndexElement[] IndexBuf = new IndexElement[(512 * 1024 * 1024) / BufSize]; //Max capitity of one table is 512M records

        int _Count = 0;
        int _IndexCount = 0; //Count of IndexBuf
        int _PayloadSize = 0; //How may elements in one payload data. sizeof(Data).
        int _CurIndex = 0; //Current index of IndexBuf;
        Field _DocIdReplaceField = null;

        ReplaceFieldValueToDocId _ReplaceFieldValueToDocId = null;

        internal Field DocIdReplaceField
        {
            get
            {
                return _DocIdReplaceField;
            }
        }

        object _LockObj = new object();

        unsafe private bool TryGetValue(int docId, out int* pFileIndex)
        {
            pFileIndex = null;

            if (_Count == 0)
            {
                return false;
            }

            int scanTimes = 0;

            IndexElement indexElement = null;

            while (scanTimes < _IndexCount)
            {
                if (docId >= IndexBuf[_CurIndex].StartDocId)
                {
                    if (_CurIndex == _IndexCount - 1)
                    {
                        //last index element
                        indexElement = IndexBuf[_CurIndex];
                        break;
                    }
                    else if (docId < IndexBuf[_CurIndex + 1].StartDocId)
                    {
                        indexElement = IndexBuf[_CurIndex];
                        break;
                    }
                    else
                    {
                        _CurIndex++;
                    }
                }
                else
                {
                    _CurIndex--;

                    if (_CurIndex < 0)
                    {
                        //less then first docid
                        _CurIndex = 0;
                        indexElement = null;
                        break;
                    }
                    
                    if (docId >= IndexBuf[_CurIndex].StartDocId)
                    {
                        indexElement = IndexBuf[_CurIndex];
                        break;
                    }
                }

                scanTimes++;
            }

            if (indexElement == null)
            {
                return false;
            }
            else
            {
                int* docPtr = indexElement.MB.Find(docId);

                if (docPtr == null)
                {
                    return false;
                }

                docPtr++;

                pFileIndex = docPtr;

                return true;
            }
        }

        public PayloadProvider()
            :this(null)
        {

        }

        public PayloadProvider(Field docIdReplaceField)
        {
            if (docIdReplaceField != null)
            {
                _DocIdReplaceField = docIdReplaceField;

                _ReplaceFieldValueToDocId = new ReplaceFieldValueToDocId(docIdReplaceField.DataType == DataType.BigInt);
            }
        }

        internal void RemoveDocIdReplaceFieldValue(long value)
        {
            lock (_LockObj)
            {
                if (_DocIdReplaceField == null)
                {
                    return;
                }

                if (_DocIdReplaceField.DataType == DataType.BigInt)
                {
                    _ReplaceFieldValueToDocId.Remove(value);
                }
                else
                {
                    _ReplaceFieldValueToDocId.Remove((int)value);
                }

            }
        }

        public int GetDocIdByDocIdReplaceFieldValue(long value)
        {
            lock (_LockObj)
            {
                int docId = int.MinValue;

                if (_DocIdReplaceField == null)
                {
                    throw new DataException("Can't get docid for the table that has not DocId Replace Field attribute.");
                }

                if (_DocIdReplaceField.DataType == DataType.BigInt)
                {
                    if (_ReplaceFieldValueToDocId.TryGetValue(value, out docId))
                    {
                        return docId;
                    }
                    else
                    {
                        return int.MinValue;
                    }
                }
                else
                {
                    if (_ReplaceFieldValueToDocId.TryGetValue((int)value, out docId))
                    {
                        return docId;
                    }
                    else
                    {
                        return int.MinValue;
                    }
                }
            }
        }

        unsafe public void SetFileIndex(int docId, int fileIndex)
        {
            lock (_LockObj)
            {
                int* pFileIndex;
                if (TryGetValue(docId, out pFileIndex))
                {
                    *pFileIndex = fileIndex;
                }
            }
        }

        unsafe public bool TryGetWordCount(int docId, int tabIndex, ref int count)
        {
            lock (_LockObj)
            {
                int* pFileIndex;

                if (TryGetValue(docId, out pFileIndex))
                {
                    int* pData = pFileIndex + 1;
                    count = pData[tabIndex];
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        unsafe public bool TryGetFileIndex(int docId, out int fileIndex)
        {
            lock (_LockObj)
            {
                int* pFileIndex;
                if (!TryGetValue(docId, out pFileIndex))
                {
                    fileIndex = -1;
                    return false;
                }
                else
                {
                    fileIndex = *pFileIndex;
                    return true;
                }
            }
        }

        unsafe public bool TryGetDataAndFileIndex(int docId, out int fileIndex, out int* data, out int payloadLength)
        {
            lock (_LockObj)
            {
                payloadLength = _PayloadSize;

                int* pFileIndex;

                if (!TryGetValue(docId, out pFileIndex))
                {
                    data = null;
                    fileIndex = -1;
                    return false;
                }
                else
                {
                    fileIndex = *pFileIndex;
                    data = pFileIndex + 1;
                    return true;
                }
            }
        }

        unsafe public bool TryGetData(int docId, out int* data)
        {
            lock (_LockObj)
            {
                int* pFileIndex;
                if (!TryGetValue(docId, out pFileIndex))
                {
                    data = null;
                    return false;
                }
                else
                {
                    data = pFileIndex + 1;
                    return true;
                }
            }
        }

        unsafe public void Add(int docId, Payload payload)
        {
            lock (_LockObj)
            {
                if (payload.Data.Length == 0)
                {
                    return;
                }


                if (_DocIdReplaceField != null)
                {
                    if (_DocIdReplaceField.DataType == DataType.BigInt)
                    {
                        long value = (((long)payload.Data[_DocIdReplaceField.TabIndex]) << 32) +
                            (uint)payload.Data[_DocIdReplaceField.TabIndex + 1];

                        if (!_ReplaceFieldValueToDocId.ContainsKey(value))
                        {
                            _ReplaceFieldValueToDocId.Add(value, docId);
                        }
                        else
                        {
                            _ReplaceFieldValueToDocId[value] = docId;
                        }
                    }
                    else
                    {
                        int value = payload.Data[_DocIdReplaceField.TabIndex];

                        if (!_ReplaceFieldValueToDocId.ContainsKey(value))
                        {
                            _ReplaceFieldValueToDocId.Add(value, docId);
                        }
                        else
                        {
                            _ReplaceFieldValueToDocId[value] = docId;
                        }

                    }
                }

                if (_PayloadSize == 0)
                {
                    _PayloadSize = payload.Data.Length;
                }

                int realPayloadSize = (_PayloadSize + 1 + 1) * sizeof(int) ; //DocId + FileIndex + Payload

                IndexElement ie;

                if ((_Count % BufSize) == 0)
                {
                    IndexBuf[_IndexCount++] = new IndexElement(docId, realPayloadSize);
                }

                ie = IndexBuf[_IndexCount - 1];
                ie.MB.UsedCount++;
                int* head = (int*)(IntPtr)ie.MB;

                int* documentId = (int*)((byte*)head + (ie.MB.UsedCount-1) * realPayloadSize);
                int* fileIndex = documentId + 1;
                int* payloadData = fileIndex + 1;

                *documentId = docId;
                *fileIndex = payload.FileIndex;

                for (int i = 0; i < payload.Data.Length; i++)
                {
                    payloadData[i] = payload.Data[i];
                }

                _Count++;

            }
        }

        public void Clear()
        {
            lock (_LockObj)
            {
                for (int i = 0; i < _IndexCount; i++)
                {
                    IndexBuf[i].MB.Dispose();
                }

                IndexBuf = new IndexElement[(512 * 1024 * 1024) / BufSize];

                _Count = 0;
                _IndexCount = 0; //Count of IndexBuf
                _PayloadSize = 0; //How may elements in one payload data. sizeof(Data).
                _CurIndex = 0; //Current index of IndexBuf;
            }
        }
    }
}
