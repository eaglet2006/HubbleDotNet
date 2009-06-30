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

        public string IndexDir
        {
            get
            {
                return _IndexDir;
            }
        }

        #region Private methods

        private bool DoMerge(OptimizationOption option)
        {
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
                            foreach (IndexFile.FilePosition fp in wpl.FilePositionList)
                            {
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
                                    new IndexFile.FilePosition(mergeInfos.MergedSerial, position, m.Length),
                                    wpl.OrginalFilePositionList));

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

                _IndexFileProxy.DoMergeAck(mergeAck);

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
                _Option = option;

                if (_Thread == null)
                {
                    _Thread = new Thread(new ThreadStart(MergeThread));
                    _Thread.IsBackground = true;
                    _Thread.Start();
                }
            }
        }
    }
}
