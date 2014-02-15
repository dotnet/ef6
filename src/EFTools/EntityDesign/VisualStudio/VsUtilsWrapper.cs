// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class VsUtilsWrapper : IVsUtils
    {
        public ProjectItem FindFirstProjectItemWithName(ProjectItems projectItems, string nameToMatch)
        {
            return VsUtils.FindFirstProjectItemWithName(projectItems, nameToMatch);
        }

        public DirectoryInfo GetProjectRoot(Project project, IServiceProvider serviceProvider)
        {
            return VsUtils.GetProjectRoot(project, serviceProvider);
        }

        public VisualStudioProjectSystem GetApplicationType(IServiceProvider serviceProvider, Project project)
        {
            return VsUtils.GetApplicationType(serviceProvider, project);
        }

        public LangEnum GetLanguageForProject(Project project)
        {
            return VsUtils.GetLanguageForProject(project);
        }

        public uint GetProjectItemId(IVsHierarchy hierarchy, ProjectItem projectItem)
        {
            return VsUtils.GetProjectItemId(hierarchy, projectItem);
        }

        public IVsHierarchy GetVsHierarchy(Project project, IServiceProvider serviceProvider)
        {
            return VsUtils.GetVsHierarchy(project, serviceProvider);
        }

        public void WriteCheckoutXmlFilesInProject(IDictionary<string, object> filesMap)
        {
            VsUtils.WriteCheckoutXmlFilesInProject(filesMap);
        }

        public void WriteCheckoutTextFilesInProject(IDictionary<string, object> filesMap)
        {
            VsUtils.WriteCheckoutTextFilesInProject(filesMap);
        }
    }
}
