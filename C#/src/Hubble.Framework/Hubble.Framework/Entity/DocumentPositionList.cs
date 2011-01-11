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
using System.Diagnostics;
using Hubble.Framework.Serialization;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Entity
{
    public class MergeStream
    {
        public System.IO.Stream Stream;
        public int Count;
        public long Length;
        public List<DocumentPosition> SkipDocIndex = null;

        public MergeStream(System.IO.Stream stream, long length)
        {
            Stream = stream;
            Length = length;
        }
    }

    public struct DocumentPosition
    {
        public int DocId;

        /// <summary>
        /// Position is the relational position to the 
        /// first byte of the index.
        /// </summary>
        public int Position;

        public DocumentPosition(int docid, int position)
        {
            DocId = docid;
            Position = position;
        }
    }


    /// <summary>
    /// This class record all the position for one word in one document.
    /// 
    /// first bit of first byte is 1
    /// first bit of remain bytes is 0
    /// |11111111 01111111 00111111| 11111111|
    /// means two int
    /// first int is 0x5f7f7f
    /// second int is 0x7f
    /// Input int must not be less then zero!
    /// </summary>
    //[StructLayout(LayoutKind.Sequential,Pack=4)]
    public struct DocumentPositionList
    {
        readonly static int[] TotalWordsInDocRank = { 64, 192, 576, 1728, 5184, 15562, 46656, 139968 };

        //[FieldOffset(0)]
        public int DocumentId ;

        //[FieldOffset(8)]
        /// <summary>
        /// High 2 bytes is Count of items in one document.
        /// Other 2 bytes is Word count in one document
        /// </summary>
        //public int CountAndWordCount;

        /// <summary>
        /// Count of items of this word in one document.
        /// </summary>
        public Int16 Count;

        /// <summary>
        /// Total words count in this document
        /// </summary>
        private Int16 _TotalWordsInThisDocumentIndex;

        public int TotalWordsInThisDocument
        {
            get
            {
                return TotalWordsInDocRank[_TotalWordsInThisDocumentIndex];
            }
        }

        public Int16 TotalWordsInThisDocumentIndex
        {
            get
            {
                return _TotalWordsInThisDocumentIndex;
            }
        }


        public void SetTotalWordsInThisDocumentIndex(Int16 value)
        {
            _TotalWordsInThisDocumentIndex = value;
        }

        //[FieldOffset(12)]
        /// <summary>
        /// First word position
        /// </summary>
        public int FirstPosition;

        /// <summary>
        /// Point to next item
        /// Used for index not for query
        /// </summary>
        public int Next;

        public DocumentPositionList(int docId)
        {
            DocumentId = docId;
            //CountAndWordCount = 0;
            Count = 0;
            _TotalWordsInThisDocumentIndex = 0;
            FirstPosition = 0;
            Next = -1;
        }

        public DocumentPositionList(int docId, int count, Int16 totalWordsInDocIndex, int firstPosition)
        {
            DocumentId = docId;
            //CountAndWordCount = 0;

            if (count > Int16.MaxValue)
            {
                Count = Int16.MaxValue;
            }
            else if (count <= 0)
            {
                Count = 1;
            }
            else
            {
                Count = (Int16)count;
            }

            _TotalWordsInThisDocumentIndex = totalWordsInDocIndex;

            if (_TotalWordsInThisDocumentIndex >= 7)
            {
                _TotalWordsInThisDocumentIndex = 7;
            }

            FirstPosition = firstPosition;

            Next = -1;

        }

        public DocumentPositionList(int docId, int count, Int16 totalWordsInDocIndex)
        {
            DocumentId = docId;
            //CountAndWordCount = 0;
            
            if (count > Int16.MaxValue)
            {
                Count = Int16.MaxValue;
            }
            else if (count <= 0)
            {
                Count = 1;
            }
            else
            {
                Count = (Int16)count;
            }

            _TotalWordsInThisDocumentIndex = totalWordsInDocIndex;

            if (_TotalWordsInThisDocumentIndex >= 7)
            {
                _TotalWordsInThisDocumentIndex = 7;
            }

            FirstPosition = 0;

            Next = -1;

        }


        #region Static methods

        static public Int16 GetTotalWordsInDocIndex(int count)
        {
            for (Int16 i = 0; i < TotalWordsInDocRank.Length; i++)
            {
                if (TotalWordsInDocRank[i] > count)
                {
                    return i;
                }
            }

            return (Int16)(TotalWordsInDocRank.Length - 1);
        }


        static public void Serialize(DocumentPositionList first, int docsCount, IEnumerable<DocumentPositionList> docPositions, System.IO.Stream stream, bool simple)
        {
            //int docsCount = docPositions.Count;
           
            //Write documets count
            VInt.sWriteToStream(docsCount, stream);

            //DocumentPositionList first = docPositions.GetEnumerator();

            //Write first document id
            int lstDocId = first.DocumentId;
            VInt.sWriteToStream(lstDocId, stream);

            int count = first.Count;

            if (count >= 32768)
            {
                count = 32767;
            }

            count *= 8; //Shift 3 bit
            count += first._TotalWordsInThisDocumentIndex;

            VInt.sWriteToStream(count, stream);
            if (!simple)
            {
                VInt.sWriteToStream(first.FirstPosition, stream);
            }

            int i = 0;

            foreach(DocumentPositionList docPosition in docPositions)
            {
                i++;

                if (i == 1)
                {
                    continue;
                }

                VInt.sWriteToStream(docPosition.DocumentId - lstDocId, stream);

                count = docPosition.Count;

                if (count >= 32768)
                {
                    count = 32767;
                }

                count *= 8; //Shift 3 bit
                count += docPosition._TotalWordsInThisDocumentIndex;

                VInt.sWriteToStream(count, stream);

                if (!simple)
                {
                    VInt.sWriteToStream(docPosition.FirstPosition, stream);
                }

                lstDocId = docPosition.DocumentId;
            }

            byte[] lstDocIdBuf = BitConverter.GetBytes(lstDocId);
            stream.Write(lstDocIdBuf, 0, lstDocIdBuf.Length);
        }

        static public int GetDocumentsCount(System.IO.Stream stream)
        {
            return VInt.sReadFromStream(stream);
        }

        static public DocumentPositionList[] Deserialize(System.IO.Stream stream, bool simple, out long wordCountSum)
        {
            int count = int.MaxValue;
            return Deserialize(stream, ref count, simple, out wordCountSum);
        }

        static public DocumentPositionList GetNextDocumentPositionList(ref int docId, System.IO.Stream stream, bool simple)
        {
            if (docId < 0)
            {
                docId = VInt.sReadFromStream(stream);

                int count = VInt.sReadFromStream(stream);

                if (!simple)
                {
                    int firstPosition = VInt.sReadFromStream(stream);
                    return new DocumentPositionList(docId, count / 8, (Int16)(count % 8), firstPosition);
                }
                else
                {
                    return new DocumentPositionList(docId, count / 8, (Int16)(count % 8));
                }
            }
            else
            {
                docId = VInt.sReadFromStream(stream) + docId;
                int count = VInt.sReadFromStream(stream);
                int docCount = (Int16)(count / 8);

                if (docCount >= 32768)
                {
                    docCount = 32767;
                }

                if (!simple)
                {
                    int firstPosition = VInt.sReadFromStream(stream);
                    return new DocumentPositionList(docId, docCount, (Int16)(count % 8), firstPosition);
                }
                else
                {
                    return new DocumentPositionList(docId, docCount, (Int16)(count % 8));
                }
            }
        }


        static public DocumentPositionList[] Deserialize(System.IO.Stream stream, ref int documentsCount, bool simple, out long wordCountSum)
        {
            wordCountSum = 0;

            int docsCount = VInt.sReadFromStream(stream);

            if (docsCount == 0)
            {
                //This index has skip doc index
                DeserializeSkipDocIndex(stream, true);

                docsCount = VInt.sReadFromStream(stream);
            }

            int relDocCount = docsCount;

            int lastDocId = VInt.sReadFromStream(stream);

            int count = VInt.sReadFromStream(stream);

            docsCount = Math.Min(docsCount, documentsCount);

            DocumentPositionList[] result = new DocumentPositionList[docsCount];

            if (docsCount <= 0)
            {
                documentsCount = relDocCount;
                return result;
            }

            if (!simple)
            {
                int firstPosition = VInt.sReadFromStream(stream);
                result[0] = new DocumentPositionList(lastDocId, count / 8, (Int16)(count % 8), firstPosition);
            }
            else
            {
                result[0] = new DocumentPositionList(lastDocId, count / 8, (Int16)(count % 8));
            }

            if (docsCount == 1)
            {
                wordCountSum = 1;
            }

            for (int i = 1; i < docsCount; i++)
            {
                lastDocId = VInt.sReadFromStream(stream) + lastDocId;
                count = VInt.sReadFromStream(stream);
                int docCount = (Int16)(count / 8);

                if (docCount >= 32768)
                {
                    docCount = 32767;
                }

                if (!simple)
                {
                    int firstPosition = VInt.sReadFromStream(stream);
                    result[i] = new DocumentPositionList(lastDocId, docCount, (Int16)(count % 8), firstPosition);
                }
                else
                {
                    result[i] = new DocumentPositionList(lastDocId, docCount, (Int16)(count % 8));
                }

                wordCountSum += docCount;
            }

            documentsCount = relDocCount;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">stream of the index</param>
        /// <param name="skipBeginByte">if it is true, skip the first byte 0, sometime this byte has been read.</param>
        /// <returns></returns>
        static public List<DocumentPosition> DeserializeSkipDocIndex(System.IO.Stream stream, bool skipBeginByte)
        {
            int count;

            return DeserializeSkipDocIndex(stream, skipBeginByte, out count);
        }

        /// <summary>
        /// Get skip document index.
        /// This index mark the document id and position skipping every 1000 records or more 
        /// inside the index buffer.
        /// </summary>
        /// <param name="stream">stream of the index</param>
        /// <param name="count">if has stepDocIndx, return count of stepDocIndex, else return count of the document records of this index</param>
        /// <param name="skipBeginByte">if it is true, skip the first byte 0, sometime this byte has been read.</param>
        /// <returns></returns>
        static public List<DocumentPosition> DeserializeSkipDocIndex(System.IO.Stream stream, bool skipBeginByte, out int count)
        {
            if (!skipBeginByte)
            {
                count = VInt.sReadFromStream(stream); //First item is the count of the list;

                if (count > 0)
                {
                    //if hasn't stepDocIndex, count > 0
                    return null;
                }
            }

            List<DocumentPosition> result = new List<DocumentPosition>();

            count = VInt.sReadFromStream(stream); //First item is the count of the list;

            int docId = 0;
            int position = 0; // Position is the relational position to the first byte of the index.

            for (int i = 0; i < count; i++)
            {
                docId += VInt.sReadFromStream(stream);
                position += VInt.sReadFromStream(stream);

                result.Add(new DocumentPosition(docId, position));
            }

            return result;
        }

        static private void SerializeSkipDocIndex(System.IO.Stream stream, List<DocumentPosition> skipDocIndex)
        {
            if (skipDocIndex.Count <= 0)
            {
                return;
            }

            VInt.sWriteToStream(0, stream); //Begin flag
            VInt.sWriteToStream(skipDocIndex.Count, stream); //Write the count of skip doc index's items

            int docId = 0;
            int position = 0;

            for (int i = 0; i < skipDocIndex.Count; i++)
            {
                VInt.sWriteToStream(skipDocIndex[i].DocId - docId, stream);

                docId = skipDocIndex[i].DocId;

                VInt.sWriteToStream(skipDocIndex[i].Position - position, stream);

                position = skipDocIndex[i].Position;
            }
        }

        static public void Merge(IList<MergeStream> srcList, System.IO.Stream destStream)
        {
            List<long> srcEndPositionList = new List<long>();

            for (int i = 0; i < srcList.Count; i++)
            {
                srcEndPositionList.Add(srcList[i].Stream.Position + srcList[i].Length);
            }

            int docsCount = 0;

            List<DocumentPosition> skipDocIndex = new List<DocumentPosition>();

            foreach (MergeStream ms in srcList)
            {
                int count = VInt.sReadFromStream(ms.Stream);
                ms.Count = count;

                if (count == 0)
                {
                    //This index has skip doc index
                    ms.SkipDocIndex = (DeserializeSkipDocIndex(ms.Stream, true));

                    count = VInt.sReadFromStream(ms.Stream);
                }

                docsCount += count;
            }

            int lastDocId = -1;

            System.IO.Stream originalDestStream = destStream;

            destStream = new System.IO.MemoryStream(8192);

            int remainCount = 0; //remain count does not build in skip doc index

            for (int i = 0; i < srcList.Count; i++)
            {
                System.IO.Stream src = srcList[i].Stream;

                long srcFirstDocIdPosition = src.Position;

                int firstDocId = VInt.sReadFromStream(src);

                long srcFirstDocIdLength = src.Position - srcFirstDocIdPosition;

                long destFirstDocIdPosition = destStream.Position;

                if (lastDocId < 0)
                {
                    VInt.sWriteToStream(firstDocId, destStream);
                }
                else
                {
                    VInt.sWriteToStream(firstDocId - lastDocId, destStream);
                }

                long destFirstDocIdLength = destStream.Position - destFirstDocIdPosition;

                int delta = (int)(destFirstDocIdLength - srcFirstDocIdLength);

                //build skip doc index
                if (srcList[i].SkipDocIndex != null)
                {
                    //Merge skip doc index
                    if (i > 0)
                    {
                        skipDocIndex.Add(new DocumentPosition(lastDocId, (int)destFirstDocIdPosition));
                    }

                    foreach (DocumentPosition dp in srcList[i].SkipDocIndex)
                    {
                        skipDocIndex.Add(new DocumentPosition(dp.DocId,
                            (int)(destFirstDocIdPosition + dp.Position + delta)));
                    }
                }
                else
                {
                    if (remainCount > 1024)
                    {
                        skipDocIndex.Add(new DocumentPosition(lastDocId, (int)destFirstDocIdPosition));
                        remainCount = 0;
                    }
                    else
                    {
                        remainCount += srcList[i].Count;
                    }
                }


                byte[] buf = new byte[8192];
                int remain = (int)(srcEndPositionList[i] - sizeof(int) - src.Position);

                int len = src.Read(buf, 0, Math.Min(buf.Length, remain));

                while (len > 0)
                {
                    destStream.Write(buf, 0, len);

                    remain -= len;

                    len = src.Read(buf, 0, Math.Min(buf.Length, remain));
                }

                //Get last docid of src
                byte[] lastDocIdBuf = new byte[sizeof(int)];

                src.Read(lastDocIdBuf, 0, lastDocIdBuf.Length);

                lastDocId = BitConverter.ToInt32(lastDocIdBuf, 0);
            }

            //Write last doc id
            destStream.Write(BitConverter.GetBytes(lastDocId), 0, sizeof(int));

            //Write skip doc index
            if (skipDocIndex.Count > 0)
            {
                SerializeSkipDocIndex(originalDestStream, skipDocIndex);
            }

            //Write docs count
            VInt.sWriteToStream(docsCount, originalDestStream);


            //Write memory buffer to original dest stream
            destStream.Position = 0;

            byte[] buffer = new byte[8192];
            int c = 0;

            do
            {
                c = destStream.Read(buffer, 0, buffer.Length);

                if (c > 0)
                {
                    originalDestStream.Write(buffer, 0, c);
                }

            } while (c > 0);
        }

        static public void Merge(System.IO.Stream src1, long src1Length, System.IO.Stream src2, long src2Length, System.IO.Stream destStream)
        {
            long src1EndPosition = src1.Position + src1Length;
            long src2EndPosition = src2.Position + src2Length;

            int src1DocsCount = VInt.sReadFromStream(src1);
            int src2DocsCount = VInt.sReadFromStream(src2);

            //Write docs count
            VInt.sWriteToStream(src1DocsCount + src2DocsCount, destStream);

            //Merge src1
            byte[] buf = new byte[8192];
            int remain = (int)(src1EndPosition - sizeof(int) - src1.Position);

            int len = src1.Read(buf, 0, Math.Min(buf.Length, remain));

            while (len > 0)
            {
                destStream.Write(buf, 0, len);

                remain -= len;
                len = src1.Read(buf, 0, Math.Min(buf.Length, remain));
            }

            //Get last docid of src1
            byte[] lastDocIdBuf = new byte[sizeof(int)];

            src1.Read(lastDocIdBuf, 0, lastDocIdBuf.Length);

            int lastDocid = BitConverter.ToInt32(lastDocIdBuf, 0);

            //Get first docid of src2
            int src2FirstDocId = VInt.sReadFromStream(src2);
            
            //Write gap between above
            VInt.sWriteToStream(src2FirstDocId - lastDocid, destStream);

            //Merge src2
            remain = (int)(src2EndPosition - src2.Position);

            len = src2.Read(buf, 0, Math.Min(buf.Length, remain));

            while (len > 0)
            {
                destStream.Write(buf, 0, len);

                remain -= len;
                len = src2.Read(buf, 0, Math.Min(buf.Length, remain));
            }
        }

        #endregion

    }
}
