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
        Dictionary<long, int> _DeleteTbl = new Dictionary<long,int>(); //DocId is the key

        string _DelFileName;
        int _DeleteStamp;
        object _DeleteStampLock = new object();

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

        public void Open(string indexFolder)
        {
            _DelFileName = Hubble.Framework.IO.Path.AppendDivision(indexFolder, '\\') + FileName;

            using (FileStream fs = new FileStream(_DelFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                byte[] buf = new byte[sizeof(long)];

                while (fs.Read(buf, 0, buf.Length) == buf.Length)
                {
                    long docId = BitConverter.ToInt64(buf, 0);

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
        }

        public void IncDeleteStamp()
        {
            lock (_DeleteStampLock)
            {
                _DeleteStamp++;
            }
        }

        public void Delete(IList<long> docs)
        {
            lock (this)
            {
                using (FileStream fs = new FileStream(_DelFileName, FileMode.Append, FileAccess.Write))
                {
                    for (int i = 0; i < docs.Count; i++)
                    {
                        long docId = docs[i];

                        if (!_DeleteTbl.ContainsKey(docId))
                        {
                            _DeleteTbl.Add(docId, 0);
                            fs.Write(BitConverter.GetBytes(docId), 0, sizeof(long));
                        }
                    }
                }

                lock (_DeleteStampLock)
                {
                    _DeleteStamp++;
                }
            }
        }

        public void FilterDocScore(InvertedIndex.WordIndexReader.DocScore[] docScoreList)
        {
            lock (this)
            {
                if (_DeleteTbl.Count <= 0)
                {
                    return;
                }

                for (int i = 0; i < docScoreList.Length; i++)
                {
                    if (_DeleteTbl.ContainsKey(docScoreList[i].DocId))
                    {
                        docScoreList[i].DocId = -1;
                    }
                }
            }
        }

        public List<DocumentPositionList> GetDocumentPositionList(List<DocumentPositionList> docList)
        {
            lock (this)
            {
                List<DocumentPositionList> result;

                bool needDel = false;

                for (int i = 0; i < docList.Count; i++)
                {
                    if (_DeleteTbl.ContainsKey(docList[i].DocumentId))
                    {
                        needDel = true;
                        docList[i] = null;
                    }
                }

                if (needDel)
                {
                    result = new List<DocumentPositionList>();

                    for (int i = 0; i < docList.Count; i++)
                    {
                        if (docList[i] != null)
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
