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

using Hubble.Core.Data;
using Hubble.Core.Entity;

using Hubble.Framework.IO;
using Hubble.Framework.Threading;

namespace Hubble.Core.Store
{
    public class IndexFileProxy : /*MessageQueue,*/ IIndexFile
    {
        const int Timeout = 300000; //300s

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
            int _TotalDocs;
            int _MaxReturnCount = -1;
            private Data.DBProvider _DBProvider;
            //private int _TabIndex;

            public string Word
            {
                get
                {
                    return _Word;
                }
            }

            public int TotalDocs
            {
                get
                {
                    return _TotalDocs;
                }
            }

            public int MaxReturnCount
            {
                get
                {
                    return _MaxReturnCount;
                }
            }

            public Data.DBProvider DBProvider
            {
                get
                {
                    return _DBProvider;
                }
            }

            //public int TabIndex
            //{
            //    get
            //    {
            //        return _TabIndex;
            //    }
            //}

            public GetInfo(string word, int totalDocs, Data.DBProvider dbProvider, int maxReturnCount)
            {
                _Word = word;
                _TotalDocs = totalDocs;
                _DBProvider = dbProvider;
                _MaxReturnCount = maxReturnCount;
                //_TabIndex = tabIndex;
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
                internal IndexFile.FilePosition MergedFilePostion;

                internal string Word;

                //internal WordFilePositionList FilePostionList;

                internal MergeFilePosition(IndexFile.FilePosition filePostion, string word)
                {
                    MergedFilePostion = filePostion;
                    //FilePostionList = pList;
                    this.Word = word;
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
            class DDXFileEnum : IComparable<DDXFileEnum>
            {
                internal DDXFile DDXFile;

                internal DDXUnit Current;

                internal int Serial;

                public DDXFileEnum(DDXFile file, int serial)
                {
                    DDXFile = new DDXFile(file.FilePath, DDXFile.Mode.Enum);
                    Serial = serial;
                    Current = DDXFile.GetNext();
                }

                #region IComparable<DDXFileEnum> Members

                public int CompareTo(DDXFileEnum other)
                {
                    return this.Serial.CompareTo(other.Serial);
                }

                #endregion
            }

            private DDXFileEnum[] _DDXFileEnum = null; 

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

            List<IndexFile.IndexFileInfo> _IndexFileListForMerge;

            public List<IndexFile.IndexFileInfo> IndexFileListForMerge
            {
                get
                {
                    return _IndexFileListForMerge;
                }
            }

            List<DDXFile> _DDXFiles;


            //List<MergedWordFilePostionList> _MergedWordFilePostionList;

            public IEnumerable<MergedWordFilePostionList> MergedWordFilePostionList
            {
                get
                {
                    while (true)
                    {

                        string minWord = null;

                        if (_DDXFileEnum.Length <= 0)
                        {
                            yield break;
                        }

                        //Get Min Doc id
                        for (int i = 0; i < _DDXFileEnum.Length; i++)
                        {
                            if (minWord == null)
                            {
                                minWord = _DDXFileEnum[i].Current.Word;
                                continue;
                            }

                            if (Hubble.Framework.Text.UnicodeString.Comparer(_DDXFileEnum[i].Current.Word, minWord) < 0)
                            {
                                minWord = _DDXFileEnum[i].Current.Word;
                            }
                        }

                        MergedWordFilePostionList result = new MergedWordFilePostionList(minWord);
                        bool needShrink = false;

                        //Return min word file position list;
                        for (int i = 0; i < _DDXFileEnum.Length; i++)
                        {
                            if (minWord == _DDXFileEnum[i].Current.Word)
                            {
                                result.FilePositionList.Add(new IndexFile.FilePosition(_DDXFileEnum[i].Serial,
                                    _DDXFileEnum[i].Current.Position, (int)_DDXFileEnum[i].Current.Length));

                                _DDXFileEnum[i].Current = _DDXFileEnum[i].DDXFile.GetNext();

                                if (_DDXFileEnum[i].Current == null)
                                {
                                    needShrink = true;
                                }
                            }
                        }

                        //Shrink 
                        if (needShrink)
                        {
                            List<DDXFileEnum> ddxFileEnumList = new List<DDXFileEnum>();

                            for (int i = 0; i < _DDXFileEnum.Length; i++)
                            {
                                if (_DDXFileEnum[i].Current != null)
                                {
                                    ddxFileEnumList.Add(_DDXFileEnum[i]);
                                }
                            }

                            _DDXFileEnum = ddxFileEnumList.ToArray();
                        }

                        yield return result;
                    }
                }
            }

            int _Count;

            long _TotalFileLength;

            /// <summary>
            /// Total length of all ddx files that need be merged.
            /// </summary>
            public long TotalFileLength
            {
                get
                {
                    return _TotalFileLength;
                }
            }

            /// <summary>
            /// Get file length that has been finished merge.
            /// </summary>
            public long FileLengthFinished
            {
                get
                {
                    long length = 0;

                    foreach (DDXFile ddxFile in _DDXFiles)
                    {
                        length += ddxFile.CurrentFilePosition;
                    }

                    return length;
                }
            }


            public int Count
            {
                get
                {
                    return _Count;
                }
            }

            private void InitCount()
            {
                _Count = 0;

                List<DDXFileEnum> ddxFileEnumList = new List<DDXFileEnum>();
                _DDXFiles = new List<DDXFile>();

                foreach (IndexFile.IndexFileInfo ifi in _IndexFileListForMerge)
                {
                    _TotalFileLength += ifi.DDXFile.FileLength;

                    if (_Count < ifi.DDXFile.GetTotalWords())
                    {
                        _Count = ifi.DDXFile.GetTotalWords();
                    }

                    ddxFileEnumList.Add(new DDXFileEnum(ifi.DDXFile, ifi.Serial));
                    _DDXFiles.Add(ddxFileEnumList[ddxFileEnumList.Count - 1].DDXFile);

                }

                ddxFileEnumList.Sort();

                _DDXFileEnum = ddxFileEnumList.ToArray();

                _Count *= 2;
            }

            public MergeInfos(string headFileName, string indexFileName,
                List<IndexFile.IndexFileInfo> indexFileListForMerge, int begin, int end, int mergedSerial)
            {
                _MergeHeadFileName = headFileName;
                _MergeIndexFileName = indexFileName;
                _IndexFileListForMerge = indexFileListForMerge;
                //_MergedWordFilePostionList = list;
                _BeginSerial = begin;
                _EndSerial = end;
                _MergedSerial = mergedSerial;

                InitCount();
            }

            public void Close()
            {
                foreach (DDXFile file in _DDXFiles)
                {
                    file.Close();
                }
            }
        }

        public class MergedWordFilePostionList
        {
            private string _Word;

            public string Word
            {
                get
                {
                    return _Word;
                }
            }

            private WordFilePositionList _FilePositionList;

            internal WordFilePositionList FilePositionList
            {
                get
                {
                    return _FilePositionList;
                }
            }

            //private WordFilePositionList _OrginalFilePositionList;

            //internal WordFilePositionList OrginalFilePositionList
            //{
            //    get
            //    {
            //        return _OrginalFilePositionList;
            //    }
            //}

            internal MergedWordFilePostionList(string word)
            {
                _Word = word;
                //_OrginalFilePositionList = orginal;
                _FilePositionList = new WordFilePositionList();
            }

        }


        private object _MergeLockObj = new object();

        private bool _CanMerge = true;

        private IndexFile _IndexFile;

        private int _WordCount = 0;

        private bool _NeedClose = false;

        private bool _CanClose = true;

        private object _LockObj = new object();

        private object _MergeProgressLock = new object();
        private double _MergeProgress = -1;

        private Hubble.Core.Data.Field.IndexMode _IndexMode;

        private DBProvider _DBProvider;

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

        //private Index.DelegateWordUpdate _WordUpdateDelegate;

        #region Public properties

        //public Index.DelegateWordUpdate WordUpdateDelegate
        //{
        //    get
        //    {
        //        return _WordUpdateDelegate;
        //    }

        //    set
        //    {
        //        _WordUpdateDelegate = value;
        //    }
        //}

        /// <summary>
        /// get or set the progress of Merge
        /// </summary>
        internal double MergeProgress
        {
            get
            {
                lock (_MergeProgressLock)
                {
                    return _MergeProgress;
                }
            }

            set
            {
                lock (_MergeProgressLock)
                {
                    _MergeProgress = value;
                }
            }
        }
             

        public int WordTableSize
        {
            get
            {
                return InnerWordTableSize;
            }
        }

        internal bool CanMerge
        {
            get
            {
                lock (_MergeLockObj)
                {
                    return _CanMerge;
                }
            }

            set
            {
                lock (_MergeLockObj)
                {
                    _CanMerge = value ;
                }
            }
        }

        internal bool CanClose
        {
            get
            {
                HBMonitor.Enter(_LockObj);

                try
                {
                    return _CanClose;
                }
                finally
                {
                    HBMonitor.Exit(_LockObj);
                }

            }
        }

        internal string LastDDXFilePath
        {
            get
            {
                return _IndexFile.LastDDXFilePath;
            }
        }

        //internal string LastHeadFilePath
        //{
        //    get
        //    {
        //        return _IndexFile.LastHeadFilePath;
        //    }
        //}

        internal string LastIndexFilePath
        {
            get
            {
                return _IndexFile.LastIndexFilePath;
            }
        }

        #endregion


        private WordFilePositionList GetFilePositionListByWord(string word)
        {
            List<IndexFile.FilePosition> fpList = _IndexFile.GetFilePositionListByWord(word);

            if (fpList.Count <= 0)
            {
                return null;
            }

            //foreach (IndexFile.FilePosition fp in fpList)
            //{
            //    pList.AddOnly(fp);
            //}


            return new WordFilePositionList(fpList);

            //if (_WordFilePositionTable.TryGetValue(word, out pList))
            //{
            //    return pList;
            //}
            //else
            //{
            //    return null;
            //}
        }

        //private void PatchWordFilePositionTable(List<IndexFile.WordFilePosition> wordFilePostionList)
        //{
        //    foreach (IndexFile.WordFilePosition p in wordFilePostionList)
        //    {
        //        WordFilePositionList pList;

        //        if (_WordFilePositionTable.TryGetValue(p.Word, out pList))
        //        {
        //            pList.Add(p.Position);
        //        }
        //        else
        //        {
        //            pList = new WordFilePositionList();
        //            pList.AddOnly(p.Position);

        //            string internedWord = string.IsInterned(p.Word);

        //            if (internedWord == null)
        //            {
        //                internedWord = p.Word;
        //            }

        //            _WordFilePositionTable.Add(internedWord, pList);
        //        }
        //    }

        //    InnerWordTableSize = _WordFilePositionTable.Count;

        //    GC.Collect();
        //    GC.Collect();
        //    GC.Collect();

        //}

        internal const int MaxIndexFilesNeedMerge = 64;
        internal const int MinIndexFilesNeedMerge = 32;
        const int MergeThreshold = 10;

        /// <summary>
        /// Get the serial number range for merge
        /// </summary>
        /// <param name="option">Merge option</param>
        /// <param name="serial">Merge to this serial. This is dest serial number.</param>
        /// <param name="begin">begin serial number need be merged</param>
        /// <param name="end">end serial number need be merged</param>
        /// <returns>If don't need be merged, return false</returns>
        private bool GetMergeRange(OptimizationOption option,
            out int serial, out int begin, out int end)
        {
            serial = 0;
            begin = 0;
            end = 0;

            if (_IndexFile.IndexFileList.Count <= 2)
            {
                if (_IndexFile.IndexFileList.Count <= 1)
                {
                    //If only one index file, don't need be merged.
                    return false;
                }
                else if (option != OptimizationOption.Minimum)
                {
                    //If only two index files, and it is not Minimum option. don't need be merged.
                    return false;
                }
            }

            long lastFileSize = long.MaxValue;

            int i;

            for (i = _IndexFile.IndexFileList.Count - 1; i >= 0; i--)
            {
                if (lastFileSize == long.MaxValue)
                {
                    //last index file
                    lastFileSize = _IndexFile.IndexFileList[i].Size;
                    continue;
                }

                if (lastFileSize * MergeThreshold < _IndexFile.IndexFileList[i].Size)
                {
                    if (i < MinIndexFilesNeedMerge)
                    {
                        break;
                    }
                }

                lastFileSize = _IndexFile.IndexFileList[i].Size;
            }

            if (i < 0)
            {
                if (_IndexFile.IndexFileList.Count < MaxIndexFilesNeedMerge && option == OptimizationOption.Speedy)
                {
                    return false;
                }
                else
                {
                    begin = _IndexFile.IndexFileList[0].Serial;
                    end = _IndexFile.IndexFileList[_IndexFile.IndexFileList.Count - 1].Serial;
                    serial = 0;
                }
            }
            else
            {
                switch (option)
                {
                    case OptimizationOption.Speedy:
                        {
                            if (_IndexFile.IndexFileList.Count < MaxIndexFilesNeedMerge)
                            {
                                return false;
                            }

                            i++;

                            if (i >= MinIndexFilesNeedMerge)
                            {
                                lastFileSize = long.MinValue;

                                int j;
                                for (j = 0; j < i ; j++)
                                {
                                    if (j == 0)
                                    {
                                        //firs index file
                                        lastFileSize = _IndexFile.IndexFileList[j].Size;
                                        continue;
                                    }

                                    if (lastFileSize > _IndexFile.IndexFileList[j].Size * MergeThreshold)
                                    {
                                        break;
                                    }

                                    lastFileSize = _IndexFile.IndexFileList[j].Size;
                                }

                                if (j >= i - 1)
                                {
                                    j = 0;
                                }

                                begin = _IndexFile.IndexFileList[j].Serial;
                                end = _IndexFile.IndexFileList[_IndexFile.IndexFileList.Count - 1].Serial;
                                serial = begin;

                                if (j > 0)
                                {
                                    serial = _IndexFile.IndexFileList[j-1].Serial + 1;
                                }
                            }
                            else
                            {
                                begin = _IndexFile.IndexFileList[i].Serial;
                                end = _IndexFile.IndexFileList[_IndexFile.IndexFileList.Count - 1].Serial;
                                serial = begin;

                                if (i > 0)
                                {
                                    serial = _IndexFile.IndexFileList[i - 1].Serial + 1;
                                }
                            }

                            if (serial > 0)
                            {
                                if (end - begin < MaxIndexFilesNeedMerge - 1)
                                {
                                    return false;
                                }
                            }
                        }
                        break;
                    case OptimizationOption.Middle:
                        {
                            long sizeSumAfterFirst = 0;

                            for (int j = 1; j < _IndexFile.IndexFileList.Count; j++)
                            {
                                sizeSumAfterFirst += _IndexFile.IndexFileList[j].Size;
                            }

                            if (_IndexFile.IndexFileList[0].Size > sizeSumAfterFirst)
                            {
                                //Fisrt index large than the others

                                begin = _IndexFile.IndexFileList[1].Serial;
                                end = _IndexFile.IndexFileList[_IndexFile.IndexFileList.Count - 1].Serial;
                                //serial = begin;
                                serial = _IndexFile.IndexFileList[0].Serial + 1;
                            }
                            else
                            {
                                begin = _IndexFile.IndexFileList[0].Serial;
                                end = _IndexFile.IndexFileList[_IndexFile.IndexFileList.Count - 1].Serial;
                                serial = 0;
                            }
                        }
                        break;
                    case OptimizationOption.Minimum:
                        {
                            begin = _IndexFile.IndexFileList[0].Serial;
                            end = _IndexFile.IndexFileList[_IndexFile.IndexFileList.Count - 1].Serial;
                            serial = 0;
                        }
                        break;
                    default:
                        return false;
                }


            }


            return true;
        }



        private object ProcessGetFilePositionList(int evt, MessageQueue.MessageFlag flag, object data)
        {
            OptimizationOption option = (OptimizationOption)data;

            int begin;
            int end;
            int serial;

            if (!GetMergeRange(option, out serial, out begin, out end))
            {
                return null;
            }

            List<IndexFile.IndexFileInfo> indexFileInfoForMerge = new List<IndexFile.IndexFileInfo>();

            foreach (IndexFile.IndexFileInfo ifi in _IndexFile.IndexFileList)
            {
                if (ifi.Serial >= begin && ifi.Serial <= end)
                {
                    indexFileInfoForMerge.Add(ifi);
                }
            }

            return new MergeInfos(_IndexFile.GetDDXFileName(serial),
                _IndexFile.GetIndexFileName(serial), indexFileInfoForMerge, begin, end, serial);
        }


        private object ProcessGetFilePositionList08(int evt, MessageQueue.MessageFlag flag, object data)
        {
            OptimizationOption option = (OptimizationOption)data;

            //List<MergedWordFilePostionList> result = new List<MergedWordFilePostionList>();

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

            //foreach (string word in _WordFilePositionTable.Keys)
            //{
            //    WordFilePositionList pList = _WordFilePositionTable[word];
            //    MergedWordFilePostionList wfpl = new MergedWordFilePostionList(word);

            //    foreach (IndexFile.FilePosition fp in pList.Values)
            //    {
            //        if (fp.Serial >= begin && fp.Serial <= end)
            //        {
            //            wfpl.FilePositionList.AddOnly(new IndexFile.FilePosition(fp.Serial, fp.Position, fp.Length));
            //        }
            //    }

            //    result.Add(wfpl);
            //}

            int serial;

            if (begin == _IndexFile.IndexFileList[0].Serial)
            {
                serial = 0;
            }
            else
            {
                serial = 1;
            }

            List<IndexFile.IndexFileInfo> indexFileInfoForMerge = new List<IndexFile.IndexFileInfo>();

            foreach (IndexFile.IndexFileInfo ifi in _IndexFile.IndexFileList)
            {
                if (ifi.Serial >= begin && ifi.Serial <= end)
                {
                    indexFileInfoForMerge.Add(ifi);
                }
            }

            return new MergeInfos(_IndexFile.GetDDXFileName(serial),
                _IndexFile.GetIndexFileName(serial), indexFileInfoForMerge, begin, end, serial);
        }

        private bool ProcessMergeAck(int evt, MessageQueue.MessageFlag flag, object data)
        {
            if (!_DBProvider.MergeLock.Enter(Lock.Mode.Mutex, 100))
            {
                return false;
            }

            try
            {
                MergeAck mergeAck = (MergeAck)data;

                int begin = mergeAck.BeginSerial;
                int end = mergeAck.EndSerial;
                int time = 0;

                for (int serial = begin; serial <= end; serial++)
                {
                    string fileName;

                    fileName = _IndexFile.IndexDir + GetDDXFileName(serial);

                    //Close DDX file of this serial
                    _IndexFile.CloseSerial(serial);

                    //Delete DDX File 
                    while (true)
                    {
                        try
                        {
                            if (System.IO.File.Exists(fileName))
                            {
                                System.IO.File.Delete(fileName);
                            }

                            break;
                        }
                        catch
                        {
                            if (time < 40)
                            {
                                System.Threading.Thread.Sleep(20);
                                time++;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }


                    fileName = _IndexFile.IndexDir + GetIndexFileName(serial);
                    time = 0;

                    while (true)
                    {
                        try
                        {
                            if (System.IO.File.Exists(fileName))
                            {
                                System.IO.File.Delete(fileName);
                            }

                            break;
                        }
                        catch
                        {
                            if (time < 40)
                            {
                                System.Threading.Thread.Sleep(20);
                                time++;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }

                System.Threading.Thread.Sleep(20);

                try
                {
                    System.IO.File.Move(_IndexFile.IndexDir + @"Optimize\" + mergeAck.MergeHeadFileName,
                        _IndexFile.IndexDir + mergeAck.MergeHeadFileName);
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog(string.Format("ProcessMergeAck begin = {0} end = {1} dest file name:{2}",
                        begin, end, _IndexFile.IndexDir + mergeAck.MergeHeadFileName),
                        e);
                    throw e;
                }

                try
                {
                    System.IO.File.Move(_IndexFile.IndexDir + @"Optimize\" + mergeAck.MergeIndexFileName,
                        _IndexFile.IndexDir + mergeAck.MergeIndexFileName);
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog(string.Format("ProcessMergeAck begin = {0} end = {1} dest file name:{2}",
                        begin, end, _IndexFile.IndexDir + mergeAck.MergeIndexFileName),
                        e);
                    throw e;
                }

                //foreach (MergeAck.MergeFilePosition mfp in mergeAck.MergeFilePositionList)
                //{
                //    //WordFilePositionList pList = mfp.FilePostionList;

                //    WordFilePositionList pList = _WordFilePositionTable[mfp.Word];

                //    if (pList == null)
                //    {
                //        continue;
                //    }

                //    int i = 0;
                //    bool fst = true;

                //    while (i < pList.Count)
                //    {
                //        if (pList[i].Serial >= begin && pList[i].Serial <= end)
                //        {
                //            if (fst)
                //            {
                //                pList[i] = mfp.MergedFilePostion;
                //                fst = false;
                //                i++;
                //            }
                //            else
                //            {
                //                pList.RemoveAt(i);
                //            }
                //        }
                //        else
                //        {
                //            i++;
                //        }
                //    }

                //    _WordFilePositionTable.Reset(pList.Word, pList.FPList);
                //}

                _IndexFile.AfterMerge(begin, end, mergeAck.MergedSerial);
            }
            finally
            {
                _DBProvider.MergeLock.Leave(Lock.Mode.Mutex);
            }

            return true;
        }

        //private object ProcessMessage(int evt, MessageQueue.MessageFlag flag, object data)
        //{
        //    try
        //    {
        //        switch ((Event)evt)
        //        {
        //            case Event.Add:
        //                WordDocList wl = (WordDocList)data;
        //                //_IndexFile.AddWordAndDocList(wl.Word, wl.DocList);
        //                //if (WordUpdateDelegate != null)
        //                //{
        //                //    WordUpdateDelegate(wl.Word, wl.DocList);
        //                //}

        //                break;
        //            case Event.Collect:

        //                lock (_LockObj)
        //                {
        //                    if (_NeedClose)
        //                    {
        //                        break;
        //                    }

        //                    _CanClose = false;
        //                }

        //                _IndexFile.Collect();

        //                //PatchWordFilePositionTable(_IndexFile.WordFilePositionList);
        //                _IndexFile.ClearWordFilePositionList();

        //                lock (_LockObj)
        //                {
        //                    _CanClose = true;
        //                }

        //                break;
        //            case Event.Get:
        //                {
        //                    GetInfo getInfo = data as GetInfo;
        //                    WordFilePositionList pList = GetFilePositionListByWord(getInfo.Word);
        //                    return _IndexFile.GetWordIndex(getInfo.Word, pList, getInfo.TotalDocs,
        //                        getInfo.DBProvider, getInfo.MaxReturnCount);
        //                }
        //            case Event.GetFilePositionList:
        //                return ProcessGetFilePositionList(evt, flag, data);

        //            case Event.MergeAck:
        //                ProcessMergeAck(evt, flag, data);
        //                break;

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Global.Report.WriteErrorLog(string.Format("Index File Proxy Fail! Event={0}", ((Event)evt).ToString()), e);

        //        throw e;
        //    }

        //    return null;
        //}


        public IndexFileProxy(string path, string fieldName, Hubble.Core.Data.Field.IndexMode indexMode, 
            DBProvider dbProvider)
            : this(path, fieldName, false, indexMode, dbProvider)
        {

        }

        public IndexFileProxy(string path, string fieldName, 
            bool rebuild, Hubble.Core.Data.Field.IndexMode indexMode,
            DBProvider dbProvider)
            : base()
        {
            _IndexMode = indexMode;
            //OnMessageEvent = ProcessMessage;
            _IndexFile = new IndexFile(path, this, this);
            _IndexFile.Create(fieldName, rebuild, indexMode);

            _DBProvider = dbProvider;

            //this.Start();
        }

        //public void AddDocInfos(List<IndexFile.DocInfo> docInfos)
        //{
        //}

        internal void SetRamIndex(Hubble.Framework.IO.CachedFileStream.CachedType type, int minLoadSize)
        {
            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                if (_NeedClose)
                {
                    return;
                }

                throw new TimeoutException();
            }

            try
            {
                _IndexFile.SetRamIndex(type, minLoadSize);
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }
        }

        public MergeInfos GetMergeInfos(Data.OptimizationOption option)
        {
            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                if (_NeedClose)
                {
                    return null;
                }

                throw new TimeoutException();
            }

            try
            {
                return (MergeInfos)ProcessGetFilePositionList((int)Event.GetFilePositionList, MessageQueue.MessageFlag.None, option);
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }



            //return SSendMessage((int)Event.GetFilePositionList,
            //    option, 30 * 1000) as MergeInfos;
        }

        public void DoMergeAck(MergeAck mergeAck)
        {
MergeAckLoop:
            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                if (_NeedClose)
                {
                    return;
                }

                throw new TimeoutException();
            }

            if (!this.CanMerge) //if can't merge now, waitting for CanMerge
            {
                HBMonitor.Exit(_LockObj);

                System.Threading.Thread.Sleep(10);
                goto MergeAckLoop;
            }

            bool gotoMergeAckLoop = false;

            try
            {
                if (!ProcessMergeAck((int)Event.MergeAck, MessageQueue.MessageFlag.None, mergeAck))
                {
                    HBMonitor.Exit(_LockObj);

                    System.Threading.Thread.Sleep(10);
                    gotoMergeAckLoop = true;

                    goto MergeAckLoop;
                }
            }
            finally
            {
                if (!gotoMergeAckLoop)
                {
                    HBMonitor.Exit(_LockObj);
                }
            }

            //SSendMessage((int)Event.MergeAck, mergeAck, 300 * 1000); //time out 5 min
        }

        public void AddWordPositionAndDocumentPositionList(string word, DocumentPositionList first, int docsCount,
            IEnumerable<Entity.DocumentPositionList> docList)
        {
            DateTime startTime = DateTime.Now;

            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                if (_NeedClose)
                {
                    return;
                }

                TimeSpan timeSpan = DateTime.Now - startTime;

                Global.Report.WriteAppLog(string.Format("AddWordPositionAndDocumentPositionList timeout. Span={0}ms Timeout={1}ms",
                    timeSpan.TotalMilliseconds, Timeout), true);

                throw new TimeoutException();
            }

            try
            {
                 _IndexFile.AddWordAndDocList(word, first, docsCount, docList);
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }

            //ASendMessage((int)Event.Add, new WordDocList(word, docList));
        }

        public BufferMemory GetIndexBufferMemory(int serial, long position, long length)
        {
            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                if (_NeedClose)
                {
                    return null;
                }

                throw new TimeoutException();
            }

            try
            {
                return _IndexFile.GetIndexBufferMemory(serial, position, length);
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }
        }

        public System.IO.MemoryStream GetIndexBuf(int serial, long position, long length)
        {
            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                if (_NeedClose)
                {
                    return null;
                }

                throw new TimeoutException();
            }

            try
            {
                return _IndexFile.GetIndexBuf(serial, position, length);
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }
        }

        public Hubble.Core.Index.WordIndexReader GetWordIndex(GetInfo getInfo)
        {
            return GetWordIndex(getInfo, false);
        }

        /// <summary>
        /// Get word index reader
        /// </summary>
        /// <param name="getInfo">get info</param>
        /// <param name="onlyStepDocIndex">only return step doc index</param>
        /// <returns>WordIndexReader</returns>
        public Hubble.Core.Index.WordIndexReader GetWordIndex(GetInfo getInfo, bool onlyStepDocIndex)
        {
            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                if (_NeedClose)
                {
                    return null;
                }

                throw new TimeoutException();
            }

            try
            {
                WordFilePositionList pList = GetFilePositionListByWord(getInfo.Word);

                if (pList == null)
                {
                    return null;
                }

                if (onlyStepDocIndex)
                {
                    return _IndexFile.GetWordIndexWithWordStepDocIndex(getInfo.Word, pList, getInfo.TotalDocs,
                        getInfo.DBProvider, getInfo.MaxReturnCount);
                }
                else
                {
                    return _IndexFile.GetWordIndex(getInfo.Word, pList, getInfo.TotalDocs,
                        getInfo.DBProvider, getInfo.MaxReturnCount);
                }
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }

            //return SSendMessage((int)Event.Get, getInfo, 30 * 1000) as
            //    Hubble.Core.Index.WordIndexReader;

            //lock (_LockObj)
            //{
            //    WordFilePositionList pList = GetFilePositionListByWord(getInfo.Word);
            //    return _IndexFile.GetWordIndex(getInfo.Word, pList, getInfo.TotalDocs,
            //        getInfo.DBProvider, getInfo.TabIndex);

            //}
        }

        /// <summary>
        /// Too many index files.
        /// Need to pause index process and optimize the index.
        /// </summary>
        /// <returns></returns>
        public bool TooManyIndexFiles()
        {
            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                return false;
            }

            try
            {
                return _IndexFile.IndexFileList.Count > MaxIndexFilesNeedMerge + 2 * MinIndexFilesNeedMerge;
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }

        }
        public void Collect()
        {
            if (!HBMonitor.TryEnter(_LockObj, Timeout))
            {
                if (_NeedClose)
                {
                    return;
                }

                throw new TimeoutException();
            }

            try
            {
                _IndexFile.Collect();

                //PatchWordFilePositionTable(_IndexFile.WordFilePositionList);
                _IndexFile.ClearWordFilePositionList();
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }

            //ASendMessage((int)Event.Collect, null);
        }

        internal void SafelyClose()
        {
            HBMonitor.Enter(_LockObj);

            _NeedClose = true;

            HBMonitor.Exit(_LockObj);
        }

               
        public List<string> InnerLike(string str, InnerLikeType type)
        {
            HBMonitor.Enter(_LockObj);
            try
            {
                //return _WordFilePositionTable.InnerLike(str, type);
                return new List<string>();
            }
            finally
            {
                HBMonitor.Exit(_LockObj);
            }
        }


        internal void Close(int millisecondsTimeout)
        {
            //base.Close(millisecondsTimeout);

            //_WordFilePositionTable.Clear();

            if (_IndexFile != null)
            {
                _IndexFile.Close();
                _IndexFile = null;
                GC.Collect();
            }
        }

        public string GetDDXFileName(int serialNo)
        {
            return _IndexFile.GetDDXFileName(serialNo);
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
            //PatchWordFilePositionTable(wordFilePositionList);
        }

        public void CollectWordFilePositionList()
        {
            //_WordFilePositionTable.Collect();
        }

        #endregion

    }

}
