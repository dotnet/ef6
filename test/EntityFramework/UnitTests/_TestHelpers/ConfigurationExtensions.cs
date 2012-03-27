namespace System.Data.Entity
{
    using System.Configuration;
    using System.Data.Entity.Internal.ConfigFile;

    public static class ConfigurationExtensions
    {
        public static Configuration AddContextConfig(
            this Configuration config,
            string contextType,
            bool? isDatabaseInitializationDisabled = null)
        {
            var ef = (EntityFrameworkSection)config.GetSection("entityFramework");
            var init = ef.Contexts.NewElement(contextType);
            init.DatabaseInitializer = null;

            if (isDatabaseInitializationDisabled.HasValue)
            {
                init.IsDatabaseInitializationDisabled = isDatabaseInitializationDisabled.Value;
            }

            config.Save();

            return ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap() { ExeConfigFilename = config.FilePath },
                    ConfigurationUserLevel.None);
        }

        public static Configuration AddContextConfig(
            this Configuration config,
            string contextType,
            string initializerType,
            bool? isDatabaseInitializationDisabled = null,
            string[] initializerParameters = null,
            string[] initializerParameterTypes = null)
        {
            var ef = (EntityFrameworkSection)config.GetSection("entityFramework");
            var init = ef.Contexts.NewElement(contextType);

            if (initializerType != null)
            {
                init.DatabaseInitializer.InitializerTypeName = initializerType;
            }

            if (isDatabaseInitializationDisabled.HasValue)
            {
                init.IsDatabaseInitializationDisabled = isDatabaseInitializationDisabled.Value;
            }

            if (initializerParameters != null)
            {
                for (int i = 0; i < initializerParameters.Length; i++)
                {
                    var element = init.DatabaseInitializer.Parameters.NewElement();
                    element.ValueString = initializerParameters[i];
                    if (initializerParameterTypes != null && i < initializerParameterTypes.Length)
                    {
                        element.TypeName = initializerParameterTypes[i];
                    }

                }
            }

            config.Save();

            return ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap() { ExeConfigFilename = config.FilePath },
                    ConfigurationUserLevel.None);
        }

        public static Configuration AddLegacyDatabaseInitializer(
            this Configuration config,
            string contextType,
            string initializerType)
        {
            config.AppSettings.Settings.Add(
                "DatabaseInitializerForType " + contextType,
                initializerType);

            config.Save();

            return ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap() { ExeConfigFilename = config.FilePath },
                    ConfigurationUserLevel.None);
        }

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
                    new ExeConfigurationFileMap() { ExeConfigFilename = config.FilePath },
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
                    new ExeConfigurationFileMap() { ExeConfigFilename = config.FilePath },
                    ConfigurationUserLevel.None);
        }
    }
}