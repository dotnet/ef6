// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    using System.Configuration;
    using System.IO;
    using Xunit;

    public class AppConfigReaderTests
    {
        [Fact]
        public void Ctor_validates_parameter()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new AppConfigReader(null));

            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void GetProviderServices_returns_provider_when_exists()
        {
            var reader = new AppConfigReader(
                CreateConfig("<provider invariantName='My.Invariant1' type='MyProvider1'/>"));

            var provider = reader.GetProviderServices("My.Invariant1");

            Assert.Equal("MyProvider1", provider);
        }

        [Fact]
        public void GetProviderServices_returns_null_when_not_exists()
        {
            var reader = new AppConfigReader(
                CreateConfig("<provider invariantName='My.Invariant1' type='MyProvider1'/>"));

            var provider = reader.GetProviderServices("My.Invariant2");

            Assert.Null(provider);
        }

        private static Configuration CreateConfig(string providers)
        {
            var file = Path.GetTempFileName();
            File.WriteAllText(
                file,
                @"<?xml version='1.0' encoding='utf-8'?>
                <configuration>
                  <configSections>
                    <section name='entityFramework' type='System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework' />
                  </configSections>
                  <entityFramework>
                    <providers>
                " + providers + @"
                    </providers>
                  </entityFramework>
                </configuration>");

            return ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap { ExeConfigFilename = file },
                ConfigurationUserLevel.None);
        }
    }
}
