// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure.VS
{
    using System;
    using System.Diagnostics;
    using EnvDTE;

    internal static class ProjectExtensions
    {
        /// <summary>
        ///     Return a project item corresponding to a string name.
        /// </summary>
        /// <param name="project">The project that the search will begin from.</param>
        /// <param name="itemName">The name of the item we are looking for.</param>
        /// <returns>A project item corresponding to the item, null if not found.</returns>
        public static ProjectItem GetProjectItemByName(this Project project, string itemName)
        {
            Debug.Assert(project != null, "Project is null.");
            Debug.Assert(!string.IsNullOrEmpty(itemName), "Search string is null.");

            return GetProjectItemByName(project.ProjectItems, itemName);
        }

        /// <summary>
        ///     Search all the items of the project tree
        ///     Return a project item corresponding to a string name.
        /// </summary>
        /// <param name="project">The project that the search will begin from.</param>
        /// <param name="itemName">The name of the item we are looking for.</param>
        /// <returns>A project item corresponding to the item, null if not found.</returns>
        private static ProjectItem GetProjectItemByName(ProjectItems projectItems, string itemName)
        {
            foreach (ProjectItem item in projectItems)
            {
                if (string.Equals(item.Name, itemName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return item;
                }

                if (item.ProjectItems != null)
                {
                    var projectSubItem = GetProjectItemByName(item.ProjectItems, itemName);
                    if (projectSubItem != null)
                    {
                        return projectSubItem;
                    }
                }
            }

            return null;
        }
    }
}
