// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal interface IEdmPackage : IXmlDesignerPackage
    {
        IEntityDesignCommandSet CommandSet { get; }
        ExplorerWindow ExplorerWindow { get; }
        MappingDetailsWindow MappingDetailsWindow { get; }
        new EntityDesignModelManager ModelManager { get; }
        ConnectionManager ConnectionManager { get; }
        ModelChangeEventListener ModelChangeEventListener { get; }
        AggregateProjectTypeGuidCache AggregateProjectTypeGuidCache { get; }
        ModelGenErrorCache ModelGenErrorCache { get; }

        // we need these handlers in order to communicate a file rename to the designer's codebase
        void OnFileNameChanged(string oldFileName, string newFileName);

        // needed to broadcast the possibility we are in a command-line build
        bool IsBuildingFromCommandLine { get; }
    }

    internal static class PackageManager
    {
        private static IEdmPackage _package;

        internal static bool IsLoaded
        {
            get { return _package != null; }
        }

        public static IEdmPackage Package
        {
            get
            {
                Debug.Assert(_package != null, "PackageManager.Package: package is null and someone is trying to access it!");
                return _package;
            }
            set { _package = value; }
        }

        internal static void LoadEDMPackage()
        {
            if (_package == null)
            {
                var vsShell = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShell)) as IVsShell;
                LoadEDMPackage(vsShell);
            }
        }

        internal static void LoadEDMPackage(IServiceProvider serviceProvider)
        {
            if (_package == null)
            {
                var vsShell = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
                if (vsShell != null)
                {
                    LoadEDMPackage(vsShell);
                }
                else
                {
                    LoadEDMPackage();
                }
            }
        }

        private static void LoadEDMPackage(IVsShell vsShell)
        {
            Debug.Assert(vsShell != null, "unexpected null value for vsShell");
            IVsPackage package = null;
            if (vsShell != null)
            {
                var packageGuid = PackageConstants.guidEscherPkg;
                var hr = vsShell.IsPackageLoaded(ref packageGuid, out package);
                if (NativeMethods.Failed(hr) || package == null)
                {
                    hr = vsShell.LoadPackage(ref packageGuid, out package);
                    if (NativeMethods.Failed(hr))
                    {
                        var msg = String.Format(CultureInfo.CurrentCulture, Resources.PackageLoadFailureExceptionMessage, hr);
                        throw new InvalidOperationException(msg);
                    }
                }
            }
        }
    }
}
