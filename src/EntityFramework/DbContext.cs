// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Data.Entity.Validation;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A DbContext instance represents a combination of the Unit Of Work and Repository patterns such that
    ///     it can be used to query from a database and group together changes that will then be written
    ///     back to the store as a unit.
    ///     DbContext is conceptually similar to ObjectContext.
    /// </summary>
    /// <remarks>
    ///     DbContext is usually used with a derived type that contains <see cref="DbSet{TEntity}" /> properties for
    ///     the root entities of the model. These sets are automatically initialized when the
    ///     instance of the derived class is created.  This behavior can be modified by applying the
    ///     <see cref="SuppressDbSetInitializationAttribute" />  attribute to either the entire derived context
    ///     class, or to individual properties on the class.
    ///     The Entity Data Model backing the context can be specified in several ways.  When using the Code First
    ///     approach, the <see cref="DbSet{TEntity}" /> properties on the derived context are used to build a model
    ///     by convention.  The protected OnModelCreating method can be overridden to tweak this model.  More
    ///     control over the model used for the Model First approach can be obtained by creating a <see cref="DbCompiledModel" />
    ///     explicitly from a <see cref="DbModelBuilder" /> and passing this model to one of the DbContext constructors.
    ///     When using the Database First or Model First approach the Entity Data Model can be created using the
    ///     Entity Designer (or manually through creation of an EDMX file) and then this model can be specified using
    ///     entity connection string or an <see cref="System.Data.Entity.Core.EntityClient.EntityConnection" /> object.
    ///     The connection to the database (including the name of the database) can be specified in several ways.
    ///     If the parameterless DbContext constructor is called from a derived context, then the name of the derived context
    ///     is used to find a connection string in the app.config or web.config file.  If no connection string is found, then
    ///     the name is passed to the DefaultConnectionFactory registered on the <see cref="Entity.Database" /> class.  The connection
    ///     factory then uses the context name as the database name in a default connection string.  (This default connection
    ///     string points to .\SQLEXPRESS on the local machine unless a different DefaultConnectionFactory is registered.)
    ///     Instead of using the derived context name, the connection/database name can also be specified explicitly by
    ///     passing the name to one of the DbContext constructors that takes a string.  The name can also be passed in
    ///     the form "name=myname", in which case the name must be found in the config file or an exception will be thrown.
    ///     Note that the connection found in the app.config or web.config file can be a normal database connection
    ///     string (not a special Entity Framework connection string) in which case the DbContext will use Code First.
    ///     However, if the connection found in the config file is a special Entity Framework connection string, then the
    ///     DbContext will use Database/Model First and the model specified in the connection string will be used.
    ///     An existing or explicitly created DbConnection can also be used instead of the database/connection name.
    ///     A <see cref="DbModelBuilderVersionAttribute" /> can be applied to a class derived from DbContext to set the
    ///     version of conventions used by the context when it creates a model. If no attribute is applied then the
    ///     latest version of conventions will be used.
    /// </remarks>
    public class DbContext : IDisposable, IObjectContextAdapter
    {
        #region Construction and fields

        // Handles lazy creation of an underlying ObjectContext
        private InternalContext _internalContext;

        /// <summary>
        ///     Constructs a new context instance using conventions to create the name of the database to
        ///     which a connection will be made.  The by-convention name is the full name (namespace + class name)
        ///     of the derived context class.
        ///     See the class remarks for how this is used to create a connection.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected DbContext()
        {
            InitializeLazyInternalContext(new LazyInternalConnection(GetType().DatabaseName()));
        }

        /// <summary>
        ///     Constructs a new context instance using conventions to create the name of the database to
        ///     which a connection will be made, and initializes it from the given model.
        ///     The by-convention name is the full name (namespace + class name) of the derived context class.
        ///     See the class remarks for how this is used to create a connection.
        /// </summary>
        /// <param name="model"> The model that will back this context. </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected DbContext(DbCompiledModel model)
        {
            Check.NotNull(model, "model");

            InitializeLazyInternalContext(new LazyInternalConnection(GetType().DatabaseName()), model);
        }

        /// <summary>
        ///     Constructs a new context instance using the given string as the name or connection string for the
        ///     database to which a connection will be made.
        ///     See the class remarks for how this is used to create a connection.
        /// </summary>
        /// <param name="nameOrConnectionString"> Either the database name or a connection string. </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public DbContext(string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            InitializeLazyInternalContext(new LazyInternalConnection(nameOrConnectionString));
        }

        /// <summary>
        ///     Constructs a new context instance using the given string as the name or connection string for the
        ///     database to which a connection will be made, and initializes it from the given model.
        ///     See the class remarks for how this is used to create a connection.
        /// </summary>
        /// <param name="nameOrConnectionString"> Either the database name or a connection string. </param>
        /// <param name="model"> The model that will back this context. </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public DbContext(string nameOrConnectionString, DbCompiledModel model)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
            Check.NotNull(model, "model");

            InitializeLazyInternalContext(new LazyInternalConnection(nameOrConnectionString), model);
        }

        /// <summary>
        ///     Constructs a new context instance using the existing connection to connect to a database.
        ///     The connection will not be disposed when the context is disposed if <paramref name="contextOwnsConnection" />
        ///     is <c>false</c>.
        /// </summary>
        /// <param name="existingConnection"> An existing connection to use for the new context. </param>
        /// <param name="contextOwnsConnection">
        ///     If set to <c>true</c> the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public DbContext(DbConnection existingConnection, bool contextOwnsConnection)
        {
            Check.NotNull(existingConnection, "existingConnection");

            InitializeLazyInternalContext(new EagerInternalConnection(existingConnection, contextOwnsConnection));
        }

        /// <summary>
        ///     Constructs a new context instance using the existing connection to connect to a database,
        ///     and initializes it from the given model.
        ///     The connection will not be disposed when the context is disposed if <paramref name="contextOwnsConnection" />
        ///     is <c>false</c>.
        ///     <param name="existingConnection"> An existing connection to use for the new context. </param>
        ///     <param name="model"> The model that will back this context. </param>
        ///     <param name="contextOwnsConnection">
        ///         If set to <c>true</c> the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.
        ///     </param>
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public DbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
        {
            Check.NotNull(existingConnection, "existingConnection");
            Check.NotNull(model, "model");

            InitializeLazyInternalContext(new EagerInternalConnection(existingConnection, contextOwnsConnection), model);
        }

        /// <summary>
        ///     Constructs a new context instance around an existing ObjectContext.
        ///     <param name="objectContext"> An existing ObjectContext to wrap with the new context. </param>
        ///     <param name="dbContextOwnsObjectContext">
        ///         If set to <c>true</c> the ObjectContext is disposed when the DbContext is disposed, otherwise the caller must dispose the connection.
        ///     </param>
        /// </summary>
        public DbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
        {
            Check.NotNull(objectContext, "objectContext");

            DbConfigurationManager.Instance.EnsureLoadedForContext(GetType());

            _internalContext = new EagerInternalContext(this, objectContext, dbContextOwnsObjectContext);
            DiscoverAndInitializeSets();
        }

        /// <summary>
        ///     Initializes the internal context, discovers and initializes sets, and initializes from a model if one is provided.
        /// </summary>
        private void InitializeLazyInternalContext(IInternalConnection internalConnection, DbCompiledModel model = null)
        {
            DbConfigurationManager.Instance.EnsureLoadedForContext(GetType());

            _internalContext = new LazyInternalContext(
                this, internalConnection, model
                , DbConfiguration.GetService<IDbModelCacheKeyFactory>()
                , DbConfiguration.GetService<AttributeProvider>());
            DiscoverAndInitializeSets();
        }

        /// <summary>
        ///     Discovers DbSets and initializes them.
        /// </summary>
        private void DiscoverAndInitializeSets()
        {
            new DbSetDiscoveryService(this).InitializeSets();
        }

        #endregion

        #region Model building

        /// <summary>
        ///     This method is called when the model for a derived context has been initialized, but
        ///     before the model has been locked down and used to initialize the context.  The default
        ///     implementation of this method does nothing, but it can be overridden in a derived class
        ///     such that the model can be further configured before it is locked down.
        /// </summary>
        /// <remarks>
        ///     Typically, this method is called only once when the first instance of a derived context
        ///     is created.  The model for that context is then cached and is for all further instances of
        ///     the context in the app domain.  This caching can be disabled by setting the ModelCaching
        ///     property on the given ModelBuidler, but note that this can seriously degrade performance.
        ///     More control over caching is provided through use of the DbModelBuilder and DbContextFactory
        ///     classes directly.
        /// </remarks>
        /// <param name="modelBuilder"> The builder that defines the model for the context being created. </param>
        protected virtual void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        /// <summary>
        ///     Internal method used to make the call to the real OnModelCreating method.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        internal void CallOnModelCreating(DbModelBuilder modelBuilder)
        {
            OnModelCreating(modelBuilder);
        }

        #endregion

        #region Database management

        /// <summary>
        ///     Creates a Database instance for this context that allows for creation/deletion/existence checks
        ///     for the underlying database.
        /// </summary>
        public Database Database
        {
            get { return new Database(InternalContext); }
        }

        #endregion

        #region Context methods

        /// <summary>
        ///     Returns a DbSet instance for access to entities of the given type in the context,
        ///     the ObjectStateManager, and the underlying store.
        /// </summary>
        /// <remarks>
        ///     See the DbSet class for more details.
        /// </remarks>
        /// <typeparam name="TEntity"> The type entity for which a set should be returned. </typeparam>
        /// <returns> A set for the given entity type. </returns>
        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return (DbSet<TEntity>)InternalContext.Set<TEntity>();
        }

        /// <summary>
        ///     Returns a non-generic DbSet instance for access to entities of the given type in the context,
        ///     the ObjectStateManager, and the underlying store.
        /// </summary>
        /// <param name="entityType"> The type of entity for which a set should be returned. </param>
        /// <returns> A set for the given entity type. </returns>
        /// <remarks>
        ///     See the DbSet class for more details.
        /// </remarks>
        public DbSet Set(Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            return (DbSet)InternalContext.Set(entityType);
        }

        /// <summary>
        ///     Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns> The number of objects written to the underlying database. </returns>
        /// <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        public virtual int SaveChanges()
        {
            return InternalContext.SaveChanges();
        }

#if !NET40

        /// <summary>
        ///     Saves all changes made in this context to the underlying database asynchronously.
        /// </summary>
        /// <returns> The number of objects written to the underlying database. </returns>
        /// <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        public Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }

        /// <summary>
        ///     An asynchronous version of SaveChanges, which
        ///     saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task that contains the number of objects written to the underlying database. </returns>
        /// <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return InternalContext.SaveChangesAsync(cancellationToken);
        }

#endif

        /// <summary>
        ///     Returns the Entity Framework ObjectContext that is underlying this context.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        ObjectContext IObjectContextAdapter.ObjectContext
        {
            get
            {
                // When dropping down to ObjectContext we force o-space loading for the types
                // that we know about so that code can use the ObjectContext with o-space metadata
                // without having to explicitly call LoadFromAssembly. For example Dynamic Data does
                // this--see Dev11 142609.
                InternalContext.ForceOSpaceLoadingForKnownEntityTypes();
                return InternalContext.ObjectContext;
            }
        }

        /// <summary>
        ///     Validates tracked entities and returns a Collection of <see cref="DbEntityValidationResult" /> containing validation results.
        /// </summary>
        /// <returns> Collection of validation results for invalid entities. The collection is never null and must not contain null values or results for valid entities. </returns>
        /// <remarks>
        ///     1. This method calls DetectChanges() to determine states of the tracked entities unless
        ///     DbContextConfiguration.AutoDetectChangesEnabled is set to false.
        ///     2. By default only Added on Modified entities are validated. The user is able to change this behavior
        ///     by overriding ShouldValidateEntity method.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<DbEntityValidationResult> GetValidationErrors()
        {
            var validationResults = new List<DbEntityValidationResult>();

            //ChangeTracker.Entries() will call DetectChanges unless disabled by the user
            foreach (var dbEntityEntry in ChangeTracker.Entries())
            {
#pragma warning disable 612,618
                if (dbEntityEntry.InternalEntry.EntityType != typeof(EdmMetadata)
                    &&
#pragma warning restore 612,618
                    ShouldValidateEntity(dbEntityEntry))
                {
                    var validationResult = ValidateEntity(dbEntityEntry, new Dictionary<object, object>());

                    if (validationResult != null
                        && !validationResult.IsValid)
                    {
                        validationResults.Add(validationResult);
                    }
                }
            }

            return validationResults;
        }

        /// <summary>
        ///     Extension point allowing the user to override the default behavior of validating only
        ///     added and modified entities.
        /// </summary>
        /// <param name="entityEntry"> DbEntityEntry instance that is supposed to be validated. </param>
        /// <returns> true to proceed with validation; false otherwise. </returns>
        protected virtual bool ShouldValidateEntity(DbEntityEntry entityEntry)
        {
            Check.NotNull(entityEntry, "entityEntry");

            return (entityEntry.State & (EntityState.Added | EntityState.Modified)) != 0;
        }

        /// <summary>
        ///     Extension point allowing the user to customize validation of an entity or filter out validation results.
        ///     Called by <see cref="GetValidationErrors" />.
        /// </summary>
        /// <param name="entityEntry"> DbEntityEntry instance to be validated. </param>
        /// <param name="items">
        ///     User-defined dictionary containing additional info for custom validation. It will be passed to
        ///     <see
        ///         cref="System.ComponentModel.DataAnnotations.ValidationContext" />
        ///     and will be exposed as
        ///     <see
        ///         cref="System.ComponentModel.DataAnnotations.ValidationContext.Items" />
        ///     . This parameter is optional and can be null.
        /// </param>
        /// <returns> Entity validation result. Possibly null when overridden. </returns>
        protected virtual DbEntityValidationResult ValidateEntity(
            DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            Check.NotNull(entityEntry, "entityEntry");

            return entityEntry.InternalEntry.GetValidationResult(items);
        }

        /// <summary>
        ///     Internal method that calls the protected ValidateEntity method.
        /// </summary>
        /// <param name="entityEntry"> DbEntityEntry instance to be validated. </param>
        /// <param name="items">
        ///     User-defined dictionary containing additional info for custom validation. It will be passed to
        ///     <see
        ///         cref="System.ComponentModel.DataAnnotations.ValidationContext" />
        ///     and will be exposed as
        ///     <see
        ///         cref="System.ComponentModel.DataAnnotations.ValidationContext.Items" />
        ///     . This parameter is optional and can be null.
        /// </param>
        /// <returns> Entity validation result. Possibly null when ValidateEntity is overridden. </returns>
        internal virtual DbEntityValidationResult CallValidateEntity(DbEntityEntry entityEntry)
        {
            return ValidateEntity(entityEntry, new Dictionary<object, object>());
        }

        #endregion

        #region Entity entries

        /// <summary>
        ///     Gets a <see cref="DbEntityEntry{T}" /> object for the given entity providing access to
        ///     information about the entity and the ability to perform actions on the entity.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity. </param>
        /// <returns> An entry for the entity. </returns>
        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            Check.NotNull(entity, "entity");

            return new DbEntityEntry<TEntity>(new InternalEntityEntry(InternalContext, entity));
        }

        /// <summary>
        ///     Gets a <see cref="DbEntityEntry" /> object for the given entity providing access to
        ///     information about the entity and the ability to perform actions on the entity.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> An entry for the entity. </returns>
        public DbEntityEntry Entry(object entity)
        {
            Check.NotNull(entity, "entity");

            return new DbEntityEntry(new InternalEntityEntry(InternalContext, entity));
        }

        #endregion

        #region ChangeTracker and Configuration

        /// <summary>
        ///     Provides access to features of the context that deal with change tracking of entities.
        /// </summary>
        /// <value> An object used to access features that deal with change tracking. </value>
        public DbChangeTracker ChangeTracker
        {
            get { return new DbChangeTracker(InternalContext); }
        }

        /// <summary>
        ///     Provides access to configuration options for the context.
        /// </summary>
        /// <value> An object used to access configuration options. </value>
        public DbContextConfiguration Configuration
        {
            get { return new DbContextConfiguration(InternalContext); }
        }

        #endregion

        #region Disposable

        /// <summary>
        ///     Calls the protected Dispose method.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);

            // This class has no unmanaged resources but it is possible that somebody could add some in a subclass.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes the context. The underlying <see cref="ObjectContext" /> is also disposed if it was created
        ///     is by this context or ownership was passed to this context when this context was created.
        ///     The connection to the database (<see cref="DbConnection" /> object) is also disposed if it was created
        ///     is by this context or ownership was passed to this context when this context was created.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            InternalContext.Dispose();
        }

        #endregion

        #region InternalContext access

        /// <summary>
        ///     Provides access to the underlying InternalContext for other parts of the internal design.
        /// </summary>
        internal virtual InternalContext InternalContext
        {
            get { return _internalContext; }
        }

        #endregion

        #region Hidden Object methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion
    }
}
