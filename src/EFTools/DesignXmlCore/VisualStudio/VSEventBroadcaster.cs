// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     This class can be used to get notified about UI change events in Visual Studio such
    ///     as font changes.
    /// </summary>
    internal class VSEventBroadcaster : IVsBroadcastMessageEvents, IDisposable
    {
        public event EventHandler OnFontChanged;
        private uint cookie;
        private readonly IServiceProvider provider;
        private IVsShell shellService;

        public VSEventBroadcaster(IServiceProvider provider)
        {
            this.provider = provider;
        }

        #region IDisposable

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsShell.UnadviseBroadcastMessages(System.UInt32)")]
        public void Dispose()
        {
            if (cookie != 0
                && shellService != null)
            {
                try
                {
                    shellService.UnadviseBroadcastMessages(cookie);
                }
                catch (Exception)
                {
                    // Possibly VS is shutting down so ignore the exception.
                }
                finally
                {
                    cookie = 0;
                    shellService = null;
                }
            }
            GC.SuppressFinalize(this);
        }

        #endregion

        public void Initialize()
        {
            shellService = provider.GetService(typeof(SVsShell)) as IVsShell;
            if (shellService != null)
            {
                ErrorHandler.ThrowOnFailure(shellService.AdviseBroadcastMessages(this, out cookie));
            }
        }

        #region IVsBroadcastMessageEvents Members

        /// <summary>
        ///     Watches for system color and font change event from Visual Studio
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public int OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == NativeMethods.WM_SYSCOLORCHANGE)
            {
                if (OnFontChanged != null)
                {
                    OnFontChanged(this, EventArgs.Empty);
                }
            }
            return VSConstants.S_OK;
        }

        #endregion
    }
}
