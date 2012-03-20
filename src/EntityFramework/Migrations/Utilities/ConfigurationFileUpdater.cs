namespace System.Data.Entity.Migrations.Utilities
{
    using System.Data.Entity.Migrations.Extensions;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;

    /// <summary>
    ///     Utility class to prep the user's config file to run in an AppDomain
    /// </summary>
    internal class ConfigurationFileUpdater
    {
        private static readonly XNamespace Asm = "urn:schemas-microsoft-com:asm.v1";
        private static readonly XElement DependentAssemblyElement;

        static ConfigurationFileUpdater()
        {
            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();

            DependentAssemblyElement
                = new XElement(
                    Asm + "dependentAssembly",
                    new XElement(
                        Asm + "assemblyIdentity",
                        new XAttribute("name", "EntityFramework"),
                        new XAttribute("culture", "neutral"),
                        new XAttribute("publicKeyToken", "b77a5c561934e089")),
                    new XElement(
                        Asm + "codeBase",
                        new XAttribute("version", executingAssemblyName.Version.ToString()),
                        new XAttribute("href", executingAssemblyName.CodeBase)));
        }

        /// <summary>
        ///     Updates a config file by adding binding redirects for EntityFramework.dll.
        ///     This ensures that the user's code can be ran in an AppDomain and the exact
        ///     same version of the assembly will be used for both domains.
        /// </summary>
        /// <param name = "configurationFile">That path of the user's config file. Can also be null or a path to an non-existent file.</param>
        /// <returns>The path of the updated config file. It is the caller's responsibility to delete this.</returns>
        public string Update(string configurationFile)
        {
            var fileExists = !string.IsNullOrWhiteSpace(configurationFile) && File.Exists(configurationFile);
            var configuration
                = fileExists
                    ? XDocument.Load(configurationFile)
                    : new XDocument();

            configuration.GetOrAddElement("configuration")
                .GetOrAddElement("runtime")
                .GetOrAddElement(Asm + "assemblyBinding")
                .Add(DependentAssemblyElement);

            var newConfigurationFile = Path.GetTempFileName();

            if (fileExists)
            {
                File.Delete(newConfigurationFile);
                newConfigurationFile
                    = Path.Combine(
                        Path.GetDirectoryName(configurationFile),
                        Path.GetFileName(newConfigurationFile));
            }

            configuration.Save(newConfigurationFile);

            return newConfigurationFile;
        }
    }
}