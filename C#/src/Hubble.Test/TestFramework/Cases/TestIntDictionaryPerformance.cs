using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TestFramework.Cases
{
    class TestIntDictionaryPerformance : TestCaseBase
    {
        private void TestIDictionary(IDictionary<int, int> iDict, int maxNum)
        {
            _Report.AppendFormat("Test {0}\r\n", iDict.GetType().Name);

            Stopwatch stopwatch = new Stopwatch();

            //Test dictinary
            //Dictionary<int, int> _Dict = new Dictionary<int, int>();
            IDictionary<int, int> _Dict = iDict;

            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < maxNum; i++)
            {
                _Dict.Add((int)i, i);
            }
            stopwatch.Stop();
            _Report.AppendFormat("Add ElapsedMilliseconds {0}\r\n", stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < maxNum; i++)
            {
                if (i != _Dict[(int)i])
                {
                    _Report.AppendFormat("this[] fail i= {0}\r\n", i);
                }
            }
            stopwatch.Stop();
            _Report.AppendFormat("this[] ElapsedMilliseconds {0}\r\n", stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < maxNum; i++)
            {
                if (!_Dict.ContainsKey((int)i))
                {
                    _Report.AppendFormat("ContainsKey fail i= {0}\r\n", i);
                }
            }
            stopwatch.Stop();
            _Report.AppendFormat("ContainsKey ElapsedMilliseconds {0}\r\n", stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < maxNum; i++)
            {
                if (!_Dict.Remove((int)i))
                {
                    _Report.AppendFormat("Remove fail i= {0}\r\n", i);
                }
            }

            stopwatch.Stop();
            _Report.AppendFormat("Remove ElapsedMilliseconds {0}\r\n", stopwatch.ElapsedMilliseconds);
        }

        public override void Test()
        {
            const int MaxNum = 1 * 1024 * 1024;

            TestIDictionary(new Dictionary<int, int>(), MaxNum);
            TestIDictionary(new SortedDictionary<int, int>(), MaxNum);
            TestIDictionary(new Hubble.Framework.Arithmetic.IntDictionary<int>(), MaxNum);
        }
    }
}
