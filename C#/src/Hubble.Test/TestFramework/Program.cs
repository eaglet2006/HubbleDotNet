using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TestFramework
{
    class Program
    {
        static List<string> GenerateStringList(int len, int count)
        {
            StringBuilder s = new StringBuilder();
            Random rand = new Random();
            List<string> list = new List<string>();

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    s.Append((char)rand.Next(1, 127));
                }

                list.Add(s.ToString());

                s = new StringBuilder();
            }

            return list;
        }

        static void Main(string[] args)
        {
            Hubble.Framework.DataStructure.FingerPrintDictionary<int> fd =
                new Hubble.Framework.DataStructure.FingerPrintDictionary<int>();

            Dictionary<string, int> sd = new Dictionary<string, int>();

            List<string> list;

            //list = GenerateStringList(128, 10000000);
            
            int count = 2000000;
            int len = 128;

            CodeTimer.Time("Test Dictionay Add 128", 1, delegate()
            {
                StringBuilder s = new StringBuilder();
                Random rand = new Random();
                //List<string> list = new List<string>();

                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < len; j++)
                    {
                        s.Append((char)rand.Next(1, 127));
                    }

                    //list.Add(s.ToString());
                    sd.Add(s.ToString(), 0);
                    s = new StringBuilder();
                }


                //foreach (string s in list)
                //{
                //    sd.Add(s, 0);
                //}
            });

            Console.WriteLine("Press any key");
            Console.ReadKey();
            sd = null;

            GC.Collect();

            Console.ReadKey();

            CodeTimer.Time("Test FingerPrint Add 128", 1, delegate()
            {
                StringBuilder s = new StringBuilder();
                Random rand = new Random();
                //List<string> list = new List<string>();

                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < len; j++)
                    {
                        s.Append((char)rand.Next(1, 127));
                    }

                    //list.Add(s.ToString());
                    fd.Add(s.ToString(), 0);
                    s = new StringBuilder();
                }

                //foreach (string s in list)
                //{
                //    fd.Add(s, 0);
                //}
            });

            Console.ReadKey();

            //int v;
            //CodeTimer.Time("Test Dictionay GetValue 128", 1, delegate()
            //{
            //    foreach (string s in list)
            //    {
            //        sd.TryGetValue(s, out v);
            //    }
            //});

            //CodeTimer.Time("Test FingerPrint Add 128", 1, delegate()
            //{
            //    foreach (string s in list)
            //    {
            //        fd.TryGetValue(s, out v);
            //    }
            //});

            Console.ReadKey();

            return;

            List<TestCaseBase> testCases = new List<TestCaseBase>();

            //Add test cases
            //testCases.Add(new Cases.TestLinkedSegmentFileStream());
            //testCases.Add(new Cases.TestSingleSortedLinkedTable());
            //testCases.Add(new Cases.TestCompressIntList());
            //testCases.Add(new Cases.TestIntDictionary());
            //testCases.Add(new Cases.TestIntDictionaryPerformance());
            testCases.Add(new Cases.TestMessageQueue());

            //Test
            foreach (TestCaseBase testCase in testCases)
            {
                testCase.Test();

                Console.WriteLine(testCase.Report);
            }

            GC.Collect();

            Console.ReadKey();
        }
    }
}
