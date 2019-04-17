// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Resources;
    using System.Data.Entity.TestHelpers;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.ServiceProcess;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Win32;
    using Moq;
    using Xunit;

    public class ConnectionFactoryConfigTests : TestBase
    {
        #region Well-known values

        // Hard-coding this rather than getting it dynamically because the product code gets it dynamically
        // and the tests need to make sure it gets the correct thing. This will need to be updated when the
        // assembly version number is incremented.
        private static readonly Version _net45EntityFrameworkVersion = new Version("6.0.0.0");
        private static readonly Version _net40EntityFrameworkVersion = new Version("6.0.0.0");

        private const string EntityFrameworkSectionFormat =
            "System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089";

        private static readonly string _net45EntityFrameworkSectionName = string.Format(
            CultureInfo.InvariantCulture, EntityFrameworkSectionFormat, _net45EntityFrameworkVersion);

        private static readonly string _net40EntityFrameworkSectionName = string.Format(
            CultureInfo.InvariantCulture, EntityFrameworkSectionFormat, _net40EntityFrameworkVersion);

        #endregion

        #region Updating entityFramework section and adding defaultConnectionFactory

        [Fact]
        public void ConfigFileProcessor_calls_both_AddConnectionFactoryToConfig_and_AddOrUpdateConfigSection_even_if_both_return_true()
        {
            // Test ensures code does not skip calling one method if the other returns true through the use of
            // short-circuit evaluation.
            RunTestWithTempFilename(
                tempFileName =>
                    {
                        var mockManipulator = new Mock<ConfigFileManipulator>();

                        mockManipulator
                            .Setup(
                                m =>
                                m.AddConnectionFactoryToConfig(
                                    It.IsAny<XDocument>(),
                                    It.IsAny<ConnectionFactorySpecification>())).
                             Returns(true);

                        mockManipulator
                            .Setup(m => m.AddOrUpdateConfigSection(It.IsAny<XDocument>(), It.IsAny<Version>())).
                             Returns(true);

                        new XDocument(new XElement("fake")).Save(tempFileName);

                        var mockedItem = new Mock<ProjectItem>();
                        mockedItem.Setup(p => p.get_FileNames(0)).Returns(tempFileName);

                        new ConfigFileProcessor()
                            .ProcessConfigFile(
                                mockedItem.Object, new Func<XDocument, bool>[]
                                    {
                                        c =>
                                        mockManipulator.Object.AddOrUpdateConfigSection(c, _net45EntityFrameworkVersion),
                                        c =>
                                        mockManipulator.Object.AddConnectionFactoryToConfig(
                                            c,
                                            new ConnectionFactorySpecification("F"))
                                    });

                        mockManipulator.Verify(
                            m =>
                            m.AddConnectionFactoryToConfig(
                                It.IsAny<XDocument>(),
                                It.IsAny<ConnectionFactorySpecification>()));
                        mockManipulator.Verify(m => m.AddOrUpdateConfigSection(It.IsAny<XDocument>(), _net45EntityFrameworkVersion));
                    });
        }

        [Fact]
        public void AddOrUpdateConnectionFactoryInConfig_does_nothing_if_correct_SQL_Compact_entry_already_exists()
        {
            var config = CreateConnectionFactoryConfigDoc(
                ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                new[] { ConnectionFactorySpecification.SqlCompactProviderName });

            var factoryAdded = new ConfigFileManipulator()
                .AddOrUpdateConnectionFactoryInConfig(
                    config,
                    new ConnectionFactorySpecification(
                        ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                        ConnectionFactorySpecification.SqlCompactProviderName));

            Assert.False(factoryAdded);
            Assert.Equal(
                ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                GetFactoryName(config));
            Assert.Equal(
                ConnectionFactorySpecification.SqlCompactProviderName,
                GetArgument(config));
        }

        [Fact]
        public void AddOrUpdateConnectionFactoryInConfig_adds_factory_if_no_factory_name_already_exists()
        {
            var config = CreateConnectionFactoryConfigDoc(null);

            var factoryAdded = new ConfigFileManipulator()
                .AddOrUpdateConnectionFactoryInConfig(
                    config,
                    new ConnectionFactorySpecification(
                        ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                        ConnectionFactorySpecification.SqlCompactProviderName));

            Assert.True(factoryAdded);
            Assert.Equal(
                ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                GetFactoryName(config));
            Assert.Equal(
                ConnectionFactorySpecification.SqlCompactProviderName,
                GetArgument(config));
        }

        [Fact]
        public void AddOrUpdateConnectionFactoryInConfig_adds_factory_if_entityFramework_element_is_missing()
        {
            var config =
                new XDocument(
                    new XElement(ConfigFileManipulator.ConfigurationElementName));

            var factoryAdded = new ConfigFileManipulator()
                .AddOrUpdateConnectionFactoryInConfig(
                    config,
                    new ConnectionFactorySpecification(
                        ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                        ConnectionFactorySpecification.SqlCompactProviderName));

            Assert.True(factoryAdded);
            Assert.Equal(
                ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                GetFactoryName(config));
            Assert.Equal(
                ConnectionFactorySpecification.SqlCompactProviderName,
                GetArgument(config));
        }

        [Fact]
        public void AddOrUpdateConnectionFactoryInConfig_adds_factory_if_configuration_element_is_missing()
        {
            var config = new XDocument();

            var factoryAdded = new ConfigFileManipulator()
                .AddOrUpdateConnectionFactoryInConfig(
                    config,
                    new ConnectionFactorySpecification(
                        ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                        ConnectionFactorySpecification.SqlCompactProviderName));

            Assert.True(factoryAdded);
            Assert.Equal(
                ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                GetFactoryName(config));
            Assert.Equal(
                ConnectionFactorySpecification.SqlCompactProviderName,
                GetArgument(config));
        }

        [Fact]
        public void AddOrUpdateConnectionFactoryInConfig_sets_factory_to_SQL_Compact_even_if_entry_already_exists()
        {
            var config = CreateConnectionFactoryConfigDoc("SomeConnectionFactory");

            var factoryAdded = new ConfigFileManipulator()
                .AddOrUpdateConnectionFactoryInConfig(
                    config,
                    new ConnectionFactorySpecification(
                        ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                        ConnectionFactorySpecification.SqlCompactProviderName));

            Assert.True(factoryAdded);
            Assert.Equal(
                ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                GetFactoryName(config));
            Assert.Equal(
                ConnectionFactorySpecification.SqlCompactProviderName,
                GetArgument(config));
        }

        [Fact]
        public void AddOrUpdateConnectionFactoryInConfig_sets_factory_to_SQL_Compact_even_if_entry_with_param_already_exists()
        {
            var config =
                CreateConnectionFactoryConfigDoc(
                    ConnectionFactorySpecification.SqlConnectionFactoryName,
                    new[] { "Database=Bob" });

            var factoryAdded = new ConfigFileManipulator()
                .AddOrUpdateConnectionFactoryInConfig(
                    config,
                    new ConnectionFactorySpecification(
                        ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                        ConnectionFactorySpecification.SqlCompactProviderName));

            Assert.True(factoryAdded);
            Assert.Equal(
                ConnectionFactorySpecification.SqlCeConnectionFactoryName,
                GetFactoryName(config));
            Assert.Equal(
                ConnectionFactorySpecification.SqlCompactProviderName,
                GetArgument(config));
        }

        [Fact]
        public void AddOrUpdateConnectionFactoryInConfig_sets_updates_argument_even_if_everything_else_already_exists()
        {
            var config =
                CreateConnectionFactoryConfigDoc(
                    ConnectionFactorySpecification.SqlConnectionFactoryName,
                    new[] { "Database=Bob" });

            var factoryAdded = new ConfigFileManipulator()
                .AddOrUpdateConnectionFactoryInConfig(
                    config,
                    new ConnectionFactorySpecification(
                        ConnectionFactorySpecification.SqlConnectionFactoryName,
                        "Database=Bobby"));

            Assert.True(factoryAdded);
            Assert.Equal(ConnectionFactorySpecification.SqlConnectionFactoryName, GetFactoryName(config));
            Assert.Equal("Database=Bobby", GetArgument(config));
        }

        [Fact]
        public void AddOrUpdateConnectionFactoryInConfig_sets_updates_some_arguments_even_if_everything_else_already_exists()
        {
            var config =
                CreateConnectionFactoryConfigDoc(
                    ConnectionFactorySpecification.SqlConnectionFactoryName,
                    new[] { "Database=Bob", "Dave=Matthews", "Too=Much" });

            var factoryAdded = new ConfigFileManipulator()
                .AddOrUpdateConnectionFactoryInConfig(
                    config,
                    new ConnectionFactorySpecification(
                        ConnectionFactorySpecification.SqlConnectionFactoryName,
                        "Database=Bobby", "Dave=Matthews", "Tripping=Billies"));

            Assert.True(factoryAdded);
            Assert.Equal(ConnectionFactorySpecification.SqlConnectionFactoryName, GetFactoryName(config));
            Assert.Equal("Database=Bobby", GetArgument(config, 0));
            Assert.Equal("Dave=Matthews", GetArgument(config, 1));
            Assert.Equal("Tripping=Billies", GetArgument(config, 2));
        }

        [Fact]
        public void AddProviderToConfig_does_nothing_if_only_given_provider_already_exists()
        {
            var config = CreateProviderConfigDoc(Tuple.Create("Dave.Matthews.Band", "Crash"));

            var factoryAdded = new ConfigFileManipulator()
                .AddProviderToConfig(config, "Dave.Matthews.Band", "Crash");

            Assert.False(factoryAdded);
            Assert.Equal("Dave.Matthews.Band", GetProviders(config).Single().Item1);
            Assert.Equal("Crash", GetProviders(config).Single().Item2);
        }

        [Fact]
        public void AddProviderToConfig_does_nothing_if_given_provider_exists_and_is_at_end()
        {
            var config = CreateProviderConfigDoc(
                Tuple.Create("The.Clash", "London.Calling"),
                Tuple.Create("Dave.Matthews.Band", "Crash"));

            var factoryAdded = new ConfigFileManipulator()
                .AddProviderToConfig(config, "Dave.Matthews.Band", "Crash");

            Assert.False(factoryAdded);
            Assert.Equal("The.Clash", GetProviders(config).First().Item1);
            Assert.Equal("London.Calling", GetProviders(config).First().Item2);
            Assert.Equal("Dave.Matthews.Band", GetProviders(config).Skip(1).Single().Item1);
            Assert.Equal("Crash", GetProviders(config).Skip(1).Single().Item2);
        }

        [Fact]
        public void AddProviderToConfig_moves_entry_to_end_if_exists()
        {
            var config = CreateProviderConfigDoc(
                Tuple.Create("Dave.Matthews.Band", "Crash"),
                Tuple.Create("The.Clash", "London.Calling"));

            var factoryAdded = new ConfigFileManipulator()
                .AddProviderToConfig(config, "Dave.Matthews.Band", "Crash");

            Assert.True(factoryAdded);
            Assert.Equal("The.Clash", GetProviders(config).First().Item1);
            Assert.Equal("London.Calling", GetProviders(config).First().Item2);
            Assert.Equal("Dave.Matthews.Band", GetProviders(config).Skip(1).Single().Item1);
            Assert.Equal("Crash", GetProviders(config).Skip(1).Single().Item2);
        }

        [Fact]
        public void AddProviderToConfig_adds_provider_if_no_config_exists()
        {
            var config = new XDocument();

            var factoryAdded = new ConfigFileManipulator()
                .AddProviderToConfig(config, "Dave.Matthews.Band", "Crash");

            Assert.True(factoryAdded);
            Assert.Equal("Dave.Matthews.Band", GetProviders(config).Single().Item1);
            Assert.Equal("Crash", GetProviders(config).Single().Item2);
        }

        [Fact]
        public void AddProviderToConfig_adds_provider_if_config_exists_but_with_no_provider()
        {
            var config = CreateProviderConfigDoc();

            var factoryAdded = new ConfigFileManipulator()
                .AddProviderToConfig(config, "Dave.Matthews.Band", "Crash");

            Assert.True(factoryAdded);
            Assert.Equal("Dave.Matthews.Band", GetProviders(config).Single().Item1);
            Assert.Equal("Crash", GetProviders(config).Single().Item2);
        }

        [Fact]
        public void AddProviderToConfig_updates_provider_type_if_invariant_name_already_exists()
        {
            var config = CreateProviderConfigDoc(
                Tuple.Create("The.Clash", "London.Calling"),
                Tuple.Create("Dave.Matthews.Band", "Crash"));

            var factoryAdded = new ConfigFileManipulator()
                .AddProviderToConfig(config, "Dave.Matthews.Band", "Broken.Things");

            Assert.True(factoryAdded);
            Assert.Equal("The.Clash", GetProviders(config).First().Item1);
            Assert.Equal("London.Calling", GetProviders(config).First().Item2);
            Assert.Equal("Dave.Matthews.Band", GetProviders(config).Skip(1).Single().Item1);
            Assert.Equal("Broken.Things", GetProviders(config).Skip(1).Single().Item2);
        }

        [Fact]
        public void AddProviderToConfig_moves_entry_to_end_and_updates_provider_type_if_invariant_name_already_exists()
        {
            var config = CreateProviderConfigDoc(
                Tuple.Create("Dave.Matthews.Band", "Crash"),
                Tuple.Create("The.Clash", "London.Calling"));

            var factoryAdded = new ConfigFileManipulator()
                .AddProviderToConfig(config, "Dave.Matthews.Band", "Broken.Things");

            Assert.True(factoryAdded);
            Assert.Equal("The.Clash", GetProviders(config).First().Item1);
            Assert.Equal("London.Calling", GetProviders(config).First().Item2);
            Assert.Equal("Dave.Matthews.Band", GetProviders(config).Skip(1).Single().Item1);
            Assert.Equal("Broken.Things", GetProviders(config).Skip(1).Single().Item2);
        }

        [Fact]
        public void AddConnectionFactoryToConfig_does_nothing_if_factory_name_already_exists()
        {
            var config = CreateConnectionFactoryConfigDoc("SomeConnectionFactory");

            var factoryAdded = new ConfigFileManipulator().
                AddConnectionFactoryToConfig(
                    config,
                    new ConnectionFactorySpecification(
                        "NewConnectionFactory", "NewBaseConnectionString"));

            Assert.False(factoryAdded);
            Assert.Equal("SomeConnectionFactory", GetFactoryName(config));
        }

        [Fact]
        public void AddConnectionFactoryToConfig_adds_factory_if_no_factory_name_already_exists()
        {
            var config = CreateConnectionFactoryConfigDoc(null);

            var factoryAdded = new ConfigFileManipulator().
                AddConnectionFactoryToConfig(
                    config,
                    new ConnectionFactorySpecification(
                        "NewConnectionFactory", "NewBaseConnectionString"));

            Assert.True(factoryAdded);
            Assert.Equal("NewConnectionFactory", GetFactoryName(config));
            Assert.Equal("NewBaseConnectionString", GetArgument(config));
        }

        [Fact]
        public void AddConnectionFactoryToConfig_adds_factory_if_entityFramework_element_is_missing()
        {
            var config =
                new XDocument(
                    new XElement(ConfigFileManipulator.ConfigurationElementName));

            var factoryAdded = new ConfigFileManipulator().
                AddConnectionFactoryToConfig(
                    config,
                    new ConnectionFactorySpecification(
                        "NewConnectionFactory", "NewBaseConnectionString"));

            Assert.True(factoryAdded);
            Assert.Equal("NewConnectionFactory", GetFactoryName(config));
            Assert.Equal("NewBaseConnectionString", GetArgument(config));
        }

        [Fact]
        public void AddConnectionFactoryToConfig_adds_factory_if_configuration_element_is_missing()
        {
            var config = new XDocument();

            var factoryAdded = new ConfigFileManipulator().
                AddConnectionFactoryToConfig(
                    config,
                    new ConnectionFactorySpecification(
                        "NewConnectionFactory", "NewBaseConnectionString"));

            Assert.True(factoryAdded);
            Assert.Equal("NewConnectionFactory", GetFactoryName(config));
            Assert.Equal("NewBaseConnectionString", GetArgument(config));
        }

        [Fact]
        public void AddConnectionFactoryToConfig_adds_factory_with_no_parameters()
        {
            var config = new XDocument();

            var factoryAdded = new ConfigFileManipulator().
                AddConnectionFactoryToConfig(
                    config,
                    new ConnectionFactorySpecification("NewConnectionFactory"));

            Assert.True(factoryAdded);
            Assert.Equal("NewConnectionFactory", GetFactoryName(config));

            Assert.Null(
                config.Element(ConfigFileManipulator.ConfigurationElementName)
                      .Element(ConfigFileManipulator.EntityFrameworkElementName)
                      .Element(
                          ConfigFileManipulator.DefaultConnectionFactoryElementName)
                      .Element(ConfigFileManipulator.ParametersElementName));
        }

        [Fact]
        public void AddConnectionFactoryToConfig_adds_factory_with_many_parameters()
        {
            var config = new XDocument();

            var factoryAdded = new ConfigFileManipulator().
                AddConnectionFactoryToConfig(
                    config,
                    new ConnectionFactorySpecification(
                        "NewConnectionFactory", "1", "2", "3"));

            Assert.True(factoryAdded);
            Assert.Equal("NewConnectionFactory", GetFactoryName(config));
            Assert.Equal("1", GetArguments(config).First());
            Assert.Equal("2", GetArguments(config).Skip(1).First());
            Assert.Equal("3", GetArguments(config).Skip(2).First());
        }

        private XDocument CreateConnectionFactoryConfigDoc(string factoryName)
        {
            return new XDocument(
                new XElement(
                    ConfigFileManipulator.ConfigurationElementName,
                    new XElement(
                        ConfigFileManipulator.EntityFrameworkElementName,
                        factoryName != null
                            ? new XElement(
                                  ConfigFileManipulator.
                                  DefaultConnectionFactoryElementName, new XAttribute("type", factoryName))
                            : null)));
        }

        private XDocument CreateConnectionFactoryConfigDoc(string factoryName, string[] args)
        {
            return new XDocument(
                new XElement(
                    ConfigFileManipulator.ConfigurationElementName,
                    new XElement(
                        ConfigFileManipulator.EntityFrameworkElementName,
                        new XElement(
                            ConfigFileManipulator.DefaultConnectionFactoryElementName,
                            new XAttribute("type", factoryName),
                            new XElement(
                                ConfigFileManipulator.ParametersElementName,
                                args.Select(
                                    a => new XElement(
                                             ConfigFileManipulator.ParameterElementName,
                                             new XAttribute("value", a))))))));
        }

        private XDocument CreateProviderConfigDoc(params Tuple<string, string>[] providers)
        {
            return new XDocument(
                new XElement(
                    ConfigFileManipulator.ConfigurationElementName,
                    new XElement(
                        ConfigFileManipulator.EntityFrameworkElementName,
                        new XElement(
                            ConfigFileManipulator.ProvidersElementName,
                            providers.Select(
                                p => new XElement(
                                         ConfigFileManipulator.ProviderElementName,
                                         new XAttribute("invariantName", p.Item1),
                                         new XAttribute("type", p.Item2)))))));
        }

        private IEnumerable<Tuple<string, string>> GetProviders(XDocument config)
        {
            return config.Element(ConfigFileManipulator.ConfigurationElementName)
                         .Element(ConfigFileManipulator.EntityFrameworkElementName)
                         .Element(ConfigFileManipulator.ProvidersElementName)
                         .Elements(ConfigFileManipulator.ProviderElementName)
                         .Select(p => Tuple.Create(p.Attribute("invariantName").Value, p.Attribute("type").Value));
        }

        private string GetFactoryName(XDocument config)
        {
            return config.Element(ConfigFileManipulator.ConfigurationElementName)
                         .Element(ConfigFileManipulator.EntityFrameworkElementName)
                         .Element(ConfigFileManipulator.DefaultConnectionFactoryElementName)
                         .Attribute("type")
                         .Value;
        }

        private string GetArgument(XDocument config, int index = 0)
        {
            return GetArguments(config).Skip(index).First();
        }

        private IEnumerable<string> GetArguments(XDocument config)
        {
            return config.Element(ConfigFileManipulator.ConfigurationElementName)
                         .Element(ConfigFileManipulator.EntityFrameworkElementName)
                         .Element(ConfigFileManipulator.DefaultConnectionFactoryElementName)
                         .Element(ConfigFileManipulator.ParametersElementName)
                         .Elements(ConfigFileManipulator.ParameterElementName)
                         .Select(e => e.Attribute("value").Value);
        }

        [Fact]
        public void AddOrUpdateConfigSection_does_nothing_if_EF_assembly_name_is_up_to_date()
        {
            var config = CreateConfigSectionDoc(_net45EntityFrameworkSectionName, addRequirePermission: true);

            var sectionModified =
                new ConfigFileManipulator().AddOrUpdateConfigSection(
                    config, _net45EntityFrameworkVersion);

            Assert.False(sectionModified);
            Assert.Equal(_net45EntityFrameworkSectionName, GetEfSectionName(config));
        }

        [Fact]
        public void AddOrUpdateConfigSection_adds_EF_section_if_configuration_element_is_missing()
        {
            var config = new XDocument();

            var sectionModified =
                new ConfigFileManipulator().AddOrUpdateConfigSection(
                    config, _net45EntityFrameworkVersion);

            Assert.True(sectionModified);
            Assert.Equal(_net45EntityFrameworkSectionName, GetEfSectionName(config));
        }

        [Fact]
        public void AddOrUpdateConfigSection_adds_EF_section_if_configSections_element_is_missing()
        {
            var config =
                new XDocument(
                    new XElement(ConfigFileManipulator.ConfigurationElementName));

            var sectionModified =
                new ConfigFileManipulator().AddOrUpdateConfigSection(
                    config, _net45EntityFrameworkVersion);

            Assert.True(sectionModified);
            Assert.Equal(_net45EntityFrameworkSectionName, GetEfSectionName(config));
        }

        [Fact]
        public void AddOrUpdateConfigSection_adds_EF_section_if_configSections_element_contains_no_entries()
        {
            var config =
                new XDocument(
                    new XElement(
                        ConfigFileManipulator.ConfigurationElementName,
                        new XElement(ConfigFileManipulator.ConfigSectionsElementName)));

            var sectionModified =
                new ConfigFileManipulator().AddOrUpdateConfigSection(
                    config, _net45EntityFrameworkVersion);

            Assert.True(sectionModified);
            Assert.Equal(_net45EntityFrameworkSectionName, GetEfSectionName(config));
        }

        [Fact]
        public void AddOrUpdateConfigSection_adds_EF_section_if_configSections_element_has_no_entityFramework_entry()
        {
            var config = CreateConfigSectionDoc(assemblyName: null);

            var sectionModified =
                new ConfigFileManipulator().AddOrUpdateConfigSection(
                    config, _net45EntityFrameworkVersion);

            Assert.True(sectionModified);
            Assert.Equal(_net45EntityFrameworkSectionName, GetEfSectionName(config));
        }

        [Fact]
        public void AddOrUpdateConfigSection_updates_EF_section_if_configSections_element_is_out_of_date()
        {
            var config = CreateConfigSectionDoc(_net40EntityFrameworkSectionName);

            var sectionModified =
                new ConfigFileManipulator().AddOrUpdateConfigSection(
                    config, _net45EntityFrameworkVersion);

            Assert.True(sectionModified);
            Assert.Equal(_net45EntityFrameworkSectionName, GetEfSectionName(config));
        }

        [Fact]
        public void AddOrUpdateConfigSection_when_using_NET4_EF_assembly_does_nothing_if_EF_assembly_name_is_up_to_date()
        {
            var config = CreateConfigSectionDoc(_net40EntityFrameworkSectionName, addRequirePermission: true);

            var sectionModified =
                new ConfigFileManipulator().AddOrUpdateConfigSection(
                    config, _net40EntityFrameworkVersion);

            Assert.False(sectionModified);
            Assert.Equal(_net40EntityFrameworkSectionName, GetEfSectionName(config));
        }

        [Fact]
        public void AddOrUpdateConfigSection_when_using_NET4_EF_assembly_updates_EF_section_if_configSections_element_is_too_new()
        {
            var config = CreateConfigSectionDoc(_net45EntityFrameworkSectionName);

            var sectionModified =
                new ConfigFileManipulator().AddOrUpdateConfigSection(
                    config, _net40EntityFrameworkVersion);

            Assert.True(sectionModified);
            Assert.Equal(_net40EntityFrameworkSectionName, GetEfSectionName(config));
        }

        private XDocument CreateConfigSectionDoc(string assemblyName, bool addRequirePermission = false)
        {
            var dummyElement = new XElement(
                ConfigFileManipulator.SectionElementName,
                new XAttribute("name", "SamVimes"),
                new XAttribute("type", "Treacle Mine Road"));
            XElement sectionElement;

            if (assemblyName == null)
            {
                sectionElement = dummyElement;
            }
            else
            {
                sectionElement = new XElement(
                    ConfigFileManipulator.SectionElementName,
                    new XAttribute(
                        "name", ConfigFileManipulator.EntityFrameworkElementName),
                    new XAttribute("type", assemblyName));

                if (addRequirePermission)
                {
                    sectionElement.Add(new XAttribute("requirePermission", false));
                }
            }

            return new XDocument(
                new XElement(
                    ConfigFileManipulator.ConfigurationElementName,
                    new XElement(
                        ConfigFileManipulator.ConfigSectionsElementName,
                        dummyElement,
                        sectionElement)));
        }

        private string GetEfSectionName(XDocument config)
        {
            return config.Element(ConfigFileManipulator.ConfigurationElementName)
                         .Element(ConfigFileManipulator.ConfigSectionsElementName)
                         .Elements(ConfigFileManipulator.SectionElementName)
                         .Where(
                             e =>
                             e.Attributes("name").Any(
                                 a =>
                                 a.Value == ConfigFileManipulator.EntityFrameworkElementName))
                         .Select(e => e.Attribute("type").Value)
                         .Single();
        }

        #endregion

        #region Config file processing tests

        [Fact]
        public void ConfigFileFinder_finds_items_named_app_config_and_web_config_in_Visual_Studio_ProjectItems()
        {
            var items = new List<ProjectItem>();

            new ConfigFileFinder()
                .FindConfigFiles(
                    CreateMockedItems("app.config", "App.config", "Foo.config", "web.config", "Web.config", "Bar"),
                    i => items.Add(i));

            Assert.Equal(4, items.Count);
            Assert.True(items.Any(i => i.Name == "app.config"));
            Assert.True(items.Any(i => i.Name == "App.config"));
            Assert.True(items.Any(i => i.Name == "web.config"));
            Assert.True(items.Any(i => i.Name == "Web.config"));
        }

        private ProjectItems CreateMockedItems(params string[] itemNames)
        {
            var items = new List<ProjectItem>();
            foreach (var name in itemNames)
            {
                var mockedItem = new Mock<ProjectItem>();
                mockedItem.Setup(p => p.Name).Returns(name);
                items.Add(mockedItem.Object);
            }

            var mockedProjectItems = new Mock<ProjectItems>();
            mockedProjectItems.Setup(p => p.GetEnumerator()).Returns(items.GetEnumerator());

            return mockedProjectItems.Object;
        }

        [Fact]
        public void ConfigFileProcessor_saves_config_file_if_config_document_is_modified()
        {
            ConfigFileSaveTest(shouldSave: true);
        }

        [Fact]
        public void ConfigFileProcessor_does_not_save_config_file_if_config_document_is_not_modified()
        {
            ConfigFileSaveTest(shouldSave: false);
        }

        [Fact]
        public void If_config_file_cannot_be_saved_when_it_needs_to_be_then_an_exception_is_thrown()
        {
            ConfigFileSaveTest(shouldSave: true, writeProtectFile: true);
        }

        private void RunTestWithTempFilename(Action<string> test)
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                test(tempFileName);
            }
            finally
            {
                try
                {
                    File.SetAttributes(tempFileName, File.GetAttributes(tempFileName) & ~FileAttributes.ReadOnly);
                    File.Delete(tempFileName);
                }
                catch (FileNotFoundException)
                {
                }
            }
        }

        private void ConfigFileSaveTest(bool shouldSave, bool writeProtectFile = false)
        {
            RunTestWithTempFilename(
                tempFileName =>
                    {
                        new XDocument(new XElement("fake")).Save(tempFileName);

                        if (writeProtectFile)
                        {
                            File.SetAttributes(tempFileName, File.GetAttributes(tempFileName) | FileAttributes.ReadOnly);
                        }

                        var mockedItem = new Mock<ProjectItem>();
                        mockedItem.Setup(p => p.get_FileNames(0)).Returns(tempFileName);

                        var mockedManipulator = new Mock<ConfigFileManipulator>();
                        mockedManipulator
                            .Setup(
                                m =>
                                m.AddConnectionFactoryToConfig(
                                    It.IsAny<XDocument>(),
                                    It.IsAny<ConnectionFactorySpecification>()))
                            .Callback(
                                (XDocument config, ConnectionFactorySpecification _)
                                => config.Element("fake").Add(new XElement("modified")))
                            .Returns(shouldSave);

                        Action test =
                            () => new ConfigFileProcessor()
                                      .ProcessConfigFile(
                                          mockedItem.Object,
                                          new Func<XDocument, bool>[]
                                              {
                                                  c => mockedManipulator.Object.AddOrUpdateConfigSection(c, _net45EntityFrameworkVersion),
                                                  c =>
                                                  mockedManipulator.Object.AddConnectionFactoryToConfig(
                                                      c,
                                                      new ConnectionFactorySpecification("F"))
                                              });

                        if (shouldSave && writeProtectFile)
                        {
                            Assert.Equal(
                                Strings.SaveConnectionFactoryInConfigFailed(
                                    tempFileName), Assert.Throws<IOException>(test).Message);
                        }
                        else
                        {
                            test();
                        }

                        var doc = XDocument.Load(tempFileName);
                        Assert.Equal(shouldSave && !writeProtectFile, doc.Element("fake").Elements("modified").Any());
                    });
        }

        #endregion

        #region SQL Server detection tests

        [Fact]
        public void SqlServerDetector_detects_SQL_Express_when_service_is_running()
        {
            Assert.True(
                new SqlServerDetector(
                    new Mock<RegistryKeyProxy>().Object,
                    CreateMockedController()).IsSqlExpressInstalled());
        }

        [Fact]
        public void SqlServerDetector_detects_no_SQL_Express_when_service_is_present_but_not_running()
        {
            Assert.False(
                new SqlServerDetector(
                    new Mock<RegistryKeyProxy>().Object,
                    CreateMockedController(ServiceControllerStatus.Stopped)).IsSqlExpressInstalled());
        }

        [Fact]
        public void SqlServerDetector_detects_no_SQL_Express_when_service_is_not_found()
        {
            Assert.False(
                new SqlServerDetector(
                    new Mock<RegistryKeyProxy>().Object,
                    CreateMockedController(status: null)).IsSqlExpressInstalled());
        }

        private ServiceControllerProxy CreateMockedController(
            ServiceControllerStatus? status = ServiceControllerStatus.Running)
        {
            var mockController = new Mock<ServiceControllerProxy>();

            if (status != null)
            {
                mockController.Setup(m => m.Status).Returns(status.Value);
            }
            else
            {
                mockController.Setup(m => m.Status).Throws(new InvalidOperationException());
            }

            return mockController.Object;
        }

        [Fact]
        public void SqlServerDetector_detects_the_version_of_LocalDB_in_the_registry_when_only_one_version_is_installed()
        {
            Assert.Equal(
                "mssqllocaldb",
                new SqlServerDetector(
                    CreatedMockedRegistryKey("v13.0"),
                    new Mock<ServiceControllerProxy>().Object)
                    .GetLocalDBVersionInstalled());
        }

        [Fact]
        public void SqlServerDetector_detects_the_highest_orderable_version_of_LocalDB_in_the_registry_when_multiple_versions_are_installed()
        {
            Assert.Equal(
                "mssqllocaldb",
                new SqlServerDetector(
                    CreatedMockedRegistryKey("11.0", "12.0", "11.5"),
                    new Mock<ServiceControllerProxy>().Object)
                    .GetLocalDBVersionInstalled());
        }

        [Fact]
        public void SqlServerDetector_orders_LocalDB_versions_numerically_when_multiple_versions_are_installed()
        {
            Assert.Equal(
                "mssqllocaldb",
                new SqlServerDetector(
                    CreatedMockedRegistryKey("20", "100"),
                    new Mock<ServiceControllerProxy>().Object)
                    .GetLocalDBVersionInstalled());
        }

        [Fact]
        public void SqlServerDetector_ignores_LocalDB_versions_that_are_not_numeric_when_multiple_versions_are_installed()
        {
            Assert.Equal(
                "mssqllocaldb",
                 new SqlServerDetector(
                    CreatedMockedRegistryKey("11.0", "12.0", "dingo", "11.5"),
                    new Mock<ServiceControllerProxy>().Object)
                    .GetLocalDBVersionInstalled());
        }

        [Fact]
        public void SqlServerDetector_returns_mssqllocaldb_if_multiple_non_numeric_LocalDB_versions_are_installed()
        {
            Assert.Equal(
                "mssqllocaldb",
                new SqlServerDetector(
                    CreatedMockedRegistryKey("kangaroo", "dingo", "wallaby"),
                    new Mock<ServiceControllerProxy>().Object)
                    .GetLocalDBVersionInstalled());
        }

        [Fact]
        public void SqlServerDetector_returns_mssqllocaldb_if_LocalDB_registry_exists_but_no_version_keys_are_found()
        {
            Assert.Equal(
                "mssqllocaldb",
                new SqlServerDetector(
                    CreatedMockedRegistryKey(new string[0]),
                    new Mock<ServiceControllerProxy>().Object)
                    .GetLocalDBVersionInstalled());
        }

        private RegistryKeyProxy CreatedMockedRegistryKey(params string[] versions)
        {
            var mockedRegistryKey = new Mock<RegistryKeyProxy>();

            mockedRegistryKey.Setup(k => k.OpenSubKey("SOFTWARE")).Returns(mockedRegistryKey.Object);
            mockedRegistryKey.Setup(k => k.OpenSubKey("Microsoft")).Returns(mockedRegistryKey.Object);
            mockedRegistryKey.Setup(k => k.OpenSubKey("Microsoft SQL Server Local DB")).Returns(mockedRegistryKey.Object);
            mockedRegistryKey.Setup(k => k.OpenSubKey("Installed Versions")).Returns(mockedRegistryKey.Object);

            mockedRegistryKey.Setup(k => k.SubKeyCount).Returns(versions.Length);
            mockedRegistryKey.Setup(k => k.GetSubKeyNames()).Returns(versions);

            mockedRegistryKey.Setup(k => k.OpenSubKey("Wow6432Node")).Returns(
                new RegistryKeyProxy(null));

            return mockedRegistryKey.Object;
        }

        [Fact]
        public void SqlServerDetector_returns_mssqllocaldb_if_LocalDB_registry_keys_do_not_exist()
        {
            var mockedRegistryKey = new Mock<RegistryKeyProxy>();

            mockedRegistryKey.Setup(k => k.OpenSubKey("SOFTWARE")).Returns(mockedRegistryKey.Object);
            mockedRegistryKey.Setup(k => k.OpenSubKey("Microsoft")).Returns(mockedRegistryKey.Object);
            mockedRegistryKey.Setup(k => k.OpenSubKey("Microsoft SQL Server Local DB")).Returns(
                new RegistryKeyProxy(null));
            mockedRegistryKey.Setup(k => k.OpenSubKey("Wow6432Node")).Returns(
                new RegistryKeyProxy(null));

            Assert.Equal(
                "mssqllocaldb", 
                new SqlServerDetector(
                    mockedRegistryKey.Object,
                    new Mock<ServiceControllerProxy>().Object)
                    .GetLocalDBVersionInstalled());
        }

        [Fact]
        public void SqlServerDetector_detects_LocalDB_installation_in_Wow6432Node_if_not_found_in_normal_hive()
        {
            var mockedRegistryKey = new Mock<RegistryKeyProxy>();

            mockedRegistryKey.Setup(k => k.OpenSubKey("SOFTWARE")).Returns(mockedRegistryKey.Object);
            mockedRegistryKey.Setup(k => k.OpenSubKey("Microsoft")).Returns(mockedRegistryKey.Object);
            mockedRegistryKey.Setup(k => k.OpenSubKey("Microsoft SQL Server Local DB")).Returns(
                new RegistryKeyProxy(null));

            var mockedWow64RegistryKey = new Mock<RegistryKeyProxy>();
            mockedRegistryKey.Setup(k => k.OpenSubKey("Wow6432Node")).Returns(mockedWow64RegistryKey.Object);

            mockedWow64RegistryKey.Setup(k => k.OpenSubKey("Microsoft")).Returns(mockedWow64RegistryKey.Object);
            mockedWow64RegistryKey.Setup(k => k.OpenSubKey("Microsoft SQL Server Local DB")).Returns(mockedWow64RegistryKey.Object);
            mockedWow64RegistryKey.Setup(k => k.OpenSubKey("Installed Versions")).Returns(mockedWow64RegistryKey.Object);

            mockedWow64RegistryKey.Setup(k => k.SubKeyCount).Returns(1);
            mockedWow64RegistryKey.Setup(k => k.GetSubKeyNames()).Returns(new[] { "11.5" });

            Assert.Equal(
                "v11.5",
                new SqlServerDetector(
                    mockedRegistryKey.Object,
                    new Mock<ServiceControllerProxy>().Object)
                    .GetLocalDBVersionInstalled());
        }

        [Fact]
        public void SqlServerDetector_disposes_RegisterKey_and_ManagementObjectSearcher_when_it_is_disposed()
        {
            var mockedRegistryKey = new Mock<RegistryKeyProxy>();
            var mockedSearcher = new Mock<ServiceControllerProxy>();

            new SqlServerDetector(mockedRegistryKey.Object, mockedSearcher.Object).
                Dispose();

            mockedRegistryKey.Verify(k => k.Dispose());
            mockedSearcher.Verify(s => s.Dispose());
        }

        [Fact]
        public void SqlServerDetector_generates_SQL_Express_base_connection_string_if_both_Express_and_LocalDB_are_installed()
        {
            var specification =
                new SqlServerDetector(
                    CreatedMockedRegistryKey("11.0"), CreateMockedController())
                    .BuildConnectionFactorySpecification();

            Assert.Equal(
                ConnectionFactorySpecification.SqlConnectionFactoryName,
                specification.ConnectionFactoryName);
            Assert.Empty(specification.ConstructorArguments);
        }

        [Fact]
        public void SqlServerDetector_generates_SQL_Express_base_connection_string_if_Express_is_installed_and_LocalDB_is_not()
        {
            var specification =
                new SqlServerDetector(
                    CreatedMockedRegistryKey(new string[0]), CreateMockedController())
                    .BuildConnectionFactorySpecification();

            Assert.Equal(
                ConnectionFactorySpecification.SqlConnectionFactoryName,
                specification.ConnectionFactoryName);
            Assert.Empty(specification.ConstructorArguments);
        }

        [Fact]
        public void SqlServerDetector_generates_LocalDB_base_connection_string_if_LocalDB_is_installed_and_Express_is_not()
        {
            var specification =
                new SqlServerDetector(
                    CreatedMockedRegistryKey("12.0"), CreateMockedController(status: null))
                    .BuildConnectionFactorySpecification();

            Assert.Equal(
                ConnectionFactorySpecification.LocalDbConnectionFactoryName,
                specification.ConnectionFactoryName);
            Assert.Equal("mssqllocaldb", specification.ConstructorArguments.Single());
        }

        [Fact]
        public void SqlServerDetector_generates_LocalDB_12_base_connection_string_if_neither_LocalDB_or_Express_are_installed()
        {
            var specification =
                new SqlServerDetector(
                    CreatedMockedRegistryKey(new string[0]), CreateMockedController(status: null))
                    .BuildConnectionFactorySpecification();

            Assert.Equal(
                ConnectionFactorySpecification.LocalDbConnectionFactoryName,
                specification.ConnectionFactoryName);
            Assert.Equal("mssqllocaldb", specification.ConstructorArguments.Single());
        }

        [Fact(Skip = "Fails without SQL Server Express")]
        public void SqlServerDetector_detects_SQL_Express_on_dev_machine()
        {
            using (
                var detector = new SqlServerDetector(
                    new Mock<RegistryKeyProxy>().Object,
                    new ServiceControllerProxy(
                        new ServiceController("MSSQL$SQLEXPRESS"))))
            {
                Assert.True(detector.IsSqlExpressInstalled());
            }
        }

#if !NET40

        [Fact]
        public void SqlServerDetector_detects_LocalDB_on_dev_machine()
        {
            using (
                var detector = new SqlServerDetector(
                    Registry.LocalMachine, new Mock<ServiceControllerProxy>().Object)
                )
            {
                Assert.NotNull(detector.GetLocalDBVersionInstalled());
            }
        }

#endif

        [Fact(Skip = "Fails without SQL Server Express")]
        public void Base_connection_string_on_dev_box_with_SQL_Express_installed_has_SQL_Express_connection_string()
        {
            using (
                var detector = new SqlServerDetector(
                    Registry.LocalMachine,
                    new ServiceControllerProxy(
                        new ServiceController("MSSQL$SQLEXPRESS"))))
            {
                var specification = detector.BuildConnectionFactorySpecification();

                Assert.Equal(
                    ConnectionFactorySpecification.SqlConnectionFactoryName,
                    specification.ConnectionFactoryName);
                Assert.Empty(specification.ConstructorArguments);
            }
        }

        #endregion

        #region Tests using real Visual Studio objects

        [Fact]
        public void Assembly_reference_can_be_found_for_real_assembly()
        {
            Run_Project_test_if_Visual_Studio_is_running(
                (p, v) =>
                    {
                        var dataRef = new ReferenceRemover(p).TryFindReference("System.Data", "b77a5c561934e089");
                        Assert.Equal("System.Data", dataRef.Identity);
                        Assert.Equal("b77a5c561934e089", dataRef.PublicKeyToken.ToLowerInvariant());
                        Assert.True(dataRef.StrongName);
                    });
        }

        [Fact]
        public void Default_connection_factory_is_added_to_real_Visual_Studio_project_and_config_file()
        {
            var configFilesFound = new List<string>();

            Run_Project_test_if_Visual_Studio_is_running(
                (p, v) =>
                    {
                        new ConfigFileFinder().FindConfigFiles(
                            p.ProjectItems, i =>
                                {
                                    configFilesFound.Add(i.Name);

                                    var config = XDocument.Load(i.FileNames[0]);

                                    // Checked in app.config for unit tests has no connection factory, so one should be added
                                    var modified = new ConfigFileManipulator().AddConnectionFactoryToConfig(
                                        config,
                                        new ConnectionFactorySpecification(
                                            ConnectionFactorySpecification.SqlConnectionFactoryName,
                                            "SomeConnectionString"));

                                    Assert.True(modified);

                                    Assert.Equal(
                                        ConnectionFactorySpecification.SqlConnectionFactoryName, GetFactoryName(config));
                                    Assert.Equal("SomeConnectionString", GetArgument(config));
                                });

                        Assert.Equal(1, configFilesFound.Count);
                        Assert.Equal("App.config", configFilesFound.Single());
                    });
        }

        //[Fact] Disabling--this test is too flakey for the value it brings
        public void GetProjectTypes_returns_project_types()
        {
            Run_Project_test_if_Visual_Studio_is_running(
                (p, v) =>
                    {
                        var types = p.GetProjectTypes(v);
                        Assert.Equal(new[] { "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" }, types);
                    });
        }

        private void Run_Project_test_if_Visual_Studio_is_running(Action<Project, int> test)
        {
            // Note that these tests are setup to work with the latest released version of Visual
            // Studio used by the EF team in day-to-day development. Currently this is VS 2013 which
            // is version 12. See CodePlex 1367.
            const int targetVsVersion = 12;

            MessageFilter.Register();

            try
            {
                Project project = null;
                try
                {
                    var dte = (DTE)Marshal.GetActiveObject(
                        string.Format(CultureInfo.InvariantCulture, "VisualStudio.DTE.{0}.0", targetVsVersion));
                    project = TryGetPowerShellUnitTests(dte);
                }
                catch (COMException)
                {
                    // This is thrown when running as part of a razzle build. The test doesn't work in
                    // the razzle environment.
                }

                if (project != null)
                {
                    test(project, targetVsVersion);
                }
            }
            finally
            {
                MessageFilter.Revoke();
            }
        }

        private static Project TryGetPowerShellUnitTests(DTE dte)
        {
            return dte
                .Solution
                .Projects
                .OfType<Project>()
                .Where(p => p.Name == "Tests")
                .SelectMany(p => p.ProjectItems.OfType<ProjectItem>())
                .Where(p => p.Name == "UnitTests")
                .Select(p => p.SubProject)
                .FirstOrDefault();
        }

        #endregion
    }

    #region Fake connection factories

    public abstract class FakeBaseConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            throw new NotImplementedException();
        }
    }

    public class FakeConnectionFactoryNoParams : FakeBaseConnectionFactory
    {
    }

    public class FakeConnectionFactoryOneParam : FakeBaseConnectionFactory
    {
        public string Arg { get; set; }

        public FakeConnectionFactoryOneParam(string arg)
        {
            Arg = arg;
        }
    }

    public class FakeConnectionFactoryManyParams : FakeBaseConnectionFactory
    {
        public List<string> Args { get; set; }

        public FakeConnectionFactoryManyParams(
            string arg0, string arg1, string arg2, string arg3, string arg4,
            string arg5, string arg6, string arg7, string arg8, string arg9,
            string arg10)
        {
            Args = new List<string>
                {
                    arg0,
                    arg1,
                    arg2,
                    arg3,
                    arg4,
                    arg5,
                    arg6,
                    arg7,
                    arg8,
                    arg9,
                    arg10
                };
        }
    }

    public class FakeNonConnectionFactory
    {
    }

    #endregion

    #region Visual Studio threading helpers

    /// <summary>
    /// This class handles re-tries that can be required when calling into Visual Studio
    /// from a non-VS thread.
    /// See http://msdn.microsoft.com/en-us/library/ms228772(v=VS.100).aspx
    /// </summary>
    public class MessageFilter : IOleMessageFilter
    {
        public static void Register()
        {
            IOleMessageFilter _;
            CoRegisterMessageFilter(new MessageFilter(), out _);
        }

        public static void Revoke()
        {
            IOleMessageFilter _;
            CoRegisterMessageFilter(null, out _);
        }

        int IOleMessageFilter.HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
        {
            return 0;
        }

        int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2)
            {
                // Retry the thread call immediately if return >= 0 & < 100.
                return 99;
            }
            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
        {
            return 2;
        }

        [DllImport("Ole32.dll")]
        private static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);
    }

    [ComImport]
    [Guid("00000016-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleMessageFilter
    {
        [PreserveSig]
        int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);

        [PreserveSig]
        int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);

        [PreserveSig]
        int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);
    }

    #endregion
}

#endif
