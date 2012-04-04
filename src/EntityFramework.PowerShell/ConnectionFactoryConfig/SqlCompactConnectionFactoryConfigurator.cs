namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Diagnostics.Contracts;
    using System.Xml.Linq;
    using EnvDTE;

    /// <summary>
    /// Handles adding entries to .config files of a Visual Studio project to configure the
    /// default connection factory to use SQL Server Compact Edition.
    /// </summary>
    public class SqlCompactConnectionFactoryConfigurator
    {
        /// <summary>
        /// Modifies all "app.config" and "web.config" items in the given project to have a
        /// "defaultConnectionFactory" entry specifying the use of SQL Server Compact Edition.
        /// </summary>
        /// <remarks>
        /// This code is usually invoked on installation of the Entity Framework nuget package into a project.
        /// </remarks>
        /// <param name="project">The Visual Studio project to configure.</param>
        [CLSCompliant(false)]
        public SqlCompactConnectionFactoryConfigurator(Project project)
        {
            Contract.Requires(project != null);

            var manipulator = new ConfigFileManipulator();
            var processor = new ConfigFileProcessor();

            new ConfigFileFinder().FindConfigFiles(
                project.ProjectItems,
                i => processor.ProcessConfigFile(
                    i, new Func<XDocument, bool>[]
                           {
                               c => manipulator.AddOrUpdateConnectionFactoryInConfig(
                                   c,
                                   new ConnectionFactorySpecification(
                                        ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                                        ConnectionFactorySpecification.SqlCompactProviderName))

                           }));
        }
    }
}
