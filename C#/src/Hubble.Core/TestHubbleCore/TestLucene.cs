using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using SimpleSearch;
using SimpleSearch.News;

namespace TestHubbleCore
{
    class TestLucene
    {
        public string NewsXml = @"C:\ApolloWorkFolder\test\laboratory\Opensource\KTDictSeg\V1.4.01\Release\news.xml";

        public void Test()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(NewsXml);
                XmlNodeList nodes = xmlDoc.SelectNodes(@"News/Item");


                Stopwatch watch = new Stopwatch();
                Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer.Duration = 0;

                DateTime old = DateTime.Now;
                int count = 0;
                int MaxCount = Math.Min(1000, nodes.Count);

                long totalChars = 0;
                Index.CreateIndex(Index.INDEX_DIR);
                Index.MaxMergeFactor = 100;
                Index.MinMergeDocs = 100;

                foreach (XmlNode node in nodes)
                {
                    String content = node.Attributes["Content"].Value;

                    totalChars += content.Length;

                    watch.Start();

                    Index.IndexString(content);

                    watch.Stop();

                    count++;

                    if (count >= MaxCount)
                    {
                        break;
                    }
                }

                watch.Start();
                Index.Close();

                watch.Stop();

                TimeSpan s = DateTime.Now - old;
                Console.WriteLine(String.Format("插入{0}行数据,共{1}字符,用时{2}秒 分词用时{3}秒",
                    MaxCount, totalChars, watch.ElapsedMilliseconds / 1000 + "." + watch.ElapsedMilliseconds % 1000,
                    Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer.Duration / 1000 + "." +
                    Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer.Duration % 1000));
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.Message);
            }
        }
    }
}
