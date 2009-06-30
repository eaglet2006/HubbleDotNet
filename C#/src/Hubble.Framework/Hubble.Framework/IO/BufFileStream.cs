using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Security;
#if !FEATURE_PAL
using System.Security.AccessControl;
#endif
using System.Security.Permissions;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Versioning;
using System.IO;

namespace Hubble.Framework.IO
{
    public class BufFileStream : System.IO.FileStream
    {
        private long _BufPosition = 0;
        private long _BeginPosition = 0;
        private int _Current = -1;
        private int _CurrentCount = 0;
        private int _BufSize = 1024;
        private byte[] _Buf = new byte[1024];

        #region public properties

        /// <summary>
        /// Buf size
        /// </summary>
        public int BufSize
        {
            get
            {
                return _BufSize;
            }

            set
            {
                _BufSize = value;
            }
        }

        #endregion

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(String path, FileMode mode)
            : base(path, mode)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(String path, FileMode mode, FileAccess access)
            : base(path, mode, access)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(String path, FileMode mode, FileAccess access, FileShare share)
            : base(path, mode, access, share)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(String path, FileMode mode, FileAccess access, FileShare share,
            int bufferSize)
            : base(path, mode, access, share, bufferSize)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(String path, FileMode mode, FileAccess access, FileShare share,
            int bufferSize, FileOptions options)
            : base(path, mode, access, share, bufferSize, options)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(String path, FileMode mode, FileAccess access, FileShare share,
            int bufferSize, bool useAsync)
            : base(path, mode, access, share, bufferSize, useAsync)
        {
        }
        public BufFileStream(String path, FileMode mode, FileSystemRights rights, FileShare share,
            int bufferSize, FileOptions options, FileSecurity fileSecurity)
            : base(path, mode, rights, share, bufferSize, options, fileSecurity)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(String path, FileMode mode, FileSystemRights rights, FileShare share,
            int bufferSize, FileOptions options)
            : base(path, mode, rights, share, bufferSize, options)
        {
        }

        [Obsolete("This constructor has been deprecated.  Please use new BufFileStream(SafeFileHandle handle, FileAccess access) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(IntPtr handle, FileAccess access)
            : base(handle, access)
        {
        }

        [Obsolete("This constructor has been deprecated.  Please use new BufFileStream(SafeFileHandle handle, FileAccess access) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(IntPtr handle, FileAccess access, bool ownsHandle)
            : base(handle, access, ownsHandle)
        {
        }

        [Obsolete("This constructor has been deprecated.  Please use new BufFileStream(SafeFileHandle handle, FileAccess access, int bufferSize) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
            : base(handle, access, ownsHandle, bufferSize)
        {
        }

        [Obsolete("This constructor has been deprecated.  Please use new BufFileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(IntPtr handle, FileAccess access, bool ownsHandle,
            int bufferSize, bool isAsync)
            : base(handle, access, ownsHandle, bufferSize, isAsync)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(SafeFileHandle handle, FileAccess access)
            : base(handle, access)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public BufFileStream(SafeFileHandle handle, FileAccess access, int bufferSize)
            : base(handle, access, bufferSize)
        {
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public BufFileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
            : base(handle, access, bufferSize, isAsync)
        {
        }


        #region override methods

        new public long Position
        {
            get
            {
                if (_Current < 0)
                {
                    return base.Position;
                }

                return _BeginPosition + _Current;
            }
        }

        new public long Seek(long offset, SeekOrigin origin)
        {
            long goPosition = 0;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    goPosition = offset;
                    break;
                case SeekOrigin.Current:
                    goPosition = Position + offset;
                    break;
                case SeekOrigin.End:
                    goPosition = base.Length + offset;
                    break;
            }

            if (goPosition >= _BeginPosition + _CurrentCount || goPosition < _BeginPosition || _Current < 0)
            {
                _Current = -1;
                return base.Seek(offset, origin);
            }
            else
            {
                _Current = (int)(goPosition - _BeginPosition);
                return Position;
            }
        }

        new public int ReadByte()
        {

            byte[] buf = new byte[1];
            if (Read(buf, 0, 1) <= 0)
            {
                if (Read(buf, 0, 1) <= 0)
                {
                    throw new IOException("End of file");
                }
            }

            return buf[0];
        }

        new public int Read(byte[] array, int offset, int count)
        {
            if (_Buf.Length != _BufSize)
            {
                _Buf = new byte[_BufSize];
                _BufPosition = 0;
                _Current = -1;
            }

            if (_Current < 0 || _Current >= _CurrentCount)
            {
                _Current = 0;
                _BeginPosition = base.Position;
                _CurrentCount = base.Read(_Buf, 0, _Buf.Length);
                _BufPosition = base.Position;

                if (_CurrentCount < 0)
                {
                    return _CurrentCount;
                }
            }

            int remain = _CurrentCount - _Current;
            int read = remain < count ? remain : count;
            if (read <= 8)
            {
                int byteCount = read;
                while (--byteCount >= 0)
                    array[offset + byteCount] = _Buf[_Current + byteCount];
            }
            else
            {
                Array.Copy(_Buf, _Current, array, offset, read);
            }

            _Current += read;
            return read;

        }

        new public void Write(byte[] array, int offset, int count)
        {
            _Current = -1;
            base.Write(array, offset, count);
        }

        new public void WriteByte(byte value)
        {
            _Current = -1;
            base.WriteByte(value);
        }

        #endregion
    }
}
