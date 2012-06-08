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
using System.Diagnostics;
using Hubble.Core.Entity;
using Hubble.Core.Store;
using Hubble.Core.Data;
using Hubble.Framework.DataStructure;
using Hubble.Framework.IO;
using Hubble.Core.SFQL.Parse;

namespace Hubble.Core.Index
{
    public class WordIndexReader
    {
        #region Private fields
        string _Word;

        WordDocumentsList _ListForReader = new WordDocumentsList();
        long _WordCountSum;
        int _Count; //Count of docs that Reader will read.
        int _RelDocCount; //Rel doc count of this word. It is large than or equal with _Count.

        Data.DBProvider _DBProvider;

        //int _TabIndex;
        int _TotalDocs;

        //delete
        DeleteProvider _DelProvider = null;

        IndexReader _IndexReader = null;

        #endregion

        #region Private properties


        internal Store.WordDocumentsList WordDocList
        {
            get
            {
                lock (this)
                {
                    return _ListForReader;
                }
            }
        }


        #endregion

        #region Public Properties

        internal IndexReader IndexReader
        {
            get
            {
                return _IndexReader;
            }
        }

        /// <summary>
        /// Word
        /// </summary>
        public string Word
        {
            get
            {
                lock (this)
                {
                    return _Word;
                }
            }
        }



        /// <summary>
        /// Total number of documents
        /// </summary>
        public int Count
        {
            get
            {
                lock (this)
                {
                    return _Count;
                }
            }
        }

        /// <summary>
        /// if doc count > max return count
        /// this field return rel doc count
        /// </summary>
        public int RelDocCount
        {
            get
            {
                return _RelDocCount;
            }
        }


        /// <summary>
        /// Total number of words 
        /// </summary>
        public long WordCount
        {
            get
            {
                lock (this)
                {
                    return _WordCountSum;
                }
            }
        }

        public DocumentPositionList this[int index]
        {
            get
            {
                lock (this)
                {
                    return _ListForReader[index];
                }
            }
        }

        public DocumentPositionList[] DocPositionBuf
        {
            get
            {
                return _ListForReader.Buf;
            }
        }

        #endregion

        //internal void WordUpdate(string word, List<DocumentPositionList> docList)
        //{
        //    lock (this)
        //    {
        //        _ListForReader.AddRange(docList);
        //        UpdateDocScoreList();
        //    }
        //}

        public WordIndexReader(string word, WordDocumentsList docList, int totalDocs,
            Data.DBProvider dbProvider)
        {
            _Word = word;
            _ListForReader = docList;
            _DBProvider = dbProvider;
            _DelProvider = _DBProvider.DelProvider;
            //_TabIndex = tabIndex;
            _TotalDocs = totalDocs;
            _WordCountSum = _ListForReader.WordCountSum;
            _RelDocCount = _ListForReader.RelDocCount;
            _Count = _ListForReader.RelDocCount;

            //UpdateDocScoreList();
        }

        public WordIndexReader(string word, WordStepDocIndex wordStepDocIndex, int totalDocs,
            Data.DBProvider dbProvider, IndexFileProxy indexProxy, int maxReturnCount)
        {
            _Word = word;
            _IndexReader = new IndexReader(wordStepDocIndex, indexProxy);
            _DBProvider = dbProvider;
            _DelProvider = _DBProvider.DelProvider;
            //_TabIndex = tabIndex;
            _TotalDocs = totalDocs;
            _WordCountSum = wordStepDocIndex.WordCountSum;
            _RelDocCount = wordStepDocIndex.RelDocCount;

            if (maxReturnCount < 0)
            {
                _Count = _RelDocCount;
            }
            else
            {
                _Count = Math.Min(maxReturnCount, _RelDocCount);
            }


            //UpdateDocScoreList();
        }

        public void Reset()
        {
            if (_IndexReader != null)
            {
                _IndexReader.Reset();
            }
        }

        public bool GetNextOriginalWithDocId(ref OriginalDocumentPositionList odpl, int docId)
        {
            if (_IndexReader == null)
            {
                odpl.DocumentId = -1;
                return false;
            }

            return _IndexReader.GetNextOriginalWithDocId(ref odpl, docId);
        }


        public DocumentPositionList Get(int docid)
        {
            if (_IndexReader == null)
            {
                return new DocumentPositionList(-1);
            }

            return _IndexReader.Get(docid);
        }

        public bool GetNextOriginal(ref OriginalDocumentPositionList odpl)
        {
            if (_IndexReader == null)
            {
                odpl.DocumentId = -1;
                return false;
            }

            return _IndexReader.GetNextOriginal(ref odpl);
        }

        public OriginalDocumentPositionList GetNextOriginal()
        {
            if (_IndexReader == null)
            {
                return new OriginalDocumentPositionList(-1);
            }

            OriginalDocumentPositionList odpl = new OriginalDocumentPositionList();
            _IndexReader.GetNextOriginal(ref odpl);
            return odpl;
        }

        public DocumentPositionList GetNext()
        {
            if (_IndexReader == null)
            {
                return new DocumentPositionList(-1);
            }

            return _IndexReader.GetNext();
        }

        public override string ToString()
        {
            return Word;
        }

    }

}
