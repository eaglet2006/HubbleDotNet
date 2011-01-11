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

using Hubble.Core.Entity;
using Hubble.Core.Data;

/***********************************************************************
 * Delay for index cache because it is not very urgent.
 * I will do it later.
 * Global.Cache is used to access the index cache.
 * 2011-01-11 eaglet
 * http://www.cnblogs.com/eaglet/archive/2011/01/11/1932758.html
 **********************************************************************/
namespace Hubble.Core.Cache
{
    /// <summary>
    /// Document information in index for index cache
    /// </summary>
    unsafe struct DocIndexInfo
    {
        internal int DocumentId;

        internal int FirstPosition;

        //internal int* PayloadData;

        internal Int16 Count;

        internal Int16 TotalWordsInThisDocumentIndex;

        internal DocIndexInfo(DocumentPositionList dpl)
        {
            this.DocumentId = dpl.DocumentId;
            this.FirstPosition = dpl.FirstPosition;
            this.Count = dpl.Count;
            this.TotalWordsInThisDocumentIndex = dpl.TotalWordsInThisDocumentIndex;
            //this.PayloadData = dbProvider.GetPayloadData(this.DocumentId);
        }

        internal DocumentPositionList GetDocPositionList()
        {
            DocumentPositionList dpl = new DocumentPositionList();

            dpl.DocumentId = this.DocumentId;
            dpl.FirstPosition = this.FirstPosition;
            dpl.Count = this.Count;
            dpl.SetTotalWordsInThisDocumentIndex(this.TotalWordsInThisDocumentIndex);
            //this.PayloadData   = dbProvider.GetPayloadData(this.DocumentId);
            return dpl;
        }
    }

    class IndexCache
    {
        class CacheUnit
        {
            readonly internal int UnitId = -1;
            internal int FirstDocId = -1;
            internal int Count = 0;
            internal DocIndexInfo[] Unit = null;

            internal CacheUnit(int id)
            {
                UnitId = id;
            }

            internal void Init()
            {
                DocIndexInfo[] Unit = new DocIndexInfo[65536];
            }

            internal bool Add(DocIndexInfo docInfo)
            {
                if (Count >= Unit.Length)
                {
                    return false;
                }

                Unit[Count++] = docInfo;

                return true;
            }
        }

        class CacheUnitAlloc
        {
            CacheUnit[] _Pool;
            Stack<int> _UnUsedIdStack = new Stack<int>();

            internal CacheUnitAlloc(int maxUnitCount)
            {
                _Pool = new CacheUnit[maxUnitCount];

                for (int i = 0; i < maxUnitCount; i++)
                {
                    _Pool[i] = new CacheUnit(i);
                    _UnUsedIdStack.Push(i);
                }
            }

            internal CacheUnit Get()
            {
                if (_UnUsedIdStack.Count <= 0)
                {
                    return null;
                }

                int unitId = _UnUsedIdStack.Pop();

                if (_Pool[unitId].Unit == null)
                {
                    _Pool[unitId].Init();
                }

                return _Pool[unitId];
            }

            internal void Return(CacheUnit cacheUnit)
            {
                _UnUsedIdStack.Push(cacheUnit.UnitId);
            }
                 
        }

        /// <summary>
        /// Index cache for a word
        /// </summary>
        class WordIndexCache
        {
            string _Word;

            internal string Word
            {
                get
                {
                    return _Word;
                }
            }

            LinkedList<CacheUnit> _DocumentIndexInfoList = new LinkedList<CacheUnit>();

            internal WordIndexCache(string word)
            {
                _Word = word;
            }





        }

        private object _LockObj = new object();

        readonly int DocSize;

        private int _MaxMemory = 128 * 1024 * 1024; //In bytes

        /// <summary>
        /// Max memory of index cache, default is 128MB
        /// </summary>
        internal int MaxMemroy
        {
            get
            {
                return _MaxMemory;
            }

            set
            {
                _MaxMemory = value;

                if (_MaxMemory < 64 * 1024 * 1024)
                {
                    _MaxMemory = 64 * 1024 * 1024;
                }
            }
        }

        internal IndexCache()
        {
            DocSize = System.Runtime.InteropServices.Marshal.SizeOf(new DocIndexInfo());
        }



    }
}
