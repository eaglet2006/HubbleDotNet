using System;
using System.Collections.Generic;
using System.Text;

namespace TestFramework.Cases
{
    class TestIntDictionary : TestCaseBase
    {
        private int TestAddContainGetCountOneKey(Hubble.Framework.Arithmetic.IntDictionary<int> intDict, int key, int value, int count)
        {
            intDict.Add(key, value);
            AssignEquals(intDict.ContainsKey(key), true, "Test ContainsKey");
            AssignEquals(intDict[key], value, "Test this[int key]");
            intDict[key] = value + 4;
            AssignEquals(intDict[key], value + 4, "Test this[int key]");

            count++;
            AssignEquals(intDict.Count, count, "Test Count");
            
            return count;

        }

        private int TestRemoveOneKey(Hubble.Framework.Arithmetic.IntDictionary<int> intDict, int key, int count)
        {
            AssignEquals(intDict.Remove(key), true, "Test ContainsKey");
            AssignEquals(intDict.ContainsKey(key), false, "Test ContainsKey");

            count--;
            AssignEquals(intDict.Count, count, "Test Count");

            return count;
        }


        public override void Test()
        {
            Hubble.Framework.Arithmetic.IntDictionary<int> intDict = new Hubble.Framework.Arithmetic.IntDictionary<int>(4);

            int count = 0;

            KeyValuePair<int, int>[] keyValues = {
                new KeyValuePair<int, int>(1,1),
                new KeyValuePair<int, int>(0,1),
                new KeyValuePair<int, int>(16,110),
                new KeyValuePair<int, int>(3,11110),
                new KeyValuePair<int, int>(101,111110),
                new KeyValuePair<int, int>(17,28),
                new KeyValuePair<int, int>(15,29),
                new KeyValuePair<int, int>(int.MaxValue - 1,4456),
            
            };

            List<int> sortKeyList = new List<int>();

            foreach (KeyValuePair<int, int> k in keyValues)
            {
                count = TestAddContainGetCountOneKey(intDict, k.Key, k.Value, count);
                sortKeyList.Add(k.Key);
            }

            sortKeyList.Sort();
            int i = 0;
            foreach (int key in intDict.Keys)
            {
                AssignEquals(key, sortKeyList[i++], "Test Keys");
            }

            AssignEquals(i, intDict.Count, "Test Keys");

            i = 0;

            foreach (int value in intDict.Values)
            {
                AssignEquals(value, intDict[sortKeyList[i++]], "Test Values");
            }

            AssignEquals(i, intDict.Count, "Test Values");

            foreach (KeyValuePair<int, int> k in keyValues)
            {
                count = TestRemoveOneKey(intDict, k.Key, count);
            }

            count = 0;
            foreach (KeyValuePair<int, int> k in keyValues)
            {
                count = TestAddContainGetCountOneKey(intDict, k.Key, k.Value, count);
                sortKeyList.Add(k.Key);
            }

            intDict.Remove(int.MaxValue - 1);

            for (i = 0; i < 128; i++)
            {
                if (i == 121)
                {
                    intDict.Remove(10);
                    intDict.Remove(0);
                }

                int key = intDict.Add(i);
                AssignEquals(intDict.ContainsKey(key), true, "Test Add");
            }


            intDict = new Hubble.Framework.Arithmetic.IntDictionary<int>();
            count = 0;

            keyValues = new KeyValuePair<int,int>[]{
                new KeyValuePair<int, int>(1,1),
                new KeyValuePair<int, int>(0,1),
                new KeyValuePair<int, int>(16,110),
                new KeyValuePair<int, int>(3,11110),
                new KeyValuePair<int, int>(101,111110),
                new KeyValuePair<int, int>(17,28),
                new KeyValuePair<int, int>(15,29),
                new KeyValuePair<int, int>(int.MaxValue - 1,4456),
            };

            sortKeyList = new List<int>();

            foreach (KeyValuePair<int, int> k in keyValues)
            {
                count = TestAddContainGetCountOneKey(intDict, k.Key, k.Value, count);
                sortKeyList.Add(k.Key);
            }

            sortKeyList.Sort();
            i = 0;

            foreach (int key in intDict.Keys)
            {
                AssignEquals(key, sortKeyList[i++], "Test Keys");
            }

            AssignEquals(i, intDict.Count, "Test Keys");

            i = 0;

            foreach (int value in intDict.Values)
            {
                AssignEquals(value, intDict[sortKeyList[i++]], "Test Values");
            }


            AssignEquals(i, intDict.Count, "Test Values");

        }
    }
}
