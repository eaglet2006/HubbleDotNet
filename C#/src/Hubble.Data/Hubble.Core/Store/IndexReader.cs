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

namespace Hubble.Core.Store
{
    class IndexReader : IDisposable
    {
        string _HeadFilePath;
        string _IndexFilePath;
        private int _Serial;
        bool _ReadHead;

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

            _IndexFile.Close();

            _IndexFile = null;

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
