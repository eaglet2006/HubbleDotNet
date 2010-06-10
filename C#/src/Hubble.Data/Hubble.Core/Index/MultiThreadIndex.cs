using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.Data;

namespace Hubble.Core.Index
{
    class MultiThreadIndex
    {
        class IndexThread
        {
            private InvertedIndex _Index;

            private IList<Document> _Docs;
            private int _FieldIndex;

            private System.Threading.Thread _Thread;

            private object _LockObj = new object();

            private Exception _Exception = null;

            internal string FieldName
            {
                get
                {
                    lock (_LockObj)
                    {
                        return _Index.FieldName;
                    }
                }
            }

            internal Exception Exception
            {
                get
                {
                    lock (_LockObj)
                    {
                        return _Exception;
                    }
                }
            }

            private bool _Finished = false;

            internal bool Finished
            {
                get
                {
                    lock (_LockObj)
                    {
                        return _Finished;
                    }
                }
            }

            internal IndexThread(InvertedIndex index, IList<Document> docs, int fieldIndex)
            {
                _Finished = false;
                _Index = index;
                _Docs = docs;
                _FieldIndex = fieldIndex;

                _Thread = new System.Threading.Thread(ThreadProc);

                _Thread.IsBackground = true;

                _Thread.Start();
            }

            private void ThreadProc()
            {
                try
                {
                    _Index.Index(_Docs, _FieldIndex);
                    _Index.FinishIndex();
                }
                catch (Exception e)
                {
                    if (_Index != null)
                    {
                        Global.Report.WriteErrorLog(string.Format("Index field:{0} fail!", _Index.FieldName), e);
                    }

                    lock (_LockObj)
                    {
                        _Exception = e;
                    }
                }
                finally
                {
                    lock (_LockObj)
                    {
                        _Finished = true;
                    }
                }
            }
        }

        private IndexThread[] _IndexPool;

        internal MultiThreadIndex(int threadNumber)
        {
            if (threadNumber <= 0)
            {
                threadNumber = 1;
            }

            if (threadNumber > 8)
            {
                threadNumber = 8;
            }

            _IndexPool = new IndexThread[threadNumber];
        }

        internal void Index(InvertedIndex index, IList<Document> docs, int fieldIndex)
        {
            do
            {
                for (int i = 0; i < _IndexPool.Length; i++)
                {
                    if (_IndexPool[i] == null)
                    {
                        _IndexPool[i] = new IndexThread(index, docs, fieldIndex);
                        return;
                    }
                    else
                    {
                        if (_IndexPool[i].Finished)
                        {
                            if (_IndexPool[i].Exception != null)
                            {
                                //Some field index excetpion
                                WaitForAllFinished();
                                return;
                            }

                            _IndexPool[i] = new IndexThread(index, docs, fieldIndex);
                            return;
                        }
                    }
                }

                System.Threading.Thread.Sleep(50);

            }while (true);
        }

        internal void WaitForAllFinished()
        {
            bool finished = false;
            bool hasException = false;

            while (!finished)
            {
                finished = true;

                for (int i = 0; i < _IndexPool.Length; i++)
                {
                    if (_IndexPool[i] != null)
                    {
                        if (!_IndexPool[i].Finished)
                        {
                            finished = false;
                            break;
                        }
                        else
                        {
                            if (_IndexPool[i].Exception != null)
                            {
                                hasException = true;
                            }
                        }
                    }
                }

                System.Threading.Thread.Sleep(10);
            }

            if (hasException)
            {
                StringBuilder exceptionMessage = new StringBuilder();

                for (int i = 0; i < _IndexPool.Length; i++)
                {
                    if (_IndexPool[i] != null)
                    {
                        if (_IndexPool[i].Exception != null)
                        {
                            exceptionMessage.AppendFormat("Field:{0} index fail! Message:{1} StackTrace{2}\r\n",
                                _IndexPool[i].FieldName, _IndexPool[i].Exception.Message,
                                _IndexPool[i].Exception.StackTrace);
                        }
                    }
                }

                throw new Data.DataException(exceptionMessage.ToString());
            }
        }
    }
}
