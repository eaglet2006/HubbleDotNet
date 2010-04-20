/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


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

        public void Start(int maxThread)
        {
            sema = new System.Threading.Semaphore(0, _Threads.Count);

            int finishTreads = 0;

            int count = _Threads.Count;

            maxThread = Math.Min(maxThread, count);

            int i = 0;
            for (i = 0; i < maxThread; i++)
            {
                _Threads[i].Start(_Paras[i]);
            }

            while (finishTreads < count)
            {
                sema.WaitOne();
                finishTreads++;

                if (i < count)
                {
                    _Threads[i].Start(_Paras[i]);
                    i++;
                }
            }

            //for (int i = 0; i < _Threads.Count; i++)
            //{
            //    _Threads[i].Start(_Paras[i]);
            //}

            //Waitting
            //for (int i = 0; i < _Threads.Count; i++)
            //{
            //    sema.WaitOne();
            //}
        }

        void ThreadProc(object para)
        {
            _ThreadProc(para);
            sema.Release();
        }

    }
}
