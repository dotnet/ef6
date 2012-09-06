// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Validation;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Instances of this class provide access to information about and control of entities that
    ///     are being tracked by the <see cref="DbContext" />.  Use the Entity or Entities methods of
    ///     the context to obtain objects of this type.
    /// </summary>
    /// <typeparam name="TEntity"> The type of the entity. </typeparam>
    public class DbEntityEntry<TEntity>
        where TEntity : class
    {
        #region Fields and constructors

        private readonly InternalEntityEntry _internalEntityEntry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbEntityEntry{TEntity}" /> class.
        /// </summary>
        /// <param name="internalEntityEntry"> The internal entry. </param>
        internal DbEntityEntry(InternalEntityEntry internalEntityEntry)
        {
            Contract.Requires(internalEntityEntry != null);

            _internalEntityEntry = internalEntityEntry;
        }

        #endregion

        #region Entity access

        /// <summary>
        ///     Gets the entity.
        /// </summary>
        /// <value> The entity. </value>
        public TEntity Entity
        {
            get { return (TEntity)_internalEntityEntry.Entity; }
        }

        #endregion

        #region Entity state

        /// <summary>
        ///     Gets or sets the state of the entity.
        /// </summary>
        /// <value> The state. </value>
        public EntityState State
        {
            get { return _internalEntityEntry.State; }
            set { _internalEntityEntry.State = value; }
        }

        #endregion

        #region Property values and concurrency

        /// <summary>
        ///     Gets the current property values for the tracked entity represented by this object.
        /// </summary>
        /// <value> The current values. </value>
        public DbPropertyValues CurrentValues
        {
            get { return new DbPropertyValues(_internalEntityEntry.CurrentValues); }
        }

        /// <summary>
        ///     Gets the original property values for the tracked entity represented by this object.
        ///     The original values are usually the entity's property values as they were when last queried from
        ///     the database.
        /// </summary>
        /// <value> The original values. </value>
        public DbPropertyValues OriginalValues
        {
            get { return new DbPropertyValues(_internalEntityEntry.OriginalValues); }
        }

        /// <summary>
        ///     Queries the database for copies of the values of the tracked entity as they currently exist in the database.
        ///     Note that changing the values in the returned dictionary will not update the values in the database.
        ///     If the entity is not found in the database then null is returned.
        /// </summary>
        /// <returns> The store values. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public DbPropertyValues GetDatabaseValues()
        {
            var storeValues = _internalEntityEntry.GetDatabaseValues();
            return storeValues == null ? null : new DbPropertyValues(storeValues);
        }

#if !NET40

        /// <summary>
        ///     An asynchronous version of GetDatabaseValues, which
        ///     queries the database for copies of the values of the tracked entity as they currently exist in the database.
        ///     Note that changing the values in the returned dictionary will not update the values in the database.
        ///     If the entity is not found in the database then null is returned.
        /// </summary>
        /// <returns> A Task that contains the store values. </returns>
        public Task<DbPropertyValues> GetDatabaseValuesAsync()
        {
            return GetDatabaseValuesAsync(CancellationToken.None);
        }

        /// <summary>
        ///     An asynchronous version of GetDatabaseValues, which
        ///     queries the database for copies of the values of the tracked entity as they currently exist in the database.
        ///     Note that changing the values in the returned dictionary will not update the values in the database.
        ///     If the entity is not found in the database then null is returned.
        /// </summary>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task that contains the store values. </returns>
        public async Task<DbPropertyValues> GetDatabaseValuesAsync(CancellationToken cancellationToken)
        {
            var storeValues =
                await _internalEntityEntry.GetDatabaseValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return storeValues == null ? null : new DbPropertyValues(storeValues);
        }

#endif

        /// <summary>
        ///     Reloads the entity from the database overwriting any property values with values from the database.
        ///     The entity will be in the Unchanged state after calling this method.
        /// </summary>
        public void Reload()
        {
            _internalEntityEntry.Reload();
        }

        #endregion

        #region Property, Reference, and Collection fluents

        /// <summary>
        ///     Gets an object that represents the reference (i.e. non-collection) navigation property from this
        ///     entity to another entity.
        /// </summary>
        /// <param name="navigationProperty"> The name of the navigation property. </param>
        /// <returns> An object representing the navigation property. </returns>
        public DbReferenceEntry Reference(string navigationProperty)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(navigationProperty));

            return DbReferenceEntry.Create(_internalEntityEntry.Reference(navigationProperty));
        }

        /// <summary>
        ///     Gets an object that represents the reference (i.e. non-collection) navigation property from this
        ///     entity to another entity.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <param name="navigationProperty"> The name of the navigation property. </param>
        /// <returns> An object representing the navigation property. </returns>
        public DbReferenceEntry<TEntity, TProperty> Reference<TProperty>(string navigationProperty)
            where TProperty : class
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(navigationProperty));

            return
                DbReferenceEntry<TEntity, TProperty>.Create(
                    _internalEntityEntry.Reference(navigationProperty, typeof(TProperty)));
        }

        /// <summary>
        ///     Gets an object that represents the reference (i.e. non-collection) navigation property from this
        ///     entity to another entity.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <param name="navigationProperty"> An expression representing the navigation property. </param>
        /// <returns> An object representing the navigation property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DbReferenceEntry<TEntity, TProperty> Reference<TProperty>(
            Expression<Func<TEntity, TProperty>> navigationProperty)
            where TProperty : class
        {
            Contract.Requires(navigationProperty != null);

            return
                DbReferenceEntry<TEntity, TProperty>.Create(
                    _internalEntityEntry.Reference(
                        DbHelpers.ParsePropertySelector(navigationProperty, "Reference", "navigationProperty"),
                        typeof(TProperty)));
        }

        /// <summary>
        ///     Gets an object that represents the collection navigation property from this
        ///     entity to a collection of related entities.
        /// </summary>
        /// <param name="navigationProperty"> The name of the navigation property. </param>
        /// <returns> An object representing the navigation property. </returns>
        public DbCollectionEntry Collection(string navigationProperty)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(navigationProperty));

            return DbCollectionEntry.Create(_internalEntityEntry.Collection(navigationProperty));
        }

        /// <summary>
        ///     Gets an object that represents the collection navigation property from this
        ///     entity to a collection of related entities.
        /// </summary>
        /// <typeparam name="TElement"> The type of elements in the collection. </typeparam>
        /// <param name="navigationProperty"> The name of the navigation property. </param>
        /// <returns> An object representing the navigation property. </returns>
        public DbCollectionEntry<TEntity, TElement> Collection<TElement>(string navigationProperty)
            where TElement : class
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(navigationProperty));

            return
                DbCollectionEntry<TEntity, TElement>.Create(
                    _internalEntityEntry.Collection(navigationProperty, typeof(TElement)));
        }

        /// <summary>
        ///     Gets an object that represents the collection navigation property from this
        ///     entity to a collection of related entities.
        /// </summary>
        /// <typeparam name="TElement"> The type of elements in the collection. </typeparam>
        /// <param name="navigationProperty"> An expression representing the navigation property. </param>
        /// <returns> An object representing the navigation property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DbCollectionEntry<TEntity, TElement> Collection<TElement>(
            Expression<Func<TEntity, ICollection<TElement>>> navigationProperty) where TElement : class
        {
            Contract.Requires(navigationProperty != null);

            return
                Collection<TElement>(
                    DbHelpers.ParsePropertySelector(navigationProperty, "Collection", "navigationProperty"));
        }

        /// <summary>
        ///     Gets an object that represents a scalar or complex property of this entity.
        /// </summary>
        /// <param name="propertyName"> The name of the property. </param>
        /// <returns> An object representing the property. </returns>
        public DbPropertyEntry Property(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return DbPropertyEntry.Create(_internalEntityEntry.Property(propertyName));
        }

        /// <summary>
        ///     Gets an object that represents a scalar or complex property of this entity.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <param name="propertyName"> The name of the property. </param>
        /// <returns> An object representing the property. </returns>
        public DbPropertyEntry<TEntity, TProperty> Property<TProperty>(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return
                DbPropertyEntry<TEntity, TProperty>.Create(
                    _internalEntityEntry.Property(propertyName, typeof(TProperty)));
        }

        /// <summary>
        ///     Gets an object that represents a scalar or complex property of this entity.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <param name="navigationProperty"> An expression representing the property. </param>
        /// <returns> An object representing the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "Rule predates more fluent naming conventions.")]
        public DbPropertyEntry<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            Contract.Requires(property != null);

            return Property<TProperty>(DbHelpers.ParsePropertySelector(property, "Property", "property"));
        }

        /// <summary>
        ///     Gets an object that represents a complex property of this entity.
        /// </summary>
        /// <param name="propertyName"> The name of the complex property. </param>
        /// <returns> An object representing the complex property. </returns>
        public DbComplexPropertyEntry ComplexProperty(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return DbComplexPropertyEntry.Create(
                _internalEntityEntry.Property(propertyName, null, requireComplex: true));
        }

        /// <summary>
        ///     Gets an object that represents a complex property of this entity.
        /// </summary>
        /// <typeparam name="TComplexProperty"> The type of the complex property. </typeparam>
        /// <param name="propertyName"> The name of the complex property. </param>
        /// <returns> An object representing the complex property. </returns>
        public DbComplexPropertyEntry<TEntity, TComplexProperty> ComplexProperty<TComplexProperty>(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return
                DbComplexPropertyEntry<TEntity, TComplexProperty>.Create(
                    _internalEntityEntry.Property(propertyName, typeof(TComplexProperty), requireComplex: true));
        }

        /// <summary>
        ///     Gets an object that represents a complex property of this entity.
        /// </summary>
        /// <typeparam name="TComplexProperty"> The type of the complex property. </typeparam>
        /// <param name="navigationProperty"> An expression representing the complex property. </param>
        /// <returns> An object representing the complex property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "Rule predates more fluent naming conventions.")]
        public DbComplexPropertyEntry<TEntity, TComplexProperty> ComplexProperty<TComplexProperty>(
            Expression<Func<TEntity, TComplexProperty>> property)
        {
            Contract.Requires(property != null);

            return ComplexProperty<TComplexProperty>(DbHelpers.ParsePropertySelector(property, "Property", "property"));
        }

        /// <summary>
        ///     Gets an object that represents a member of the entity.  The runtime type of the returned object will
        ///     vary depending on what kind of member is asked for.  The currently supported member types and their return
        ///     types are:
        ///     Reference navigation property: <see cref="DbReferenceEntry" />.
        ///     Collection navigation property: <see cref="DbCollectionEntry" />.
        ///     Primitive/scalar property: <see cref="DbPropertyEntry" />.
        ///     Complex property: <see cref="DbComplexPropertyEntry" />.
        /// </summary>
        /// <param name="propertyName"> The name of the member. </param>
        /// <returns> An object representing the member. </returns>
        public DbMemberEntry Member(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return DbMemberEntry.Create(_internalEntityEntry.Member(propertyName));
        }

        /// <summary>
        ///     Gets an object that represents a member of the entity.  The runtime type of the returned object will
        ///     vary depending on what kind of member is asked for.  The currently supported member types and their return
        ///     types are:
        ///     Reference navigation property: <see cref="DbReferenceEntry{TEntity, TProperty}" />.
        ///     Collection navigation property: <see cref="DbCollectionEntry{TEntity, TElement}" />.
        ///     Primitive/scalar property: <see cref="DbPropertyEntry{TEntity, TProperty}" />.
        ///     Complex property: <see cref="DbComplexPropertyEntry{TEntity, TProperty}" />.
        /// </summary>
        /// <typeparam name="TMember"> The type of the member. </typeparam>
        /// <param name="propertyName"> The name of the member. </param>
        /// <returns> An object representing the member. </returns>
        public DbMemberEntry<TEntity, TMember> Member<TMember>(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return _internalEntityEntry.Member(propertyName, typeof(TMember)).CreateDbMemberEntry<TEntity, TMember>();
        }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        ///     Returns a new instance of the non-generic <see cref="DbEntityEntry" /> class for 
        ///     the tracked entity represented by this object.
        /// </summary>
        /// <returns> A non-generic version. </returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbEntityEntry(DbEntityEntry<TEntity> entry)
        {
            return new DbEntityEntry(entry._internalEntityEntry);
        }

        #endregion

        #region Validation

        /// <summary>
        ///     Validates this <see cref="DbEntityEntry{T}" /> instance and returns validation result.
        /// </summary>
        /// <returns> Entity validation result. Possibly null if <see
        ///      cref="DbContext.ValidateEntity(DbEntityEntry, IDictionary{object, object})" /> method is overridden. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public DbEntityValidationResult GetValidationResult()
        {
            // need to call the method on DbContext to pickup potential validation 
            // customizations the user potentially implemented
            return _internalEntityEntry.InternalContext.Owner.CallValidateEntity(this);
        }

        #endregion

        #region Equals\GetHashCode implementation

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        ///     Two <see cref="DbEntityEntry{TEntity}" /> instances are considered equal if they are both entries for
        ///     the same entity on the same <see cref="DbContext" />.
        /// </summary>
        /// <param name="obj"> The <see cref="System.Object" /> to compare with this instance. </param>
        /// <returns> <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c> . </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // Still hide it since it is generally not useful to see when dotting in the API.
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)
                || obj.GetType() != typeof(DbEntityEntry<TEntity>))
            {
                return false;
            }

            return Equals((DbEntityEntry<TEntity>)obj);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="DbEntityEntry{TEntity}" /> is equal to this instance.
        ///     Two <see cref="DbEntityEntry{TEntity}" /> instances are considered equal if they are both entries for
        ///     the same entity on the same <see cref="DbContext" />.
        /// </summary>
        /// <param name="other"> The <see cref="DbEntityEntry{TEntity}" /> to compare with this instance. </param>
        /// <returns> <c>true</c> if the specified <see cref="DbEntityEntry{TEntity}" /> is equal to this instance; otherwise, <c>false</c> . </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // Still hide it since it is generally not useful to see when dotting in the API.
        public bool Equals(DbEntityEntry<TEntity> other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return !ReferenceEquals(null, other) && _internalEntityEntry.Equals(other._internalEntityEntry);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns> A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // Still hide it since it is generally not useful to see when dotting in the API.
        public override int GetHashCode()
        {
            return _internalEntityEntry.GetHashCode();
        }

        #endregion

        #region Hidden Object methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
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
