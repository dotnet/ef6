// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using EnvDTE;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    // Helpful utility functions for finding and using the proper error list for a given document
    internal static class ErrorListHelper
    {
        // Uniquely identifies a given document so it can be associated with an error list
        private class SingleDocErrorListsIdentifier : Pair<IVsHierarchy, uint>
        {
            internal SingleDocErrorListsIdentifier(IVsHierarchy hier, uint ItemID)
                : base(hier, ItemID)
            {
            }
        }

        private enum MultiDocErrorListIdentifier
        {
            Extension,
            Wizard,
            MiscFiles
        }

        private static uint _rdtEventsCookie = 0;
        private static uint _solutionEventsCookie;
        private static uint _updateSolutionEventsCookie;

        private static readonly Dictionary<SingleDocErrorListsIdentifier, DesignerErrorList> _singleDocErrorLists =
            new Dictionary<SingleDocErrorListsIdentifier, DesignerErrorList>();

        private static readonly Dictionary<MultiDocErrorListIdentifier, DesignerErrorList> _multiDocErrorLists =
            new Dictionary<MultiDocErrorListIdentifier, DesignerErrorList>();

        public static DesignerErrorList GetExtensionErrorList(IServiceProvider serviceProvider)
        {
            return LazilyLoadMultiDocErrorList(serviceProvider, MultiDocErrorListIdentifier.Extension);
        }

        internal static DesignerErrorList MiscErrorList
        {
            get { return LazilyLoadMultiDocErrorList(PackageManager.Package, MultiDocErrorListIdentifier.MiscFiles); }
        }

        private static DesignerErrorList WizardErrorList
        {
            get { return LazilyLoadMultiDocErrorList(PackageManager.Package, MultiDocErrorListIdentifier.Wizard); }
        }

        private static DesignerErrorList LazilyLoadMultiDocErrorList(
            IServiceProvider serviceProvider, MultiDocErrorListIdentifier identifier)
        {
            var edmErrorList = GetMultiDocErrorList(identifier);
            if (edmErrorList == null)
            {
                edmErrorList = new DesignerErrorList(serviceProvider);
                _multiDocErrorLists.Add(identifier, edmErrorList);
            }
            return edmErrorList;
        }

        private static bool TryGetSingleDocErrorList(
            IVsHierarchy hier, uint ItemID, out SingleDocErrorListsIdentifier id, out DesignerErrorList errorList)
        {
            id = new SingleDocErrorListsIdentifier(hier, ItemID);
            if (_singleDocErrorLists == null)
            {
                errorList = null;
                return false;
            }
            else
            {
                return _singleDocErrorLists.TryGetValue(id, out errorList);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static DesignerErrorList GetSingleDocErrorList(IVsHierarchy hier, uint ItemID)
        {
            DesignerErrorList errorList = null;

            //
            // assert that the solution is open & not closing.  Otherwise, this method will add new entries into the _singleDocErrorLists. 
            // these entries will hold a ref to IVsHierarchy, and prevent the IVsHierarchy from being GC'd.
            //
            Debug.Assert(
                EdmSolutionEvents.Instance.IsAfterErrorListClearedOnSolutionClose == false,
                "unexpected solution state during GetSingleDocErrorList. This should only be called if the solution is open.");

            if (EdmSolutionEvents.Instance.IsAfterErrorListClearedOnSolutionClose == false)
            {
                _singleDocErrorLists.TryGetValue(new SingleDocErrorListsIdentifier(hier, ItemID), out errorList);

                // Create a new error list if one doesn't already exist for the given document
                if (errorList == null)
                {
                    errorList = new DesignerErrorList(PackageManager.Package);
                    _singleDocErrorLists.Add(new SingleDocErrorListsIdentifier(hier, ItemID), errorList);
                }
            }
            return errorList;
        }

        internal static DesignerErrorList GetSingleDocErrorList(Uri uri)
        {
            DesignerErrorList errorList = null;

            var currentProject = VSHelpers.GetProjectForDocument(uri.LocalPath, PackageManager.Package);

            if (currentProject != null)
            {
                var hierarchy = VsUtils.GetVsHierarchy(currentProject, Services.ServiceProvider);
                if (hierarchy != null)
                {
                    var fileFinder = new VSFileFinder(uri.LocalPath);
                    fileFinder.FindInProject(hierarchy);

                    Debug.Assert(fileFinder.MatchingFiles.Count <= 1, "Unexpected count of matching files in project");

                    foreach (var vsFileInfo in fileFinder.MatchingFiles)
                    {
                        if (vsFileInfo.Hierarchy == VsUtils.GetVsHierarchy(currentProject, Services.ServiceProvider))
                        {
                            errorList = GetSingleDocErrorList(vsFileInfo.Hierarchy, vsFileInfo.ItemId);
                            break;
                        }
                    }
                }
            }

            return errorList;
        }

        private static DesignerErrorList GetMultiDocErrorList(MultiDocErrorListIdentifier identifier)
        {
            DesignerErrorList errorList;
            _multiDocErrorLists.TryGetValue(identifier, out errorList);
            return errorList;
        }

        internal static void ClearHierarchyErrors(IVsHierarchy hierarchy)
        {
            foreach (var pair in _singleDocErrorLists)
            {
                if (pair.Key.First == hierarchy)
                {
                    pair.Value.Clear();
                }
            }
        }

        internal static void ClearAll()
        {
            foreach (var errorList in _singleDocErrorLists.Values)
            {
                errorList.Clear();
            }
            foreach (var errorList in _multiDocErrorLists.Values)
            {
                errorList.Clear();
            }
        }

        internal static void ClearErrorsForDocAcrossLists(IVsHierarchy pHier, uint itemId)
        {
            //
            // WARNING!  DO NOT call GetSingleDocErrorList here.  This code-path can get called after the solution-closed event has been raised. 
            // this means that calling GetSingleDocErrorList will create a new SingleDocErrorListsIdentifier and store it in a static dictionary, and 
            // that dictionary won't be cleared.  The SingleDocErrorListIdentifier will hold a pointer to the IVSHierarchy as well, preventing that 
            // from being GC'd.  This is bad.  Don't do this. 
            //
            SingleDocErrorListsIdentifier id;
            DesignerErrorList singleDocErrorList;
            if (TryGetSingleDocErrorList(pHier, itemId, out id, out singleDocErrorList))
            {
                singleDocErrorList.Clear();
                _singleDocErrorLists.Remove(id);
            }

            // now we have to look through each error in all of the "special" lists, such as the WizardErrorList.
            foreach (var multiDocErrorList in _multiDocErrorLists)
            {
                // don't clear out errors added by extensions - these are managed by the extension dispatchers
                // - see the finally block in DispatchLoadToExtensions() in MicrosoftDataEntityDesignDocData.cs
                // - see the finally block in DispatchSaveToExtensions() in MicrosoftDataEntityDesignDocData.cs
                if (multiDocErrorList.Key == MultiDocErrorListIdentifier.Extension)
                {
                    continue;
                }

                var errorTasksToRemove = new List<ErrorTask>();

                // go through all multi-doc error lists and mark the errors that are associated with the given document for removal
                foreach (var xmlModelErrorTask in multiDocErrorList.Value.Provider.Tasks.OfType<IXmlModelErrorTask>())
                {
                    var genericErrorTask = xmlModelErrorTask as ErrorTask;
                    if (genericErrorTask != null)
                    {
                        if (genericErrorTask.HierarchyItem == pHier
                            && xmlModelErrorTask.ItemID == itemId)
                        {
                            errorTasksToRemove.Add(genericErrorTask);
                        }
                    }
                }

                // go through the remove list and remove each error from the error task provider
                foreach (var errorTaskToRemove in errorTasksToRemove)
                {
                    multiDocErrorList.Value.Provider.Tasks.Remove(errorTaskToRemove);
                }
            }
        }

        internal static void ClearErrorsForDocAcrossLists(Uri uri)
        {
            var currentProject = VSHelpers.GetProjectForDocument(uri.LocalPath, PackageManager.Package);

            if (currentProject != null)
            {
                var hierarchy = VsUtils.GetVsHierarchy(currentProject, Services.ServiceProvider);
                if (hierarchy != null)
                {
                    var fileFinder = new VSFileFinder(uri.LocalPath);
                    fileFinder.FindInProject(hierarchy);

                    Debug.Assert(fileFinder.MatchingFiles.Count <= 1, "Unexpected count of matching files in project");
                    foreach (var vsFileInfo in fileFinder.MatchingFiles)
                    {
                        if (vsFileInfo.Hierarchy == hierarchy)
                        {
                            ClearErrorsForDocAcrossLists(vsFileInfo.Hierarchy, vsFileInfo.ItemId);
                            break;
                        }
                    }
                }
            }
        }

        internal static void RemoveAll()
        {
            ClearAll();
            _singleDocErrorLists.Clear();
        }

        internal static void AddErrorInfosToErrorList(
            ICollection<ErrorInfo> errors, IVsHierarchy vsHierarchy, uint itemID, bool bringErrorListToFront = false)
        {
            var errorList = GetSingleDocErrorList(vsHierarchy, itemID);
            Debug.Assert(errorList != null, "errorList is null!");

            if (errorList != null)
            {
                AddErrorInfosToErrorList(errors, vsHierarchy, itemID, errorList, bringErrorListToFront);
            }
        }

        internal static void AddErrorInfosToErrorList(
            ICollection<ErrorInfo> errors, IVsHierarchy vsHierarchy, uint itemID, DesignerErrorList errorList,
            bool bringErrorListToFront = false)
        {
            errorList.Clear();

            var errorCount = 0;
            foreach (var error in errors)
            {
                if (errorCount++ > 99)
                {
                    break;
                }
                var errorTask = EFModelErrorTaskFactory.CreateErrorTask(error, vsHierarchy, itemID);
                errorList.AddItem(errorTask);
            }

            if (errors.Count > 0)
            {
                errorList.Provider.Show();

                if (bringErrorListToFront)
                {
                    errorList.Provider.BringToFront();
                }
            }
        }

        /// <summary>
        ///     This method will log errors to the "Wizard" Multi-doc error list.
        /// </summary>
        internal static void LogUpdateModelWizardErrors(IEnumerable<EdmSchemaError> errors, string projectItemFileName)
        {
            var serviceProvider = Services.ServiceProvider;

            Debug.Assert(serviceProvider != null, "ServiceProvider does not exist");
            if (null != serviceProvider)
            {
                var solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
                if (null == solution)
                {
                    // If the serviceProvider cannot provide a solution we are being called outside
                    // of VS - so there is no Error List window in which to log the errors. This can 
                    // happen in e.g. Model.Tests and is not an error.
                    return;
                }
                var projectItem = VsUtils.GetProjectItemForDocument(projectItemFileName, serviceProvider);
                Debug.Assert(
                    projectItem != null,
                    "The filename you passed in, " + projectItemFileName + ", does not have a corresponding ProjectItem");
                if (projectItem != null)
                {
                    LogWizardErrors(errors, projectItem, MARKERTYPE.MARKER_OTHER_ERROR);
                }
            }
        }

        /// <summary>
        ///     This method will log errors to the "Wizard" Multi-doc error list.
        /// </summary>
        internal static void LogUpdateModelWizardError(ErrorInfo errorInfo, string projectItemFileName)
        {
            var serviceProvider = Services.ServiceProvider;

            Debug.Assert(serviceProvider != null, "ServiceProvider does not exist");
            if (null != serviceProvider)
            {
                var solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
                if (null == solution)
                {
                    // If the serviceProvider cannot provide a solution we are being called outside
                    // of VS - so there is no Error List window in which to log the errors. This can 
                    // happen in e.g. Model.Tests and is not an error.
                    return;
                }
                var projectItem = VsUtils.GetProjectItemForDocument(projectItemFileName, serviceProvider);
                Debug.Assert(
                    projectItem != null,
                    "The filename you passed in, " + projectItemFileName + ", does not have a corresponding ProjectItem");
                if (projectItem != null)
                {
                    var hierarchy = VsUtils.GetVsHierarchy(projectItem.ContainingProject, serviceProvider);
                    var itemId = VsUtils.GetProjectItemId(hierarchy, projectItem);
                    var errorList = WizardErrorList;
                    var errors = new List<ErrorInfo>();
                    errors.Add(errorInfo);
                    AddErrorInfosToErrorList(errors, hierarchy, itemId, errorList);
                }
            }
        }

        /// <summary>
        ///     This method will log errors to the "Extension" Multi-doc error list.
        /// </summary>
        internal static void LogExtensionErrors(IEnumerable<ExtensionError> errors, ProjectItem projectItem)
        {
            if (null == errors)
            {
                throw new ArgumentNullException("errors");
            }

            if (null == projectItem)
            {
                throw new ArgumentNullException("projectItem");
            }

            if (PackageManager.Package != null)
            {
                var hierarchy = VsUtils.GetVsHierarchy(projectItem.ContainingProject, Services.ServiceProvider);
                var itemId = VsUtils.GetProjectItemId(hierarchy, projectItem);

                var errorList = GetExtensionErrorList(PackageManager.Package);

                var errorCount = 0;

                foreach (var error in errors)
                {
                    // only display the first 100 errors.  VS gets really slow if you try to display more
                    if (errorCount++ > 99)
                    {
                        break;
                    }

                    var category = TaskErrorCategory.Message;
                    if (error.Severity == ExtensionErrorSeverity.Error)
                    {
                        category = TaskErrorCategory.Error;
                    }
                    else if (error.Severity == ExtensionErrorSeverity.Warning)
                    {
                        category = TaskErrorCategory.Warning;
                    }

                    string filePath = null;
                    var fullPathProperty = projectItem.Properties.Item("FullPath");
                    if (fullPathProperty != null)
                    {
                        filePath = fullPathProperty.Value as String;
                    }
                    if (filePath == null)
                    {
                        filePath = projectItem.Name;
                    }

                    var textSpan = new TextSpan();
                    textSpan.iStartLine = error.Line;
                    textSpan.iStartIndex = error.Column;
                    textSpan.iEndLine = error.Line;
                    textSpan.iEndIndex = error.Column;
                    errorList.AddItem(
                        EFModelErrorTaskFactory.CreateErrorTask(
                            filePath, error.Message, textSpan, category, hierarchy, itemId, MARKERTYPE.MARKER_OTHER_ERROR));
                }
            }
        }

        /// <summary>
        ///     This method will log errors to the "Wizard" Multi-doc error list.
        /// </summary>
        private static void LogWizardErrors(IEnumerable<EdmSchemaError> errors, ProjectItem projectItem, MARKERTYPE markerType)
        {
            if (null == errors)
            {
                throw new ArgumentNullException("errors");
            }

            if (null == projectItem)
            {
                throw new ArgumentNullException("projectItem");
            }

            if (PackageManager.Package != null)
            {
                var hierarchy = VsUtils.GetVsHierarchy(projectItem.ContainingProject, Services.ServiceProvider);
                var itemId = VsUtils.GetProjectItemId(hierarchy, projectItem);
                var errorList = WizardErrorList;

                var errorCount = 0;

                foreach (var error in errors)
                {
                    // only display the first 100 errors.  VS gets really slow if you try to display more
                    if (errorCount++ > 99)
                    {
                        break;
                    }

                    var category = TaskErrorCategory.Message;
                    if (error.Severity == EdmSchemaErrorSeverity.Error)
                    {
                        category = TaskErrorCategory.Error;
                    }
                    else if (error.Severity == EdmSchemaErrorSeverity.Warning)
                    {
                        category = TaskErrorCategory.Message;
                    }

                    string filePath = null;
                    var fullPathProperty = projectItem.Properties.Item("FullPath");
                    if (fullPathProperty != null)
                    {
                        filePath = fullPathProperty.Value as String;
                    }
                    if (filePath == null)
                    {
                        filePath = projectItem.Name;
                    }

                    var textSpan = new TextSpan();
                    textSpan.iStartLine = error.Line;
                    textSpan.iStartIndex = error.Column;
                    textSpan.iEndLine = error.Line + 1;
                    textSpan.iEndIndex = error.Column + 1;
                    errorList.AddItem(
                        EFModelErrorTaskFactory.CreateErrorTask(filePath, error.Message, textSpan, category, hierarchy, itemId, markerType));
                }
            }
        }

        /// <summary>
        ///     Logs SchemaErrors to the a special provider for wizard errors.  We won't ever clear this provider,
        ///     because we have no way to get the wizard errors back.  This way, they will persist for the time that
        ///     the document is open. This will log errors to the 'Wizard' multi-doc error list.
        /// </summary>
        internal static void LogWizardErrors(IEnumerable<EdmSchemaError> errors, ProjectItem projectItem)
        {
            LogWizardErrors(errors, projectItem, MARKERTYPE.MARKER_COMPILE_ERROR);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolutionBuildManager2.AdviseUpdateSolutionEvents(Microsoft.VisualStudio.Shell.Interop.IVsUpdateSolutionEvents,System.UInt32@)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.AdviseSolutionEvents(Microsoft.VisualStudio.Shell.Interop.IVsSolutionEvents,System.UInt32@)")]
        internal static void RegisterForNotifications()
        {
            // The document frame listens for the RunningDocumentTable events, and it will 
            // clean out errros for a document when it is closed. 

            // Register to receive solution events
            var IVsSolution = Services.IVsSolution;
            Debug.Assert(IVsSolution != null, "Failed to get IVsSolution!");

            if (IVsSolution != null)
            {
                IVsSolution.AdviseSolutionEvents(EdmSolutionEvents.Instance, out _solutionEventsCookie);
            }

            // Register to receive update solution events
            var solutionBuildManager = Services.IVsSolutionBuildManager2;
            Debug.Assert(solutionBuildManager != null, "Failed to get IVsSolutionBuildManager!");

            if (solutionBuildManager != null)
            {
                solutionBuildManager.AdviseUpdateSolutionEvents(EdmUpdateSolutionEvents.Instance, out _updateSolutionEventsCookie);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolutionBuildManager2.UnadviseUpdateSolutionEvents(System.UInt32)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.UnadviseSolutionEvents(System.UInt32)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.UnadviseRunningDocTableEvents(System.UInt32)")]
        internal static void UnregisterForNotifications()
        {
            // Unregister for RunningDocTable notifications
            if (_rdtEventsCookie != 0)
            {
                var IVsRDT = Services.IVsRunningDocumentTable;
                Debug.Assert(IVsRDT != null, "Failed to get IVsRunningDocumentTable");

                if (IVsRDT != null)
                {
                    IVsRDT.UnadviseRunningDocTableEvents(_rdtEventsCookie);
                }
            }

            // Unregister for solution events
            if (_solutionEventsCookie != 0)
            {
                var IVsSolution = Services.IVsSolution;
                Debug.Assert(IVsSolution != null, "Failed to get IVsSolution!");

                if (IVsSolution != null)
                {
                    IVsSolution.UnadviseSolutionEvents(_solutionEventsCookie);
                }
            }

            // Unregister for update solution events
            if (_updateSolutionEventsCookie != 0)
            {
                var solutionBuildManager = Services.IVsSolutionBuildManager2;
                Debug.Assert(solutionBuildManager != null, "Failed to get IVsSolutionBuildManager!");

                if (solutionBuildManager != null)
                {
                    solutionBuildManager.UnadviseUpdateSolutionEvents(_updateSolutionEventsCookie);
                }
            }
        }
    }
}
