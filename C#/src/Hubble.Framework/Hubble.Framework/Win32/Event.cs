using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.Win32
{
    public class Event : IDisposable
    {
        IntPtr m_Handle;

        /// <summary>
        /// Constructor
        /// </summary>
        public Event()
            : this(null, true, false, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bManualReset">
        /// If this parameter is TRUE, the function creates a manual-reset event object, which requires the use of the ResetEvent function to set the event state to nonsignaled. If this parameter is FALSE, the function creates an auto-reset event object, and system automatically resets the event state to nonsignaled after a single waiting thread has been released.
        /// </param>
        /// <param name="bInitialState">
        /// If this parameter is TRUE, the initial state of the event object is signaled; otherwise, it is nonsignaled.
        /// </param>
        public Event(bool bManualReset, bool bInitialState)
            : this(null, bManualReset, bInitialState, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lpEventAttributes">
        /// A pointer to a SECURITY_ATTRIBUTES structure. If this parameter is NULL, the handle cannot be inherited by child processes.
        /// The lpSecurityDescriptor member of the structure specifies a security descriptor for the new event. If lpEventAttributes is NULL, the event gets a default security descriptor. The ACLs in the default security descriptor for an event come from the primary or impersonation token of the creator.
        /// </param>
        /// <param name="bManualReset">
        /// If this parameter is TRUE, the function creates a manual-reset event object, which requires the use of the ResetEvent function to set the event state to nonsignaled. If this parameter is FALSE, the function creates an auto-reset event object, and system automatically resets the event state to nonsignaled after a single waiting thread has been released.
        /// </param>
        /// <param name="bInitialState">
        /// If this parameter is TRUE, the initial state of the event object is signaled; otherwise, it is nonsignaled.
        /// </param>
        /// <param name="lpName">
        /// The name of the event object. The name is limited to MAX_PATH characters. Name comparison is case sensitive.
        /// </param>
        public Event(SecurityAttributes lpEventAttributes, bool bManualReset, bool bInitialState, string lpName)
        {
            m_Handle = NTKernel.CreateEvent(lpEventAttributes, bManualReset, bInitialState, lpName);

            if (m_Handle == IntPtr.Zero)
            {
                int err = NTKernel.GetLastError();
                throw new Exception(String.Format("Create Event fail, error={0}",
                    err));
            }
        }

        public bool Open(EventAccess dwDesiredAccess, bool bInheritHandle, string lpName)
        {
            m_Handle = NTKernel.OpenEvent((int)dwDesiredAccess, bInheritHandle, lpName);

            if (m_Handle == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Timeout 
        /// </summary>
        /// <param name="dwMilliseconds"></param>
        /// <returns></returns>
        public WaitForState WaitFor(int dwMilliseconds)
        {
            return (WaitForState)NTKernel.WaitForSingleObject((int)m_Handle, dwMilliseconds);
        }

        public WaitForState WaitFor()
        {
            return WaitFor(NTKernel.INFINITE);
        }

        public void SetEvent()
        {
            NTKernel.SetEvent(m_Handle);
        }

        public void Release()
        {
            NTKernel.ResetEvent(m_Handle);
        }

        public void Close()
        {
            if (m_Handle != IntPtr.Zero)
            {
                if (NTKernel.CloseHandle((int)m_Handle))
                {
                    m_Handle = IntPtr.Zero;
                }
            }
        }

        ~Event()
        {
            Dispose();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }

}
