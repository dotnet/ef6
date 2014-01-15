// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class InternalSet<TEntity> : InternalQuery<TEntity>, IInternalSet<TEntity>
        where TEntity : class
    {
        #region Fields and constructors and initalization

        private DbLocalView<TEntity> _localView;
        private EntitySet _entitySet;
        private string _entitySetName;
        private string _quotedEntitySetName;
        private Type _baseType;

        // <summary>
        // Creates a new set that will be backed by the given InternalContext.
        // </summary>
        // <param name="internalContext"> The backing context. </param>
        public InternalSet(InternalContext internalContext)
            : base(internalContext)
        {
        }

        // <summary>
        // Resets the set to its uninitialized state so that it will be re-lazy initialized the next
        // time it is used.  This allows the ObjectContext backing a DbContext to be switched out.
        // </summary>
        public override void ResetQuery()
        {
            _entitySet = null;
            _localView = null;
            base.ResetQuery();
        }

        #endregion

        #region Find

        // <summary>
        // Finds an entity with the given primary key values.
        // If an entity with the given primary key values exists in the context, then it is
        // returned immediately without making a request to the store.  Otherwise, a request
        // is made to the store for an entity with the given primary key values and this entity,
        // if found, is attached to the context and returned.  If no entity is found in the
        // context or the store, then null is returned.
        // </summary>
        // <remarks>
        // The ordering of composite key values is as defined in the EDM, which is in turn as defined in
        // the designer, by the Code First fluent API, or by the DataMember attribute.
        // </remarks>
        // <param name="keyValues"> The values of the primary key for the entity to be found. </param>
        // <returns> The entity found, or null. </returns>
        // <exception cref="InvalidOperationException">Thrown if multiple entities exist in the context with the primary key values given.</exception>
        // <exception cref="InvalidOperationException">Thrown if the type of entity is not part of the data model for this context.</exception>
        // <exception cref="InvalidOperationException">Thrown if the types of the key values do not match the types of the key values for the entity type to be found.</exception>
        // <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        public TEntity Find(params object[] keyValues)
        {
            InternalContext.ObjectContext.AsyncMonitor.EnsureNotEntered();

            // This DetectChanges is useful in the case where objects are added to the graph and then the user
            // attempts to find one of those added objects.
            InternalContext.DetectChanges();

            var key = new WrappedEntityKey(EntitySet, EntitySetName, keyValues, "keyValues");

            // First, check for the entity in the state manager.  This includes first checking
            // for non-Added objects that match the key.  If the entity was not found, then
            // we check for Added objects.  We don't just use GetObjectByKey
            // because it would go to the store before checking for Added objects, and also
            // because if the object found was of the wrong type then it would still get into
            // the state manager.
            var entity = FindInStateManager(key) ?? FindInStore(key, "keyValues");

            if (entity != null
                && !(entity is TEntity))
            {
                throw Error.DbSet_WrongEntityTypeFound(entity.GetType().Name, typeof(TEntity).Name);
            }
            return (TEntity)entity;
        }

#if !NET40

        // <summary>
        // An asynchronous version of Find, which
        // finds an entity with the given primary key values.
        // If an entity with the given primary key values exists in the context, then it is
        // returned immediately without making a request to the store.  Otherwise, a request
        // is made to the store for an entity with the given primary key values and this entity,
        // if found, is attached to the context and returned.  If no entity is found in the
        // context or the store, then null is returned.
        // </summary>
        // <remarks>
        // The ordering of composite key values is as defined in the EDM, which is in turn as defined in
        // the designer, by the Code First fluent API, or by the DataMember attribute.
        // </remarks>
        // <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        // <param name="keyValues"> The values of the primary key for the entity to be found. </param>
        // <returns> A Task containing the entity found, or null. </returns>
        // <exception cref="InvalidOperationException">Thrown if multiple entities exist in the context with the primary key values given.</exception>
        // <exception cref="InvalidOperationException">Thrown if the type of entity is not part of the data model for this context.</exception>
        // <exception cref="InvalidOperationException">Thrown if the types of the key values do not match the types of the key values for the entity type to be found.</exception>
        // <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        public Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            cancellationToken.ThrowIfCancellationRequested();

            InternalContext.ObjectContext.AsyncMonitor.EnsureNotEntered();

            return FindInternalAsync(cancellationToken, keyValues);
        }

        private async Task<TEntity> FindInternalAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            // This DetectChanges is useful in the case where objects are added to the graph and then the user
            // attempts to find one of those added objects.
            InternalContext.DetectChanges();

            var key = new WrappedEntityKey(EntitySet, EntitySetName, keyValues, "keyValues");

            // First, check for the entity in the state manager.  This includes first checking
            // for non-Added objects that match the key.  If the entity was not found, then
            // we check for Added objects.  We don't just use GetObjectByKey
            // because it would go to the store before checking for Added objects, and also
            // because if the object found was of the wrong type then it would still get into
            // the state manager.
            var entity = FindInStateManager(key)
                         ?? await FindInStoreAsync(key, "keyValues", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            if (entity != null
                && !(entity is TEntity))
            {
                throw Error.DbSet_WrongEntityTypeFound(entity.GetType().Name, typeof(TEntity).Name);
            }
            return (TEntity)entity;
        }

#endif

        // <summary>
        // Finds an entity in the state manager with the given primary key values, or returns null
        // if no such entity can be found.  This includes looking for Added entities with the given
        // key values.
        // </summary>
        private object FindInStateManager(WrappedEntityKey key)
        {
            DebugCheck.NotNull(key);

            // If the key has null values, then it cannot be in the state manager in anything other
            // than the Added state and we cannot create an EntityKey for it, so skip the first check.
            if (!key.HasNullValues)
            {
                // First lookup non-added entries by key.  Added entries won't be found this way because they
                // have temp keys.
                ObjectStateEntry stateEntry;
                if (InternalContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(
                    key.EntityKey, out stateEntry))
                {
                    return stateEntry.Entity;
                }
            }

            // If we didn't find it that way, then look through all the Added entries.  In this case we
            // need to look at the key values in entity itself because temp keys don't contain any useful
            // information.
            object entity = null;
            foreach (
                var addedEntry in
                    from e in InternalContext.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added)
                    where !e.IsRelationship &&
                          e.Entity != null &&
                          EntitySetBaseType.IsAssignableFrom(e.Entity.GetType())
                    select e)
            {
                var match = true;
                // Note that key names from the entity set and CurrentValues are both c-space, so we don't need any mapping here.
                foreach (var keyProperty in key.KeyValuePairs)
                {
                    var ordinal = addedEntry.CurrentValues.GetOrdinal(keyProperty.Key);
                    if (!DbHelpers.KeyValuesEqual(keyProperty.Value, addedEntry.CurrentValues.GetValue(ordinal)))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    if (entity != null)
                    {
                        throw Error.DbSet_MultipleAddedEntitiesFound();
                    }
                    entity = addedEntry.Entity;
                }
            }

            // May still be null
            return entity;
        }

        // <summary>
        // Finds an entity in the store with the given primary key values, or returns null
        // if no such entity can be found.  This code is adapted from TryGetObjectByKey to
        // include type checking in the query.
        // </summary>
        private object FindInStore(WrappedEntityKey key, string keyValuesParamName)
        {
            DebugCheck.NotNull(key);

            // If the key has null values, then we cannot query it from the store, so it cannot
            // be found, so just return null.
            if (key.HasNullValues)
            {
                return null;
            }

            try
            {
                return BuildFindQuery(key).SingleOrDefault();
            }
            catch (EntitySqlException ex)
            {
                throw new ArgumentException(Strings.DbSet_WrongKeyValueType, keyValuesParamName, ex);
            }
        }

#if !NET40

        // <summary>
        // An asynchronous version of FindInStore, which
        // finds an entity in the store with the given primary key values, or returns null
        // if no such entity can be found.  This code is adapted from TryGetObjectByKey to
        // include type checking in the query.
        // </summary>
        private async Task<object> FindInStoreAsync(WrappedEntityKey key, string keyValuesParamName, CancellationToken cancellationToken)
        {
            DebugCheck.NotNull(key);

            // If the key has null values, then we cannot query it from the store, so it cannot
            // be found, so just return null.
            if (key.HasNullValues)
            {
                return null;
            }

            try
            {
                return await BuildFindQuery(key).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (EntitySqlException ex)
            {
                throw new ArgumentException(Strings.DbSet_WrongKeyValueType, keyValuesParamName, ex);
            }
        }

#endif

        private ObjectQuery<TEntity> BuildFindQuery(WrappedEntityKey key)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendFormat("SELECT VALUE X FROM {0} AS X WHERE ", QuotedEntitySetName);

            var entityKeyValues = key.EntityKey.EntityKeyValues;
            var parameters = new ObjectParameter[entityKeyValues.Length];

            for (var i = 0; i < entityKeyValues.Length; i++)
            {
                if (i > 0)
                {
                    queryBuilder.Append(" AND ");
                }

                var name = string.Format(CultureInfo.InvariantCulture, "p{0}", i.ToString(CultureInfo.InvariantCulture));
                queryBuilder.AppendFormat("X.{0} = @{1}", DbHelpers.QuoteIdentifier(entityKeyValues[i].Key), name);
                parameters[i] = new ObjectParameter(name, entityKeyValues[i].Value);
            }

            return InternalContext.ObjectContext.CreateQuery<TEntity>(queryBuilder.ToString(), parameters);
        }

        #endregion

        #region Data binding/local view

        // <summary>
        // Gets the ObservableCollection representing the local view for the set based on this query.
        // </summary>
        public ObservableCollection<TEntity> Local
        {
            get
            {
                InternalContext.DetectChanges();

                return _localView ?? (_localView = new DbLocalView<TEntity>(InternalContext));
            }
        }

        #endregion

        #region Attach/Add/Remove

        // <summary>
        // Attaches the given entity to the context underlying the set.  That is, the entity is placed
        // into the context in the Unchanged state, just as if it had been read from the database.
        // </summary>
        // <remarks>
        // Attach is used to repopulate a context with an entity that is known to already exist in the database.
        // SaveChanges will therefore not attempt to insert an attached entity into the database because
        // it is assumed to already be there.
        // Note that entities that are already in the context in some other state will have their state set
        // to Unchanged.  Attach is a no-op if the entity is already in the context in the Unchanged state.
        // This method is virtual so that it can be mocked.
        // </remarks>
        // <param name="entity"> The entity to attach. </param>
        public virtual void Attach(object entity)
        {
            DebugCheck.NotNull(entity);

            ActOnSet(
                () => InternalContext.ObjectContext.AttachTo(EntitySetName, entity), EntityState.Unchanged, entity,
                "Attach");
        }

        // <summary>
        // Adds the given entity to the context underlying the set in the Added state such that it will
        // be inserted into the database when SaveChanges is called.
        // </summary>
        // <remarks>
        // Note that entities that are already in the context in some other state will have their state set
        // to Added.  Add is a no-op if the entity is already in the context in the Added state.
        // This method is virtual so that it can be mocked.
        // </remarks>
        // <param name="entity"> The entity to add. </param>
        public virtual void Add(object entity)
        {
            DebugCheck.NotNull(entity);

            ActOnSet(
                () => InternalContext.ObjectContext.AddObject(EntitySetName, entity), EntityState.Added, entity, "Add");
        }

        public virtual void AddRange(IEnumerable entities)
        {
            DebugCheck.NotNull(entities);

            InternalContext.DetectChanges();

            ActOnSet(
                entity => InternalContext.ObjectContext.AddObject(EntitySetName, entity), EntityState.Added, entities, "AddRange");
        }

        // <summary>
        // Marks the given entity as Deleted such that it will be deleted from the database when SaveChanges
        // is called.  Note that the entity must exist in the context in some other state before this method
        // is called.
        // </summary>
        // <remarks>
        // Note that if the entity exists in the context in the Added state, then this method
        // will cause it to be detached from the context.  This is because an Added entity is assumed not to
        // exist in the database such that trying to delete it does not make sense.
        // This method is virtual so that it can be mocked.
        // </remarks>
        // <param name="entity"> The entity to remove. </param>
        public virtual void Remove(object entity)
        {
            DebugCheck.NotNull(entity);

            if (!(entity is TEntity))
            {
                throw Error.DbSet_BadTypeForAddAttachRemove("Remove", entity.GetType().Name, typeof(TEntity).Name);
            }

            InternalContext.DetectChanges();

            InternalContext.ObjectContext.DeleteObject(entity);
        }

        public virtual void RemoveRange(IEnumerable entities)
        {
            DebugCheck.NotNull(entities);

            // prevent "enumerator was changed" exception
            // if entities is syncronized with other elements
            // (e.g: local view from DbSet.Local.)
            var copyOfEntities = entities.Cast<object>().ToList();

            InternalContext.DetectChanges();

            foreach (var entity in copyOfEntities)
            {
                Check.NotNull(entity, "entity");

                if (!(entity is TEntity))
                {
                    throw Error.DbSet_BadTypeForAddAttachRemove("RemoveRange", entity.GetType().Name, typeof(TEntity).Name);
                }

                InternalContext.ObjectContext.DeleteObject(entity);
            }
        }

        // <summary>
        // This method checks whether an entity is already in the context.  If it is, then the state
        // is changed to the new state given.  If it isn't, then the action delegate is executed to
        // either Add or Attach the entity.
        // </summary>
        // <param name="action"> A delegate to Add or Attach the entity. </param>
        // <param name="newState"> The new state to give the entity if it is already in the context. </param>
        // <param name="entity"> The entity. </param>
        // <param name="methodName"> Name of the method. </param>
        private void ActOnSet(Action action, EntityState newState, object entity, string methodName)
        {
            DebugCheck.NotNull(entity);

            if (!(entity is TEntity))
            {
                throw Error.DbSet_BadTypeForAddAttachRemove(methodName, entity.GetType().Name, typeof(TEntity).Name);
            }

            InternalContext.DetectChanges();

            ObjectStateEntry stateEntry;
            if (InternalContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out stateEntry))
            {
                // Will be no-op if state is already newState.
                stateEntry.ChangeState(newState);
            }
            else
            {
                action();
            }
        }

        private void ActOnSet(Action<object> action, EntityState newState, IEnumerable entities, string methodName)
        {
            DebugCheck.NotNull(entities);

            foreach (var entity in entities)
            {
                Check.NotNull(entity, "entity");

                if (!(entity is TEntity))
                {
                    throw Error.DbSet_BadTypeForAddAttachRemove(methodName, entity.GetType().Name, typeof(TEntity).Name);
                }

                ObjectStateEntry stateEntry;
                if (InternalContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out stateEntry))
                {
                    // Will be no-op if state is already added.
                    stateEntry.ChangeState(newState);
                }
                else
                {
                    action(entity);
                }
            }
        }

        #endregion

        #region Create

        // <summary>
        // Creates a new instance of an entity for the type of this set.
        // Note that this instance is NOT added or attached to the set.
        // The instance returned will be a proxy if the underlying context is configured to create
        // proxies and the entity type meets the requirements for creating a proxy.
        // </summary>
        // <returns> The entity instance, which may be a proxy. </returns>
        public TEntity Create()
        {
            return InternalContext.CreateObject<TEntity>();
        }

        // <summary>
        // Creates a new instance of an entity for the type of this set or for a type derived
        // from the type of this set.
        // Note that this instance is NOT added or attached to the set.
        // The instance returned will be a proxy if the underlying context is configured to create
        // proxies and the entity type meets the requirements for creating a proxy.
        // </summary>
        // <param name="derivedEntityType"> The type of entity to create. </param>
        // <returns> The entity instance, which may be a proxy. </returns>
        public TEntity Create(Type derivedEntityType)
        {
            DebugCheck.NotNull(derivedEntityType);

            if (!typeof(TEntity).IsAssignableFrom(derivedEntityType))
            {
                throw Error.DbSet_BadTypeForCreate(derivedEntityType.Name, typeof(TEntity).Name);
            }

            return (TEntity)InternalContext.CreateObject(ObjectContextTypeCache.GetObjectType(derivedEntityType));
        }

        #endregion

        #region Query\set properties

        // <summary>
        // The underlying ObjectQuery.  Accessing this property will trigger lazy initialization of the query.
        // </summary>
        public override ObjectQuery<TEntity> ObjectQuery
        {
            get
            {
                Initialize();
                return base.ObjectQuery;
            }
        }

        // <summary>
        // The underlying EntitySet name.  Accessing this property will trigger lazy initialization of the query.
        // </summary>
        public string EntitySetName
        {
            get
            {
                Initialize();
                return _entitySetName;
            }
        }

        // <summary>
        // The underlying EntitySet name, quoted for ESQL.  Accessing this property will trigger lazy initialization of the query.
        // </summary>
        public string QuotedEntitySetName
        {
            get
            {
                Initialize();
                return _quotedEntitySetName;
            }
        }

        // <summary>
        // The underlying EntitySet.  Accessing this property will trigger lazy initialization of the query.
        // </summary>
        public EntitySet EntitySet
        {
            get
            {
                Initialize();
                return _entitySet;
            }
        }

        // <summary>
        // The base type for the underlying entity set.  Accessing this property will trigger lazy initialization of the query.
        // </summary>
        public Type EntitySetBaseType
        {
            get
            {
                Initialize();
                return _baseType;
            }
        }

        #endregion

        #region Initialization

        // <summary>
        // Performs lazy initialization of the underlying ObjectContext, ObjectQuery, and EntitySet objects
        // so that the query can be used.
        // This method is virtual so that it can be mocked.
        // </summary>
        public virtual void Initialize()
        {
            if (_entitySet == null)
            {
                // This call initializes the context, performs o-space loading if necessary, and checks that the
                // type is valid and is part of the model. It will throw if the entity type for this set is not mapped.
                InitializeUnderlyingTypes(base.InternalContext.GetEntitySetAndBaseTypeForType(typeof(TEntity)));
            }
        }

        // <summary>
        // Attempts to perform lazy initialization of the underlying ObjectContext, ObjectQuery, and EntitySet objects
        // so that o-space loading has happened and the query can be used. This method doesn't throw if the type
        // for the set is not mapped.
        // </summary>
        public virtual void TryInitialize()
        {
            if (_entitySet == null)
            {
                // This call initializes the context, performs o-space loading if necessary, and checks that the
                // type is valid and is part of the model. It will return null if the entity type for this set is
                // not mapped.
                var pair = base.InternalContext.TryGetEntitySetAndBaseTypeForType(typeof(TEntity));
                if (pair != null)
                {
                    InitializeUnderlyingTypes(pair);
                }
            }
        }

        private void InitializeUnderlyingTypes(EntitySetTypePair pair)
        {
            DebugCheck.NotNull(pair);

            _entitySet = pair.EntitySet;
            _baseType = pair.BaseType;

            _entitySetName = string.Format(
                CultureInfo.InvariantCulture, "{0}.{1}", _entitySet.EntityContainer.Name, _entitySet.Name);
            _quotedEntitySetName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}",
                DbHelpers.QuoteIdentifier(_entitySet.EntityContainer.Name),
                DbHelpers.QuoteIdentifier(_entitySet.Name));

            InitializeQuery(CreateObjectQuery(asNoTracking: false));
        }

        // <summary>
        // Creates an underlying <see cref="System.Data.Entity.Core.Objects.ObjectQuery{T}" /> for this set.
        // </summary>
        // <param name="asNoTracking">
        // if set to <c>true</c> then the query is set to be no-tracking.
        // </param>
        // <returns> The query. </returns>
        private ObjectQuery<TEntity> CreateObjectQuery(bool asNoTracking, bool? streaming = null, IDbExecutionStrategy executionStrategy = null)
        {
            var objectQuery = InternalContext.ObjectContext.CreateQuery<TEntity>(_quotedEntitySetName);
            if (_baseType != typeof(TEntity))
            {
                objectQuery = objectQuery.OfType<TEntity>();
            }

            if (asNoTracking)
            {
                objectQuery.MergeOption = MergeOption.NoTracking;
            }

            if (streaming.HasValue) { objectQuery.Streaming = streaming.Value; }

            objectQuery.ExecutionStrategy = executionStrategy;

            return objectQuery;
        }

        #endregion

        #region ToString

        // <summary>
        // Returns a <see cref="System.String" /> representation of the underlying query, equivalent
        // to ToTraceString on ObjectQuery.
        // </summary>
        // <returns> The query string. </returns>
        public override string ToString()
        {
            Initialize();

            return base.ToString();
        }

        #endregion

        #region Underlying context

        // <summary>
        // The underlying InternalContext.  Accessing this property will trigger lazy initialization of the query.
        // </summary>
        public override InternalContext InternalContext
        {
            get
            {
                Initialize();
                return base.InternalContext;
            }
        }

        #endregion

        #region Include

        // <summary>
        // Updates the underlying ObjectQuery with the given include path.
        // </summary>
        // <param name="path"> The include path. </param>
        // <returns> A new query containing the defined include path. </returns>
        public override IInternalQuery<TEntity> Include(string path)
        {
            DebugCheck.NotEmpty(path);

            Initialize();
            return base.Include(path);
        }

        #endregion

        #region AsNoTracking

        // <summary>
        // Returns a new query where the entities returned will not be cached in the <see cref="DbContext" />.
        // </summary>
        // <returns> A new query with NoTracking applied. </returns>
        public override IInternalQuery<TEntity> AsNoTracking()
        {
            Initialize();

            // AsNoTracking called directly on the DbSet (as opposed to a DbQuery) is special-cased so that
            // it doesn't result in a LINQ query being created where one is not needed. This adds a perf boost
            // for simple no-tracking queries such as context.Products.AsNoTracking().
            return new InternalQuery<TEntity>(InternalContext, CreateObjectQuery(asNoTracking: true));
        }

        #endregion

        #region AsStreaming

        // <summary>
        // Returns a new query that will stream the results instead of buffering.
        // </summary>
        // <returns> A new query with AsStreaming applied. </returns>
        public override IInternalQuery<TEntity> AsStreaming()
        {
            Initialize();

            // AsStreaming called directly on the DbSet (as opposed to a DbQuery) is special-cased so that
            // it doesn't result in a LINQ query being created where one is not needed. This adds a perf boost
            // for simple streaming queries such as context.Products.AsStreaming().
            return new InternalQuery<TEntity>(InternalContext, CreateObjectQuery(asNoTracking: false, streaming: true));
        }

        #endregion

        public override IInternalQuery<TEntity> WithExecutionStrategy(IDbExecutionStrategy executionStrategy)
        {
            Initialize();

            // WithExecutionStrategy called directly on the DbSet (as opposed to a DbQuery) is special-cased so that
            // it doesn't result in a LINQ query being created where one is not needed. This adds a perf boost
            // for simple  queries such as context.Products.WithExecutionStrategy().
            return new InternalQuery<TEntity>(InternalContext, CreateObjectQuery(asNoTracking: false, streaming: false, executionStrategy: executionStrategy));
        }

        #region Raw SQL query

        // <summary>
        // Returns an <see cref="IEnumerator" /> which when enumerated will execute the given SQL query against the database
        // materializing entities into the entity set that backs this set.
        // </summary>
        // <param name="sql"> The SQL query. </param>
        // <param name="asNoTracking">
        // If <c>true</c> then the entities are not tracked, otherwise they are.
        // </param>
        // <param name="streaming"> Whether the query is streaming or buffering. </param>
        // <param name="parameters"> The parameters. </param>
        // <returns> The query results. </returns>
        public IEnumerator ExecuteSqlQuery(string sql, bool asNoTracking, bool? streaming, object[] parameters)
        {
            DebugCheck.NotNull(sql);
            DebugCheck.NotNull(parameters);

            InternalContext.ObjectContext.AsyncMonitor.EnsureNotEntered();

            Initialize();
            var mergeOption = asNoTracking ? MergeOption.NoTracking : MergeOption.AppendOnly;

            return new LazyEnumerator<TEntity>(() => InternalContext.ObjectContext.ExecuteStoreQuery<TEntity>(
                            sql, EntitySetName, new ExecutionOptions(mergeOption, streaming), parameters));
        }

#if !NET40

        // <summary>
        // Returns an <see cref="IDbAsyncEnumerator" /> which when enumerated will execute the given SQL query against the database
        // materializing entities into the entity set that backs this set.
        // </summary>
        // <param name="sql"> The SQL query. </param>
        // <param name="asNoTracking">
        // If <c>true</c> then the entities are not tracked, otherwise they are.
        // </param>
        // <param name="streaming"> Whether the query is streaming or buffering. </param>
        // <param name="parameters"> The parameters. </param>
        // <returns> The query results. </returns>
        public IDbAsyncEnumerator ExecuteSqlQueryAsync(string sql, bool asNoTracking, bool? streaming, object[] parameters)
        {
            DebugCheck.NotNull(sql);
            DebugCheck.NotNull(parameters);

            InternalContext.ObjectContext.AsyncMonitor.EnsureNotEntered();

            Initialize();
            var mergeOption = asNoTracking ? MergeOption.NoTracking : MergeOption.AppendOnly;

            return new LazyAsyncEnumerator<TEntity>(
                cancellationToken => InternalContext.ObjectContext.ExecuteStoreQueryAsync<TEntity>(
                    sql, EntitySetName, new ExecutionOptions(mergeOption, streaming), cancellationToken, parameters));
        }

#endif

        #endregion

        #region IQueryable

        // <summary>
        // The LINQ query expression.
        // </summary>
        public override Expression Expression
        {
            get
            {
                Initialize();
                return base.Expression;
            }
        }

        // <summary>
        // The LINQ query provider for the underlying <see cref="ObjectQuery" />.
        // </summary>
        public override ObjectQueryProvider ObjectQueryProvider
        {
            get
            {
                Initialize();
                return base.ObjectQueryProvider;
            }
        }

        #endregion

        #region IEnumerable

        // <summary>
        // Returns an <see cref="IEnumerator{TEntity}" /> which when enumerated will execute the backing query against the database.
        // </summary>
        // <returns> The query results. </returns>
        public override IEnumerator<TEntity> GetEnumerator()
        {
            Initialize();
            return base.GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable

#if !NET40

        // <summary>
        // Returns an <see cref="IDbAsyncEnumerator{TEntity}" /> which when enumerated will execute the backing query against the database.
        // </summary>
        // <returns> The query results. </returns>
        public override IDbAsyncEnumerator<TEntity> GetAsyncEnumerator()
        {
            Initialize();
            return base.GetAsyncEnumerator();
        }

#endif

        #endregion
    }
}
