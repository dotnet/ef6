// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics;
    using EnvDTE;

    /// <summary>
    ///     Creates an <see cref="ExecutorWrapper" /> that can be used to execute the user's code contained
    ///     in the specified project.
    /// </summary>
    internal class ProjectExecutionContext : IDisposable
    {
        private readonly AppDomain _domain;
        private readonly ExecutorWrapper _executor;

        public ProjectExecutionContext(Project project, IServiceProvider serviceProvider)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(!VsUtils.IsMiscellaneousProject(project), "project is misc files project.");

            _domain = AppDomain.CreateDomain(
                "ProjectExecutionContextDomain",
                null,
                new AppDomainSetup
                    {
                        ApplicationBase = VsUtils.GetProjectTargetDir(project, serviceProvider),
                        ConfigurationFile = VsUtils.GetProjectConfigurationFile(project, serviceProvider),
                        ShadowCopyFiles = "true" // Prevents locking
                    });

            var dataDirectory = VsUtils.GetProjectDataDirectory(project, serviceProvider);
            if (dataDirectory != null)
            {
                _domain.SetData("DataDirectory", dataDirectory);
            }

            _executor = new ExecutorWrapper(_domain, VsUtils.GetProjectTargetFileName(project));
        }

        public ExecutorWrapper Executor
        {
            get { return _executor; }
        }

        public void Dispose()
        {
            AppDomain.Unload(_domain);
        }
    }
}
