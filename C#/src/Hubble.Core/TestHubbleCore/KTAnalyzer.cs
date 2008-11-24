using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Hubble.Core.Analyze;
using KTDictSeg;
using FTAlgorithm;

namespace TestHubbleCore
{
    class KTAnalyzer : IAnalyzer
    {
        private static CSimpleDictSeg _SimpleDictSeg;
        private static object _LockObj = new object();

        public Stopwatch Stopwatch = new Stopwatch();

        private string GetAssemblyPath()
        {
            const string _PREFIX = @"file:///";
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

            codeBase = codeBase.Substring(_PREFIX.Length, codeBase.Length - _PREFIX.Length).Replace("/", "\\");
            return System.IO.Path.GetDirectoryName(codeBase) + @"\";
        }

        private void InitSimpleDictSeg()
        {
            //Init SimpleDictSeg.
            if (_SimpleDictSeg == null)
            {
                try
                {
                    _SimpleDictSeg = new CSimpleDictSeg();
                    _SimpleDictSeg.LoadConfig(GetAssemblyPath() + "KTDictSeg.xml");

                    _SimpleDictSeg.LoadDict();
                }
                catch (Exception e)
                {
                    _SimpleDictSeg = null;
                    throw e;
                }
            }
        }

        public KTAnalyzer()
        {
            lock (_LockObj)
            {
                InitSimpleDictSeg();
            }
        }

        #region IAnalyzer Members

        public IEnumerable<Hubble.Core.Entities.WordInfo> Tokenize(string text)
        {
            Stopwatch.Start();
            List<T_WordInfo> wordInfos = _SimpleDictSeg.SegmentToWordInfos(text);
            Stopwatch.Stop();

            foreach (T_WordInfo wordInfo in wordInfos)
            {
                if (wordInfo != null)
                {
                    yield return new Hubble.Core.Entities.WordInfo(wordInfo.Word, wordInfo.Position); 
                }
            }
        }

        #endregion
    }
}
