// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Common
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using Microsoft.Win32;

    internal static class CriticalException
    {
        private const string RegistryKeyPath = @"Software\Microsoft\DataTools";
        private const string RegistryValueName = "DisableExceptionFilter";

        private static readonly object _syncRoot = new object();
        private static volatile bool _initialized;
        private static bool _disableFiltering;

        /// <summary>
        ///     Gets whether exception filtering is not-enabled based on registry settings.
        /// </summary>
        internal static bool DisableExceptionFilter
        {
            get
            {
                if (!_initialized)
                {
                    lock (_syncRoot)
                    {
                        if (!_initialized)
                        {
                            using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false))
                            {
                                var value = key != null ? key.GetValue(RegistryValueName) : null;
                                if (value != null
                                    && value.ToString() != "0")
                                {
                                    _disableFiltering = true;
                                }
                            }
                            _initialized = true;
                        }
                    }
                }
                return _disableFiltering;
            }
        }

        /// <summary>
        ///     Gets whether exception is a critical one and can't be ignored with corrupting
        ///     AppDomain state.
        /// </summary>
        /// <param name="ex">Exception to test.</param>
        /// <returns>True if exception should not be swallowed.</returns>
        internal static bool IsCriticalException(Exception ex)
        {
            // When filtering is not-enabled, all exceptions are critical and should be reported to Watson.
            if (DisableExceptionFilter)
            {
                return true;
            }

            if (ex is NullReferenceException
                || ex is StackOverflowException
                || ex is OutOfMemoryException
                || ex is ThreadAbortException)
            {
                return true;
            }

            if (ex.InnerException != null)
            {
                return IsCriticalException(ex.InnerException);
            }

            return false;
        }

        /// <summary>
        ///     Shows non-critical exceptions to the user and returns false or
        ///     returns true for critical exceptions.
        /// </summary>
        /// <param name="serviceProvider">Service provider to use to display error message.</param>
        /// <param name="ex">Exception to handle.</param>
        /// <returns>True if exception is critical and can't be ignored.</returns>
        internal static bool ThrowOrShow(IServiceProvider serviceProvider, Exception ex)
        {
            if (IsCriticalException(ex))
            {
                return true;
            }

            if (serviceProvider != null)
            {
                var uiService = serviceProvider.GetService(typeof(IUIService)) as IUIService;
                if (uiService != null)
                {
                    uiService.ShowError(ex);
                    return false;
                }
            }

            Debug.Fail("unable to get IUIService.  Falling back on MessageBox.Show()");
            MessageBoxOptions options = 0;
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                options = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
            }
            MessageBox.Show(
                null, ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1,
                options);

            return false;
        }
    }
}
