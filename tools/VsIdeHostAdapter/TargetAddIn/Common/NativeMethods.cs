// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    using DWORD = System.UInt32;
    using LONG = System.Int32;
    using Microsoft.VisualStudio.OLE.Interop;

    internal static class NativeMethods
    {
        #region Constants
        // COM return codes
        public const int S_OK = 0;

        public const int INVALID_HANDLE_VALUE = -1;
        public const int TH32CS_SNAPPROCESS = 0x2;
        public const int PROCESS_QUERY_INFORMATION = 0x400;
        public const int MAX_PATH = 260;
        public const int ERROR_INVALID_PARAMETER = 87;    // winerror.h
        public const int ERROR_INVALID_HANDLE = 6;        // winerror.h
        public const int PROCESS_TERMINATE = 1;
        private const string KERNEL32 = "kernel32.dll";
        #endregion

        #region Types
        // TODO: How do we define Dispose on this struct?
        [SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable")]
        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PROCESSENTRY32
        {
            public DWORD dwSize;                 // DWORD
            public DWORD cntUsage;               // DWORD
            public DWORD th32ProcessID;          // DWORD
            [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            public IntPtr th32DefaultHeapID;     // ULONG_PTR	// TODO: CriticalHandle?
            public DWORD th32ModuleID;           // DWORD
            public DWORD cntThreads;             // DWORD
            public DWORD th32ParentProcessID;    // DWORD
            public LONG pcPriClassBase;          // LONG. This HAS to be managed 'int', not 'long' otherwise size mismatch as sizeof(LONG) = 4.
            public DWORD dwFlags;                // DWORD
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szExeFile; // The meaning of TStr is determined by StructLayout.CharSet.
        }
        #endregion

        #region Win32
        /// <summary>
        /// Takes a snapshot of the specified processes in the system, as well as the heaps, modules, and threads used by these processes.
        /// </summary>
        /// <param name="dwFlags">
        /// Portions of the system to include in the snapshot. Use TH32CS_SNAPPROCESS to enum processes.
        /// </param>
        /// <param name="th32ProcessID">
        /// Process identifier of the process to be included in the snapshot. Ignored for TH32CS_SNAPPROCESS.
        /// </param>
        /// <returns>HANDLE of the snapshot.</returns>
        [DllImport(KERNEL32, SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(   // HANDLE
            DWORD dwFlags,       // DWORD
            DWORD th32ProcessID);// DWORD

        /// <summary>
        /// Retrieves information about the first process encountered in a system snapshot.
        /// </summary>
        /// <param name="hSnapshot"></param>
        /// <param name="lppe"></param>
        /// <returns>
        /// TRUE if the first entry of the process list has been copied to the buffer or FALSE otherwise. 
        /// The ERROR_NO_MORE_FILES error value is returned by the GetLastError function if no processes exist or 
        /// the snapshot does not contain process information.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api")]
        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Unicode)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool Process32First(
            IntPtr hSnapshot,           // HANDLE
            ref PROCESSENTRY32 lppe);

        [SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api")]
        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Unicode)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool Process32Next(
            IntPtr hSnapshot,           // HANDLE
            ref PROCESSENTRY32 lpProcessEntry32);

        /// <summary>
        /// The OpenProcess function opens an existing process object.
        /// </summary>
        /// <param name="dwDesiredAccess">Access to the process object.</param>
        /// <param name="bInheritHandle">
        /// If this parameter is TRUE, the handle is inheritable. If the parameter is FALSE, the handle cannot be inherited.
        /// </param>
        /// <param name="dwProcessId">ID of the process to open.</param>
        /// <returns>
        /// If the function succeeds, the return value is an open handle to the specified process.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>If the process does not exist, GetLastError is set to ERROR_INVALID_PARAMETER.</remarks>
        [DllImport(KERNEL32, SetLastError = true)]
        public static extern IntPtr OpenProcess(   // HANDLE
            DWORD dwDesiredAccess,  // DWORD
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,    // BOOL
            DWORD dwProcessId);     // DWORD

        [DllImport(KERNEL32, SetLastError = true)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern void SetLastError(int errorCode);
        #endregion

        #region COM
        /// <summary>
        /// Get the ROT.
        /// </summary>
        /// <param name="reserved"></param>
        /// <param name="prot">Pointer to running object table interface</param>
        /// <returns></returns>
        [DllImport("ole32.dll")]
        internal static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        /// <summary>
        /// Create a Bind context.
        /// </summary>
        /// <param name="reserved"></param>
        /// <param name="ppbc"></param>
        /// <returns>HRESULT</returns>
        [DllImport("ole32.dll")]
        internal static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        /// <summary>
        /// Register message filter for COM.
        /// </summary>
        /// <param name="lpMessageFilter">New filter to register.</param>
        /// <param name="lplpMessageFilter">Old filter. Save it if you need to restore it later.</param>
        [DllImport("ole32.dll")]
        internal static extern int CoRegisterMessageFilter(IMessageFilter lpMessageFilter, out IMessageFilter lplpMessageFilter);
        #endregion
    }
}
