// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.IO;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Common;

    interface IVsUtils
    {
        ProjectItem FindFirstProjectItemWithName(ProjectItems projectItems, string nameToMatch);
        DirectoryInfo GetProjectRoot(Project project, IServiceProvider serviceProvider);
        VisualStudioProjectSystem GetApplicationType(IServiceProvider serviceProvider, Project project);
        LangEnum GetLanguageForProject(Project project);
    }
}
