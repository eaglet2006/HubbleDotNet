using System;
using System.Collections.Generic;
using System.Text;

namespace TestHubbleCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dictionary<int, int> test = new Dictionary<int, int>();

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

            TestHubble testHubble = new TestHubble();
            //testHubble.NewsXml = @"G:\opensource\KTDictSeg\News\news.xml";
            testHubble.Test();

            Console.ReadKey();
            GC.Collect();
            Console.ReadKey();

            TestLucene testLucene = new TestLucene();
            testLucene.NewsXml = @"G:\opensource\KTDictSeg\News\news.xml";
            testLucene.Test();
            Console.ReadKey();

        }
    }

}
