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
    /// <summary>
    /// Index for every word for writer
    /// </summary>
    public struct WordIndexWriter : IComparable<WordIndexWriter>
    {
        #region Private fields
        string _Word;
        int _Count;

        //List<DocumentPositionList> _ListForWriter;

        Data.Field.IndexMode _IndexMode;

        DocumentPositionAlloc _DocPositionAlloc;
        int _First;
        int _Cur;

        #endregion

        #region internal fields

        internal int TempDocId;
        internal int TempFirstPosition;
        internal int TempWordCountInThisDoc; //How many words (this word) in this doc
        internal int TempTotalWordsInDoc; //Total words in this doc

        #endregion


        #region Private properties

        #endregion

        #region Public Properties

        /// <summary>
        /// Word
        /// </summary>
        public string Word
        {
            get
            {
                return _Word;
            }
        }

        /// <summary>
        /// Total number of documents
        /// </summary>
        public int Count
        {
            get
            {
                return _Count;
            }
        }

        #endregion


        #region Private methods

        #endregion

        #region Public methods

        #endregion

        #region internal methods

        internal DocumentPositionList GetFirstDocList()
        {
            return _DocPositionAlloc.DocPositionPool[_First];
        }

        internal IEnumerable<DocumentPositionList> GetDocListForWriter()
        {
            DocumentPositionList result;
            int cur = _First;

            do
            {
                result = _DocPositionAlloc.DocPositionPool[cur];
                cur = result.Next;

                yield return result;


            } while (cur >= 0);

        }

        //internal IEnumerable<DocumentPositionList> GetDocListForWriter1()
        //{
        //    int j = 0;
        //    foreach (DocumentPositionList docPositionList in GetDocListForWriter1())
        //    {
        //        if (docPositionList.DocumentId != _ListForWriter[j].DocumentId ||
        //            docPositionList.Count != _ListForWriter[j].Count ||
        //            docPositionList.FirstPosition != _ListForWriter[j].FirstPosition ||
        //            docPositionList.TotalWordsInThisDocument != _ListForWriter[j].TotalWordsInThisDocument)
        //        {
        //            Console.WriteLine();
        //        }

        //        j++;
        //    }

        //    for (int i = 0; i < _ListForWriter.Count; i++)
        //    {
        //        yield return _ListForWriter[i];
        //    }
        //}

        internal WordIndexWriter(string word, Data.Field.IndexMode mode, DocumentPositionAlloc alloc)
        {
            _Word = word;
            _IndexMode = mode;
            _DocPositionAlloc = alloc;
            _Count = 0;
            TempDocId = 0;
            TempFirstPosition = 0;
            TempWordCountInThisDoc = 0; //How many words (this word) in this doc
            TempTotalWordsInDoc = 0; //Total words in this doc
            //_ListForWriter = new List<DocumentPositionList>();
            _First = -1;
            _Cur = -1;
        }

        private void Add(DocumentPositionList docPositionList)
        {
            int docPositionId = _DocPositionAlloc.Alloc();

            if (_First < 0)
            {
                _First = docPositionId;
            }

            _DocPositionAlloc.DocPositionPool[docPositionId] = docPositionList;

            if (_Cur < 0)
            {
                _Cur = _First;
            }
            else
            {
                _DocPositionAlloc.DocPositionPool[_Cur].Next = docPositionId;
                _Cur = docPositionId;
            }
        }

        /// <summary>
        /// Index one document into the _ListForWriter
        /// </summary>
        internal void Index()
        {
            try
            {
                if (_IndexMode == Hubble.Core.Data.Field.IndexMode.Simple)
                {
                    Add(new DocumentPositionList(TempDocId, TempWordCountInThisDoc,
                        DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc)));

                    //_ListForWriter.Add(new DocumentPositionList(DocId, WordCountInThisDoc,
                    //    DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc)));
                }
                else
                {
                    if (TempWordCountInThisDoc > 0)
                    {
                        Add(new DocumentPositionList(TempDocId,
                            TempWordCountInThisDoc,
                            DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc),
                            TempFirstPosition));

                        //_ListForWriter.Add(new DocumentPositionList(DocId,
                        //    WordCountInThisDoc,
                        //    DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc),
                        //    FirstPosition));
                    }
                    else
                    {
                        Add(new DocumentPositionList(TempDocId, TempWordCountInThisDoc,
                            DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc)));

                        //_ListForWriter.Add(new DocumentPositionList(DocId, WordCountInThisDoc,
                        //    DocumentPositionList.GetTotalWordsInDocIndex(TempTotalWordsInDoc)));
                    }
                }

                _Count++;
            }
            finally
            {
            }
        }

        #endregion

        #region IComparable<WordIndexWriter> Members

        public int CompareTo(WordIndexWriter other)
        {
            return Hubble.Framework.Text.UnicodeString.Comparer(this.Word, other.Word);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0}:{1}", this.Word, this.Count);
        }
    }


}
