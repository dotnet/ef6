// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    internal interface ITrackEdmxUIEvents
    {
        // if we have no App.Config/Web.Config for the edmx file then we have to create it
        int OnBeforeGenerateDDL(Project project, EFArtifact artifact);
        // if we don't have an App.Config/Web.Config for the edmx file then we have to create it (this also gets raised during build)
        int OnBeforeValidateModel(Project project, EFArtifact artifact, bool onBuild);
    }

    internal class ModelChangeEventListener : IVsTrackProjectDocumentsEvents2, IVsRunningDocTableEvents3, ITrackEdmxUIEvents,
                                              IVsSolutionEvents, IDisposable
    {
        private uint trackDocEventsCookie;
        private uint trackRDTEventsCookie;
        private uint trackSolEventsCookie;

        /// <summary>
        ///     Handlers
        /// </summary>
        internal event ModelChangeEventHandler BeforeCloseProject;

        internal event ModelChangeEventHandler BeforeGenerateDDL;
        internal event ModelChangeEventHandler BeforeValidateModel;
        internal event ModelChangeEventHandler AfterOpenProject;
        internal event ModelChangeEventHandler AfterAddFile;
        internal event ModelChangeEventHandler AfterRemoveFile;
        internal event ModelChangeEventHandler QueryRemoveFile;
        internal event ModelChangeEventHandler AfterRenameFile;
        internal event ModelChangeEventHandler AfterSaveFile;
        internal event ModelChangeEventHandler AfterEntityContainerNameChange;
        internal event ModelChangeEventHandler AfterMetadataArtifactProcessingChange;

        internal ModelChangeEventListener()
        {
            StartTrackingProjectEvents();
            StartTrackingRDTEvents();
            StartTrackingSolutionEvents();
            PackageManager.Package.ModelManager.ModelChangesCommitted += OnModelChangesCommitted;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopTrackingProjectEvents();
                StopTrackingRDTEvents();
                StopTrackingSolutionEvents();
                PackageManager.Package.ModelManager.ModelChangesCommitted -= OnModelChangesCommitted;
            }
            else
            {
                Debug.Fail("ModelChangeEventListener is finalized before disposing");
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.AdviseRunningDocTableEvents(Microsoft.VisualStudio.Shell.Interop.IVsRunningDocTableEvents,System.UInt32@)")]
        private void StartTrackingRDTEvents()
        {
            var rdt = Services.ServiceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (rdt != null)
            {
                rdt.AdviseRunningDocTableEvents(this, out trackRDTEventsCookie);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.UnadviseRunningDocTableEvents(System.UInt32)")]
        private void StopTrackingRDTEvents()
        {
            var rdt = Services.ServiceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (rdt != null)
            {
                if (trackRDTEventsCookie != 0)
                {
                    rdt.UnadviseRunningDocTableEvents(trackRDTEventsCookie);
                    trackRDTEventsCookie = 0;
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsTrackProjectDocuments2.AdviseTrackProjectDocumentsEvents(Microsoft.VisualStudio.Shell.Interop.IVsTrackProjectDocumentsEvents2,System.UInt32@)")]
        private void StartTrackingProjectEvents()
        {
            var trackProjDocs = Services.ServiceProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
            if (trackProjDocs != null)
            {
                trackProjDocs.AdviseTrackProjectDocumentsEvents(this, out trackDocEventsCookie);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsTrackProjectDocuments2.UnadviseTrackProjectDocumentsEvents(System.UInt32)")]
        private void StopTrackingProjectEvents()
        {
            var trackProjDocs = Services.ServiceProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
            if (trackProjDocs != null)
            {
                if (trackDocEventsCookie != 0)
                {
                    trackProjDocs.UnadviseTrackProjectDocumentsEvents(trackDocEventsCookie);
                    trackDocEventsCookie = 0;
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.AdviseSolutionEvents(Microsoft.VisualStudio.Shell.Interop.IVsSolutionEvents,System.UInt32@)")]
        private void StartTrackingSolutionEvents()
        {
            var trackSol = Services.ServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (trackSol != null)
            {
                trackSol.AdviseSolutionEvents(this, out trackSolEventsCookie);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.UnadviseSolutionEvents(System.UInt32)")]
        private void StopTrackingSolutionEvents()
        {
            var trackSol = Services.ServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (trackSol != null)
            {
                if (trackSolEventsCookie != 0)
                {
                    trackSol.UnadviseSolutionEvents(trackSolEventsCookie);
                    trackSolEventsCookie = 0;
                }
            }
        }

        /// <summary>
        ///     Event handler when we change any properties of the model. For now we'll handle just the renaming
        ///     the entity container name.
        /// </summary>
        private void OnModelChangesCommitted(object sender, EfiChangedEventArgs e)
        {
            var changeEnum = e.ChangeGroup.Changes.GetEnumerator();
            while (changeEnum.MoveNext())
            {
                // update operation?
                if (changeEnum.Current.Type == EfiChange.EfiChangeType.Update)
                {
                    // are we updating the entity container name?
                    var entityContainer = changeEnum.Current.Changed as SingleItemBinding<ConceptualEntityContainer>;
                    if (entityContainer != null)
                    {
                        // get the values from the EfiChange properties, use those to construct the arguments
                        var pair = changeEnum.Current.Properties[EntityContainerMapping.AttributeCdmEntityContainer];
                        var args = new ModelChangeEventArgs();
                        args.OldEntityContainerName = (string)pair.OldValue;

                        AfterEntityContainerNameChange(this, args);

                        // ignore any further action
                        continue;
                    }
                }

                // are we updating the metadata artifact processing value?
                var metadataArtifactProcessingValue = changeEnum.Current.Changed as DefaultableValue<string>;
                if (metadataArtifactProcessingValue != null)
                {
                    var mapProp = metadataArtifactProcessingValue.Parent as DesignerProperty;

                    if (mapProp != null
                        && mapProp.LocalName != null
                        && String.Compare(
                            mapProp.LocalName.Value, ConnectionDesignerInfo.AttributeMetadataArtifactProcessing,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var pair = changeEnum.Current.Properties[DesignerProperty.AttributeValue];
                        var args = new ModelChangeEventArgs();
                        args.OldMetadataArtifactProcessing = (string)pair.OldValue;
                        AfterMetadataArtifactProcessingChange(this, args);
                        continue;
                    }
                }
            }
        }

        #region IVsTrackProjectDocumentsEvents2 Helpers

        private static Project GetProjectFromArray(int cProjects, int fileIndex, IVsProject[] rgpProjects, int[] rgFirstIndices)
        {
            if (cProjects == 0)
            {
                return null;
            }
            var projIndex = 0;
            for (; projIndex + 1 < cProjects; projIndex++)
            {
                if (fileIndex > rgFirstIndices[projIndex]
                    && fileIndex < rgFirstIndices[projIndex + 1])
                {
                    break;
                }
            }
            var hierarchy = rgpProjects[projIndex] as IVsHierarchy;
            if (hierarchy == null)
            {
                return null;
            }
            return VSHelpers.GetProject(hierarchy);
        }

        #endregion

        #region IVsTrackProjectDocumentsEvents2 Members

        public int OnAfterAddDirectoriesEx(
            int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments,
            VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAddFilesEx(
            int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            var hr = VSConstants.S_OK;
            var handler = AfterAddFile;
            if (handler != null)
            {
                for (var fileCount = 0; fileCount < cFiles; fileCount++)
                {
                    var args = new ModelChangeEventArgs();
                    args.NewFileName = rgpszMkDocuments[fileCount];
                    args.ProjectObj = GetProjectFromArray(cProjects, fileCount, rgpProjects, rgFirstIndices);
                    if (args.ProjectObj == null)
                    {
                        continue;
                    }

                    hr = handler(this, args);
                }
            }
            return hr;
        }

        public int OnAfterRemoveDirectories(
            int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments,
            VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveFiles(
            int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments,
            VSREMOVEFILEFLAGS[] rgFlags)
        {
            var hr = VSConstants.S_OK;
            var handler = AfterRemoveFile;
            if (handler != null)
            {
                if (cFiles <= rgpszMkDocuments.Length)
                {
                    for (var fileCount = 0; fileCount < cFiles; fileCount++)
                    {
                        var args = new ModelChangeEventArgs();
                        args.OldFileName = rgpszMkDocuments[fileCount];
                        args.ProjectObj = GetProjectFromArray(cProjects, fileCount, rgpProjects, rgFirstIndices);
                        if (args.ProjectObj == null)
                        {
                            continue;
                        }

                        hr = handler(this, args);
                    }
                }
            }
            return hr;
        }

        public int OnAfterRenameDirectories(
            int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames,
            VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public int OnAfterRenameFiles(
            int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames,
            VSRENAMEFILEFLAGS[] rgFlags)
        {
            var hr = VSConstants.S_OK;
            var handler = AfterRenameFile;
            if (handler != null)
            {
                for (var fileCount = 0; fileCount < cFiles; fileCount++)
                {
                    var args = new ModelChangeEventArgs();
                    args.OldFileName = rgszMkOldNames[fileCount];
                    args.NewFileName = rgszMkNewNames[fileCount];
                    args.ProjectObj = GetProjectFromArray(cProjects, fileCount, rgpProjects, rgFirstIndices);
                    if (args.ProjectObj == null)
                    {
                        continue;
                    }

                    var newUri = Utils.FileName2Uri(args.NewFileName);
                    ModelManager modelManager = PackageManager.Package.ModelManager;
                    var artifact = modelManager.GetArtifact(newUri);
                    ModelManager tempModelManager = null;
                    try
                    {
                        if (artifact == null
                            && Path.GetExtension(args.NewFileName)
                                   .Equals(EntityDesignArtifact.ExtensionEdmx, StringComparison.CurrentCulture))
                        {
                            tempModelManager = new EntityDesignModelManager(new EFArtifactFactory(), new VSArtifactSetFactory());
                            artifact = tempModelManager.GetNewOrExistingArtifact(
                                newUri, new StandaloneXmlModelProvider(PackageManager.Package));
                        }
                        args.Artifact = artifact;

                        hr = handler(this, args);
                    }
                    finally
                    {
                        if (tempModelManager != null)
                        {
                            tempModelManager.Dispose();
                        }
                    }
                }
            }
            return hr;
        }

        public int OnAfterSccStatusChanged(
            int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryAddDirectories(
            IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags,
            VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryAddFiles(
            IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags,
            VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            if (rgResults != null)
            {
                for (var i = 0; i < cFiles; i++)
                {
                    rgResults[i] = VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddOK;
                }
            }
            if (pSummaryResult != null
                && pSummaryResult.Length > 0)
            {
                pSummaryResult[0] = VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddOK;
            }
            return VSConstants.S_OK;
        }

        public int OnQueryRemoveDirectories(
            IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags,
            VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryRemoveFiles(
            IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags,
            VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            var hr = VSConstants.S_OK;
            var handler = QueryRemoveFile;
            if (handler != null)
            {
                if (cFiles <= rgpszMkDocuments.Length)
                {
                    for (var fileCount = 0; fileCount < cFiles; fileCount++)
                    {
                        var args = new ModelChangeEventArgs();
                        args.OldFileName = rgpszMkDocuments[fileCount];
                        args.ProjectObj = VSHelpers.GetProject(pProject as IVsHierarchy);
                        if (args.ProjectObj == null)
                        {
                            continue;
                        }

                        hr = handler(this, args);
                    }
                }
            }
            return hr;
        }

        public int OnQueryRenameDirectories(
            IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags,
            VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryRenameFiles(
            IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags,
            VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region ITrackEdmxUIEvents Members

        public int OnBeforeGenerateDDL(Project project, EFArtifact artifact)
        {
            var args = new ModelChangeEventArgs();
            args.ProjectObj = project;
            args.Artifact = artifact;
            if (BeforeGenerateDDL != null)
            {
                return BeforeGenerateDDL(this, args);
            }
            else
            {
                return 0;
            }
        }

        public int OnBeforeValidateModel(Project project, EFArtifact artifact, bool isCurrentlyBuilding)
        {
            var args = new ModelChangeEventArgs();
            args.ProjectObj = project;
            args.Artifact = artifact;
            args.IsCurrentlyBuilding = isCurrentlyBuilding;
            if (BeforeValidateModel != null)
            {
                return BeforeValidateModel(this, args);
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region IVsRunningDocTableEvents3 Members

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(
            uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew,
            uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            var hr = VSConstants.S_OK;
            if (AfterSaveFile != null)
            {
                var docTable = Services.IVsRunningDocumentTable;
                uint rdtFlags, readLocks, editLocks, itemId;
                string fileName;
                IVsHierarchy hierarchy;
                var docData = IntPtr.Zero;

                try
                {
                    hr = docTable.GetDocumentInfo(
                        docCookie, out rdtFlags, out readLocks, out editLocks, out fileName, out hierarchy, out itemId, out docData);
                }
                finally
                {
                    if (docData != IntPtr.Zero)
                    {
                        Marshal.Release(docData);
                    }
                }

                if (hr == VSConstants.S_OK
                    && hierarchy != null)
                {
                    var projectObj = VSHelpers.GetProject(hierarchy);
                    if (projectObj != null)
                    {
                        var args = new ModelChangeEventArgs();
                        args.DocCookie = docCookie;
                        args.Artifact = PackageManager.Package.ModelManager.GetArtifact(Utils.FileName2Uri(fileName));
                        args.ProjectObj = projectObj;
                        hr = AfterSaveFile(this, args);
                    }
                }
            }
            return hr;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsSolutionEvents Members

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            if (AfterOpenProject != null)
            {
                var args = new ModelChangeEventArgs();
                try
                {
                    args.ProjectObj = VSHelpers.GetProject(pRealHierarchy);
                }
                catch (ArgumentException)
                {
                    return VSConstants.E_NOTIMPL;
                }

                if (args.ProjectObj == null)
                {
                    return VSConstants.E_NOTIMPL;
                }
                return AfterOpenProject(this, args);
            }

            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            if (AfterOpenProject != null)
            {
                var args = new ModelChangeEventArgs();
                try
                {
                    args.ProjectObj = VSHelpers.GetProject(pHierarchy);
                }
                catch (ArgumentException)
                {
                    return VSConstants.E_NOTIMPL;
                }

                if (args.ProjectObj == null)
                {
                    return VSConstants.E_NOTIMPL;
                }
                return AfterOpenProject(this, args);
            }

            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            var handler = BeforeCloseProject;
            if (handler != null)
            {
                var args = new ModelChangeEventArgs();
                try
                {
                    args.ProjectObj = VSHelpers.GetProject(pHierarchy);
                }
                catch (ArgumentException)
                {
                    return VSConstants.E_NOTIMPL;
                }

                if (args.ProjectObj == null)
                {
                    return VSConstants.E_NOTIMPL;
                }
                return handler(this, args);
            }

            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            var handler = BeforeCloseProject;
            if (handler != null)
            {
                var args = new ModelChangeEventArgs();
                try
                {
                    args.ProjectObj = VSHelpers.GetProject(pRealHierarchy);
                }
                catch (ArgumentException)
                {
                    return VSConstants.E_NOTIMPL;
                }

                if (args.ProjectObj == null)
                {
                    return VSConstants.E_NOTIMPL;
                }
                return handler(this, args);
            }

            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
