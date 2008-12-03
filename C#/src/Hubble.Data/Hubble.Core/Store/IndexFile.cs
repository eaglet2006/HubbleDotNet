using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Index;

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
        #region Private fields
        private string _FilePath;
        private IndexHead _Head;
        private System.IO.FileStream _FileStream;
        private byte[] _CurrentSegment;
        private InvertedIndex _InvertedIndex;
        private int _LastWordIndexSegment = 0;
        private int _LastWordIndexPositionInSegment = 0;
        #endregion

        #region Private properties

        private int NextSegment
        {
            get
            {
                return BitConverter.ToInt32(_CurrentSegment, _CurrentSegment.Length - 4);
            }

            set
            {
                byte[] buf = BitConverter.GetBytes(value);

                Array.Copy(buf, 0, _CurrentSegment, _CurrentSegment.Length - 4, 4);
            }
        }

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

        public InvertedIndex InvertedIndex
        {
            get
            {
                return _InvertedIndex;
            }
        }

        #endregion

        #region Private methods

        private void ReadOneSegment(int segment)
        {
            _FileStream.Seek(Head.SegmentSize * segment, System.IO.SeekOrigin.Begin);

            int len = _FileStream.Read(_CurrentSegment, 0, _CurrentSegment.Length);
            int offset = len;

            while (offset < _CurrentSegment.Length)
            {
                len = _FileStream.Read(_CurrentSegment, offset, _CurrentSegment.Length - offset);
                offset += len;
            }
        }

        /// <summary>
        /// Load all the words in inverted index
        /// </summary>
        private void LoadWordIndexes()
        {
            _LastWordIndexSegment = 1;

            do
            {
                ReadOneSegment(_LastWordIndexSegment);

                int nextSegment = NextSegment;

                if (nextSegment > 0)
                {
                    _LastWordIndexSegment = nextSegment;
                }
                else
                {
                    break;
                }

            } while (true);
        }

        private void OpenIndex()
        {
            _FileStream = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);

            object head ;
            Hubble.Framework.Serialization.BinSerialization.DeserializeBinary(_FileStream, out head);

            _Head = head as IndexHead;

            if (Hubble.Framework.IO.File.GetFileLength(FilePath) < Head.SegmentSize * Head.ReserveSegments)
            {
                throw new StoreException(string.Format("Invalid index file length, filepath:{0} segment size:{1} reservesegments:{2}",
                    FilePath, Head.SegmentSize, Head.ReserveSegments));
            }


            _InvertedIndex = new InvertedIndex();

            _CurrentSegment = new byte[Head.SegmentSize];
        }

        private void CreateNewIndex()
        {
            if (System.IO.File.Exists(FilePath))
            {
                _FileStream = new System.IO.FileStream(FilePath, System.IO.FileMode.Truncate, System.IO.FileAccess.ReadWrite);
            }
            else
            {
                _FileStream = new System.IO.FileStream(FilePath, System.IO.FileMode.CreateNew, System.IO.FileAccess.ReadWrite);
            }

            _CurrentSegment = new byte[Head.SegmentSize];

            _FileStream.SetLength(Head.SegmentSize * Head.ReserveSegments);

            //for (int i = 0; i < Head.ReserveSegments; i++)
            //{
            //    _FileStream.Write(_CurrentSegment, 0, _CurrentSegment.Length);
            //}

            _FileStream.Seek(0, System.IO.SeekOrigin.Begin);

            //SerializeBinary has flushed
            Hubble.Framework.Serialization.BinSerialization.SerializeBinary(this.Head, _FileStream);

            _InvertedIndex = new InvertedIndex();
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
            if (_FileStream != null)
            {
                _FileStream.Close();
            }
        }

        #endregion
    }
}
