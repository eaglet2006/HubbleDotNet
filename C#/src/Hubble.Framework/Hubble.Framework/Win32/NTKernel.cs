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

namespace Hubble.Framework.Win32
{
    #region Data Structures


    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct SECURITY_DESCRIPTOR
    {
        public byte revision;
        public byte size;
        public short control;
        public IntPtr owner;
        public IntPtr group;
        public IntPtr sacl;
        public IntPtr dacl;

    }

    enum TOKEN_INFORMATION_CLASS
    {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics,
        TokenRestrictedSids,
        TokenSessionId,
        TokenGroupsAndPrivileges,
        TokenSessionReference,
        TokenSandBoxInert,
        TokenAuditPolicy,
        TokenOrigin
    }

    public struct TOKEN_USER
    {
        public SID_AND_ATTRIBUTES User;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SID_AND_ATTRIBUTES
    {

        public IntPtr Sid;
        public int Attributes;
    }

    enum SID_NAME_USE
    {
        SidTypeUser = 1,
        SidTypeGroup,
        SidTypeDomain,
        SidTypeAlias,
        SidTypeWellKnownGroup,
        SidTypeDeletedAccount,
        SidTypeInvalid,
        SidTypeUnknown,
        SidTypeComputer
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes : IDisposable
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool SetSecurityDescriptorDacl(IntPtr sd, bool daclPresent, IntPtr dacl, bool daclDefaulted);


        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        static extern bool InitializeSecurityDescriptor(IntPtr pSecurityDescriptor, int dwRevision);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        static extern bool OpenThreadToken(
            IntPtr ThreadHandle,
            int DesiredAccess,
            bool OpenAsSelf,
            out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        public static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            int DesiredAccess,
            out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        static extern bool GetTokenInformation(
            IntPtr TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            int TokenInformationLength,
            out int ReturnLength);

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        static extern bool ConvertSidToStringSid(
            IntPtr Sid,
            out IntPtr ptrSid);

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        static extern bool ConvertStringSidToSid(
          out IntPtr ptrSid,
          out IntPtr Sid
        );



        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int LocalFree(IntPtr hMem);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        static extern bool LookupAccountName(
            string lpSystemName,
            string lpAccountName,
            IntPtr Sid,
            out int cbSid,
            IntPtr ReferencedDomainName,
            out int cchReferencedDomainName,
            out SID_NAME_USE peUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetLengthSid(IntPtr pSid);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        public static extern bool InitializeAcl(ref IntPtr pAcl, int nAclLength, int dwAclRevision);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        public static extern bool AddAccessAllowedAce(ref IntPtr pAcl, int dwAceRevision, int AccessMask, IntPtr pSid);

        private IntPtr GetToken()
        {
            IntPtr tokenHandle = IntPtr.Zero;

            const int TOKEN_QUERY = 0x00000008;
            if (!OpenThreadToken(GetCurrentThread(), TOKEN_QUERY, true, out tokenHandle))
            {
                if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, out tokenHandle))
                {
                    return IntPtr.Zero;
                }
            }

            return tokenHandle;
        }

        IntPtr GetSidByAccountName(String accountName)
        {
            IntPtr Sid = IntPtr.Zero;
            int cbSid;
            IntPtr ReferencedDomainName = IntPtr.Zero;
            int cchReferencedDomainName;

            SID_NAME_USE sidUse;

            LookupAccountName(null, accountName, Sid, out cbSid, ReferencedDomainName, out cchReferencedDomainName, out sidUse);

            Sid = Marshal.AllocCoTaskMem((int)cbSid);
            ReferencedDomainName = Marshal.AllocCoTaskMem((int)cchReferencedDomainName);

            if (!LookupAccountName(null, accountName, Sid, out cbSid, ReferencedDomainName, out cchReferencedDomainName, out sidUse))
            {
                Marshal.FreeCoTaskMem(Sid);
                Marshal.FreeCoTaskMem(ReferencedDomainName);
                return IntPtr.Zero;
            }

            Marshal.FreeCoTaskMem(ReferencedDomainName);

            return Sid;
        }

        IntPtr GetSelfSid(IntPtr tokenHandle)
        {
            int tokenInfLength = 0;
            bool result;

            // first call gets lenght of TokenInformation
            result = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenUser,
                IntPtr.Zero, tokenInfLength, out tokenInfLength);

            IntPtr tokenInformation = Marshal.AllocCoTaskMem((int)tokenInfLength);

            result = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenUser, tokenInformation, tokenInfLength, out tokenInfLength);

            if (result)
            {
                TOKEN_USER tokenUser = (TOKEN_USER)Marshal.PtrToStructure(tokenInformation, typeof(TOKEN_USER));

                return tokenUser.User.Sid;
            }

            Marshal.FreeCoTaskMem(tokenInformation);

            return IntPtr.Zero;
        }


        private int nLength;
        private IntPtr lpSecurityDescriptor;
        private int bInheritHandle;

        public SecurityAttributes(String accountName)
        {
            const int ACL_REVISION = 2;
            //const int ACL_REVISION_DS = 4;

            unchecked { const int GENERIC_READ = (int)0x80000000; }
            //const int GENERIC_WRITE       = 0x40000000;
            //const int GENERIC_EXECUTE     = 0x20000000;
            const int GENERIC_ALL = 0x10000000;

            IntPtr token = GetToken();
            //byte[] sd1 = GetSidByAccountName("ASPNET");
            IntPtr sd1 = GetSidByAccountName("Administrator");
            IntPtr sd2 = GetSelfSid(token);

            //Get SecurityAttributes size
            nLength = Marshal.SizeOf(typeof(SecurityAttributes));

            //Inherit handle
            bInheritHandle = 1;

            IntPtr pacl = Marshal.AllocCoTaskMem(1024);

            bool ret = InitializeAcl(ref pacl, 1024, ACL_REVISION);

            ret = AddAccessAllowedAce(ref pacl, ACL_REVISION, GENERIC_ALL, sd1);
            ret = AddAccessAllowedAce(ref pacl, ACL_REVISION, GENERIC_ALL, sd2);

            //lpSecurityDescriptor = Marshal.AllocCoTaskMem(dest.Length);
            //Marshal.Copy(dest, 0, lpSecurityDescriptor, dest.Length);
            //Struct to Ptr

            //bool ret = InitializeSecurityDescriptor(lpSecurityDescriptor, 1);
            //ret = SetSecurityDescriptorDacl(lpSecurityDescriptor, true, IntPtr.Zero, false);

            SECURITY_DESCRIPTOR sd = new SECURITY_DESCRIPTOR();

            //Alloc memory for security descriptor
            lpSecurityDescriptor = Marshal.AllocCoTaskMem(Marshal.SizeOf(sd));

            //Struct to Ptr
            Marshal.StructureToPtr(sd, lpSecurityDescriptor, false);

            ret = InitializeSecurityDescriptor(lpSecurityDescriptor, 1);
            ret = SetSecurityDescriptorDacl(lpSecurityDescriptor, true, pacl, true);
        }

        public SecurityAttributes()
        {

            //Get SecurityAttributes size
            nLength = Marshal.SizeOf(typeof(SecurityAttributes));

            //Inherit handle
            bInheritHandle = 1;

            //Create a NULL DACL 
            SECURITY_DESCRIPTOR sd = new SECURITY_DESCRIPTOR();

            //Alloc memory for security descriptor
            lpSecurityDescriptor = Marshal.AllocCoTaskMem(Marshal.SizeOf(sd));

            //Struct to Ptr
            Marshal.StructureToPtr(sd, lpSecurityDescriptor, false);

            InitializeSecurityDescriptor(lpSecurityDescriptor, 1);
            SetSecurityDescriptorDacl(lpSecurityDescriptor, true, IntPtr.Zero, false);
        }

        public void Dispose()
        {
            lock (this)
            {
                if (lpSecurityDescriptor != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(lpSecurityDescriptor);
                    //Marshal.FreeHGlobal(lpSecurityDescriptor);
                    lpSecurityDescriptor = IntPtr.Zero;
                }
            }
        }

        ~SecurityAttributes()
        {
            Dispose();
        }

    }

    #endregion

    public enum WaitForState : int
    {
        WAIT_ABANDONED = 0x00000080,
        WAIT_OBJECT_0 = 0x00000000,
        WAIT_TIMEOUT = 0x00000102,
    }

    public enum EventAccess : int
    {
        STANDARD_RIGHTS_REQUIRED = 0x000F0000,
        SYNCHRONIZE = 0x00100000,
        EVENT_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3),
        EVENT_MODIFY_STATE = 0x0002,
    }

    public class NTKernel
    {
        #region General

        internal const int INFINITE = -1;

        [DllImport("kernel32", EntryPoint = "CloseHandle", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool CloseHandle(int hHandle);

        [DllImport("kernel32", EntryPoint = "GetLastError", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int GetLastError();

        #endregion

        #region Semaphore

        [DllImport("kernel32", EntryPoint = "CreateSemaphore", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int CreateSemaphore(SecurityAttributes auth, int initialCount, int maximumCount, string name);

        [DllImport("kernel32", EntryPoint = "WaitForSingleObject", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int WaitForSingleObject(int hHandle, int dwMilliseconds);

        [DllImport("kernel32", EntryPoint = "ReleaseSemaphore", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool ReleaseSemaphore(int hHandle, int lReleaseCount, out int lpPreviousCount);

        #endregion

        #region Mutex
        [DllImport("kernel32.dll")]
        internal static extern IntPtr CreateMutex(SecurityAttributes lpEventAttributes, bool bInitialOwner, string lpName);

        [DllImport("kernel32.dll")]
        internal static extern bool ReleaseMutex(IntPtr hHandle);

        #endregion

        #region Event

        [DllImport("kernel32.dll")]
        internal static extern IntPtr CreateEvent(SecurityAttributes lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenEvent(int dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll")]
        internal static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll")]
        internal static extern bool ResetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll")]
        static extern bool PulseEvent(IntPtr hEvent);

        #endregion

        #region Memory Mapped Files

        [DllImport("Kernel32.dll", EntryPoint = "CreateFileMapping", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateFileMapping(int hFile, SecurityAttributes lpAttributes, int flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);

        [DllImport("Kernel32.dll", EntryPoint = "OpenFileMapping", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenFileMapping(int dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("Kernel32.dll", EntryPoint = "MapViewOfFile", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, int dwNumberOfBytesToMap);

        [DllImport("Kernel32.dll", EntryPoint = "UnmapViewOfFile", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("Kernel32.dll", EntryPoint = "FlushViewOfFile", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool FlushViewOfFile(IntPtr lpBaseAddress, int dwNumberOfBytesToFlush);

        #endregion

    }
}
