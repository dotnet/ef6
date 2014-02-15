// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.VisualStudio.Shell.Interop;

    interface IVsUtils
    {
        ProjectItem FindFirstProjectItemWithName(ProjectItems projectItems, string nameToMatch);
        uint GetProjectItemId(IVsHierarchy hierarchy, ProjectItem projectItem);
        IVsHierarchy GetVsHierarchy(Project project, IServiceProvider serviceProvider);
        DirectoryInfo GetProjectRoot(Project project, IServiceProvider serviceProvider);
        VisualStudioProjectSystem GetApplicationType(IServiceProvider serviceProvider, Project project);
        LangEnum GetLanguageForProject(Project project);
        void WriteCheckoutXmlFilesInProject(IDictionary<string, object> filesMap);
        void WriteCheckoutTextFilesInProject(IDictionary<string, object> filesMap);
    }
}
