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
using Hubble.Framework.IO;


namespace Hubble.Core.Store
{
    class IndexWriter
    {
        string _HeadFilePath;
        string _IndexFilePath;

        System.IO.FileStream _HeadFile;
        System.IO.FileStream _IndexFile;

        private Hubble.Core.Data.Field.IndexMode _IndexMode;

        public string HeadFilePath
        {
            get
            {
                return _HeadFilePath;
            }
        }

        public string IndexFilePath
        {
            get
            {
                return _IndexFilePath;
            }
        }

        public IndexWriter(int serial, string path, string fieldName, Hubble.Core.Data.Field.IndexMode indexMode)
        {
            _IndexMode = indexMode;

            _HeadFilePath = Path.AppendDivision(path, '\\') + 
                string.Format("{0:D7}{1}.hdx", serial, fieldName);
            _IndexFilePath = Path.AppendDivision(path, '\\') +
                string.Format("{0:D7}{1}.idx", serial, fieldName);

            _HeadFile = new System.IO.FileStream(_HeadFilePath, System.IO.FileMode.Create,
                 System.IO.FileAccess.Write);
            _IndexFile = new System.IO.FileStream(_IndexFilePath, System.IO.FileMode.Create,
                 System.IO.FileAccess.Write);


        }

        public long AddWordAndDocList(string word, DocumentPositionList first, int docsCount, IEnumerable<Entity.DocumentPositionList> docList, out int length)
        {
            long position = _IndexFile.Position;
            bool simple = _IndexMode == Hubble.Core.Data.Field.IndexMode.Simple;
            Entity.DocumentPositionList.Serialize(first, docsCount, docList, _IndexFile, simple);

            length = (int)(_IndexFile.Position - position);

            WriteHeadFile(_HeadFile, word, position, length);

            return position;
        }

        public void Close()
        {
            _HeadFile.Close();
            _IndexFile.Close();
        }

        #region static methods
        public static void WriteHeadFile(System.IO.FileStream fs, string word, long position, long length)
        {
            byte[] wordBuf = Encoding.UTF8.GetBytes(word);

            fs.Write(BitConverter.GetBytes(wordBuf.Length + sizeof(long) + sizeof(long)), 0, sizeof(int)); //Size
            fs.Write(BitConverter.GetBytes(position), 0, sizeof(long)); //doc position in index file

            fs.Write(BitConverter.GetBytes(length), 0, sizeof(long)); //word index data length

            fs.Write(wordBuf, 0, wordBuf.Length);

        }

        #endregion
    }
}
