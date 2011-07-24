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
using System.Runtime.InteropServices;

namespace Hubble.Framework.IO
{
    /// <summary>
    /// Alloc memory from HGlobal
    /// </summary>
    public class HGlobalMemoryBlock : IDisposable
    {
        protected IntPtr Ptr = IntPtr.Zero;

        int _Size;

        public int Size
        {
            get
            {
                return _Size;
            }
        }

        public static explicit operator IntPtr (HGlobalMemoryBlock mb)
        {
            return mb.Ptr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">bytes for alloc</param>
        public HGlobalMemoryBlock(int size)
        {
            Ptr = Marshal.AllocHGlobal(size);
            _Size = size;
        }

        ~HGlobalMemoryBlock()
        {
            try
            {
                if (Ptr != IntPtr.Zero)
                {
                    Dispose();
                }
            }
            catch
            {
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            Marshal.FreeHGlobal(Ptr);
            Ptr = IntPtr.Zero;
        }

        #endregion
    }
}
