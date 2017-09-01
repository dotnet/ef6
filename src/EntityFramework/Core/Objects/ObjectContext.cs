// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Text;
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif
    using System.Transactions;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ObjectContext is the top-level object that encapsulates a connection between the CLR and the database,
    /// serving as a gateway for Create, Read, Update, and Delete operations.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class ObjectContext : IDisposable, IObjectContextAdapter
    {
        #region Fields

        private bool _disposed;
        private readonly IEntityAdapter _adapter;

        // Connection may be null if used by ObjectMaterializer for detached ObjectContext,
        // but those code paths should not touch the connection.
        //
        // If the connection is null, this indicates that this object has been disposed.
        // Disposal for this class doesn't mean complete disposal, 
        // but rather the disposal of the underlying connection object if the ObjectContext owns the connection,
        // or the separation of the underlying connection object from the ObjectContext if the ObjectContext does not own the connection.
        //
        // Operations that require a connection should throw an ObjectDiposedException if the connection is null.
        // Other operations that do not need a connection should continue to work after disposal.
        private EntityConnection _connection;

        private readonly MetadataWorkspace _workspace;
        private ObjectStateManager _objectStateManager;
        private ClrPerspective _perspective;
        private bool _contextOwnsConnection;
        private bool _openedConnection; // whether or not the context opened the connection to do an operation
        private int _connectionRequestCount; // the number of active requests for an open connection
        private int? _queryTimeout;
        private Transaction _lastTransaction;

        private readonly bool _disallowSettingDefaultContainerName;

        private EventHandler _onSavingChanges;

        private ObjectMaterializedEventHandler _onObjectMaterialized;

        private ObjectQueryProvider _queryProvider;

        private readonly EntityWrapperFactory _entityWrapperFactory;
        private readonly ObjectQueryExecutionPlanFactory _objectQueryExecutionPlanFactory;
        private readonly Translator _translator;
        private readonly ColumnMapFactory _columnMapFactory;

        private readonly ObjectContextOptions _options = new ObjectContextOptions();

        private const string UseLegacyPreserveChangesBehavior = "EntityFramework_UseLegacyPreserveChangesBehavior";

        private readonly ThrowingMonitor _asyncMonitor = new ThrowingMonitor();
        private DbInterceptionContext _interceptionContext;

        // Dictionary of types that derive from ObjectContext or DbContext that were already processed
        // in terms of retrieving the DbMappingViewCacheTypeAttribute that associates the context type 
        // with a mapping view cache type. InitializeMappingViewCacheFactory shortcuts the execution 
        // if the context type was already processed.
        private static readonly ConcurrentDictionary<Type, bool> _contextTypesWithViewCacheInitialized
            = new ConcurrentDictionary<Type, bool>();

        private TransactionHandler _transactionHandler;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> class with the given connection. During construction, the metadata workspace is extracted from the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnection" />
        /// object.
        /// </summary>
        /// <param name="connection">
        /// An <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnection" /> that contains references to the model and to the data source connection.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">The  connection  is null.</exception>
        /// <exception cref="T:System.ArgumentException">The  connection  is invalid or the metadata workspace is invalid. </exception>
        public ObjectContext(EntityConnection connection)
            : this(connection, true, null)
        {
            _contextOwnsConnection = false;
        }

        /// <summary>
        /// Creates an ObjectContext with the given connection and metadata workspace.
        /// </summary>
        /// <param name="connection"> connection to the store </param>
        /// <param name="contextOwnsConnection"> If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection. </param>
        public ObjectContext(EntityConnection connection, bool contextOwnsConnection)
            : this(connection, true, null)
        {
            _contextOwnsConnection = contextOwnsConnection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> class with the given connection string and default entity container name.
        /// </summary>
        /// <param name="connectionString">The connection string, which also provides access to the metadata information.</param>
        /// <exception cref="T:System.ArgumentNullException">The  connectionString  is null.</exception>
        /// <exception cref="T:System.ArgumentException">The  connectionString  is invalid or the metadata workspace is not valid. </exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For CreateEntityConnection method. But the paths are not created in this method.
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        public ObjectContext(string connectionString)
            : this(CreateEntityConnection(connectionString), false, null)
        {
            _contextOwnsConnection = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> class with a given connection string and entity container name.
        /// </summary>
        /// <param name="connectionString">The connection string, which also provides access to the metadata information.</param>
        /// <param name="defaultContainerName">The name of the default entity container. When the  defaultContainerName  is set through this method, the property becomes read-only.</param>
        /// <exception cref="T:System.ArgumentNullException">The  connectionString  is null.</exception>
        /// <exception cref="T:System.ArgumentException">The  connectionString ,  defaultContainerName , or metadata workspace is not valid.</exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For ObjectContext method. But the paths are not created in this method.
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Class is internal and methods are made virtual for testing purposes only. They cannot be overrided by user.")]
        protected ObjectContext(string connectionString, string defaultContainerName)
            : this(connectionString)
        {
            DefaultContainerName = defaultContainerName;
            if (!string.IsNullOrEmpty(defaultContainerName))
            {
                _disallowSettingDefaultContainerName = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> class with a given connection and entity container name.
        /// </summary>
        /// <param name="connection">
        /// An <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnection" /> that contains references to the model and to the data source connection.
        /// </param>
        /// <param name="defaultContainerName">The name of the default entity container. When the  defaultContainerName  is set through this method, the property becomes read-only.</param>
        /// <exception cref="T:System.ArgumentNullException">The  connection  is null.</exception>
        /// <exception cref="T:System.ArgumentException">The  connection ,  defaultContainerName , or metadata workspace is not valid.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Class is internal and methods are made virtual for testing purposes only. They cannot be overrided by user.")]
        protected ObjectContext(EntityConnection connection, string defaultContainerName)
            : this(connection)
        {
            DefaultContainerName = defaultContainerName;
            if (!string.IsNullOrEmpty(defaultContainerName))
            {
                _disallowSettingDefaultContainerName = true;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal ObjectContext(
            EntityConnection connection,
            bool isConnectionConstructor,
            ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory,
            Translator translator = null,
            ColumnMapFactory columnMapFactory = null)
        {
            Check.NotNull(connection, "connection");

            _interceptionContext = new DbInterceptionContext().WithObjectContext(this);

            _objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
            _translator = translator ?? new Translator();
            _columnMapFactory = columnMapFactory ?? new ColumnMapFactory();
            _adapter = new EntityAdapter(this);

            _connection = connection;
            _connection.AssociateContext(this);

            _connection.StateChange += ConnectionStateChange;
            _entityWrapperFactory = new EntityWrapperFactory();
            // Ensure a valid connection
            var connectionString = connection.ConnectionString;
            if (connectionString == null
                || connectionString.Trim().Length == 0)
            {
                throw isConnectionConstructor
                          ? new ArgumentException(Strings.ObjectContext_InvalidConnection, "connection", null)
                          : new ArgumentException(Strings.ObjectContext_InvalidConnectionString, "connectionString", null);
            }

            try
            {
                _workspace = RetrieveMetadataWorkspaceFromConnection();
            }
            catch (InvalidOperationException e)
            {
                // Intercept exceptions retrieving workspace, and wrap exception in appropriate
                // message based on which constructor pattern is being used.
                throw isConnectionConstructor
                          ? new ArgumentException(Strings.ObjectContext_InvalidConnection, "connection", e)
                          : new ArgumentException(Strings.ObjectContext_InvalidConnectionString, "connectionString", e);
            }

            Debug.Assert(_workspace != null);

            // load config file properties
            var value = ConfigurationManager.AppSettings[UseLegacyPreserveChangesBehavior];
            var useV35Behavior = false;
            if (Boolean.TryParse(value, out useV35Behavior))
            {
                ContextOptions.UseLegacyPreserveChangesBehavior = useV35Behavior;
            }

            InitializeMappingViewCacheFactory();
        }

        // <summary>
        // For testing purposes only.
        // </summary>
        internal ObjectContext(
            ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null,
            Translator translator = null,
            ColumnMapFactory columnMapFactory = null,
            IEntityAdapter adapter = null)
        {
            _interceptionContext = new DbInterceptionContext().WithObjectContext(this);

            _objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
            _translator = translator ?? new Translator();
            _columnMapFactory = columnMapFactory ?? new ColumnMapFactory();
            _adapter = adapter ?? new EntityAdapter(this);
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Gets the connection used by the object context.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbConnection" /> object that is the connection.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">
        /// When the <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> instance has been disposed.
        /// </exception>
        public virtual DbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    throw new ObjectDisposedException(null, Strings.ObjectContext_ObjectDisposed);
                }

                return _connection;
            }
        }

        /// <summary>Gets or sets the default container name.</summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that is the default container name.
        /// </returns>
        public virtual string DefaultContainerName
        {
            get
            {
                var container = Perspective.GetDefaultContainer();
                return ((null != container) ? container.Name : String.Empty);
            }
            set
            {
                if (!_disallowSettingDefaultContainerName)
                {
                    Perspective.SetDefaultContainer(value);
                }
                else
                {
                    throw new InvalidOperationException(Strings.ObjectContext_CannotSetDefaultContainerName);
                }
            }
        }

        /// <summary>Gets the metadata workspace used by the object context. </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" /> object associated with this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </returns>
        public virtual MetadataWorkspace MetadataWorkspace
        {
            get { return _workspace; }
        }

        /// <summary>Gets the object state manager used by the object context to track object changes.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" /> used by this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </returns>
        public virtual ObjectStateManager ObjectStateManager
        {
            get
            {
                if (_objectStateManager == null)
                {
                    _objectStateManager = new ObjectStateManager(_workspace);
                }

                return _objectStateManager;
            }
        }

        // <summary>
        // ContextOwnsConnection sets whether this context should dispose
        // its underlying EntityConnection.
        // </summary>
        internal bool ContextOwnsConnection
        {
            set
            {
                _contextOwnsConnection = value;
            }
        }

        // <summary>
        // ClrPerspective based on the MetadataWorkspace.
        // </summary>
        internal ClrPerspective Perspective
        {
            get
            {
                if (_perspective == null)
                {
                    _perspective = new ClrPerspective(MetadataWorkspace);
                }

                return _perspective;
            }
        }

        /// <summary>Gets or sets the timeout value, in seconds, for all object context operations. A null value indicates that the default value of the underlying provider will be used.</summary>
        /// <returns>
        /// An <see cref="T:System.Int32" /> value that is the timeout value, in seconds.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The timeout value is less than 0. </exception>
        public virtual int? CommandTimeout
        {
            get { return _queryTimeout; }
            set
            {
                if (value.HasValue
                    && value < 0)
                {
                    throw new ArgumentException(Strings.ObjectContext_InvalidCommandTimeout, "value");
                }

                _queryTimeout = value;
            }
        }

        /// <summary>Gets the LINQ query provider associated with this object context.</summary>
        /// <returns>
        /// The <see cref="T:System.Linq.IQueryProvider" /> instance used by this object context.
        /// </returns>
        protected internal virtual IQueryProvider QueryProvider
        {
            get
            {
                if (null == _queryProvider)
                {
                    _queryProvider = new ObjectQueryProvider(this);
                }

                return _queryProvider;
            }
        }

        // <summary>
        // Whether or not we are in the middle of materialization
        // Used to suppress operations such as lazy loading that are not allowed during materialization
        // </summary>
        internal bool InMaterialization { get; set; }

        // <summary>
        // Indicates whether there is an asynchronous method currently running that uses this instance
        // </summary>
        internal ThrowingMonitor AsyncMonitor
        {
            get { return _asyncMonitor; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Objects.ObjectContextOptions" /> instance that contains options that affect the behavior of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectContextOptions" /> instance that contains options that affect the behavior of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </returns>
        public virtual ObjectContextOptions ContextOptions
        {
            get { return _options; }
        }

        internal CollectionColumnMap ColumnMapBuilder { get; set; }

        internal virtual EntityWrapperFactory EntityWrapperFactory
        {
            get { return _entityWrapperFactory; }
        }

        /// <summary>
        /// Returns itself. ObjectContext implements <see cref="IObjectContextAdapter" /> to provide a common
        /// interface for <see cref="DbContext" /> and ObjectContext both of which will return the underlying
        /// ObjectContext.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        ObjectContext IObjectContextAdapter.ObjectContext
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the transaction handler in use by this context. May be null if no transaction have been started.
        /// </summary>
        /// <value>
        /// The transaction handler.
        /// </value>
        public TransactionHandler TransactionHandler
        {
            get
            {
                EnsureTransactionHandlerRegistered();

                return _transactionHandler;
            }
        }

        /// <summary>
        /// Returns the <see cref="DbInterceptionContext"/> being used for this context.
        /// </summary>
        public DbInterceptionContext InterceptionContext
        {
            get { return _interceptionContext; }
            internal set
            {
                DebugCheck.NotNull(_interceptionContext);
                Debug.Assert(_interceptionContext.ObjectContexts.Contains(this));

                _interceptionContext = value;
            }
        }

        #endregion //Properties

        #region Events

        /// <summary>Occurs when changes are saved to the data source. </summary>
        public event EventHandler SavingChanges
        {
            add { _onSavingChanges += value; }
            remove { _onSavingChanges -= value; }
        }

        // <summary>
        // A private helper function for the _savingChanges/SavingChanges event.
        // </summary>
        private void OnSavingChanges()
        {
            if (null != _onSavingChanges)
            {
                _onSavingChanges(this, new EventArgs());
            }
        }

        /// <summary>Occurs when a new entity object is created from data in the data source as part of a query or load operation. </summary>
        public event ObjectMaterializedEventHandler ObjectMaterialized
        {
            add { _onObjectMaterialized += value; }
            remove { _onObjectMaterialized -= value; }
        }

        internal void OnObjectMaterialized(object entity)
        {
            if (null != _onObjectMaterialized)
            {
                _onObjectMaterialized(this, new ObjectMaterializedEventArgs(entity));
            }
        }

        // <summary>
        // Returns true if any handlers for the ObjectMaterialized event exist.  This is
        // used for perf reasons to avoid collecting the information needed for the event
        // if there is no point in firing it.
        // </summary>
        internal bool OnMaterializedHasHandlers
        {
            get { return _onObjectMaterialized != null && _onObjectMaterialized.GetInvocationList().Length != 0; }
        }

        #endregion //Events

        #region Methods

        /// <summary>Accepts all changes made to objects in the object context.</summary>
        public virtual void AcceptAllChanges()
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();

            if (ObjectStateManager.SomeEntryWithConceptualNullExists())
            {
                throw new InvalidOperationException(Strings.ObjectContext_CommitWithConceptualNull);
            }

            // There are scenarios in which order of calling AcceptChanges does matter:
            // in case there is an entity in Deleted state and another entity in Added state with the same ID -
            // it is necessary to call AcceptChanges on Deleted entity before calling AcceptChanges on Added entity
            // (doing this in the other order there is conflict of keys).
            foreach (var entry in ObjectStateManager.GetObjectStateEntries(EntityState.Deleted))
            {
                entry.AcceptChanges();
            }

            foreach (var entry in ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified))
            {
                entry.AcceptChanges();
            }

            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        private void VerifyRootForAdd(
            bool doAttach, string entitySetName, IEntityWrapper wrappedEntity, EntityEntry existingEntry, out EntitySet entitySet,
            out bool isNoOperation)
        {
            isNoOperation = false;

            EntitySet entitySetFromName = null;

            if (doAttach)
            {
                // For AttachTo the entity set name is optional
                if (!String.IsNullOrEmpty(entitySetName))
                {
                    entitySetFromName = GetEntitySetFromName(entitySetName);
                }
            }
            else
            {
                // For AddObject the entity set name is obligatory
                entitySetFromName = GetEntitySetFromName(entitySetName);
            }

            // Find entity set using entity key
            EntitySet entitySetFromKey = null;

            var key = existingEntry != null ? existingEntry.EntityKey : wrappedEntity.GetEntityKeyFromEntity();
            if (null != (object)key)
            {
                entitySetFromKey = key.GetEntitySet(MetadataWorkspace);

                if (entitySetFromName != null)
                {
                    // both entity sets are not null, compare them
                    EntityUtil.ValidateEntitySetInKey(key, entitySetFromName, "entitySetName");
                }
                key.ValidateEntityKey(_workspace, entitySetFromKey);
            }

            entitySet = entitySetFromKey ?? entitySetFromName;

            // Check if entity set was found
            if (entitySet == null)
            {
                throw new InvalidOperationException(Strings.ObjectContext_EntitySetNameOrEntityKeyRequired);
            }

            ValidateEntitySet(entitySet, wrappedEntity.IdentityType);

            // If in the middle of Attach, try to find the entry by key
            if (doAttach && existingEntry == null)
            {
                // If we don't already have a key, create one now
                if (null == (object)key)
                {
                    key = ObjectStateManager.CreateEntityKey(entitySet, wrappedEntity.Entity);
                }
                existingEntry = ObjectStateManager.FindEntityEntry(key);
            }

            if (null != existingEntry
                && !(doAttach && existingEntry.IsKeyEntry))
            {
                if (!ReferenceEquals(existingEntry.Entity, wrappedEntity.Entity))
                {
                    throw new InvalidOperationException(
                        Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey(wrappedEntity.IdentityType.FullName));
                }
                else
                {
                    var exptectedState = doAttach ? EntityState.Unchanged : EntityState.Added;

                    if (existingEntry.State != exptectedState)
                    {
                        throw doAttach
                                  ? new InvalidOperationException(Strings.ObjectContext_EntityAlreadyExistsInObjectStateManager)
                                  : new InvalidOperationException(
                                        Strings.ObjectStateManager_DoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity(
                                            existingEntry.State));
                    }
                    else
                    {
                        // AttachTo:
                        // Attach is no-op when the existing entry is not a KeyEntry
                        // and it's entity is the same entity instance and it's state is Unchanged

                        // AddObject:
                        // AddObject is no-op when the existing entry's entity is the same entity 
                        // instance and it's state is Added
                        isNoOperation = true;
                        return;
                    }
                }
            }
        }

        /// <summary>Adds an object to the object context. </summary>
        /// <param name="entitySetName">Represents the entity set name, which may optionally be qualified by the entity container name. </param>
        /// <param name="entity">
        /// The <see cref="T:System.Object" /> to add.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">The  entity  parameter is null or the  entitySetName  does not qualify.</exception>
        public virtual void AddObject(string entitySetName, object entity)
        {
            Check.NotNull(entity, "entity");

            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            EntityEntry existingEntry;
            var wrappedEntity = EntityWrapperFactory.WrapEntityUsingContextGettingEntry(entity, this, out existingEntry);

            if (existingEntry == null)
            {
                // If the exact object being added is already in the context, there there is no way we need to
                // load the type for it, and since this is expensive, we only do the load if we have to.

                // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
                // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
                // We will auto-load the entity type's assembly into the ObjectItemCollection.
                // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
                MetadataWorkspace.ImplicitLoadAssemblyForType(wrappedEntity.IdentityType, null);
            }
            else
            {
                Debug.Assert(
                    existingEntry.Entity == entity, "FindEntityEntry should return null if existing entry contains a different object.");
            }

            EntitySet entitySet;
            bool isNoOperation;

            VerifyRootForAdd(false, entitySetName, wrappedEntity, existingEntry, out entitySet, out isNoOperation);
            if (isNoOperation)
            {
                return;
            }

            var transManager = ObjectStateManager.TransactionManager;
            transManager.BeginAddTracking();

            try
            {
                var relationshipManager = wrappedEntity.RelationshipManager;
                Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");

                var doCleanup = true;
                try
                {
                    // Add the root of the graph to the cache.
                    AddSingleObject(entitySet, wrappedEntity, "entity");
                    doCleanup = false;
                }
                finally
                {
                    // If we failed after adding the entry but before completely attaching the related ends to the context, we need to do some cleanup.
                    // If the context is null, we didn't even get as far as trying to attach the RelationshipManager, so something failed before the entry
                    // was even added, therefore there is nothing to clean up.
                    if (doCleanup && wrappedEntity.Context == this)
                    {
                        // If the context is not null, it be because the failure happened after it was attached, or it
                        // could mean that this entity was already attached, in which case we don't want to clean it up
                        // If we find the entity in the context and its key is temporary, we must have just added it, so remove it now.
                        var entry = ObjectStateManager.FindEntityEntry(wrappedEntity.Entity);
                        if (entry != null
                            && entry.EntityKey.IsTemporary)
                        {
                            // devnote: relationshipManager is valid, so entity must be IEntityWithRelationships and casting is safe
                            relationshipManager.NodeVisited = true;
                            // devnote: even though we haven't added the rest of the graph yet, we need to go through the related ends and
                            //          clean them up, because some of them could have been attached to the context before the failure occurred
                            RelationshipManager.RemoveRelatedEntitiesFromObjectStateManager(wrappedEntity);
                            RelatedEnd.RemoveEntityFromObjectStateManager(wrappedEntity);
                        }
                        // else entry was not added or the key is not temporary, so it must have already been in the cache before we tried to add this product, so don't remove anything
                    }
                }

                relationshipManager.AddRelatedEntitiesToObjectStateManager( /*doAttach*/false);
            }
            finally
            {
                transManager.EndAddTracking();
                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            }
        }

        // <summary>
        // Adds an object to the cache without adding its related
        // entities.
        // </summary>
        // <param name="entitySet"> EntitySet for the Object to be added. </param>
        // <param name="wrappedEntity"> Object to be added. </param>
        // <param name="argumentName"> Name of the argument passed to a public method, for use in exceptions. </param>
        internal void AddSingleObject(EntitySet entitySet, IEntityWrapper wrappedEntity, string argumentName)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(wrappedEntity);
            DebugCheck.NotNull(wrappedEntity.Entity);

            var key = wrappedEntity.GetEntityKeyFromEntity();
            if (null != (object)key)
            {
                EntityUtil.ValidateEntitySetInKey(key, entitySet);
                key.ValidateEntityKey(_workspace, entitySet);
            }

            VerifyContextForAddOrAttach(wrappedEntity);
            wrappedEntity.Context = this;
            var entry = ObjectStateManager.AddEntry(wrappedEntity, null, entitySet, argumentName, true);

            // If the entity supports relationships, AttachContext on the
            // RelationshipManager object - with load option of
            // AppendOnly (if adding a new object to a context, set
            // the relationships up to cache by default -- load option
            // is only set to other values when AttachContext is
            // called by the materializer). Also add all related entitites to
            // cache.
            //
            // NOTE: AttachContext must be called after adding the object to
            // the cache--otherwise the object might not have a key
            // when the EntityCollections expect it to.            
            Debug.Assert(
                ObjectStateManager.TransactionManager.TrackProcessedEntities, "Expected tracking processed entities to be true when adding.");
            Debug.Assert(ObjectStateManager.TransactionManager.ProcessedEntities != null, "Expected non-null collection when flag set.");

            ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);

            wrappedEntity.AttachContext(this, entitySet, MergeOption.AppendOnly);

            // Find PK values in referenced principals and use these to set FK values
            entry.FixupFKValuesFromNonAddedReferences();

            ObjectStateManager.FixupReferencesByForeignKeys(entry);
            wrappedEntity.TakeSnapshotOfRelationships(entry);
        }

        /// <summary>Explicitly loads an object related to the supplied object by the specified navigation property and using the default merge option. </summary>
        /// <param name="entity">The entity for which related objects are to be loaded.</param>
        /// <param name="navigationProperty">The name of the navigation property that returns the related objects to be loaded.</param>
        /// <exception cref="T:System.InvalidOperationException">
        /// The  entity  is in a <see cref="F:System.Data.Entity.EntityState.Detached" />,
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Added," />
        /// or <see cref="F:System.Data.Entity.EntityState.Deleted" /> state or the  entity  is attached to another instance of
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </exception>
        public virtual void LoadProperty(object entity, string navigationProperty)
        {
            var wrappedEntity = WrapEntityAndCheckContext(entity, "property");
            wrappedEntity.RelationshipManager.GetRelatedEnd(navigationProperty).Load();
        }

        /// <summary>Explicitly loads an object that is related to the supplied object by the specified navigation property and using the specified merge option. </summary>
        /// <param name="entity">The entity for which related objects are to be loaded.</param>
        /// <param name="navigationProperty">The name of the navigation property that returns the related objects to be loaded.</param>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> value to use when you load the related objects.
        /// </param>
        /// <exception cref="T:System.InvalidOperationException">
        /// The  entity  is in a <see cref="F:System.Data.Entity.EntityState.Detached" />,
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Added," />
        /// or <see cref="F:System.Data.Entity.EntityState.Deleted" /> state or the  entity  is attached to another instance of
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </exception>
        public virtual void LoadProperty(object entity, string navigationProperty, MergeOption mergeOption)
        {
            var wrappedEntity = WrapEntityAndCheckContext(entity, "property");
            wrappedEntity.RelationshipManager.GetRelatedEnd(navigationProperty).Load(mergeOption);
        }

        /// <summary>Explicitly loads an object that is related to the supplied object by the specified LINQ query and by using the default merge option. </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The source object for which related objects are to be loaded.</param>
        /// <param name="selector">A LINQ expression that defines the related objects to be loaded.</param>
        /// <exception cref="T:System.ArgumentException"> selector  does not supply a valid input parameter.</exception>
        /// <exception cref="T:System.ArgumentNullException"> selector  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The  entity  is in a <see cref="F:System.Data.Entity.EntityState.Detached" />,
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Added," />
        /// or <see cref="F:System.Data.Entity.EntityState.Deleted" /> state or the  entity  is attached to another instance of
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector)
        {
            // We used to throw an ArgumentException if the expression contained a Convert.  Now we remove the convert,
            // but if we still need to throw, then we should still throw an ArgumentException to avoid a breaking change.
            // Therefore, we keep track of whether or not we removed the convert.
            bool removedConvert;
            var navProp = ParsePropertySelectorExpression(selector, out removedConvert);
            var wrappedEntity = WrapEntityAndCheckContext(entity, "property");
            wrappedEntity.RelationshipManager.GetRelatedEnd(navProp, throwArgumentException: removedConvert).Load();
        }

        /// <summary>Explicitly loads an object that is related to the supplied object by the specified LINQ query and by using the specified merge option. </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The source object for which related objects are to be loaded.</param>
        /// <param name="selector">A LINQ expression that defines the related objects to be loaded.</param>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> value to use when you load the related objects.
        /// </param>
        /// <exception cref="T:System.ArgumentException"> selector  does not supply a valid input parameter.</exception>
        /// <exception cref="T:System.ArgumentNullException"> selector  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The  entity  is in a <see cref="F:System.Data.Entity.EntityState.Detached" />,
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Added," />
        /// or <see cref="F:System.Data.Entity.EntityState.Deleted" /> state or the  entity  is attached to another instance of
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector, MergeOption mergeOption)
        {
            // We used to throw an ArgumentException if the expression contained a Convert.  Now we remove the convert,
            // but if we still need to throw, then we should still throw an ArgumentException to avoid a breaking change.
            // Therefore, we keep track of whether or not we removed the convert.
            bool removedConvert;
            var navProp = ParsePropertySelectorExpression(selector, out removedConvert);
            var wrappedEntity = WrapEntityAndCheckContext(entity, "property");
            wrappedEntity.RelationshipManager.GetRelatedEnd(navProp, throwArgumentException: removedConvert).Load(mergeOption);
        }

        // Wraps the given entity and checks that it has a non-null context (i.e. that is is not detached).
        private IEntityWrapper WrapEntityAndCheckContext(object entity, string refType)
        {
            var wrappedEntity = EntityWrapperFactory.WrapEntityUsingContext(entity, this);
            if (wrappedEntity.Context == null)
            {
                throw new InvalidOperationException(Strings.ObjectContext_CannotExplicitlyLoadDetachedRelationships(refType));
            }

            if (wrappedEntity.Context
                != this)
            {
                throw new InvalidOperationException(Strings.ObjectContext_CannotLoadReferencesUsingDifferentContext(refType));
            }

            return wrappedEntity;
        }

        // Validates that the given property selector may represent a navigation property and returns the nav prop string.
        // The actual check that the navigation property is valid is performed by the
        // RelationshipManager while loading the RelatedEnd.
        internal static string ParsePropertySelectorExpression<TEntity>(Expression<Func<TEntity, object>> selector, out bool removedConvert)
        {
            Check.NotNull(selector, "selector");

            // We used to throw an ArgumentException if the expression contained a Convert.  Now we remove the convert,
            // but if we still need to throw, then we should still throw an ArgumentException to avoid a breaking change.
            // Therefore, we keep track of whether or not we removed the convert.
            removedConvert = false;
            var body = selector.Body;
            while (body.NodeType == ExpressionType.Convert
                   || body.NodeType == ExpressionType.ConvertChecked)
            {
                removedConvert = true;
                body = ((UnaryExpression)body).Operand;
            }

            var bodyAsMember = body as MemberExpression;
            if (bodyAsMember == null
                || !bodyAsMember.Member.DeclaringType.IsAssignableFrom(typeof(TEntity))
                || bodyAsMember.Expression.NodeType != ExpressionType.Parameter)
            {
                throw new ArgumentException(Strings.ObjectContext_SelectorExpressionMustBeMemberAccess);
            }

            return bodyAsMember.Member.Name;
        }

        /// <summary>Applies property changes from a detached object to an object already attached to the object context.</summary>
        /// <param name="entitySetName">The name of the entity set to which the object belongs.</param>
        /// <param name="changed">The detached object that has property updates to apply to the original object.</param>
        /// <exception cref="T:System.ArgumentNullException">When  entitySetName  is null or an empty string or when  changed  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// When the <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> from  entitySetName  does not match the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />
        /// of the object
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// or when the entity is in a state other than
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Modified" />
        /// or
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Unchanged" />
        /// or the original object is not attached to the context.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">When the type of the  changed  object is not the same type as the original object.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [Obsolete("Use ApplyCurrentValues instead")]
        public virtual void ApplyPropertyChanges(string entitySetName, object changed)
        {
            Check.NotNull(changed, "changed");
            Check.NotEmpty(entitySetName, "entitySetName");

            ApplyCurrentValues(entitySetName, changed);
        }

        /// <summary>
        /// Copies the scalar values from the supplied object into the object in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// that has the same key.
        /// </summary>
        /// <returns>The updated object.</returns>
        /// <param name="entitySetName">The name of the entity set to which the object belongs.</param>
        /// <param name="currentEntity">
        /// The detached object that has property updates to apply to the original object. The entity key of  currentEntity  must match the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Objects.ObjectStateEntry.EntityKey" />
        /// property of an entry in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </param>
        /// <typeparam name="TEntity">The entity type of the object.</typeparam>
        /// <exception cref="T:System.ArgumentNullException"> entitySetName  or  current  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> from  entitySetName  does not match the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />
        /// of the object
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        ///  or the object is not in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" />
        /// or it is in a
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Detached" />
        /// state or the entity key of the supplied object is invalid.
        /// </exception>
        /// <exception cref="T:System.ArgumentException"> entitySetName  is an empty string.</exception>
        public virtual TEntity ApplyCurrentValues<TEntity>(string entitySetName, TEntity currentEntity) where TEntity : class
        {
            Check.NotNull(currentEntity, "currentEntity");
            Check.NotEmpty(entitySetName, "entitySetName");

            var wrappedEntity = EntityWrapperFactory.WrapEntityUsingContext(currentEntity, this);

            // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // We will auto-load the entity type's assembly into the ObjectItemCollection.
            // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
            MetadataWorkspace.ImplicitLoadAssemblyForType(wrappedEntity.IdentityType, null);

            var entitySet = GetEntitySetFromName(entitySetName);

            var key = wrappedEntity.EntityKey;
            if (null != (object)key)
            {
                EntityUtil.ValidateEntitySetInKey(key, entitySet, "entitySetName");
                key.ValidateEntityKey(_workspace, entitySet);
            }
            else
            {
                key = ObjectStateManager.CreateEntityKey(entitySet, currentEntity);
            }

            // Check if entity is already in the cache
            var entityEntry = ObjectStateManager.FindEntityEntry(key);
            if (entityEntry == null
                || entityEntry.IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_EntityNotTracked);
            }

            entityEntry.ApplyCurrentValuesInternal(wrappedEntity);

            return (TEntity)entityEntry.Entity;
        }

        /// <summary>
        /// Copies the scalar values from the supplied object into set of original values for the object in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// that has the same key.
        /// </summary>
        /// <returns>The updated object.</returns>
        /// <param name="entitySetName">The name of the entity set to which the object belongs.</param>
        /// <param name="originalEntity">
        /// The detached object that has original values to apply to the object. The entity key of  originalEntity  must match the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Objects.ObjectStateEntry.EntityKey" />
        /// property of an entry in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </param>
        /// <typeparam name="TEntity">The type of the entity object.</typeparam>
        /// <exception cref="T:System.ArgumentNullException"> entitySetName  or  original  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> from  entitySetName  does not match the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />
        /// of the object
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        ///  or an
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" />
        /// for the object cannot be found in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" />
        ///  or the object is in an
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Added" />
        /// or a
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Detached" />
        /// state  or the entity key of the supplied object is invalid or has property changes.
        /// </exception>
        /// <exception cref="T:System.ArgumentException"> entitySetName  is an empty string.</exception>
        public virtual TEntity ApplyOriginalValues<TEntity>(string entitySetName, TEntity originalEntity) where TEntity : class
        {
            Check.NotNull(originalEntity, "originalEntity");

            Check.NotEmpty(entitySetName, "entitySetName");
            var wrappedOriginalEntity = EntityWrapperFactory.WrapEntityUsingContext(originalEntity, this);

            // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // We will auto-load the entity type's assembly into the ObjectItemCollection.
            // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
            MetadataWorkspace.ImplicitLoadAssemblyForType(wrappedOriginalEntity.IdentityType, null);

            var entitySet = GetEntitySetFromName(entitySetName);

            var key = wrappedOriginalEntity.EntityKey;
            if (null != (object)key)
            {
                EntityUtil.ValidateEntitySetInKey(key, entitySet, "entitySetName");
                key.ValidateEntityKey(_workspace, entitySet);
            }
            else
            {
                key = ObjectStateManager.CreateEntityKey(entitySet, originalEntity);
            }

            // Check if the entity is already in the cache
            var entityEntry = ObjectStateManager.FindEntityEntry(key);
            if (entityEntry == null
                || entityEntry.IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectContext_EntityNotTrackedOrHasTempKey);
            }

            if (entityEntry.State != EntityState.Modified
                && entityEntry.State != EntityState.Unchanged
                && entityEntry.State != EntityState.Deleted)
            {
                throw new InvalidOperationException(
                    Strings.ObjectContext_EntityMustBeUnchangedOrModifiedOrDeleted(entityEntry.State.ToString()));
            }

            if (entityEntry.WrappedEntity.IdentityType
                != wrappedOriginalEntity.IdentityType)
            {
                throw new ArgumentException(
                    Strings.ObjectContext_EntitiesHaveDifferentType(
                        entityEntry.Entity.GetType().FullName, originalEntity.GetType().FullName));
            }

            entityEntry.CompareKeyProperties(originalEntity);

            // The ObjectStateEntry.UpdateModifiedFields uses a variation of Shaper.UpdateRecord method 
            // which additionaly marks properties as modified as necessary.
            entityEntry.UpdateOriginalValues(wrappedOriginalEntity.Entity);

            // return the current entity
            return (TEntity)entityEntry.Entity;
        }

        /// <summary>Attaches an object or object graph to the object context in a specific entity set. </summary>
        /// <param name="entitySetName">Represents the entity set name, which may optionally be qualified by the entity container name. </param>
        /// <param name="entity">
        /// The <see cref="T:System.Object" /> to attach.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">The  entity  is null. </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// Invalid entity set  or the object has a temporary key or the object has an
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// and the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />
        /// does not match with the entity set passed in as an argument of the method or the object does not have an
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// and no entity set is provided or any object from the object graph has a temporary
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        ///  or any object from the object graph has an invalid
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// (for example, values in the key do not match values in the object) or the entity set could not be found from a given  entitySetName  name and entity container name or any object from the object graph already exists in another state manager.
        /// </exception>
        public virtual void AttachTo(string entitySetName, object entity)
        {
            Check.NotNull(entity, "entity");

            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();

            EntityEntry existingEntry;
            var wrappedEntity = EntityWrapperFactory.WrapEntityUsingContextGettingEntry(entity, this, out existingEntry);

            if (existingEntry == null)
            {
                // If the exact object being added is already in the context, there there is no way we need to
                // load the type for it, and since this is expensive, we only do the load if we have to.

                // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
                // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
                // We will auto-load the entity type's assembly into the ObjectItemCollection.
                // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
                MetadataWorkspace.ImplicitLoadAssemblyForType(wrappedEntity.IdentityType, null);
            }
            else
            {
                Debug.Assert(
                    existingEntry.Entity == entity, "FindEntityEntry should return null if existing entry contains a different object.");
            }

            EntitySet entitySet;
            bool isNoOperation;

            VerifyRootForAdd(true, entitySetName, wrappedEntity, existingEntry, out entitySet, out isNoOperation);
            if (isNoOperation)
            {
                return;
            }

            var transManager = ObjectStateManager.TransactionManager;
            transManager.BeginAttachTracking();

            try
            {
                ObjectStateManager.TransactionManager.OriginalMergeOption = wrappedEntity.MergeOption;
                var relationshipManager = wrappedEntity.RelationshipManager;
                Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");

                var doCleanup = true;
                try
                {
                    // Attach the root of entity graph to the cache.
                    AttachSingleObject(wrappedEntity, entitySet);
                    doCleanup = false;
                }
                finally
                {
                    // SQLBU 555615 Be sure that wrappedEntity.Context == this to not try to detach 
                    // entity from context if it was already attached to some other context.
                    // It's enough to check this only for the root of the graph since we can assume that all entities
                    // in the graph are attached to the same context (or none of them is attached).
                    if (doCleanup && wrappedEntity.Context == this)
                    {
                        // SQLBU 509900 RIConstraints: Entity still exists in cache after Attach fails
                        //
                        // Cleaning up is needed only when root of the graph violates some referential constraint.
                        // Normal cleaning is done in RelationshipManager.AddRelatedEntitiesToObjectStateManager()
                        // (referential constraints properties are checked in AttachSingleObject(), before
                        // AddRelatedEntitiesToObjectStateManager is called, that's why normal cleaning
                        // doesn't work in this case)

                        relationshipManager.NodeVisited = true;
                        // devnote: even though we haven't attached the rest of the graph yet, we need to go through the related ends and
                        //          clean them up, because some of them could have been attached to the context.
                        RelationshipManager.RemoveRelatedEntitiesFromObjectStateManager(wrappedEntity);
                        RelatedEnd.RemoveEntityFromObjectStateManager(wrappedEntity);
                    }
                }
                relationshipManager.AddRelatedEntitiesToObjectStateManager( /*doAttach*/true);
            }
            finally
            {
                transManager.EndAttachTracking();
                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            }
        }

        /// <summary>Attaches an object or object graph to the object context when the object has an entity key. </summary>
        /// <param name="entity">The object to attach.</param>
        /// <exception cref="T:System.ArgumentNullException">The  entity  is null. </exception>
        /// <exception cref="T:System.InvalidOperationException">Invalid entity key. </exception>
        public virtual void Attach(IEntityWithKey entity)
        {
            Check.NotNull(entity, "entity");

            if (null == (object)entity.EntityKey)
            {
                throw new InvalidOperationException(Strings.ObjectContext_CannotAttachEntityWithoutKey);
            }

            AttachTo(null, entity);
        }

        // <summary>
        // Attaches single object to the cache without adding its related entities.
        // </summary>
        // <param name="wrappedEntity"> Entity to be attached. </param>
        // <param name="entitySet"> "Computed" entity set. </param>
        internal void AttachSingleObject(IEntityWrapper wrappedEntity, EntitySet entitySet)
        {
            DebugCheck.NotNull(wrappedEntity);
            DebugCheck.NotNull(wrappedEntity.Entity);
            DebugCheck.NotNull(entitySet);

            // Try to detect if the entity is invalid as soon as possible
            // (before adding the entity to the ObjectStateManager)
            var relationshipManager = wrappedEntity.RelationshipManager;
            Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");

            var key = wrappedEntity.GetEntityKeyFromEntity();
            if (null != (object)key)
            {
                EntityUtil.ValidateEntitySetInKey(key, entitySet);
                key.ValidateEntityKey(_workspace, entitySet);
            }
            else
            {
                key = ObjectStateManager.CreateEntityKey(entitySet, wrappedEntity.Entity);
            }

            Debug.Assert(key != null, "GetEntityKey should have returned a non-null key");

            // Temporary keys are not allowed
            if (key.IsTemporary)
            {
                throw new InvalidOperationException(Strings.ObjectContext_CannotAttachEntityWithTemporaryKey);
            }

            if (wrappedEntity.EntityKey != key)
            {
                wrappedEntity.EntityKey = key;
            }

            // Check if entity already exists in the cache.
            // NOTE: This check could be done earlier, but this way I avoid creating key twice.
            var entry = ObjectStateManager.FindEntityEntry(key);

            if (null != entry)
            {
                if (entry.IsKeyEntry)
                {
                    // devnote: SQLBU 555615. This method was extracted from PromoteKeyEntry to have consistent
                    // behavior of AttachTo in case of attaching entity which is already attached to some other context.
                    // We can not detect if entity is attached to another context until we call SetChangeTrackerOntoEntity
                    // which throws exception if the change tracker is already set.  
                    // SetChangeTrackerOntoEntity is now called from PromoteKeyEntryInitialization(). 
                    // Calling PromoteKeyEntryInitialization() before calling relationshipManager.AttachContext prevents
                    // overriding Context property on relationshipManager (and attaching relatedEnds to current context).
                    ObjectStateManager.PromoteKeyEntryInitialization(this, entry, wrappedEntity, replacingEntry: false);

                    Debug.Assert(
                        ObjectStateManager.TransactionManager.TrackProcessedEntities,
                        "Expected tracking processed entities to be true when adding.");
                    Debug.Assert(
                        ObjectStateManager.TransactionManager.ProcessedEntities != null, "Expected non-null collection when flag set.");

                    ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);

                    wrappedEntity.TakeSnapshotOfRelationships(entry);

                    ObjectStateManager.PromoteKeyEntry(
                        entry,
                        wrappedEntity,
                        replacingEntry: false,
                        setIsLoaded: false,
                        keyEntryInitialized: true);

                    ObjectStateManager.FixupReferencesByForeignKeys(entry);

                    relationshipManager.CheckReferentialConstraintProperties(entry);
                }
                else
                {
                    Debug.Assert(!ReferenceEquals(entry.Entity, wrappedEntity.Entity));
                    throw new InvalidOperationException(
                        Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey(wrappedEntity.IdentityType.FullName));
                }
            }
            else
            {
                VerifyContextForAddOrAttach(wrappedEntity);
                wrappedEntity.Context = this;
                entry = ObjectStateManager.AttachEntry(key, wrappedEntity, entitySet);

                Debug.Assert(
                    ObjectStateManager.TransactionManager.TrackProcessedEntities,
                    "Expected tracking processed entities to be true when adding.");
                Debug.Assert(ObjectStateManager.TransactionManager.ProcessedEntities != null, "Expected non-null collection when flag set.");

                ObjectStateManager.TransactionManager.ProcessedEntities.Add(wrappedEntity);

                wrappedEntity.AttachContext(this, entitySet, MergeOption.AppendOnly);

                ObjectStateManager.FixupReferencesByForeignKeys(entry);
                wrappedEntity.TakeSnapshotOfRelationships(entry);

                relationshipManager.CheckReferentialConstraintProperties(entry);
            }
        }

        // <summary>
        // When attaching we need to check that the entity is not already attached to a different context
        // before we wipe away that context.
        // </summary>
        private void VerifyContextForAddOrAttach(IEntityWrapper wrappedEntity)
        {
            if (wrappedEntity.Context != null
                && wrappedEntity.Context != this
                && !wrappedEntity.Context.ObjectStateManager.IsDisposed
                && wrappedEntity.MergeOption != MergeOption.NoTracking)
            {
                throw new InvalidOperationException(Strings.Entity_EntityCantHaveMultipleChangeTrackers);
            }
        }

        /// <summary>Creates the entity key for a specific object, or returns the entity key if it already exists. </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.EntityKey" /> of the object.
        /// </returns>
        /// <param name="entitySetName">The fully qualified name of the entity set to which the entity object belongs.</param>
        /// <param name="entity">The object for which the entity key is being retrieved. </param>
        /// <exception cref="T:System.ArgumentNullException">When either parameter is null. </exception>
        /// <exception cref="T:System.ArgumentException">When  entitySetName  is empty or when the type of the  entity  object does not exist in the entity set or when the  entitySetName  is not fully qualified.</exception>
        /// <exception cref="T:System.InvalidOperationException">When the entity key cannot be constructed successfully based on the supplied parameters.</exception>
        public virtual EntityKey CreateEntityKey(string entitySetName, object entity)
        {
            Check.NotNull(entity, "entity");
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            Check.NotEmpty(entitySetName, "entitySetName");

            // SQLBUDT 480919: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // We will auto-load the entity type's assembly into the ObjectItemCollection.
            // We don't need the user's calling assembly for LoadAssemblyForType since entityType is sufficient.
            MetadataWorkspace.ImplicitLoadAssemblyForType(EntityUtil.GetEntityIdentityType(entity.GetType()), null);

            var entitySet = GetEntitySetFromName(entitySetName);

            return ObjectStateManager.CreateEntityKey(entitySet, entity);
        }

        internal EntitySet GetEntitySetFromName(string entitySetName)
        {
            string setName;
            string containerName;

            GetEntitySetName(entitySetName, "entitySetName", this, out setName, out containerName);

            // Find entity set using entitySetName and entityContainerName
            return GetEntitySet(setName, containerName);
        }

        private void AddRefreshKey(
            object entityLike, Dictionary<EntityKey, EntityEntry> entities, Dictionary<EntitySet, List<EntityKey>> currentKeys)
        {
            Debug.Assert(!(entityLike is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            if (null == entityLike)
            {
                throw new InvalidOperationException(Strings.ObjectContext_NthElementIsNull(entities.Count));
            }

            var wrappedEntity = EntityWrapperFactory.WrapEntityUsingContext(entityLike, this);
            var key = wrappedEntity.EntityKey;
            RefreshCheck(entities, key);

            // Retrieve the EntitySet for the EntityKey and add an entry in the dictionary
            // that maps a set to the keys of entities that should be refreshed from that set.
            var entitySet = key.GetEntitySet(MetadataWorkspace);

            List<EntityKey> setKeys = null;
            if (!currentKeys.TryGetValue(entitySet, out setKeys))
            {
                setKeys = new List<EntityKey>();
                currentKeys.Add(entitySet, setKeys);
            }

            setKeys.Add(key);
        }

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Core.Objects.ObjectSet`1" /> instance that is used to query, add, modify, and delete objects of the specified entity type.
        /// </summary>
        /// <returns>
        /// The new <see cref="T:System.Data.Entity.Core.Objects.ObjectSet`1" /> instance.
        /// </returns>
        /// <typeparam name="TEntity">
        /// Entity type of the requested <see cref="T:System.Data.Entity.Core.Objects.ObjectSet`1" />.
        /// </typeparam>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <see cref="P:System.Data.Entity.Core.Objects.ObjectContext.DefaultContainerName" /> property is not set on the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        ///  or the specified type belongs to more than one entity set.
        /// </exception>
        public virtual ObjectSet<TEntity> CreateObjectSet<TEntity>()
            where TEntity : class
        {
            var entitySet = GetEntitySetForType(typeof(TEntity), "TEntity");
            return new ObjectSet<TEntity>(entitySet, this);
        }

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Core.Objects.ObjectSet`1" /> instance that is used to query, add, modify, and delete objects of the specified type and with the specified entity set name.
        /// </summary>
        /// <returns>
        /// The new <see cref="T:System.Data.Entity.Core.Objects.ObjectSet`1" /> instance.
        /// </returns>
        /// <param name="entitySetName">
        /// Name of the entity set for the returned <see cref="T:System.Data.Entity.Core.Objects.ObjectSet`1" />. The string must be qualified by the default container name if the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Objects.ObjectContext.DefaultContainerName" />
        /// property is not set on the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </param>
        /// <typeparam name="TEntity">
        /// Entity type of the requested <see cref="T:System.Data.Entity.Core.Objects.ObjectSet`1" />.
        /// </typeparam>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> from  entitySetName  does not match the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />
        /// of the object
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        ///  or the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Objects.ObjectContext.DefaultContainerName" />
        /// property is not set on the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// and the name is not qualified as part of the  entitySetName  parameter or the specified type belongs to more than one entity set.
        /// </exception>
        public virtual ObjectSet<TEntity> CreateObjectSet<TEntity>(string entitySetName)
            where TEntity : class
        {
            var entitySet = GetEntitySetForNameAndType(entitySetName, typeof(TEntity), "TEntity");
            return new ObjectSet<TEntity>(entitySet, this);
        }

        // <summary>
        // Find the EntitySet in the default EntityContainer for the specified CLR type.
        // Must be a valid mapped entity type and must be mapped to exactly one EntitySet across all of the EntityContainers in the metadata for this context.
        // </summary>
        // <param name="entityCLRType"> CLR type to use for EntitySet lookup. </param>
        private EntitySet GetEntitySetForType(Type entityCLRType, string exceptionParameterName)
        {
            EntitySet entitySetForType = null;

            var defaultContainer = Perspective.GetDefaultContainer();
            if (defaultContainer == null)
            {
                // We don't have a default container, so look through all EntityContainers in metadata to see if
                // we can find exactly one EntitySet that matches the specified CLR type.
                var entityContainers = MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace);
                foreach (var entityContainer in entityContainers)
                {
                    // See if this container has exactly one EntitySet for this type
                    var entitySetFromContainer = GetEntitySetFromContainer(entityContainer, entityCLRType, exceptionParameterName);

                    if (entitySetFromContainer != null)
                    {
                        // Verify we haven't already found a matching EntitySet in some other container
                        if (entitySetForType != null)
                        {
                            // There is more than one EntitySet for this type across all containers in metadata, so we can't determine which one the user intended
                            throw new ArgumentException(
                                Strings.ObjectContext_MultipleEntitySetsFoundInAllContainers(entityCLRType.FullName), exceptionParameterName);
                        }

                        entitySetForType = entitySetFromContainer;
                    }
                }
            }
            else
            {
                // There is a default container, so restrict the search to EntitySets within it
                entitySetForType = GetEntitySetFromContainer(defaultContainer, entityCLRType, exceptionParameterName);
            }

            // We still may not have found a matching EntitySet for this type
            if (entitySetForType == null)
            {
                throw new ArgumentException(Strings.ObjectContext_NoEntitySetFoundForType(entityCLRType.FullName), exceptionParameterName);
            }

            return entitySetForType;
        }

        private EntitySet GetEntitySetFromContainer(EntityContainer container, Type entityCLRType, string exceptionParameterName)
        {
            // Verify that we have an EdmType mapping for the specified CLR type
            var entityEdmType = GetTypeUsage(entityCLRType).EdmType;

            // Try to find a single EntitySet for the specified type
            EntitySet entitySet = null;
            foreach (var es in container.BaseEntitySets)
            {
                // This is a match if the set is an EntitySet (not an AssociationSet) and the EntitySet
                // is defined for the specified entity type. Must be an exact match, not a base type. 
                if (es.BuiltInTypeKind == BuiltInTypeKind.EntitySet
                    && es.ElementType == entityEdmType)
                {
                    if (entitySet != null)
                    {
                        // There is more than one EntitySet for this type, so we can't determine which one the user intended
                        throw new ArgumentException(
                            Strings.ObjectContext_MultipleEntitySetsFoundInSingleContainer(entityCLRType.FullName, container.Name),
                            exceptionParameterName);
                    }

                    entitySet = (EntitySet)es;
                }
            }

            return entitySet;
        }

        // <summary>
        // Finds an EntitySet with the specified name and verifies that its type matches the specified type.
        // </summary>
        // <param name="entitySetName"> Name of the EntitySet to find. Can be fully-qualified or unqualified if the DefaultContainerName is set </param>
        // <param name="entityCLRType"> Expected CLR type of the EntitySet. Must exactly match the type for the EntitySet, base types are not valid. </param>
        // <param name="exceptionParameterName"> Argument name to use if an exception occurs. </param>
        // <returns> EntitySet that was found in metadata with the specified parameters </returns>
        private EntitySet GetEntitySetForNameAndType(string entitySetName, Type entityCLRType, string exceptionParameterName)
        {
            // Verify that the specified entitySetName exists in metadata
            var entitySet = GetEntitySetFromName(entitySetName);

            // Verify that the EntitySet type matches the specified type exactly (a base type is not valid)
            var entityEdmType = GetTypeUsage(entityCLRType).EdmType;
            if (entitySet.ElementType != entityEdmType)
            {
                throw new ArgumentException(
                    Strings.ObjectContext_InvalidObjectSetTypeForEntitySet(
                        entityCLRType.FullName, entitySet.ElementType.FullName, entitySetName), exceptionParameterName);
            }

            return entitySet;
        }

        #region Connection Management

        // <summary>
        // Ensures that the connection is opened for an operation that requires an open connection to the store.
        // Calls to EnsureConnection MUST be matched with a single call to ReleaseConnection.
        // </summary>
        // <param name="shouldMonitorTransactions"> Whether there will be a transaction started on the connection that should be monitored. </param>
        // <exception cref="ObjectDisposedException">
        // If the <see cref="ObjectContext" /> instance has been disposed.
        // </exception>
        internal virtual void EnsureConnection(bool shouldMonitorTransactions)
        {
            if (shouldMonitorTransactions)
            {
                EnsureTransactionHandlerRegistered();
            }

            if (Connection.State == ConnectionState.Broken)
            {
                Connection.Close();
            }

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
                _openedConnection = true;
            }

            if (_openedConnection)
            {
                _connectionRequestCount++;
            }

            try
            {
                var currentTransaction = Transaction.Current;

                EnsureContextIsEnlistedInCurrentTransaction(
                    currentTransaction,
                    () =>
                    {
                        Connection.Open();
                        Debug.Assert(_openedConnection);
                        return true;
                    },
                    false);

                // If we get here, we have an open connection, either enlisted in the current
                // transaction (if it's non-null) or unenlisted from all transactions (if the
                // current transaction is null)
                _lastTransaction = currentTransaction;
            }
            catch (Exception)
            {
                // when the connection is unable to enlist properly or another error occured, be sure to release this connection
                ReleaseConnection();
                throw;
            }
        }

#if !NET40

        // <summary>
        // Ensures that the connection is opened for an operation that requires an open connection to the store.
        // Calls to EnsureConnection MUST be matched with a single call to ReleaseConnection.
        // </summary>
        // <param name="shouldMonitorTransactions"> Whether there will be a transaction started on the connection that should be monitored. </param>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <exception cref="ObjectDisposedException">
        // If the <see cref="ObjectContext" /> instance has been disposed.
        // </exception>
        internal virtual async Task EnsureConnectionAsync(bool shouldMonitorTransactions, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (shouldMonitorTransactions)
            {
                EnsureTransactionHandlerRegistered();
            }

            if (Connection.State == ConnectionState.Broken)
            {
                Connection.Close();
            }

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).WithCurrentCulture();
                _openedConnection = true;
            }

            if (_openedConnection)
            {
                _connectionRequestCount++;
            }

            try
            {
                var currentTransaction = Transaction.Current;

                await EnsureContextIsEnlistedInCurrentTransaction(
                    currentTransaction, async () =>
                    {
                        await Connection.OpenAsync(cancellationToken).WithCurrentCulture();
                        Debug.Assert(_openedConnection);
                        return true;
                    },
                    Task.FromResult(false)).WithCurrentCulture();

                // If we get here, we have an open connection, either enlisted in the current
                // transaction (if it's non-null) or unenlisted from all transactions (if the
                // current transaction is null)
                _lastTransaction = currentTransaction;
            }
            catch (Exception)
            {
                // when the connection is unable to enlist properly or another error occured, be sure to release this connection
                ReleaseConnection();
                throw;
            }
        }

#endif

        private void EnsureTransactionHandlerRegistered()
        {
            if (_transactionHandler == null
                && !InterceptionContext.DbContexts.Any(dbc => dbc is TransactionContext))
            {
                var storeMetadata = (StoreItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.SSpace);

                var providerInvariantName =
                    DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(storeMetadata.ProviderFactory).Name;

                var transactionHandlerFactory = DbConfiguration.DependencyResolver.GetService<Func<TransactionHandler>>(
                    new ExecutionStrategyKey(providerInvariantName, Connection.DataSource));

                if (transactionHandlerFactory != null)
                {
                    _transactionHandler = transactionHandlerFactory();
                    _transactionHandler.Initialize(this);
                }
            }
        }

        private T EnsureContextIsEnlistedInCurrentTransaction<T>(Transaction currentTransaction, Func<T> openConnection, T defaultValue)
        {
            if (Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.BadConnectionWrapping);
            }

            // IF YOU MODIFIED THIS TABLE YOU MUST UPDATE TESTS IN SaveChangesTransactionTests SUITE ACCORDINGLY AS SOME CASES REFER TO NUMBERS IN THIS TABLE
            //
            // TABLE OF ACTIONS WE PERFORM HERE:
            //
            //  #  lastTransaction     currentTransaction         ConnectionState   WillClose      Action                                  Behavior when no explicit transaction (started with .ElistTransaction())     Behavior with explicit transaction (started with .ElistTransaction())
            //  1   null                null                       Open              No             no-op;                                  implicit transaction will be created and used                                explicit transaction should be used
            //  2   non-null tx1        non-null tx1               Open              No             no-op;                                  the last transaction will be used                                            N/A - it is not possible to EnlistTransaction if another transaction has already enlisted
            //  3   null                non-null                   Closed            Yes            connection.Open();                      Opening connection will automatically enlist into Transaction.Current        N/A - cannot enlist in transaction on a closed connection
            //  4   null                non-null                   Open              No             connection.Enlist(currentTransaction);  currentTransaction enlisted and used                                         N/A - it is not possible to EnlistTransaction if another transaction has already enlisted
            //  5   non-null            null                       Open              No             no-op;                                  implicit transaction will be created and used                                explicit transaction should be used
            //  6   non-null            null                       Closed            Yes            no-op;                                  implicit transaction will be created and used                                N/A - cannot enlist in transaction on a closed connection
            //  7   non-null tx1        non-null tx2               Open              No             connection.Enlist(currentTransaction);  currentTransaction enlisted and used                                         N/A - it is not possible to EnlistTransaction if another transaction has already enlisted
            //  8   non-null tx1        non-null tx2               Open              Yes            connection.Close(); connection.Open();  Re-opening connection will automatically enlist into Transaction.Current     N/A - only applies to TransactionScope - requires two transactions and CommitableTransaction and TransactionScope cannot be mixed
            //  9   non-null tx1        non-null tx2               Closed            Yes            connection.Open();                      Opening connection will automatcially enlist into Transaction.Current        N/A - cannot enlist in transaction on a closed connection

            var transactionHasChanged = (null != currentTransaction && !currentTransaction.Equals(_lastTransaction)) ||
                                        (null != _lastTransaction && !_lastTransaction.Equals(currentTransaction));

            if (transactionHasChanged)
            {
                if (!_openedConnection)
                {
                    // We didn't open the connection so, just try to enlist the connection in the current transaction. 
                    // Note that the connection can already be enlisted in a transaction (since the user opened 
                    // it s/he could enlist it manually using EntityConnection.EnlistTransaction() method). If the 
                    // transaction the connection is enlisted in has not completed (e.g. nested transaction) this call 
                    // will fail (throw). Also currentTransaction can be null here which means that the transaction
                    // used in the previous operation has completed. In this case we should not enlist the connection
                    // in "null" transaction as the user might have enlisted in a transaction manually between calls by 
                    // calling EntityConnection.EnlistTransaction() method. Enlisting with null would in this case mean "unenlist" 
                    // and would cause an exception (see above). Had the user not enlisted in a transaction between the calls
                    // enlisting with null would be a no-op - so again no reason to do it. 
                    if (currentTransaction != null)
                    {
                        Connection.EnlistTransaction(currentTransaction);
                    }
                }
                else if (_connectionRequestCount > 1)
                {
                    // We opened the connection. In addition we are here because there are multiple
                    // active requests going on (read: enumerators that has not been disposed yet) 
                    // using the same connection. (If there is only one active request e.g. like SaveChanges
                    // or single enumerator there is no need for any specific transaction handling - either
                    // we use the implicit ambient transaction (Transaction.Current) if one exists or we 
                    // will create our own local transaction. Also if there is only one active request
                    // the user could not enlist it in a transaction using EntityConnection.EnlistTransaction()
                    // because we opened the connection).
                    // If there are multiple active requests the user might have "played" with transactions
                    // after the first transaction. This code tries to deal with this kind of changes.

                    if (null == _lastTransaction)
                    {
                        Debug.Assert(currentTransaction != null, "transaction has changed and the lastTransaction was null");

                        // Two cases here: 
                        // - the previous operation was not run inside a transaction created by the user while this one is - just
                        //   enlist the connection in the transaction
                        // - the previous operation ran withing explicit transaction started with EntityConnection.EnlistTransaction()
                        //   method - try enlisting the connection in the transaction. This may fail however if the transactions 
                        //   are nested as you cannot enlist the connection in the transaction until the previous transaction has
                        //   completed.
                        Connection.EnlistTransaction(currentTransaction);
                    }
                    else
                    {
                        // We'll close and reopen the connection to get the benefit of automatic transaction enlistment.
                        // Remarks: We get here only if there is more than one active query (e.g. nested foreach or two subsequent queries or SaveChanges
                        // inside a for each) and each of these queries are using a different transaction (note that using TransactionScopeOption.Required 
                        // will not create a new transaction if an ambient transaction already exists - the ambient transaction will be used and we will 
                        // not end up in this code path). If we get here we are already in a loss-loss situation - we cannot enlist to the second transaction
                        // as this would cause an exception saying that there is already an active transaction that needs to be committed or rolled back
                        // before we can enlist the connection to a new transaction. The other option (and this is what we do here) is to close and reopen
                        // the connection. This will enlist the newly opened connection to the second transaction but will also close the reader being used
                        // by the first active query. As a result when trying to continue reading results from the first query the user will get an exception
                        // saying that calling "Read" on a closed data reader is not a valid operation.
                        Connection.Close();
                        return openConnection();
                    }
                }
            }
            else
            {
                // we don't need to do anything, nothing has changed.
            }

            return defaultValue;
        }

        // <summary>
        // Resets the state of connection management when the connection becomes closed.
        // </summary>
        private void ConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState
                == ConnectionState.Closed)
            {
                _connectionRequestCount = 0;
                _openedConnection = false;
            }
        }

        // <summary>
        // Releases the connection, potentially closing the connection if no active operations
        // require the connection to be open. There should be a single ReleaseConnection call
        // for each EnsureConnection call.
        // </summary>
        // <exception cref="ObjectDisposedException">
        // If the
        // <see cref="ObjectContext" />
        // instance has been disposed.
        // </exception>
        internal virtual void ReleaseConnection()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null, Strings.ObjectContext_ObjectDisposed);
            }

            if (_openedConnection)
            {
                Debug.Assert(_connectionRequestCount > 0, "_connectionRequestCount is zero or negative");
                if (_connectionRequestCount > 0)
                {
                    _connectionRequestCount--;
                }

                // When no operation is using the connection and the context had opened the connection
                // the connection can be closed
                if (_connectionRequestCount == 0)
                {
                    Connection.Close();
                    _openedConnection = false;
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates an <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> in the current object context by using the specified query string.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> of the specified type.
        /// </returns>
        /// <param name="queryString">The query string to be executed.</param>
        /// <param name="parameters">Parameters to pass to the query.</param>
        /// <typeparam name="T">
        /// The entity type of the returned <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" />.
        /// </typeparam>
        /// <exception cref="T:System.ArgumentNullException">The  queryString  or  parameters  parameter is null.</exception>
        public virtual ObjectQuery<T> CreateQuery<T>(string queryString, params ObjectParameter[] parameters)
        {
            Check.NotNull(queryString, "queryString");
            Check.NotNull(parameters, "parameters");

            // Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // We either auto-load <T>'s assembly into the ObjectItemCollection or we auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the user must manually call LoadFromAssembly.
            // *GetCallingAssembly returns the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), Assembly.GetCallingAssembly());

            // create a ObjectQuery<T> with default settings
            var query = new ObjectQuery<T>(queryString, this, MergeOption.AppendOnly);

            foreach (var parameter in parameters)
            {
                query.Parameters.Add(parameter);
            }

            return query;
        }

        // <summary>
        // Creates an EntityConnection from the given connection string.
        // </summary>
        // <param name="connectionString"> the connection string </param>
        // <returns> the newly created connection </returns>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For EntityConnection constructor. But the paths are not created in this method.
        private static EntityConnection CreateEntityConnection(string connectionString)
        {
            Check.NotEmpty(connectionString, "connectionString");

            // create the connection
            var connection = new EntityConnection(connectionString);

            return connection;
        }

        // <summary>
        // Given an entity connection, returns a copy of its MetadataWorkspace. Ensure we get
        // all of the metadata item collections by priming the entity connection.
        // </summary>
        // <exception cref="ObjectDisposedException">
        // If the
        // <see cref="ObjectContext" />
        // instance has been disposed.
        // </exception>
        private MetadataWorkspace RetrieveMetadataWorkspaceFromConnection()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null, Strings.ObjectContext_ObjectDisposed);
            }

            return _connection.GetMetadataWorkspace();
        }

        /// <summary>Marks an object for deletion. </summary>
        /// <param name="entity">
        /// An object that specifies the entity to delete. The object can be in any state except
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Detached" />
        /// .
        /// </param>
        public virtual void DeleteObject(object entity)
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            // This method and ObjectSet.DeleteObject are expected to have identical behavior except for the extra validation ObjectSet
            // requests by passing a non-null expectedEntitySetName. Any changes to this method are expected to be made in the common
            // internal overload below that ObjectSet also uses, unless there is a specific reason why a behavior is desired when the
            // call comes from ObjectContext only.
            DeleteObject(entity, null /*expectedEntitySetName*/);
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        // <summary>
        // Common DeleteObject method that is used by both ObjectContext.DeleteObject and ObjectSet.DeleteObject.
        // </summary>
        // <param name="entity"> Object to be deleted. </param>
        // <param name="expectedEntitySet"> EntitySet that the specified object is expected to be in. Null if the caller doesn't want to validate against a particular EntitySet. </param>
        internal void DeleteObject(object entity, EntitySet expectedEntitySet)
        {
            DebugCheck.NotNull(entity);
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            var cacheEntry = ObjectStateManager.FindEntityEntry(entity);
            if (cacheEntry == null
                || !ReferenceEquals(cacheEntry.Entity, entity))
            {
                throw new InvalidOperationException(Strings.ObjectContext_CannotDeleteEntityNotInObjectStateManager);
            }

            if (expectedEntitySet != null)
            {
                var actualEntitySet = cacheEntry.EntitySet;
                if (actualEntitySet != expectedEntitySet)
                {
                    throw new InvalidOperationException(
                        Strings.ObjectContext_EntityNotInObjectSet_Delete(
                            actualEntitySet.EntityContainer.Name, actualEntitySet.Name, expectedEntitySet.EntityContainer.Name,
                            expectedEntitySet.Name));
                }
            }

            cacheEntry.Delete();
            // Detaching from the context happens when the object
            // actually detaches from the cache (not just when it is
            // marked for deletion).
        }

        /// <summary>Removes the object from the object context.</summary>
        /// <param name="entity">
        /// Object to be detached. Only the  entity  is removed; if there are any related objects that are being tracked by the same
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" />
        /// , those will not be detached automatically.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">The  entity  is null. </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The  entity  is not associated with this <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> (for example, was newly created and not associated with any context yet, or was obtained through some other context, or was already detached).
        /// </exception>
        public virtual void Detach(object entity)
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();

            // This method and ObjectSet.DetachObject are expected to have identical behavior except for the extra validation ObjectSet
            // requests by passing a non-null expectedEntitySetName. Any changes to this method are expected to be made in the common
            // internal overload below that ObjectSet also uses, unless there is a specific reason why a behavior is desired when the
            // call comes from ObjectContext only.
            Detach(entity, expectedEntitySet: null);
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        // <summary>
        // Common Detach method that is used by both ObjectContext.Detach and ObjectSet.Detach.
        // </summary>
        // <param name="entity"> Object to be detached. </param>
        // <param name="expectedEntitySet"> EntitySet that the specified object is expected to be in. Null if the caller doesn't want to validate against a particular EntitySet. </param>
        internal void Detach(object entity, EntitySet expectedEntitySet)
        {
            DebugCheck.NotNull(entity);
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            var cacheEntry = ObjectStateManager.FindEntityEntry(entity);

            // this condition includes key entries and relationship entries
            if (cacheEntry == null
                || !ReferenceEquals(cacheEntry.Entity, entity)
                || cacheEntry.Entity == null)
            {
                throw new InvalidOperationException(Strings.ObjectContext_CannotDetachEntityNotInObjectStateManager);
            }

            if (expectedEntitySet != null)
            {
                var actualEntitySet = cacheEntry.EntitySet;
                if (actualEntitySet != expectedEntitySet)
                {
                    throw new InvalidOperationException(
                        Strings.ObjectContext_EntityNotInObjectSet_Detach(
                            actualEntitySet.EntityContainer.Name, actualEntitySet.Name, expectedEntitySet.EntityContainer.Name,
                            expectedEntitySet.Name));
                }
            }

            cacheEntry.Detach();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ObjectContext"/> class.
        /// </summary>
        ~ObjectContext()
        {
            Dispose(false);
        }

        /// <summary>Releases the resources used by the object context.</summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources used by the object context.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Need to dispose _transactionHandler even if being finalized
                if (_transactionHandler != null)
                {
                    _transactionHandler.Dispose();
                }

                if (disposing)
                {
                    // Release managed resources here.
                    if (_connection != null)
                    {
                        _connection.StateChange -= ConnectionStateChange;

                        // Dispose the connection the ObjectContext created
                        if (_contextOwnsConnection)
                        {
                            _connection.Dispose();
                        }
                    }
                    _connection = null; // Marks this object as disposed.
                    if (_objectStateManager != null)
                    {
                        _objectStateManager.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        internal bool IsDisposed
        {
            get { return _disposed; }
        }

        #region GetEntitySet

        // <summary>
        // Returns the EntitySet with the given name from given container.
        // </summary>
        // <param name="entitySetName"> Name of entity set. </param>
        // <param name="entityContainerName"> Name of container. </param>
        // <returns> The appropriate EntitySet. </returns>
        // <exception cref="InvalidOperationException">The entity set could not be found for the given name.</exception>
        // <exception cref="InvalidOperationException">The entity container could not be found for the given name.</exception>
        internal EntitySet GetEntitySet(string entitySetName, string entityContainerName)
        {
            DebugCheck.NotNull(entitySetName);

            EntityContainer container = null;

            if (String.IsNullOrEmpty(entityContainerName))
            {
                container = Perspective.GetDefaultContainer();
                Debug.Assert(container != null, "Problem with metadata - default container not found");
            }
            else
            {
                if (!MetadataWorkspace.TryGetEntityContainer(entityContainerName, DataSpace.CSpace, out container))
                {
                    throw new InvalidOperationException(Strings.ObjectContext_EntityContainerNotFoundForName(entityContainerName));
                }
            }

            EntitySet entitySet = null;

            if (!container.TryGetEntitySetByName(entitySetName, false, out entitySet))
            {
                throw new InvalidOperationException(
                    Strings.ObjectContext_EntitySetNotFoundForName(TypeHelpers.GetFullName(container.Name, entitySetName)));
            }

            return entitySet;
        }

        private static void GetEntitySetName(
            string qualifiedName, string parameterName, ObjectContext context, out string entityset, out string container)
        {
            entityset = null;
            container = null;
            Check.NotEmpty(qualifiedName, parameterName);

            var result = qualifiedName.Split('.');
            if (result.Length > 2)
            {
                throw new ArgumentException(Strings.ObjectContext_QualfiedEntitySetName, parameterName);
            }
            if (result.Length == 1) // if not '.' at all
            {
                entityset = result[0];
            }
            else
            {
                container = result[0];
                entityset = result[1];
                if (container == null
                    || container.Length == 0) // if it starts with '.'
                {
                    throw new ArgumentException(Strings.ObjectContext_QualfiedEntitySetName, parameterName);
                }
            }
            if (entityset == null
                || entityset.Length == 0) // if it's not in the form "ES name . containername"
            {
                throw new ArgumentException(Strings.ObjectContext_QualfiedEntitySetName, parameterName);
            }

            if (context != null
                && String.IsNullOrEmpty(container)
                && context.Perspective.GetDefaultContainer() == null)
            {
                throw new ArgumentException(Strings.ObjectContext_ContainerQualifiedEntitySetNameRequired, parameterName);
            }
        }

        // <summary>
        // Validate that an EntitySet is compatible with a given entity instance's CLR type.
        // </summary>
        // <param name="entitySet"> an EntitySet </param>
        // <param name="entityType"> The CLR type of an entity instance </param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private void ValidateEntitySet(EntitySet entitySet, Type entityType)
        {
            var entityTypeUsage = GetTypeUsage(entityType);
            if (!entitySet.ElementType.IsAssignableFrom(entityTypeUsage.EdmType))
            {
                throw new ArgumentException(Strings.ObjectContext_InvalidEntitySetOnEntity(entitySet.Name, entityType), "entity");
            }
        }

        internal TypeUsage GetTypeUsage(Type entityCLRType)
        {
            // Register the assembly so the type information will be sure to be loaded in metadata
            MetadataWorkspace.ImplicitLoadAssemblyForType(entityCLRType, Assembly.GetCallingAssembly());

            TypeUsage entityTypeUsage = null;
            if (!Perspective.TryGetType(entityCLRType, out entityTypeUsage)
                || !TypeSemantics.IsEntityType(entityTypeUsage))
            {
                Debug.Assert(entityCLRType != null, "The type cannot be null.");
                throw new InvalidOperationException(Strings.ObjectContext_NoMappingForEntityType(entityCLRType.FullName));
            }

            Debug.Assert(entityTypeUsage != null, "entityTypeUsage is null");
            return entityTypeUsage;
        }

        #endregion

        /// <summary>Returns an object that has the specified entity key.</summary>
        /// <returns>
        /// An <see cref="T:System.Object" /> that is an instance of an entity type.
        /// </returns>
        /// <param name="key">The key of the object to be found.</param>
        /// <exception cref="T:System.ArgumentNullException">The  key  parameter is null.</exception>
        /// <exception cref="T:System.Data.Entity.Core.ObjectNotFoundException">
        /// The object is not found in either the <see cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" /> or the data source.
        /// </exception>
        public virtual object GetObjectByKey(EntityKey key)
        {
            Check.NotNull(key, "key");

            var entitySet = key.GetEntitySet(MetadataWorkspace);
            Debug.Assert(entitySet != null, "Key's EntitySet should not be null in the MetadataWorkspace");

            // Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // Either the entity type's assembly is already in the ObjectItemCollection or we auto-load the user's calling assembly and its referenced assemblies.
            // *GetCallingAssembly returns the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadFromEntityType(entitySet.ElementType, Assembly.GetCallingAssembly());

            object entity;
            if (!TryGetObjectByKey(key, out entity))
            {
                throw new ObjectNotFoundException(Strings.ObjectContext_ObjectNotFound);
            }

            return entity;
        }

        #region Refresh

        /// <summary>Updates a collection of objects in the object context with data from the database. </summary>
        /// <param name="refreshMode">
        /// A <see cref="T:System.Data.Entity.Core.Objects.RefreshMode" /> value that indicates whether 
        /// property changes in the object context are overwritten with property values from the database.
        /// </param>
        /// <param name="collection">
        /// An <see cref="T:System.Collections.IEnumerable" /> collection of objects to refresh.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> collection  is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"> refreshMode  is not valid.</exception>
        /// <exception cref="T:System.ArgumentException"> collection is empty or an object is not attached to the context. </exception>
        public virtual void Refresh(RefreshMode refreshMode, IEnumerable collection)
        {
            Check.NotNull(collection, "collection");

            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            EntityUtil.CheckArgumentRefreshMode(refreshMode);

            // collection may not contain any entities -- this is valid for this overload
            RefreshEntities(refreshMode, collection);
        }

        /// <summary>Updates an object in the object context with data from the database. </summary>
        /// <param name="refreshMode">
        /// A <see cref="T:System.Data.Entity.Core.Objects.RefreshMode" /> value that indicates whether 
        /// property changes in the object context are overwritten with property values from the database.
        /// </param>
        /// <param name="entity">The object to be refreshed. </param>
        /// <exception cref="T:System.ArgumentNullException"> entity  is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"> refreshMode  is not valid.</exception>
        /// <exception cref="T:System.ArgumentException"> entity is not attached to the context. </exception>
        public virtual void Refresh(RefreshMode refreshMode, object entity)
        {
            Check.NotNull(entity, "entity");
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            EntityUtil.CheckArgumentRefreshMode(refreshMode);

            RefreshEntities(refreshMode, new[] { entity });
        }

#if !NET40

        /// <summary>Asynchronously updates a collection of objects in the object context with data from the database. </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="refreshMode">
        /// A <see cref="T:System.Data.Entity.Core.Objects.RefreshMode" /> value that indicates whether 
        /// property changes in the object context are overwritten with property values from the database.
        /// </param>
        /// <param name="collection">
        /// An <see cref="T:System.Collections.IEnumerable" /> collection of objects to refresh.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"> collection  is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"> refreshMode  is not valid.</exception>
        /// <exception cref="T:System.ArgumentException"> collection is empty or an object is not attached to the context. </exception>
        public Task RefreshAsync(RefreshMode refreshMode, IEnumerable collection)
        {
            return RefreshAsync(refreshMode, collection, CancellationToken.None);
        }

        /// <summary>Asynchronously updates a collection of objects in the object context with data from the database. </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="refreshMode">
        /// A <see cref="T:System.Data.Entity.Core.Objects.RefreshMode" /> value that indicates whether 
        /// property changes in the object context are overwritten with property values from the database.
        /// </param>
        /// <param name="collection">
        /// An <see cref="T:System.Collections.IEnumerable" /> collection of objects to refresh.
        /// </param>
        ///  <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"> collection  is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"> refreshMode  is not valid.</exception>
        /// <exception cref="T:System.ArgumentException"> collection is empty or an object is not attached to the context. </exception>
        public virtual Task RefreshAsync(RefreshMode refreshMode, IEnumerable collection, CancellationToken cancellationToken)
        {
            Check.NotNull(collection, "collection");

            cancellationToken.ThrowIfCancellationRequested();

            AsyncMonitor.EnsureNotEntered();
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            EntityUtil.CheckArgumentRefreshMode(refreshMode);

            return RefreshEntitiesAsync(refreshMode, collection, cancellationToken);
        }

        /// <summary>Asynchronously updates an object in the object context with data from the database. </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="refreshMode">
        /// A <see cref="T:System.Data.Entity.Core.Objects.RefreshMode" /> value that indicates whether 
        /// property changes in the object context are overwritten with property values from the database.
        /// </param>
        /// <param name="entity">The object to be refreshed. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"> entity  is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"> refreshMode  is not valid.</exception>
        /// <exception cref="T:System.ArgumentException"> entity is not attached to the context. </exception>
        public Task RefreshAsync(RefreshMode refreshMode, object entity)
        {
            return RefreshAsync(refreshMode, entity, CancellationToken.None);
        }

        /// <summary>Asynchronously updates an object in the object context with data from the database. </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="refreshMode">
        /// A <see cref="T:System.Data.Entity.Core.Objects.RefreshMode" /> value that indicates whether 
        /// property changes in the object context are overwritten with property values from the database.
        /// </param>
        /// <param name="entity">The object to be refreshed. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"> entity  is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"> refreshMode  is not valid.</exception>
        /// <exception cref="T:System.ArgumentException"> entity is not attached to the context. </exception>
        public virtual Task RefreshAsync(RefreshMode refreshMode, object entity, CancellationToken cancellationToken)
        {
            Check.NotNull(entity, "entity");
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            cancellationToken.ThrowIfCancellationRequested();

            AsyncMonitor.EnsureNotEntered();
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            EntityUtil.CheckArgumentRefreshMode(refreshMode);

            return RefreshEntitiesAsync(refreshMode, new[] { entity }, cancellationToken);
        }

#endif

        // <summary>
        // Validates that the given entity/key pair has an ObjectStateEntry
        // and that entry is not in the added state.
        // The entity is added to the entities dictionary, and checked for duplicates.
        // </summary>
        // <param name="entities"> on exit, entity is added to this dictionary. </param>
        private void RefreshCheck(
            Dictionary<EntityKey, EntityEntry> entities, EntityKey key)
        {
            var entry = ObjectStateManager.FindEntityEntry(key);
            if (null == entry)
            {
                throw new InvalidOperationException(Strings.ObjectContext_NthElementNotInObjectStateManager(entities.Count));
            }

            if (EntityState.Added
                == entry.State)
            {
                throw new InvalidOperationException(Strings.ObjectContext_NthElementInAddedState(entities.Count));
            }

            Debug.Assert(EntityState.Added != entry.State, "not expecting added");
            Debug.Assert(EntityState.Detached != entry.State, "not expecting detached");

            try
            {
                entities.Add(key, entry); // don't ignore duplicates
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException(Strings.ObjectContext_NthElementIsDuplicate(entities.Count));
            }

            Debug.Assert(null != (object)key, "null entity.Key");
            Debug.Assert(null != key.EntitySetName, "null entity.Key.EntitySetName");
        }

        private void RefreshEntities(RefreshMode refreshMode, IEnumerable collection)
        {
            // refreshMode and collection should already be validated prior to this call -- collection can be empty in one Refresh overload
            // but not in the other, so we need to do that check before we get to this common method
            DebugCheck.NotNull(collection);

            AsyncMonitor.EnsureNotEntered();

            var openedConnection = false;

            try
            {
                var entities = new Dictionary<EntityKey, EntityEntry>(RefreshEntitiesSize(collection));

                #region 1) Validate and bucket the entities by entity set

                var refreshKeys = new Dictionary<EntitySet, List<EntityKey>>();
                foreach (var entity in collection) // anything other than object risks InvalidCastException
                {
                    AddRefreshKey(entity, entities, refreshKeys);
                }

                #endregion

                #region 2) build and execute the query for each set of entities

                if (refreshKeys.Count > 0)
                {
                    EnsureConnection(shouldMonitorTransactions: false);
                    openedConnection = true;

                    // All entities from a single set can potentially be refreshed in the same query.
                    // However, the refresh operations are batched in an attempt to avoid the generation
                    // of query trees or provider SQL that exhaust available client or server resources.
                    foreach (var targetSet in refreshKeys.Keys)
                    {
                        var setKeys = refreshKeys[targetSet];
                        var refreshedCount = 0;
                        while (refreshedCount < setKeys.Count)
                        {
                            refreshedCount = BatchRefreshEntitiesByKey(refreshMode, entities, targetSet, setKeys, refreshedCount);
                        }
                    }
                }

                #endregion

                #region 3) process the unrefreshed entities

                if (RefreshMode.StoreWins == refreshMode)
                {
                    // remove all entites that have been removed from the store, not added by client
                    foreach (var item in entities)
                    {
                        Debug.Assert(EntityState.Added != item.Value.State, "should not be possible");
                        if (EntityState.Detached
                            != item.Value.State)
                        {
                            // We set the detaching flag here even though we are deleting because we are doing a
                            // Delete/AcceptChanges cycle to simulate a Detach, but we can't use Detach directly
                            // because legacy behavior around cascade deletes should be preserved.  However, we
                            // do want to prevent FK values in dependents from being nulled, which is why we
                            // need to set the detaching flag.
                            ObjectStateManager.TransactionManager.BeginDetaching();
                            try
                            {
                                item.Value.Delete();
                            }
                            finally
                            {
                                ObjectStateManager.TransactionManager.EndDetaching();
                            }
                            Debug.Assert(EntityState.Detached != item.Value.State, "not expecting detached");

                            item.Value.AcceptChanges();
                        }
                    }
                }
                else if (RefreshMode.ClientWins == refreshMode
                         && 0 < entities.Count)
                {
                    // throw an exception with all appropriate entity keys in text
                    var prefix = String.Empty;
                    var builder = new StringBuilder();
                    foreach (var item in entities)
                    {
                        Debug.Assert(EntityState.Added != item.Value.State, "should not be possible");
                        if (item.Value.State
                            == EntityState.Deleted)
                        {
                            // Detach the deleted items because this is the client changes and the server
                            // does not have these items any more
                            item.Value.AcceptChanges();
                        }
                        else
                        {
                            builder.Append(prefix).Append(Environment.NewLine);
                            builder.Append('\'').Append(item.Value.WrappedEntity.IdentityType.FullName).Append('\'');
                            prefix = ",";
                        }
                    }

                    // If there were items that could not be found, throw an exception
                    if (builder.Length > 0)
                    {
                        throw new InvalidOperationException(Strings.ObjectContext_ClientEntityRemovedFromStore(builder.ToString()));
                    }
                }

                #endregion
            }
            finally
            {
                if (openedConnection)
                {
                    ReleaseConnection();
                }

                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            }
        }

        private int BatchRefreshEntitiesByKey(
            RefreshMode refreshMode, Dictionary<EntityKey, EntityEntry> trackedEntities,
            EntitySet targetSet, List<EntityKey> targetKeys, int startFrom)
        {
            var queryPlanAndNextPosition = PrepareRefreshQuery(refreshMode, targetSet, targetKeys, startFrom);

            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
            var results = executionStrategy.Execute(
                () => ExecuteInTransaction(
                    () => queryPlanAndNextPosition.Item1.Execute<object>(this, null),
                    executionStrategy, startLocalTransaction: false,
                    releaseConnectionOnSuccess: true));

            ProcessRefreshedEntities(trackedEntities, results);

            // Return the position in the list from which the next refresh operation should start.
            // This will be equal to the list count if all remaining entities in the list were
            // refreshed during this call.
            return queryPlanAndNextPosition.Item2;
        }

#if !NET40

        private async Task RefreshEntitiesAsync(RefreshMode refreshMode, IEnumerable collection, CancellationToken cancellationToken)
        {
            // refreshMode and collection should already be validated prior to this call -- collection can be empty in one Refresh overload
            // but not in the other, so we need to do that check before we get to this common method
            DebugCheck.NotNull(collection);

            AsyncMonitor.Enter();

            var openedConnection = false;

            try
            {
                var entities = new Dictionary<EntityKey, EntityEntry>(RefreshEntitiesSize(collection));

                #region 1) Validate and bucket the entities by entity set

                var refreshKeys = new Dictionary<EntitySet, List<EntityKey>>();
                foreach (var entity in collection) // anything other than object risks InvalidCastException
                {
                    AddRefreshKey(entity, entities, refreshKeys);
                }

                #endregion

                #region 2) build and execute the query for each set of entities

                if (refreshKeys.Count > 0)
                {
                    await EnsureConnectionAsync(/*shouldMonitorTransactions:*/ false, cancellationToken).WithCurrentCulture();
                    openedConnection = true;

                    // All entities from a single set can potentially be refreshed in the same query.
                    // However, the refresh operations are batched in an attempt to avoid the generation
                    // of query trees or provider SQL that exhaust available client or server resources.
                    foreach (var targetSet in refreshKeys.Keys)
                    {
                        var setKeys = refreshKeys[targetSet];
                        var refreshedCount = 0;
                        while (refreshedCount < setKeys.Count)
                        {
                            refreshedCount =
                                await
                                BatchRefreshEntitiesByKeyAsync(refreshMode, entities, targetSet, setKeys, refreshedCount, cancellationToken)
                                    .WithCurrentCulture();
                        }
                    }
                }

                #endregion

                #region 3) process the unrefreshed entities

                if (RefreshMode.StoreWins == refreshMode)
                {
                    // remove all entites that have been removed from the store, not added by client
                    foreach (var item in entities)
                    {
                        Debug.Assert(EntityState.Added != item.Value.State, "should not be possible");
                        if (EntityState.Detached
                            != item.Value.State)
                        {
                            // We set the detaching flag here even though we are deleting because we are doing a
                            // Delete/AcceptChanges cycle to simulate a Detach, but we can't use Detach directly
                            // because legacy behavior around cascade deletes should be preserved.  However, we
                            // do want to prevent FK values in dependents from being nulled, which is why we
                            // need to set the detaching flag.
                            ObjectStateManager.TransactionManager.BeginDetaching();
                            try
                            {
                                item.Value.Delete();
                            }
                            finally
                            {
                                ObjectStateManager.TransactionManager.EndDetaching();
                            }
                            Debug.Assert(EntityState.Detached != item.Value.State, "not expecting detached");

                            item.Value.AcceptChanges();
                        }
                    }
                }
                else if (RefreshMode.ClientWins == refreshMode
                         && 0 < entities.Count)
                {
                    // throw an exception with all appropriate entity keys in text
                    var prefix = String.Empty;
                    var builder = new StringBuilder();
                    foreach (var item in entities)
                    {
                        Debug.Assert(EntityState.Added != item.Value.State, "should not be possible");
                        if (item.Value.State
                            == EntityState.Deleted)
                        {
                            // Detach the deleted items because this is the client changes and the server
                            // does not have these items any more
                            item.Value.AcceptChanges();
                        }
                        else
                        {
                            builder.Append(prefix).Append(Environment.NewLine);
                            builder.Append('\'').Append(item.Value.WrappedEntity.IdentityType.FullName).Append('\'');
                            prefix = ",";
                        }
                    }

                    // If there were items that could not be found, throw an exception
                    if (builder.Length > 0)
                    {
                        throw new InvalidOperationException(Strings.ObjectContext_ClientEntityRemovedFromStore(builder.ToString()));
                    }
                }

                #endregion
            }
            finally
            {
                if (openedConnection)
                {
                    ReleaseConnection();
                }

                AsyncMonitor.Exit();
                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            }
        }

        private async Task<int> BatchRefreshEntitiesByKeyAsync(
            RefreshMode refreshMode, Dictionary<EntityKey, EntityEntry> trackedEntities,
            EntitySet targetSet, List<EntityKey> targetKeys, int startFrom, CancellationToken cancellationToken)
        {
            var queryPlanAndNextPosition = PrepareRefreshQuery(refreshMode, targetSet, targetKeys, startFrom);

            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
            var results = await executionStrategy.ExecuteAsync(
                () => ExecuteInTransactionAsync(
                    () => queryPlanAndNextPosition.Item1.ExecuteAsync<object>(this, null, cancellationToken),
                    executionStrategy, startLocalTransaction: false,
                    releaseConnectionOnSuccess: true, cancellationToken: cancellationToken), cancellationToken)
                                                 .WithCurrentCulture();

            ProcessRefreshedEntities(trackedEntities, results);

            // Return the position in the list from which the next refresh operation should start.
            // This will be equal to the list count if all remaining entities in the list were
            // refreshed during this call.
            return queryPlanAndNextPosition.Item2;
        }

#endif

        internal virtual Tuple<ObjectQueryExecutionPlan, int> PrepareRefreshQuery(
            RefreshMode refreshMode, EntitySet targetSet, List<EntityKey> targetKeys, int startFrom)
        {
            // A single refresh query can be built for all entities from the same set.
            // For each entity set, a DbFilterExpression is constructed that
            // expresses the equivalent of:
            //
            // SELECT VALUE e
            // FROM <entityset> AS e
            // WHERE
            // GetRefKey(GetEntityRef(e)) == <ref1>.KeyValues
            // [OR GetRefKey(GetEntityRef(e)) == <ref2>.KeyValues
            // [..OR GetRefKey(GetEntityRef(e)) == <refN>.KeyValues]]
            //
            // Note that a LambdaFunctionExpression is used so that instead
            // of repeating GetRefKey(GetEntityRef(e)) a VariableReferenceExpression
            // to a Lambda argument with the value GetRefKey(GetEntityRef(e)) is used instead.
            // The query is therefore logically equivalent to:
            //
            // SELECT VALUE e
            // FROM <entityset> AS e
            // WHERE
            //   LET(x = GetRefKey(GetEntityRef(e)) IN (
            //      x == <ref1>.KeyValues
            //     [OR x == <ref2>.KeyValues
            //     [..OR x == <refN>.KeyValues]]
            //   )

            // The batch size determines the maximum depth of the predicate OR tree and
            // also limits the size of the generated provider SQL that is sent to the server.
            const int maxBatch = 250;

            // Bind the target EntitySet under the name "EntitySet".
            var entitySetBinding = targetSet.Scan().BindAs("EntitySet");

            // Use the variable from the set binding as the 'e' in a new GetRefKey(GetEntityRef(e)) expression.
            DbExpression sourceEntityKey = entitySetBinding.Variable.GetEntityRef().GetRefKey();

            // Build the where predicate as described above. A maximum of <batchsize> entity keys will be included
            // in the predicate, starting from position <startFrom> in the list of entity keys. As each key is
            // included, both <batchsize> and <startFrom> are incremented to ensure that the batch size is
            // correctly constrained and that the new starting position for the next call to this method is calculated.
            var batchSize = Math.Min(maxBatch, (targetKeys.Count - startFrom));
            var keyFilters = new DbExpression[batchSize];
            for (var idx = 0; idx < batchSize; idx++)
            {
                // Create a row constructor expression based on the key values of the EntityKey.
                var keyValueColumns = targetKeys[startFrom++].GetKeyValueExpressions(targetSet);
                DbExpression keyFilter = DbExpressionBuilder.NewRow(keyValueColumns);

                // Create an equality comparison between the row constructor and the lambda variable
                // that refers to GetRefKey(GetEntityRef(e)), which also produces a row
                // containing key values, but for the current entity from the entity set.
                keyFilters[idx] = sourceEntityKey.Equal(keyFilter);
            }

            // Sanity check that the batch includes at least one element.
            Debug.Assert(batchSize > 0, "Didn't create a refresh expression?");

            // Build a balanced binary tree that OR's the key filters together.
            var entitySetFilter = Helpers.BuildBalancedTreeInPlace(keyFilters, DbExpressionBuilder.Or);

            // Create a FilterExpression based on the EntitySet binding and the Lambda predicate.
            // This FilterExpression encapsulated the logic required for the refresh query as described above.
            DbExpression refreshQuery = entitySetBinding.Filter(entitySetFilter);

            // Initialize the command tree used to issue the refresh query.
            var tree = DbQueryCommandTree.FromValidExpression(
                MetadataWorkspace, DataSpace.CSpace, refreshQuery, useDatabaseNullSemantics: true);

            // Evaluate the refresh query using ObjectQuery<T> and process the results to update the ObjectStateManager.
            var mergeOption = (RefreshMode.StoreWins == refreshMode
                                   ? MergeOption.OverwriteChanges
                                   : MergeOption.PreserveChanges);
            var objectQueryExecutionPlan = _objectQueryExecutionPlanFactory.Prepare(
                this, tree, typeof(object), mergeOption,
                /*streaming:*/ false, null, null, DbExpressionBuilder.AliasGenerator);

            return new Tuple<ObjectQueryExecutionPlan, int>(objectQueryExecutionPlan, startFrom);
        }

        private void ProcessRefreshedEntities(Dictionary<EntityKey, EntityEntry> trackedEntities, ObjectResult<object> results)
        {
            foreach (var entity in results)
            {
                // There is a risk that, during an event, the Entity removed itself from the cache.
                var entry = ObjectStateManager.FindEntityEntry(entity);
                if (entry != null
                    && entry.State == EntityState.Modified)
                {
                    // this is 'ForceChanges' - which is the same as PreserveChanges, except all properties are marked modified.
                    entry.SetModifiedAll();
                }

                var wrappedEntity = EntityWrapperFactory.WrapEntityUsingContext(entity, this);
                var key = wrappedEntity.EntityKey;
                if ((object)key == null)
                {
                    throw Error.EntityKey_UnexpectedNull();
                }

                // An incorrectly returned entity should result in an exception to avoid further corruption to the ObjectStateManager.
                if (!trackedEntities.Remove(key))
                {
                    throw new InvalidOperationException(Strings.ObjectContext_StoreEntityNotPresentInClient);
                }
            }
        }

        private static int RefreshEntitiesSize(IEnumerable collection)
        {
            var list = collection as ICollection;
            return null != list ? list.Count : 0;
        }

        #endregion

        #region SaveChanges

        /// <summary>Persists all updates to the database and resets change tracking in the object context.</summary>
        /// <returns>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for 
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Core.OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        public virtual int SaveChanges()
        {
            return SaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave);
        }

#if !NET40

        /// <summary>Asynchronously persists all updates to the database and resets change tracking in the object context.</summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for 
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Core.OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        public virtual Task<Int32> SaveChangesAsync()
        {
            return SaveChangesAsync(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, CancellationToken.None);
        }

        /// <summary>Asynchronously persists all updates to the database and resets change tracking in the object context.</summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for 
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Core.OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        public virtual Task<Int32> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return SaveChangesAsync(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, cancellationToken);
        }

#endif

        /// <summary>Persists all updates to the database and optionally resets change tracking in the object context.</summary>
        /// <param name="acceptChangesDuringSave">
        /// This parameter is needed for client-side transaction support. If true, the change tracking on all objects is reset after
        /// <see cref="M:System.Data.Entity.Core.Objects.ObjectContext.SaveChanges(System.Boolean)" />
        /// finishes. If false, you must call the <see cref="M:System.Data.Entity.Core.Objects.ObjectContext.AcceptAllChanges" />
        /// method after <see cref="M:System.Data.Entity.Core.Objects.ObjectContext.SaveChanges(System.Boolean)" />.
        /// </param>
        /// <returns>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for 
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Core.OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [Obsolete("Use SaveChanges(SaveOptions options) instead.")]
        public virtual int SaveChanges(bool acceptChangesDuringSave)
        {
            return SaveChanges(
                acceptChangesDuringSave
                    ? SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave
                    : SaveOptions.DetectChangesBeforeSave);
        }

        /// <summary>Persists all updates to the database and optionally resets change tracking in the object context.</summary>
        /// <param name="options">
        /// A <see cref="T:System.Data.Entity.Core.Objects.SaveOptions" /> value that determines the behavior of the operation.
        /// </param>
        /// <returns>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for 
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Core.OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        public virtual int SaveChanges(SaveOptions options)
        {
            return SaveChangesInternal(options, executeInExistingTransaction: false);
        }

        internal int SaveChangesInternal(SaveOptions options, bool executeInExistingTransaction)
        {
            AsyncMonitor.EnsureNotEntered();

            PrepareToSaveChanges(options);

            var entriesAffected = 0;

            // if there are no changes to save, perform fast exit to avoid interacting with or starting of new transactions
            if (ObjectStateManager.HasChanges())
            {
                if (executeInExistingTransaction)
                {
                    entriesAffected = SaveChangesToStore(options, null, startLocalTransaction: false);
                }
                else
                {
                    var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
                    entriesAffected = executionStrategy.Execute(
                        () => SaveChangesToStore(options, executionStrategy, startLocalTransaction: true));
                }
            }

            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            return entriesAffected;
        }

#if !NET40

        /// <summary>Asynchronously persists all updates to the database and optionally resets change tracking in the object context.</summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="options">
        /// A <see cref="T:System.Data.Entity.Core.Objects.SaveOptions" /> value that determines the behavior of the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for 
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Core.OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        public virtual Task<Int32> SaveChangesAsync(SaveOptions options)
        {
            return SaveChangesAsync(options, CancellationToken.None);
        }

        /// <summary>Asynchronously persists all updates to the database and optionally resets change tracking in the object context.</summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="options">
        /// A <see cref="T:System.Data.Entity.Core.Objects.SaveOptions" /> value that determines the behavior of the operation.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are created for 
        /// many-to-many relationships and relationships where there is no foreign key property
        /// included in the entity class (often referred to as independent associations).
        /// </returns>
        /// <exception cref="T:System.Data.Entity.Core.OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        public virtual Task<Int32> SaveChangesAsync(SaveOptions options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AsyncMonitor.EnsureNotEntered();

            return SaveChangesInternalAsync(options, /*executeInExistingTransaction:*/ false, cancellationToken);
        }

        internal async Task<Int32> SaveChangesInternalAsync(SaveOptions options, bool executeInExistingTransaction, CancellationToken cancellationToken)
        {
            AsyncMonitor.Enter();
            try
            {
                PrepareToSaveChanges(options);

                var entriesAffected = 0;

                // if there are no changes to save, perform fast exit to avoid interacting with or starting of new transactions
                if (ObjectStateManager.HasChanges())
                {
                    if (executeInExistingTransaction)
                    {
                        entriesAffected =
                            await SaveChangesToStoreAsync(
                                options, /*executionStrategy:*/ null, /*startLocalTransaction:*/ false,
                                cancellationToken).WithCurrentCulture();
                    }
                    else
                    {
                        var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
                        entriesAffected = await executionStrategy.ExecuteAsync(
                            () => SaveChangesToStoreAsync(options, executionStrategy, /*startLocalTransaction:*/ true, cancellationToken),
                            cancellationToken).WithCurrentCulture();
                    }
                }

                ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
                return entriesAffected;
            }
            finally
            {
                AsyncMonitor.Exit();
            }
        }

#endif

        private void PrepareToSaveChanges(SaveOptions options)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null, Strings.ObjectContext_ObjectDisposed);
            }

            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();

            OnSavingChanges();

            if ((SaveOptions.DetectChangesBeforeSave & options) != 0)
            {
                ObjectStateManager.DetectChanges();
            }

            if (ObjectStateManager.SomeEntryWithConceptualNullExists())
            {
                throw new InvalidOperationException(Strings.ObjectContext_CommitWithConceptualNull);
            }
        }

        private int SaveChangesToStore(SaveOptions options, IDbExecutionStrategy executionStrategy, bool startLocalTransaction)
        {
            // only accept changes after the local transaction commits
            _adapter.AcceptChangesDuringUpdate = false;
            _adapter.Connection = Connection;
            _adapter.CommandTimeout = CommandTimeout;

            var entriesAffected
                = ExecuteInTransaction(
                    () => _adapter.Update(),
                    executionStrategy,
                    startLocalTransaction,
                    releaseConnectionOnSuccess: true);

            if ((SaveOptions.AcceptAllChangesAfterSave & options) != 0)
            {
                try
                {
                    AcceptAllChanges();
                }
                catch (Exception e)
                {
                    // If AcceptAllChanges throws - let's inform user that changes in database were committed 
                    // and that Context and Database can be in inconsistent state.
                    throw new InvalidOperationException(Strings.ObjectContext_AcceptAllChangesFailure(e.Message), e);
                }
            }

            return entriesAffected;
        }

#if !NET40

        private async Task<int> SaveChangesToStoreAsync(
            SaveOptions options, IDbExecutionStrategy executionStrategy, bool startLocalTransaction, CancellationToken cancellationToken)
        {
            // only accept changes after the local transaction commits
            _adapter.AcceptChangesDuringUpdate = false;
            _adapter.Connection = Connection;
            _adapter.CommandTimeout = CommandTimeout;

            var entriesAffected = await ExecuteInTransactionAsync(
                () => _adapter.UpdateAsync(cancellationToken), executionStrategy,
                startLocalTransaction, /*releaseConnectionOnSuccess:*/ true, cancellationToken)
                                            .WithCurrentCulture();

            if ((SaveOptions.AcceptAllChangesAfterSave & options) != 0)
            {
                try
                {
                    AcceptAllChanges();
                }
                catch (Exception e)
                {
                    // If AcceptAllChanges throws - let's inform user that changes in database were committed 
                    // and that Context and Database can be in inconsistent state.
                    throw new InvalidOperationException(Strings.ObjectContext_AcceptAllChangesFailure(e.Message), e);
                }
            }

            return entriesAffected;
        }

#endif

        #endregion //SaveChanges

        // <summary>
        // Executes a function in a local transaction and returns the result.
        // </summary>
        // <remarks>
        // A local transaction is created only if there are no existing local nor ambient transactions.
        // This method will ensure that the connection is opened and release it if an exception is thrown.
        // The caller is responsible of releasing the connection if no exception is thrown, unless
        // <paramref name="releaseConnectionOnSuccess" /> is set to <c>true</c>.
        // </remarks>
        // <typeparam name="T"> Type of the result. </typeparam>
        // <param name="func"> The function to invoke. </param>
        // <param name="executionStrategy"> The execution strategy used for this operation. </param>
        // <param name="startLocalTransaction"> Whether should start a new local transaction when there's no existing one. </param>
        // <param name="releaseConnectionOnSuccess"> Whether the connection will also be released when no exceptions are thrown. </param>
        // <returns>
        // The result from invoking <paramref name="func" />.
        // </returns>
        internal virtual T ExecuteInTransaction<T>(
            Func<T> func, IDbExecutionStrategy executionStrategy, bool startLocalTransaction, bool releaseConnectionOnSuccess)
        {
            EnsureConnection(startLocalTransaction);

            var needLocalTransaction = false;
            var connection = (EntityConnection)Connection;
            if (connection.CurrentTransaction == null
                && !connection.EnlistedInUserTransaction
                && _lastTransaction == null)
            {
                needLocalTransaction = startLocalTransaction;
            }
            else if (executionStrategy != null
                && executionStrategy.RetriesOnFailure)
            {
                throw new InvalidOperationException(Strings.ExecutionStrategy_ExistingTransaction(executionStrategy.GetType().Name));
            }
            // else the caller already has his own local transaction going; caller will do the abort or commit.

            DbTransaction localTransaction = null;
            try
            {
                // EntityConnection tracks the CurrentTransaction we don't need to pass it around
                if (needLocalTransaction)
                {
                    localTransaction = connection.BeginTransaction();
                }

                var result = func();

                if (localTransaction != null)
                {
                    // we started the local transaction; so we also commit it
                    localTransaction.Commit();
                }
                // else on success with no exception is thrown, caller generally commits the transaction

                if (releaseConnectionOnSuccess)
                {
                    ReleaseConnection();
                }

                return result;
            }
            catch (Exception)
            {
                ReleaseConnection();
                throw;
            }
            finally
            {
                if (localTransaction != null)
                {
                    // we started the local transaction; so it requires disposal (rollback if not previously committed
                    localTransaction.Dispose();
                }
                // else on failure with an exception being thrown, caller generally aborts (default action with transaction without an explict commit)
            }
        }

#if !NET40

        // <summary>
        // An asynchronous version of ExecuteStoreQuery, which
        // executes a function in a local transaction and returns the result.
        // </summary>
        // <remarks>
        // A local transaction is created only if there are no existing local nor ambient transactions.
        // This method will ensure that the connection is opened and release it if an exception is thrown.
        // The caller is responsible of releasing the connection if no exception is thrown, unless
        // <paramref name="releaseConnectionOnSuccess" /> is set to <c>true</c>.
        // </remarks>
        // <typeparam name="T"> Type of the result. </typeparam>
        // <param name="func"> The function to invoke. </param>
        // <param name="executionStrategy"> The execution strategy used for this operation. </param>
        // <param name="startLocalTransaction"> Whether should start a new local transaction when there's no existing one. </param>
        // <param name="releaseConnectionOnSuccess"> Whether the connection will also be released when no exceptions are thrown. </param>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <returns>
        // A task containing the result from invoking <paramref name="func" />.
        // </returns>
        internal virtual async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> func, IDbExecutionStrategy executionStrategy,
            bool startLocalTransaction, bool releaseConnectionOnSuccess, CancellationToken cancellationToken)
        {
            await EnsureConnectionAsync(startLocalTransaction, cancellationToken).WithCurrentCulture();

            var needLocalTransaction = false;
            var connection = (EntityConnection)Connection;
            if (connection.CurrentTransaction == null
                && !connection.EnlistedInUserTransaction
                && _lastTransaction == null)
            {
                needLocalTransaction = startLocalTransaction;
            }
            else if (executionStrategy.RetriesOnFailure)
            {
                throw new InvalidOperationException(Strings.ExecutionStrategy_ExistingTransaction(executionStrategy.GetType().Name));
            }
            // else the caller already has his own local transaction going; caller will do the abort or commit.

            DbTransaction localTransaction = null;
            try
            {
                // EntityConnection tracks the CurrentTransaction we don't need to pass it around
                if (needLocalTransaction)
                {
                    localTransaction = connection.BeginTransaction();
                }

                var result = await func().WithCurrentCulture();

                if (localTransaction != null)
                {
                    // we started the local transaction; so we also commit it
                    localTransaction.Commit();
                }
                // else on success with no exception is thrown, caller generally commits the transaction

                if (releaseConnectionOnSuccess)
                {
                    ReleaseConnection();
                }

                return result;
            }
            catch (Exception)
            {
                ReleaseConnection();
                throw;
            }
            finally
            {
                if (localTransaction != null)
                {
                    // we started the local transaction; so it requires disposal (rollback if not previously committed
                    localTransaction.Dispose();
                }
                // else on failure with an exception being thrown, caller generally aborts (default action with transaction without an explict commit)
            }
        }

#endif

        /// <summary>
        /// Ensures that <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> changes are synchronized with changes in all objects that are tracked by the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" />
        /// .
        /// </summary>
        public virtual void DetectChanges()
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            ObjectStateManager.DetectChanges();
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        /// <summary>Returns an object that has the specified entity key.</summary>
        /// <returns>true if the object was retrieved successfully. false if the  key  is temporary, the connection is null, or the  value  is null.</returns>
        /// <param name="key">The key of the object to be found.</param>
        /// <param name="value">When this method returns, contains the object.</param>
        /// <exception cref="T:System.ArgumentException">Incompatible metadata for  key .</exception>
        /// <exception cref="T:System.ArgumentNullException"> key  is null.</exception>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryGetObjectByKey(EntityKey key, out object value)
        {
            // try the cache first
            EntityEntry entry;
            ObjectStateManager.TryGetEntityEntry(key, out entry); // this will check key argument
            if (entry != null)
            {
                // can't find keys
                if (!entry.IsKeyEntry)
                {
                    // SQLBUDT 511296 returning deleted object.
                    value = entry.Entity;
                    return value != null;
                }
            }

            if (key.IsTemporary)
            {
                // If the key is temporary, we cannot find a corresponding object in the store.
                value = null;
                return false;
            }

            var entitySet = key.GetEntitySet(MetadataWorkspace);
            Debug.Assert(entitySet != null, "Key's EntitySet should not be null in the MetadataWorkspace");

            // Validate the EntityKey values against the EntitySet
            key.ValidateEntityKey(_workspace, entitySet, true /*isArgumentException*/, "key");

            // Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // If the schema types are not loaded: metadata, cache & query would be unable to reason about the type.
            // Either the entity type's assembly is already in the ObjectItemCollection or we auto-load the user's calling assembly and its referenced assemblies.
            // *GetCallingAssembly returns the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadFromEntityType(entitySet.ElementType, Assembly.GetCallingAssembly());

            // Execute the query:
            // SELECT VALUE X FROM [EC].[ES] AS X
            // WHERE X.KeyProp0 = @p0 AND X.KeyProp1 = @p1 AND ... 
            // parameters are the key values 

            // Build the Entity SQL query
            var esql = new StringBuilder();
            esql.AppendFormat(
                "SELECT VALUE X FROM {0}.{1} AS X WHERE ", EntityUtil.QuoteIdentifier(entitySet.EntityContainer.Name),
                EntityUtil.QuoteIdentifier(entitySet.Name));
            var members = key.EntityKeyValues;
            var keyMembers = entitySet.ElementType.KeyMembers;
            var parameters = new ObjectParameter[members.Length];

            for (var i = 0; i < members.Length; i++)
            {
                if (i > 0)
                {
                    esql.Append(" AND ");
                }

                var parameterName = string.Format(CultureInfo.InvariantCulture, "p{0}", i.ToString(CultureInfo.InvariantCulture));
                esql.AppendFormat("X.{0} = @{1}", EntityUtil.QuoteIdentifier(members[i].Key), parameterName);
                parameters[i] = new ObjectParameter(parameterName, members[i].Value);

                // Try to set the TypeUsage on the ObjectParameter
                EdmMember keyMember = null;
                if (keyMembers.TryGetValue(members[i].Key, true, out keyMember))
                {
                    parameters[i].TypeUsage = keyMember.TypeUsage;
                }
            }

            // Execute the query
            object entity = null;
            var results = CreateQuery<object>(esql.ToString(), parameters).Execute(MergeOption.AppendOnly);
            foreach (var queriedEntity in results)
            {
                Debug.Assert(entity == null, "Query for a key returned more than one entity!");
                entity = queriedEntity;
            }

            value = entity;
            return value != null;
        }

        /// <summary>
        /// Executes a stored procedure or function that is defined in the data source and mapped in the conceptual model, with the specified parameters. Returns a typed
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" />
        /// .
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> for the data that is returned by the stored procedure.
        /// </returns>
        /// <param name="functionName">The name of the stored procedure or function. The name can include the container name, such as &lt;Container Name&gt;.&lt;Function Name&gt;. When the default container name is known, only the function name is required.</param>
        /// <param name="parameters">
        /// An array of <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> objects. If output parameters are used, 
        /// their values will not be available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <typeparam name="TElement">
        /// The entity type of the <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> returned when the function is executed against the data source. This type must implement
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.IEntityWithChangeTracker" />
        /// .
        /// </typeparam>
        /// <exception cref="T:System.ArgumentException"> function  is null or empty or function  is not found.</exception>
        /// <exception cref="T:System.InvalidOperationException">The entity reader does not support this  function or there is a type mismatch on the reader and the  function .</exception>
        public ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, params ObjectParameter[] parameters)
        {
            Check.NotNull(parameters, "parameters");

            return ExecuteFunction<TElement>(functionName, MergeOption.AppendOnly, parameters);
        }

        /// <summary>
        /// Executes the given stored procedure or function that is defined in the data source and expressed in the conceptual model, with the specified parameters, and merge option. Returns a typed
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" />
        /// .
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> for the data that is returned by the stored procedure.
        /// </returns>
        /// <param name="functionName">The name of the stored procedure or function. The name can include the container name, such as &lt;Container Name&gt;.&lt;Function Name&gt;. When the default container name is known, only the function name is required.</param>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when executing the query.
        /// </param>
        /// <param name="parameters">
        /// An array of <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> objects. If output parameters are used, 
        /// their values will not be available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <typeparam name="TElement">
        /// The entity type of the <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> returned when the function is executed against the data source. This type must implement
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.IEntityWithChangeTracker" />
        /// .
        /// </typeparam>
        /// <exception cref="T:System.ArgumentException"> function  is null or empty or function  is not found.</exception>
        /// <exception cref="T:System.InvalidOperationException">The entity reader does not support this  function or there is a type mismatch on the reader and the  function .</exception>
        public virtual ObjectResult<TElement> ExecuteFunction<TElement>(
            string functionName, MergeOption mergeOption, params ObjectParameter[] parameters)
        {
            Check.NotNull(parameters, "parameters");
            Check.NotEmpty(functionName, "functionName");
            return ExecuteFunction<TElement>(functionName, new ExecutionOptions(mergeOption), parameters);
        }

        /// <summary>
        /// Executes the given function on the default container.
        /// </summary>
        /// <typeparam name="TElement"> Element type for function results. </typeparam>
        /// <param name="functionName">
        /// Name of function. May include container (e.g. ContainerName.FunctionName) or just function name when DefaultContainerName is known.
        /// </param>
        /// <param name="executionOptions"> The options for executing this function. </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the function. If output parameters are used, their values 
        /// will not be available until the results have been read completely. This is due to the underlying 
        /// behavior of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>An object representing the result of executing this function.</returns>
        /// <exception cref="ArgumentException"> If function is null or empty </exception>
        /// <exception cref="InvalidOperationException">
        /// If function is invalid (syntax,
        /// does not exist, refers to a function with return type incompatible with T)
        /// </exception>
        public virtual ObjectResult<TElement> ExecuteFunction<TElement>(
            string functionName, ExecutionOptions executionOptions, params ObjectParameter[] parameters)
        {
            Check.NotNull(parameters, "parameters");
            Check.NotEmpty(functionName, "functionName");

            AsyncMonitor.EnsureNotEntered();

            EdmFunction functionImport;
            var entityCommand = CreateEntityCommandForFunctionImport(functionName, out functionImport, parameters);
            var returnTypeCount = Math.Max(1, functionImport.ReturnParameters.Count);
            var expectedEdmTypes = new EdmType[returnTypeCount];
            expectedEdmTypes[0] = MetadataHelper.GetAndCheckFunctionImportReturnType<TElement>(functionImport, 0, MetadataWorkspace);
            for (var i = 1; i < returnTypeCount; i++)
            {
                if (!MetadataHelper.TryGetFunctionImportReturnType(functionImport, i, out expectedEdmTypes[i]))
                {
                    throw EntityUtil.ExecuteFunctionCalledWithNonReaderFunction(functionImport);
                }
            }

            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);

            if (executionStrategy.RetriesOnFailure
                && executionOptions.UserSpecifiedStreaming.HasValue && executionOptions.UserSpecifiedStreaming.Value)
            {
                throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
            }

            if (!executionOptions.UserSpecifiedStreaming.HasValue)
            {
                executionOptions = new ExecutionOptions(executionOptions.MergeOption, !executionStrategy.RetriesOnFailure);
            }

            var startLocalTransaction = !executionOptions.UserSpecifiedStreaming.Value
                                        && _options.EnsureTransactionsForFunctionsAndCommands;
            return executionStrategy.Execute(
                () => ExecuteInTransaction(
                    () => CreateFunctionObjectResult<TElement>(entityCommand, functionImport.EntitySets, expectedEdmTypes, executionOptions),
                    executionStrategy, startLocalTransaction: startLocalTransaction,
                    releaseConnectionOnSuccess: !executionOptions.UserSpecifiedStreaming.Value));
        }

        /// <summary>Executes a stored procedure or function that is defined in the data source and expressed in the conceptual model; discards any results returned from the function; and returns the number of rows affected by the execution.</summary>
        /// <returns>The number of rows affected.</returns>
        /// <param name="functionName">The name of the stored procedure or function. The name can include the container name, such as &lt;Container Name&gt;.&lt;Function Name&gt;. When the default container name is known, only the function name is required.</param>
        /// <param name="parameters">
        /// An array of <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> objects. If output parameters are used, 
        /// their values will not be available until the results have been read completely. This is due to the underlying 
        /// behavior of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <exception cref="T:System.ArgumentException"> function  is null or empty or function  is not found.</exception>
        /// <exception cref="T:System.InvalidOperationException">The entity reader does not support this  function or there is a type mismatch on the reader and the  function .</exception>
        public virtual int ExecuteFunction(string functionName, params ObjectParameter[] parameters)
        {
            Check.NotNull(parameters, "parameters");
            Check.NotEmpty(functionName, "functionName");

            AsyncMonitor.EnsureNotEntered();

            EdmFunction functionImport;
            var entityCommand = CreateEntityCommandForFunctionImport(functionName, out functionImport, parameters);

            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
            return executionStrategy.Execute(
                () => ExecuteInTransaction(
                    () => ExecuteFunctionCommand(entityCommand), executionStrategy,
                    startLocalTransaction: _options.EnsureTransactionsForFunctionsAndCommands,
                    releaseConnectionOnSuccess: true));
        }

        private static int ExecuteFunctionCommand(EntityCommand entityCommand)
        {
            // Prepare the command before calling ExecuteNonQuery, so that exceptions thrown during preparation are not wrapped in EntityCommandExecutionException
            entityCommand.Prepare();

            try
            {
                return entityCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                if (e.IsCatchableEntityExceptionType())
                {
                    throw new EntityCommandExecutionException(Strings.EntityClient_CommandExecutionFailed, e);
                }

                throw;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private EntityCommand CreateEntityCommandForFunctionImport(
            string functionName, out EdmFunction functionImport, params ObjectParameter[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (null == parameter)
                {
                    throw new InvalidOperationException(Strings.ObjectContext_ExecuteFunctionCalledWithNullParameter(i));
                }
            }

            string containerName;
            string functionImportName;

            functionImport =
                MetadataHelper.GetFunctionImport(
                    functionName, DefaultContainerName, MetadataWorkspace,
                    out containerName, out functionImportName);

            var connection = (EntityConnection)Connection;

            // create query
            var entityCommand = new EntityCommand(InterceptionContext);
            entityCommand.CommandType = CommandType.StoredProcedure;
            entityCommand.CommandText = containerName + "." + functionImportName;
            entityCommand.Connection = connection;
            if (CommandTimeout.HasValue)
            {
                entityCommand.CommandTimeout = CommandTimeout.Value;
            }

            PopulateFunctionImportEntityCommandParameters(parameters, functionImport, entityCommand);

            return entityCommand;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Reader disposed by the returned ObjectResult")]
        private ObjectResult<TElement> CreateFunctionObjectResult<TElement>(
            EntityCommand entityCommand, ReadOnlyCollection<EntitySet> entitySets, EdmType[] edmTypes,
            ExecutionOptions executionOptions)
        {
            DebugCheck.NotNull(edmTypes);
            Debug.Assert(edmTypes.Length > 0);

            var commandDefinition = entityCommand.GetCommandDefinition();

            // get store data reader
            DbDataReader storeReader = null;
            try
            {
                storeReader = commandDefinition.ExecuteStoreCommands(
                    entityCommand, executionOptions.UserSpecifiedStreaming.Value
                        ? CommandBehavior.Default
                        : CommandBehavior.SequentialAccess);
            }
            catch (Exception e)
            {
                if (e.IsCatchableEntityExceptionType())
                {
                    throw new EntityCommandExecutionException(Strings.EntityClient_CommandExecutionFailed, e);
                }

                throw;
            }

            ShaperFactory<TElement> shaperFactory = null;
            if (!executionOptions.UserSpecifiedStreaming.Value)
            {
                BufferedDataReader bufferedReader = null;
                try
                {
                    var storeItemCollection = (StoreItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                    var providerServices = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);

                    shaperFactory = _translator.TranslateColumnMap<TElement>(
                        commandDefinition.CreateColumnMap(storeReader, 0),
                        MetadataWorkspace, null, executionOptions.MergeOption, false, valueLayer: false);
                    bufferedReader = new BufferedDataReader(storeReader);
                    bufferedReader.Initialize(
                        storeItemCollection.ProviderManifestToken, providerServices, shaperFactory.ColumnTypes,
                        shaperFactory.NullableColumns);
                    storeReader = bufferedReader;
                }
                catch (Exception)
                {
                    if (bufferedReader != null)
                    {
                        bufferedReader.Dispose();
                    }

                    throw;
                }
            }

            return MaterializedDataRecord<TElement>(
                entityCommand, storeReader, 0, entitySets, edmTypes, shaperFactory, executionOptions.MergeOption, executionOptions.UserSpecifiedStreaming.Value);
        }

        // <summary>
        // Get the materializer for the resultSetIndexth result set of storeReader.
        // </summary>
        internal ObjectResult<TElement> MaterializedDataRecord<TElement>(
            EntityCommand entityCommand, DbDataReader storeReader, int resultSetIndex, ReadOnlyCollection<EntitySet> entitySets,
            EdmType[] edmTypes, ShaperFactory<TElement> shaperFactory, MergeOption mergeOption, bool streaming)
        {
            DebugCheck.NotNull(entityCommand);
            DebugCheck.NotNull(storeReader);
            DebugCheck.NotNull(entitySets);
            DebugCheck.NotNull(edmTypes);

            var commandDefinition = entityCommand.GetCommandDefinition();
            try
            {
                // We want the shaper to close the reader if it is the last result set.
                var shaperOwnsReader = edmTypes.Length <= resultSetIndex + 1;

                //Note: Defensive check for historic reasons, we expect entitySets.Count > resultSetIndex 
                var entitySet = entitySets.Count > resultSetIndex ? entitySets[resultSetIndex] : null;

                // create the shaper
                if (shaperFactory == null)
                {
                    shaperFactory = _translator.TranslateColumnMap<TElement>(
                        commandDefinition.CreateColumnMap(storeReader, resultSetIndex),
                        MetadataWorkspace, null, mergeOption, streaming, valueLayer: false);
                }

                var shaper = shaperFactory.Create(
                    storeReader, this, MetadataWorkspace, mergeOption, shaperOwnsReader, streaming);

                NextResultGenerator nextResultGenerator;

                // We need to run notifications when the data reader is closed in order to propagate any out parameters.
                // We do this whenever the last (declared) result set's enumerator is disposed (this calls Finally on the shaper)
                // or when the underlying reader is closed as a result of the ObjectResult itself getting disposed.   
                // We use onReaderDisposeHasRun to ensure that this notification is only called once.   
                // the alternative approach of not making the final ObjectResult's disposal result do cleanup doesn't work in the case where
                // its GetEnumerator is called explicitly, and the resulting enumerator is never disposed.
                var onReaderDisposeHasRun = false;
                Action<object, EventArgs> onReaderDispose = (object sender, EventArgs e) =>
                {
                    if (!onReaderDisposeHasRun)
                    {
                        onReaderDisposeHasRun = true;
                        // consume the store reader
                        CommandHelper.ConsumeReader(storeReader);
                        // trigger event callback
                        entityCommand.NotifyDataReaderClosing();
                    }
                };

                if (shaperOwnsReader)
                {
                    shaper.OnDone += new EventHandler(onReaderDispose);
                    nextResultGenerator = null;
                }
                else
                {
                    nextResultGenerator = new NextResultGenerator(
                        this, entityCommand, edmTypes, entitySets, mergeOption, streaming, resultSetIndex + 1);
                }

                // We want the ObjectResult to close the reader in its Dispose method, even if it is not the last result set.
                // This is to allow users to cancel reading results without the unnecessary iteration thru all the result sets.
                return new ObjectResult<TElement>(
                    shaper, entitySet, TypeUsage.Create(edmTypes[resultSetIndex]), true, streaming, nextResultGenerator,
                    onReaderDispose);
            }
            catch
            {
                ReleaseConnection();
                storeReader.Dispose();
                throw;
            }
        }

        private void PopulateFunctionImportEntityCommandParameters(
            ObjectParameter[] parameters, EdmFunction functionImport, EntityCommand command)
        {
            // attach entity parameters
            for (var i = 0; i < parameters.Length; i++)
            {
                var objectParameter = parameters[i];
                var entityParameter = new EntityParameter();

                var functionParameter = FindParameterMetadata(functionImport, parameters, i);

                if (null != functionParameter)
                {
                    entityParameter.Direction = MetadataHelper.ParameterModeToParameterDirection(
                        functionParameter.Mode);
                    entityParameter.ParameterName = functionParameter.Name;
                }
                else
                {
                    entityParameter.ParameterName = objectParameter.Name;
                }

                entityParameter.Value = objectParameter.Value ?? DBNull.Value;

                if (DBNull.Value == entityParameter.Value
                    || entityParameter.Direction != ParameterDirection.Input)
                {
                    TypeUsage typeUsage;
                    if (functionParameter != null)
                    {
                        // give precedence to the statically declared type usage
                        typeUsage = functionParameter.TypeUsage;
                    }
                    else if (null == objectParameter.TypeUsage)
                    {
                        Debug.Assert(objectParameter.MappableType != null, "MappableType must not be null");
                        Debug.Assert(Nullable.GetUnderlyingType(objectParameter.MappableType) == null, "Nullable types not expected here.");

                        // since ObjectParameters do not allow users to especify 'facets', make 
                        // sure that the parameter typeusage is not populated with the provider
                        // dafault facet values.
                        // Try getting the type from the workspace. This may fail however for one of the following reasons:
                        // - the type is not a model type
                        // - the types were not loaded into the workspace yet
                        // If the types were not loaded into the workspace we try loading types from the assembly the type lives in and re-try
                        // loading the type. We don't care if the type still cannot be loaded - in this case the result TypeUsage will be null
                        // which we handle later.
                        if (!Perspective.TryGetTypeByName(
                            objectParameter.MappableType.FullNameWithNesting(), /*ignoreCase */ false, out typeUsage))
                        {
                            MetadataWorkspace.ImplicitLoadAssemblyForType(objectParameter.MappableType, null);
                            Perspective.TryGetTypeByName(
                                objectParameter.MappableType.FullNameWithNesting(), /*ignoreCase */ false, out typeUsage);
                        }
                    }
                    else
                    {
                        typeUsage = objectParameter.TypeUsage;
                    }

                    // set type information (if the provider cannot determine it from the actual value)
                    EntityCommandDefinition.PopulateParameterFromTypeUsage(
                        entityParameter, typeUsage, entityParameter.Direction != ParameterDirection.Input);
                }

                if (entityParameter.Direction
                    != ParameterDirection.Input)
                {
                    var binder = new ParameterBinder(entityParameter, objectParameter);
                    command.OnDataReaderClosing += binder.OnDataReaderClosingHandler;
                }

                command.Parameters.Add(entityParameter);
            }
        }

        private static FunctionParameter FindParameterMetadata(EdmFunction functionImport, ObjectParameter[] parameters, int ordinal)
        {
            // Retrieve parameter information from functionImport.
            // We first attempt to resolve by case-sensitive name. If there is no exact match,
            // check if there is a case-insensitive match. Case insensitive matches are only permitted
            // when a single parameter would match.
            FunctionParameter functionParameter;
            var parameterName = parameters[ordinal].Name;
            if (!functionImport.Parameters.TryGetValue(parameterName, false, out functionParameter))
            {
                // if only one parameter has this name, try a case-insensitive lookup
                var matchCount = 0;
                for (var i = 0; i < parameters.Length && matchCount < 2; i++)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(parameters[i].Name, parameterName))
                    {
                        matchCount++;
                    }
                }

                if (matchCount == 1)
                {
                    functionImport.Parameters.TryGetValue(parameterName, true, out functionParameter);
                }
            }

            return functionParameter;
        }

        /// <summary>Generates an equivalent type that can be used with the Entity Framework for each type in the supplied enumeration.</summary>
        /// <param name="types">
        /// An enumeration of <see cref="T:System.Type" /> objects that represent custom data classes that map to the conceptual model.
        /// </param>
        public virtual void CreateProxyTypes(IEnumerable<Type> types)
        {
            var ospaceItems = (ObjectItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.OSpace);

            // Ensure metadata is loaded for each type,
            // and attempt to create proxy type only for types that have a mapping to an O-Space EntityType.
            EntityProxyFactory.TryCreateProxyTypes(
                types.Select(
                    type =>
                    {
                        // Ensure the assembly containing the entity's CLR type is loaded into the workspace.
                        MetadataWorkspace.ImplicitLoadAssemblyForType(type, null);

                        EntityType entityType;
                        ospaceItems.TryGetItem(type.FullNameWithNesting(), out entityType);
                        return entityType;
                    }).Where(entityType => entityType != null),
                MetadataWorkspace
                );
        }

        /// <summary>Returns all the existing proxy types.</summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of all the existing proxy types.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static IEnumerable<Type> GetKnownProxyTypes()
        {
            return EntityProxyFactory.GetKnownProxyTypes();
        }

        /// <summary>Returns the entity type of the POCO entity associated with a proxy object of a specified type.</summary>
        /// <returns>
        /// The <see cref="T:System.Type" /> of the associated POCO entity.
        /// </returns>
        /// <param name="type">
        /// The <see cref="T:System.Type" /> of the proxy object.
        /// </param>
        public static Type GetObjectType(Type type)
        {
            Check.NotNull(type, "type");

            return EntityProxyFactory.IsProxyType(type) ? type.BaseType() : type;
        }

        /// <summary>Creates and returns an instance of the requested type .</summary>
        /// <returns>An instance of the requested type  T , or an instance of a derived type that enables  T  to be used with the Entity Framework. The returned object is either an instance of the requested type or an instance of a derived type that enables the requested type to be used with the Entity Framework.</returns>
        /// <typeparam name="T">Type of object to be returned.</typeparam>
        public virtual T CreateObject<T>()
            where T : class
        {
            T instance = null;
            var clrType = typeof(T);

            // Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            MetadataWorkspace.ImplicitLoadAssemblyForType(clrType, null);

            // Retrieve the OSpace EntityType that corresponds to the supplied CLR type.
            // This call ensure that this mapping exists.
            var entityType = MetadataWorkspace.GetItem<ClrEntityType>(clrType.FullNameWithNesting(), DataSpace.OSpace);
            EntityProxyTypeInfo proxyTypeInfo = null;

            if (ContextOptions.ProxyCreationEnabled
                && ((proxyTypeInfo = EntityProxyFactory.GetProxyType(entityType, MetadataWorkspace)) != null))
            {
                instance = (T)proxyTypeInfo.CreateProxyObject();

                // After creating the proxy we need to add additional state to the proxy such
                // that it is able to function correctly when returned.  In particular, it needs
                // an initialized set of RelatedEnd objects because it will not be possible to
                // create these for convention based mapping once the metadata in the context has
                // been lost.
                var wrappedEntity = EntityWrapperFactory.CreateNewWrapper(instance, null);
                wrappedEntity.InitializingProxyRelatedEnds = true;
                try
                {
                    // We're setting the context temporarily here so that we can go through the process
                    // of creating RelatedEnds even with convention-based mapping.
                    // However, we also need to tell the wrapper that we're doing this so that we don't
                    // try to do things that we normally do when we have a context, such as adding the
                    // context to the RelatedEnds.  We can't do these things since they require an
                    // EntitySet, and, because of MEST, we don't have one.
                    wrappedEntity.AttachContext(this, null, MergeOption.NoTracking);
                    proxyTypeInfo.SetEntityWrapper(wrappedEntity);
                    if (proxyTypeInfo.InitializeEntityCollections != null)
                    {
                        proxyTypeInfo.InitializeEntityCollections.Invoke(null, new object[] { wrappedEntity });
                    }
                }
                finally
                {
                    wrappedEntity.InitializingProxyRelatedEnds = false;
                    wrappedEntity.DetachContext();
                }
            }
            else
            {
                instance = DelegateFactory.GetConstructorDelegateForType(entityType)() as T;
            }

            return instance;
        }

        /// <summary>
        /// Executes an arbitrary command directly against the data source using the existing connection.
        /// The command is specified using the server's native query language, such as SQL.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// If there isn't an existing local transaction a new transaction will be used
        /// to execute the command.
        /// </remarks>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="parameters"> The parameter values to use for the query. </param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteStoreCommand(string commandText, params object[] parameters)
        {
            return ExecuteStoreCommand(
                _options.EnsureTransactionsForFunctionsAndCommands ? TransactionalBehavior.EnsureTransaction : TransactionalBehavior.DoNotEnsureTransaction,
                commandText,
                parameters);
        }

        /// <summary>
        /// Executes an arbitrary command directly against the data source using the existing connection.
        /// The command is specified using the server's native query language, such as SQL.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreCommand("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <param name="transactionalBehavior"> Controls the creation of a transaction for this command. </param>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="parameters"> The parameter values to use for the query. </param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteStoreCommand(TransactionalBehavior transactionalBehavior, string commandText, params object[] parameters)
        {
            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
            AsyncMonitor.EnsureNotEntered();

            return executionStrategy.Execute(
                () => ExecuteInTransaction(
                    () => ExecuteStoreCommandInternal(commandText, parameters),
                    executionStrategy,
                    startLocalTransaction: transactionalBehavior != TransactionalBehavior.DoNotEnsureTransaction,
                    releaseConnectionOnSuccess: true));
        }

        private int ExecuteStoreCommandInternal(string commandText, object[] parameters)
        {
            var command = CreateStoreCommand(commandText, parameters);
            try
            {
                return command.ExecuteNonQuery();
            }
            finally
            {
                command.Parameters.Clear();
                command.Dispose();
            }
        }

#if !NET40

        /// <summary>
        /// Asynchronously executes an arbitrary command directly against the data source using the existing connection.
        /// The command is specified using the server's native query language, such as SQL.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// 
        /// If there isn't an existing local transaction a new transaction will be used
        /// to execute the command.
        /// </remarks>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="parameters"> The parameter values to use for the query. </param>
        /// <returns>
        ///  A task that represents the asynchronous operation.
        /// The task result contains the number of rows affected.
        /// </returns>
        public Task<int> ExecuteStoreCommandAsync(string commandText, params object[] parameters)
        {
            return ExecuteStoreCommandAsync(
                _options.EnsureTransactionsForFunctionsAndCommands ? TransactionalBehavior.EnsureTransaction : TransactionalBehavior.DoNotEnsureTransaction,
                commandText,
                CancellationToken.None,
                parameters);
        }

        /// <summary>
        /// Asynchronously executes an arbitrary command directly against the data source using the existing connection.
        /// The command is specified using the server's native query language, such as SQL.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="transactionalBehavior"> Controls the creation of a transaction for this command. </param>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="parameters"> The parameter values to use for the query. </param>
        /// <returns>
        ///  A task that represents the asynchronous operation.
        /// The task result contains the number of rows affected.
        /// </returns>
        public Task<int> ExecuteStoreCommandAsync(TransactionalBehavior transactionalBehavior, string commandText, params object[] parameters)
        {
            return ExecuteStoreCommandAsync(transactionalBehavior, commandText, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Asynchronously executes an arbitrary command directly against the data source using the existing connection.
        /// The command is specified using the server's native query language, such as SQL.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// 
        /// If there isn't an existing local transaction a new transaction will be used
        /// to execute the command.
        /// </remarks>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="parameters"> The parameter values to use for the query. </param>
        /// <returns>
        ///  A task that represents the asynchronous operation.
        /// The task result contains the number of rows affected.
        /// </returns>
        public virtual Task<int> ExecuteStoreCommandAsync(
            string commandText, CancellationToken cancellationToken, params object[] parameters)
        {
            return ExecuteStoreCommandAsync(
                _options.EnsureTransactionsForFunctionsAndCommands ? TransactionalBehavior.EnsureTransaction : TransactionalBehavior.DoNotEnsureTransaction,
                commandText,
                cancellationToken,
                parameters);
        }

        /// <summary>
        /// Asynchronously executes an arbitrary command directly against the data source using the existing connection.
        /// The command is specified using the server's native query language, such as SQL.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreCommandAsync("UPDATE dbo.Posts SET Rating = 5 WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="transactionalBehavior"> Controls the creation of a transaction for this command. </param>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="parameters"> The parameter values to use for the query. </param>
        /// <returns>
        ///  A task that represents the asynchronous operation.
        /// The task result contains the number of rows affected.
        /// </returns>
        public virtual Task<int> ExecuteStoreCommandAsync(
            TransactionalBehavior transactionalBehavior, string commandText, CancellationToken cancellationToken, params object[] parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AsyncMonitor.EnsureNotEntered();
            return ExecuteStoreCommandInternalAsync(transactionalBehavior, commandText, cancellationToken, parameters);
        }

        private async Task<int> ExecuteStoreCommandInternalAsync(
            TransactionalBehavior transactionalBehavior, string commandText, CancellationToken cancellationToken, params object[] parameters)
        {
            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
            AsyncMonitor.Enter();

            try
            {
                return await executionStrategy.ExecuteAsync(
                    () => ExecuteInTransactionAsync(
                        () => ExecuteStoreCommandInternalAsync(commandText, cancellationToken, parameters),
                        executionStrategy,
                        /*startLocalTransaction:*/ transactionalBehavior != TransactionalBehavior.DoNotEnsureTransaction,
                        /*releaseConnectionOnSuccess:*/ true, cancellationToken),
                    cancellationToken).WithCurrentCulture();
            }
            finally
            {
                AsyncMonitor.Exit();
            }
        }

        private async Task<int> ExecuteStoreCommandInternalAsync(string commandText, CancellationToken cancellationToken, object[] parameters)
        {
            var command = CreateStoreCommand(commandText, parameters);
            try
            {
                return await command.ExecuteNonQueryAsync(cancellationToken).WithCurrentCulture();
            }
            finally
            {
                command.Parameters.Clear();
                command.Dispose();
            }
        }

#endif

        /// <summary>
        /// Executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// Results are not tracked by the context, use the overload that specifies an entity set name to track results.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// An enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        public virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, params object[] parameters)
        {
            return ExecuteStoreQueryReliably<TElement>(
                commandText, /*entitySetName:*/null, ExecutionOptions.Default, parameters);
        }

        /// <summary>
        /// Executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// Results are not tracked by the context, use the overload that specifies an entity set name to track results.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="executionOptions"> The options for executing this query. </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior of 
        /// DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// An enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        public virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(
            string commandText, ExecutionOptions executionOptions, params object[] parameters)
        {
            return ExecuteStoreQueryReliably<TElement>(
                commandText, /*entitySetName:*/null, executionOptions, parameters);
        }

        /// <summary>
        /// Executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// If an entity set name is specified, results are tracked by the context.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="entitySetName">The entity set of the  TResult  type. If an entity set name is not provided, the results are not going to be tracked.</param>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when executing the query. The default is
        /// <see cref="F:System.Data.Entity.Core.Objects.MergeOption.AppendOnly" />.
        /// </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// An enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        public virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(
            string commandText, string entitySetName, MergeOption mergeOption, params object[] parameters)
        {
            Check.NotEmpty(entitySetName, "entitySetName");
            return ExecuteStoreQueryReliably<TElement>(
                commandText, entitySetName, new ExecutionOptions(mergeOption), parameters);
        }

        /// <summary>
        /// Executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// If an entity set name is specified, results are tracked by the context.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQuery&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="entitySetName">The entity set of the  TResult  type. If an entity set name is not provided, the results are not going to be tracked.</param>
        /// <param name="executionOptions"> The options for executing this query. </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// An enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        public virtual ObjectResult<TElement> ExecuteStoreQuery<TElement>(
            string commandText, string entitySetName, ExecutionOptions executionOptions, params object[] parameters)
        {
            Check.NotEmpty(entitySetName, "entitySetName");
            return ExecuteStoreQueryReliably<TElement>(commandText, entitySetName, executionOptions, parameters);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Buffer disposed by the returned ObjectResult")]
        private ObjectResult<TElement> ExecuteStoreQueryReliably<TElement>(
            string commandText, string entitySetName, ExecutionOptions executionOptions, params object[] parameters)
        {
            AsyncMonitor.EnsureNotEntered();

            // Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <TElement>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), Assembly.GetCallingAssembly());

            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);

            if (executionStrategy.RetriesOnFailure
                && executionOptions.UserSpecifiedStreaming.HasValue && executionOptions.UserSpecifiedStreaming.Value)
            {
                throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
            }

            if (!executionOptions.UserSpecifiedStreaming.HasValue)
            {
                executionOptions = new ExecutionOptions(executionOptions.MergeOption, !executionStrategy.RetriesOnFailure);
            }

            return executionStrategy.Execute(
                () => ExecuteInTransaction(
                    () => ExecuteStoreQueryInternal<TElement>(
                        commandText, entitySetName, executionOptions, parameters),
                    executionStrategy, startLocalTransaction: false,
                    releaseConnectionOnSuccess: !executionOptions.UserSpecifiedStreaming.Value));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by ObjectResult")]
        private ObjectResult<TElement> ExecuteStoreQueryInternal<TElement>(
            string commandText, string entitySetName, ExecutionOptions executionOptions, params object[] parameters)
        {
            DbDataReader reader = null;
            DbCommand command = null;
            EntitySet entitySet;
            TypeUsage edmType;
            ShaperFactory<TElement> shaperFactory;
            try
            {
                command = CreateStoreCommand(commandText, parameters);
                reader = command.ExecuteReader(
                    executionOptions.UserSpecifiedStreaming.Value
                        ? CommandBehavior.Default
                        : CommandBehavior.SequentialAccess);

                shaperFactory = InternalTranslate<TElement>(
                    reader, entitySetName, executionOptions.MergeOption, executionOptions.UserSpecifiedStreaming.Value, out entitySet,
                    out edmType);
            }
            catch
            {
                // We only release the connection and dispose the reader when there is an exception.
                // Otherwise, the ObjectResult is in charge of doing it.
                if (reader != null)
                {
                    reader.Dispose();
                }

                if (command != null)
                {
                    // We need to clear the parameters
                    // from the command in case we need to retry it
                    // to avoid getting the Sql parameter is contained in a collection error
                    command.Parameters.Clear();
                    command.Dispose();
                }

                throw;
            }

            if (!executionOptions.UserSpecifiedStreaming.Value)
            {
                BufferedDataReader bufferedReader = null;
                try
                {
                    var storeItemCollection = (StoreItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                    var providerServices = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);

                    bufferedReader = new BufferedDataReader(reader);
                    bufferedReader.Initialize(storeItemCollection.ProviderManifestToken, providerServices, shaperFactory.ColumnTypes, shaperFactory.NullableColumns);
                    reader = bufferedReader;
                }
                catch
                {
                    if (bufferedReader != null)
                    {
                        bufferedReader.Dispose();
                    }

                    throw;
                }
            }

            return ShapeResult(
                reader, 
                executionOptions.MergeOption, 
                /*readerOwned:*/ true, 
                executionOptions.UserSpecifiedStreaming.Value, 
                shaperFactory, 
                entitySet, 
                edmType, 
                command);
        }

#if !NET40

        /// <summary>
        /// Asynchronously executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// Results are not tracked by the context, use the overload that specifies an entity set name to track results.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText, params object[] parameters)
        {
            return ExecuteStoreQueryAsync<TElement>(commandText, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Asynchronously executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// Results are not tracked by the context, use the overload that specifies an entity set name to track results.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(
            string commandText, CancellationToken cancellationToken, params object[] parameters)
        {
            AsyncMonitor.EnsureNotEntered();

            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);

            return ExecuteStoreQueryReliablyAsync<TElement>(
                commandText, /*entitySetName:*/null, ExecutionOptions.Default, cancellationToken, executionStrategy, parameters);
        }

        /// <summary>
        /// Asynchronously executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// Results are not tracked by the context, use the overload that specifies an entity set name to track results.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="executionOptions"> The options for executing this query. </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(
            string commandText, ExecutionOptions executionOptions, params object[] parameters)
        {
            return ExecuteStoreQueryAsync<TElement>(
                commandText, executionOptions, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Asynchronously executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// Results are not tracked by the context, use the overload that specifies an entity set name to track results.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="executionOptions"> The options for executing this query. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(
            string commandText, ExecutionOptions executionOptions, CancellationToken cancellationToken, params object[] parameters)
        {
            AsyncMonitor.EnsureNotEntered();

            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
            if (executionStrategy.RetriesOnFailure
                && executionOptions.UserSpecifiedStreaming.HasValue && executionOptions.UserSpecifiedStreaming.Value)
            {
                throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
            }

            return ExecuteStoreQueryReliablyAsync<TElement>(
                commandText, /*entitySetName:*/null, executionOptions, cancellationToken, executionStrategy, parameters);
        }

        /// <summary>
        /// Asynchronously executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// If an entity set name is specified, results are tracked by the context.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="entitySetName">The entity set of the  TResult  type. If an entity set name is not provided, the results are not going to be tracked.</param>
        /// <param name="executionOptions"> The options for executing this query. </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(
            string commandText, string entitySetName, ExecutionOptions executionOptions, params object[] parameters)
        {
            return ExecuteStoreQueryAsync<TElement>(commandText, entitySetName, executionOptions, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Asynchronously executes a query directly against the data source and returns a sequence of typed results. 
        /// The query is specified using the server's native query language, such as SQL.
        /// If an entity set name is specified, results are tracked by the context.
        ///
        /// As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @p0", userSuppliedAuthor);
        /// Alternatively, you can also construct a DbParameter and supply it to SqlQuery. This allows you to use named parameters in the SQL query string.
        /// context.ExecuteStoreQueryAsync&lt;Post&gt;("SELECT * FROM dbo.Posts WHERE Author = @author", new SqlParameter("@author", userSuppliedAuthor));
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TElement"> The element type of the result sequence. </typeparam>
        /// <param name="commandText"> The query specified in the server's native query language. </param>
        /// <param name="entitySetName">The entity set of the  TResult  type. If an entity set name is not provided, the results are not going to be tracked.</param>
        /// <param name="executionOptions"> The options for executing this query. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="parameters"> 
        /// The parameter values to use for the query. If output parameters are used, their values will not be 
        /// available until the results have been read completely. This is due to the underlying behavior 
        /// of DbDataReader, see http://go.microsoft.com/fwlink/?LinkID=398589 for more details.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an enumeration of objects of type <typeparamref name="TElement" /> .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(
            string commandText, string entitySetName, ExecutionOptions executionOptions, CancellationToken cancellationToken,
            params object[] parameters)
        {
            Check.NotEmpty(entitySetName, "entitySetName");
            AsyncMonitor.EnsureNotEntered();

            var executionStrategy = DbProviderServices.GetExecutionStrategy(Connection, MetadataWorkspace);
            if (executionStrategy.RetriesOnFailure
                && executionOptions.UserSpecifiedStreaming.HasValue && executionOptions.UserSpecifiedStreaming.Value)
            {
                throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
            }

            return ExecuteStoreQueryReliablyAsync<TElement>(
                commandText, entitySetName, executionOptions, cancellationToken, executionStrategy, parameters);
        }

        private async Task<ObjectResult<TElement>> ExecuteStoreQueryReliablyAsync<TElement>(
            string commandText, string entitySetName, ExecutionOptions executionOptions, CancellationToken cancellationToken,
            IDbExecutionStrategy executionStrategy, params object[] parameters)
        {
            if (executionOptions.MergeOption != MergeOption.NoTracking)
            {
                AsyncMonitor.Enter();
            }

            try
            {
                // Ensure the assembly containing the entity's CLR type
                // is loaded into the workspace. If the schema types are not loaded
                // metadata, cache & query would be unable to reason about the type. We
                // either auto-load <TElement>'s assembly into the ObjectItemCollection or we
                // auto-load the user's calling assembly and its referenced assemblies.
                // If the entities in the user's result spans multiple assemblies, the
                // user must manually call LoadFromAssembly. *GetCallingAssembly returns
                // the assembly of the method that invoked the currently executing method.
                MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), Assembly.GetCallingAssembly());
                if (!executionOptions.UserSpecifiedStreaming.HasValue)
                {
                    executionOptions = new ExecutionOptions(executionOptions.MergeOption, !executionStrategy.RetriesOnFailure);
                }

                return await executionStrategy.ExecuteAsync(
                    () => ExecuteInTransactionAsync(
                        () => ExecuteStoreQueryInternalAsync<TElement>(
                            commandText, entitySetName, executionOptions, cancellationToken, parameters),
                        executionStrategy,
                        /*startLocalTransaction:*/ false, /*releaseConnectionOnSuccess:*/ !executionOptions.UserSpecifiedStreaming.Value,
                        cancellationToken),
                    cancellationToken).WithCurrentCulture();
            }
            finally
            {
                if (executionOptions.MergeOption != MergeOption.NoTracking)
                {
                    AsyncMonitor.Exit();
                }
            }
        }

        private async Task<ObjectResult<TElement>> ExecuteStoreQueryInternalAsync<TElement>(
            string commandText, string entitySetName, ExecutionOptions executionOptions,
            CancellationToken cancellationToken, params object[] parameters)
        {
            DbDataReader reader = null;
            DbCommand command = null;
            EntitySet entitySet;
            TypeUsage edmType;
            ShaperFactory<TElement> shaperFactory;
            try
            {
                command = CreateStoreCommand(commandText, parameters);
                reader = await command.ExecuteReaderAsync(
                    executionOptions.UserSpecifiedStreaming.Value
                        ? CommandBehavior.Default
                        : CommandBehavior.SequentialAccess,
                    cancellationToken).WithCurrentCulture();

                shaperFactory = InternalTranslate<TElement>(
                    reader, entitySetName, executionOptions.MergeOption, executionOptions.UserSpecifiedStreaming.Value, out entitySet,
                    out edmType);
            }
            catch
            {
                // We only release the connection and dispose the reader when there is an exception.
                // Otherwise, the ObjectResult is in charge of doing it.
                if (reader != null)
                {
                    reader.Dispose();
                }

                if (command != null)
                {
                    // We need to clear the parameters
                    // from the command in case we need to retry it
                    // to avoid getting the Sql parameter is contained in a collection error
                    command.Parameters.Clear();
                    command.Dispose();
                }

                throw;
            }

            if (!executionOptions.UserSpecifiedStreaming.Value)
            {
                BufferedDataReader bufferedReader = null;
                try
                {
                    var storeItemCollection = (StoreItemCollection)MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                    var providerServices = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);

                    bufferedReader = new BufferedDataReader(reader);
                    await bufferedReader.InitializeAsync(storeItemCollection.ProviderManifestToken, providerServices, shaperFactory.ColumnTypes, shaperFactory.NullableColumns, cancellationToken)
                        .WithCurrentCulture();
                    reader = bufferedReader;
                }
                catch
                {
                    if (bufferedReader != null)
                    {
                        bufferedReader.Dispose();
                    }

                    throw;
                }
            }

            return ShapeResult(
                reader,
                executionOptions.MergeOption, 
                /*readerOwned:*/ true, 
                executionOptions.UserSpecifiedStreaming.Value, 
                shaperFactory, 
                entitySet, 
                edmType,
                command);
        }

#endif

        /// <summary>
        /// Translates a <see cref="T:System.Data.Common.DbDataReader" /> that contains rows of entity data to objects of the requested entity type.
        /// </summary>
        /// <typeparam name="TElement">The entity type.</typeparam>
        /// <returns>An enumeration of objects of type  TResult .</returns>
        /// <param name="reader">
        /// The <see cref="T:System.Data.Common.DbDataReader" /> that contains entity data to translate into entity objects.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">When  reader  is null.</exception>
        public virtual ObjectResult<TElement> Translate<TElement>(DbDataReader reader)
        {
            // Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <TElement>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), Assembly.GetCallingAssembly());

            EntitySet entitySet;
            TypeUsage edmType;
            var shaperFactory = InternalTranslate<TElement>(
                reader, /*entitySetName:*/ null, MergeOption.AppendOnly, /*streaming:*/ false, out entitySet, out edmType);
            return ShapeResult(
                reader, MergeOption.AppendOnly, /*readerOwned:*/ false, /*streaming:*/ false, shaperFactory, entitySet, edmType);
        }

        /// <summary>
        /// Translates a <see cref="T:System.Data.Common.DbDataReader" /> that contains rows of entity data to objects of the requested entity type, in a specific entity set, and with the specified merge option.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <returns>An enumeration of objects of type  TResult .</returns>
        /// <param name="reader">
        /// The <see cref="T:System.Data.Common.DbDataReader" /> that contains entity data to translate into entity objects.
        /// </param>
        /// <param name="entitySetName">The entity set of the  TResult  type.</param>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when translated objects are added to the object context. The default is
        /// <see
        ///     cref="F:System.Data.Entity.Core.Objects.MergeOption.AppendOnly" />
        /// .
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">When  reader  is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// When the supplied  mergeOption  is not a valid <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> value.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">When the supplied  entitySetName  is not a valid entity set for the  TResult  type. </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Generic parameters are required for strong-typing of the return type.")]
        public virtual ObjectResult<TEntity> Translate<TEntity>(DbDataReader reader, string entitySetName, MergeOption mergeOption)
        {
            Check.NotEmpty(entitySetName, "entitySetName");

            // Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <TEntity>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TEntity), Assembly.GetCallingAssembly());

            EntitySet entitySet;
            TypeUsage edmType;
            var shaperFactory = InternalTranslate<TEntity>(
                reader, entitySetName, mergeOption, /*streaming:*/ false, out entitySet, out edmType);
            return ShapeResult(
                reader, mergeOption, /*readerOwned:*/ false, /*streaming:*/ false, shaperFactory, entitySet, edmType);
        }

        private ShaperFactory<TElement> InternalTranslate<TElement>(
            DbDataReader reader, string entitySetName, MergeOption mergeOption, bool streaming, out EntitySet entitySet, out TypeUsage edmType)
        {
            DebugCheck.NotNull(reader);
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            entitySet = null;
            if (!string.IsNullOrEmpty(entitySetName))
            {
                entitySet = GetEntitySetFromName(entitySetName);
            }

            // get the expected EDM type
            EdmType modelEdmType;
            var unwrappedTElement = Nullable.GetUnderlyingType(typeof(TElement)) ?? typeof(TElement);
            CollectionColumnMap columnMap;
            // for enums that are not in the model we use the enum underlying type
            if (MetadataWorkspace.TryDetermineCSpaceModelType<TElement>(out modelEdmType)
                || (unwrappedTElement.IsEnum() &&
                    MetadataWorkspace.TryDetermineCSpaceModelType(unwrappedTElement.GetEnumUnderlyingType(), out modelEdmType)))
            {
                if (entitySet != null
                    && !entitySet.ElementType.IsAssignableFrom(modelEdmType))
                {
                    throw new InvalidOperationException(
                        Strings.ObjectContext_InvalidEntitySetForStoreQuery(
                            entitySet.EntityContainer.Name,
                            entitySet.Name, typeof(TElement)));
                }

                columnMap = _columnMapFactory.CreateColumnMapFromReaderAndType(reader, modelEdmType, entitySet, null);
            }
            else
            {
                columnMap = _columnMapFactory.CreateColumnMapFromReaderAndClrType(reader, typeof(TElement), MetadataWorkspace);
            }

            edmType = columnMap.Type;

            // build a shaper for the column map to produce typed results
            return _translator.TranslateColumnMap<TElement>(columnMap, MetadataWorkspace, null, mergeOption, streaming, valueLayer: false);
        }

        private ObjectResult<TElement> ShapeResult<TElement>(
            DbDataReader reader, MergeOption mergeOption, bool readerOwned, bool streaming, ShaperFactory<TElement> shaperFactory, EntitySet entitySet,
            TypeUsage edmType, DbCommand command = null)
        {
            var shaper = shaperFactory.Create(
                reader, this, MetadataWorkspace, mergeOption, readerOwned, streaming);
            return new ObjectResult<TElement>(
                shaper, entitySet, MetadataHelper.GetElementType(edmType), readerOwned, streaming, command);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private DbCommand CreateStoreCommand(string commandText, params object[] parameters)
        {
            var command = ((EntityConnection)Connection).StoreConnection.CreateCommand();
            command.CommandText = commandText;

            // get relevant state from the object context
            if (CommandTimeout.HasValue)
            {
                command.CommandTimeout = CommandTimeout.Value;
            }

            var entityTransaction = ((EntityConnection)Connection).CurrentTransaction;
            if (null != entityTransaction)
            {
                command.Transaction = entityTransaction.StoreTransaction;
            }

            if (null != parameters
                && parameters.Length > 0)
            {
                var dbParameters = new DbParameter[parameters.Length];

                // three cases: all explicit DbParameters, no explicit DbParameters
                // or a mix of the two (throw in the last case)
                if (parameters.All(p => p is DbParameter))
                {
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        dbParameters[i] = (DbParameter)parameters[i];
                    }
                }
                else if (!parameters.Any(p => p is DbParameter))
                {
                    var parameterNames = new string[parameters.Length];
                    var parameterSql = new string[parameters.Length];
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        parameterNames[i] = string.Format(CultureInfo.InvariantCulture, "p{0}", i);
                        dbParameters[i] = command.CreateParameter();
                        dbParameters[i].ParameterName = parameterNames[i];
                        dbParameters[i].Value = parameters[i] ?? DBNull.Value;

                        // By default, we attempt to swap in a SQL Server friendly representation of the parameter.
                        // For other providers, users may write:
                        //
                        //      ExecuteStoreQuery("select * from xyz f where f.X = ?", 1);
                        //
                        // rather than:
                        //
                        //      ExecuteStoreQuery("select * from xyz f where f.X = {0}", 1);
                        parameterSql[i] = "@" + parameterNames[i];
                    }
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, command.CommandText, parameterSql);
                }
                else
                {
                    throw new InvalidOperationException(Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues);
                }

                command.Parameters.AddRange(dbParameters);
            }

            return new InterceptableDbCommand(command, InterceptionContext);
        }

        /// <summary>
        /// Creates the database by using the current data source connection and the metadata in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.StoreItemCollection" />
        /// .
        /// </summary>
        public virtual void CreateDatabase()
        {
            var storeConnection = ((EntityConnection)Connection).StoreConnection;
            var services = GetStoreItemCollection().ProviderFactory.GetProviderServices();
            services.CreateDatabase(storeConnection, CommandTimeout, GetStoreItemCollection());
        }

        /// <summary>Deletes the database that is specified as the database in the current data source connection.</summary>
        public virtual void DeleteDatabase()
        {
            var storeConnection = ((EntityConnection)Connection).StoreConnection;
            var services = GetStoreItemCollection().ProviderFactory.GetProviderServices();
            services.DeleteDatabase(storeConnection, CommandTimeout, GetStoreItemCollection());
        }

        /// <summary>
        /// Checks if the database that is specified as the database in the current store connection exists on the store. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        /// <returns>true if the database exists; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual bool DatabaseExists()
        {
            var storeConnection = ((EntityConnection)Connection).StoreConnection;
            var services = GetStoreItemCollection().ProviderFactory.GetProviderServices();
            try
            {
                return services.DatabaseExists(storeConnection, CommandTimeout, GetStoreItemCollection());
            }
            catch (Exception)
            {
                // In situations where the user does not have access to the master database
                // the above DatabaseExists call fails and throws an exception.  Rather than
                // just let that exception escape to the caller we instead try a different
                // approach to see if the database really does exist or not.  The approach
                // is to try to open a connection to the database.  If this succeeds then
                // we know that the database exists.  If it fails then the database may
                // not exist or there may be some other issue connecting to it.  In either
                // case for the purpose of this call we assume that it does not exist and
                // return false since this functionally gives the best experience in most
                // scenarios.
                if (Connection.State == ConnectionState.Open)
                {
                    return true;
                }
                try
                {
                    Connection.Open();
                    return true;
                }
                catch (EntityException)
                {
                    return false;
                }
                finally
                {
                    Connection.Close();
                }
            }
        }

        private StoreItemCollection GetStoreItemCollection()
        {
            var entityConnection = (EntityConnection)Connection;
            // retrieve the item collection from the entity connection rather than the context since:
            // a) it forces creation of the metadata workspace if it's not already there
            // b) the store item collection isn't guaranteed to exist on the context.MetadataWorkspace
            return (StoreItemCollection)entityConnection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
        }

        /// <summary>
        /// Generates a data definition language (DDL) script that creates schema objects (tables, primary keys, foreign keys) for the metadata in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.StoreItemCollection" />
        /// . The
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.StoreItemCollection" />
        /// loads metadata from store schema definition language (SSDL) files.
        /// </summary>
        /// <returns>
        /// A DDL script that creates schema objects for the metadata in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.StoreItemCollection" />
        /// .
        /// </returns>
        public virtual String CreateDatabaseScript()
        {
            var services = GetStoreItemCollection().ProviderFactory.GetProviderServices();
            var targetProviderManifestToken = GetStoreItemCollection().ProviderManifestToken;
            return services.CreateDatabaseScript(targetProviderManifestToken, GetStoreItemCollection());
        }

        // <summary>
        // Attempts to retrieve an DbGeneratedViewCacheTypeAttribute specified at assembly level,
        // that associates the type of the context with an mapping view cache type. If one is found
        // this method initializes the mapping view cache factory for this context with a new 
        // instance of DefaultDbMappingViewCacheFactory.
        // </summary>
        // <param name="owner">A DbContext that owns this ObjectContext.</param>
        internal void InitializeMappingViewCacheFactory(DbContext owner = null)
        {
            var itemCollection = (StorageMappingItemCollection)
                MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

            if (itemCollection == null)
            {
                return;
            }

            var contextType = owner != null ? owner.GetType() : GetType();

            _contextTypesWithViewCacheInitialized.GetOrAdd(contextType, (t) =>
            {
                var attributes = t.Assembly().GetCustomAttributes<DbMappingViewCacheTypeAttribute>().Where(a => a.ContextType == t);

                var attributeCount = attributes.Count();
                if (attributeCount > 1)
                {
                    throw new InvalidOperationException(
                        Strings.DbMappingViewCacheTypeAttribute_MultipleInstancesWithSameContextType(t));
                }

                if (attributeCount == 1)
                {
                    itemCollection.MappingViewCacheFactory
                        = new DefaultDbMappingViewCacheFactory(attributes.First().CacheType);
                }

                return true;
            });
        }

        #endregion //Methods

        #region Nested types

        // <summary>
        // Supports binding EntityClient parameters to Object Services parameters.
        // </summary>
        private class ParameterBinder
        {
            private readonly EntityParameter _entityParameter;
            private readonly ObjectParameter _objectParameter;

            internal ParameterBinder(EntityParameter entityParameter, ObjectParameter objectParameter)
            {
                _entityParameter = entityParameter;
                _objectParameter = objectParameter;
            }

            internal void OnDataReaderClosingHandler(object sender, EventArgs args)
            {
                // When the reader is closing, out/inout parameter values are set on the EntityParameter
                // instance. Pass this value through to the corresponding ObjectParameter.
                if (_entityParameter.Value != DBNull.Value
                    && _objectParameter.MappableType.IsEnum())
                {
                    _objectParameter.Value = Enum.ToObject(_objectParameter.MappableType, _entityParameter.Value);
                }
                else
                {
                    _objectParameter.Value = _entityParameter.Value;
                }
            }
        }

        #endregion
    }
}
