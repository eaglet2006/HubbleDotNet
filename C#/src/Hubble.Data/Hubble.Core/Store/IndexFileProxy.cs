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
using Hubble.Framework.IO;
using Hubble.Framework.Threading;
using Hubble.Core.Data;

namespace Hubble.Core.Store
{
    public class IndexFileProxy : MessageQueue, IIndexFile
    {
        enum Event
        {
            Add = 1,
            Collect = 2,
            Get = 3,
            GetFilePositionList = 4,
            MergeAck = 5,
        }

        public class GetInfo
        {
            string _Word;
            long _TotalDocs;
            private Data.DBProvider _DBProvider;
            private int _TabIndex;

            public string Word
            {
                get
                {
                    return _Word;
                }
            }

            public long TotalDocs
            {
                get
                {
                    return _TotalDocs;
                }
            }

            public Data.DBProvider DBProvider
            {
                get
                {
                    return _DBProvider;
                }
            }

            public int TabIndex
            {
                get
                {
                    return _TabIndex;
                }
            }

            public GetInfo(string word, long totalDocs, Data.DBProvider dbProvider, 
                int tabIndex)
            {
                _Word = word;
                _TotalDocs = totalDocs;
                _DBProvider = dbProvider;
                _TabIndex = tabIndex;
            }
        }

        class WordDocList
        {
            string _Word;

            public string Word
            {
                get
                {
                    return _Word;
                }
            }

            List<Entity.DocumentPositionList> _DocList;

            public List<Entity.DocumentPositionList> DocList
            {
                get
                {
                    return _DocList;
                }
            }

            public WordDocList(string word, List<Entity.DocumentPositionList> docList)
            {
                _Word = word;
                _DocList = docList;
            }
        }

        public class MergeAck
        {
            public class MergeFilePosition
            {
                public IndexFile.FilePosition MergedFilePostion;

                public List<IndexFile.FilePosition> FilePostionList;

                public MergeFilePosition(IndexFile.FilePosition filePostion,
                     List<IndexFile.FilePosition> pList)
                {
                    MergedFilePostion = filePostion;
                    FilePostionList = pList;
                }

            }

            private int _BeginSerial; // Begin file serial;

            public int BeginSerial
            {
                get
                {
                    return _BeginSerial;
                }
            }

            private int _EndSerial; // End file serial;

            public int EndSerial
            {
                get
                {
                    return _EndSerial;
                }
            }

            private int _MergedSerial; // File serial merged;

            public int MergedSerial
            {
                get
                {
                    return _MergedSerial;
                }
            }

            string _MergeHeadFileName;

            public string MergeHeadFileName
            {
                get
                {
                    return _MergeHeadFileName;
                }
            }

            string _MergeIndexFileName;

            public string MergeIndexFileName
            {
                get
                {
                    return _MergeIndexFileName;
                }
            }

            public List<MergeFilePosition> MergeFilePositionList = new List<MergeFilePosition>();

            public MergeAck(int begin, int end, string mergeHead, string mergeIndex, int mergedSerial)
            {
                _BeginSerial = begin;
                _EndSerial = end;
                _MergeHeadFileName = mergeHead;
                _MergeIndexFileName = mergeIndex;
                _MergedSerial = mergedSerial;
            }
        }

        public class MergeInfos
        {
            private int _BeginSerial; // Begin file serial;

            public int BeginSerial
            {
                get
                {
                    return _BeginSerial;
                }
            }

            private int _EndSerial; // End file serial;

            public int EndSerial
            {
                get
                {
                    return _EndSerial;
                }
            }


            private int _MergedSerial; // File serial merged;

            public int MergedSerial
            {
                get
                {
                    return _MergedSerial;
                }
            }

            string _MergeHeadFileName;

            public string MergeHeadFileName
            {
                get
                {
                    return _MergeHeadFileName;
                }
            }

            string _MergeIndexFileName;

            public string MergeIndexFileName
            {
                get
                {
                    return _MergeIndexFileName;
                }
            }

            List<MergedWordFilePostionList> _MergedWordFilePostionList;

            public List<MergedWordFilePostionList> MergedWordFilePostionList
            {
                get
                {
                    return _MergedWordFilePostionList;
                }
            }

            public MergeInfos(string headFileName, string indexFileName,
                List<MergedWordFilePostionList> list, int begin, int end, int mergedSerial)
            {
                _MergeHeadFileName = headFileName;
                _MergeIndexFileName = indexFileName;
                _MergedWordFilePostionList = list;
                _BeginSerial = begin;
                _EndSerial = end;
                _MergedSerial = mergedSerial;
            }
        }

        public class MergedWordFilePostionList : IComparable<MergedWordFilePostionList>
        {
            private string _Word;

            public string Word
            {
                get
                {
                    return _Word;
                }
            }

            private List<IndexFile.FilePosition> _FilePositionList;

            public List<IndexFile.FilePosition> FilePositionList
            {
                get
                {
                    return _FilePositionList;
                }
            }

            private List<IndexFile.FilePosition> _OrginalFilePositionList;

            public List<IndexFile.FilePosition> OrginalFilePositionList
            {
                get
                {
                    return _OrginalFilePositionList;
                }
            }

            public MergedWordFilePostionList(string word, List<IndexFile.FilePosition> orginal)
            {
                _Word = word;
                _OrginalFilePositionList = orginal;
                _FilePositionList = new List<IndexFile.FilePosition>();
            }

            #region IComparable<WordFilePostionList> Members

            public int CompareTo(MergedWordFilePostionList other)
            {
                if (other == null)
                {
                    return 1;
                }

                if (this.FilePositionList.Count == 0 && other.FilePositionList.Count == 0)
                {
                    return 0;
                }

                if (other.FilePositionList.Count == 0)
                {
                    return 1;
                }

                if (this.FilePositionList.Count == 0)
                {
                    return -1;
                }

                if (this.FilePositionList[0].Serial > other.FilePositionList[0].Serial)
                {
                    return 1;
                }
                else if (this.FilePositionList[0].Serial < other.FilePositionList[0].Serial)
                {
                    return -1;
                }
                else
                {
                    long myPosition = this.FilePositionList[0].Position;
                    long otherPosition = other.FilePositionList[0].Position;

                    if (myPosition > otherPosition)
                    {
                        return 1;
                    }
                    else if (myPosition < otherPosition)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }

            }

            #endregion
        }


        public class WordFilePostionList : IComparable<WordFilePostionList>
        {
            private string _Word;

            public string Word
            {
                get
                {
                    return _Word;
                }
            }

            private List<IndexFile.FilePosition> _FilePositionList;

            public List<IndexFile.FilePosition> FilePositionList
            {
                get
                {
                    return _FilePositionList;
                }
            }

            public WordFilePostionList(string word)
            {
                _Word = word;
                _FilePositionList = new List<IndexFile.FilePosition>();
            }

            #region IComparable<WordFilePostionList> Members

            public int CompareTo(WordFilePostionList other)
            {
                if (other == null)
                {
                    return 1;
                }

                if (this.FilePositionList.Count == 0 && other.FilePositionList.Count == 0)
                {
                    return 0;
                }

                if (other.FilePositionList.Count == 0)
                {
                    return 1;
                }

                if (this.FilePositionList[0].Serial > other.FilePositionList[0].Serial)
                {
                    return 1;
                }
                else if (this.FilePositionList[0].Serial < other.FilePositionList[0].Serial)
                {
                    return -1;
                }
                else
                {
                    long myPosition = this.FilePositionList[0].Position;
                    long otherPosition = other.FilePositionList[0].Position;

                    if (myPosition > otherPosition)
                    {
                        return 1;
                    }
                    else if (myPosition < otherPosition)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            #endregion
        }

        private IndexFile _IndexFile;

        private Dictionary<string, List<IndexFile.FilePosition>> _WordFilePositionTable = new Dictionary<string, List<IndexFile.FilePosition>>();

        private int _WordCount = 0;

        private int InnerWordTableSize
        {
            get
            {
                lock (this)
                {
                    return _WordCount;
                }
            }

            set
            {
                lock (this)
                {
                    _WordCount = value;
                }
            }
        }

        private Index.DelegateWordUpdate _WordUpdateDelegate;

        #region Public properties

        public Index.DelegateWordUpdate WordUpdateDelegate
        {
            get
            {
                return _WordUpdateDelegate;
            }

            set
            {
                _WordUpdateDelegate = value;
            }
        }

        public int WordTableSize
        {
            get
            {
                return InnerWordTableSize;
            }
        }
        #endregion

        private List<IndexFile.FilePosition> GetFilePositionListByWord(string word)
        {
            List<IndexFile.FilePosition> pList;

            if (_WordFilePositionTable.TryGetValue(word, out pList))
            {
                return pList;
            }
            else
            {
                return null;
            }
        }

        private void PatchWordFilePositionTable(List<IndexFile.WordFilePosition> wordFilePostionList)
        {
            foreach (IndexFile.WordFilePosition p in wordFilePostionList)
            {
                List<IndexFile.FilePosition> pList;

                if (_WordFilePositionTable.TryGetValue(p.Word, out pList))
                {
                    pList.Add(p.Position);
                }
                else
                {
                    pList = new List<IndexFile.FilePosition>(2);
                    pList.Add(p.Position);

                    string internedWord = string.IsInterned(p.Word);

                    if (internedWord == null)
                    {
                        internedWord = p.Word;
                    }

                    _WordFilePositionTable.Add(internedWord, pList);
                }
            }

            InnerWordTableSize = _WordFilePositionTable.Count;
        }

        private object ProcessGetFilePositionList(int evt, MessageFlag flag, object data)
        {
            OptimizationOption option = (OptimizationOption)data;

            List<MergedWordFilePostionList> result = new List<MergedWordFilePostionList>();

            if (_IndexFile.IndexFileList.Count <= 2)
            {
                if (_IndexFile.IndexFileList.Count <= 1)
                {
                    return null;
                }
                else if (option != OptimizationOption.Minimum)
                {
                    return null;
                }
            }

            int i = 0;
            long fstFileSize = 0;
            long secFileSize = 0;
            long otherFileSize = 0;

            foreach (IndexFile.IndexFileInfo ifi in _IndexFile.IndexFileList)
            {
                if (i == 0)
                {
                    fstFileSize = ifi.Size;
                }
                else if (i == 1)
                {
                    secFileSize = ifi.Size;
                }
                else
                {
                    otherFileSize += ifi.Size;
                }

                i++;
            }

            int begin;
            int end = _IndexFile.IndexFileList[_IndexFile.IndexFileList.Count - 1].Serial;

            switch (option)
            {
                case OptimizationOption.Minimum:
                    begin = _IndexFile.IndexFileList[0].Serial;
                    break;
                case OptimizationOption.Middle:
                    if (fstFileSize < otherFileSize + secFileSize)
                    {
                        begin = _IndexFile.IndexFileList[0].Serial;
                    }
                    else
                    {
                        begin = _IndexFile.IndexFileList[1].Serial;
                    }
                    break;
                case OptimizationOption.Speedy:
                    if (fstFileSize < otherFileSize + secFileSize)
                    {
                        begin = _IndexFile.IndexFileList[0].Serial;
                    }
                    else
                    {
                        begin = _IndexFile.IndexFileList[1].Serial;

                        if (secFileSize > otherFileSize * 10 && _IndexFile.IndexFileList.Count < 32)
                        {
                            //If the index file count < 32 and all other files is small file
                            //Does not need optimize
                            return null;
                        }
                    }
                    break;
                default:
                    return null;
            }

            foreach (string word in _WordFilePositionTable.Keys)
            {
                List<IndexFile.FilePosition> pList = _WordFilePositionTable[word];
                MergedWordFilePostionList wfpl = new MergedWordFilePostionList(word, pList);

                foreach (IndexFile.FilePosition fp in pList)
                {
                    if (fp.Serial >= begin && fp.Serial <= end)
                    {
                        wfpl.FilePositionList.Add(new IndexFile.FilePosition(fp.Serial, fp.Position, fp.Length));
                    }
                }

                result.Add(wfpl);
            }

            int serial;

            if (begin == _IndexFile.IndexFileList[0].Serial)
            {
                serial = 0;
            }
            else
            {
                serial = 1;
            }

            return new MergeInfos(_IndexFile.GetHeadFileName(serial),
                _IndexFile.GetIndexFileName(serial), result, begin, end, serial);
        }

        private void ProcessMergeAck(int evt, MessageFlag flag, object data)
        {
            MergeAck mergeAck = (MergeAck)data;

            int begin = mergeAck.BeginSerial;
            int end = mergeAck.EndSerial;

            for (int serial = begin; serial <= end; serial++)
            {
                string fileName = _IndexFile.IndexDir + GetHeadFileName(serial);

                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }

                fileName = _IndexFile.IndexDir + GetIndexFileName(serial);
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
            }

            System.IO.File.Move(_IndexFile.IndexDir + @"Optimize\" + mergeAck.MergeHeadFileName,
                _IndexFile.IndexDir + mergeAck.MergeHeadFileName);

            System.IO.File.Move(_IndexFile.IndexDir + @"Optimize\" + mergeAck.MergeIndexFileName,
                _IndexFile.IndexDir + mergeAck.MergeIndexFileName);

            foreach (MergeAck.MergeFilePosition mfp in mergeAck.MergeFilePositionList)
            {
                List<IndexFile.FilePosition> pList = mfp.FilePostionList;
                int i = 0;
                bool fst = true;

                while (i < pList.Count)
                {
                    if (pList[i].Serial >= begin && pList[i].Serial <= end)
                    {
                        if (fst)
                        {
                            pList[i] = mfp.MergedFilePostion;
                            fst = false;
                            i++;
                        }
                        else
                        {
                            pList.RemoveAt(i);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            _IndexFile.AfterMerge(begin, end, mergeAck.MergedSerial);
        }

        private object ProcessMessage(int evt, MessageFlag flag, object data)
        {
            switch ((Event)evt)
            {
                case Event.Add:
                    WordDocList wl = (WordDocList)data;
                    _IndexFile.AddWordAndDocList(wl.Word, wl.DocList);
                    if (WordUpdateDelegate != null)
                    {
                        WordUpdateDelegate(wl.Word, wl.DocList);
                    }

                    break;
                case Event.Collect:
                    _IndexFile.Collect();
                    PatchWordFilePositionTable(_IndexFile.WordFilePositionList);
                    _IndexFile.ClearWordFilePositionList();
                    break;
                case Event.Get:
                    {
                        GetInfo getInfo = data as GetInfo;
                        List<IndexFile.FilePosition> pList = GetFilePositionListByWord(getInfo.Word);
                        return _IndexFile.GetWordIndex(getInfo.Word, pList, getInfo.TotalDocs,
                            getInfo.DBProvider, getInfo.TabIndex);
                    }
                case Event.GetFilePositionList:
                    return ProcessGetFilePositionList(evt, flag, data);

                case Event.MergeAck:
                    ProcessMergeAck(evt, flag, data);
                    break;

            }

            return null;
        }


        public IndexFileProxy(string path, string fieldName)
            : this(path, fieldName, false)
        {

        }

        public IndexFileProxy(string path, string fieldName, bool rebuild)
            : base()
        {
            OnMessageEvent = ProcessMessage;
            _IndexFile = new IndexFile(path, this);
            _IndexFile.Create(fieldName, rebuild);

            this.Start();
        }

        //public void AddDocInfos(List<IndexFile.DocInfo> docInfos)
        //{
        //}

        public MergeInfos GetMergeInfos(Data.OptimizationOption option)
        {
            return SSendMessage((int)Event.GetFilePositionList,
                option, 30 * 1000) as MergeInfos;
        }

        public void DoMergeAck(MergeAck mergeAck)
        {
            SSendMessage((int)Event.MergeAck, mergeAck, 300 * 1000); //time out 5 min
        }

        public void AddWordPositionAndDocumentPositionList(string word,
            List<Entity.DocumentPositionList> docList)
        {
            ASendMessage((int)Event.Add, new WordDocList(word, docList));
        }


        public Hubble.Core.Index.InvertedIndex.WordIndexReader GetWordIndex(GetInfo getInfo)
        {
            return SSendMessage((int)Event.Get, getInfo, 30 * 1000) as
                Hubble.Core.Index.InvertedIndex.WordIndexReader;

            //List<IndexFile.FilePosition> pList = GetFilePositionListByWord(word);
            //return _IndexFile.GetWordIndex(word, pList);
        }

        public void Collect()
        {
            ASendMessage((int)Event.Collect, null);
        }


        new public void Close(int millisecondsTimeout)
        {
            base.Close(millisecondsTimeout);

            _WordFilePositionTable.Clear();
            _IndexFile.Close();
            _IndexFile = null;
            GC.Collect();
        }

        public string GetHeadFileName(int serialNo)
        {
            return _IndexFile.GetHeadFileName(serialNo);
        }

        public string GetIndexFileName(int serialNo)
        {
            return _IndexFile.GetIndexFileName(serialNo);
        }

        #region IndexFileInit Members

        public void ImportWordFilePositionList(List<IndexFile.WordFilePosition> wordFilePositionList)
        {
            PatchWordFilePositionTable(wordFilePositionList);
        }

        #endregion

    }

}
