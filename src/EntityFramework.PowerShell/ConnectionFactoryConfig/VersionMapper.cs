namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Diagnostics.Contracts;
    using System.Runtime.Versioning;
    using EnvDTE;

    /// <summary>
    ///     Maps target framework version to the version of EntityFramework.dll that gets installed by NuGet.
    /// </summary>
    internal class VersionMapper
    {
        /// <summary>
        ///     Returns the version of EntityFramework.dll that is installed by NuGet on the .NET Framework version targetted by the given project.
        /// </summary>
        public Version GetEntityFrameworkVersion(Project project)
        {
            Contract.Requires(project != null);

            // This gets the correct version of the most recent assembly based on shared version info.
            // For the older assembly the string is hard coded because there is nowhere to pull it from.
            var targetFrameworkVersion
                = (string)project.Properties.Item("TargetFrameworkMoniker").Value;

            return
                new FrameworkName(targetFrameworkVersion).Version < new Version(4, 5)
                    ? new Version("4.4.0.0")
                    : GetType().Assembly.GetName().Version;
        }
    }
}
