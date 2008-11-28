using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;

namespace TestFramework.Cases
{
    class TestSingleSortedLinkedTable : TestCaseBase
    {
        class LongComparer : IComparer<long>
        {
            #region IComparer<long> Members

            public int Compare(long x, long y)
            {
                return x.CompareTo(y);
            }

            #endregion
        }

        public override void Test()
        {
            long[] testData = { 6, 8, 8, 5, 4, 7, 9, 10, 256, 256, 256, 256, 4, 4, 6, 4836, 76000, 16777216, 16777217, 9, 16777217, 16777217, 16777217 };

            SingleSortedLinkedTable<long, long> sortedLinked = new SingleSortedLinkedTable<long, long>(new LongComparer());

            List<long> input = new List<long>();

            foreach (long value in testData)
            {
                input.Add(value);
                sortedLinked.Add(value, value);
            }

            input.Sort();

            int j = 0;
            foreach (SingleSortedLinkedTable<long, long>.Node node in sortedLinked.GetAll())
            {
                AssignEquals(input[j], node.Key, "Test GetAll");
                j++;
            }

            j = 0;
            foreach (SingleSortedLinkedTable<long, long>.Node node in sortedLinked.GetFirstKeys())
            {
                Console.WriteLine(node.Value);
                AssignEquals(input[j], node.Key, "Test GetFirstKeys");
                j++;
            }

            sortedLinked.RemoveFirstKeys();
            foreach (SingleSortedLinkedTable<long, long>.Node node in sortedLinked.GetAll())
            {
                Console.WriteLine(node.Value);
            }

            sortedLinked.RemoveFirstKeys();
            foreach (SingleSortedLinkedTable<long, long>.Node node in sortedLinked.GetAll())
            {
                Console.WriteLine(node.Value);
            }

            sortedLinked.RemoveFirstKeys();
            foreach (SingleSortedLinkedTable<long, long>.Node node in sortedLinked.GetAll())
            {
                Console.WriteLine(node.Value);
            }
        }
    }
}
