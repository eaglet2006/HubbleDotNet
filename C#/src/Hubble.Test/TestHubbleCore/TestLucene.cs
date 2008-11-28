using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using SimpleSearch;
using SimpleSearch.News;

using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;
using FTAlgorithm;

namespace TestHubbleCore
{
    class TestLucene
    {
        public string NewsXml = @"C:\ApolloWorkFolder\test\laboratory\Opensource\KTDictSeg\V1.4.01\Release\news.xml";

        public void Test(List<XmlNode> documentList)
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer.Duration = 0;

                DateTime old = DateTime.Now;
                int count = 0;

                long totalChars = 0;
                Index.CreateIndex(Index.INDEX_DIR);
                Index.MaxMergeFactor = 100;
                Index.MinMergeDocs = 100;
                long docId = 0;
                foreach (XmlNode node in documentList)
                {
                    String content = node.Attributes["Content"].Value;

                    totalChars += content.Length;

                    watch.Start();

                    if (Global.Cfg.TestShortText)
                    {
                        for (int i = 0; i < content.Length / 100; i++)
                        {
                            Index.IndexString(content.Substring(i * 100, 100));
                            docId++;
                        }
                    }
                    else
                    {
                        Index.IndexString(content);
                        docId++;
                    }


                    watch.Stop();

                    count++;

                    if (count >= Global.Cfg.TestRows)
                    {
                        break;
                    }
                }

                watch.Start();
                Index.Close();

                watch.Stop();

                TimeSpan s = DateTime.Now - old;
                Console.WriteLine(String.Format("Lucene.Net 插入{0}行数据,共{1}字符,用时{2}秒 分词用时{3}秒",
                    docId, totalChars, watch.ElapsedMilliseconds / 1000 + "." + watch.ElapsedMilliseconds % 1000,
                    Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer.Duration / 1000 + "." +
                    Lucene.Net.Analysis.KTDictSeg.KTDictSegAnalyzer.Duration % 1000));

                //Begin test performance

                count = 0;
                int loopCount;
                if (Global.Cfg.PerformanceTest)
                {
                    loopCount = 1000;
                }
                else
                {
                    loopCount = 1;
                }

                string queryString = Global.Cfg.QueryString;
                Console.WriteLine("QueryString:" + queryString);

                List<Hubble.Core.Entity.WordInfo> queryWords = new List<Hubble.Core.Entity.WordInfo>();

                KTAnalyzer ktAnalyzer = new KTAnalyzer();

                foreach (Hubble.Core.Entity.WordInfo wordInfo in ktAnalyzer.Tokenize(queryString))
                {
                    queryWords.Add(wordInfo);
                }


                count = 0;
                watch.Reset();
                watch.Start();

                for (int i = 0; i < loopCount; i++)
                {
                    count = Index.GetTotalCount(queryWords);
                }
                watch.Stop();
                Console.WriteLine("查询出来的文章总数:" + count.ToString());

                Console.WriteLine("仅搜索不取记录数据");
                Console.WriteLine("单次的查询时间:" + ((double)watch.ElapsedMilliseconds / loopCount).ToString() + "ms");


                Console.WriteLine("取前100条记录");

                count = 0;
                watch.Reset();
                watch.Start();
                for (int i = 0; i < loopCount; i++)
                {
                    foreach (Hubble.Core.Query.DocumentRank docRank in Index.GetRankEnumerable(queryWords))
                    {
                        count++;

                        if (count >= 100)
                        {
                            break;
                        }
                    }
                }
                watch.Stop();
                Console.WriteLine("单次的查询时间:" + ((double)watch.ElapsedMilliseconds / loopCount).ToString() + "ms");



                StringBuilder report = new StringBuilder();
                count = 0;

                watch.Reset();
                watch.Start();

                for (int i = 0; i < loopCount; i++)
                {
                    foreach (Hubble.Core.Query.DocumentRank docRank in Index.GetRankEnumerable(queryWords, true))
                    {
                        if (!Global.Cfg.PerformanceTest)
                        {
                            string content = documentList[(int)docRank.DocumentId].Attributes["Content"].Value.Replace("\r\n", "");
                            string title = documentList[(int)docRank.DocumentId].Attributes["Title"].Value;
                            KTDictSeg.HighLight.SimpleHTMLFormatter simpleHTMLFormatter =
                               new KTDictSeg.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");

                            KTDictSeg.HighLight.Highlighter highlighter =
                                new KTDictSeg.HighLight.Highlighter(simpleHTMLFormatter,
                                new Lucene.Net.Analysis.KTDictSeg.KTDictSegTokenizer());

                            highlighter.FragmentSize = 100;

                            Console.WriteLine(docRank);
                            report.AppendLine("Title:" + title);
                            report.AppendLine("</br>");
                            report.AppendLine(highlighter.GetBestFragment(queryString, content));
                            report.AppendLine("</br>");
                            report.AppendLine("</br>");

                            if (count > 10)
                            {
                                using (FileStream fs = new FileStream("lucene.htm", FileMode.Create, FileAccess.ReadWrite))
                                {
                                    TextWriter w = new StreamWriter(fs, Encoding.UTF8);
                                    w.Write(report.ToString());
                                    w.Flush();
                                    w.Close();
                                }

                                break;
                            }

                        }

                        count++;
                    }
                }

                watch.Stop();


                Console.WriteLine("取所有记录");
                Console.WriteLine("单次的查询时间:" + ((double)watch.ElapsedMilliseconds / loopCount).ToString() + "ms");

                Console.WriteLine("查询出来的文章总数:" + (count / loopCount).ToString());

            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.Message);
            }
        }
    }
}
