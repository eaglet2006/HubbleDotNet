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
