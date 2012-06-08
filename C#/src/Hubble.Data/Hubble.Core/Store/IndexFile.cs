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
using Hubble.Core.Index;
using Hubble.Core.Entity;
using Hubble.Framework.IO;
using FilePositionType = System.Int64;
using FileLengthType = System.Int32;
using FileSizeType = System.Int64;

namespace Hubble.Core.Store
{
    public interface IIndexFile
    {
        void ImportWordFilePositionList(List<IndexFile.WordFilePosition> wordFilePositionList);

        void CollectWordFilePositionList();
    }

    /// <summary>
    /// Index file, Store one field's full text index
    /// File format:
    /// IndexHead - Segment 0
    /// Word table - Segment 1 to ReserveSegments - 1 
    /// If word table overflow, alloc data segment for it, and link store
    /// Data segments - large than ReserveSegments - 1 
    /// </summary>
    public class IndexFile 
    {
        //public class DocInfo : Hubble.Framework.Serialization.IMySerialization<DocInfo>
        //{
        //    public long DocId;
        //    public int WordCount; //How many words in this document

        //    public DocInfo()
        //    {
        //        DocId = 0;
        //        WordCount = 0;
        //    }

        //    public DocInfo(long docId, int wordCount)
        //    {
        //        DocId = docId;
        //        WordCount = wordCount;
        //    }

        //    #region IMySerialization<DocInfo> Members

        //    public byte Version
        //    {
        //        get
        //        {
        //            return 1;
        //        }
        //    }

        //    public void Serialize(System.IO.Stream s)
        //    {
        //        s.Write(BitConverter.GetBytes(DocId), 0, sizeof(long));
        //        s.Write(BitConverter.GetBytes(WordCount), 0, sizeof(int));
        //    }

        //    public DocInfo Deserialize(System.IO.Stream s, short version)
        //    {
        //        switch (version)
        //        {
        //            case 1:
        //                byte[] buf = new byte[sizeof(long)];
        //                Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(long));
        //                DocId = BitConverter.ToInt64(buf, 0);

        //                Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(int));

        //                WordCount = BitConverter.ToInt32(buf, 0);

        //                return this;
        //            default:
        //                throw new System.Runtime.Serialization.SerializationException(
        //                    string.Format("Invalid version:{0}", version));
        //        }
        //    }

        //    #endregion
        //}

        public struct IndexFileInfo : IComparable<IndexFileInfo>
        {
            public int Serial;

            public FileSizeType Size;

            public DDXFile DDXFile;

            public IDXFile IDXFile;

            public IndexFileInfo(int serial, FileSizeType size, DDXFile ddxFile, IDXFile idxFile)
            {
                Serial = serial;
                Size = size;

                DDXFile = ddxFile;
                IDXFile = idxFile;
            }

            #region IComparable<IndexFileInfo> Members

            public int CompareTo(IndexFileInfo other)
            {
                return Serial.CompareTo(other.Serial);
            }

            #endregion
        }

        public struct FilePosition
        {
            public int Serial; //Index file serial number
            public FileLengthType Length; //word index length
            public FilePositionType Position; //Position in the file for the word point

            public FilePosition(int serial, FilePositionType position, FileLengthType length)
            {
                Serial = serial;
                Position = position;
                Length = length;
            }
        }

        /// <summary>
        /// For the hash table from word to position
        /// </summary>
        public struct WordFilePosition : IComparable<WordFilePosition>
        {
            public string Word;
            public FilePosition Position;

            public WordFilePosition(string word, int serial, FilePositionType position, FileLengthType length)
            {
                Word = word;
                Position = new FilePosition(serial, position, length);
            }

            #region IComparable<WordFilePosition> Members

            public int CompareTo(WordFilePosition other)
            {
                return Hubble.Framework.Text.UnicodeString.Comparer(this.Word, other.Word);
            }

            #endregion
        }

        public struct WordPosition : Hubble.Framework.Serialization.IMySerialization<WordPosition>
        {
            /// <summary>
            /// Word
            /// </summary>
            public string Word;
            
            /// <summary>
            /// The first segment store this word's inverted index
            /// </summary>
            public int FirstSegment;

            /// <summary>
            /// The last segment store this word's inverted index
            /// </summary>
            public int LastSegment;

            /// <summary>
            /// The poistion in the last segment
            /// </summary>
            public int LastPositionInSegment;


            public int Size
            {
                get
                {
                    return Word.Length + 1 * sizeof(int);
                }
            }

            public WordPosition(string word, int fstSegment, int lstSegment, int lstPosition)
            {
                Word = word;
                FirstSegment = fstSegment;
                LastSegment = lstSegment;
                LastPositionInSegment = lstPosition;
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3}", Word, FirstSegment, LastSegment, LastPositionInSegment);
            }

            #region IMySerialization<IndexFile> Members

            public byte Version
            {
                get 
                {
                    return 1;
                }
            }

            public void Serialize(System.IO.Stream s)
            {
                s.Write(BitConverter.GetBytes(Size), 0, sizeof(int));
                
                s.Write(BitConverter.GetBytes(FirstSegment), 0, sizeof(int));
                //s.Write(BitConverter.GetBytes(LastSegment), 0, sizeof(int));
                //s.Write(BitConverter.GetBytes(LastPositionInSegment), 0, sizeof(int));

                byte[] wordBuf = Encoding.UTF8.GetBytes(Word);
                s.Write(wordBuf, 0, wordBuf.Length);

            }

            public WordPosition Deserialize(System.IO.Stream s, short version)
            {
                switch (version)
                {
                    case 1:
                        byte[] buf = new byte[sizeof(int)];
                        Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(int));
                        int size = BitConverter.ToInt32(buf, 0);

                        Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(int));
                        FirstSegment = BitConverter.ToInt32(buf, 0);

                        //Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(int));
                        //LastSegment = BitConverter.ToInt32(buf, 0);
                        
                        //Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, sizeof(int));
                        //LastPositionInSegment = BitConverter.ToInt32(buf, 0);
                        
                        buf = new byte[size - (sizeof(int) * 3)];
                        Hubble.Framework.IO.Stream.ReadToBuf(s, buf, 0, buf.Length);
                        Word = Encoding.UTF8.GetString(buf);
                        return this;
                    default:
                        throw new System.Runtime.Serialization.SerializationException(
                            string.Format("Invalid version:{0}", version));
                }
            }

            #endregion
        }

        const int MaxDocCountInSmallIndex = 5;

        #region Private fields

        private string _FieldName;

        private IndexWriter _IndexWriter;
        private int _MaxSerial = 0;

        private List<WordFilePosition> _WordFilePositionList = null;

        private List<IndexFileInfo> _IndexFileList = new List<IndexFileInfo>();

        private IIndexFile _IndexFileInterface;
        private string _Path;
        private Hubble.Core.Data.Field.IndexMode _IndexMode;
        IndexFileProxy _IndexFileProxy;

        CachedFileStream.CachedType _CachedType = CachedFileStream.CachedType.NoCache;
        int _MinLoadSize = 200; //In KB

        #endregion

        #region Public properties

        internal int MaxSerial
        {
            get
            {
                return _MaxSerial;
            }
        }

        public string IndexDir
        {
            get
            {
                return _Path;
            }
        }

        public IIndexFile IndexFileInterface
        {
            get
            {
                return _IndexFileInterface;
            }
        }

        public List<IndexFileInfo> IndexFileList
        {
            get
            {
                return _IndexFileList;
            }
        }

        /// <summary>
        /// *.idx for normal index
        /// </summary>
        public string FieldName
        {
            get
            {
                return _FieldName;
            }
        }

        public List<WordFilePosition> WordFilePositionList
        {
            get
            {
                return _WordFilePositionList;
            }
        }

        internal string LastDDXFilePath
        {
            get
            {
                return GetDDXFileName(_MaxSerial);
            }
        }

        //internal string LastHeadFilePath
        //{
        //    get
        //    {
        //        return GetHeadFileName(_MaxSerial);
        //    }
        //}

        internal string LastIndexFilePath
        {
            get
            {
                return GetIndexFileName(_MaxSerial);
            }
        }

        public void ClearWordFilePositionList()
        {
            _WordFilePositionList = new List<WordFilePosition>();
        }

        #endregion

        #region Private methods

        private List<IndexFile.WordFilePosition> GetWordFilePositionList(System.IO.FileStream hFile, int serial)
        {
            List<IndexFile.WordFilePosition> list = new List<IndexFile.WordFilePosition>();

            hFile.Seek(0, System.IO.SeekOrigin.Begin);

            while (hFile.Position < hFile.Length)
            {
                byte[] buf = new byte[sizeof(int)];

                hFile.Read(buf, 0, sizeof(int));

                int size = BitConverter.ToInt32(buf, 0);

                //byte[] wordBuf = new byte[size - sizeof(long) - sizeof(long)];
                byte[] wordBuf = new byte[size];
                hFile.Read(wordBuf, 0, size);

                //_HeadFile.Read(buf, 0, sizeof(long));
                long position = BitConverter.ToInt64(wordBuf, 0);

                //_HeadFile.Read(buf, 0, sizeof(long));
                int length = (int)BitConverter.ToInt64(wordBuf, sizeof(long));

                //_HeadFile.Read(wordBuf, 0, wordBuf.Length);
                string word = Encoding.UTF8.GetString(wordBuf, 2 * sizeof(long), size - 2 * sizeof(long));

                list.Add(new IndexFile.WordFilePosition(word, serial, position, length));
            }

            list.Sort();

            return list;
        }

        private void TransferHeadFiles()
        {
            //Get .hdx files in the index folder
            string[] files = System.IO.Directory.GetFiles(_Path, "???????" + FieldName + ".hdx");

            //Transfer .hdx to .ddx file
            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                int serial = int.Parse(fileName.Substring(0, 7));

                string ddxFileName = Path.AppendDivision(_Path, '\\') +
                       GetDDXFileName(serial);

                if (Hubble.Framework.IO.File.GetFileLength(file) <= 0)
                {
                    System.IO.File.Delete(file);
                    continue;
                }

                if (System.IO.File.Exists(ddxFileName))
                {
                    System.IO.File.Delete(ddxFileName);
                }

                using (System.IO.FileStream hFile = new System.IO.FileStream(file, System.IO.FileMode.Open,
                    System.IO.FileAccess.Read, System.IO.FileShare.Read))
                {
                    using (DDXFile ddxFile = new DDXFile(ddxFileName, DDXFile.Mode.Write))
                    {
                        foreach (IndexFile.WordFilePosition wfp in GetWordFilePositionList(hFile, serial))
                        {
                            ddxFile.Add(wfp.Word, wfp.Position.Position, wfp.Position.Length);
                        }

                        ddxFile.Close();
                    }
                }

                string bakFile = Path.AppendDivision(_Path, '\\') + GetHeadBakFileName(serial);

                if (System.IO.File.Exists(bakFile))
                {
                    System.IO.File.Delete(bakFile);
                }

                System.IO.File.Move(file, bakFile);
            }
        }

        private void LoadIndexFiles(bool createNew)
        {
            if (!createNew)
            {
                TransferHeadFiles();
            }

            string[] files = System.IO.Directory.GetFiles(_Path, "???????" + FieldName + ".ddx");

            foreach (string file in files)
            {
                try
                {
                    string fileName = System.IO.Path.GetFileName(file);

                    int serial = int.Parse(fileName.Substring(0, 7));

                    string hFile = Path.AppendDivision(_Path, '\\') +
                        GetHeadFileName(serial);
                    string iFile = Path.AppendDivision(_Path, '\\') +
                        GetIndexFileName(serial);

                    string ddxFile = Path.AppendDivision(_Path, '\\') +
                       GetDDXFileName(serial);

                    if (!System.IO.File.Exists(iFile))
                    {
                        if (System.IO.File.Exists(hFile))
                        {
                            System.IO.File.Delete(hFile);
                        }

                        if (System.IO.File.Exists(ddxFile))
                        {
                            System.IO.File.Delete(ddxFile);
                        }
                    }
                    else if (File.GetFileLength(ddxFile) == 0 || File.GetFileLength(iFile) == 0)
                    {
                        try
                        {
                            if (System.IO.File.Exists(hFile))
                            {
                                System.IO.File.Delete(hFile);
                            }

                            if (System.IO.File.Exists(iFile))
                            {
                                System.IO.File.Delete(iFile);
                            }

                            if (System.IO.File.Exists(ddxFile))
                            {
                                System.IO.File.Delete(ddxFile);
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                    else
                    {
                        if (createNew)
                        {
                            try
                            {
                                if (System.IO.File.Exists(hFile))
                                {
                                    System.IO.File.Delete(hFile);
                                }

                                if (System.IO.File.Exists(iFile))
                                {
                                    System.IO.File.Delete(iFile);
                                }

                                if (System.IO.File.Exists(ddxFile))
                                {
                                    System.IO.File.Delete(ddxFile);
                                }
                            }
                            catch (Exception e)
                            {
                            }
                        }
                        else
                        {
                            DDXFile iDDXFile = new DDXFile(ddxFile, DDXFile.Mode.Read);
                            IDXFile idxfile = new IDXFile(iFile, IDXFile.Mode.Read);

                            IndexFileInfo ifi = new IndexFileInfo(serial, File.GetFileLength(iFile),
                                iDDXFile, idxfile);

                            _IndexFileList.Add(ifi);
                            SetRamIndex(ifi, _CachedType, _MinLoadSize);
                        }
                    }
                }
                catch(Exception e)
                {

                }
            }

            _IndexFileList.Sort();

            //foreach (IndexFileInfo fi in _IndexFileList)
            //{
            //    using (IndexReader ir = new IndexReader(fi.Serial, _Path, FieldName, _IndexMode))
            //    {
            //        List<WordFilePosition> wfp = ir.GetWordFilePositionList();

            //        IndexFileInterface.ImportWordFilePositionList(wfp);

            //        wfp.Clear();
            //        wfp = null;
            //        GC.Collect();
            //        GC.Collect();
            //    }
            //}

            if (_IndexFileList.Count == 0)
            {
                _MaxSerial = 0;
            }
            else
            {
                _MaxSerial = _IndexFileList[_IndexFileList.Count - 1].Serial + 1;
            }

            IndexFileInterface.CollectWordFilePositionList();
        }

        private void CreateIndexFile(Hubble.Core.Data.Field.IndexMode indexMode)
        {
            _IndexWriter = new IndexWriter(_MaxSerial, _Path, FieldName, indexMode);

            _WordFilePositionList = new List<WordFilePosition>();
        }

        private IDXFile GetIDXFile(int serial)
        {
            foreach (IndexFileInfo ifi in _IndexFileList)
            {
                if (ifi.Serial == serial)
                {
                    return ifi.IDXFile;
                }
            }

            return null;
        }

        private void SetRamIndex(IndexFileInfo ifi, Hubble.Framework.IO.CachedFileStream.CachedType type, int minLoadSize)
        {
            if (type == CachedFileStream.CachedType.NoCache)
            {
                ifi.DDXFile.SetRamIndex(CachedFileStream.CachedType.NoCache, minLoadSize);
            }
            else
            {
                ifi.DDXFile.SetRamIndex(CachedFileStream.CachedType.Full, minLoadSize);
            }

            ifi.IDXFile.SetRamIndex(type, minLoadSize);
        }

        #endregion

        #region Internal methods
        internal void SetRamIndex(Hubble.Framework.IO.CachedFileStream.CachedType type, int minLoadSize)
        {
            foreach (IndexFileInfo ifi in _IndexFileList)
            {
                SetRamIndex(ifi, type, minLoadSize);
            }

            _CachedType = type;
            _MinLoadSize = minLoadSize;
        }

        #endregion

        #region Public methods

        public IndexFile(string path, IIndexFile indexFileInterface, IndexFileProxy indexProxy)
        {
            _IndexFileInterface = indexFileInterface;
            _Path = Hubble.Framework.IO.Path.AppendDivision(path, '\\');
            _IndexFileProxy = indexProxy;
        }

        /// <summary>
        /// Create file
        /// </summary>
        /// <param name="createNew">
        /// if createNew is ture, delete index file and create a new file
        /// else if FilePath exist, open the index
        /// </param>
        public void Create(string fieldName, bool createNew, Hubble.Core.Data.Field.IndexMode indexMode)
        {
            _IndexMode = indexMode;

            _FieldName = fieldName;

            LoadIndexFiles(createNew);

            CreateIndexFile(indexMode);
        }

        public void Create(string fieldName, Hubble.Core.Data.Field.IndexMode indexMode)
        {
            Create(fieldName, false, indexMode);
        }

        public void Close()
        {
            if (_IndexWriter != null)
            {
                _IndexWriter.Close();
            }

            if (_IndexFileList != null)
            {
                foreach (IndexFileInfo fi in _IndexFileList)
                {
                    if (fi.DDXFile != null)
                    {
                        fi.DDXFile.Close();
                        fi.IDXFile.Close();
                    }
                }
            }
        }

        public void CloseSerial(int serial)
        {
            if (_IndexFileList != null)
            {
                foreach (IndexFileInfo fi in _IndexFileList)
                {
                    if (fi.DDXFile != null)
                    {
                        if (fi.Serial == serial)
                        {
                            fi.DDXFile.Close();
                            fi.IDXFile.Close();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read File Position List of this word from .ddx file.
        /// </summary>
        /// <param name="word">word</param>
        /// <returns>File position list</returns>
        public List<FilePosition> GetFilePositionListByWord(string word)
        {
            List<FilePosition> result = new List<FilePosition>();

            foreach (IndexFileInfo ifi in _IndexFileList)
            {
                DDXUnit ddxUnit = ifi.DDXFile.Find(word);

                if (ddxUnit != null)
                {
                    FilePosition fp = new FilePosition(ifi.Serial, ddxUnit.Position, (int)ddxUnit.Length);

                    result.Add(fp);
                }
            }

            return result;
        }

        public void AddWordAndDocList(string word, DocumentPositionList first, int docsCount, IEnumerable<Entity.DocumentPositionList> docList)
        {
            FileLengthType length;
            FilePositionType position = _IndexWriter.AddWordAndDocList(word, first, docsCount, docList, out length);

            _WordFilePositionList.Add(new WordFilePosition(word, _MaxSerial, position, length));
        }


        /// <summary>
        /// Only get the word step doc index.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="filePositionList"></param>
        /// <param name="totalDocs"></param>
        /// <param name="dbProvider"></param>
        /// <param name="maxReturnCount"></param>
        /// <returns></returns>
        internal Hubble.Core.Index.WordIndexReader GetWordIndexWithWordStepDocIndex(string word,
            WordFilePositionList filePositionList, int totalDocs, Data.DBProvider dbProvider, int maxReturnCount)
        {
            List<WordStepDocIndex.IndexFileInfo> indexFileInfoList =
                new List<WordStepDocIndex.IndexFileInfo>(); //StepDocIndex of all index files

            bool simple = _IndexMode == Hubble.Core.Data.Field.IndexMode.Simple;
            long wordCountSum = 0;

            foreach (FilePosition filePosition in filePositionList.FPList)
            {
                IDXFile idxFile = GetIDXFile(filePosition.Serial);

                if (idxFile == null)
                {
                    continue;
                }

                int count;
                long indexPosition;
                
                List<Hubble.Core.Entity.DocumentPosition> stepDocIndex =
                    idxFile.GetStepDocIndex(filePosition.Position, filePosition.Length, out count, out indexPosition);

                indexFileInfoList.Add(new WordStepDocIndex.IndexFileInfo(stepDocIndex,
                    new FilePosition(filePosition.Serial, indexPosition,
                    (int)(filePosition.Length - (indexPosition - filePosition.Position) - sizeof(int))), //Subtract last 4 bytes for last doc id and Subtract stepdocindex length
                    count));

                wordCountSum += count;
            }

            int relDocCount = (int)wordCountSum;

            //if (maxReturnCount >= 0)
            //{
            //    if (wordCountSum > maxReturnCount)
            //    {
            //        relDocCount = maxReturnCount;
            //    }
            //}

            WordStepDocIndex wordStepDocIndex = new WordStepDocIndex(indexFileInfoList,
                wordCountSum, relDocCount, simple);

            return new WordIndexReader(word, wordStepDocIndex, totalDocs, 
                dbProvider, _IndexFileProxy, maxReturnCount);

        }

        internal BufferMemory GetIndexBufferMemory(int serial, long position, long length)
        {
            IDXFile idxFile = GetIDXFile(serial);

            if (idxFile == null)
            {
                throw new Hubble.Core.Store.StoreException(string.Format("GetIndex Buf fail! serial={0} does not exist",
                    serial));
            }

            return idxFile.GetIndexBufferMemory(position, length);
        }

        internal System.IO.MemoryStream GetIndexBuf(int serial, long position, long length)
        {
            IDXFile idxFile = GetIDXFile(serial);

            if (idxFile == null)
            {
                throw new Hubble.Core.Store.StoreException(string.Format("GetIndex Buf fail! serial={0} does not exist",
                    serial));
            }

            return idxFile.GetIndexBuf(position, length);
        }


        internal Hubble.Core.Index.WordIndexReader GetWordIndex(string word,
            WordFilePositionList filePositionList, int totalDocs, Data.DBProvider dbProvider, int maxReturnCount)
        {
            WordDocumentsList docList = new WordDocumentsList();

            bool simple = _IndexMode == Hubble.Core.Data.Field.IndexMode.Simple;

            if (maxReturnCount < 0)
            {
                foreach (FilePosition filePosition in filePositionList.FPList)
                {
                    IDXFile idxFile = GetIDXFile(filePosition.Serial);
                    
                    if (idxFile == null)
                    {
                        continue;
                    }

                    WordDocumentsList wdl = idxFile.GetDocList(filePosition.Position, filePosition.Length, -1, simple);

                    if (filePositionList.Count == 1)
                    {
                        docList = wdl;
                    }
                    else
                    {
                        docList.AddRange(wdl);
                        docList.WordCountSum += wdl.WordCountSum;
                    }
                }
            }
            else
            {
                int remain = maxReturnCount;

                foreach (FilePosition filePosition in filePositionList.FPList)
                {
                    IDXFile idxFile = GetIDXFile(filePosition.Serial);

                    if (idxFile == null)
                    {
                        continue;
                    }

                    WordDocumentsList wdl = idxFile.GetDocList(filePosition.Position, filePosition.Length, remain, simple);

                    if (filePositionList.Count == 1)
                    {
                        docList = wdl;
                    }
                    else
                    {
                        docList.AddRange(wdl);
                        docList.WordCountSum += wdl.WordCountSum;
                        docList.RelDocCount += wdl.RelDocCount;
                    }

                    remain -= wdl.Count;
                }
            }

            return new WordIndexReader(word, docList, totalDocs, dbProvider);
        }


        /// <summary>
        /// Collect forcedly
        /// </summary>
        public void Collect()
        {
            _IndexWriter.Close();

            string ddxFile = Path.AppendDivision(_Path, '\\') +
               GetDDXFileName(_MaxSerial);

            string iFile = Path.AppendDivision(_Path, '\\') +
               GetIndexFileName(_MaxSerial);

            DDXFile iDDXFile = new DDXFile(ddxFile, DDXFile.Mode.Read);

            IDXFile idxfile = new IDXFile(iFile, IDXFile.Mode.Read);

            IndexFileInfo ifi = new IndexFileInfo(_MaxSerial, File.GetFileLength(_IndexWriter.IndexFilePath),
                iDDXFile, idxfile);

            _IndexFileList.Add(ifi);

            SetRamIndex(ifi, _CachedType, _MinLoadSize);

            _MaxSerial++;

            _IndexWriter = new IndexWriter(_MaxSerial, _Path,
                System.IO.Path.GetFileNameWithoutExtension(_FieldName), _IndexMode);
        }

        public void AfterMerge(int beginSerial, int endSerial, int mergedSerial)
        {
            int i = 0;
            bool fst = true;

            while (i < IndexFileList.Count)
            {
                if (IndexFileList[i].Serial >= beginSerial && IndexFileList[i].Serial <= endSerial)
                {
                    if (fst)
                    {
                        string ddxFile = Path.AppendDivision(_Path, '\\') +
                           GetDDXFileName(mergedSerial);
                        DDXFile iDDXFile = new DDXFile(ddxFile, DDXFile.Mode.Read);

                        string iFile = Path.AppendDivision(_Path, '\\') +
                           GetIndexFileName(mergedSerial);
                        IDXFile idxfile = new IDXFile(iFile, IDXFile.Mode.Read);

                        IndexFileInfo ifi = new IndexFile.IndexFileInfo(mergedSerial,
                            File.GetFileLength(IndexDir + GetIndexFileName(mergedSerial)),
                            iDDXFile, idxfile);

                        IndexFileList[i] = ifi;

                        SetRamIndex(ifi, _CachedType, _MinLoadSize);

                        fst = false;
                        i++;
                    }
                    else
                    {
                        IndexFileList.RemoveAt(i);
                    }
                }
                else
                {
                    i++;
                }
            }

            //If IndexWriter is writting currently, don't change _MaxSerial
            //No longer insert after optimize
            if (IndexFileList[IndexFileList.Count - 1].Serial == mergedSerial &&
                _WordFilePositionList.Count == 0)
            {
                _IndexWriter.Close();

                if (System.IO.File.Exists(_IndexWriter.DDXFilePath))
                {
                    System.IO.File.Delete(_IndexWriter.DDXFilePath);
                }

                if (System.IO.File.Exists(_IndexWriter.HeadFilePath))
                {
                    System.IO.File.Delete(_IndexWriter.HeadFilePath);
                }

                if (System.IO.File.Exists(_IndexWriter.IndexFilePath))
                {
                    System.IO.File.Delete(_IndexWriter.IndexFilePath);
                }

                _MaxSerial = mergedSerial + 1;
                _IndexWriter = new IndexWriter(_MaxSerial, _Path, 
                    System.IO.Path.GetFileNameWithoutExtension(_FieldName), _IndexMode);
            }
        }

        private string GetHeadBakFileName(int serialNo)
        {
            return string.Format("{0:D7}{1}.hbk", serialNo, FieldName);
        }

        public string GetDDXFileName(int serialNo)
        {
            return string.Format("{0:D7}{1}.ddx", serialNo, FieldName);
        }

        public string GetHeadFileName(int serialNo)
        {
            return string.Format("{0:D7}{1}.hdx", serialNo, FieldName);
        }

        public string GetIndexFileName(int serialNo)
        {
            return string.Format("{0:D7}{1}.idx", serialNo, FieldName);
        }

        #endregion

    }
}
