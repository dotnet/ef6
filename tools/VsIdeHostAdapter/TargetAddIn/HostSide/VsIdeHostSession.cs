// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// Identifies VS IDE Session.
    /// Used to attaching debugger to VS Test Authoring IDE.
    /// </summary>
    internal static class VsIdeHostSession
    {
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]    // Can be used in friend assembly.
        internal static readonly string RemoteObjectName = "EqtAddinSessionDebugging";
        internal const string Prefix = "EqtAddinSession_";

        private static string s_id = Prefix + Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Id of VS session: used for server channel name in authoring IDE.
        /// The format is: Prefix + CurrentProcess id.
        /// </summary>
        public static string Id
        {
            get
            {
                return s_id;
            }
        }
    }
}
