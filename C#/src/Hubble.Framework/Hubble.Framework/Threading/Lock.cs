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
using System.Threading;
using System.Diagnostics;

namespace Hubble.Framework.Threading
{
    /// <summary>
    /// Share or Mutex lock
    /// </summary>
    public class Lock
    {
        public enum Mode
        {
            Share = 0,
            Mutex = 1,
        }

        ReaderWriterLockSlim _RWL = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public bool Enter(Mode mode)
        {
            switch (mode)
            {
                case Mode.Share:
                    return _RWL.TryEnterReadLock(-1);
                case Mode.Mutex:
                    return _RWL.TryEnterWriteLock(-1);
            }

            return false;
        }

        /// <summary>
        /// Enter lock
        /// </summary>
        /// <param name="mode">Share or mutex</param>
        /// <param name="timeout">how many milliseconds waitting for. If timeout less than 0, wait until enter lock</param>
        public bool Enter(Mode mode, int timeout)
        {
            switch (mode)
            {
                case Mode.Share:
                    return _RWL.TryEnterReadLock(timeout);
                case Mode.Mutex:
                    return _RWL.TryEnterWriteLock(timeout);
            }

            return false;
           
        }

        public void Leave(Mode mode)
        {
            switch (mode)
            {
                case Mode.Share:
                    _RWL.ExitReadLock();
                    break;
                case Mode.Mutex:
                    _RWL.ExitWriteLock();
                    break;
            }
        }
    }
}
