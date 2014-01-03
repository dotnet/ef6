// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using EnvDTE;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

    internal static class VisualStudioEdmxValidator
    {
        internal static bool LoadAndValidateAllFilesInProject(
            IVsHierarchy pHierProj, bool doEscherValidation, Func<EFArtifact, bool> shouldValidateArtifact)
        {
            // clear all errors for this project
            ErrorListHelper.ClearHierarchyErrors(pHierProj);

            var fileFinder = new VSFileFinder(EntityDesignArtifact.ExtensionEdmx);
            fileFinder.FindInProject(pHierProj);

            var edmxFilesToValidate = new List<VSFileFinder.VSFileInfo>(fileFinder.MatchingFiles);

            return LoadAndValidateFiles(edmxFilesToValidate, doEscherValidation, shouldValidateArtifact);
        }

        internal static bool LoadAndValidateFiles(params Uri[] uris)
        {
            var filesToValidate = new List<VSFileFinder.VSFileInfo>();
            foreach (var uri in uris)
            {
                Project project;
                IVsHierarchy projectHierarchy;
                uint itemId;
                bool isDocumentInProject;
                VSFileFinder.VSFileInfo fileInfo;

                VSHelpers.GetProjectAndFileInfoForPath(
                    uri.LocalPath, PackageManager.Package, out projectHierarchy, out project, out itemId, out isDocumentInProject);
                fileInfo.Hierarchy = projectHierarchy;
                fileInfo.ItemId = itemId;
                fileInfo.Path = uri.LocalPath;

                filesToValidate.Add(fileInfo);
            }

            return LoadAndValidateFiles(filesToValidate, doEscherValidation: true, shouldValidateArtifact: a => true);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool LoadAndValidateFiles(
            IEnumerable<VSFileFinder.VSFileInfo> edmxFilesToValidate, bool doEscherValidation, Func<EFArtifact, bool> shouldValidateArtifact)
        {
            var validationSuccessful = true;

            // load all the artifacts, and clear out the error list for them.
            using (var modelManager = new EntityDesignModelManager(new VSArtifactFactory(), new VSArtifactSetFactory()))
            {
                foreach (var vsFileInfo in edmxFilesToValidate)
                {
                    var uri = Utils.FileName2Uri(vsFileInfo.Path);
                    try
                    {
                        var artifact = GetArtifactForValidation(uri, vsFileInfo.Hierarchy, modelManager);
                        if (artifact != null
                            && shouldValidateArtifact(artifact))
                        {
                            // we need to continue validating even if validation for an artifact failed so just
                            // set the flag and continue validating.
                            if (!ValidateArtifactAndWriteErrors(artifact, vsFileInfo, doEscherValidation))
                            {
                                validationSuccessful = false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // an exception occurred loading the document, so add an error for it into the error pane.
                        var errorList = ErrorListHelper.GetSingleDocErrorList(vsFileInfo.Hierarchy, vsFileInfo.ItemId);

                        Debug.Assert(errorList != null, "errorList is null!");

                        errorList.AddItem(
                            EFModelErrorTaskFactory
                                .CreateErrorTask(uri.LocalPath, e, vsFileInfo.Hierarchy, vsFileInfo.ItemId));

                        validationSuccessful = false;
                    }
                }
            }

            return validationSuccessful;
        }

        private static bool ValidateArtifactAndWriteErrors(EFArtifact artifact, VSFileFinder.VSFileInfo vsFileInfo, bool doEscherValidation)
        {
            return ValidateArtifactAndWriteErrors(artifact, vsFileInfo.Hierarchy, vsFileInfo.ItemId, doEscherValidation);
        }

        private static bool ValidateArtifactAndWriteErrors(
            EFArtifact artifact, IVsHierarchy hierarchy, uint itemId, bool doEscherValidation)
        {
            Debug.Assert(artifact != null, "artifact != null!");
            Debug.Assert(hierarchy != null, "project hierarchy is null!");
            Debug.Assert(itemId != VSConstants.VSITEMID_NIL, "itemid is nil");

            var errorList = ErrorListHelper.GetSingleDocErrorList(hierarchy, itemId);
            Debug.Assert(errorList != null, "Couldn't get error list for artifact " + artifact.Uri);

            errorList.Clear();

            var artifactSet = (EntityDesignArtifactSet)artifact.ArtifactSet;
            Debug.Assert(
                artifactSet.Artifacts.OfType<EntityDesignArtifact>().Count() == 1,
                "Expected there is 1 instance of EntityDesignArtifact; Actual:" +
                artifactSet.Artifacts.OfType<EntityDesignArtifact>().Count());

            VsUtils.EnsureProvider(artifact);
            ((EntityDesignModelManager)artifact.ModelManager)
                .ValidateAndCompileMappings(artifactSet, doEscherValidation);

            var errors = artifactSet.GetArtifactOnlyErrors(artifact);

            if (errors.Count > 0)
            {
                ErrorListHelper.AddErrorInfosToErrorList(errors, hierarchy, itemId, errorList);
                return false;
            }

            return true;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsHierarchy.GetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider@)")]
        private static EFArtifact GetArtifactForValidation(Uri uri, IVsHierarchy hierarchy, ModelManager modelManager)
        {
            IServiceProvider oleServiceProvider = null;
            var modelListener = PackageManager.Package.ModelChangeEventListener;
            hierarchy.GetSite(out oleServiceProvider);
            System.IServiceProvider sp = new ServiceProvider(oleServiceProvider);
            var escherDocData = VSHelpers.GetDocData(sp, uri.LocalPath) as IEntityDesignDocData;

            EFArtifact artifact = null;
            //
            // If we opened the document with Escher, then use the XmlEditor's xlinq tree
            // If we opened the document with the xml editor, but not escher, then 
            // we don't want to use the XmlEditor's xlinq tree, because then we would be receiving events when
            // the document changes, and we currently don't support that.
            //

            if (escherDocData != null)
            {
                artifact = PackageManager.Package.ModelManager.GetNewOrExistingArtifact(
                    uri, new VSXmlModelProvider(PackageManager.Package, PackageManager.Package));
                if (modelListener != null)
                {
                    modelListener.OnBeforeValidateModel(VSHelpers.GetProject(hierarchy), artifact, true);
                }
            }
            else
            {
                if (Path.GetExtension(uri.LocalPath).Equals(EntityDesignArtifact.ExtensionEdmx, StringComparison.OrdinalIgnoreCase))
                {
                    // no doc data exists for this document, so load it into a temp model manager that can be disposed of when we're done. 
                    // Using the LoaderBasedXmlModelProvider will let us catch XML scanner and parser errors (the xml editor will try to 
                    // recover from these, and we won't know that the problem occurred. 
                    artifact = modelManager.GetNewOrExistingArtifact(uri, new StandaloneXmlModelProvider(PackageManager.Package));
                    if (modelListener != null)
                    {
                        modelListener.OnBeforeValidateModel(VSHelpers.GetProject(hierarchy), artifact, true);
                    }
                }
            }

            return artifact;
        }
    }
}
