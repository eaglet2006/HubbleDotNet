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
using Hubble.Framework.IO;
using Hubble.Core.Entity;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Store
{
    class IndexReader : IDisposable
    {
        string _HeadFilePath;
        string _IndexFilePath;
        private int _Serial;
        bool _ReadHead;

        WordStepDocIndex _WordStepDocIndex;
        IndexFileProxy _IndexFileProxy;

        #region Read DocumentPositionList Dynamicly

        private int _CurrentFileIndex = -1; //Current file index. it is the index of StepDocIndexList and WordFilePositionList
        private int _CurrentDocId = -1;
        private int _CurrentStepDocIndex = -1;
        private BufferMemory _CurrentIndexBuf = null;
        private bool _End = false;
        private DocumentPositionList _NextDocPositionList = new DocumentPositionList(-1);

        #endregion

        //System.IO.FileStream _HeadFile;

        System.IO.Stream _IndexFile;
        //System.IO.FileStream _IndexFile;

        private Hubble.Core.Data.Field.IndexMode _IndexMode;

        private string HeadFilePath
        {
            get
            {
                return _HeadFilePath;
            }
        }

        private string IndexFilePath
        {
            get
            {
                return _IndexFilePath;
            }
        }

        public IndexReader(WordStepDocIndex wsdi, IndexFileProxy indexProxy)
        {
            _WordStepDocIndex = wsdi;
            _IndexFileProxy = indexProxy;
        }

        public IndexReader(int serial, string path, string fieldName, Hubble.Core.Data.Field.IndexMode indexMode)
            :this(serial, path, fieldName, indexMode, true)
        {
        }

        public IndexReader(int serial, string path, string fieldName, Hubble.Core.Data.Field.IndexMode indexMode, bool readHead)
        {
            _IndexMode = indexMode;

            _Serial = serial;

            _ReadHead = readHead;

            if (_ReadHead)
            {
                _HeadFilePath = Path.AppendDivision(path, '\\') +
                    string.Format("{0:D7}{1}.hdx", serial, fieldName);
            }

            _IndexFilePath = Path.AppendDivision(path, '\\') +
                string.Format("{0:D7}{1}.idx", serial, fieldName);

            if (_ReadHead)
            {
                //_HeadFile = new System.IO.FileStream(_HeadFilePath, System.IO.FileMode.Open,
                //     System.IO.FileAccess.Read, System.IO.FileShare.Read);
            }

            //_IndexFile = IndexFileStreamCache.GetIndexFile(_IndexFilePath);

            //if (_IndexFile == null)
            //{
                //_IndexFile = new CachedFileStream(new System.IO.FileStream(_IndexFilePath, System.IO.FileMode.Open,
                //     System.IO.FileAccess.Read, System.IO.FileShare.Read));

            //    _IndexFile = new System.IO.FileStream(_IndexFilePath, System.IO.FileMode.Open,
            //         System.IO.FileAccess.Read, System.IO.FileShare.Read);

            //    IndexFileStreamCache.AddIndexFile(_IndexFilePath, _IndexFile);
            //}

            _IndexFile = new System.IO.FileStream(_IndexFilePath, System.IO.FileMode.Open,
                 System.IO.FileAccess.Read, System.IO.FileShare.Read);

        }


        public void Reset()
        {
            _CurrentFileIndex = -1;
            _CurrentIndexBuf = null;
            _CurrentDocId = -1;
            _CurrentStepDocIndex = -1;
            _End = false;
            _NextDocPositionList = new DocumentPositionList(-1);
        }

        private bool NextFileIndexBuf()
        {
            if (_WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex == null)
            {
                return NextFileIndex();
            }

            _CurrentStepDocIndex++;

            if (_CurrentStepDocIndex >= _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex.Count)
            {
                return NextFileIndex();
            }

            IndexFile.FilePosition fp = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].IndexFilePostion;

            _CurrentDocId = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex[_CurrentStepDocIndex].DocId;
            int positionInIndex = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex[_CurrentStepDocIndex].Position;

            int length;
            if (_CurrentStepDocIndex < _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex.Count - 1)
            {
                length = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex[_CurrentStepDocIndex + 1].Position - positionInIndex;
            }
            else
            {
                length = fp.Length - positionInIndex;
            }

            _CurrentIndexBuf = _IndexFileProxy.GetIndexBufferMemory(fp.Serial, fp.Position + positionInIndex, length);

            return true;
        }


        private bool NextFileIndex()
        {
            _CurrentFileIndex++;

            if (_CurrentFileIndex >= _WordStepDocIndex.IndexFileInfoList.Count)
            {
                _End = true;
                return false;
            }

            IndexFile.FilePosition fp = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].IndexFilePostion;

            if (_WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex == null)
            {
                _CurrentIndexBuf = _IndexFileProxy.GetIndexBufferMemory(fp.Serial, fp.Position, fp.Length);
                _CurrentDocId = -1;
            }
            else
            {
                _CurrentStepDocIndex = -1;
                _CurrentDocId = -1;
                int length = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex[0].Position;

                _CurrentIndexBuf = _IndexFileProxy.GetIndexBufferMemory(fp.Serial, fp.Position, length);

            }
            return true;
        }

        /// <summary>
        /// Get next original document position list
        /// </summary>
        /// <returns>if end of the index, docid of result is -1</returns>
        public bool GetNextOriginal(ref OriginalDocumentPositionList odpl)
        {
            if (_End)
            {
                odpl.DocumentId = -1;
                return false;
                //return new OriginalDocumentPositionList(-1);
            }

            if (_CurrentDocId < 0)
            {
                if (!NextFileIndex())
                {
                    //End of the index
                    odpl.DocumentId = -1;
                    return false;
                    //return new OriginalDocumentPositionList(-1);
                }
            }

            if (_CurrentIndexBuf.Position < _CurrentIndexBuf.Start + _CurrentIndexBuf.Length)
            {
                DocumentPositionList.GetNextOriginalDocumentPositionList(ref odpl, ref _CurrentDocId,
                    _CurrentIndexBuf, _WordStepDocIndex.SimpleMode);
                return true;
            }
            else
            {
                if (!NextFileIndexBuf())
                {
                    odpl.DocumentId = -1;
                    return false;
                    //return new OriginalDocumentPositionList(-1);
                }

                DocumentPositionList.GetNextOriginalDocumentPositionList(ref odpl, ref _CurrentDocId,
                    _CurrentIndexBuf, _WordStepDocIndex.SimpleMode);
                return true;
            }
        }

        /// <summary>
        /// Get next document position list
        /// </summary>
        /// <returns>if end of the index, docid of result is -1</returns>
        public DocumentPositionList GetNext()
        {
            if (_End)
            {
                return new DocumentPositionList(-1);
            }

            if (_CurrentDocId < 0)
            {
                if (!NextFileIndex())
                {
                    //End of the index
                    return new DocumentPositionList(-1);
                }
            }

            if (_CurrentIndexBuf.Position < _CurrentIndexBuf.Start + _CurrentIndexBuf.Length)
            {
                return DocumentPositionList.GetNextDocumentPositionList(ref _CurrentDocId,
                    _CurrentIndexBuf, _WordStepDocIndex.SimpleMode);
            }
            else
            {
                if (!NextFileIndexBuf())
                {
                    return new DocumentPositionList(-1);
                }

                return DocumentPositionList.GetNextDocumentPositionList(ref _CurrentDocId,
                    _CurrentIndexBuf, _WordStepDocIndex.SimpleMode);
            }
        }

        /// <summary>
        /// Match step doc index for this docid
        /// </summary>
        /// <param name="docId"></param>
        private bool Match(int docid)
        {
            if (_WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex == null)
            {
                //Current file index hasn't skip doc index

                return false;
            }
            else
            {
                int nextStepDocIndex = _CurrentStepDocIndex + 1;

                bool lessThenNextStepDocIndex = false;

                for (; nextStepDocIndex < _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex.Count; nextStepDocIndex++)
                {
                    if (docid <= _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex[nextStepDocIndex].DocId)
                    {
                        lessThenNextStepDocIndex = true;
                        break;
                    }
                }

                if (!lessThenNextStepDocIndex)
                {
                    return false;
                }
                else
                {
                    if (_CurrentStepDocIndex + 1 == nextStepDocIndex)
                    {
                        return false;
                    }
                    else
                    {
                        _CurrentStepDocIndex = nextStepDocIndex - 1;
                        _CurrentDocId = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex[_CurrentStepDocIndex].DocId;
                        IndexFile.FilePosition fp = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].IndexFilePostion;

                        int positionInIndex = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex[_CurrentStepDocIndex].Position;

                        int length;
                        if (_CurrentStepDocIndex < _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex.Count - 1)
                        {
                            length = _WordStepDocIndex.IndexFileInfoList[_CurrentFileIndex].StepDocIndex[_CurrentStepDocIndex + 1].Position - positionInIndex;
                        }
                        else
                        {
                            length = fp.Length - positionInIndex;
                        }

                        _CurrentIndexBuf = _IndexFileProxy.GetIndexBufferMemory(fp.Serial, fp.Position + positionInIndex, length);

                        return true;
                    }
                }
            }
        }

        public DocumentPositionList Get(int docId)
        {
            if (_End)
            {
                return new DocumentPositionList(-1);
            }

            if (docId < 0)
            {
                throw new StoreException(string.Format("Invalid docid = {0}", docId));
            }

            if (_CurrentFileIndex < 0)
            {
                _NextDocPositionList = GetNext();
            }

            if (docId < _NextDocPositionList.DocumentId)
            {
                return new DocumentPositionList(-1);
            }

            if (docId == _NextDocPositionList.DocumentId)
            {
                DocumentPositionList result = _NextDocPositionList;

                _NextDocPositionList = GetNext();

                return result;
            }

            Match(docId); //Get the matched step doc index

            _NextDocPositionList = GetNext();

            while (_NextDocPositionList.DocumentId < docId && _NextDocPositionList.DocumentId >= 0)
            {
                _NextDocPositionList = GetNext();
            }

            if (docId == _NextDocPositionList.DocumentId)
            {
                DocumentPositionList result = _NextDocPositionList;

                _NextDocPositionList = GetNext();

                return result;
            }
            else
            {
                return new DocumentPositionList(-1);
            }

        }

        //public List<IndexFile.WordFilePosition> GetWordFilePositionList()
        //{

        //    List<IndexFile.WordFilePosition> list = new List<IndexFile.WordFilePosition>();

        //    _HeadFile.Seek(0, System.IO.SeekOrigin.Begin);

        //    while (_HeadFile.Position < _HeadFile.Length)
        //    {
        //        byte[] buf = new byte[sizeof(int)];

        //        _HeadFile.Read(buf, 0, sizeof(int));

        //        int size = BitConverter.ToInt32(buf, 0);

        //        //byte[] wordBuf = new byte[size - sizeof(long) - sizeof(long)];
        //        byte[] wordBuf = new byte[size];
        //        _HeadFile.Read(wordBuf, 0, size);

        //        //_HeadFile.Read(buf, 0, sizeof(long));
        //        long position = BitConverter.ToInt64(wordBuf, 0);

        //        //_HeadFile.Read(buf, 0, sizeof(long));
        //        int length = (int)BitConverter.ToInt64(wordBuf, sizeof(long));

        //        //_HeadFile.Read(wordBuf, 0, wordBuf.Length);
        //        string word = Encoding.UTF8.GetString(wordBuf, 2 * sizeof(long), size - 2 * sizeof(long));

        //        list.Add(new IndexFile.WordFilePosition(word, _Serial, position, length));
        //    }

        //    return list;
        //}


        public WordDocumentsList GetDocList(long position, long length, int count)
        {

            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport();

            _IndexFile.Seek(position, System.IO.SeekOrigin.Begin);
            
            //byte[] buf = new byte[length];
            //System.IO.MemoryStream ms = new System.IO.MemoryStream(buf);
            //Hubble.Framework.IO.Stream.ReadToBuf(_IndexFile, buf, 0, buf.Length);
            //ms.Position = 0;

            WordDocumentsList result = new WordDocumentsList();
            bool simple = _IndexMode == Hubble.Core.Data.Field.IndexMode.Simple;

            if (count >= 0)
            {
                result.AddRange(Entity.DocumentPositionList.Deserialize(_IndexFile, ref count, simple, out result.WordCountSum));
                result.RelDocCount = count;
            }
            else
            {
                result.AddRange(Entity.DocumentPositionList.Deserialize(_IndexFile, simple, out result.WordCountSum));
                result.RelDocCount = result.Count;
            }

            performanceReport.Stop(string.Format("Read index file: len={0}, {1} results. ", _IndexFile.Position - position,
                result.Count));

            return result;

        }

        public void Close()
        {
            if (_ReadHead)
            {
                //_HeadFile.Close();
                //_HeadFile = null;
            }

            if (_IndexFile != null)
            {
                _IndexFile.Close();

                _IndexFile = null;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (_IndexFile != null)
                {
                    Close();
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
