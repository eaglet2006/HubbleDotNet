using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Hubble.Core.Query;

namespace TestHubbleCore
{
    class Program
    {
        static void Main(string[] args)
        {
            List<DocumentRank> docList = new List<DocumentRank>();
            Random rand = new Random();
            DocRankRadixSortedList docRankList = new DocRankRadixSortedList();
            docRankList.Top = 100;

            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();

            s.Reset();
            s.Start();

            while (docRankList.Count < 1000000)
            {
                DocumentRank v = new DocumentRank(docRankList.Count, rand.Next(0, 10000));
                docRankList.Add(v);
            }

            foreach (DocumentRank d in docRankList)
            {
                //docList.Add(d);
            }

            //while (docList.Count < 1000000)
            //{
            //    DocumentRank v = new DocumentRank(docList.Count, rand.Next(0, 1000000));
            //    docList.Add(v);
            //}


            //docList.Sort();
            s.Stop();
            Console.WriteLine(s.ElapsedMilliseconds);
            Console.WriteLine(s.ElapsedMilliseconds);

            //Hubble.Framework.Arithmetic.IntDictionary<int> test = new Hubble.Framework.Arithmetic.IntDictionary<int>(64);

            //foreach (int v in testList)
            //{
            //    test.SortInsert(v, v);
            //}

            //s.Stop();

            //Console.WriteLine(s.ElapsedMilliseconds);

            //s.Reset();
            //s.Start();

            //testList.Sort();

            //s.Stop();

            //Console.WriteLine(s.ElapsedMilliseconds);

            

            //System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            
            //s.Reset();
            //s.Start();

            //for (int i = 0; i < 10 * 1024 * 1024; i++)
            //{
            //    test.Add(i, i);
            //}
            //s.Stop();
            //Console.WriteLine(s.ElapsedMilliseconds);

            //test.Clear();

            //s.Reset();
            //s.Start();

            //for (int i = 0; i < 10 * 1024 * 1024; i++)
            //{
            //    test.Add(i, i);
            //}
            //s.Stop();
            //Console.WriteLine(s.ElapsedMilliseconds);

            //test.Clear();

            //s.Reset();
            //s.Start();

            //for (int i = 0; i < 10 * 1024 * 1024; i++)
            //{
            //    test.Add(i, i);
            //}
            //s.Stop();
            //Console.WriteLine(s.ElapsedMilliseconds);

            Global.Cfg = new Config();
            Global.Cfg.FileName = Environment.CurrentDirectory + @"\Test.ini";
            Global.Cfg.ReadOnly = true;
            Global.Cfg.Open();
            Global.Cfg.Close();

            try
            {
                string NewsXml = Global.Cfg.NewsXmlFilePath;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(NewsXml);
                XmlNodeList nodes = xmlDoc.SelectNodes(@"News/Item");
                List<XmlNode> documentList = new List<XmlNode>();

                foreach (XmlNode node in nodes)
                {
                    documentList.Add(node);
                }

                if (Global.Cfg.TestHubble)
                {
                    TestHubble testHubble = new TestHubble();
                    testHubble.Test(documentList);
                }

                GC.Collect();

                if (Global.Cfg.TestLucene)
                {
                    TestLucene testLucene = new TestLucene();
                    testLucene.Test(documentList);
                }

                while (true)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey();

                    if (keyInfo.KeyChar == 'h')
                    {
                        TestHubble testHubble = new TestHubble();

                        testHubble.Test(documentList);

                        Console.ReadKey();
                        GC.Collect();
                    }
                    else if (keyInfo.KeyChar == 'l')
                    {

                        TestLucene testLucene = new TestLucene();

                        testLucene.Test(documentList);

                    }
                    else if (keyInfo.KeyChar == 'q')
                    {
                        Console.WriteLine("Please input the document id");
                        string docId = Console.ReadLine();

                        try
                        {
                            int id = int.Parse(docId);
                            Console.WriteLine(documentList[id].Attributes["Title"].Value);
                            Console.WriteLine(documentList[id].Attributes["Time"].Value);
                            Console.WriteLine(documentList[id].Attributes["Url"].Value);
                            Console.WriteLine(documentList[id].Attributes["Content"].Value);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
            catch(Exception e)

            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }
    }

}
