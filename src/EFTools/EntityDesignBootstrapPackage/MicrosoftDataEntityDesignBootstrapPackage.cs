// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using VSErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSShell = Microsoft.VisualStudio.Shell;

namespace Microsoft.Data.Entity.Design.BootstrapPackage
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
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
    // ProvideAutoLoad and ProvideUIContextRule attributes.
    //
    [VSShell.DefaultRegistryRootAttribute("Software\\Microsoft\\VisualStudio\\11.0")]
    [VSShell.PackageRegistrationAttribute(RegisterUsing = VSShell.RegistrationMethod.Assembly, UseManagedResourcesOnly = true)]
    [ComVisible(true)]
    [Guid(Constants.MicrosoftDataEntityDesignBootstrapPackageId)]
#if (VS11 || VS12 || VS14)
    [VSShell.ProvideAutoLoadAttribute("93694fa0-0397-11d1-9f4e-00a0c911004f")] // VSConstants.UICONTEXT_SolutionHasMultipleProjects
    [VSShell.ProvideAutoLoadAttribute("adfc4e66-0397-11d1-9f4e-00a0c911004f")] // VSConstants.UICONTEXT_SolutionHasSingleProject
#else
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
#endif
    [SuppressMessage("Microsoft.Performance", "CA1812: AvoidUninstantiatedInternalClasses")]
    internal sealed class BootstrapPackage : VSShell.Package, IVsSolutionEvents
    {
        private uint _trackSolEventsCookie;
        private const string ExtensionEdmx = ".edmx";

        private const string guidEscherPkgString = "8889e051-b7f9-4781-bb33-2a36a9bdb3a5";
        private static readonly Guid guidEscherPkg = new Guid(guidEscherPkgString);

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.AdviseSolutionEvents(Microsoft.VisualStudio.Shell.Interop.IVsSolutionEvents,System.UInt32@)")]
        protected override void Initialize()
        {
            base.Initialize();

            var solution = GetService(typeof(SVsSolution)) as IVsSolution;
            Debug.Assert(solution != null, "Unexpected null value for IVsSolution");
            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out _trackSolEventsCookie);
            }

            // OnAfterOpenProject has already been fired to event sinks at this point, so
            // we need to perform a manual check
            if (solution != null)
            {
                CheckAndLoadEDMPackage(solution);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.UnadviseSolutionEvents(System.UInt32)")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                var solution = GetService(typeof(SVsSolution)) as IVsSolution;
                Debug.Assert(solution != null, "Unexpected null value for IVsSolution");
                if (solution != null)
                {
                    if (_trackSolEventsCookie != 0)
                    {
                        solution.UnadviseSolutionEvents(_trackSolEventsCookie);
                        _trackSolEventsCookie = 0;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CheckAndLoadEDMPackage(IVsHierarchy project)
        {
            if (!IsEDMPackageLoaded())
            {
                try
                {
                    var shouldLoadEDMPackage = ShouldLoadEDMPackage(project);
                    if (shouldLoadEDMPackage)
                    {
                        LoadEDMPackage();
                    }
                }
                catch (Exception loadPackageException)
                {
                    Debug.Assert(false, "Caught exception while trying to load EDM package: " + loadPackageException.Message);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CheckAndLoadEDMPackage(IVsSolution solution)
        {
            if (!IsEDMPackageLoaded())
            {
                // if there is at least one EDMX file in the solution, load the EDM package
                Debug.Assert(solution != null, "Unexpected null value of project");
                if (solution != null)
                {
                    foreach (var project in BootstrapUtils.GetProjectsInSolution(solution))
                    {
                        var shouldLoadEDMPackage = ShouldLoadEDMPackage(project);
                        if (shouldLoadEDMPackage)
                        {
                            try
                            {
                                LoadEDMPackage();
                                return;
                            }
                            catch (Exception loadPackageException)
                            {
                                Debug.Assert(false, "Caught exception while trying to load EDM package: " + loadPackageException.Message);
                            }
                        }
                    }
                }
            }
        }

        private static bool ShouldLoadEDMPackage(IVsHierarchy project)
        {
            var shouldLoadEDMPackage = false;
            try
            {
                if (BootstrapUtils.IsEDMSupportedInProject(project))
                {
                    // Check if project is a website project. If yes, search for edmx files under the app_code folder.
                    // Note that website project doesn't have project file and to exclude files from project you append ".exclude" in file name.
                    var dteProject = BootstrapUtils.GetProject(project);
                    if (dteProject != null
                        && BootstrapUtils.IsWebProject(dteProject))
                    {
                        var projectFullPathProperty = dteProject.Properties.Item("FullPath");
                        var projectFullPath = projectFullPathProperty.Value as string;

                        Debug.Assert(String.IsNullOrWhiteSpace(projectFullPath) == false, "Unable to get project full path");
                        if (String.IsNullOrWhiteSpace(projectFullPath) == false)
                        {
                            // App_Code is a special folder that will not be localized should always be under root folder.
                            var appCodePath = Path.Combine(projectFullPath, "App_Code");

                            if (Directory.Exists(appCodePath))
                            {
                                // Directory.GetFiles could potentially be an expensive operation. 
                                // If the performance is not acceptable, we should look into calling Win32 API to search for files.
                                shouldLoadEDMPackage =
                                    Directory.GetFiles(appCodePath, "*" + ExtensionEdmx, SearchOption.AllDirectories).Any();
                            }
                        }
                    }
                    else
                    {
                        var fileFinder = new VSFileFinder(ExtensionEdmx);
                        shouldLoadEDMPackage = fileFinder.ExistInProject(project);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail("Caught exception in BootstrapPackage's ShouldLoadEDMPackage method. Message: " + ex.Message);
                // We want to continue loading the package if the exception is not critical.
                // Do not do Debug.Assert here because it could cause a lot of DDBasic failures in CHK build. 
                if (VSErrorHandler.IsCriticalException(ex))
                {
                    throw;
                }
            }
            return shouldLoadEDMPackage;
        }

        private bool IsEDMPackageLoaded()
        {
            var vsShell = (IVsShell)GetService(typeof(SVsShell));
            Debug.Assert(vsShell != null, "unexpected null value for vsShell");
            if (vsShell != null)
            {
                var packageGuid = guidEscherPkg;
                IVsPackage package = null;
                var hr = vsShell.IsPackageLoaded(ref packageGuid, out package);
                if (VSErrorHandler.Succeeded(hr) && package != null)
                {
                    return true;
                }
            }
            return false;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsShell.IsPackageLoaded(System.Guid@,Microsoft.VisualStudio.Shell.Interop.IVsPackage@)")]
        private void LoadEDMPackage()
        {
            var vsShell = (IVsShell)GetService(typeof(SVsShell));
            Debug.Assert(vsShell != null, "unexpected null value for vsShell");
            IVsPackage package = null;
            if (vsShell != null)
            {
                var packageGuid = guidEscherPkg;
                vsShell.IsPackageLoaded(ref packageGuid, out package);
                if (package == null)
                {
                    var hr = vsShell.LoadPackage(ref packageGuid, out package);
                    if (VSErrorHandler.Failed(hr))
                    {
                        var msg = String.Format(CultureInfo.CurrentCulture, Resources.PackageLoadFailureExceptionMessage, hr);
                        throw new InvalidOperationException(msg);
                    }
                }
            }
        }

        #region IVsSolutionEvents Members

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        // if solutions are opened, this will get called for each project
        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            if (pHierarchy != null)
            {
                CheckAndLoadEDMPackage(pHierarchy);
            }
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
