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
namespace Hubble.Core.Store
{
    /// <summary>
    /// This class packages all the functions to read or write inverted index file (.idx)
    /// </summary>
    public class IDXFile : IDisposable
    {
        public enum Mode
        {
            Read = 0,
            Write = 1
        }

        private Mode _Mode;
        private string _FilePath;
        //private FileStream _IndexFile = null;
        private Hubble.Framework.IO.CachedFileStream _IndexFile = null;

        /// <summary>
        /// file path of .idx file
        /// </summary>
        public string FilePath
        {
            get
            {
                return _FilePath;
            }
        }

        /// <summary>
        /// Constractor 
        /// </summary>
        /// <param name="filePath">.idx file path</param>
        /// <param name="mode">file access mode</param>
        public IDXFile(string filePath, Mode mode)
        {
            _Mode = mode;
            _FilePath = filePath;

            switch (mode)
            {
                case Mode.Read:
                    //_IndexFile = new FileStream(_FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _IndexFile = new Hubble.Framework.IO.CachedFileStream(Hubble.Framework.IO.CachedFileStream.CachedType.NoCache, _FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    break;

                case Mode.Write:
                    //_IndexFile = new FileStream(_FilePath, FileMode.Create, FileAccess.ReadWrite);
                    _IndexFile = new Hubble.Framework.IO.CachedFileStream(_FilePath, FileMode.Create, FileAccess.ReadWrite);
                    break;
            }
        }

        ~IDXFile()
        {
            Dispose();
        }

        #region Public methods

        internal void SetRamIndex(Hubble.Framework.IO.CachedFileStream.CachedType type, int minLoadSize)
        {
            if (_Mode == Mode.Read)
            {
                _IndexFile.ChangeCachedType(type);
                _IndexFile.MinCacheLength = minLoadSize * 1024;
            }
        }

        /// <summary>
        /// Close .idx file
        /// </summary>
        public void Close()
        {
            try
            {
                if (_IndexFile != null)
                {
                    if (_Mode == Mode.Write)
                    {
                        _IndexFile.Flush();
                    }

                    _IndexFile.Close();
                    _IndexFile = null;
                }
            }
            catch
            {
            }
        }

        #region Motheds for read

        /// <summary>
        /// Get step doc index.
        /// </summary>
        /// <param name="position">first byte of the index (include step doc index)</param>
        /// <param name="length">index buffer length</param>
        /// <param name="count">count of the document records in this index</param>
        /// <param name="indexPostion">first byte of the index (exclude step doc index)<</param>
        /// <returns>step doc index. If no step doc index, return null</returns>
        public List<DocumentPosition> GetStepDocIndex(long position, long length, 
            out int count, out long indexPostion)
        {
            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport();


            List<DocumentPosition> result;

            //indexPostion = position;

            _IndexFile.Seek(position, System.IO.SeekOrigin.Begin);

            result = DocumentPositionList.DeserializeSkipDocIndex(_IndexFile, false, out count);

            if (result != null)
            {
                count = DocumentPositionList.GetDocumentsCount(_IndexFile);
            }

            indexPostion = _IndexFile.Position; //The index position exclude count;

            performanceReport.Stop(string.Format("Read index file: len={0}, {1} results. ", _IndexFile.Position - position,
                count));

            return result;

        }

        /// <summary>
        /// Get the specify document position list from .idx file
        /// </summary>
        /// <param name="position">the position of the first byte of the index</param>
        /// <param name="length">index content length</param>
        /// <param name="count">max count of the documents that want read</param>
        /// <param name="simple">is it simple index mode</param>
        /// <returns>Word position list</returns>
        public WordDocumentsList GetDocList(long position, long length, int count, bool simple)
        {
            Query.PerformanceReport performanceReport = new Hubble.Core.Query.PerformanceReport();

            _IndexFile.Seek(position, System.IO.SeekOrigin.Begin);

            WordDocumentsList result = new WordDocumentsList();

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

        private byte[] GetIndexBufToArray(long position, long length)
        {
            _IndexFile.Seek(position, System.IO.SeekOrigin.Begin);

            byte[] buf = new byte[length];

            if (!Hubble.Framework.IO.File.ReadToBuffer(_IndexFile, buf))
            {
                throw new Hubble.Core.Store.StoreException(string.Format("GetIndex Buf fail! position={0} length={1}",
                    position, length));

            }

            return buf;
        }

        public Hubble.Framework.IO.BufferMemory GetIndexBufferMemory(long position, long length)
        {
            _IndexFile.Seek(position, System.IO.SeekOrigin.Begin);

            byte[] buf;
            int offsetInBuf;

            int count = _IndexFile.Read(out buf, out offsetInBuf, (int)length);

            if (count != length)
            {
                throw new Hubble.Core.Store.StoreException(string.Format("GetIndex Buf fail! position={0} length={1}",
                    position, length));
            }

            return new Hubble.Framework.IO.BufferMemory(buf, offsetInBuf, count);
        }

        public System.IO.MemoryStream GetIndexBuf(long position, long length)
        {
            return new MemoryStream(GetIndexBufToArray(position, length));
        }

        #endregion

        #region Motheds for write

        /// <summary>
        /// Add document list of one word into inverted index file.
        /// </summary>
        /// <param name="word">word</param>
        /// <param name="simple">is it simple index?</param>
        /// <param name="first">first document position information</param>
        /// <param name="docsCount">document count that want to added</param>
        /// <param name="docList">doucment position list</param>
        /// <param name="length">output the length of index content</param>
        /// <returns>position of the first byte of this word's index in .idx file</returns>
        public long AddWordAndDocList(string word, bool simple, 
            DocumentPositionList first, int docsCount, 
            IEnumerable<Entity.DocumentPositionList> docList, out int length)
        {
            long position = _IndexFile.Position;
            Entity.DocumentPositionList.Serialize(first, docsCount, docList, _IndexFile, simple);

            length = (int)(_IndexFile.Position - position);

            return position;
        }


        #endregion


        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
