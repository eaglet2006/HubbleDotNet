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
using System.IO;

using Hubble.Core.Entity;
using Hubble.Core.Index;

namespace Hubble.Core.Data
{
    class DeleteProvider
    {
        const string FileName = "Delete.db";
        Dictionary<int, int> _DeleteTbl = new Dictionary<int,int>(); //DocId is the key

        string _DelFileName;
        int _DeleteStamp;
        object _DeleteStampLock = new object();

        int[] _DelDocs = new int[0];

        public int DeleteStamp
        {
            get
            {
                lock (_DeleteStampLock)
                {
                    return _DeleteStamp;
                }
            }
        }

        internal int Count
        {
            get
            {
                return _DeleteTbl.Count;
            }
        }

        private void GetDelDocs()
        {
            _DelDocs = new int[Count];
            int i = 0;

            foreach (int docid in _DeleteTbl.Keys)
            {
                _DelDocs[i++] = docid;
            }

            Array.Sort(_DelDocs);
        }


        public void Open(string indexFolder)
        {
            _DelFileName = Hubble.Framework.IO.Path.AppendDivision(indexFolder, '\\') + FileName;

            using (FileStream fs = new FileStream(_DelFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                byte[] buf = new byte[sizeof(long)];

                while (fs.Read(buf, 0, buf.Length) == buf.Length)
                {
                    int docId = (int)BitConverter.ToInt64(buf, 0);

                    if (!_DeleteTbl.ContainsKey(docId))
                    {
                        _DeleteTbl.Add(docId, 0);
                    }
                }

                //If delete file crash before, try to fix it.
                long remain = fs.Length % sizeof(long);
                if (remain != 0)
                {
                    fs.SetLength(fs.Length - remain);
                }
            }

            GetDelDocs();
        }

        public void IncDeleteStamp()
        {
            lock (_DeleteStampLock)
            {
                _DeleteStamp++;
            }
        }

        /// <summary>
        /// Delete 
        /// </summary>
        /// <param name="docs"></param>
        /// <returns>Return actually delete count</returns>
        public int Delete(IList<int> docs)
        {
            int count = 0;

            lock (this)
            {
                using (FileStream fs = new FileStream(_DelFileName, FileMode.Append, FileAccess.Write))
                {
                    for (int i = 0; i < docs.Count; i++)
                    {
                        int docId = docs[i];

                        if (!_DeleteTbl.ContainsKey(docId))
                        {
                            count++;
                            _DeleteTbl.Add(docId, 0);
                            fs.Write(BitConverter.GetBytes((long)docId), 0, sizeof(long));
                        }
                    }
                }

                lock (_DeleteStampLock)
                {
                    _DeleteStamp++;
                }

                GetDelDocs();

                return count;
            }
        }

        internal int[] DelDocs 
        {
            get
            {
                lock (this)
                {
                    return _DelDocs;
                }
            }
        }

        public bool DocIdDeleted(int docId)
        {
            lock (this)
            {
                return _DeleteTbl.ContainsKey(docId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="docIdResult">docid result dictionary</param>
        /// <returns>Delete count</returns>
        public int Filter(Core.SFQL.Parse.DocumentResultWhereDictionary docIdResult)
        {
            lock (this)
            {
                int deleteCount = 0;

                if (_DeleteTbl.Count <= 0)
                {
                    return 0;
                }

                bool hasGroupByRecords = docIdResult.GroupByCollection.Count > 0;

                if (_DeleteTbl.Count < docIdResult.Count)
                {
                    foreach (int docid in _DeleteTbl.Keys)
                    {
                        if (docIdResult.ContainsKey(docid))
                        {
                            deleteCount++;
                            docIdResult.Remove(docid);
                        }

                        if (hasGroupByRecords)
                        {
                            if (docIdResult.GroupByContains(docid))
                            {
                                docIdResult.RemoveFromGroupByCollection(docid);
                            }
                        }
                    }
                }
                else
                {
                    List<int> deleDocIdList = new List<int>();

                    foreach (int docid in docIdResult.Keys)
                    {
                        if (_DeleteTbl.ContainsKey(docid))
                        {
                            deleDocIdList.Add(docid);
                        }
                    }

                    foreach (int docid in deleDocIdList)
                    {
                        if (docIdResult.Remove(docid))
                        {
                            deleteCount++;
                        }

                        if (hasGroupByRecords)
                        {
                            docIdResult.RemoveFromGroupByCollection(docid);
                        }
                    }

                    deleDocIdList = null;
                }

                return deleteCount;
            }
        }

        public Store.WordDocumentsList GetDocumentPositionList(Store.WordDocumentsList docList)
        {
            lock (this)
            {
                Store.WordDocumentsList result;

                bool needDel = false;

                for (int i = 0; i < docList.Count; i++)
                {
                    if (_DeleteTbl.ContainsKey(docList[i].DocumentId))
                    {
                        needDel = true;
                        docList[i] = new DocumentPositionList(-1);
                    }
                }

                if (needDel)
                {
                    result = new Store.WordDocumentsList();

                    for (int i = 0; i < docList.Count; i++)
                    {
                        if (docList[i].DocumentId < 0)
                        {
                            result.Add(docList[i]);
                        }
                    }
                }
                else
                {
                    result = docList;
                }

                return result;
            }
        }

    }
}
