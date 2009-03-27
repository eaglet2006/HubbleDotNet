using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace TestFramework
{

    public class TestSleep3000 : IAction
    {
        #region IAction Members

        public void Action()
        {
            Thread.Sleep(3000);
        }

        #endregion
    }


    public class TestEmptyMethod : IAction
    {
        #region IAction Members

        public void Action()
        {
        }

        #endregion
    }

    public class TestStringConcat : IAction
    {
        string s = "";

        #region IAction Members

        public void Action()
        {
            s += "a";
        }

        #endregion
    }

    public class TestStringBuilderConcat : IAction
    {
        StringBuilder s = new StringBuilder();

        #region IAction Members

        public void Action()
        {
            s.Append("a");
        }

        #endregion
    }

    public interface IAction
    {
        void Action();
    }

    public static class CodeTimer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime,
           out long lpExitTime, out long lpKernelTime, out long lpUserTime);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        public delegate void ActionDelegate();

        private static long GetCurrentThreadTimes()
        {
            long l;
            long kernelTime, userTimer;
            GetThreadTimes(GetCurrentThread(), out l, out l, out kernelTime, out userTimer);
            return kernelTime + userTimer;
        }

        static CodeTimer()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

        }

        public static void Time(string name, int iteration, ActionDelegate action)
        {
            if (String.IsNullOrEmpty(name))
            {
                return;
            }

            if (action == null)
            {
                return;
            }

            //1. Print name
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(name);


            // 2. Record the latest GC counts
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.Collect(GC.MaxGeneration);
            int[] gcCounts = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }

            // 3. Run action
            Stopwatch watch = new Stopwatch();
            watch.Start();
            long ticksFst = GetCurrentThreadTimes(); //100 nanosecond one tick

            for (int i = 0; i < iteration; i++) action();
            long ticks = GetCurrentThreadTimes() - ticksFst;
            watch.Stop();

            // 4. Print CPU
            Console.ForegroundColor = currentForeColor;
            Console.WriteLine("\tTime Elapsed:\t\t" + watch.ElapsedMilliseconds.ToString("N0") + "ms");
            Console.WriteLine("\tTime Elapsed (one time):" + (watch.ElapsedMilliseconds / iteration).ToString("N0") + "ms");

            Console.WriteLine("\tCPU time:\t\t" + (ticks * 100).ToString("N0") + "ns");
            Console.WriteLine("\tCPU time (one time):\t" + (ticks * 100 / iteration).ToString("N0") + "ns");

            // 5. Print GC
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                int count = GC.CollectionCount(i) - gcCounts[i];
                Console.WriteLine("\tGen " + i + ": \t\t\t" + count);
            }

            Console.WriteLine();

        }

        public static void Time(string name, int iteration, IAction action)
        {
            if (String.IsNullOrEmpty(name))
            {
                return;
            }

            if (action == null)
            {
                return;
            }

            //1. Print name
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(name);


            // 2. Record the latest GC counts
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.Collect(GC.MaxGeneration);
            int[] gcCounts = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }

            // 3. Run action
            Stopwatch watch = new Stopwatch();
            watch.Start();
            long ticksFst = GetCurrentThreadTimes(); //100 nanosecond one tick

            for (int i = 0; i < iteration; i++) action.Action();
            long ticks = GetCurrentThreadTimes() - ticksFst;
            watch.Stop();

            // 4. Print CPU
            Console.ForegroundColor = currentForeColor;
            Console.WriteLine("\tTime Elapsed:\t\t" + watch.ElapsedMilliseconds.ToString("N0") + "ms");
            Console.WriteLine("\tTime Elapsed (one time):" + (watch.ElapsedMilliseconds / iteration).ToString("N0") + "ms");

            Console.WriteLine("\tCPU time:\t\t" + (ticks * 100).ToString("N0") + "ns");
            Console.WriteLine("\tCPU time (one time):\t" + (ticks * 100 / iteration).ToString("N0") + "ns");

            // 5. Print GC
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                int count = GC.CollectionCount(i) - gcCounts[i];
                Console.WriteLine("\tGen " + i + ": \t\t\t" + count);
            }

            Console.WriteLine();

        }
    }

}
