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
    /// Data segments - large then ReserveSegments - 1 
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

            public IndexFileInfo(int serial, FileSizeType size)
            {
                Serial = serial;
                Size = size;
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
        public struct WordFilePosition
        {
            public string Word;
            public FilePosition Position;

            public WordFilePosition(string word, int serial, FilePositionType position, FileLengthType length)
            {
                Word = word;
                Position = new FilePosition(serial, position, length);
            }
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

        internal string LastHeadFilePath
        {
            get
            {
                return GetHeadFileName(_MaxSerial);
            }
        }

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

        private void LoadIndexFiles(bool createNew)
        {
            string[] files = System.IO.Directory.GetFiles(_Path, "???????" + FieldName + ".hdx");

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

                    if (!System.IO.File.Exists(iFile))
                    {
                        System.IO.File.Delete(hFile);
                    }
                    else if (File.GetFileLength(hFile) == 0 || File.GetFileLength(iFile) == 0)
                    {
                        try
                        {
                            System.IO.File.Delete(hFile);
                            System.IO.File.Delete(iFile);
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
                                System.IO.File.Delete(hFile);
                                System.IO.File.Delete(iFile);
                            }
                            catch (Exception e)
                            {
                            }
                        }
                        else
                        {
                            _IndexFileList.Add(new IndexFileInfo(serial, File.GetFileLength(iFile)));
                        }
                    }
                }
                catch(Exception e)
                {

                }
            }

            _IndexFileList.Sort();

            foreach (IndexFileInfo fi in _IndexFileList)
            {
                using (IndexReader ir = new IndexReader(fi.Serial, _Path, FieldName, _IndexMode))
                {
                    List<WordFilePosition> wfp = ir.GetWordFilePositionList();

                    IndexFileInterface.ImportWordFilePositionList(wfp);

                    wfp.Clear();
                    wfp = null;
                    GC.Collect();
                    GC.Collect();
                }
            }

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

        #endregion


        #region Public methods

        public IndexFile(string path, IIndexFile indexFileInterface)
        {
            _IndexFileInterface = indexFileInterface;
            _Path = Hubble.Framework.IO.Path.AppendDivision(path, '\\');
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
        }

        public void AddWordAndDocList(string word, List<Entity.DocumentPositionList> docList)
        {
            FileLengthType length;
            FilePositionType position = _IndexWriter.AddWordAndDocList(word, docList, out length);

            _WordFilePositionList.Add(new WordFilePosition(word, _MaxSerial, position, length));
        }

        public LinkedSegmentFileStream.SegmentPosition AddDocList(LinkedSegmentFileStream.SegmentPosition segPosition, 
            List<Entity.DocumentPositionList> docList)
        {
            return new LinkedSegmentFileStream.SegmentPosition();
        }

        internal Hubble.Core.Index.InvertedIndex.WordIndexReader GetWordIndex(string word,
            WordFilePositionList filePositionList, int totalDocs, Data.DBProvider dbProvider, int maxReturnCount)
        {
            WordDocumentsList docList = new WordDocumentsList();

            if (maxReturnCount < 0)
            {
                foreach (FilePosition filePosition in filePositionList.Values)
                {
                    using (IndexReader ir = new IndexReader(filePosition.Serial, _Path, FieldName, _IndexMode, false))
                    {
                        WordDocumentsList wdl = ir.GetDocList(filePosition.Position, filePosition.Length, -1);

                        if (filePositionList.Count == 1)
                        {
                            docList = wdl;
                        }
                        else
                        {
                            docList.AddRange(wdl);
                            docList.WordCountSum += wdl.WordCountSum;
                        }

                        //foreach (Entity.DocumentPositionList dList in ir.GetDocList(filePosition.Position, filePosition.Length))
                        //{
                        //    docList.Add(dList);
                        //}
                    }
                }
            }
            else
            {
                int remain = maxReturnCount;

                foreach (FilePosition filePosition in filePositionList.Values)
                {
                    using (IndexReader ir = new IndexReader(filePosition.Serial, _Path, FieldName, _IndexMode, false))
                    {
                        WordDocumentsList wdl = ir.GetDocList(filePosition.Position, filePosition.Length, remain);

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

                        //foreach (Entity.DocumentPositionList dList in ir.GetDocList(filePosition.Position, filePosition.Length))
                        //{
                        //    docList.Add(dList);
                        //}
                    }
                }
            }

            return new InvertedIndex.WordIndexReader(word, docList, totalDocs, dbProvider);
        }

        /// <summary>
        /// Collect forcedly
        /// </summary>
        public void Collect()
        {
            _IndexWriter.Close();

            _IndexFileList.Add(new IndexFileInfo(_MaxSerial, File.GetFileLength(_IndexWriter.IndexFilePath)));

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
                        IndexFileList[i] = new IndexFile.IndexFileInfo(mergedSerial,
                            File.GetFileLength(IndexDir + GetIndexFileName(mergedSerial)));
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
