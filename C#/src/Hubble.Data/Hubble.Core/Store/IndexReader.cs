using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.IO;

namespace Hubble.Core.Store
{
    class IndexReader : IDisposable
    {
        string _HeadFilePath;
        string _IndexFilePath;
        private int _Serial;

        System.IO.FileStream _HeadFile;
        System.IO.FileStream _IndexFile;

        private string HeadFilePath
        {
            get
            {
                return _HeadFilePath;
            }
        }

        private string IndexFilePath
        {
            get
            {
                return _IndexFilePath;
            }
        }

        public IndexReader(int serial, string path, string fieldName)
        {
            _Serial = serial;

            _HeadFilePath = Path.AppendDivision(path, '\\') + 
                string.Format("{0:D4}{1}.hdx", serial, fieldName);
            _IndexFilePath = Path.AppendDivision(path, '\\') +
                string.Format("{0:D4}{1}.idx", serial, fieldName);

            _HeadFile = new System.IO.FileStream(_HeadFilePath, System.IO.FileMode.Open,
                 System.IO.FileAccess.Read, System.IO.FileShare.Read);
            _IndexFile = new System.IO.FileStream(_IndexFilePath, System.IO.FileMode.Open,
                 System.IO.FileAccess.Read, System.IO.FileShare.Read);
        }

        public List<IndexFile.WordFilePosition> GetWordFilePositionList()
        {
            List<IndexFile.WordFilePosition> list = new List<IndexFile.WordFilePosition>();

            _HeadFile.Seek(0, System.IO.SeekOrigin.Begin);

            while (_HeadFile.Position < _HeadFile.Length)
            {
                byte[] buf = new byte[sizeof(long)];

                _HeadFile.Read(buf, 0, sizeof(int));

                int size = BitConverter.ToInt32(buf, 0);

                byte[] wordBuf = new byte[size - sizeof(long)];

                _HeadFile.Read(buf, 0, sizeof(long));
                long position = BitConverter.ToInt64(buf, 0);

                _HeadFile.Read(wordBuf, 0, wordBuf.Length);
                string word = string.Intern(Encoding.UTF8.GetString(wordBuf));

                list.Add(new IndexFile.WordFilePosition(word, _Serial, position));
            }

            return list;
        }

        public List<Entity.DocumentPositionList> GetDocList(long position)
        {
            _IndexFile.Seek(position, System.IO.SeekOrigin.Begin);

            List<Entity.DocumentPositionList> result = new List<Hubble.Core.Entity.DocumentPositionList>();

            Entity.DocumentPositionList iDocList = new Entity.DocumentPositionList();

            do
            {
                try
                {
                    Entity.DocumentPositionList docList = 
                        Hubble.Framework.Serialization.MySerialization<Entity.DocumentPositionList>.Deserialize(
                        _IndexFile, iDocList);

                    if (docList == null)
                    {
                        break;
                    }

                    result.Add(docList);

                }
                catch
                {
                }

            } while (true);

            return result;
        }

        public void Close()
        {
            _HeadFile.Close();
            _IndexFile.Close();

            _HeadFile = null;
            _IndexFile = null;

        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (_HeadFile != null)
                {
                    Close();
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
