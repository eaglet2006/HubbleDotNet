using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;

using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;

namespace TestHubbleCore
{
    class TestHubble
    {
        public void Test(List<XmlNode> documentList)
        {
            try
            {
                Hubble.Core.Index.InvertedIndex invertedIndex = new Hubble.Core.Index.InvertedIndex();

                KTAnalyzer ktAnalyzer = new KTAnalyzer();

                Stopwatch watch = new Stopwatch();
                ktAnalyzer.Stopwatch.Reset();
                long docId = 0;
                int totalChars = 0;
                int contentCount = 0;
                foreach (XmlNode node in documentList)
                {
                    String title = node.Attributes["Title"].Value;
                    DateTime time = DateTime.Parse(node.Attributes["Time"].Value);
                    String Url = node.Attributes["Url"].Value;
                    String content = node.Attributes["Content"].Value;

                    totalChars += content.Length;

                    watch.Start();

                    if (Global.Cfg.TestShortText)
                    {
                        for (int i = 0; i < content.Length / 100; i++)
                        {
                            invertedIndex.Index(content.Substring(i * 100, 100), docId++, ktAnalyzer);
                        }
                    }
                    else
                    {
                        invertedIndex.Index(content, docId++, ktAnalyzer);
                    }
                    watch.Stop();
                    contentCount++;

                    if (contentCount == Global.Cfg.TestRows)
                    {
                        break;
                    }
                }

                watch.Start();
                watch.Stop();

                long indexElapsedMilliseconds = watch.ElapsedMilliseconds - ktAnalyzer.Stopwatch.ElapsedMilliseconds;

                Console.WriteLine(String.Format("Hubble.Net 插入{0}行数据,共{1}字符,用时{2}秒 分词用时{3}秒 索引时间{4}秒",
                    docId, totalChars, watch.ElapsedMilliseconds / 1000 + "." + watch.ElapsedMilliseconds % 1000,
                    ktAnalyzer.Stopwatch.ElapsedMilliseconds / 1000 + "." + ktAnalyzer.Stopwatch.ElapsedMilliseconds % 1000,
                    indexElapsedMilliseconds / 1000 + "." + indexElapsedMilliseconds % 1000
                    ));

                //Begin test performance
                int count = 0;

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

                foreach (Hubble.Core.Entity.WordInfo wordInfo in ktAnalyzer.Tokenize(queryString))
                {
                    queryWords.Add(wordInfo);
                }

                StringBuilder report = new StringBuilder();


                Console.WriteLine("取前100条记录");

                count = 0;
                watch.Reset();
                watch.Start();
                for (int i = 0; i < loopCount; i++)
                {
                    Hubble.Core.Query.IQuery query;

                    if (Global.Cfg.TestFullTextQuery)
                    {
                        query = new Hubble.Core.Query.FullTextQuery();
                    }
                    else
                    {
                        query = new Hubble.Core.Query.MutiStringQuery();
                    }

                    //Hubble.Core.Query.IQuery query = new Hubble.Core.Query.MutiStringQuery();
                    //Hubble.Core.Query.IQuery query = new Hubble.Core.Query.FullTextQuery();

                    query.FieldName = "";
                    query.InvertedIndex = invertedIndex;
                    query.QueryWords = queryWords;

                    Hubble.Core.Query.Searcher searcher = new Hubble.Core.Query.Searcher(query);

                    foreach (Hubble.Core.Query.DocumentRank docRank in searcher.Get(0, 100))
                    {
                        count++;

                        if (count >= 100)
                        {
                            Console.Write("");
                            break;
                        }
                    }
                }
                watch.Stop();

                Console.WriteLine("单次的查询时间:" + ((double)watch.ElapsedMilliseconds / loopCount).ToString() + "ms");
                
                count = 0;
                
                watch.Reset();
                watch.Start();

                for (int i = 0; i < loopCount; i++)
                {
                    Hubble.Core.Query.IQuery query;

                    if (Global.Cfg.TestFullTextQuery)
                    {
                        query = new Hubble.Core.Query.FullTextQuery();
                    }
                    else
                    {
                        query = new Hubble.Core.Query.MutiStringQuery();
                    }

                    //Hubble.Core.Query.IQuery query = new Hubble.Core.Query.MutiStringQuery();
                    //Hubble.Core.Query.IQuery query = new Hubble.Core.Query.FullTextQuery();

                    query.FieldName = "";
                    query.InvertedIndex = invertedIndex;
                    query.QueryWords = queryWords;

                    Hubble.Core.Query.Searcher searcher = new Hubble.Core.Query.Searcher(query);


                    foreach (Hubble.Core.Query.DocumentRank docRank in searcher.Get())
                    {
                        if (!Global.Cfg.PerformanceTest)
                        {
                            string content = documentList[(int)docRank.DocumentId].Attributes["Content"].Value.Replace("\r\n", "");

                            int index = content.IndexOf(queryString);

                            if (index >= 0)
                            {
                                int fst = Math.Max(0, index - 20);
                                int len = Math.Min(content.Length - fst, 100);
                                content = content.Substring(fst, len);
                            }

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

                            if (count > 25)
                            {
                                using (FileStream fs = new FileStream("hubble.htm", FileMode.Create, FileAccess.ReadWrite))
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
                Console.WriteLine(e1.StackTrace);
            }
        }
    }
}
