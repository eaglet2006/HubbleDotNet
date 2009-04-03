using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Hubble.Framework.IO;
using Hubble.Framework.Threading;

namespace Hubble.Core.Store
{
    class IndexFileProxy : MessageQueue, IIndexFile
    {
        enum Event
        {
            Add = 1,
            Collect = 2,
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
        #region Public properties

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
            lock (this)
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
        }

        private void PatchWordFilePositionTable(List<IndexFile.WordFilePosition> wordFilePostionList)
        {
            lock (this)
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
                        _WordFilePositionTable.Add(p.Word, pList);
                    }
                }
            }

            InnerWordTableSize = _WordFilePositionTable.Count;
        }

        private object ProcessMessage(int evt, MessageFlag flag, object data)
        {
            switch ((Event)evt)
            {
                case Event.Add:
                    WordDocList wl = (WordDocList)data;
                    _IndexFile.AddWordAndDocList(wl.Word, wl.DocList);
                    break;
                case Event.Collect:
                    _IndexFile.Collect();
                    PatchWordFilePositionTable(_IndexFile.WordFilePositionList);
                    _IndexFile.ClearWordFilePositionList();

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

        public List<IndexFile.WordPosition> GetWordPositionList()
        {
            return _IndexFile.GetWordPositionList();
        }

        public void AddDocInfos(List<IndexFile.DocInfo> docInfos)
        {
        }

        public void AddWordPositionAndDocumentPositionList(string word,
            List<Entity.DocumentPositionList> docList)
        {
            ASendMessage((int)Event.Add, new WordDocList(word, docList));
        }


        public Hubble.Core.Index.InvertedIndex.WordIndexReader GetWordIndex(string word)
        {
            List<IndexFile.FilePosition> pList = GetFilePositionListByWord(word);

            return _IndexFile.GetWordIndex(word, pList);
        }

        public void Collect()
        {
            ASendMessage((int)Event.Collect, null);
        }

        #region IndexFileInit Members

        public void ImportWordFilePositionList(List<IndexFile.WordFilePosition> wordFilePositionList)
        {
            PatchWordFilePositionTable(wordFilePositionList);
        }

        #endregion
    }

}
