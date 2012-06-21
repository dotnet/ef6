namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Diagnostics.Contracts;
    using System.Linq;

    // TODO: Thread safety
    internal class DbConfigurationManager
    {
        private static readonly DbConfigurationManager _configManager = new DbConfigurationManager();

        private readonly ISet<Type> _knownContexts = new HashSet<Type>();
        private DbConfiguration _configuration;
        private readonly IList<Tuple<AppConfig, DbConfiguration>> _configurationOverrides
            = new List<Tuple<AppConfig, DbConfiguration>>();

        public static DbConfigurationManager Instance
        {
            get { return _configManager; }
        }

        public virtual DbConfiguration GetConfiguration()
        {
            if (_configurationOverrides.Count != 0)
            {
                return _configurationOverrides.Last().Item2;
            }

            if (_configuration == null)
            {
                SetConfiguration(new DbConfiguration());
            }

            return _configuration;
        }

        public virtual void SetConfiguration(DbConfiguration configuration)
        {
            Contract.Requires(configuration != null);

            configuration = TryLoadFromConfig(AppConfig.DefaultInstance) ?? configuration;

            if (_configuration == null)
            {
                _configuration = configuration;
                _configuration.Lock();
            }
            else if (_configuration.GetType() != configuration.GetType())
            {
                if (_configuration.GetType() == typeof(DbConfiguration))
                {
                    throw new InvalidOperationException("Default DbConfiguration was used before call to set configuration.");
                }

                throw new InvalidOperationException("DbConfiguration can only be set once.");
            }
        }

        public virtual void EnsureLoadedForContext(Type contextType)
        {
            if (contextType == typeof(DbContext) 
                || _knownContexts.Contains(contextType) 
                || _configurationOverrides.Count != 0)
            {
                return;
            }

            _knownContexts.Add(contextType);

            // TODO: Make sure to get types in a partial-trust safe way
            var finder = new DbConfigurationFinder(contextType.Assembly.GetTypes());

            if (_configuration == null)
            {
                SetConfiguration(finder.TryCreateConfiguration() ?? new DbConfiguration());
            }
            else
            {
                var foundType = finder.TryFindConfigurationType();
                if (!typeof(DbNullConfiguration).IsAssignableFrom(foundType))
                {
                    if (_configuration.GetType() == typeof(DbConfiguration))
                    {
                        if (foundType != null)
                        {
                            throw new InvalidOperationException(
                                "Was using default config but not the one found in same assembly. Put config in the same assembly as context.");
                        }
                    }
                    else
                    {
                        if (foundType == null)
                        {
                            throw new InvalidOperationException(
                                "Was using set config but not the one found in same assembly. Put config in the same assembly as context.");
                        }
                        if (foundType != _configuration.GetType())
                        {
                            throw new InvalidOperationException(
                                "Was using specified config but different to the one in the context. Put config in the same assembly as context.");
                        }
                    }
                }
            }
        }

        public virtual void PushConfuguration(AppConfig config, Type contextType)
        {
            // TODO: Make sure to get types in a partial-trust safe way
            var configuration = TryLoadFromConfig(config)
                                ?? new DbConfigurationFinder(contextType.Assembly.GetTypes()).TryCreateConfiguration()
                                ?? new DbConfiguration();
            configuration.AddAppConfigResolver(new AppConfigDependencyResolver(config));

            configuration.Lock();
            _configurationOverrides.Add(Tuple.Create(config, configuration));
        }

        public virtual void PopConfuguration(AppConfig config)
        {
            var configuration = _configurationOverrides.FirstOrDefault(c => c.Item1 == config);
            if (configuration != null)
            {
                _configurationOverrides.Remove(configuration);
            }
        }

        public virtual DbConfiguration TryLoadFromConfig(AppConfig config)
        {
            // TODO: Implement loading from app.config
            return null;
        }
    }
}