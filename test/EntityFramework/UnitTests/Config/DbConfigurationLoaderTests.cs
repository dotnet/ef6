// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Configuration;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.TestHelpers;
    using Moq;
    using Xunit;

    public class DbConfigurationLoaderTests
    {
        [Fact]
        public void TryLoadFromConfig_returns_null_if_config_element_is_missing_or_empty()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());

            mockConfig.Setup(m => m.ConfigurationTypeName).Returns((string)null);
            Assert.Null(new DbConfigurationLoader().TryLoadFromConfig(mockConfig.Object));

            mockConfig.Setup(m => m.ConfigurationTypeName).Returns("");
            Assert.Null(new DbConfigurationLoader().TryLoadFromConfig(mockConfig.Object));

            mockConfig.Setup(m => m.ConfigurationTypeName).Returns("");
            Assert.Null(new DbConfigurationLoader().TryLoadFromConfig(mockConfig.Object));
        }

        [Fact]
        public void TryLoadFromConfig_returns_correct_DbConfiguration_type()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.ConfigurationTypeName).Returns(typeof(FunctionalTestsConfiguration).AssemblyQualifiedName);

            Assert.Same(typeof(FunctionalTestsConfiguration), new DbConfigurationLoader().TryLoadFromConfig(mockConfig.Object));
        }

        [Fact]
        public void TryLoadFromConfig_throws_if_type_cannot_be_loaded()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.ConfigurationTypeName).Returns("I.Is.Not.A.Type");

            var loader = new DbConfigurationLoader();
            Assert.Equal(
                Strings.DbConfigurationTypeNotFound("I.Is.Not.A.Type"),
                Assert.Throws<InvalidOperationException>(() => loader.TryLoadFromConfig(mockConfig.Object)).Message);
        }

        [Fact]
        public void TryLoadFromConfig_throws_if_type_does_not_extend_DbConfiguration()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.ConfigurationTypeName).Returns(typeof(Random).AssemblyQualifiedName);

            var loader = new DbConfigurationLoader();
            Assert.Equal(
                Strings.CreateInstance_BadDbConfigurationType(typeof(Random).ToString(), typeof(DbConfiguration).ToString()),
                Assert.Throws<InvalidOperationException>(() => loader.TryLoadFromConfig(mockConfig.Object)).Message);
        }

        [Fact]
        public void AppConfigContainsDbConfigurationType_returns_false_if_config_element_is_missing_or_empty()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());

            mockConfig.Setup(m => m.ConfigurationTypeName).Returns((string)null);
            Assert.False(new DbConfigurationLoader().AppConfigContainsDbConfigurationType(mockConfig.Object));

            mockConfig.Setup(m => m.ConfigurationTypeName).Returns("");
            Assert.False(new DbConfigurationLoader().AppConfigContainsDbConfigurationType(mockConfig.Object));

            mockConfig.Setup(m => m.ConfigurationTypeName).Returns("");
            Assert.False(new DbConfigurationLoader().AppConfigContainsDbConfigurationType(mockConfig.Object));
        }

        [Fact]
        public void AppConfigContainsDbConfigurationType_returns_true_if_config_element_is_specified()
        {
            var mockConfig = new Mock<AppConfig>(new ConnectionStringSettingsCollection());
            mockConfig.Setup(m => m.ConfigurationTypeName).Returns(typeof(FunctionalTestsConfiguration).AssemblyQualifiedName);

            Assert.True(new DbConfigurationLoader().AppConfigContainsDbConfigurationType(mockConfig.Object));
        }
    }
}
