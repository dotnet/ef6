namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Linq;

    internal class DbConfigurationFinder
    {
        private readonly IEnumerable<Type> _typesToSearch;

        public DbConfigurationFinder(IEnumerable<Type> typesToSearch)
        {
            _typesToSearch = typesToSearch;
        }

        public virtual Type TryFindConfigurationType()
        {
            var configurations = _typesToSearch
                .Where(
                    t => typeof(DbConfiguration).IsAssignableFrom(t)
                         && t != typeof(DbConfiguration)
                         && t != typeof(DbProxyConfiguration))
                .ToList();

            if (configurations.Count > 1)
            {
                throw new Exception("Multiple configuration classes defined.");
            }

            var configType = configurations.FirstOrDefault();

            return typeof(DbProxyConfiguration).IsAssignableFrom(configType)
                       ? CreateConfiguration<DbProxyConfiguration>(configType).ConfigurationToUse()
                       : configType;
        }

        public virtual DbConfiguration TryCreateConfiguration()
        {
            var configType = TryFindConfigurationType();

            return configType == null || typeof(DbNullConfiguration).IsAssignableFrom(configType)
                       ? null
                       : CreateConfiguration<DbConfiguration>(configType);
        }

        private static TConfig CreateConfiguration<TConfig>(Type configurationType) where TConfig : DbConfiguration
        {
            if (!typeof(TConfig).IsAssignableFrom(configurationType))
            {
                throw new InvalidOperationException("Bad type.");
            }

            if (configurationType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException("No constructor.");
            }

            if (configurationType.IsAbstract)
            {
                throw new InvalidOperationException("Is abstract.");
            }

            if (configurationType.IsGenericType)
            {
                throw new InvalidOperationException("Is generic.");
            }

            return (TConfig)Activator.CreateInstance(configurationType);
        }
    }
}
