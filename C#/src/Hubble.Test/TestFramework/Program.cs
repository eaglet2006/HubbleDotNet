using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TestFramework
{
    class Program
    {
        static void Main(string[] args)
        {
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
