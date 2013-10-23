// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase",
    Scope = "namespace", Target = "Microsoft.VisualStudio.TestTools.VsIdeTesting")] // Be consistent with VS Interop Assemblies.

namespace Microsoft.VisualStudio.TestTools.VsIdeTesting
{
    using System;
    using System.Diagnostics;
    using EnvDTE;

    /// <summary>
    ///     This can be used inside tests hosted in VS IDE.
    ///     We take advantage of the fact that hosted tests run in the same app domain.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Vs",
        Justification = "Public class, cannot rename")]
    public static class VsIdeTestHostContext
    {
        private static IServiceProvider s_serviceProvider;
        private static readonly object s_lock = new object();
        private static DTE s_dte;

        /// <summary>
        ///     Service provider to hook up to VS instance.
        /// </summary>
        public static IServiceProvider ServiceProvider
        {
            get
            {
                Debug.Assert(s_serviceProvider != null, "VsIdeTestHostContext.ServiceProvider.get: s_serviceProvider is null!");
                return s_serviceProvider;
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // Called from friend assembly.
            internal set
            {
                Debug.Assert(value != null, "VsIdeTestHostContext.ServiceProvider.set: passed value = null!");
                // TODO: figure out why sometimes this is called more than 1 time. Nothing bad though.
                //Debug.Assert(s_serviceProvider == null, "VsIdeTestHostContext.ServiceProvider.set: Why are we trying to set service provider more than once?");

                s_serviceProvider = value;
            }
        }

        /// <summary>
        ///     Parameter to access test data string from run config host data
        /// </summary>
        public static string AdditionalTestData { get; [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // Called from friend assembly.
        internal set; }

        /// <summary>
        ///     Returns Visual Studio DTE.
        /// </summary>
        /// <remarks>
        ///     The reason for this is GetService is not thread safe.
        ///     This property is thread safe.
        /// </remarks>
        [CLSCompliant(false)]
        public static DTE Dte
        {
            get
            {
                var serviceProvider = s_serviceProvider; // Get snapshot of s_serviceProvider.
                if (serviceProvider == null)
                {
                    Debug.Fail("VsIdeTestHostContext.Dte: m_serviceProvider is null!");
                    return null;
                }

                if (s_dte == null)
                {
                    lock (s_lock) // Protect GetService.
                    {
                        if (s_dte == null)
                        {
                            s_dte = (DTE)serviceProvider.GetService(typeof(DTE));
                        }
                    }
                }
                return s_dte;
            }
        }
    }
}
