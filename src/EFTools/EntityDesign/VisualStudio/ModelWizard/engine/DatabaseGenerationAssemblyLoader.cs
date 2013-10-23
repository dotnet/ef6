// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.DatabaseGeneration;
    using VSLangProj80;
    using VsWebSite;

    internal class DatabaseGenerationAssemblyLoader : IAssemblyLoader
    {
        private readonly bool _isWebsite;
        private readonly Dictionary<string, string> _assembliesInstalledUnderVisualStudio;
        private readonly Dictionary<string, Reference3> _projectReferenceLookup;
        private readonly Dictionary<string, AssemblyReference> _websiteReferenceLookup;

        internal DatabaseGenerationAssemblyLoader(Project project, string vsInstallPath)
        {
            _assembliesInstalledUnderVisualStudio = new Dictionary<string, string>();
            // For these DLLs we should use the version pre-installed under the VS directory,
            // not whatever reference the project may have
            _assembliesInstalledUnderVisualStudio.Add(
                "ENTITYFRAMEWORK", Path.Combine(vsInstallPath, "EntityFramework.dll"));
            _assembliesInstalledUnderVisualStudio.Add(
                "ENTITYFRAMEWORK.SQLSERVER", Path.Combine(vsInstallPath, "EntityFramework.SqlServer.dll"));
            _assembliesInstalledUnderVisualStudio.Add(
                "ENTITYFRAMEWORK.SQLSERVERCOMPACT", Path.Combine(vsInstallPath, "EntityFramework.SqlServerCompact.dll"));

            _projectReferenceLookup = new Dictionary<string, Reference3>();
            _websiteReferenceLookup = new Dictionary<string, AssemblyReference>();
            if (project != null)
            {
                var vsProject = project.Object as VSProject2;
                var vsWebSite = project.Object as VSWebSite;
                if (vsProject != null)
                {
                    _isWebsite = false;
                    CacheProjectReferences(vsProject);
                }
                else if (vsWebSite != null)
                {
                    _isWebsite = true;
                    CacheWebsiteReferences(vsWebSite);
                }
            }
        }

        private void CacheProjectReferences(VSProject2 vsProject)
        {
            foreach (Reference3 reference in vsProject.References)
            {
                if (_assembliesInstalledUnderVisualStudio.ContainsKey(reference.Identity.ToUpperInvariant()))
                {
                    // Ignore these DLLs - should be loaded using _assembliesInstalledUnderVisualStudio instead
                    continue;
                }

                if (reference.Resolved
                    && !string.IsNullOrEmpty(reference.Path))
                {
                    _projectReferenceLookup.Add(reference.Identity, reference);
                }
            }
        }

        private void CacheWebsiteReferences(VSWebSite vsWebSite)
        {
            foreach (AssemblyReference reference in vsWebSite.References)
            {
                // FullPath is non-empty for everything except GAC'd assemblies, which our schema context does not care about.
                // We will use this to determine the assembly name without path or extension, which is equivalent to the 'Identity'.
                var indexOfLastBackslash = reference.FullPath.LastIndexOf('\\');
                var startOfAssemblyName = indexOfLastBackslash != -1 ? indexOfLastBackslash + 1 : 0;
                var assemblyName = Path.GetFileNameWithoutExtension(reference.FullPath.Substring(startOfAssemblyName));

                if (_assembliesInstalledUnderVisualStudio.ContainsKey(assemblyName.ToUpperInvariant()))
                {
                    // Ignore these DLLs - should be loaded using _assembliesInstalledUnderVisualStudio instead
                    continue;
                }

                if (!_websiteReferenceLookup.ContainsKey(assemblyName))
                {
                    _websiteReferenceLookup.Add(assemblyName, reference);
                }
            }
        }

        #region IAssemblyLoader Members

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        public Assembly LoadAssembly(string assemblyName)
        {
            var pathToLoad = GetAssemblyPath(assemblyName);
            if (!string.IsNullOrEmpty(pathToLoad))
            {
                if (File.Exists(pathToLoad))
                {
                    return Assembly.LoadFrom(pathToLoad);
                }
            }
            return null;
        }

        #endregion

        internal string GetAssemblyPath(string assemblyName)
        {
            string pathToAssembly;
            if (_assembliesInstalledUnderVisualStudio.TryGetValue(assemblyName.ToUpperInvariant(), out pathToAssembly))
            {
                return pathToAssembly;
            }

            if (_isWebsite)
            {
                AssemblyReference assemblyReference;
                if (_websiteReferenceLookup.TryGetValue(assemblyName, out assemblyReference))
                {
                    return assemblyReference.FullPath;
                }
            }
            else
            {
                Reference3 assemblyReference;
                if (_projectReferenceLookup.TryGetValue(assemblyName, out assemblyReference))
                {
                    return assemblyReference.Path;
                }
            }

            return null;
        }
    }
}
