namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// ObjectContext is the top-level object that encapsulates a connection between the CLR and the database,
    /// serving as a gateway for Create, Read, Update, and Delete operations.
    /// </summary>
    public class ObjectContext : IDisposable
    {
        private bool _disposed = false;
        private InternalObjectContext _internalObjectContext;

        private EventHandler _onSavingChanges;
        private ObjectMaterializedEventHandler _onObjectMaterialized;

        /// <summary>
        /// Creates an ObjectContext with the given connection and metadata workspace.
        /// </summary>
        /// <param name="connection">connection to the store</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        public ObjectContext(EntityConnection connection)
            : this(new InternalObjectContext(connection))
        {
        }

        /// <summary>
        /// Creates an ObjectContext with the given connection string and
        /// default entity container name.  This constructor
        /// creates and initializes an EntityConnection so that the context is
        /// ready to use; no other initialization is necessary.  The given
        /// connection string must be valid for an EntityConnection; connection
        /// strings for other connection types are not supported.
        /// </summary>
        /// <param name="connectionString">the connection string to use in the underlying EntityConnection to the store</param>
        /// <exception cref="ArgumentNullException">connectionString is null</exception>
        /// <exception cref="ArgumentException">if connectionString is invalid</exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For CreateEntityConnection method. But the paths are not created in this method.
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        public ObjectContext(string connectionString)
            : this(new InternalObjectContext(connectionString))
        {
        }

        /// <summary>
        /// Creates an ObjectContext with the given connection string and
        /// default entity container name.  This protected constructor creates and initializes an EntityConnection so that the context 
        /// is ready to use; no other initialization is necessary.  The given connection string must be valid for an EntityConnection; 
        /// connection strings for other connection types are not supported.
        /// </summary>
        /// <param name="connectionString">the connection string to use in the underlying EntityConnection to the store</param>
        /// <param name="defaultContainerName">the name of the default entity container</param>
        /// <exception cref="ArgumentNullException">connectionString is null</exception>
        /// <exception cref="ArgumentException">either connectionString or defaultContainerName is invalid</exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For ObjectContext method. But the paths are not created in this method.
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        protected ObjectContext(string connectionString, string defaultContainerName)
            : this(new InternalObjectContext(connectionString, defaultContainerName))
        {
        }

        /// <summary>
        /// Creates an ObjectContext with the given connection and metadata workspace.
        /// </summary>
        /// <param name="connection">connection to the store</param>
        /// <param name="defaultContainerName">the name of the default entity container</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        protected ObjectContext(EntityConnection connection, string defaultContainerName)
            : this(new InternalObjectContext(connection, defaultContainerName))
        {
        }

        internal ObjectContext(InternalObjectContext internalObjectContext)
        {
            _internalObjectContext = internalObjectContext;
            _internalObjectContext.ObjectContextWrapper = this;
        }

        #region Properties

        /// <summary>
        /// Gets the connection to the store.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the <see cref="ObjectContext"/> instance has been disposed.</exception>
        public DbConnection Connection
        {
            get { return _internalObjectContext.Connection; }
        }

        /// <summary>
        /// Gets or sets the default container name.
        /// </summary>
        public string DefaultContainerName
        {
            get { return _internalObjectContext.DefaultContainerName; }
            set { _internalObjectContext.DefaultContainerName = value; }
        }

        /// <summary>
        /// Gets the metadata workspace associated with this ObjectContext.
        /// </summary>
        [CLSCompliant(false)]
        public MetadataWorkspace MetadataWorkspace
        {
            get { return _internalObjectContext.MetadataWorkspace; }
        }

        /// <summary>
        /// Gets the ObjectStateManager used by this ObjectContext.
        /// </summary>
        public ObjectStateManager ObjectStateManager
        {
            get { return _internalObjectContext.ObjectStateManager; }
        }

        /// <summary>
        /// ClrPerspective based on the MetadataWorkspace.
        /// </summary>
        internal ClrPerspective Perspective
        {
            get { return _internalObjectContext.Perspective; }
        }

        /// <summary>
        /// Gets and sets the timeout value used for queries with this ObjectContext.
        /// A null value indicates that the default value of the underlying provider
        /// will be used.
        /// </summary>
        public int? CommandTimeout
        {
            get { return _internalObjectContext.CommandTimeout; }
            set { _internalObjectContext.CommandTimeout = value; }
        }

        /// <summary>
        /// Gets the LINQ query provider associated with this object context.
        /// </summary>
        protected internal IQueryProvider QueryProvider
        {
            get { return _internalObjectContext.QueryProvider; }
        }

        /// <summary>
        /// Whether or not we are in the middle of materialization
        /// Used to suppress operations such as lazy loading that are not allowed during materialization
        /// </summary>
        internal bool InMaterialization
        {
            get { return _internalObjectContext.InMaterialization; }
            set { _internalObjectContext.InMaterialization = value; }
        }

        /// <summary>
        /// Get <see cref="ObjectContextOptions"/> instance that contains options 
        /// that affect the behavior of the ObjectContext.
        /// </summary>
        /// <value>
        /// Instance of <see cref="ObjectContextOptions"/> for the current ObjectContext.
        /// This value will never be null.
        /// </value>
        public ObjectContextOptions ContextOptions
        {
            get { return _internalObjectContext.ContextOptions; }
        }

        internal CollectionColumnMap ColumnMapBuilder
        {
            get { return _internalObjectContext.ColumnMapBuilder; }
            set { _internalObjectContext.ColumnMapBuilder = value; }
        }

        internal EntityWrapperFactory EntityWrapperFactory
        {
            get { return _internalObjectContext.EntityWrapperFactory; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Property for adding a delegate to the SavingChanges Event.
        /// </summary>
        public event EventHandler SavingChanges
        {
            add { _onSavingChanges += value; }
            remove { _onSavingChanges -= value; }
        }

        /// <summary>
        /// A private helper function for the _savingChanges/SavingChanges event.
        /// </summary>
        internal void InternalOnSavingChanges()
        {
            if (null != _onSavingChanges)
            {
                _onSavingChanges(this, new EventArgs());
            }
        }

        /// <summary>
        /// Event raised when a new entity object is materialized.  That is, the event is raised when
        /// a new entity object is created from data in the store as part of a query or load operation.
        /// </summary>
        /// <remarks>
        /// Note that the event is raised after included (spanned) referenced objects are loaded, but
        /// before included (spanned) collections are loaded.  Also, for independent associations,
        /// any stub entities for related objects that have not been loaded will also be created before
        /// the event is raised.
        /// 
        /// It is possible for an entity object to be created and then thrown away if it is determined
        /// that an entity with the same ID already exists in the Context.  This event is not raised
        /// in those cases.
        /// </remarks>
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

        /// <summary>
        /// Returns true if any handlers for the ObjectMaterialized event exist.  This is
        /// used for perf reasons to avoid collecting the information needed for the event
        /// if there is no point in firing it.
        /// </summary>
        internal bool OnMaterializedHasHandlers
        {
            get { return _onObjectMaterialized != null && _onObjectMaterialized.GetInvocationList().Length != 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// AcceptChanges on all associated entries in the ObjectStateManager so their resultant state is either unchanged or detached.
        /// </summary>
        /// <returns></returns>
        public void AcceptAllChanges()
        {
            _internalObjectContext.AcceptAllChanges();
        }

        /// <summary>
        /// Adds an object to the cache.  If it doesn't already have an entity key, the
        /// entity set is determined based on the type and the O-C map.
        /// If the object supports relationships (i.e. it implements IEntityWithRelationships),
        /// this also sets the context onto its RelationshipManager object.
        /// </summary>
        /// <param name="entitySetName">entitySetName the Object to be added. It might be qualifed with container name </param>
        /// <param name="entity">Object to be added.</param>
        public void AddObject(string entitySetName, object entity)
        {
            Contract.Requires(entity != null);

            _internalObjectContext.AddObject(entitySetName, entity);
        }

        /// <summary>
        /// Adds an object to the cache without adding its related
        /// entities.
        /// </summary>
        /// <param name="entity">Object to be added.</param>
        /// <param name="setName">EntitySet name for the Object to be added. It may be qualified with container name</param>
        /// <param name="containerName">Container name for the Object to be added.</param>
        /// <param name="argumentName">Name of the argument passed to a public method, for use in exceptions.</param>
        internal void AddSingleObject(EntitySet entitySet, IEntityWrapper wrappedEntity, string argumentName)
        {
            Contract.Requires(entitySet != null);
            Contract.Requires(wrappedEntity != null);
            Contract.Requires(wrappedEntity.Entity != null);

            _internalObjectContext.AddSingleObject(entitySet, wrappedEntity, argumentName);
        }

        /// <summary>
        /// Explicitly loads a referenced entity or collection of entities into the given entity.
        /// </summary>
        /// <remarks>
        /// After loading, the referenced entity or collection can be accessed through the properties
        /// of the source entity.
        /// </remarks>
        /// <param name="entity">The source entity on which the relationship is defined</param>
        /// <param name="navigationProperty">The name of the property to load</param>
        public void LoadProperty(object entity, string navigationProperty)
        {
            _internalObjectContext.LoadProperty(entity, navigationProperty);
        }

        /// <summary>
        /// Explicitly loads a referenced entity or collection of entities into the given entity.
        /// </summary>
        /// <remarks>
        /// After loading, the referenced entity or collection can be accessed through the properties
        /// of the source entity.
        /// </remarks>
        /// <param name="entity">The source entity on which the relationship is defined</param>
        /// <param name="navigationProperty">The name of the property to load</param>
        /// <param name="mergeOption">The merge option to use for the load</param>
        public void LoadProperty(object entity, string navigationProperty, MergeOption mergeOption)
        {
            _internalObjectContext.LoadProperty(entity, navigationProperty, mergeOption);
        }

        /// <summary>
        /// Explicitly loads a referenced entity or collection of entities into the given entity.
        /// </summary>
        /// <remarks>
        /// After loading, the referenced entity or collection can be accessed through the properties
        /// of the source entity.
        /// The property to load is specified by a LINQ expression which must be in the form of
        /// a simple property member access.  For example, <code>(entity) => entity.PropertyName</code>
        /// where PropertyName is the navigation property to be loaded.  Other expression forms will
        /// be rejected at runtime.
        /// </remarks>
        /// <param name="entity">The source entity on which the relationship is defined</param>
        /// <param name="selector">A LINQ expression specifying the property to load</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector)
        {
            _internalObjectContext.LoadProperty<TEntity>(entity, selector);
        }

        /// <summary>
        /// Explicitly loads a referenced entity or collection of entities into the given entity.
        /// </summary>
        /// <remarks>
        /// After loading, the referenced entity or collection can be accessed through the properties
        /// of the source entity.
        /// The property to load is specified by a LINQ expression which must be in the form of
        /// a simple property member access.  For example, <code>(entity) => entity.PropertyName</code>
        /// where PropertyName is the navigation property to be loaded.  Other expression forms will
        /// be rejected at runtime.
        /// </remarks>
        /// <param name="entity">The source entity on which the relationship is defined</param>
        /// <param name="selector">A LINQ expression specifying the property to load</param>
        /// <param name="mergeOption">The merge option to use for the load</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void LoadProperty<TEntity>(TEntity entity, Expression<Func<TEntity, object>> selector, MergeOption mergeOption)
        {
            _internalObjectContext.LoadProperty<TEntity>(entity, selector, mergeOption);
        }

        // Validates that the given property selector may represent a navigation property and returns the nav prop string.
        // The actual check that the navigation property is valid is performed by the
        // RelationshipManager while loading the RelatedEnd.
        internal static string ParsePropertySelectorExpression<TEntity>(Expression<Func<TEntity, object>> selector, out bool removedConvert)
        {
            Contract.Requires(selector != null);

            // We used to throw an ArgumentException if the expression contained a Convert.  Now we remove the convert,
            // but if we still need to throw, then we should still throw an ArgumentException to avoid a breaking change.
            // Therefore, we keep track of whether or not we removed the convert.
            removedConvert = false;
            var body = selector.Body;
            while (body.NodeType == ExpressionType.Convert || body.NodeType == ExpressionType.ConvertChecked)
            {
                removedConvert = true;
                body = ((UnaryExpression)body).Operand;
            }

            var bodyAsMember = body as MemberExpression;
            if (bodyAsMember == null ||
                !bodyAsMember.Member.DeclaringType.IsAssignableFrom(typeof(TEntity)) ||
                bodyAsMember.Expression.NodeType != ExpressionType.Parameter)
            {
                throw new ArgumentException(Strings.ObjectContext_SelectorExpressionMustBeMemberAccess);
            }

            return bodyAsMember.Member.Name;
        }

        /// <summary>
        /// Apply modified properties to the original object.
        /// This API is obsolete.  Please use ApplyCurrentValues instead.
        /// </summary>
        /// <param name="entitySetName">name of EntitySet of entity to be updated</param>
        /// <param name="changed">object with modified properties</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [Obsolete("Use ApplyCurrentValues instead")]
        public void ApplyPropertyChanges(string entitySetName, object changed)
        {
            Contract.Requires(changed != null);
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");

            ApplyCurrentValues(entitySetName, changed);
        }

        /// <summary>
        /// Apply modified properties to the original object.
        /// </summary>
        /// <param name="entitySetName">name of EntitySet of entity to be updated</param>
        /// <param name="currentEntity">object with modified properties</param>
        public TEntity ApplyCurrentValues<TEntity>(string entitySetName, TEntity currentEntity) where TEntity : class
        {
            Contract.Requires(currentEntity != null);
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");

            return _internalObjectContext.ApplyCurrentValues<TEntity>(entitySetName, currentEntity);
        }

        /// <summary>
        /// Apply original values to the entity.
        /// The entity to update is found based on key values of the <paramref name="originalEntity"/> entity and the given <paramref name="entitySetName"/>.
        /// </summary>
        /// <param name="entitySetName">Name of EntitySet of entity to be updated.</param>
        /// <param name="originalEntity">Object with original values.</param>
        /// <returns>Updated entity.</returns>
        public TEntity ApplyOriginalValues<TEntity>(string entitySetName, TEntity originalEntity) where TEntity : class
        {
            Contract.Requires(originalEntity != null);

            return _internalObjectContext.ApplyOriginalValues<TEntity>(entitySetName, originalEntity);
        }

        /// <summary>
        /// Attach entity graph into the context in the Unchanged state.
        /// This version takes entity which doesn't have to have a Key.
        /// </summary>
        /// <param name="entitySetName">EntitySet name for the Object to be attached. It may be qualified with container name.</param>        
        /// <param name="entity">The entity to be attached.</param>
        public void AttachTo(string entitySetName, object entity)
        {
            Contract.Requires(entity != null);

            _internalObjectContext.AttachTo(entitySetName, entity);
        }

        /// <summary>
        /// Attach entity graph into the context in the Unchanged state.
        /// This version takes entity which does have to have a non-temporary Key.
        /// </summary>
        /// <param name="entity">The entity to be attached.</param>        
        public void Attach(IEntityWithKey entity)
        {
            Contract.Requires(entity != null);

            if (null == (object)entity.EntityKey)
            {
                throw new InvalidOperationException(Strings.ObjectContext_CannotAttachEntityWithoutKey);
            }

            AttachTo(null, entity);
        }

        /// <summary>
        /// Attaches single object to the cache without adding its related entities.
        /// </summary>
        /// <param name="entity">Entity to be attached.</param>
        /// <param name="entitySet">"Computed" entity set.</param>
        internal void AttachSingleObject(IEntityWrapper wrappedEntity, EntitySet entitySet)
        {
            Contract.Requires(wrappedEntity != null);
            Contract.Requires(wrappedEntity.Entity != null);
            Contract.Requires(entitySet != null);

            _internalObjectContext.AttachSingleObject(wrappedEntity, entitySet);
        }

        /// <summary>
        /// Create an entity key based on given entity set and values of given entity.
        /// </summary>
        /// <param name="entitySetName">Entity set for the entity.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>New instance of <see cref="EntityKey"/> for the provided <paramref name="entity"/>.</returns>
        public EntityKey CreateEntityKey(string entitySetName, object entity)
        {
            Contract.Requires(entity != null);
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");

            return _internalObjectContext.CreateEntityKey(entitySetName, entity);
        }

        internal EntitySet GetEntitySetFromName(string entitySetName)
        {
            return _internalObjectContext.GetEntitySetFromName(entitySetName);
        }

        /// <summary>
        /// Creates an ObjectSet based on the EntitySet that is defined for TEntity.
        /// Requires that the DefaultContainerName is set for the context and that there is a
        /// single EntitySet for the specified type. Throws exception if more than one type is found.
        /// </summary>
        /// <typeparam name="TEntity">Entity type for the requested ObjectSet</typeparam>
        public ObjectSet<TEntity> CreateObjectSet<TEntity>()
            where TEntity : class
        {
            return _internalObjectContext.CreateObjectSet<TEntity>();
        }

        /// <summary>
        /// Creates an ObjectSet based on the specified EntitySet name.
        /// </summary>
        /// <typeparam name="TEntity">Expected type of the EntitySet</typeparam>
        /// <param name="entitySetName">
        /// EntitySet to use for the ObjectSet. Can be fully-qualified or unqualified if the DefaultContainerName is set.
        /// </param>
        public ObjectSet<TEntity> CreateObjectSet<TEntity>(string entitySetName)
            where TEntity : class
        {
            return _internalObjectContext.CreateObjectSet<TEntity>(entitySetName);
        }

        #region Connection Management

        /// <summary>
        /// Ensures that the connection is opened for an operation that requires an open connection to the store.
        /// Calls to EnsureConnection MUST be matched with a single call to ReleaseConnection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the <see cref="ObjectContext"/> instance has been disposed.</exception>
        internal void EnsureConnection()
        {
            _internalObjectContext.EnsureConnection();
        }

        /// <summary>
        /// Releases the connection, potentially closing the connection if no active operations
        /// require the connection to be open. There should be a single ReleaseConnection call
        /// for each EnsureConnection call.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the <see cref="ObjectContext"/> instance has been disposed.</exception>
        internal void ReleaseConnection()
        {
            _internalObjectContext.ReleaseConnection();
        }

        internal void EnsureMetadata()
        {
            _internalObjectContext.EnsureMetadata();
        }

        #endregion

        /// <summary>
        /// Creates an ObjectQuery<typeparamref name="T"/> over the store, ready to be executed.
        /// </summary>
        /// <typeparam name="T">type of the query result</typeparam>
        /// <param name="queryString">the query string to be executed</param>
        /// <param name="parameters">parameters to pass to the query</param>
        /// <returns>an ObjectQuery instance, ready to be executed</returns>
        public ObjectQuery<T> CreateQuery<T>(string queryString, params ObjectParameter[] parameters)
        {
            Contract.Requires(queryString != null);
            Contract.Requires(parameters != null);

            return _internalObjectContext.CreateQuery<T>(queryString, parameters);
        }

        /// <summary>
        /// Marks an object for deletion from the cache.
        /// </summary>
        /// <param name="entity">Object to be deleted.</param>
        public void DeleteObject(object entity)
        {
            _internalObjectContext.DeleteObject(entity);
        }

        /// <summary>
        /// Common DeleteObject method that is used by both ObjectContext.DeleteObject and ObjectSet.DeleteObject.
        /// </summary>
        /// <param name="entity">Object to be deleted.</param>
        /// <param name="expectedEntitySet">
        /// EntitySet that the specified object is expected to be in. Null if the caller doesn't want to validate against a particular EntitySet.
        /// </param>
        internal void DeleteObject(object entity, EntitySet expectedEntitySet)
        {
            Contract.Requires(entity != null);
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            _internalObjectContext.DeleteObject(entity, expectedEntitySet);
        }

        /// <summary>
        /// Detach entity from the cache.
        /// </summary>
        /// <param name="entity">Object to be detached.</param>
        public void Detach(object entity)
        {
            _internalObjectContext.Detach(entity);
        }

        /// <summary>
        /// Common Detach method that is used by both ObjectContext.Detach and ObjectSet.Detach.
        /// </summary>
        /// <param name="entity">Object to be detached.</param>
        /// <param name="expectedEntitySet">
        /// EntitySet that the specified object is expected to be in. Null if the caller doesn't want to validate against a particular EntitySet.
        /// </param>        
        internal void Detach(object entity, EntitySet expectedEntitySet)
        {
            Contract.Requires(entity != null);
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            _internalObjectContext.Detach(entity, expectedEntitySet);
        }

        /// <summary>
        /// Disposes this ObjectContext.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this ObjectContext.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _internalObjectContext.Dispose();
                }

                _disposed = true;
            }
        }

        #region GetEntitySet

        /// <summary>
        /// Returns the EntitySet with the given name from given container.
        /// </summary>
        /// <param name="entitySetName">Name of entity set.</param>
        /// <param name="entityContainerName">Name of container.</param>
        /// <returns>The appropriate EntitySet.</returns>
        /// <exception cref="InvalidOperationException">The entity set could not be found for the given name.</exception>
        /// <exception cref="InvalidOperationException">The entity container could not be found for the given name.</exception>
        internal EntitySet GetEntitySet(string entitySetName, string entityContainerName)
        {
            Contract.Requires(entitySetName != null);

            return _internalObjectContext.GetEntitySet(entitySetName, entityContainerName);
        }

        internal TypeUsage GetTypeUsage(Type entityCLRType)
        {
            return _internalObjectContext.GetTypeUsage(entityCLRType);
        }

        #endregion

        /// <summary>
        /// Retrieves an object from the cache if present or from the
        /// store if not.
        /// </summary>
        /// <param name="key">Key of the object to be found.</param>
        /// <returns>Entity object.</returns>
        public object GetObjectByKey(EntityKey key)
        {
            Contract.Requires(key != null);

            return _internalObjectContext.GetObjectByKey(key);
        }

        #region Refresh

        /// <summary>
        /// Refreshing cache data with store data for specific entities.
        /// The order in which entites are refreshed is non-deterministic.
        /// </summary>
        /// <param name="refreshMode">Determines how the entity retrieved from the store is merged with the entity in the cache</param>
        /// <param name="collection">must not be null and all entities must be attached to this context. May be empty.</param>
        /// <exception cref="ArgumentOutOfRangeException">if refreshMode is not valid</exception>
        /// <exception cref="ArgumentNullException">collection is null</exception>
        /// <exception cref="ArgumentException">collection contains null or non entities or entities not attached to this context</exception>
        public void Refresh(RefreshMode refreshMode, IEnumerable collection)
        {
            Contract.Requires(collection != null);

            _internalObjectContext.Refresh(refreshMode, collection);
        }

        /// <summary>
        /// Refreshing cache data with store data for a specific entity.
        /// </summary>
        /// <param name="refreshMode">Determines how the entity retrieved from the store is merged with the entity in the cache</param>
        /// <param name="entity">The entity to refresh. This must be a non-null entity that is attached to this context</param>
        /// <exception cref="ArgumentOutOfRangeException">if refreshMode is not valid</exception>
        /// <exception cref="ArgumentNullException">entity is null</exception>
        /// <exception cref="ArgumentException">entity is not attached to this context</exception>
        public void Refresh(RefreshMode refreshMode, object entity)
        {
            Contract.Requires(entity != null);
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            _internalObjectContext.Refresh(refreshMode, entity);
        }

        #endregion

        #region SaveChanges

        /// <summary>
        /// Persists all updates to the store.
        /// </summary>
        /// <returns>
        /// The number of dirty (i.e., Added, Modified, or Deleted) ObjectStateEntries
        /// in the ObjectStateManager when SaveChanges was called.
        /// </returns>
        public Int32 SaveChanges()
        {
            return SaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave);
        }

        /// <summary>
        /// An asynchronous version of SaveChanges, which
        /// persists all updates to the store.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<Int32> SaveChangesAsync()
        {
            return SaveChangesAsync(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, CancellationToken.None);
        }

        /// <summary>
        /// An asynchronous version of SaveChanges, which
        /// persists all updates to the store.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<Int32> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return SaveChangesAsync(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, cancellationToken);
        }

        /// <summary>
        /// Persists all updates to the store.
        /// This API is obsolete.  Please use SaveChanges(SaveOptions options) instead.
        /// SaveChanges(true) is equivalent to SaveChanges() -- That is it detects changes and
        /// accepts all changes after save.
        /// SaveChanges(false) detects changes but does not accept changes after save.
        /// </summary>
        /// <param name="acceptChangesDuringSave">if false, user must call AcceptAllChanges</param>/>
        /// <returns>
        /// The number of dirty (i.e., Added, Modified, or Deleted) ObjectStateEntries
        /// in the ObjectStateManager when SaveChanges was called.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [Obsolete("Use SaveChanges(SaveOptions options) instead.")]
        public Int32 SaveChanges(bool acceptChangesDuringSave)
        {
            return SaveChanges(
                acceptChangesDuringSave
                    ? SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave
                    : SaveOptions.DetectChangesBeforeSave);
        }

        /// <summary>
        /// Persists all updates to the store.
        /// </summary>
        /// <param name="options">Describes behavior options of SaveChanges</param>
        /// <returns>
        /// The number of dirty (i.e., Added, Modified, or Deleted) ObjectStateEntries
        /// in the ObjectStateManager processed by SaveChanges.
        /// </returns>
        public virtual Int32 SaveChanges(SaveOptions options)
        {
            return _internalObjectContext.SaveChanges(options);
        }

        /// <summary>
        /// An asynchronous version of SaveChanges, which
        /// persists all updates to the store.
        /// </summary>
        /// <param name="options">Describes behavior options of SaveChanges</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task<Int32> SaveChangesAsync(SaveOptions options)
        {
            return SaveChangesAsync(options, CancellationToken.None);
        }

        /// <summary>
        /// An asynchronous version of SaveChanges, which
        /// persists all updates to the store.
        /// </summary>
        /// <param name="options">Describes behavior options of SaveChanges</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public virtual Task<Int32> SaveChangesAsync(SaveOptions options, CancellationToken cancellationToken)
        {
            return _internalObjectContext.SaveChangesAsync(options, cancellationToken);
        }

        #endregion

        /// <summary>
        /// For every tracked entity which doesn't implement IEntityWithChangeTracker detect changes in the entity's property values
        /// and marks appropriate ObjectStateEntry as Modified.
        /// For every tracked entity which doesn't implement IEntityWithRelationships detect changes in its relationships.
        /// 
        /// The method is used interanally by ObjectContext.SaveChanges() but can be also used if user wants to detect changes 
        /// and have ObjectStateEntries in appropriate state before the SaveChanges() method is called.
        /// </summary>
        public void DetectChanges()
        {
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
            ObjectStateManager.DetectChanges();
            ObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        /// <summary>
        /// Attempts to retrieve an object from the cache or the store.
        /// </summary>
        /// <param name="key">Key of the object to be found.</param>
        /// <param name="value">Out param for the object.</param>
        /// <returns>True if the object was found, false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public bool TryGetObjectByKey(EntityKey key, out object value)
        {
            return _internalObjectContext.TryGetObjectByKey(key, out value);
        }

        /// <summary>
        /// Executes the given function on the default container. 
        /// </summary>
        /// <typeparam name="TElement">Element type for function results.</typeparam>
        /// <param name="functionName">Name of function. May include container (e.g. ContainerName.FunctionName)
        /// or just function name when DefaultContainerName is known.</param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentException">If function is null or empty</exception>
        /// <exception cref="InvalidOperationException">If function is invalid (syntax,
        /// does not exist, refers to a function with return type incompatible with T)</exception>
        public ObjectResult<TElement> ExecuteFunction<TElement>(string functionName, params ObjectParameter[] parameters)
        {
            Contract.Requires(parameters != null);
            EntityUtil.CheckStringArgument(functionName, "function");

            return _internalObjectContext.ExecuteFunction<TElement>(functionName, MergeOption.AppendOnly, parameters);
        }

        /// <summary>
        /// Executes the given function on the default container. 
        /// </summary>
        /// <typeparam name="TElement">Element type for function results.</typeparam>
        /// <param name="functionName">Name of function. May include container (e.g. ContainerName.FunctionName)
        /// or just function name when DefaultContainerName is known.</param>
        /// <param name="mergeOption"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentException">If function is null or empty</exception>
        /// <exception cref="InvalidOperationException">If function is invalid (syntax,
        /// does not exist, refers to a function with return type incompatible with T)</exception>
        public ObjectResult<TElement> ExecuteFunction<TElement>(
            string functionName, MergeOption mergeOption, params ObjectParameter[] parameters)
        {
            Contract.Requires(parameters != null);
            EntityUtil.CheckStringArgument(functionName, "function");

            return _internalObjectContext.ExecuteFunction<TElement>(functionName, mergeOption, parameters);
        }

        /// <summary>
        /// Executes the given function on the default container and discard any results returned from the function.
        /// </summary>
        /// <param name="functionName">Name of function. May include container (e.g. ContainerName.FunctionName)
        /// or just function name when DefaultContainerName is known.</param>
        /// <param name="parameters"></param>
        /// <returns>Number of rows affected</returns>
        /// <exception cref="ArgumentException">If function is null or empty</exception>
        /// <exception cref="InvalidOperationException">If function is invalid (syntax,
        /// does not exist, refers to a function with return type incompatible with T)</exception>
        public int ExecuteFunction(string functionName, params ObjectParameter[] parameters)
        {
            Contract.Requires(parameters != null);
            EntityUtil.CheckStringArgument(functionName, "function");

            return _internalObjectContext.ExecuteFunction(functionName, parameters);
        }

        /// <summary>
        ///  Get the materializer for the resultSetIndexth result set of storeReader.
        /// </summary>
        internal ObjectResult<TElement> MaterializedDataRecord<TElement>(
            EntityCommand entityCommand,
            DbDataReader storeReader,
            int resultSetIndex,
            ReadOnlyMetadataCollection<EntitySet> entitySets,
            EdmType[] edmTypes,
            MergeOption mergeOption)
        {
            return _internalObjectContext.MaterializedDataRecord<TElement>(entityCommand, storeReader, resultSetIndex, entitySets, edmTypes, mergeOption);
        }

        /// <summary>
        /// Attempt to generate a proxy type for each type in the supplied enumeration.
        /// </summary>
        /// <param name="types">
        /// Enumeration of Type objects that should correspond to O-Space types.
        /// </param>
        /// <remarks>
        /// Types in the enumeration that do not map to an O-Space type are ignored.
        /// Also, there is no guarantee that a proxy type will be created for a given type,
        /// only that if a proxy can be generated, then it will be generated.
        /// 
        /// See <see cref="EntityProxyFactory"/> class for more information about proxy type generation.
        /// </remarks>
        // Use one of the following methods to retrieve an enumeration of all CLR types mapped to O-Space EntityType objects:
        // TODO: This could be tricky, as we're forcing the user to ensure OSpace metadata is loaded.
        // This might justify an overload that takes no arguments, that does what is outlined in this example.
        // 
        // Method 1
        // ObjectItemCollection ospaceItems = // retrieve item collection, ensure it is loaded
        // var types = ospaceItems.GetItems<EntityType>().Select( entityType => ospaceItems.GetClrType(entityType) )
        //
        // Method 2
        // ObjectItemCollection ospaceItems = // retrieve item collection, ensure it is loaded
        // var types = from entityType in ospaceItems.GetItems<EntityType>() select ospaceItems.GetClrType(entityType)
        // TODO: List of names possibly better than CreateProxyTypes:
        // LoadEntityTypeMetadata (this disrupts the semantics of the sample methods above, since it implies we load metadata)
        public void CreateProxyTypes(IEnumerable<Type> types)
        {
            _internalObjectContext.CreateProxyTypes(types);
        }

        /// <summary>
        /// Return an enumerable of the current set of CLR proxy types.
        /// </summary>
        /// <returns>
        /// Enumerable of the current set of CLR proxy types.
        /// This will never be null.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static IEnumerable<Type> GetKnownProxyTypes()
        {
            return EntityProxyFactory.GetKnownProxyTypes();
        }

        /// <summary>
        /// Given a type that may represent a known proxy type, 
        /// return the corresponding type being proxied.
        /// </summary>
        /// <param name="type">Type that may represent a proxy type.</param>
        /// <returns>
        /// Non-proxy type that corresponds to the supplied proxy type,
        /// or the supplied type if it is not a known proxy type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the value of the type parameter is null.
        /// </exception
        public static Type GetObjectType(Type type)
        {
            Contract.Requires(type != null);

            return EntityProxyFactory.IsProxyType(type) ? type.BaseType : type;
        }

        /// <summary>
        /// Create an appropriate instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of object to be returned.
        /// </typeparam>
        /// <returns>
        /// An instance of an object of type <typeparamref name="T"/>.
        /// The object will either be an instance of the exact type <typeparamref name="T"/>,
        /// or possibly an instance of the proxy type that corresponds to <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        /// The type <typeparamref name="T"/> must have an OSpace EntityType representation.
        /// </remarks>
        public T CreateObject<T>()
            where T : class
        {
            return _internalObjectContext.CreateObject<T>();
        }

        /// <summary>
        /// Execute a command against the database server that does not return a sequence of objects.
        /// The command is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>A single integer return value</returns>
        public int ExecuteStoreCommand(string commandText, params object[] parameters)
        {
            return _internalObjectContext.ExecuteStoreCommand(commandText, parameters);
        }

        /// <summary>
        /// An asynchronous version of ExecuteStoreCommand, which
        /// executes a command against the database server that does not return a sequence of objects.
        /// The command is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>A Task containing a single integer return value.</returns>
        public Task<int> ExecuteStoreCommandAsync(string commandText, params object[] parameters)
        {
            return _internalObjectContext.ExecuteStoreCommandAsync(commandText, CancellationToken.None, parameters);
        }

        /// <summary>
        /// An asynchronous version of ExecuteStoreCommand, which
        /// executes a command against the database server that does not return a sequence of objects.
        /// The command is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <param name="commandText">The command specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing a single integer return value.</returns>
        public Task<int> ExecuteStoreCommandAsync(string commandText, CancellationToken cancellationToken, params object[] parameters)
        {
            return _internalObjectContext.ExecuteStoreCommandAsync(commandText, cancellationToken, parameters);
        }

        /// <summary>
        /// Execute the sequence returning query against the database server.
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TElement">The element type of the result sequence.</typeparam>
        /// <param name="commandText">The query specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>An enumeration of objects of type <typeparamref name="TElement"/>.</returns>
        public ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, params object[] parameters)
        {
            return _internalObjectContext.ExecuteStoreQuery<TElement>(commandText, /*entitySetName:*/null, MergeOption.AppendOnly, parameters);
        }

        /// <summary>
        /// Execute the sequence returning query against the database server. 
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TElement">The element type of the resulting sequence</typeparam>
        /// <param name="commandText">The DbDataReader to translate</param>
        /// <param name="entitySetName">The entity set in which results should be tracked. Null indicates there is no entity set.</param>
        /// <param name="mergeOption">Merge option to use for entity results.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>An enumeration of objects of type <typeparamref name="TElement"/>.</returns>
        public ObjectResult<TElement> ExecuteStoreQuery<TElement>(string commandText, string entitySetName,
            MergeOption mergeOption, params object[] parameters)
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");

            return _internalObjectContext.ExecuteStoreQuery<TElement>(commandText, entitySetName, mergeOption, parameters);
        }

        /// <summary>
        /// An asynchronous version of ExecuteStoreQuery, which
        /// executes the sequence returning query against the database server.
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TElement">The element type of the result sequence.</typeparam>
        /// <param name="commandText">The query specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>A Task containing an enumeration of objects of type <typeparamref name="TElement"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText, params object[] parameters)
        {
            return _internalObjectContext.ExecuteStoreQueryAsync<TElement>(commandText,
                /*entitySetName:*/null, MergeOption.AppendOnly, CancellationToken.None, parameters);
        }

        /// <summary>
        /// An asynchronous version of ExecuteStoreQuery, which
        /// executes the sequence returning query against the database server.
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TElement">The element type of the result sequence.</typeparam>
        /// <param name="commandText">The query specified in the server's native query language.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>A Task containing an enumeration of objects of type <typeparamref name="TElement"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText,
            CancellationToken cancellationToken, params object[] parameters)
        {
            return _internalObjectContext.ExecuteStoreQueryAsync<TElement>(commandText,
                /*entitySetName:*/null, MergeOption.AppendOnly, cancellationToken, parameters);
        }

        /// <summary>
        /// An asynchronous version of ExecuteStoreQuery, which
        /// execute the sequence returning query against the database server. 
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TElement">The element type of the resulting sequence</typeparam>
        /// <param name="commandText">The DbDataReader to translate</param>
        /// <param name="entitySetName">The entity set in which results should be tracked. Null indicates there is no entity set.</param>
        /// <param name="mergeOption">Merge option to use for entity results.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>A Task containing an enumeration of objects of type <typeparamref name="TElement"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText,
            string entitySetName, MergeOption mergeOption, params object[] parameters)
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");

            return _internalObjectContext.ExecuteStoreQueryAsync<TElement>(commandText,
                entitySetName, mergeOption, CancellationToken.None, parameters);
        }

        /// <summary>
        /// An asynchronous version of ExecuteStoreQuery, which
        /// execute the sequence returning query against the database server. 
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TElement">The element type of the resulting sequence</typeparam>
        /// <param name="commandText">The DbDataReader to translate</param>
        /// <param name="entitySetName">The entity set in which results should be tracked. Null indicates there is no entity set.</param>
        /// <param name="mergeOption">Merge option to use for entity results.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>A Task containing an enumeration of objects of type <typeparamref name="TElement"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<ObjectResult<TElement>> ExecuteStoreQueryAsync<TElement>(string commandText,
            string entitySetName, MergeOption mergeOption, CancellationToken cancellationToken, params object[] parameters)
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");

            return _internalObjectContext.ExecuteStoreQueryAsync<TElement>(commandText,
                entitySetName, mergeOption, cancellationToken, parameters);
        }

        /// <summary>
        /// Translates the data from a DbDataReader into sequence of objects.
        /// </summary>
        /// <typeparam name="TElement">The element type of the resulting sequence.</typeparam>
        /// <param name="reader">The DbDataReader to translate</param>
        /// <param name="mergeOption">Merge option to use for entity results.</param>
        /// <returns>The translated sequence of objects.</returns>
        public ObjectResult<TElement> Translate<TElement>(DbDataReader reader)
        {
            return _internalObjectContext.Translate<TElement>(reader);
        }

        /// <summary>
        /// Translates the data from a DbDataReader into sequence of entities.
        /// </summary>
        /// <typeparam name="TEntity">The element type of the resulting sequence</typeparam>
        /// <param name="reader">The DbDataReader to translate</param>
        /// <param name="entitySetName">The entity set in which results should be tracked. Null indicates there is no entity set.</param>
        /// <param name="mergeOption">Merge option to use for entity results.</param>
        /// <returns>The translated sequence of objects</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "cmeek: Generic parameters are required for strong-typing of the return type.")]
        public ObjectResult<TEntity> Translate<TEntity>(DbDataReader reader, string entitySetName, MergeOption mergeOption)
        {
            EntityUtil.CheckStringArgument(entitySetName, "entitySetName");

            return _internalObjectContext.Translate<TEntity>(reader, entitySetName, mergeOption);
        }

        /// <summary>
        /// Creates the database using the current store connection and the metadata in the StoreItemCollection. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        public void CreateDatabase()
        {
            _internalObjectContext.CreateDatabase();
        }

        /// <summary>
        /// Deletes the database that is specified as the database in the current store connection. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        public void DeleteDatabase()
        {
            _internalObjectContext.DeleteDatabase();
        }

        /// <summary>
        /// Checks if the database that is specified as the database in the current store connection exists on the store. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        public bool DatabaseExists()
        {
            return _internalObjectContext.DatabaseExists();
        }

        /// <summary>
        /// Creates the sql script that can be used to create the database for the metadata in the StoreItemCollection. Most of the actual work
        /// is done by the DbProviderServices implementation for the current store connection.
        /// </summary>
        public String CreateDatabaseScript()
        {
            return _internalObjectContext.CreateDatabaseScript();
        }

        #endregion
    }
}
