// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.TestTools.VsIdeTesting
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    ///     Helper class to invoke a method on UI thread.
    /// </summary>
    public static class UIThreadInvoker
    {
        /// <summary>
        ///     Used to invoke code on UI thread.
        /// </summary>
        private static Control s_uiThreadControl;

        public static object Invoke(Delegate method)
        {
            Debug.Assert(
                s_uiThreadControl != null, "UIThreadInvoker.Invoke: the Control used to invoke code on UI thread has not been initialized!");

            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            Object retValue;
            if (s_uiThreadControl.InvokeRequired)
            {
                retValue = s_uiThreadControl.Invoke(method);
            }
            else
            {
                retValue = method.DynamicInvoke();
            }

            return retValue;
        }

        public static object Invoke(Delegate method, params object[] args)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            Object retValue;
            if (s_uiThreadControl.InvokeRequired)
            {
                retValue = s_uiThreadControl.Invoke(method, args);
            }
            else
            {
                retValue = method.DynamicInvoke(args);
            }

            return retValue;
        }

        public static IAsyncResult BeginInvoke(Delegate method, params object[] args)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            return s_uiThreadControl.BeginInvoke(method, args);
        }

        /// <summary>
        ///     This needs to be called on the UI thread.
        ///     The control itself does not know about UI thread. How it works is:
        ///     control.Invoke is called on the same thread where Control was initialized.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "handle")]
        public static void Initialize()
        {
            Trace.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}, pid={1}, tid={2}: UIThreadInvoker.Initialize: Stack trace: {3}",
                    DateTime.Now,
                    Process.GetCurrentProcess().Id,
                    Thread.CurrentThread.ManagedThreadId,
                    Environment.StackTrace));

            if (s_uiThreadControl == null)
            {
                s_uiThreadControl = new Control();
                // Force creating the control's handle needed by Control.Invoke.
                var handle = s_uiThreadControl.Handle;
            }
        }
    }
}
