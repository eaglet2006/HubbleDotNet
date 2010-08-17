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
using Hubble.Framework.Text;

namespace Hubble.Core.Store
{
    public class DDXUnit
    {
        public string Word;
        public long Position;
        public long Length;

        public DDXUnit(string word, long position, long length)
        {
            this.Word = word;
            this.Position = position;
            this.Length = length;
        }

    }

    /// <summary>
    /// this class for ddx file
    /// The ddx file stores the dictionary for inverted index.
    /// </summary>
    public class DDXFile : IDisposable
    {
        public enum Mode
        {
            Read = 0,
            Enum = 1,
            Write = 2,
        }

        const int SegmentSize = 8192;
        const int UnitBlockSize = 256;
        const int MaxWordLength = 108;
        string _FilePath;
        FileStream _File;
        Mode _Mode = Mode.Read;

        public string FilePath
        {
            get
            {
                return _FilePath;
            }
        }

        public long FileLength
        {
            get
            {
                if (_File != null)
                {
                    return _File.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        public long CurrentFilePosition
        {
            get
            {
                if (_File != null)
                {
                    return _File.Position;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">DDX file path</param>
        /// <param name="write">if true for write else for read</param>
        public DDXFile(string filePath, Mode mode)
        {
            _Mode = mode;
            _FilePath = filePath;

            if (_Mode == Mode.Write)
            {
                _TempMem = new MemoryStream(256);
                _UnitBlock = new byte[UnitBlockSize];
                _Segment = new byte[SegmentSize]; //One segment include 32 unit blocks .
                _File = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            }
            else if (_Mode == Mode.Enum)
            {
                _File = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else if (_Mode == Mode.Read)
            {
                _File = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                GetSegmentStartStrings();
            }
        }

        ~DDXFile()
        {
            if (_File != null)
            {
                Dispose();
            }
        }

        public void Close()
        {
            if (_File == null)
            {
                return;
            }

            if (_Mode == Mode.Write)
            {
                if (_UnitBlock[0] != 0)
                {
                    Array.Copy(_UnitBlock, 0, _Segment, _CurrentUnitIndex * UnitBlockSize, _UnitBlock.Length);
                }

                if (_Segment[0] != 0)
                {
                    _File.Write(_Segment, 0, _Segment.Length);
                }

                _File.Flush();
            }

            _File.Close();
            _File = null;
        }

        #region Read

        #region Fields for read

        string[] _SegmentStartStrings = null;
        object _ReadLockObj = new object();
        byte[] _DDXFileBuffer = null;

        #endregion

        private void GetSegmentStartStrings()
        {
            List<string> strList = new List<string>(256);

            _File.Seek(0, SeekOrigin.Begin);
            byte[] segment = new byte[SegmentSize];

            if (Hubble.Framework.IO.File.ReadToBuffer(_File, segment))
            {
                do
                {
                    string word;
                    long position;
                    long length;
                    
                    if (UnicodeString.Decode(segment, 0, out word, "", out position, out length) >= 0)
                    {
                        strList.Add(word);
                    }

                } while (Hubble.Framework.IO.File.ReadToBuffer(_File, segment));
            }

            _SegmentStartStrings = strList.ToArray();
        }

        private int GetSegmentIndex(string value)
        {
            string[] objArray = _SegmentStartStrings;

            if (objArray.Length <= 0)
            {
                return -1;
            }

            int lo = 0;
            int hi = objArray.Length - 1;

            while (lo <= hi)
            {
                // i might overflow if lo and hi are both large positive numbers.
                int i = lo + ((hi - lo) >> 1);

                int c = UnicodeString.Comparer(objArray[i], value);

                if (c == 0) return i;

                if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            lo--;
            if (lo >= objArray.Length)
            {
                lo = objArray.Length - 1;
            }
            else if (lo < 0)
            {
                lo = 0;
            }

            if (UnicodeString.Comparer(objArray[lo], value) <= 0)
            {
                return lo;
            }
            else
            {
                return -1;
            }

        }

        private int GetUnitIndexTour(string value, int segmentStart, byte[] buffer)
        {
            string word;
            long position;
            long length;

            for (int i = 0; i < SegmentSize / UnitBlockSize; i++)
            {
                if (UnicodeString.Decode(buffer, segmentStart + i * UnitBlockSize, 0, out word, "",
                    out position, out length) < 0)
                {
                    return i - 1;
                }

                int c = UnicodeString.Comparer(word, value);

                if (c > 0)
                {
                    return i - 1;
                }
                else if (c == 0)
                {
                    return i;
                }
            }

            return SegmentSize / UnitBlockSize - 1;
        }

        private int GetUnitIndexBin(string value, int segmentStart, byte[] buffer)
        {
            byte[] objArray = buffer;

            if (objArray.Length <= 0)
            {
                return -1;
            }

            int lo = 0;
            int hi = (SegmentSize / UnitBlockSize) - 1;

            string word;
            long position;
            long length;

            while (lo <= hi)
            {
                // i might overflow if lo and hi are both large positive numbers.
                int i = lo + ((hi - lo) >> 1);

                if (UnicodeString.Decode(buffer, segmentStart + i * UnitBlockSize, 0, out word, "",
                    out position, out length) < 0)
                {
                    throw new StoreException(string.Format("GetUnitIndexBin fail, Word:{0} SegmentStart:{1} index={2}",
                        word, segmentStart, i));
                }

                int c = UnicodeString.Comparer(word, value);

                if (c == 0) return i;

                if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            lo--;
            if (lo >= objArray.Length)
            {
                lo = objArray.Length - 1;
            }
            else if (lo < 0)
            {
                lo = 0;
            }

            if (UnicodeString.Decode(buffer, segmentStart + lo * UnitBlockSize, 0, out word, "",
                out position, out length) < 0)
            {
                throw new StoreException(string.Format("GetUnitIndexBin fail, Word:{0} SegmentStart:{1} index={2}",
                    word, segmentStart, lo));
            }

            if (UnicodeString.Comparer(word, value) <= 0)
            {
                return lo;
            }
            else
            {
                return -1;
            }

        }

        public void LoadDDXToMemory()
        {
            lock (_ReadLockObj)
            {
                if (_DDXFileBuffer == null)
                {
                    _DDXFileBuffer = new byte[_File.Length];
                    _File.Seek(0, SeekOrigin.Begin);
                    if (!Hubble.Framework.IO.File.ReadToBuffer(_File, _DDXFileBuffer))
                    {
                        _DDXFileBuffer = null;
                        throw new StoreException("Can't load DDX file");
                    }

                    _File.Close();
                    _File = null;
                }

            }
        }

        public void UnLoadDDXFromMemory()
        {
            lock (_ReadLockObj)
            {
                if (_DDXFileBuffer != null)
                {
                    _DDXFileBuffer = null;
                    _File = new FileStream(_FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }

            }
        }

        /// <summary>
        /// Get total words count in this file.
        /// It is a approximate number
        /// </summary>
        public int GetTotalWords()
        {
            return _SegmentStartStrings.Length * 1024;
        }

        public DDXUnit Find(string word)
        {
            lock (_ReadLockObj)
            {
                int segmentIndex = GetSegmentIndex(word);

                if (segmentIndex < 0)
                {
                    return null;
                }
                else
                {
                    if (_DDXFileBuffer == null)
                    {
                        if (segmentIndex * SegmentSize >= _File.Length)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (segmentIndex * SegmentSize >= _DDXFileBuffer.Length)
                        {
                            return null;
                        }
                    }

                    byte[] segmentBuffer;

                    int segmentStart;

                    if (_DDXFileBuffer == null)
                    {
                        _File.Seek(segmentIndex * SegmentSize, SeekOrigin.Begin);
                        segmentBuffer = new byte[SegmentSize];  //One segment include 32 unit blocks .
                        segmentStart = 0;
                    }
                    else
                    {
                        segmentBuffer = _DDXFileBuffer;
                        segmentStart = segmentIndex * SegmentSize;
                    }

                    if (_DDXFileBuffer == null)
                    {
                        if (!Hubble.Framework.IO.File.ReadToBuffer(_File, segmentBuffer))
                        {
                            throw new StoreException(string.Format("DDX Find fail, Word:{0} FilePostion = {1}",
                                word, _File.Position));
                        }
                    }

                    int unitIndex;

                    if (segmentIndex == _SegmentStartStrings.Length - 1)
                    {
                        //Last segment
                        unitIndex = GetUnitIndexTour(word, segmentStart, segmentBuffer);
                    }
                    else
                    {
                        unitIndex = GetUnitIndexBin(word, segmentStart, segmentBuffer);
                    }

                    if (unitIndex < 0)
                    {
                        return null;
                    }

                    string enumWord;
                    string preWord = "";
                    long position;
                    long length;

                    int positionInUnit = 0;

                    positionInUnit = UnicodeString.Decode(segmentBuffer, segmentStart + unitIndex * UnitBlockSize, positionInUnit, 
                        out enumWord, preWord, out position, out length);

                    preWord = enumWord;

                    if (positionInUnit < 0)
                    {
                        return null;
                    }

                    int c = UnicodeString.Comparer(enumWord, word);

                    while (c < 0)
                    {
                        positionInUnit = UnicodeString.Decode(segmentBuffer, segmentStart + unitIndex * UnitBlockSize, positionInUnit,
                            out enumWord, preWord, out position, out length);
                        
                        preWord = enumWord;

                        if (positionInUnit < 0)
                        {
                            return null;
                        }

                        c = UnicodeString.Comparer(enumWord, word);
                    }

                    if (c == 0)
                    {
                        return new DDXUnit(word, position, length);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        #endregion


        #region Write

        #region Fields for write
        byte[] _UnitBlock = null;
        byte[] _Segment = null; //One segment include 32 unit blocks .
        MemoryStream _TempMem = null;
        int _CurrentUnitIndex = 0;
        int _CurrentPositionInUnitBlock = 0;
        string _PreWord = "";
        #endregion

        /// <summary>
        /// Add new word in DDX file.
        /// The word inputed must be sorted.
        /// </summary>
        /// <param name="word">word</param>
        /// <param name="position">position in .idx file</param>
        /// <param name="length">length in .idx file</param>
        public bool Add(string word, long position, long length)
        {
            if (_Mode != Mode.Write)
            {
                throw new System.IO.IOException(string.Format("DDX file: {0} is read only mode. Can't be written!", 
                    _FilePath));
            }

            if (word.Length > MaxWordLength)
            {
                return false;
            }

            try
            {
                _CurrentPositionInUnitBlock = UnicodeString.Encode(_TempMem, _UnitBlock,
                    _CurrentPositionInUnitBlock, word, _PreWord, position, length);
            }
            catch (Exception e)
            {
                Global.Report.WriteErrorLog("Unicode string encode error", e);
                return false;
            }

            if (_CurrentPositionInUnitBlock < 0)
            {
                //UnitBlock overflow

                Array.Copy(_UnitBlock, 0, _Segment, _CurrentUnitIndex * UnitBlockSize, _UnitBlock.Length);

                _CurrentUnitIndex++;
                _CurrentPositionInUnitBlock = 0;
                Array.Clear(_UnitBlock, 0, _UnitBlock.Length);
                _PreWord = "";

                if (_CurrentUnitIndex * UnitBlockSize >= _Segment.Length)
                {
                    //_Segment overflow
                    _File.Write(_Segment, 0, _Segment.Length);
                    _CurrentUnitIndex = 0;
                    Array.Clear(_Segment, 0, _Segment.Length);
                }

                _CurrentPositionInUnitBlock = UnicodeString.Encode(_TempMem, _UnitBlock,
                    _CurrentPositionInUnitBlock, word, _PreWord, position, length);
                return true;
            }

            _PreWord = word;

            return true;

        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch
            {
            }
        }

        #endregion

        #region Enum

        #region Enumerator Fields
        byte[] _SegmentEnum = null; //One segment include 32 unit blocks .
        int _CurrentUnitIndexEnum = 0;
        int _CurrentPositionInUnitBlockEnum = 0;
        string _PreWordEnum = "";
        #endregion


        private void EnumReset()
        {
            _SegmentEnum = new byte[SegmentSize];  //One segment include 32 unit blocks .

            Hubble.Framework.IO.File.ReadToBuffer(_File, _SegmentEnum);

            _CurrentUnitIndexEnum = 0;
            _CurrentPositionInUnitBlockEnum = 0;
            _PreWordEnum = "";
        }

        public DDXUnit GetNext()
        {
            if (_Mode != Mode.Enum)
            {
                throw new System.IO.IOException(string.Format("DDX file: {0} is not enum mode. Can't be read!", _FilePath));
            }

            if (_SegmentEnum == null)
            {
                EnumReset();
            }

            string word;
            long position;
            long length;

            if (_CurrentPositionInUnitBlockEnum < 0)
            {
                return null;
            }

            _CurrentPositionInUnitBlockEnum = UnicodeString.Decode(_SegmentEnum, _CurrentUnitIndexEnum * UnitBlockSize,
                _CurrentPositionInUnitBlockEnum,
                out word, _PreWordEnum, out position, out length);

            if (_CurrentPositionInUnitBlockEnum < 0)
            {
                _CurrentPositionInUnitBlockEnum = 0;
                _CurrentUnitIndexEnum++;

                if (_CurrentUnitIndexEnum * UnitBlockSize >= _SegmentEnum.Length)
                {
                    if (!Hubble.Framework.IO.File.ReadToBuffer(_File, _SegmentEnum))
                    {
                        return null;
                    }
                    else
                    {
                        _CurrentUnitIndexEnum = 0;
                    }
                }

                _CurrentPositionInUnitBlockEnum = UnicodeString.Decode(_SegmentEnum, _CurrentUnitIndexEnum * UnitBlockSize,
                    _CurrentPositionInUnitBlockEnum,
                    out word, "", out position, out length);

                if (_CurrentPositionInUnitBlockEnum < 0)
                {
                    return null;
                }
            }

            _PreWordEnum = word;
            return new DDXUnit(word, position, length);
        }

        #endregion

    }
}
