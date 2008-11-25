using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace TestHubbleCore
{
    class TestHubble
    {
        public string NewsXml = @"C:\ApolloWorkFolder\test\laboratory\Opensource\KTDictSeg\V1.4.01\Release\news.xml";

        public void Test()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(NewsXml);
                XmlNodeList nodes = xmlDoc.SelectNodes(@"News/Item");

                Hubble.Core.Index.InvertedIndex invertedIndex = new Hubble.Core.Index.InvertedIndex();

                KTAnalyzer ktAnalyzer = new KTAnalyzer();

                Stopwatch watch = new Stopwatch();
                ktAnalyzer.Stopwatch.Reset();
                long docId = 0;
                int totalChars = 0;
                
                foreach (XmlNode node in nodes)
                {
                    String title = node.Attributes["Title"].Value;
                    DateTime time = DateTime.Parse(node.Attributes["Time"].Value);
                    String Url = node.Attributes["Url"].Value;
                    String content = node.Attributes["Content"].Value;

                    totalChars += content.Length;

                    watch.Start();

                    invertedIndex.Index(content, docId++, ktAnalyzer);
                    watch.Stop();

                    if (docId == 2000)
                    {
                        break;
                    }
                }

                watch.Start();
                watch.Stop();

                Console.WriteLine(String.Format("插入{0}行数据,共{1}字符,用时{2}秒 分词用时{3}秒",
                    docId, totalChars, watch.ElapsedMilliseconds / 1000 + "." + watch.ElapsedMilliseconds % 1000,
                    ktAnalyzer.Stopwatch.ElapsedMilliseconds / 1000 + "." + ktAnalyzer.Stopwatch.ElapsedMilliseconds % 1000));

                Hubble.Core.Query.FullTextQuery fullTextQuery = new Hubble.Core.Query.FullTextQuery();
                fullTextQuery.Analyzer = ktAnalyzer;
                fullTextQuery.InvertedIndex = invertedIndex;
                fullTextQuery.QueryString = "北京大学";

                long queryDocId;
                do
                {
                    List<Hubble.Core.Entity.WordInfo> wordInfos = fullTextQuery.GetNextHitWords(out queryDocId);
                    //queryDocId = wordInfos.Count;
                } while (queryDocId >= 0);
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.Message);
            }
        }
    }
}
