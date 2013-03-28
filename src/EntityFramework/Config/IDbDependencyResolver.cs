// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Spatial;

    /// <summary>
    ///     This interface is implemented by any object that can resolve a dependency, either directly
    ///     or through use of an external container.
    /// </summary>
    /// <remarks>
    ///     Note that multiple threads may call into the same IDbDependencyResolver instance which means
    ///     that implementations of this interface must be either immutable or thread-safe.
    ///     The public services currently resolved using IDbDependencyResolver are:
    ///     <see cref="IDatabaseInitializer{TContext}" />
    ///     Object returned: A database initializer for the given context type
    ///     Lifetime of returned service: Singleton—same object may be used multiple times by different threads
    ///     Key is not used; will be null
    ///     <see cref="MigrationSqlGenerator" />
    ///     Object returned: A SQL generator that can be used for Migrations and other actions that cause a database to be created
    ///     Lifetime of returned service: Transient—a new object should be returned each time GetService is called
    ///     Key is the ADO.NET provider invariant name string
    ///     <see cref="DbProviderServices" />
    ///     Object returned: An EF provider
    ///     Lifetime of returned service: Singleton—same object may be used multiple times by different threads
    ///     Key is the ADO.NET provider invariant name string
    ///     <see cref="IDbConnectionFactory" />
    ///     Object returned: The default connection factory that will be used when EF creates a database connection by convention
    ///     Lifetime of returned service: Singleton—same object may be used multiple times by different threads
    ///     Key is not used; will be null
    ///     <see cref="IManifestTokenService" />
    ///     Object returned: A service that can generated a provider manifest token from a connection
    ///     Lifetime of returned service: Singleton—same object may be used multiple times by different threads
    ///     Key is not used; will be null
    ///     <see cref="IDbProviderFactoryService" />
    ///     Object returned: A service that can obtain a provider factory from a given connection
    ///     Lifetime of returned service: Singleton—same object may be used multiple times by different threads
    ///     Key is not used; will be null
    ///     <see cref="IDbModelCacheKeyFactory" />
    ///     Object returned: A factory that will generate a model cache key for a given context
    ///     Lifetime of returned service: Singleton—same object may be used multiple times by different threads
    ///     Key is not used; will be null
    ///     <see cref="DbSpatialServices" />
    ///     Object returned: an EF spatial provider
    ///     Lifetime of returned service: Singleton—same object may be used multiple times by different threads
    ///     Key is not used; will be null
    ///     <see cref="Func{IExecutionStrategy}" />
    ///     Object returned: An execution strategy factory for store operations
    ///     Lifetime of returned service: Singleton—same object may be used multiple times by different threads
    ///     Key is <see cref="ExecutionStrategyKey" /> consisting of the ADO.NET provider invariant name string and the database server address.
    /// </remarks>
    public interface IDbDependencyResolver
    {
        /// <summary>
        ///     Attempts to resolve a dependency for a given contract type and optionally a given key.
        ///     If the resolver cannot resolve the dependency then it must return null and not throw. This
        ///     allows resolvers to be used in a Chain of Responsibility pattern such that multiple resolvers
        ///     can be asked to resolve a dependency until one finally does.
        /// </summary>
        /// <param name="type"> The interface or abstract base class that defines the dependency to be resolved. The returned object is expected to be an instance of this type. </param>
        /// <param name="key"> Optionally, the key of the dependency to be resolved. This may be null for dependencies that are not differentiated by key. </param>
        /// <returns> The resolved dependency, which must be an instance of the given contract type, or null if the dependency could not be resolved. </returns>
        object GetService(Type type, object key);
    }
}
