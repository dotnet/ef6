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
        private readonly ResolverChain _secondaryResolvers = new ResolverChain();
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
            _resolvers.Add(new DefaultExecutionStrategyResolver());
            _resolvers.Add(new CachingDependencyResolver(defaultProviderServicesResolver));
            _resolvers.Add(new CachingDependencyResolver(new DefaultProviderFactoryResolver()));
            _resolvers.Add(new CachingDependencyResolver(new DefaultInvariantNameResolver()));
            _resolvers.Add(new SingletonDependencyResolver<IDbConnectionFactory>(new SqlConnectionFactory()));
            _resolvers.Add(new SingletonDependencyResolver<IDbModelCacheKeyFactory>(new DefaultModelCacheKeyFactory()));
            _resolvers.Add(new SingletonDependencyResolver<IManifestTokenService>(new DefaultManifestTokenService()));
            _resolvers.Add(new SingletonDependencyResolver<HistoryContextFactory>((e, d) => new HistoryContext(e, d)));
            _resolvers.Add(new SingletonDependencyResolver<IPluralizationService>(new EnglishPluralizationService()));
            _resolvers.Add(new SingletonDependencyResolver<IViewAssemblyCache>(new ViewAssemblyCache()));

#if NET40
            _resolvers.Add(new SingletonDependencyResolver<IDbProviderFactoryService>(new Net40DefaultDbProviderFactoryService()));
#else
            _resolvers.Add(new SingletonDependencyResolver<IDbProviderFactoryService>(new DefaultDbProviderFactoryService()));
#endif
        }

        public DatabaseInitializerResolver DatabaseInitializerResolver
        {
            get { return _databaseInitializerResolver; }
        }

        /// <inheritdoc />
        public virtual object GetService(Type type, object key)
        {
            return _secondaryResolvers.GetService(type, key) ?? _resolvers.GetService(type, key);
        }

        public virtual void AddSecondaryResolver(IDbDependencyResolver resolver)
        {
            DebugCheck.NotNull(resolver);

            _secondaryResolvers.Add(resolver);
        }
    }
}
