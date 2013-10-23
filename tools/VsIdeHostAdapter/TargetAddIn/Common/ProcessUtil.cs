// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.Diagnostics;
using System.IO;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// Provides utility API for Win32 processes.
    /// </summary>
    internal static class ProcessUtil
    {
        #region Public API
        #region Parent Process Id
        /// <summary>
        /// Returns PID of the parent process.
        /// The information may be unreliable: the process may already exited, etc.
        /// </summary>
        [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
        public static int GetParentProcessId(int processId)
        {
            // This may be inconsistent with snapshot but when the call returns it it not guaranteed to be consistent anyway.
            EnsureProcessExists(processId);

            IntPtr snapshot = IntPtr.Zero;
            try
            {
                snapshot = NativeMethods.CreateToolhelp32Snapshot(NativeMethods.TH32CS_SNAPPROCESS, 0);
                if (snapshot.ToInt64() == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                NativeMethods.PROCESSENTRY32 pe = new NativeMethods.PROCESSENTRY32();
                pe.dwSize = (uint)Marshal.SizeOf(pe);

                bool found = NativeMethods.Process32First(snapshot, ref pe);
                while (found)
                {
                    if (pe.th32ProcessID == processId)
                    {
                        return (int)pe.th32ParentProcessID;
                    }
                    found = NativeMethods.Process32Next(snapshot, ref pe);
                }

                // Report ArgumentException(processId) in the consistent way.
                NativeMethods.SetLastError(NativeMethods.ERROR_INVALID_PARAMETER);
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            finally
            {
                if (snapshot != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(snapshot);
                }
            }
        }
        #endregion
        #endregion

        #region Private
        /// <summary>
        /// Throws if the process with given PID does not exist.
        /// </summary>
        private static void EnsureProcessExists(int processId)
        {
            IntPtr process = IntPtr.Zero;
            try
            {
                // It process itself does not exist, we throw.
                process = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_INFORMATION, false, (uint)processId);
                if (process == IntPtr.Zero)
                {
                    throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
            }
            finally
            {
                if (process != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(process);
                }
            }
        }
        #endregion
    }
}
