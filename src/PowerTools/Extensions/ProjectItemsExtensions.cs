namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using EnvDTE;

    internal static class ProjectItemsExtensions
    {
        public static ProjectItem GetItem(this ProjectItems projectItems, string name)
        {
            Contract.Requires(projectItems != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            return projectItems
                .Cast<ProjectItem>()
                .FirstOrDefault(
                    pi => string.Equals(pi.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
