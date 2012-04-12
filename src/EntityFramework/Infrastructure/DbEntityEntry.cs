namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Validation;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     A non-generic version of the <see cref = "DbEntityEntry{T}" /> class.
    /// </summary>
    public class DbEntityEntry
    {
        #region Fields and constructors

        private readonly InternalEntityEntry _internalEntityEntry;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbEntityEntry" /> class.
        /// </summary>
        /// <param name = "internalEntityEntry">The internal entry.</param>
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
        /// <value>The entity.</value>
        public object Entity
        {
            get { return _internalEntityEntry.Entity; }
        }

        #endregion

        #region Entity state

        /// <summary>
        ///     Gets or sets the state of the entity.
        /// </summary>
        /// <value>The state.</value>
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
        /// <value>The current values.</value>
        public DbPropertyValues CurrentValues
        {
            get { return new DbPropertyValues(_internalEntityEntry.CurrentValues); }
        }

        /// <summary>
        ///     Gets the original property values for the tracked entity represented by this object.
        ///     The original values are usually the entity's property values as they were when last queried from
        ///     the database.
        /// </summary>
        /// <value>The original values.</value>
        public DbPropertyValues OriginalValues
        {
            get { return new DbPropertyValues(_internalEntityEntry.OriginalValues); }
        }

        /// <summary>
        ///     Queries the database for copies of the values of the tracked entity as they currently exist in the database.
        ///     Note that changing the values in the returned dictionary will not update the values in the database.
        ///     If the entity is not found in the database then null is returned.
        /// </summary>
        /// <returns>The store values.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public DbPropertyValues GetDatabaseValues()
        {
            var storeValues = _internalEntityEntry.GetDatabaseValues();
            return storeValues == null ? null : new DbPropertyValues(storeValues);
        }

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
        /// <param name = "navigationProperty">The name of the navigation property.</param>
        /// <returns>An object representing the navigation property.</returns>
        public DbReferenceEntry Reference(string navigationProperty)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(navigationProperty));

            return DbReferenceEntry.Create(_internalEntityEntry.Reference(navigationProperty));
        }

        /// <summary>
        ///     Gets an object that represents the collection navigation property from this
        ///     entity to a collection of related entities.
        /// </summary>
        /// <param name = "navigationProperty">The name of the navigation property.</param>
        /// <returns>An object representing the navigation property.</returns>
        public DbCollectionEntry Collection(string navigationProperty)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(navigationProperty));

            return DbCollectionEntry.Create(_internalEntityEntry.Collection(navigationProperty));
        }

        /// <summary>
        ///     Gets an object that represents a scalar or complex property of this entity.
        /// </summary>
        /// <param name = "propertyName">The name of the property.</param>
        /// <returns>An object representing the property.</returns>
        public DbPropertyEntry Property(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return DbPropertyEntry.Create(_internalEntityEntry.Property(propertyName));
        }

        /// <summary>
        ///     Gets an object that represents a complex property of this entity.
        /// </summary>
        /// <param name = "propertyName">The name of the complex property.</param>
        /// <returns>An object representing the complex property.</returns>
        public DbComplexPropertyEntry ComplexProperty(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return DbComplexPropertyEntry.Create(
                _internalEntityEntry.Property(propertyName, null, requireComplex: true));
        }

        /// <summary>
        ///     Gets an object that represents a member of the entity.  The runtime type of the returned object will
        ///     vary depending on what kind of member is asked for.  The currently supported member types and their return
        ///     types are:
        ///     Reference navigation property: <see cref = "DbReferenceEntry" />.
        ///     Collection navigation property: <see cref = "DbCollectionEntry" />.
        ///     Primitive/scalar property: <see cref = "DbPropertyEntry" />.
        ///     Complex property: <see cref = "DbComplexPropertyEntry" />.
        /// </summary>
        /// <param name = "propertyName">The name of the member.</param>
        /// <returns>An object representing the member.</returns>
        public DbMemberEntry Member(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return DbMemberEntry.Create(_internalEntityEntry.Member(propertyName));
        }

        #endregion

        #region Conversion to generic

        /// <summary>
        ///     Returns a new instance of the generic <see cref = "DbEntityEntry{T}" /> class for the given
        ///     generic type for the tracked entity represented by this object.
        ///     Note that the type of the tracked entity must be compatible with the generic type or
        ///     an exception will be thrown.
        /// </summary>
        /// <typeparam name = "TEntity">The type of the entity.</typeparam>
        /// <returns>A generic version.</returns>
        public DbEntityEntry<TEntity> Cast<TEntity>() where TEntity : class
        {
            if (!typeof(TEntity).IsAssignableFrom(_internalEntityEntry.EntityType))
            {
                throw Error.DbEntity_BadTypeForCast(
                    typeof(DbEntityEntry).Name, typeof(TEntity).Name, _internalEntityEntry.EntityType.Name);
            }

            return new DbEntityEntry<TEntity>(_internalEntityEntry);
        }

        #endregion

        #region Validation

        /// <summary>
        ///     Validates this <see cref = "DbEntityEntry" /> instance and returns validation result.
        /// </summary>
        /// <returns>
        ///     Entity validation result. Possibly null if 
        ///     <see cref = "DbContext.ValidateEntity(DbEntityEntry, IDictionary{object,object})" /> method is overridden.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public DbEntityValidationResult GetValidationResult()
        {
            // need to call the method on DbContext to pickup validation 
            // customizations the user potentially implemented
            return _internalEntityEntry.InternalContext.Owner.CallValidateEntity(this);
        }

        #endregion

        #region InternalEntityEntry access

        /// <summary>
        ///     Gets InternalEntityEntry object for this DbEntityEntry instance.
        /// </summary>
        internal InternalEntityEntry InternalEntry
        {
            get { return _internalEntityEntry; }
        }

        #endregion

        #region Equals\GetHashCode implementation

        /// <summary>
        ///     Determines whether the specified <see cref = "System.Object" /> is equal to this instance.
        ///     Two <see cref = "DbEntityEntry" /> instances are considered equal if they are both entries for
        ///     the same entity on the same <see cref = "DbContext" />.
        /// </summary>
        /// <param name = "obj">The <see cref = "System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref = "System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // Still hide it since it is generally not useful to see when dotting in the API.
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)
                || obj.GetType() != typeof(DbEntityEntry))
            {
                return false;
            }

            return Equals((DbEntityEntry)obj);
        }

        /// <summary>
        ///     Determines whether the specified <see cref = "DbEntityEntry" /> is equal to this instance.
        ///     Two <see cref = "DbEntityEntry" /> instances are considered equal if they are both entries for
        ///     the same entity on the same <see cref = "DbContext" />.
        /// </summary>
        /// <param name = "other">The <see cref = "DbEntityEntry" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref = "DbEntityEntry" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // Still hide it since it is generally not useful to see when dotting in the API.
        public bool Equals(DbEntityEntry other)
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
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
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
