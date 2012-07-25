// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Config
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;

    /// <summary>
    /// This resolver is always the last resolver in the internal resolver chain and is
    /// responsible for providing the default service for each dependency or throwing an
    /// exception if there is no reasonable default service.
    /// </summary>
    internal class RootDependencyResolver : IDbDependencyResolver
    {
        private readonly MigrationsConfigurationResolver _migrationsConfigurationResolver;
        private readonly ResolverChain _resolvers = new ResolverChain();

        public RootDependencyResolver(
            MigrationsConfigurationResolver migrationsConfigurationResolver,
            DefaultProviderServicesResolver defaultProviderServicesResolver)
        {
            _migrationsConfigurationResolver = migrationsConfigurationResolver;

            _resolvers.Add(_migrationsConfigurationResolver);
            _resolvers.Add(new CachingDependencyResolver(defaultProviderServicesResolver));
            _resolvers.Add(new SingletonDependencyResolver<IDbConnectionFactory>(new SqlConnectionFactory()));
            _resolvers.Add(new SingletonDependencyResolver<IDbModelCacheKeyFactory>(new DefaultModelCacheKeyFactory()));
        }

        public MigrationsConfigurationResolver MigrationsConfigurationResolver
        {
            get { return _migrationsConfigurationResolver; }
        }

        /// <inheritdoc/>
        public virtual object GetService(Type type, string name)
        {
            // TODO: Handle Database initializer

            return _resolvers.GetService(type, name);
        }

        /// <inheritdoc/>
        public virtual void Release(object service)
        {
            _resolvers.Release(service);
        }
    }
}
