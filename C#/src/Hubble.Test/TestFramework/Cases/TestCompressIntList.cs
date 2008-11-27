using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Hubble.Framework.DataType;
using Hubble.Framework.Arithmetic;

namespace TestFramework.Cases
{
    class TestCompressIntList : TestCaseBase
    {
        public override void Test()
        {
            int[] testData = { 6, 8, 9, 10, 256, 4836, 76000, 16777216, 16777217};

            IntDictionary<CompressIntList> compressIntDict = new IntDictionary<CompressIntList>();
            IntDictionary<CCompressIntList> ccompressIntDict = new IntDictionary<CCompressIntList>();

            List<int> input = new List<int>();

            foreach(int value in testData)
            {
                input.Add(value);
            }

            CompressIntList test = new CompressIntList(input, 0);

            int j = 0;
            foreach (int value in test)
            {
                AssignEquals(testData[j], value, "Test values");
                j++;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            for (int i = 0; i < 1 * 1024 * 1024; i++)
            {
                CCompressIntList ccompressList = new CCompressIntList(input);
                ccompressIntDict.Add(ccompressList);
            }
            stopwatch.Stop();
            _Report.AppendFormat("ElapsedMilliseconds {0}\r\n", stopwatch.ElapsedMilliseconds);

            ccompressIntDict.Clear();
            ccompressIntDict = null;
            GC.Collect();

            stopwatch.Reset();
            stopwatch.Start();

            for (int i = 0; i < 1 * 1024 * 1024; i++)
            {
                CompressIntList compressList = new CompressIntList(input, 0);
                compressIntDict.Add(compressList);
            }
            stopwatch.Stop();
            _Report.AppendFormat("ElapsedMilliseconds {0}\r\n", stopwatch.ElapsedMilliseconds);


        }
    }
}
