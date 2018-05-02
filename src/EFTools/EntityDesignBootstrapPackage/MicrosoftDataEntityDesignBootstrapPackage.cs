// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using VSErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSShell = Microsoft.VisualStudio.Shell;

namespace Microsoft.Data.Entity.Design.BootstrapPackage
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.VisualStudio.Shell.Interop;

    internal static class Constants
    {
        public const string MicrosoftDataEntityDesignBootstrapPackageId = "7A4E8D96-5D5B-4415-9FAB-D6DCC56F47FB";
        public const string UICONTEXT_AddNewEntityDataModel = "E000C7E5-DBA5-4682-ABE0-7F6CE57B236D"; // see EntityDesignPackage\Commands_VS15.vsct
    }

    //
    // Prior to Dev15 we auto load this VS package when the solution has at least one loaded
    // project. This package will then check the solution for any contained EDMX files and keep
    // monitoring projects/solutions for any EDMX files. If there is any, this package
    // will load the EDM package.
    // From Dev15 onwards auto load when a .edmx file is selected and not at solution load.
    // Note: the pkgdef files in PkgDefData _must_ be kept in sync with the
    // PackageRegistration, ProvideAutoLoad and ProvideUIContextRule attributes.
    //
    [VSShell.DefaultRegistryRootAttribute("Software\\Microsoft\\VisualStudio\\11.0")]
    [VSShell.PackageRegistrationAttribute(RegisterUsing = VSShell.RegistrationMethod.Assembly, UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ComVisible(true)]
    [Guid(Constants.MicrosoftDataEntityDesignBootstrapPackageId)]
    // Perf optimization for VS15 onwards - only load this package if an .edmx file
    // is the current selection in the active hierarchy (instead of at solution load)
    [VSShell.ProvideAutoLoadAttribute(Constants.UICONTEXT_AddNewEntityDataModel)]
    // VSShell.ProvideUIContextRule will cause a CS3016 warning. It should not because the class is internal
    // but due to DevDiv bug 94391 it does anyway. Work around this by ignoring that warning.
#pragma warning disable 3016
    [VSShell.ProvideUIContextRule(Constants.UICONTEXT_AddNewEntityDataModel,
        name: "Auto load Entity Data Model Package",
        expression: "DotEdmx",
        termNames: new[] { "DotEdmx" },
        termValues: new[] { "HierSingleSelectionName:.edmx$" })]
#pragma warning restore 3016
    [SuppressMessage("Microsoft.Performance", "CA1812: AvoidUninstantiatedInternalClasses")]
    internal sealed class BootstrapPackage : VSShell.AsyncPackage
    {
        private const string guidEscherPkgString = "8889e051-b7f9-4781-bb33-2a36a9bdb3a5";
        private static readonly Guid guidEscherPkg = new Guid(guidEscherPkgString);

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.AdviseSolutionEvents(Microsoft.VisualStudio.Shell.Interop.IVsSolutionEvents,System.UInt32@)")]
        protected override System.Threading.Tasks.Task InitializeAsync(
            CancellationToken cancellationToken, IProgress<VSShell.ServiceProgressData> progress)
        {
            CheckAndLoadEDMPackage();

            return System.Threading.Tasks.Task.FromResult<object>(null);
        }

        private void CheckAndLoadEDMPackage()
        {
            var vsShell = (IVsShell)GetServiceAsync(typeof(SVsShell));
            Debug.Assert(vsShell != null, "unexpected null value for vsShell");
            if (vsShell != null)
            {
                var packageGuid = guidEscherPkg;
                IVsPackage package = null;
                var hrIsLoaded = vsShell.IsPackageLoaded(ref packageGuid, out package);
                if (!VSErrorHandler.Succeeded(hrIsLoaded) || package == null)
                {
                    var hrLoad = vsShell.LoadPackage(ref packageGuid, out package);
                    if (VSErrorHandler.Failed(hrLoad))
                    {
                        var msg = String.Format(CultureInfo.CurrentCulture, Resources.PackageLoadFailureExceptionMessage, hrLoad);
                        throw new InvalidOperationException(msg);
                    }
                }
            }
        }
    }
}
