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
        int _RelDocCount;

        Data.DBProvider _DBProvider;

        //int _TabIndex;
        int _TotalDocs;

        //delete
        DeleteProvider _DelProvider = null;

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
                    return _ListForReader.Count;
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

        #region internal methods

        //internal void WordUpdate(string word, List<DocumentPositionList> docList)
        //{
        //    lock (this)
        //    {
        //        _ListForReader.AddRange(docList);
        //        UpdateDocScoreList();
        //    }
        //}

        internal WordIndexReader(string word, WordDocumentsList docList, int totalDocs,
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

            //UpdateDocScoreList();
        }

        #endregion

        public override string ToString()
        {
            return Word;
        }

    }

}
