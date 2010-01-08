using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.Threading
{
    public class MultiThreadCalculate
    {
        List<System.Threading.Thread> _Threads = new List<System.Threading.Thread>();
        List<object> _Paras = new List<object>();

        System.Threading.Semaphore sema;
        System.Threading.ParameterizedThreadStart _ThreadProc;

        public MultiThreadCalculate(System.Threading.ParameterizedThreadStart threadStart)
        {
            _ThreadProc = threadStart;
        }

        public void Add(object para)
        {
            System.Threading.Thread thread = new System.Threading.Thread(ThreadProc);
            thread.IsBackground = true;
            
            _Paras.Add(para);
            _Threads.Add(thread);
        }

        public void Start()
        {
            sema = new System.Threading.Semaphore(0, _Threads.Count);

            for (int i = 0; i < _Threads.Count; i++)
            {
                _Threads[i].Start(_Paras[i]);
            }

            //Waitting
            for (int i = 0; i < _Threads.Count; i++)
            {
                sema.WaitOne();
            }
        }

        void ThreadProc(object para)
        {
            _ThreadProc(para);
            sema.Release();
        }

    }
}
