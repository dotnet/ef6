// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.VisualStudio.Shell.Interop;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class RetargetingHandler
    {
        private readonly IVsHierarchy _hierarchy;
        private readonly IServiceProvider _serviceProvider;

        public RetargetingHandler(IVsHierarchy hierarchy, IServiceProvider serviceProvider)
        {
            Debug.Assert(hierarchy != null, "hierarchy != null");
            Debug.Assert(serviceProvider != null, "serviceProvider != null");

            _hierarchy = hierarchy;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        ///     Project retargeting event handler.
        ///     1. Check the project type, return immediately if project is misc project or a project that does not support EF.
        ///     2. Find all the EDMX files in the project. Skip Data Services edmx files and linked files.
        ///     3. Sync all the namespaces based on the new target framework
        /// </summary>
        public void RetargetFilesInProject()
        {
            var project = VSHelpers.GetProject(_hierarchy);
            if (project == null
                || !VsUtils.EntityFrameworkSupportedInProject(project, _serviceProvider, allowMiscProject: false))
            {
                return;
            }

            var targetSchemaVersion = EdmUtils.GetEntityFrameworkVersion(project, _serviceProvider, useLatestIfNoEF: false);
            Debug.Assert(targetSchemaVersion != null, "schema version must not be null for projects that support EF");

            var documentMap = new Dictionary<string, object>();

            foreach (var vsFileInfo in GetEdmxFileInfos())
            {
                try
                {
                    var projectItem = VsUtils.GetProjectItem(_hierarchy, vsFileInfo.ItemId);

                    // skip the process for astoria edmx file or a linked edmx file
                    if (IsDataServicesEdmx(projectItem.get_FileNames(1))
                        || VsUtils.IsLinkProjectItem(projectItem))
                    {
                        continue;
                    }

                    var doc = RetargetFile(vsFileInfo.Path, targetSchemaVersion);
                    if (doc != null)
                    {
                        documentMap.Add(vsFileInfo.Path, doc);
                    }
                }
                catch (Exception ex)
                {
                    // TODO: When there is an exception; should we continue?
                    VsUtils.LogStandardError(
                        string.Format(CultureInfo.CurrentCulture, Resources.ErrorSynchingEdmxNamespaces, vsFileInfo.Path, ex.Message),
                        vsFileInfo.Path, 0, 0);
                    throw;
                }
            }

            WriteModifiedFiles(project, documentMap);
        }

        // protected virtual to allow mocking
        protected virtual IEnumerable<VSFileFinder.VSFileInfo> GetEdmxFileInfos()
        {
            // since this is about retargeting EDMX files on disk, no need to process other file extensions from any converters
            return new VSFileFinder(EntityDesignArtifact.EXTENSION_EDMX).FindInProject(_hierarchy);
        }

        // protected virtual to allow mocking
        protected virtual bool IsDataServicesEdmx(string filePath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(filePath), "Invalid filePath");

            return EdmUtils.IsDataServicesEdmx(filePath);
        }

        // protected virtual virtual to allow mocking
        protected virtual XmlDocument RetargetFile(string filePath, Version targetSchemaVersion)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(filePath), "Invalid filePath");
            Debug.Assert(targetSchemaVersion != null, "targetSchemaVersion != null");

            return MetadataConverterDriver.Instance.Convert(
                SafeLoadXmlFromPath(filePath), targetSchemaVersion);
        }

        // protected virtual to allow mocking
        protected virtual XmlDocument SafeLoadXmlFromPath(string filePath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(filePath), "Invalid filePath");

            return EdmUtils.SafeLoadXmlFromPath(filePath, preserveWhitespace: true);
        }

        // protected virtual to allow mocking
        protected virtual void WriteModifiedFiles(Project project, Dictionary<string, object> documentMap)
        {
            Debug.Assert(project != null, "project != null");
            Debug.Assert(documentMap != null, "documentMap != null");

            // Do bulk update here
            if (documentMap.Count > 0)
            {
                VsUtils.WriteCheckoutXmlFilesInProject(documentMap);
                VsUtils.LogOutputWindowPaneMessage(
                    project,
                    string.Format(CultureInfo.CurrentCulture, Resources.UpdateEdmxNamespacesSuccessful, project.Name));
            }
        }
    }
}
