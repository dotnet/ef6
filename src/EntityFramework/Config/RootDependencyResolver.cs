// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This resolver is always the last resolver in the internal resolver chain and is
    ///     responsible for providing the default service for each dependency or throwing an
    ///     exception if there is no reasonable default service.
    /// </summary>
    internal class RootDependencyResolver : IDbDependencyResolver
    {
        private readonly ResolverChain _resolvers = new ResolverChain();
        private readonly DatabaseInitializerResolver _databaseInitializerResolver;

        public RootDependencyResolver()
            : this(new DefaultProviderServicesResolver(), new DatabaseInitializerResolver())
        {
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Reliability", "CA2000: Dispose objects before losing scope")]
        public RootDependencyResolver(
            DefaultProviderServicesResolver defaultProviderServicesResolver,
            DatabaseInitializerResolver databaseInitializerResolver)
        {
            DebugCheck.NotNull(defaultProviderServicesResolver);
            DebugCheck.NotNull(databaseInitializerResolver);

            _databaseInitializerResolver = databaseInitializerResolver;

            _resolvers.Add(_databaseInitializerResolver);

            _resolvers.Add(
                new TransientDependencyResolver<MigrationSqlGenerator>(
                    () => new SqlServerMigrationSqlGenerator(), "System.Data.SqlClient"));

            _resolvers.Add(
                new TransientDependencyResolver<MigrationSqlGenerator>(
                    () => new SqlCeMigrationSqlGenerator(), "System.Data.SqlServerCe.4.0"));

            _resolvers.Add(new ExecutionStrategyResolver());
            _resolvers.Add(new CachingDependencyResolver(defaultProviderServicesResolver));
            _resolvers.Add(new CachingDependencyResolver(new DefaultProviderFactoryResolver()));
            _resolvers.Add(new CachingDependencyResolver(new DefaultInvariantNameResolver()));
            _resolvers.Add(new SingletonDependencyResolver<IDbConnectionFactory>(new SqlConnectionFactory()));
            _resolvers.Add(new SingletonDependencyResolver<IDbModelCacheKeyFactory>(new DefaultModelCacheKeyFactory()));
            _resolvers.Add(new SingletonDependencyResolver<IManifestTokenService>(new DefaultManifestTokenService()));
            _resolvers.Add(new SingletonDependencyResolver<IHistoryContextFactory>(new DefaultHistoryContextFactory()));
            _resolvers.Add(new ThreadLocalDependencyResolver<IDbCommandInterceptor>(() => new DefaultCommandInterceptor()));
            _resolvers.Add(new SingletonDependencyResolver<IDbProviderFactoryService>(new DefaultDbProviderFactoryService()));
            _resolvers.Add(new SingletonDependencyResolver<IPluralizationService>(new EnglishPluralizationService()));
            _resolvers.Add(new SingletonDependencyResolver<IViewAssemblyCache>(new ViewAssemblyCache()));
        }

        public DatabaseInitializerResolver DatabaseInitializerResolver
        {
            get { return _databaseInitializerResolver; }
        }

        /// <inheritdoc />
        public virtual object GetService(Type type, object key)
        {
            return _resolvers.GetService(type, key);
        }
    }
}
