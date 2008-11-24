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
            testCases.Add(new Cases.TestCompressIntList());
            //testCases.Add(new Cases.TestIntDictionary());
            //testCases.Add(new Cases.TestIntDictionaryPerformance());

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
