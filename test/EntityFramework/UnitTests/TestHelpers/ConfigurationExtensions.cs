// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Configuration;
    using System.Data.Entity.Internal.ConfigFile;

    public static class ConfigurationExtensions
    {
        public static Configuration AddDefaultConnectionFactory(
            this Configuration config,
            string factoryName,
            params string[] parameters)
        {
            var ef = (EntityFrameworkSection)config.GetSection("entityFramework");
            ef.DefaultConnectionFactory.FactoryTypeName = factoryName;

            foreach (var argument in parameters)
            {
                var element = ef.DefaultConnectionFactory.Parameters.NewElement();
                element.ValueString = argument;
            }

            config.Save();

            return ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = config.FilePath
                    },
                ConfigurationUserLevel.None);
        }

        public static Configuration AddConnectionString(
            this Configuration config,
            string name,
            string connectionString,
            string providerName = null)
        {
            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings(name, connectionString, providerName));

            config.Save();

            return ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = config.FilePath
                    },
                ConfigurationUserLevel.None);
        }
    }
}
