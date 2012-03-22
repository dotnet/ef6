namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Validation;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     The internal class used to implement <see cref = "System.Data.Entity.Infrastructure.DbEntityEntry" />
    ///     and <see cref = "System.Data.Entity.Infrastructure.DbEntityEntry{T}" />.
    ///     This internal class contains all the common implementation between the generic and non-generic
    ///     entry classes and also allows for a clean internal factoring without compromising the public API.
    /// </summary>
    internal class InternalEntityEntry
    {
        #region Fields and constructors

        private readonly Type _entityType;
        private readonly InternalContext _internalContext;
        private readonly object _entity;
        private IEntityStateEntry _stateEntry;
        private EntityType _edmEntityType;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "InternalEntityEntry" /> class.
        /// </summary>
        /// <param name = "internalContext">The internal context.</param>
        /// <param name = "stateEntry">The state entry.</param>
        public InternalEntityEntry(InternalContext internalContext, IEntityStateEntry stateEntry)
        {
            Contract.Requires(internalContext != null);
            Contract.Requires(stateEntry != null);
            Contract.Assert(stateEntry.Entity != null);

            _internalContext = internalContext;
            _stateEntry = stateEntry;
            _entity = stateEntry.Entity;
            _entityType = ObjectContextTypeCache.GetObjectType(_entity.GetType());
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "InternalEntityEntry" /> class for an
        ///     entity which may or may not be attached to the context.
        /// </summary>
        /// <param name = "internalContext">The internal context.</param>
        /// <param name = "entity">The entity.</param>
        public InternalEntityEntry(InternalContext internalContext, object entity)
        {
            Contract.Requires(internalContext != null);
            Contract.Requires(entity != null);

            _internalContext = internalContext;
            _entity = entity;
            _entityType = ObjectContextTypeCache.GetObjectType(_entity.GetType());

            _stateEntry = _internalContext.GetStateEntry(entity);
            if (_stateEntry == null)
            {
                // This will cause the context and model to be initialized and will throw an exception
                // if the entity type is not part of the model.
                _internalContext.Set(_entityType).InternalSet.Initialize();
            }
        }

        #endregion

        #region Entity access

        /// <summary>
        ///     Gets the tracked entity.
        ///     This property is virtual to allow mocking.
        /// </summary>
        /// <value>The entity.</value>
        public virtual object Entity
        {
            get { return _entity; }
        }

        #endregion

        #region Entity state

        /// <summary>
        ///     Gets or sets the state of the entity.
        /// </summary>
        /// <value>The state.</value>
        public EntityState State
        {
            get { return IsDetached ? EntityState.Detached : _stateEntry.State; }
            set
            {
                if (!IsDetached)
                {
                    if (_stateEntry.State == EntityState.Modified
                        && value == EntityState.Unchanged)
                    {
                        // Special case modified to unchanged to be "reject changes" even
                        // ChangeState will do "accept changes".  This keeps the behavior consistent with
                        // setting modified to false at the property level (once that is supported).
                        CurrentValues.SetValues(OriginalValues);
                    }
                    _stateEntry.ChangeState(value);
                }
                else
                {
                    switch (value)
                    {
                        case EntityState.Added:
                            _internalContext.Set(_entityType).InternalSet.Add(_entity);
                            break;
                        case EntityState.Unchanged:
                            _internalContext.Set(_entityType).InternalSet.Attach(_entity);
                            break;
                        case EntityState.Modified:
                        case EntityState.Deleted:
                            _internalContext.Set(_entityType).InternalSet.Attach(_entity);
                            _stateEntry = _internalContext.GetStateEntry(_entity);
                            Contract.Assert(_stateEntry != null, "_stateEntry should not be null after Attach.");
                            _stateEntry.ChangeState(value);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #endregion

        #region Property values and concurrency

        /// <summary>
        ///     Gets the current property values for the tracked entity represented by this object.
        ///     This property is virtual to allow mocking.
        /// </summary>
        /// <value>The current values.</value>
        public virtual InternalPropertyValues CurrentValues
        {
            get
            {
                ValidateStateToGetValues("CurrentValues", EntityState.Deleted);

                return new DbDataRecordPropertyValues(
                    _internalContext, _entityType, _stateEntry.CurrentValues, isEntity: true);
            }
        }

        /// <summary>
        ///     Gets the original property values for the tracked entity represented by this object.
        ///     The original values are usually the entity's property values as they were when last queried from
        ///     the database.
        ///     This property is virtual to allow mocking.
        /// </summary>
        /// <value>The original values.</value>
        public virtual InternalPropertyValues OriginalValues
        {
            get
            {
                ValidateStateToGetValues("OriginalValues", EntityState.Added);

                return new DbDataRecordPropertyValues(
                    _internalContext, _entityType, _stateEntry.GetUpdatableOriginalValues(), isEntity: true);
            }
        }

        /// <summary>
        ///     Queries the database for copies of the values of the tracked entity as they currently exist in the database.
        /// </summary>
        /// <returns>The store values.</returns>
        public InternalPropertyValues GetDatabaseValues()
        {
            ValidateStateToGetValues("GetDatabaseValues", EntityState.Added);

            // Build an Entity SQL query that will materialize all the properties for the entity into
            // a DbDataRecord, including nested DbDataRecords for complex properties.
            // This is preferable to a no-tracking query because it doesn't materialize an object only
            // to throw it away again after the properties have been read.
            // Theoretically, it should also work for shadow state,

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT ");

            // Build the list of properties to query
            AppendEntitySqlRow(queryBuilder, "X", OriginalValues);

            // Add in a WHERE clause for the primary key values
            var quotedEntitySetName = String.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}",
                DbHelpers.QuoteIdentifier(_stateEntry.EntitySet.EntityContainer.Name),
                DbHelpers.QuoteIdentifier(_stateEntry.EntitySet.Name));

            var quotedTypeName = String.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}",
                DbHelpers.QuoteIdentifier(EntityType.Namespace),
                DbHelpers.QuoteIdentifier(EntityType.Name));

            queryBuilder.AppendFormat(
                CultureInfo.InvariantCulture,
                " FROM (SELECT VALUE TREAT (Y AS {0}) FROM {1} AS Y) AS X WHERE ",
                quotedTypeName,
                quotedEntitySetName);

            var entityKeyValues = _stateEntry.EntityKey.EntityKeyValues;
            var parameters = new ObjectParameter[entityKeyValues.Length];

            for (var i = 0; i < entityKeyValues.Length; i++)
            {
                if (i > 0)
                {
                    queryBuilder.Append(" AND ");
                }

                var name = string.Format(CultureInfo.InvariantCulture, "p{0}", i.ToString(CultureInfo.InvariantCulture));
                queryBuilder.AppendFormat(
                    CultureInfo.InvariantCulture, "X.{0} = @{1}", DbHelpers.QuoteIdentifier(entityKeyValues[i].Key),
                    name);
                parameters[i] = new ObjectParameter(name, entityKeyValues[i].Value);
            }

            // Execute the query
            var dataRecord =
                _internalContext.ObjectContext.CreateQuery<DbDataRecord>(queryBuilder.ToString(), parameters).
                    SingleOrDefault();

            return dataRecord == null ? null : new ClonedPropertyValues(OriginalValues, dataRecord);
        }

        /// <summary>
        ///     Appends a query for the properties in the entity to the given string builder that is being used to
        ///     build the eSQL query.  This method may be called recursively to query for all the sub-properties of
        ///     a complex property.
        /// </summary>
        /// <param name = "queryBuilder">The query builder.</param>
        /// <param name = "prefix">The qualifier with which to prefix each property name.</param>
        /// <param name = "templateValues">The dictionary that acts as a template for the properties to query.</param>
        private void AppendEntitySqlRow(
            StringBuilder queryBuilder, string prefix, InternalPropertyValues templateValues)
        {
            var commaRequired = false;
            foreach (var propertyName in templateValues.PropertyNames)
            {
                if (commaRequired)
                {
                    queryBuilder.Append(", ");
                }
                else
                {
                    commaRequired = true;
                }

                var quotedName = DbHelpers.QuoteIdentifier(propertyName);

                var templateItem = templateValues.GetItem(propertyName);

                if (templateItem.IsComplex)
                {
                    var nestedValues = templateItem.Value as InternalPropertyValues;
                    if (nestedValues == null)
                    {
                        throw Error.DbPropertyValues_CannotGetStoreValuesWhenComplexPropertyIsNull(
                            propertyName, EntityType.Name);
                    }

                    // Call the same method recursively to get all the values of the complex property
                    queryBuilder.Append("ROW(");
                    AppendEntitySqlRow(
                        queryBuilder, String.Format(CultureInfo.InvariantCulture, "{0}.{1}", prefix, quotedName),
                        nestedValues);
                    queryBuilder.AppendFormat(CultureInfo.InvariantCulture, ") AS {0}", quotedName);
                }
                else
                {
                    queryBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1} ", prefix, quotedName);
                }
            }
        }

        /// <summary>
        ///     Validates that a dictionary can be obtained for the state of the entity represented by this entry.
        /// </summary>
        /// <param name = "method">The method name being used to request a dictionary.</param>
        /// <param name = "invalidState">The state that is invalid for the request being processed.</param>
        private void ValidateStateToGetValues(string method, EntityState invalidState)
        {
            ValidateNotDetachedAndInitializeRelatedEnd(method);

            if (State == invalidState)
            {
                throw Error.DbPropertyValues_CannotGetValuesForState(method, State);
            }
        }

        /// <summary>
        ///     Calls Refresh with StoreWins on the underlying state entry.
        /// </summary>
        public void Reload()
        {
            ValidateStateToGetValues("Reload", EntityState.Added);

            _internalContext.ObjectContext.Refresh(RefreshMode.StoreWins, Entity);
        }

        #endregion

        #region Property, Reference, and Collection fluents

        /// <summary>
        ///     Gets an internal object representing a reference navigation property.
        ///     This method is virtual to allow mocking.
        /// </summary>
        /// <param name = "navigationProperty">The navigation property.</param>
        /// <param name = "requestedType">The type of entity requested, which may be 'object' or null if any type can be accepted.</param>
        /// <returns>The entry.</returns>
        public virtual InternalReferenceEntry Reference(string navigationProperty, Type requestedType = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(navigationProperty));

            return
                (InternalReferenceEntry)
                ValidateAndGetNavigationMetadata(
                    navigationProperty, requestedType ?? typeof(object), requireCollection: false).
                    CreateMemberEntry(this, null);
        }

        /// <summary>
        ///     Gets an internal object representing a collection navigation property.
        ///     This method is virtual to allow mocking.
        /// </summary>
        /// <param name = "navigationProperty">The navigation property.</param>
        /// <param name = "requestedType">The type of entity requested, which may be 'object' or null f any type can be accepted.</param>
        /// <returns>The entry.</returns>
        public virtual InternalCollectionEntry Collection(string navigationProperty, Type requestedType = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(navigationProperty));

            return
                (InternalCollectionEntry)
                ValidateAndGetNavigationMetadata(
                    navigationProperty, requestedType ?? typeof(object), requireCollection: true).
                    CreateMemberEntry(this, null);
        }

        /// <summary>
        ///     Gets an internal object representing a navigation, scalar, or complex property.
        ///     This method is virtual to allow mocking.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        /// <param name = "requestedType">The type of entity requested, which may be 'object' if any type can be accepted.</param>
        /// <returns>The entry.</returns>
        public virtual InternalMemberEntry Member(string propertyName, Type requestedType = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            requestedType = requestedType ?? typeof(object);

            var properties = SplitName(propertyName);
            if (properties.Count > 1)
            {
                return Property(null, propertyName, properties, requestedType, requireComplex: false);
            }

            var memberMetadata = GetNavigationMetadata(propertyName) ??
                                 (MemberEntryMetadata)
                                 ValidateAndGetPropertyMetadata(propertyName, EntityType, requestedType);

            if (memberMetadata == null)
            {
                throw Error.DbEntityEntry_NotAProperty(propertyName, EntityType.Name);
            }

            // This check is used for non-collection entries.  For collection entries there is a more specific
            // check in the DbCollectionEntry class.
            // Examples:
            // If (!SomeStringProp is Object) => okay
            // If (!SomeFeaturedProduct is Product) => okay
            // If (!SomeProduct is FeaturedProduct) => throw
            if (memberMetadata.MemberEntryType != MemberEntryType.CollectionNavigationProperty
                &&
                !requestedType.IsAssignableFrom(memberMetadata.MemberType))
            {
                throw Error.DbEntityEntry_WrongGenericForNavProp(
                    propertyName, EntityType.Name, requestedType.Name, memberMetadata.MemberType.Name);
            }

            return memberMetadata.CreateMemberEntry(this, null);
        }

        /// <summary>
        ///     Gets an internal object representing a scalar or complex property.
        ///     This method is virtual to allow mocking.
        /// </summary>
        /// <param name = "property">The property.</param>
        /// <param name = "requestedType">The type of object requested, which may be null or 'object' if any type can be accepted.</param>
        /// <param name = "requireComplex">if set to <c>true</c> then the found property must be a complex property.</param>
        /// <returns>The entry.</returns>
        public virtual InternalPropertyEntry Property(
            string property, Type requestedType = null, bool requireComplex = false)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(property));

            return Property(null, property, requestedType ?? typeof(object), requireComplex);
        }

        /// <summary>
        ///     Gets an internal object representing a scalar or complex property.
        ///     The property may be a nested property on the given <see cref = "InternalPropertyEntry" />.
        /// </summary>
        /// <param name = "parentProperty">The parent property entry, or null if this is a property directly on the entity.</param>
        /// <param name = "propertyName">Name of the property.</param>
        /// <param name = "requestedType">The type of object requested, which may be null or 'object' if any type can be accepted.</param>
        /// <param name = "requireComplex">if set to <c>true</c> then the found property must be a complex property.</param>
        /// <returns>The entry.</returns>
        public InternalPropertyEntry Property(
            InternalPropertyEntry parentProperty, string propertyName, Type requestedType, bool requireComplex)
        {
            return Property(parentProperty, propertyName, SplitName(propertyName), requestedType, requireComplex);
        }

        /// <summary>
        ///     Gets an internal object representing a scalar or complex property.
        ///     The property may be a nested property on the given <see cref = "InternalPropertyEntry" />.
        /// </summary>
        /// <param name = "parentProperty">The parent property entry, or null if this is a property directly on the entity.</param>
        /// <param name = "propertyName">Name of the property.</param>
        /// <param name = "properties">The property split out into its parts.</param>
        /// <param name = "requestedType">The type of object requested, which may be null or 'object' if any type can be accepted.</param>
        /// <param name = "requireComplex">if set to <c>true</c> then the found property must be a complex property.</param>
        /// <returns>The entry.</returns>
        private InternalPropertyEntry Property(
            InternalPropertyEntry parentProperty, string propertyName, IList<string> properties, Type requestedType,
            bool requireComplex)
        {
            var isDotted = properties.Count > 1;
            var currentRequestedType = isDotted ? typeof(object) : requestedType;
            var declaringType = parentProperty != null ? parentProperty.EntryMetadata.ElementType : EntityType;

            var propertyMetadata = ValidateAndGetPropertyMetadata(properties[0], declaringType, currentRequestedType);

            if (propertyMetadata == null
                || ((isDotted || requireComplex) && !propertyMetadata.IsComplex))
            {
                if (isDotted)
                {
                    throw Error.DbEntityEntry_DottedPartNotComplex(properties[0], propertyName, declaringType.Name);
                }
                throw requireComplex
                          ? Error.DbEntityEntry_NotAComplexProperty(properties[0], declaringType.Name)
                          : Error.DbEntityEntry_NotAScalarProperty(properties[0], declaringType.Name);
            }

            var internalPropertyEntry = (InternalPropertyEntry)propertyMetadata.CreateMemberEntry(this, parentProperty);
            return isDotted
                       ? Property(
                           internalPropertyEntry, propertyName, properties.Skip(1).ToList(), requestedType,
                           requireComplex)
                       : internalPropertyEntry;
        }

        /// <summary>
        ///     Checks that the given property name is a navigation property and is either a reference property or
        ///     collection property according to the value of requireCollection.
        /// </summary>
        private NavigationEntryMetadata ValidateAndGetNavigationMetadata(
            string navigationProperty, Type requestedType, bool requireCollection)
        {
            if (SplitName(navigationProperty).Count != 1)
            {
                throw Error.DbEntityEntry_DottedPathMustBeProperty(navigationProperty);
            }

            var propertyMetadata = GetNavigationMetadata(navigationProperty);
            if (propertyMetadata == null)
            {
                throw Error.DbEntityEntry_NotANavigationProperty(navigationProperty, EntityType.Name);
            }

            if (requireCollection)
            {
                if (propertyMetadata.MemberEntryType
                    == MemberEntryType.ReferenceNavigationProperty)
                {
                    throw Error.DbEntityEntry_UsedCollectionForReferenceProp(navigationProperty, EntityType.Name);
                }
            }
            else if (propertyMetadata.MemberEntryType
                     == MemberEntryType.CollectionNavigationProperty)
            {
                throw Error.DbEntityEntry_UsedReferenceForCollectionProp(navigationProperty, EntityType.Name);
            }

            if (!requestedType.IsAssignableFrom(propertyMetadata.ElementType))
            {
                throw Error.DbEntityEntry_WrongGenericForNavProp(
                    navigationProperty, EntityType.Name, requestedType.Name, propertyMetadata.ElementType.Name);
            }

            return propertyMetadata;
        }

        /// <summary>
        ///     Gets metadata for the given property if that property is a navigation property or returns null
        ///     if it is not a navigation property.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        /// <returns>Navigation property metadata or null.</returns>
        public virtual NavigationEntryMetadata GetNavigationMetadata(string propertyName)
        {
            EdmMember member;
            EdmEntityType.Members.TryGetValue(propertyName, false, out member);

            var asNavProperty = member as NavigationProperty;
            return asNavProperty == null
                       ? null
                       : new NavigationEntryMetadata(
                             EntityType,
                             GetNavigationTargetType(asNavProperty),
                             propertyName,
                             asNavProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many);
        }

        /// <summary>
        ///     Gets the type of entity or entities at the target end of the given navigation property.
        /// </summary>
        /// <param name = "navigationProperty">The navigation property.</param>
        /// <returns>The CLR type of the entity or entities at the other end.</returns>
        private Type GetNavigationTargetType(NavigationProperty navigationProperty)
        {
            var metadataWorkspace = _internalContext.ObjectContext.MetadataWorkspace;

            var cSpaceType =
                navigationProperty.RelationshipType.RelationshipEndMembers.Single(
                    e => navigationProperty.ToEndMember.Name == e.Name).
                    GetEntityType();
            var oSpaceType = metadataWorkspace.GetObjectSpaceType(cSpaceType);

            var objectItemCollection = (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace);
            return objectItemCollection.GetClrType(oSpaceType);
        }

        /// <summary>
        ///     Gets the related end for the navigation property with the given name.
        /// </summary>
        /// <param name = "navigationProperty">The navigation property.</param>
        /// <returns></returns>
        public virtual IRelatedEnd GetRelatedEnd(string navigationProperty)
        {
            EdmMember member;
            EdmEntityType.Members.TryGetValue(navigationProperty, false, out member);

            Contract.Assert(
                member is NavigationProperty, "Property should have already been validated as a nav property.");
            var asNavProperty = (NavigationProperty)member;

            var relationshipManager = _internalContext.ObjectContext.ObjectStateManager.GetRelationshipManager(Entity);
            return relationshipManager.GetRelatedEnd(
                asNavProperty.RelationshipType.FullName, asNavProperty.ToEndMember.Name);
        }

        /// <summary>
        ///     Uses EDM metadata to validate that the property name exists in the model and represents a scalar or
        ///     complex property or exists in the CLR type.
        ///     This method is public and virtual so that it can be mocked.
        /// </summary>
        /// <param name = "propertyName">The property name.</param>
        /// <param name = "declaringType">The type on which the property is declared.</param>
        /// <param name = "requestedType">The type of object requested, which may be 'object' if any type can be accepted.</param>
        /// <returns>Metadata for the property.</returns>
        public virtual PropertyEntryMetadata ValidateAndGetPropertyMetadata(
            string propertyName, Type declaringType, Type requestedType)
        {
            return PropertyEntryMetadata.ValidateNameAndGetMetadata(
                _internalContext, declaringType, requestedType, propertyName);
        }

        /// <summary>
        ///     Splits the given property name into parts delimited by dots.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        /// <returns>The parts of the name.</returns>
        private IList<string> SplitName(string propertyName)
        {
            Contract.Requires(propertyName != null);

            return propertyName.Split('.');
        }

        #endregion

        #region Handling entries for detached entities

        /// <summary>
        ///     Validates that this entry is associated with an underlying <see cref = "ObjectStateEntry" /> and
        ///     is not just wrapping a non-attached entity.
        /// </summary>
        private void ValidateNotDetachedAndInitializeRelatedEnd(string method)
        {
            if (IsDetached)
            {
                throw Error.DbEntityEntry_NotSupportedForDetached(method, _entityType.Name);
            }
        }

        /// <summary>
        ///     Checks whether or not this entry is associated with an underlying <see cref = "ObjectStateEntry" /> or
        ///     is just wrapping a non-attached entity.
        /// </summary>
        public virtual bool IsDetached
        {
            get
            {
                if (_stateEntry == null
                    || _stateEntry.State == EntityState.Detached)
                {
                    _stateEntry = _internalContext.GetStateEntry(_entity);
                    if (_stateEntry == null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region Entity type and state entry access

        /// <summary>
        ///     Gets the type of the entity being tracked.
        /// </summary>
        /// <value>The type of the entity.</value>
        public virtual Type EntityType
        {
            get { return _entityType; }
        }

        /// <summary>
        ///     Gets the c-space entity type for this entity from the EDM.
        /// </summary>
        public virtual EntityType EdmEntityType
        {
            get
            {
                if (_edmEntityType == null)
                {
                    var metadataWorkspace = _internalContext.ObjectContext.MetadataWorkspace;
                    var oSpaceType = metadataWorkspace.GetItem<EntityType>(_entityType.FullName, DataSpace.OSpace);
                    _edmEntityType = (EntityType)metadataWorkspace.GetEdmSpaceType(oSpaceType);
                }
                return _edmEntityType;
            }
        }

        /// <summary>
        ///     Gets the underlying object state entry.
        /// </summary>
        public IEntityStateEntry ObjectStateEntry
        {
            get
            {
                Contract.Assert(
                    _stateEntry != null, "ObjectStateEntry is not available from entries for detached entities.");

                return _stateEntry;
            }
        }

        /// <summary>
        ///     Gets the internal context.
        /// </summary>
        /// <value>The internal context.</value>
        public InternalContext InternalContext
        {
            get { return _internalContext; }
        }

        #endregion

        #region Validation

        /// <summary>
        ///     Validates entity represented by this entity entry.
        ///     This method is virtual to allow mocking.
        /// </summary>
        /// <param name = "items">User defined dictionary containing additional info for custom validation. This parameter is optional and can be null.</param>
        /// <returns><see cref = "DbEntityValidationResult" /> containing validation result. Never null.</returns>
        public virtual DbEntityValidationResult GetValidationResult(IDictionary<object, object> items)
        {
            var entityValidator = InternalContext.ValidationProvider.GetEntityValidator(this);

            var originalLazyLoadingFlag = InternalContext.LazyLoadingEnabled;
            InternalContext.LazyLoadingEnabled = false;
            DbEntityValidationResult result = null;
            try
            {
                result = entityValidator != null
                             ? entityValidator.Validate(
                                 InternalContext.ValidationProvider.GetEntityValidationContext(this, items))
                             : new DbEntityValidationResult(this, Enumerable.Empty<DbValidationError>());
            }
            finally
            {
                InternalContext.LazyLoadingEnabled = originalLazyLoadingFlag;
            }

            return result;
        }

        #endregion

        #region Equals\GetHashCode implementation

        /// <summary>
        ///     Determines whether the specified <see cref = "System.Object" /> is equal to this instance.
        ///     Two <see cref = "InternalEntityEntry" /> instances are considered equal if they are both entries for
        ///     the same entity on the same <see cref = "DbContext" />.
        /// </summary>
        /// <param name = "obj">The <see cref = "System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref = "System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)
                || obj.GetType() != typeof(InternalEntityEntry))
            {
                return false;
            }

            return Equals((InternalEntityEntry)obj);
        }

        /// <summary>
        ///     Determines whether the specified <see cref = "InternalEntityEntry" /> is equal to this instance.
        ///     Two <see cref = "InternalEntityEntry" /> instances are considered equal if they are both entries for
        ///     the same entity on the same <see cref = "DbContext" />.
        /// </summary>
        /// <param name = "other">The <see cref = "InternalEntityEntry" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref = "InternalEntityEntry" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(InternalEntityEntry other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return !ReferenceEquals(null, other) &&
                   ReferenceEquals(_entity, other._entity) &&
                   ReferenceEquals(_internalContext, other._internalContext);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _entity.GetHashCode();
        }

        #endregion
    }
}
