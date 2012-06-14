namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Diagnostics.Contracts;

    internal class DbConfigurationManager
    {
        private static readonly DbConfigurationManager _configManager = new DbConfigurationManager();

        private readonly ISet<Type> _knownContexts = new HashSet<Type>();
        private DbConfiguration _configuration;

        public static DbConfigurationManager Instance
        {
            get { return _configManager; }
        }

        public virtual DbConfiguration GetConfiguration()
        {
            if (_configuration != null)
            {
                return _configuration;
            }

            // Try to load from _appConfig

            return new DbConfiguration();
        }

        public virtual void SetConfiguration(DbConfiguration configuration)
        {
            Contract.Requires(configuration != null);

            if (_configuration != null && !ReferenceEquals(_configuration, configuration))
            {
                throw new InvalidOperationException("DbConfiguration can only be set once.");
            }

            // TODO: Do we support setting the DbConfiguration instance in app.config?
            // If so, how do we ensure that DbContextInfo uses the correct one?
            _configuration = TryLoadFromConfig(AppConfig.DefaultInstance);

            if (_configuration == null) // Setting from app.config overrides setting in code.
            {
                _configuration = configuration;
            }
        }

        public virtual void EnsureLoadedForContext(Type contextType)
        {
            if (_knownContexts.Contains(contextType))
            {
                return;
            }

            _knownContexts.Add(contextType);

            var foundConfig = TryLoadFromConfig(AppConfig.DefaultInstance)
                ?? new DbConfigurationFinder(contextType.Assembly.GetTypes()).FindConfiguration(); // TODO: Make sure not to throw in PT

            if (_configuration == null)
            {
                _configuration = foundConfig ?? new DbConfiguration();
            }
            else if (!ReferenceEquals(foundConfig, _configuration))
            {
                if (_configuration.GetType() == typeof(DbConfiguration))
                {
                    throw new InvalidOperationException("Was using default--need to set DbConfiguration.Configuration at app start-up.");
                }
                throw new InvalidOperationException("Was using specified config but different to the one in the context. Put config in the same assembly as context.");
            }
        }

        public virtual DbConfiguration TryLoadFromConfig(AppConfig config)
        {
            // TODO: Implement loading from app.config
            return null;
        }
    }
}