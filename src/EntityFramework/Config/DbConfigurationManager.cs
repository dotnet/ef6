namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    // TODO: Thread safety
    /// <summary>
    /// This class is responsible for managing the app-domain instance of the <see cref="DbConfiguration"/> class.
    /// This includes loading from config, discovery from the context assembly and pushing/popping configurations
    /// used by <see cref="DbContextInfo"/>.
    /// </summary>
    internal class DbConfigurationManager
    {
        private static readonly DbConfigurationManager _configManager
            = new DbConfigurationManager(new DbConfigurationLoader(), new DbConfigurationFinder());

        private readonly DbConfigurationLoader _loader;
        private readonly DbConfigurationFinder _finder;
        private readonly ISet<Assembly> _knownAssemblies = new HashSet<Assembly>();
        private DbConfiguration _configuration;
        private readonly IList<Tuple<AppConfig, DbConfiguration>> _configurationOverrides
            = new List<Tuple<AppConfig, DbConfiguration>>();

        public DbConfigurationManager(DbConfigurationLoader loader, DbConfigurationFinder finder)
        {
            Contract.Requires(loader != null);
            Contract.Requires(finder != null);

            _loader = loader;
            _finder = finder;
        }
        
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

            configuration = _loader.TryLoadFromConfig(AppConfig.DefaultInstance) ?? configuration;

            if (_configuration == null)
            {
                _configuration = configuration;
                _configuration.Lock();
            }
            else if (_configuration.GetType() != configuration.GetType())
            {
                if (_configuration.GetType() == typeof(DbConfiguration))
                {
                    throw new InvalidOperationException(Strings.DefaultConfigurationUsedBeforeSet(configuration.GetType().Name));
                }

                throw new InvalidOperationException(Strings.ConfigurationSetTwice(configuration.GetType().Name, _configuration.GetType().Name));
            }
        }

        public virtual void EnsureLoadedForContext(Type contextType)
        {
            Contract.Requires(contextType != null);
            Contract.Requires(typeof(DbContext).IsAssignableFrom(contextType));
            
            var contextAssembly = contextType.Assembly;

            if (contextType == typeof(DbContext)
                || _knownAssemblies.Contains(contextAssembly) 
                || _configurationOverrides.Count != 0)
            {
                return;
            }

            if (_configuration == null)
            {
                var foundConfiguration = _finder.TryCreateConfiguration(contextType.Assembly.GetAccessibleTypes());
                if (foundConfiguration != null)
                {
                    SetConfiguration(foundConfiguration);
                }
            }
            else if (!contextAssembly.IsDynamic) // Don't throw for proxy contexts created in dynamic assemblies
            {
                var foundType = _finder.TryFindConfigurationType(contextType.Assembly.GetAccessibleTypes());
                if (!typeof(DbNullConfiguration).IsAssignableFrom(foundType))
                {
                    if (_configuration.GetType() == typeof(DbConfiguration))
                    {
                        if (foundType != null)
                        {
                            throw new InvalidOperationException(Strings.ConfigurationNotDiscovered(foundType.Name));
                        }
                    }
                    else
                    {
                        if (foundType == null || foundType != _configuration.GetType())
                        {
                            throw new InvalidOperationException(
                                Strings.SetConfigurationNotDiscovered(_configuration.GetType().Name, contextType.Name));
                        }
                    }
                }
            }

            _knownAssemblies.Add(contextAssembly);
        }

        public virtual void PushConfuguration(AppConfig config, Type contextType)
        {
            Contract.Requires(config != null);
            Contract.Requires(contextType != null);
            Contract.Requires(typeof(DbContext).IsAssignableFrom(contextType));
            
            var configuration = _loader.TryLoadFromConfig(config)
                                ?? _finder.TryCreateConfiguration(contextType.Assembly.GetAccessibleTypes())
                                ?? new DbConfiguration();

            configuration.AddAppConfigResolver(new AppConfigDependencyResolver(config));

            configuration.Lock();
            _configurationOverrides.Add(Tuple.Create(config, configuration));
        }

        public virtual void PopConfuguration(AppConfig config)
        {
            Contract.Requires(config != null);

            var configuration = _configurationOverrides.FirstOrDefault(c => c.Item1 == config);
            if (configuration != null)
            {
                _configurationOverrides.Remove(configuration);
            }
        }
    }
}