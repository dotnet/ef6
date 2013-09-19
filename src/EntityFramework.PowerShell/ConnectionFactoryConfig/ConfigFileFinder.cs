// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Utilities;
    using EnvDTE;

    /// <summary>
    /// Finds Visual Studio project items that are .config files.
    /// </summary>
    internal class ConfigFileFinder
    {
        /// <summary>
        /// Finds any item called "app.config" or "web.config" in the given list of project items and performs the given action for each.
        /// </summary>
        public virtual void FindConfigFiles(ProjectItems items, Action<ProjectItem> action)
        {
            DebugCheck.NotNull(items);
            DebugCheck.NotNull(action);

            foreach (ProjectItem projectItem in items)
            {
                if (projectItem.IsConfig())
                {
                    action(projectItem);
                }
            }
        }
    }
}
