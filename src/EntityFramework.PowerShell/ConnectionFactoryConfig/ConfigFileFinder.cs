namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Diagnostics.Contracts;
    using EnvDTE;

    /// <summary>
    ///     Finds Visual Studio project items that are .config files.
    /// </summary>
    internal class ConfigFileFinder
    {
        /// <summary>
        ///     Finds any item called "app.config" or "web.config" in the given list of project items and performs the given action for each.
        /// </summary>
        public virtual void FindConfigFiles(ProjectItems items, Action<ProjectItem> action)
        {
            Contract.Requires(items != null);
            Contract.Requires(action != null);

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
