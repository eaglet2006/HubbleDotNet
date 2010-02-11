using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.IO;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Store
{
    class WordFilePositionList 
    {
        List<IndexFile.FilePosition> _FPList;
        WordFilePositionProvider _Provider;
        string _Word;

        internal string Word
        {
            get
            {
                return _Word;
            }
        }

        internal List<IndexFile.FilePosition> FPList
        {
            get
            {
                return _FPList;
            }
        }


        internal WordFilePositionList()
        {
            _FPList = new List<IndexFile.FilePosition>();
        }

        internal WordFilePositionList(WordFilePositionProvider provider, string word)
        {
            _FPList = new List<IndexFile.FilePosition>();
            _Provider = provider;
            _Word = word;
        }

        internal void AddOnly(IndexFile.FilePosition fp)
        {
#if PerformanceTest
            if (_FPList.Count > 0)
            {
                if (fp.Serial <= _FPList[_FPList.Count - 1].Serial)
                {
                    throw new ArgumentOutOfRangeException(string.Format("Add fp.Serial={0} last serial={1}",
                        fp.Serial, _FPList[_FPList.Count - 1].Serial));
                }
            }
#endif

            _FPList.Add(fp);
        }

        internal void Add(IndexFile.FilePosition fp)
        {
            #if PerformanceTest
            if (_FPList.Count > 0)
            {
                if (fp.Serial <= _FPList[_FPList.Count - 1].Serial)
                {
                    throw new ArgumentOutOfRangeException(string.Format("Add fp.Serial={0} last serial={1}",
                        fp.Serial, _FPList[_FPList.Count - 1].Serial));
                }
            }
            #endif

            _FPList.Add(fp);


            if (_Provider != null)
            {
                _Provider.Add(_Word, fp);
            }
        }

        internal IndexFile.FilePosition this[int index]
        {
            get
            {
                return _FPList[index];
            }

            set
            {
                _FPList[index] = value;
            }
        }


        internal void RemoveAt(int index)
        {
            _FPList.RemoveAt(index);
        }

        internal IEnumerable<IndexFile.FilePosition> Values
        {
            get
            {
                foreach (IndexFile.FilePosition fp in _FPList)
                {
                    yield return fp;
                }
            }
        }

        internal int Count
        {
            get
            {
                return _FPList.Count;
            }
        }
    }

    class WordFilePositionProvider
    {
        struct WordFilePositionIndex
        {
            public string Word;
            public FilePositionIndex FileIndex;

            public WordFilePositionIndex(string word, FilePositionIndex fpi)
            {
                Word = word;
                FileIndex = fpi;
            }
        }


        struct FilePositionIndex
        {
            public int Index;
            public int Count;

            public FilePositionIndex(int index, int count)
            {
                Index = index;
                Count = count;
            }
        }

        //private Dictionary<string, List<IndexFile.FilePosition>> _WordFilePositionTable = new Dictionary<string, List<IndexFile.FilePosition>>();

        private object _LockObj = new object();

        private Dictionary<string, byte[]> _WordFilePositionTable = new Dictionary<string, byte[]>();

        //private BlockedMemoryStream _BlockedMemory = new BlockedMemoryStream(1024 * 256);

        //private List<WordFilePositionIndex> _List;

        public void Add(string word, IndexFile.FilePosition fp)
        {
            lock (_LockObj)
            {
                byte[] buf;

                System.IO.MemoryStream m = new System.IO.MemoryStream();
                bool find = false;

                if (_WordFilePositionTable.TryGetValue(word, out buf))
                {
                    find = true;
                    m.Write(buf, 0, buf.Length);
                }

                VInt.sWriteToStream(fp.Serial, m);
                VLong.sWriteToStream(fp.Length, m);
                VLong.sWriteToStream(fp.Position, m);

                buf = new byte[m.Length];
                m.Position = 0;
                m.Read(buf, 0, buf.Length);

                if (find)
                {
                    _WordFilePositionTable[word] = buf;
                }
                else
                {
                    _WordFilePositionTable.Add(word, buf);
                }
            }
        }

        public void Add(string word, WordFilePositionList filePositionList)
        {
            //_WordFilePositionTable.Add(word, filePositionList);

            //_BlockedMemory.Seek(0, System.IO.SeekOrigin.End);

            //_WordFilePositionTable.Add(word, new FilePositionIndex((int)_BlockedMemory.Position, filePositionList.Count));
            lock (_LockObj)
            {

                byte[] buf;

                System.IO.MemoryStream m = new System.IO.MemoryStream();
                bool find = false;

                if (_WordFilePositionTable.TryGetValue(word, out buf))
                {
                    find = true;
                    m.Write(buf, 0, buf.Length);
                }

                foreach (IndexFile.FilePosition fp in filePositionList.Values)
                {
                    VInt.sWriteToStream(fp.Serial, m);
                    VLong.sWriteToStream(fp.Length, m);
                    VLong.sWriteToStream(fp.Position, m);
                }

                buf = new byte[m.Length];
                m.Position = 0;
                m.Read(buf, 0, buf.Length);

                if (find)
                {
                    _WordFilePositionTable[word] = buf;
                }
                else
                {
                    _WordFilePositionTable.Add(word, buf);
                }
            }
        }

        public void Reset(string word, List<IndexFile.FilePosition> filePositionList)
        {
            //_WordFilePositionTable.Add(word, filePositionList);

            //_BlockedMemory.Seek(0, System.IO.SeekOrigin.End);

            //_WordFilePositionTable.Add(word, new FilePositionIndex((int)_BlockedMemory.Position, filePositionList.Count));
            lock (_LockObj)
            {
                byte[] buf;

                System.IO.MemoryStream m = new System.IO.MemoryStream();
                bool find = false;

                if (_WordFilePositionTable.TryGetValue(word, out buf))
                {
                    find = true;
                }

                foreach (IndexFile.FilePosition fp in filePositionList)
                {
                    VInt.sWriteToStream(fp.Serial, m);
                    VLong.sWriteToStream(fp.Length, m);
                    VLong.sWriteToStream(fp.Position, m);
                }

                buf = new byte[m.Length];
                m.Position = 0;
                m.Read(buf, 0, buf.Length);

                if (find)
                {
                    _WordFilePositionTable[word] = buf;
                }
                else
                {
                    _WordFilePositionTable.Add(word, buf);
                }
            }
        }

        public void Collect()
        {
            //_List = new List<WordFilePositionIndex>(_WordFilePositionTable.Count);

            //foreach (string key in _WordFilePositionTable.Keys)
            //{
            //    _List.Add(new WordFilePositionIndex(key, _WordFilePositionTable[key]));
            //}

            //_WordFilePositionTable = null;

            //GC.Collect();
        }

        public bool TryGetValue(string word, out WordFilePositionList filePositionList)
        {
            lock (_LockObj)
            {
                byte[] buf;

                if (_WordFilePositionTable.TryGetValue(word, out buf))
                {
                    System.IO.MemoryStream m = new System.IO.MemoryStream(buf);
                    //_BlockedMemory.Seek(fpi.Index, System.IO.SeekOrigin.Begin);

                    filePositionList = new WordFilePositionList(this, word);

                    while (m.Position < m.Length)
                    {
                        IndexFile.FilePosition fp = new IndexFile.FilePosition();
                        fp.Serial = VInt.sReadFromStream(m);
                        fp.Length = (int)VLong.sReadFromStream(m);
                        fp.Position = VLong.sReadFromStream(m);
                        filePositionList.AddOnly(fp);
                    }

                    //for (int i = 0; i < fpi.Count; i++)
                    //{
                    //    IndexFile.FilePosition fp = new IndexFile.FilePosition(); 


                    //    VInt serial = new VInt();
                    //    fp.Serial = serial.ReadFromStream(_BlockedMemory);

                    //    VLong length = new VLong();
                    //    fp.Length = length.ReadFromStream(_BlockedMemory);
                    //    VLong position = new VLong();
                    //    fp.Position = position.ReadFromStream(_BlockedMemory);

                    //    filePositionList.Add(fp);
                    //}

                    return true;
                }
                else
                {
                    filePositionList = null;
                    return false;
                }
            }

            //return _WordFilePositionTable.TryGetValue(word, out filePositionList);
        }

        public void Clear()
        {
            lock (_LockObj)
            {
                _WordFilePositionTable.Clear();
            }
            //_BlockedMemory.Clear();
        }

        public WordFilePositionList this[string word]
        {
            get
            {
                WordFilePositionList fpl;
                if (TryGetValue(word, out fpl))
                {
                    return fpl;
                }
                else
                {
                    throw new Exception("Word isn't in dictionary!");
                }
                
                //return _WordFilePositionTable[word];
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                foreach (string key in _WordFilePositionTable.Keys)
                {
                    yield return key;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_LockObj)
                {
                    return _WordFilePositionTable.Count;
                }
            }
        }


    }
}
