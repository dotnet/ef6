// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class EdmSolutionEvents : IVsSolutionEvents
    {
        private static EdmSolutionEvents _instance;

        internal static EdmSolutionEvents Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new EdmSolutionEvents();
                }

                return _instance;
            }
        }

        private EdmSolutionEvents()
        {
        }

        internal bool IsAfterErrorListClearedOnSolutionClose { get; private set; }

        #region IVsSolutionEvents Members

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            // Clear and remove all error lists when the solution is closed
            ErrorListHelper.RemoveAll();

            IsAfterErrorListClearedOnSolutionClose = true;

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            IsAfterErrorListClearedOnSolutionClose = false;
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            if (PackageManager.Package != null
                && PackageManager.Package.ModelManager != null)
            {
                foreach (var fileExtension in VSArtifact.GetVSArtifactFileExtensions())
                {
                    // discard all documents in the project being closed 
                    foreach (var vsFileInfo in new VSFileFinder(fileExtension).FindInProject(pHierarchy))
                    {
                        var uri = Utils.FileName2Uri(vsFileInfo.Path);
                        PackageManager.Package.ModelManager.ClearArtifact(uri);
                    }
                }

                var project = VSHelpers.GetProject(pHierarchy);
                if (project != null)
                {
                    PackageManager.Package.AggregateProjectTypeGuidCache.Remove(project);
                }
            }
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
