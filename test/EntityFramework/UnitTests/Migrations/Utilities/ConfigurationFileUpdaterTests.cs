// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class ConfigurationFileUpdaterTests
    {
        [Fact]
        public void Update_adds_entity_framework_codebase_to_config()
        {
            var configurationFile = Path.GetTempFileName();

            try
            {
                File.WriteAllText(
                    configurationFile,
                    @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
</configuration>");

                var updatedConfigurationFile = new ConfigurationFileUpdater().Update(configurationFile);
                Assert.True(File.Exists(updatedConfigurationFile));

                try
                {
                    var updatedConfiguration = File.ReadAllText(updatedConfigurationFile);
                    var entityFrameworkAssemblyName = typeof(DbContext).Assembly.GetName();

                    Assert.Equal(
                        @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""EntityFramework"" culture=""neutral"" publicKeyToken=""b77a5c561934e089"" />
        <codeBase version=""" + entityFrameworkAssemblyName.Version + @""" href="""
                        + entityFrameworkAssemblyName.CodeBase + @""" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>",
                        updatedConfiguration);
                }
                finally
                {
                    File.Delete(updatedConfigurationFile);
                }
            }
            finally
            {
                File.Delete(configurationFile);
            }
        }

        [Fact]
        public void Update_creates_new_config_file_if_missing()
        {
            var configurationFile = Path.GetTempFileName();
            File.Delete(configurationFile);

            var updatedConfigurationFile = new ConfigurationFileUpdater().Update(configurationFile);
            Assert.True(File.Exists(updatedConfigurationFile));

            try
            {
                var updatedConfiguration = File.ReadAllText(updatedConfigurationFile);
                var entityFrameworkAssemblyName = typeof(DbContext).Assembly.GetName();

                Assert.Equal(
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""EntityFramework"" culture=""neutral"" publicKeyToken=""b77a5c561934e089"" />
        <codeBase version=""" + entityFrameworkAssemblyName.Version + @""" href="""
                    + entityFrameworkAssemblyName.CodeBase + @""" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>",
                    updatedConfiguration);
            }
            finally
            {
                File.Delete(updatedConfigurationFile);
            }
        }

        [Fact]
        public void Updated_file_can_resolve_sourceConfigs()
        {
            var directory = Path.GetTempFileName();
            File.Delete(directory);
            Directory.CreateDirectory(directory);

            try
            {
                var configPath = Path.Combine(directory, "App.config");
                File.WriteAllText(
                    configPath,
                    @"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
  <connectionStrings configSource='connectionStrings.config' />
</configuration>");

                File.WriteAllText(
                    Path.Combine(directory, "connectionStrings.config"),
                    @"<?xml version='1.0' encoding='utf-8' ?>
<connectionStrings>
  <add name='MyConnectionString' connectionString='Data Source=.\SQLEXPRESS; Data Source=MyDatabase; Integrated Security=True' providerName='System.Data.SqlClient'/>
</connectionStrings>");

                var updatedConfigPath = new ConfigurationFileUpdater().Update(configPath);

                try
                {
                    var config = ConfigurationManager.OpenMappedExeConfiguration(
                        new ExeConfigurationFileMap
                            {
                                ExeConfigFilename = updatedConfigPath
                            },
                        ConfigurationUserLevel.None);

                    Assert.True(
                        config.ConnectionStrings.ConnectionStrings.Cast<ConnectionStringSettings>().Any(
                            css => css.Name == "MyConnectionString"));
                }
                finally
                {
                    File.Delete(updatedConfigPath);
                }
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
