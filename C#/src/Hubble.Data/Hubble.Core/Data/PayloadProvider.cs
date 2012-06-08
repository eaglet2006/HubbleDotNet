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
using Hubble.Framework.DataStructure;
using Hubble.Framework.IO;
using Hubble.Framework.Threading;
using Hubble.Core.Entity;

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
        int _PayloadSize = 0; //How may elements in one payload data. sizeof(Data). count of int
        //int _CurIndex = 0; //Current index of IndexBuf;
        Field _DocIdReplaceField = null;
        int _RankTab = -1;

        ReplaceFieldValueToDocId _ReplaceFieldValueToDocId = null;

        IntDictionary<UInt16> _DocIdToRank = new IntDictionary<UInt16>();

        internal Field DocIdReplaceField
        {
            get
            {
                return _DocIdReplaceField;
            }
        }

        Lock _Lock = new Lock();
        //object _LockObj = new object();

        internal void EnterShareLock()
        {
            _Lock.Enter(Lock.Mode.Share);
        }

        internal void LeaveShareLock()
        {
            _Lock.Leave(Lock.Mode.Share);
        }

        unsafe public bool TestTryGetValue(int docId, out int* pFileIndex)
        {
            if (InnerTryGetValue(docId, out pFileIndex))
            {
                pFileIndex++;
                return true;
            }
            else
            {
                pFileIndex = null;
                return false;
            }
        }

        unsafe private bool InnerTryGetValue(int docId, out int* pFileIndex)
        {
            //IntPtr p;
            //if (_DocIdToFileIndex.TryGetValue(docId, out p))
            //{
            //    pFileIndex = (int*)p;
            //    return true;
            //}
            //else
            //{
            //    pFileIndex = null;
            //    return false;
            //}

            pFileIndex = null;

            if (_Count == 0)
            {
                return false;
            }

            int scanTimes = 0;

            IndexElement indexElement = null;

            int index = (docId - IndexBuf[0].StartDocId) / BufSize;
            if (index >= _IndexCount)
            {
                index = _IndexCount - 1;
            }

            while (scanTimes < _IndexCount)
            {
                if (docId >= IndexBuf[index].StartDocId)
                {
                    if (index == _IndexCount - 1)
                    {
                        //last index element
                        indexElement = IndexBuf[index];
                        break;
                    }
                    else if (docId < IndexBuf[index + 1].StartDocId)
                    {
                        indexElement = IndexBuf[index];
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
                else
                {
                    index--;

                    if (index < 0)
                    {
                        //less than first docid
                        index = 0;
                        indexElement = null;
                        break;
                    }

                    if (docId >= IndexBuf[index].StartDocId)
                    {
                        indexElement = IndexBuf[index];
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
            if (_Lock.Enter(Lock.Mode.Mutex))
            {
                try
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
                finally
                {
                    _Lock.Leave(Lock.Mode.Mutex);
                }
            }
        }

        public int GetDocIdByDocIdReplaceFieldValue(long value)
        {
            if (_Lock.Enter(Lock.Mode.Share))
            {
                try
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
                finally
                {
                    _Lock.Leave(Lock.Mode.Share);
                }
            }
            else
            {
                return int.MinValue;
            }
        }

        internal bool InnerTryGetRank(int docid, out UInt16 rank)
        {
            return _DocIdToRank.TryGetValue(docid, out rank);
        }

        /// <summary>
        /// Fill the array of DocidPayloadRank
        /// This method is for calculate rank
        /// </summary>
        /// <param name="count">fill count. the array maybe does not filled</param>
        /// <param name="docidPayloads"></param>
        internal unsafe void FillRank(int rankTabIndex, int count, OriginalDocumentPositionList[] docidPayloads)
        {
            if (_Lock.Enter(Lock.Mode.Share))
            {
                try
                {
                    if (count > docidPayloads.Length)
                    {
                        count = docidPayloads.Length;
                    }

                    //int rank;
                    //int* pFileIndex;
                    UInt16 urank;

                    for (int i = 0; i < count; i++)
                    {
                        if (_DocIdToRank.TryGetValue(docidPayloads[i].DocumentId, out urank))
                        {
                            docidPayloads[i].CountAndWordCount *= urank;
                        }

                        //if (InnerTryGetValue(docidPayloads[i].DocumentId, out pFileIndex))
                        //{
                        //    rank = *(pFileIndex + 1 + rankTabIndex);
                        //    docidPayloads[i].CountAndWordCount *= rank;
                        //}
                    }
                }
                finally
                {
                    _Lock.Leave(Lock.Mode.Share);
                }
            }
        }

        unsafe public void SetFileIndex(int docId, int fileIndex)
        {
            if (_Lock.Enter(Lock.Mode.Mutex))
            {
                try
                {
                    int* pFileIndex;
                    if (InnerTryGetValue(docId, out pFileIndex))
                    {
                        *pFileIndex = fileIndex;
                    }
                }
                finally
                {
                    _Lock.Leave(Lock.Mode.Mutex);
                }
            }
        }

        unsafe public bool TryGetWordCount(int docId, int tabIndex, ref int count)
        {
            if (_Lock.Enter(Lock.Mode.Share))
            {
                try
                {
                    int* pFileIndex;

                    if (InnerTryGetValue(docId, out pFileIndex))
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
                finally
                {
                    _Lock.Leave(Lock.Mode.Share);
                }
            }
            else
            {
                return false;
            }
        }

        unsafe public bool TryGetFileIndex(int docId, out int fileIndex)
        {
            if (_Lock.Enter(Lock.Mode.Share))
            {
                try
                {
                    int* pFileIndex;
                    if (!InnerTryGetValue(docId, out pFileIndex))
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
                finally
                {
                    _Lock.Leave(Lock.Mode.Share);
                }
            }
            else
            {
                fileIndex = 0;
                return false;
            }
        }

        unsafe public bool TryGetDataAndFileIndex(int docId, out int fileIndex, out int* data, out int payloadLength)
        {
            if (_Lock.Enter(Lock.Mode.Share))
            {
                try
                {
                    payloadLength = _PayloadSize;

                    int* pFileIndex;

                    if (!InnerTryGetValue(docId, out pFileIndex))
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
                finally
                {
                    _Lock.Leave(Lock.Mode.Share);
                }
            }
            else
            {
                payloadLength = 0;
                fileIndex = 0;
                data = null;
                return false;
            }
        }

        unsafe public bool TryGetData(int docId, out int* data)
        {
            if (_Lock.Enter(Lock.Mode.Share))
            {
                try
                {
                    int* pFileIndex;
                    if (!InnerTryGetValue(docId, out pFileIndex))
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
                finally
                {
                    _Lock.Leave(Lock.Mode.Share);
                }

            }
            else
            {
                data = null;
                return false;
            }
        }

        /// <summary>
        /// this function is called in first time loading.
        /// </summary>
        /// <param name="fs">file stream that position is already set to end of the header</param>
        /// <param name="storeLength">record length that stored in file</param>
        unsafe internal int LoadFromFile(System.IO.FileStream fs, int storeLength, int rankTab)
        {
            Clear();

            _RankTab = rankTab;

            _Count =(int)((fs.Length - fs.Position) / storeLength);

            byte[] block = new byte[storeLength * BufSize];
            
            int realPayloadSize = storeLength + 4; //add fileindex 4 bytes

            _PayloadSize = storeLength / 4 - 1; //subtract docid length (4 bytes); _PayloadSize is count of int

            int readCount = block.Length;
            Hubble.Framework.IO.Stream.ReadToBuf(fs, block, 0, ref readCount);
            int firstFileIndex = 0;
            int recordsInBlock = readCount / storeLength;
            int docId = -1;
            int lastDocId = -1;

            while (readCount > 0)
            {
                int fstDocid = BitConverter.ToInt32(block, 0);
                IndexElement indexElement = new IndexElement(fstDocid, realPayloadSize);
                IndexBuf[_IndexCount++] = indexElement;
                indexElement.MB.UsedCount = recordsInBlock;

                int* head = (int*)(IntPtr)indexElement.MB;
                
                int payloadIntLength = storeLength / 4 - 1; //how may int of payload data

                fixed (byte* pHead = &block[0])
                {
                    byte* pb = pHead;

                    for (int i = 0; i < recordsInBlock; i++)
                    {
                        int* pInt = (int*)pb;

                        *head = *pInt; //docid
                        docId = *head;

                        if (docId < lastDocId)
                        {
                            throw new Data.DataException(string.Format("docid = {0} < last docid ={1}", docId, lastDocId));
                        }

                        lastDocId = docId;

                        head++; //fileindex address
                        pInt++;

                        if (_DocIdReplaceField != null)
                        {
                            if (_DocIdReplaceField.DataType == DataType.BigInt)
                            {
                                long value = (((long)pInt[_DocIdReplaceField.TabIndex]) << 32) +
                                    (uint)pInt[_DocIdReplaceField.TabIndex + 1];

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
                                int value = pInt[_DocIdReplaceField.TabIndex];

                                _ReplaceFieldValueToDocId.AddOrUpdate(value, docId);

                                //if (!_ReplaceFieldValueToDocId.ContainsKey(value))
                                //{
                                //    _ReplaceFieldValueToDocId.Add(value, docId);
                                //}
                                //else
                                //{
                                //    _ReplaceFieldValueToDocId[value] = docId;
                                //}

                            }
                        }

                        *head = firstFileIndex + i; 

                        head++; //Payload Data first byte address

                        if (_RankTab > 0)
                        {
                            int rank = *(pInt + _RankTab);
                            if (rank < 0)
                            {
                                rank = 0;
                            }
                            else if (rank > 65535)
                            {
                                rank = 65535;
                            }

                            _DocIdToRank.Add(docId, (UInt16)rank);
                        }

                        for (int j = 0; j < payloadIntLength; j++)
                        {
                            *head = *pInt;
                            head++;
                            pInt++;
                        }

                        pb += storeLength;


                    }
                }


                firstFileIndex += recordsInBlock;
                readCount = block.Length;
                Hubble.Framework.IO.Stream.ReadToBuf(fs, block, 0, ref readCount);
                recordsInBlock = readCount / storeLength;
            }

            return docId;
        }

        public void UpdateRankIndex(int docId, int tabIndex, int value)
        {
            if (_RankTab < 0)
            {
                return;
            }

            if (tabIndex != _RankTab)
            {
                return;
            }

            if (_Lock.Enter(Lock.Mode.Mutex))
            {
                try
                {

                    int rank = value;
                    if (rank < 0)
                    {
                        rank = 0;
                    }
                    else if (rank > 65535)
                    {
                        rank = 65535;
                    }

                    _DocIdToRank.AddOrUpdate(docId, (UInt16)rank);
                }
                finally
                {
                    _Lock.Leave(Lock.Mode.Mutex);
                }
            }
        }

        unsafe public void Add(int docId, Payload payload)
        {
            if (_Lock.Enter(Lock.Mode.Mutex))
            {

                try
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

                    int realPayloadSize = (_PayloadSize + 1 + 1) * sizeof(int); //DocId + FileIndex + Payload

                    IndexElement ie;

                    if ((_Count % BufSize) == 0)
                    {
                        IndexBuf[_IndexCount++] = new IndexElement(docId, realPayloadSize);
                    }

                    ie = IndexBuf[_IndexCount - 1];
                    ie.MB.UsedCount++;
                    int* head = (int*)(IntPtr)ie.MB;

                    int* documentId = (int*)((byte*)head + (ie.MB.UsedCount - 1) * realPayloadSize);
                    int* fileIndex = documentId + 1;
                    int* payloadData = fileIndex + 1;

                    *documentId = docId;
                    *fileIndex = payload.FileIndex;

                    for (int i = 0; i < payload.Data.Length; i++)
                    {
                        payloadData[i] = payload.Data[i];
                    }

                    if (_RankTab > 0)
                    {
                        int rank = *(payloadData + _RankTab);
                        if (rank < 0)
                        {
                            rank = 0;
                        }
                        else if (rank > 65535)
                        {
                            rank = 65535;
                        }

                        _DocIdToRank.Add(docId, (UInt16)rank);
                    }

                    _Count++;

                }
                finally
                {
                    _Lock.Leave(Lock.Mode.Mutex);
                }
            }
        }

        public void Clear()
        {
            if (_Lock.Enter(Lock.Mode.Mutex))
            {

                try
                {
                    for (int i = 0; i < _IndexCount; i++)
                    {
                        IndexBuf[i].MB.Dispose();
                    }

                    IndexBuf = new IndexElement[(512 * 1024 * 1024) / BufSize];
                    _DocIdToRank.Clear();
                    _Count = 0;
                    _IndexCount = 0; //Count of IndexBuf
                    _PayloadSize = 0; //How may elements in one payload data. sizeof(Data).
                    //_CurIndex = 0; //Current index of IndexBuf;
                }
                finally
                {
                    _Lock.Leave(Lock.Mode.Mutex);
                }
            }
        }
    }
}
