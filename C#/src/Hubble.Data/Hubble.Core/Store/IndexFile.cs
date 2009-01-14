using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Index;
using Hubble.Framework.IO;

namespace Hubble.Core.Store
{
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

            public short Version
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

        #region Private fields

        private string _FilePath;
        private IndexHead _Head;
        private LinkedSegmentFileStream _SegmentFileStream;
        private LinkedSegmentFileStream.SegmentPosition _LastWordIndexPosition;
        #endregion

        #region Public properties

        public string FilePath
        {
            get
            {
                return _FilePath;
            }

            set
            {
                _FilePath = value;
            }
        }

        public IndexHead Head
        {
            get
            {
                if (_Head == null)
                {
                    _Head = new IndexHead();
                }

                return _Head;
            }
        }

        #endregion

        #region Private methods

        private void OpenIndex()
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                object head;
                Hubble.Framework.Serialization.BinSerialization.DeserializeBinary(fs, out head);
                _Head = head as IndexHead;
            }

            _SegmentFileStream = new LinkedSegmentFileStream(FilePath, Head.SegmentSize, Head.AutoIncreaseBytes, Head.ReserveSegments);

            try
            {
                _LastWordIndexPosition = _SegmentFileStream.GetLastSegmentNumberFrom(1);
            }
            catch (System.IO.IOException ioe)
            {
                if (ioe.Message == "Distrustful data, next segment is zero!, from segment 1")
                {
                    if (_SegmentFileStream.CurSegment == 1)
                    {
                        _LastWordIndexPosition = new LinkedSegmentFileStream.SegmentPosition(1, 0);
                    }
                    else
                    {
                        throw ioe;
                    }
                }
            }
        }

        private void CreateNewIndex()
        {
            _SegmentFileStream = new LinkedSegmentFileStream(FilePath, true, Head.SegmentSize, Head.AutoIncreaseBytes, Head.ReserveSegments);
            _SegmentFileStream.Close();

            using (System.IO.FileStream fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite))
            {
                Hubble.Framework.Serialization.BinSerialization.SerializeBinary(this.Head, fs);
            }

            OpenIndex();
        }

        private void AddWordPosition(WordPosition wordPosition)
        {
            _SegmentFileStream.Seek(_LastWordIndexPosition.Segment, _LastWordIndexPosition.PositionInSegment);

            Hubble.Framework.Serialization.MySerialization<WordPosition>.Serialize(_SegmentFileStream, wordPosition);

            _LastWordIndexPosition.Segment = _SegmentFileStream.CurSegment;
            _LastWordIndexPosition.PositionInSegment = _SegmentFileStream.CurPositionInSegment;

        }

        #endregion

        #region Public methods

        /// <summary>
        /// Create file
        /// </summary>
        /// <param name="createNew">
        /// if createNew is ture, delete index file and create a new file
        /// else if FilePath exist, open the index
        /// </param>
        public void Create(bool createNew)
        {
            //If file does not exist, create new
            if (!System.IO.File.Exists(FilePath))
            {
                createNew = true;
            }

            if (createNew)
            {
                CreateNewIndex();
            }
            else
            {
                OpenIndex();
            }
        }

        public void Create(string filePath)
        {
            _FilePath = filePath;
            Create(false);
        }

        public void CreateNew(string filePath, IndexHead head)
        {
            _Head = head;
            _FilePath = filePath;
            Create(true);
        }

        public void Close()
        {
            if (_SegmentFileStream != null)
            {
                _SegmentFileStream.Close();
                _SegmentFileStream = null;
            }
        }

        public List<WordPosition> GetWordPositionList()
        {
            List<WordPosition> result = new List<WordPosition>();

            _SegmentFileStream.Seek(1);

            byte[] buf = new byte[Head.SegmentSize - 4];
            System.IO.MemoryStream m = new System.IO.MemoryStream(2048);

            int len = 0;
            while ((len = _SegmentFileStream.Read(buf, 0, buf.Length)) > 0)
            {
                m.Write(buf, 0, len);
            }

            if (m.Length == 0)
            {
                return result;
            }

            m.Position = 0;

            do
            {
                try
                {
                    WordPosition wordPosition = Hubble.Framework.Serialization.MySerialization<WordPosition>.Deserialize(m, new WordPosition());
                    LinkedSegmentFileStream.SegmentPosition segPosition = _SegmentFileStream.GetLastSegmentNumberFrom(wordPosition.FirstSegment);

                    wordPosition.LastSegment = segPosition.Segment;
                    wordPosition.LastPositionInSegment = segPosition.PositionInSegment;

                    result.Add(wordPosition);

                }
                catch
                {
                }

            } while (m.Position < m.Length);

            return result;
        }

        public LinkedSegmentFileStream.SegmentPosition AddWordAndDocList(string word, List<Entity.DocumentPositionList> docList)
        {
            int newSegment = _SegmentFileStream.AllocSegment();

            System.IO.MemoryStream m = new System.IO.MemoryStream();

            foreach (Entity.DocumentPositionList doc in docList)
            {
                Hubble.Framework.Serialization.MySerialization<Entity.DocumentPositionList>.Serialize(m, doc);
            }

            m.Position = 0;

            _SegmentFileStream.Write(m.ToArray(), 0, (int)m.Length);

            LinkedSegmentFileStream.SegmentPosition retVal = new LinkedSegmentFileStream.SegmentPosition(_SegmentFileStream.CurSegment,
                _SegmentFileStream.CurPositionInSegment);

            AddWordPosition(new WordPosition(word, newSegment, 0, 0));

            return retVal;
        }

        public LinkedSegmentFileStream.SegmentPosition AddDocList(LinkedSegmentFileStream.SegmentPosition segPosition, 
            List<Entity.DocumentPositionList> docList)
        {
            _SegmentFileStream.Seek(segPosition.Segment, segPosition.PositionInSegment);

            System.IO.MemoryStream m = new System.IO.MemoryStream();

            foreach (Entity.DocumentPositionList doc in docList)
            {
                Hubble.Framework.Serialization.MySerialization<Entity.DocumentPositionList>.Serialize(m, doc);
            }

            m.Position = 0;

            _SegmentFileStream.Write(m.ToArray(), 0, (int)m.Length);

            return new LinkedSegmentFileStream.SegmentPosition(_SegmentFileStream.CurSegment,
                _SegmentFileStream.CurPositionInSegment);

        }

        #endregion

    }
}
