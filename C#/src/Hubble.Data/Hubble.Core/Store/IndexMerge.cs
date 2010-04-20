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
using System.Threading;

using Hubble.Core.Data;

namespace Hubble.Core.Store
{
    class IndexMerge
    {
        string _IndexDir;

        IndexFileProxy _IndexFileProxy;

        OptimizationOption _Option;

        Thread _Thread;

        bool _Closed = false;
        bool _CanClose = true;

        public string IndexDir
        {
            get
            {
                return _IndexDir;
            }
        }

        internal bool CanClose
        {
            get
            {
                lock (this)
                {
                    return _CanClose;
                }
            }
        }

        #region Private methods

        private bool DoMerge(OptimizationOption option)
        {
            lock (this)
            {
                _CanClose = true;
            }

            while (!_IndexFileProxy.CanMerge)
            {
                if (_Closed)
                {
                    _Thread = null;
                    return false;
                }

                System.Threading.Thread.Sleep(10);
            }

            Dictionary<int, System.IO.FileStream> indexSrcFileDict = new Dictionary<int, System.IO.FileStream>();

            try
            {
                IndexFileProxy.MergeInfos mergeInfos = _IndexFileProxy.GetMergeInfos(option);

                if (mergeInfos == null)
                {
                    return false;
                }

                IndexFileProxy.MergeAck mergeAck = new IndexFileProxy.MergeAck(mergeInfos.BeginSerial,
                    mergeInfos.EndSerial, mergeInfos.MergeHeadFileName, mergeInfos.MergeIndexFileName,
                    mergeInfos.MergedSerial);

                mergeInfos.MergedWordFilePostionList.Sort();

                string optimizeDir = IndexDir + @"Optimize\";

                if (!System.IO.Directory.Exists(optimizeDir))
                {
                    System.IO.Directory.CreateDirectory(optimizeDir);
                }

                string optimizeHeadFile = optimizeDir + mergeInfos.MergeHeadFileName;
                string optimizeIndexFile = optimizeDir + mergeInfos.MergeIndexFileName;

#if DEBUG
                long TestLen = 0;
                int oneFileCount = 0;
                int moreFileCount = 0;
                int maxFileCount = 0;
                Console.WriteLine(string.Format("File ={0} Begin={1} End={2}", System.IO.Path.GetFileName(mergeInfos.MergeHeadFileName),
                    mergeInfos.BeginSerial, mergeInfos.EndSerial));
#endif

                using (System.IO.FileStream headFS = new System.IO.FileStream(optimizeHeadFile, System.IO.FileMode.Create,
                    System.IO.FileAccess.ReadWrite))
                {
                    using (System.IO.FileStream indexFS = new System.IO.FileStream(optimizeIndexFile, System.IO.FileMode.Create,
                        System.IO.FileAccess.ReadWrite))
                    {
                        System.IO.MemoryStream m = new System.IO.MemoryStream();

                        _IndexFileProxy.MergeRate = 0;

                        int count = mergeInfos.MergedWordFilePostionList.Count;

                        int wplIndex = 0;

                        foreach (IndexFileProxy.MergedWordFilePostionList wpl in mergeInfos.MergedWordFilePostionList)
                        {
                            wplIndex++;

                            _IndexFileProxy.MergeRate = (double)wplIndex / (double)count;

                            List<Entity.MergeStream> mergeStreamList = new List<Hubble.Core.Entity.MergeStream>();

                            foreach (IndexFile.FilePosition fp in wpl.FilePositionList.Values)
                            {
                                lock (this)
                                {
                                    if (_Closed)
                                    {
                                        _Thread = null;
                                        return false;
                                    }
                                }

                                if (fp.Length <= 0)
                                {
                                    continue;
                                }

                                if (!indexSrcFileDict.ContainsKey(fp.Serial))
                                {
                                    indexSrcFileDict.Add(fp.Serial,
                                        new System.IO.FileStream(IndexDir + _IndexFileProxy.GetIndexFileName(fp.Serial),
                                             System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read));
                                }

                                System.IO.FileStream fs = indexSrcFileDict[fp.Serial];
                                mergeStreamList.Add(new Hubble.Core.Entity.MergeStream(fs, fp.Length));
                                fs.Position = fp.Position;
                            }


                            if (mergeStreamList.Count > 0)
                            {
#if DEBUG
                                if (mergeStreamList.Count == 1)
                                {
                                    oneFileCount++;
                                }
                                else
                                {
                                    moreFileCount++;
                                }

                                if (maxFileCount < mergeStreamList.Count)
                                {
                                    maxFileCount = mergeStreamList.Count;
                                }

#endif
                                long position = indexFS.Position;
                                Entity.DocumentPositionList.Merge(mergeStreamList, indexFS);

                                IndexWriter.WriteHeadFile(headFS, wpl.Word, position, indexFS.Position - position);

#if DEBUG
                                TestLen += indexFS.Position - position;
#endif

                                mergeAck.MergeFilePositionList.Add(new IndexFileProxy.MergeAck.MergeFilePosition(
                                    new IndexFile.FilePosition(mergeInfos.MergedSerial, position, (int)(indexFS.Position - position)),
                                    wpl.Word));
                            }
                        }
                    }
                }


                foreach (System.IO.FileStream fs in indexSrcFileDict.Values)
                {
#if DEBUG
                    Console.WriteLine(string.Format("{0} len={1}", fs.Name, fs.Length));
#endif
                    fs.Close();
                }

#if DEBUG
                Console.WriteLine(string.Format("Merge Len = {0} one={1} more={2} max={3}",
                    TestLen, oneFileCount, moreFileCount, maxFileCount));
#endif


                lock (this)
                {
                    if (_Closed)
                    {
                        _Thread = null;
                        _CanClose = true;
                        return false;
                    }

                    _CanClose = false;
                }

                _IndexFileProxy.DoMergeAck(mergeAck);

                mergeAck = null;

                GC.Collect();
                GC.Collect();

                lock (this)
                {
                    _CanClose = true;
                }

                return true;
            }
            catch (Exception e)
            {
                Global.Report.WriteErrorLog(string.Format("DoMerge fail. err:{0} stack:{1}",
                    e.Message, e.StackTrace));
            }
            finally
            {
                try
                {
                    foreach (System.IO.FileStream fs in indexSrcFileDict.Values)
                    {
                        fs.Close();
                    }
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog(string.Format("DoMerge close file fail. err:{0} stack:{1}",
                        e.Message, e.StackTrace));
                }
            }

            return false;
        }


        private bool DoMerge1(OptimizationOption option)
        {
            lock (this)
            {
                _CanClose = true;
            }

            Dictionary<int, System.IO.FileStream> indexSrcFileDict = new Dictionary<int, System.IO.FileStream>();

            try
            {
                IndexFileProxy.MergeInfos mergeInfos = _IndexFileProxy.GetMergeInfos(option);

                if (mergeInfos == null)
                {
                    return false;
                }

                IndexFileProxy.MergeAck mergeAck = new IndexFileProxy.MergeAck(mergeInfos.BeginSerial,
                    mergeInfos.EndSerial, mergeInfos.MergeHeadFileName, mergeInfos.MergeIndexFileName,
                    mergeInfos.MergedSerial);

                mergeInfos.MergedWordFilePostionList.Sort();

                string optimizeDir = IndexDir + @"Optimize\";

                if (!System.IO.Directory.Exists(optimizeDir))
                {
                    System.IO.Directory.CreateDirectory(optimizeDir);
                }

                string optimizeHeadFile = optimizeDir + mergeInfos.MergeHeadFileName;
                string optimizeIndexFile = optimizeDir + mergeInfos.MergeIndexFileName;

                using (System.IO.FileStream headFS = new System.IO.FileStream(optimizeHeadFile, System.IO.FileMode.Create,
                    System.IO.FileAccess.ReadWrite))
                {
                    using (System.IO.FileStream indexFS = new System.IO.FileStream(optimizeIndexFile, System.IO.FileMode.Create,
                        System.IO.FileAccess.ReadWrite))
                    {
                        System.IO.MemoryStream m = new System.IO.MemoryStream();

                        foreach (IndexFileProxy.MergedWordFilePostionList wpl in mergeInfos.MergedWordFilePostionList)
                        {
                            foreach (IndexFile.FilePosition fp in wpl.FilePositionList.Values)
                            {
                                lock (this)
                                {
                                    if (_Closed)
                                    {
                                        _Thread = null;
                                        return false;
                                    }
                                }

                                if (fp.Length <= 0)
                                {
                                    continue;
                                }

                                if (!indexSrcFileDict.ContainsKey(fp.Serial))
                                {
                                    indexSrcFileDict.Add(fp.Serial,
                                        new System.IO.FileStream(IndexDir + _IndexFileProxy.GetIndexFileName(fp.Serial),
                                             System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read));
                                }

                                System.IO.FileStream fs = indexSrcFileDict[fp.Serial];

                                byte[] buf = new byte[fp.Length];

                                fs.Seek(fp.Position, System.IO.SeekOrigin.Begin);

                                int read = 0;
                                int offset = 0;

                                while ((read = fs.Read(buf, offset, buf.Length - offset)) > 0)
                                {
                                    offset += read;
                                    if (offset == buf.Length)
                                    {
                                        break;
                                    }
                                }

                                m.Write(buf, 0, buf.Length - 1);
                            }

                            if (m.Length > 0)
                            {
                                m.WriteByte(0);
                                byte[] buf = m.GetBuffer();
                                long position = indexFS.Position;

                                indexFS.Write(buf, 0, (int)m.Length);

                                IndexWriter.WriteHeadFile(headFS, wpl.Word, position, m.Length);

                                mergeAck.MergeFilePositionList.Add(new IndexFileProxy.MergeAck.MergeFilePosition(
                                    new IndexFile.FilePosition(mergeInfos.MergedSerial, position, (int)m.Length),
                                    wpl.Word));

                                m.SetLength(0);
                                m.Position = 0;
                            }
                        }
                    }
                }

                foreach (System.IO.FileStream fs in indexSrcFileDict.Values)
                {
                    fs.Close();
                }

                lock (this)
                {
                    if (_Closed)
                    {
                        _Thread = null;
                        _CanClose = true;
                        return false;
                    }

                    _CanClose = false;
                }

                _IndexFileProxy.DoMergeAck(mergeAck);

                mergeAck = null;

                GC.Collect();
                GC.Collect();

                lock (this)
                {
                    _CanClose = true;
                }

                return true;
            }
            catch (Exception e)
            {
                Global.Report.WriteErrorLog(string.Format("DoMerge fail. err:{0} stack:{1}",
                    e.Message, e.StackTrace));
            }
            finally
            {
                try
                {
                    foreach (System.IO.FileStream fs in indexSrcFileDict.Values)
                    {
                        fs.Close();
                    }
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog(string.Format("DoMerge close file fail. err:{0} stack:{1}",
                        e.Message, e.StackTrace));
                }
            }

            return false;
        }

        private void MergeThread()
        {
            while (true)
            {
                OptimizationOption option;

                lock (this)
                {
                    if (_Closed)
                    {
                        _Thread = null;
                        return;
                    }

                    option = _Option;
                    _Option = OptimizationOption.Idle;

                    if (option == OptimizationOption.Idle)
                    {
                        _Thread = null;
                        return;
                    }
                }

                DoMerge(option);
            }
        }

        #endregion


        public IndexMerge(string indexDir, IndexFileProxy indexFileProxy)
        {
            _IndexDir = Hubble.Framework.IO.Path.AppendDivision(indexDir, '\\');
            _IndexFileProxy = indexFileProxy;
        }

        public void Optimize()
        {
            Optimize(OptimizationOption.Middle);
        }

        public void Optimize(OptimizationOption option)
        {
            lock (this)
            {
                if (_Closed)
                {
                    return;
                }

                _Option = option;

                if (_Thread == null)
                {
                    _Thread = new Thread(new ThreadStart(MergeThread));
                    _Thread.IsBackground = true;
                    _Thread.Start();
                }
            }
        }

        public void Close()
        {
            lock (this)
            {
                _Closed = true;
            }

Loop1:
            int i = 0;
            while (i++ < 100)
            {
                lock (this)
                {
                    if (_Thread == null)
                    {
                        return;
                    }
                }

                Thread.Sleep(50);
            }

            Thread t;
            bool canClose;
            lock (this)
            {
                t = _Thread;
                canClose = _CanClose;
            }

            if (!canClose)
            {
                goto Loop1;
            }

            if (t != null)
            {
                try
                {
                    t.Abort();
                }
                catch
                {
                }
                finally
                {
                    _Thread = null;
                }
            }

        }
    }

}
