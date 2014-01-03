// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.XLinqAnnotations;
    using Microsoft.Data.Entity.Design.UI.Views.Explorer;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    internal delegate void NavigateTo(EFObject efobject);

    /// <summary>
    ///     This class handles navigation from an ErrorTask/IEFModelErrorTask.  It will set focus to the appropriate
    ///     location in the design views based on the line/column number in the ErrorTask.
    /// </summary>
    internal static class EFModelErrorTaskNavigator
    {
        private static NavigateTo _dslDesignerOnNavigate;

        // this is the navigation delegate to navigate to a shape on the DSL surface.
        internal static NavigateTo DslDesignerOnNavigate
        {
            set { _dslDesignerOnNavigate = value; }
            get { return _dslDesignerOnNavigate; }
        }

        /// <summary>
        ///     This will handle navigation for an ErrorTask
        /// </summary>
        /// <param name="sender">This should be an instance of ErrorTask and implement IEFErrorTask</param>
        /// <param name="arguments"></param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal static void NavigateTo(Object sender, EventArgs arguments)
        {
            Debug.Assert(_dslDesignerOnNavigate != null, "DSL navigation delegate is null!");

            var task = sender as ErrorTask;
            if (task == null)
            {
                Debug.Fail("unable to cast sender to Task instance");
                return;
            }

            var efTask = task as IXmlModelErrorTask;
            if (efTask == null)
            {
                Debug.Fail("Unable to cast errorTask to IEFErrorTask");
                return;
            }

            if (String.IsNullOrEmpty(task.Document))
            {
                return;
            }

            var serviceProvider = efTask.ServiceProvider;

            var docDataObject = VSHelpers.GetDocData(serviceProvider, task.Document);
            if (docDataObject == null)
            {
                // document wasn't opened
                var uri = Utils.FileName2Uri(task.Document);
                if (Path.GetExtension(uri.LocalPath)
                    .Equals(EntityDesignArtifact.ExtensionEdmx, StringComparison.OrdinalIgnoreCase))
                {
                    // load a temp model to determine if the document is designer safe (only for EDMX, don't do this for converter docs)
                    var isArtifactDesignerSafe = IsUnloadedDocumentDesignerSafe(uri);
                    if (isArtifactDesignerSafe)
                    {
                        // this will open the document in escher
                        EscherDesignerNavigate(serviceProvider, uri, task);
                    }
                    else
                    {
                        // if the document cannot be displayed in the designer, so open it in the XML Editor and navigate to the error
                        var logicalView = VSConstants.LOGVIEWID_Primary;
                        VsShellUtilities.OpenDocumentWithSpecificEditor(
                            serviceProvider, uri.LocalPath,
                            CommonPackageConstants.xmlEditorGuid,
                            logicalView);
                        docDataObject = VSHelpers.GetDocData(serviceProvider, task.Document);
                        Debug.Assert(
                            docDataObject != null,
                            "attempt to open EDMX document in XML Editor - docDataObject should not be null");
                        VSHelpers.TextBufferNavigateTo(
                            serviceProvider, docDataObject, logicalView, task.Line,
                            task.Column);
                    }
                }
                else
                {
                    // document is not EDMX, so navigate to the document in the docdata's primary editor
                    VsShellUtilities.OpenDocument(serviceProvider, task.Document);
                    docDataObject = VSHelpers.GetDocData(serviceProvider, task.Document);
                    Debug.Assert(
                        docDataObject != null,
                        "attempt to open non-EDMX document with primary Editor - docDataObject should not be null");
                    VSHelpers.TextBufferNavigateTo(
                        serviceProvider, docDataObject, VSConstants.LOGVIEWID_Primary,
                        task.Line, task.Column);
                }
            }
            else if (docDataObject is IEntityDesignDocData)
            {
                // attempt to get the artifact. This will return non-null if the document is open in either XML Editor or the designer
                var uri = Utils.FileName2Uri(task.Document);
                var artifact = PackageManager.Package.ModelManager.GetArtifact(uri);
                if (artifact == null)
                {
                    Debug.Fail("didn't find artifact for document opened in Escher");
                    return;
                }

                if (artifact.IsDesignerSafe)
                {
                    // navigate to the correct place in the Escher designer
                    EscherDesignerNavigate(serviceProvider, uri, task);
                }
                else
                {
                    // if the document cannot be displayed in the designer, so open it in the XML Editor and navigate to the error
                    var logicalView = VSConstants.LOGVIEWID_Primary;
                    VsShellUtilities.OpenDocumentWithSpecificEditor(
                        serviceProvider, uri.LocalPath,
                        CommonPackageConstants.xmlEditorGuid, logicalView);
                    docDataObject = VSHelpers.GetDocData(serviceProvider, task.Document);
                    Debug.Assert(
                        docDataObject != null,
                        "EDMX document already open - attempt to open in XML Editor resulted in null docDataObject");
                    VSHelpers.TextBufferNavigateTo(serviceProvider, docDataObject, logicalView, task.Line, task.Column);
                }
            }
            else
            {
                // document is opened, but not in Escher, so navigate to the document in the text editor
                VSHelpers.TextBufferNavigateTo(
                    serviceProvider, docDataObject, VSConstants.LOGVIEWID_Primary, task.Line,
                    task.Column);
            }
        }

        private static void EscherDesignerNavigate(IServiceProvider serviceProvider, Uri uri, ErrorTask task)
        {
            //
            // Try to open the document in Escher. If the designer is already open, then this call will only activate the 
            // frame; it might fire a selection change event but it will not reload the artifact.
            //
            // Do this first so if there is any problem in the code below with out-of-date line numbers, the document will open
            // if it is closed, and the error list will be refreshed with correct line-numbers.
            //
            IVsUIHierarchy ppHierOpen;
            uint itemID;
            IVsWindowFrame windowFrame;
            // Check if there is already primary or logical view opened for the document.
            // If not, open the primary view for the document.
            if (
                !VsShellUtilities.IsDocumentOpen(
                    serviceProvider, uri.LocalPath, VSConstants.LOGVIEWID_Primary, out ppHierOpen, out itemID, out windowFrame)
                &&
                !VsShellUtilities.IsDocumentOpen(
                    serviceProvider, uri.LocalPath, PackageConstants.guidLogicalView, out ppHierOpen, out itemID, out windowFrame))
            {
                VsShellUtilities.OpenDocumentWithSpecificEditor(
                    serviceProvider, uri.LocalPath, PackageConstants.guidEscherEditorFactory, VSConstants.LOGVIEWID_Primary);
            }

            // sanity check.  If this is null, something bad happened with loading our package
            if (_dslDesignerOnNavigate == null)
            {
                Debug.Fail("_dslDesignerOnNavigate is null!");
                return;
            }

            // get the artifact from the model manager
            var artifact = PackageManager.Package.ModelManager.GetArtifact(uri);
            if (artifact == null)
            {
                Debug.Fail(
                    "We determined the artifact was designer-safe and we tried to open it in the designer, but where is the artifact?");
                return;
            }

            var xobject = artifact.FindXObjectForLineAndColumn(task.Line, task.Column);
            Debug.Assert(
                xobject != null,
                "couldn't get XObject for artifact " + uri + ", line " + task.Line + ", column " + task.Column);
            var efobject = ModelItemAnnotation.GetModelItem(xobject);

            // we got the root xobject node, so fix this up.
            if (efobject == null)
            {
                efobject = artifact;
                Debug.Assert(
                    task.Line <= 0 && task.Column <= 0,
                    "non-zero line/column didn't find an efobject linked to an xobject!");
            }

            var cModel = efobject.GetParentOfType(typeof(ConceptualEntityModel)) as ConceptualEntityModel;
            var sModel = efobject.GetParentOfType(typeof(StorageEntityModel)) as StorageEntityModel;
            var mModel = efobject.GetParentOfType(typeof(MappingModel)) as MappingModel;

            if (cModel != null)
            {
                var isComplexTypeOrFunctionImportOrChild = false;
                var obj = efobject;
                while (obj != null)
                {
                    if (obj is ComplexType
                        || obj is FunctionImport)
                    {
                        isComplexTypeOrFunctionImportOrChild = true;
                    }
                    obj = obj.Parent;
                }

                // node was in c-space, so navigate to appropriate node in the explorer and the designer
                ExplorerNavigationHelper.NavigateTo(efobject);

                if (_dslDesignerOnNavigate != null
                    && !isComplexTypeOrFunctionImportOrChild)
                {
                    _dslDesignerOnNavigate(efobject);
                }
            }
            else if (sModel != null)
            {
                // node is in s-space, so navigate to the appropriate node in the explorer.
                ExplorerNavigationHelper.NavigateTo(efobject);
            }
            else if (mModel != null)
            {
                // see if this is a function import error
                var fim = efobject.GetParentOfType(typeof(FunctionImportMapping)) as FunctionImportMapping;
                if (fim != null)
                {
                    ExplorerNavigationHelper.NavigateTo(fim.FunctionImportName.Target);
                }
                else
                {
                    // node was in m-space, so navigate to mapped c-space node, and show the mapping editor.
                    // show this first, so the node in the entity-designer will be highlighted.
                    PackageManager.Package.MappingDetailsWindow.Show();

                    if (_dslDesignerOnNavigate != null)
                    {
                        _dslDesignerOnNavigate(efobject);
                    }
                }
            }
        }

        /// <summary>
        ///     Loads document in an temporary model manager to see if it is designer safe.
        /// </summary>
        private static bool IsUnloadedDocumentDesignerSafe(Uri uri)
        {
            Debug.Assert(uri != null, "uri is null");

            using (var tempModelManager = new EntityDesignModelManager(new EFArtifactFactory(), new VSArtifactSetFactory()))
            {
                using (var xmlModelProvider = new StandaloneXmlModelProvider(PackageManager.Package))
                {
                    var artifact = tempModelManager.GetNewOrExistingArtifact(uri, xmlModelProvider);
                    Debug.Assert(artifact != null, "failed to get the artifact to determine if it is designer-safe");
                    if (artifact == null)
                    {
                        return true;
                    }
                    artifact.DetermineIfArtifactIsDesignerSafe();
                    return artifact.IsDesignerSafe;
                }
            }
        }
    }
}
