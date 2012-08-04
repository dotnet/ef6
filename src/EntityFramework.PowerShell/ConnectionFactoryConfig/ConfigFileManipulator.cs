// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    ///     Manipulates the XML of .config files to add Entity Framework "defaultConnectionFactory" entries
    ///     and to ensure that the "entityFramework" section is up-to-date with the current EF assembly
    ///     version.
    /// </summary>
    internal class ConfigFileManipulator
    {
        public const string ConfigurationElementName = "configuration";
        public const string EntityFrameworkElementName = "entityFramework";
        public const string DefaultConnectionFactoryElementName = "defaultConnectionFactory";
        public const string ParametersElementName = "parameters";
        public const string ParameterElementName = "parameter";
        public const string ConfigSectionsElementName = "configSections";
        public const string SectionElementName = "section";
        public const string DbConfigurationElementName = "dbConfiguration";

        /// <summary>
        ///     Checks whether or not the given XML document representing a .config file contains
        ///     an EntityFramework "defaultConnectionFactory" entry or not. If no entry is found then one
        ///     is added for the given connection factory specification.
        /// </summary>
        /// <param name="config"> An XML document representing the config file. </param>
        /// <param name="specification"> Specifies the connection factory and constructor arguments to use. </param>
        /// <returns> True if the document was modified; false if no change was made. </returns>
        public virtual bool AddConnectionFactoryToConfig(XDocument config, ConnectionFactorySpecification specification)
        {
            Contract.Requires(config != null);
            Contract.Requires(specification != null);

            var entityFramework = config
                .GetOrCreateElement(ConfigurationElementName)
                .GetOrCreateElement(EntityFrameworkElementName);

            if (entityFramework.Elements(DefaultConnectionFactoryElementName).Any())
            {
                return false;
            }

            var factoryElement = entityFramework
                .GetOrCreateElement(
                    DefaultConnectionFactoryElementName,
                    new XAttribute("type", specification.ConnectionFactoryName));

            AddFactoryArguments(factoryElement, specification);

            return true;
        }

        /// <summary>
        ///     Sets the EntityFramework "defaultConnectionFactory" in the given XML document representing a
        ///     .config file to use th given specification. This method differs from AddConnectionFactoryToConfig
        ///     in that it always sets the entry to use the given specification even if it was already present
        ///     and set to something else.
        /// </summary>
        /// <param name="config"> An XML document representing the config file. </param>
        /// <param name="specification"> Specifies the connection factory and constructor arguments to use. </param>
        /// <returns> True if the document was modified; false if no change was made. </returns>
        public virtual bool AddOrUpdateConnectionFactoryInConfig(XDocument config, ConnectionFactorySpecification specification)
        {
            Contract.Requires(config != null);

            var connectionFactoryElement = config
                .GetOrCreateElement(ConfigurationElementName)
                .GetOrCreateElement(EntityFrameworkElementName)
                .GetOrCreateElement(DefaultConnectionFactoryElementName);

            var currentFactoryAttribute = connectionFactoryElement.Attribute("type");
            if (currentFactoryAttribute != null
                && specification.ConnectionFactoryName.Equals(currentFactoryAttribute.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            connectionFactoryElement.RemoveAll();
            connectionFactoryElement.Add(new XAttribute("type", specification.ConnectionFactoryName));

            AddFactoryArguments(connectionFactoryElement, specification);

            return true;
        }

        private void AddFactoryArguments(XElement factoryElement, ConnectionFactorySpecification specification)
        {
            if (specification.ConstructorArguments.Any())
            {
                var parametersElement = factoryElement.GetOrCreateElement(ParametersElementName);
                specification.ConstructorArguments.Each(
                    a => parametersElement.Add(new XElement(ParameterElementName, new XAttribute("value", a))));
            }
        }

        /// <summary>
        ///     Ensures that the config file has a defined "entityFramework" section and that it references
        ///     the current version of the EntityFramework.dll assembly.
        /// </summary>
        /// <param name="config"> An XML document representing the config file. </param>
        /// <param name="entityFrameworkVersion"> The version of EntityFramework.dll to use. </param>
        /// <returns> True if the document was modified; false if no change was made. </returns>
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
            const string entityFrameworkSectionName =
                "System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089";

            var efSectionTypeName = string.Format(
                CultureInfo.InvariantCulture, entityFrameworkSectionName,
                entityFrameworkVersion);

            if (typeAttribute != null)
            {
                var sectionElement = typeAttribute.Parent;
                var requirePermissionAttribute = sectionElement.Attribute("requirePermission");

                if (efSectionTypeName.Equals(typeAttribute.Value, StringComparison.InvariantCultureIgnoreCase)
                    && requirePermissionAttribute != null)
                {
                    return false;
                }

                typeAttribute.Value = efSectionTypeName;

                if (requirePermissionAttribute == null)
                {
                    sectionElement.Add(new XAttribute("requirePermission", false));
                }
            }
            else
            {
                configSections.Add(
                    new XElement(
                        SectionElementName, new XAttribute("name", EntityFrameworkElementName),
                        new XAttribute("type", efSectionTypeName), new XAttribute("requirePermission", false)));
            }

            return true;
        }
    }
}
