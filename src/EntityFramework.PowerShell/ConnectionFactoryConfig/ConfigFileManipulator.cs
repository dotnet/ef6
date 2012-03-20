namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Manipulates the XML of .config files to add Entity Framework "defaultConnectionFactory" entries
    /// and to ensure that the "entityFramework" section is up-to-date with the current EF assembly
    /// version.
    /// </summary>
    internal class ConfigFileManipulator
    {
        public const string DefaultConnectionFactoryName = "System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework";
        public const string SqlCompactConnectionFactoryName = "System.Data.Entity.Infrastructure.SqlCeConnectionFactory, EntityFramework";
        public const string SqlCompactProviderName = "System.Data.SqlServerCe.4.0";
        public const string ConfigurationElementName = "configuration";
        public const string EntityFrameworkElementName = "entityFramework";
        public const string DefaultConnectionFactoryElementName = "defaultConnectionFactory";
        public const string ParametersElementName = "parameters";
        public const string ParameterElementName = "parameter";
        public const string ConfigSectionsElementName = "configSections";
        public const string SectionElementName = "section";

        /// <summary>
        /// Checks whether or not the given XML document representing a .config file contains
        /// an EntityFramework "defaultConnectionFactory" entry or not. If no entry is found then one
        /// is added for SqlServerConnectionFactory and the given base connection string is added
        /// as the only constructor argument.
        /// </summary>
        /// <param name="config"> An XML document representing the config file.</param>
        /// <param name="baseConnectionString">
        /// The base connection string that will be passed as the argument to the connection factory constructor.
        /// </param>
        /// <returns>True if the document was modified; false if no change was made.</returns>
        public virtual bool AddConnectionFactoryToConfig(XDocument config, string baseConnectionString)
        {
            Contract.Requires(config != null);
            Contract.Requires(baseConnectionString != null);

            var entityFramework = config
                .GetOrCreateElement(ConfigurationElementName)
                .GetOrCreateElement(EntityFrameworkElementName);

            if (entityFramework.Elements(DefaultConnectionFactoryElementName).Any())
            {
                return false;
            }

            entityFramework
                .GetOrCreateElement(DefaultConnectionFactoryElementName,
                                    new XAttribute("type", DefaultConnectionFactoryName))
                .GetOrCreateElement(ParametersElementName)
                .GetOrCreateElement(ParameterElementName, new XAttribute("value", baseConnectionString));

            return true;
        }

        /// <summary>
        /// Sets the EntityFramework "defaultConnectionFactory" in the given XML document representing a
        /// .config file to use SQL Server Compact. This method differs from AddConnectionFactoryToConfig
        /// in that it always sets the entry to use SQL Compact even if it was already present and set to
        /// something else.
        /// </summary>
        /// <param name="config"> An XML document representing the config file.</param>
        /// <returns>True if the document was modified; false if no change was made.</returns>
        public virtual bool AddSqlCompactConnectionFactoryToConfig(XDocument config)
        {
            Contract.Requires(config != null);

            var connectionFactoryElement = config
                .GetOrCreateElement(ConfigurationElementName)
                .GetOrCreateElement(EntityFrameworkElementName)
                .GetOrCreateElement(DefaultConnectionFactoryElementName);

            connectionFactoryElement.ReplaceAttributes(new XAttribute("type", SqlCompactConnectionFactoryName));

            var parameterElement = connectionFactoryElement
                .GetOrCreateElement(ParametersElementName)
                .GetOrCreateElement(ParameterElementName);

            if (parameterElement.Attributes("value").Any(a => a.Value == SqlCompactProviderName))
            {
                // Assumption that if the attribute is already present and represents the SqlCompactProviderName
                // then we didn't actually change anything, so return false.
                return false;
            }

            parameterElement.ReplaceAttributes(new XAttribute("value", SqlCompactProviderName));
            return true;
        }

        /// <summary>
        /// Ensures that the config file has a defined "entityFramework" section and that it references
        /// the current version of the EntityFramework.dll assembly.
        /// </summary>
        /// <param name="config">An XML document representing the config file.</param>
        /// <param name="entityFrameworkVersion">The version of EntityFramework.dll to use.</param>
        /// <returns>True if the document was modified; false if no change was made.</returns>
        public virtual bool AddOrUpdateConfigSection(XDocument config, Version entityFrameworkVersion)
        {
            Contract.Requires(config != null);
            Contract.Requires(entityFrameworkVersion != null);

            var configSections = config
                .GetOrCreateElement(ConfigurationElementName)
                .GetOrCreateElement(ConfigSectionsElementName);

            var typeAttribute = configSections
                .Elements(SectionElementName)
                .Where(e => (string)e.Attribute("name") == EntityFrameworkElementName)
                .SelectMany(e => e.Attributes("type"))
                .FirstOrDefault();

            // Hard coding this so that we don't need to load EntityFramework.dll to get it.
            const string entityFrameworkSectionName = "System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089";

            var efSectionTypeName = string.Format(CultureInfo.InvariantCulture, entityFrameworkSectionName,
                                                  entityFrameworkVersion);

            if (typeAttribute != null)
            {
                if (efSectionTypeName.Equals(typeAttribute.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                typeAttribute.Value = efSectionTypeName;
            }
            else
            {
                configSections.Add(new XElement(SectionElementName, new XAttribute("name", EntityFrameworkElementName),
                                                new XAttribute("type", efSectionTypeName)));
            }

            return true;
        }
    }
}