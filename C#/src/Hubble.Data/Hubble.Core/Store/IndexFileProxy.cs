using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Hubble.Framework.IO;

namespace Hubble.Core.Store
{
    class IndexFileProxy
    {
        #region Events
        public class AddDocListFinishedEventArgs : EventArgs
        {
            private LinkedSegmentFileStream.SegmentPosition _LastSegmentPosition;

            public LinkedSegmentFileStream.SegmentPosition LastSegmentPosition
            {
                get
                {
                    return _LastSegmentPosition;
                }
            }

            public AddDocListFinishedEventArgs(LinkedSegmentFileStream.SegmentPosition position)
            {
                _LastSegmentPosition = position;
            }
        }

        public event EventHandler<AddDocListFinishedEventArgs> AddDocListFinishedEventHandler;

        protected void OnAddDocListFinished(AddDocListFinishedEventArgs e)
        {
            EventHandler<AddDocListFinishedEventArgs> handler = AddDocListFinishedEventHandler;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        private IndexFile _IndexFile;

        public IndexFileProxy(string filePath)
        {
            _IndexFile = new IndexFile();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _IndexFile.Head.Version = info.FileVersion;
            _IndexFile.Head.FieldName = System.IO.Path.GetFileName(filePath);
            _IndexFile.Head.ReserveSegments = 2048;
            _IndexFile.Head.SegmentSize = 2048;
            _IndexFile.Head.AutoIncreaseBytes = 10 * 1024 * 1024;

            _IndexFile.Create(filePath);
        }

        public List<IndexFile.WordPosition> GetWordPositionList()
        {
            return _IndexFile.GetWordPositionList();
        }

        public LinkedSegmentFileStream.SegmentPosition AddWordPositionAndDocumentPositionList(string word,
            List<Entity.DocumentPositionList> docList)
        {
            return _IndexFile.AddWordAndDocList(word, docList);
        }

        public void AddDocumentPositionList(LinkedSegmentFileStream.SegmentPosition segPosition, 
            List<Entity.DocumentPositionList> docList)
        {
            LinkedSegmentFileStream.SegmentPosition position = _IndexFile.AddDocList(segPosition, docList);

            OnAddDocListFinished(new AddDocListFinishedEventArgs(position));

        }

    }

}
