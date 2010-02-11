using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Hubble.Framework.IO
{
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
