// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Instances of this class are returned from the Property method of
    /// <see cref="DbEntityEntry{T}" /> and allow access to the state of the scalar
    /// or complex property.
    /// </summary>
    /// <typeparam name="TEntity"> The type of the entity to which this property belongs. </typeparam>
    /// <typeparam name="TProperty"> The type of the property. </typeparam>
    public class DbPropertyEntry<TEntity, TProperty> : DbMemberEntry<TEntity, TProperty>
        where TEntity : class
    {
        #region Fields and constructors

        private readonly InternalPropertyEntry _internalPropertyEntry;

        // <summary>
        // Creates a <see cref="DbPropertyEntry{TEntity,TProperty}" /> from information in the given
        // <see
        //     cref="InternalPropertyEntry" />
        // .
        // Use this method in preference to the constructor since it may potentially create a subclass depending on
        // the type of member represented by the InternalCollectionEntry instance.
        // </summary>
        // <param name="internalPropertyEntry"> The internal property entry. </param>
        // <returns> The new entry. </returns>
        internal static DbPropertyEntry<TEntity, TProperty> Create(InternalPropertyEntry internalPropertyEntry)
        {
            DebugCheck.NotNull(internalPropertyEntry);

            return (DbPropertyEntry<TEntity, TProperty>)internalPropertyEntry.CreateDbMemberEntry<TEntity, TProperty>();
        }

        // <summary>
        // Initializes a new instance of the <see cref="DbPropertyEntry{TEntity, TProperty}" /> class.
        // </summary>
        // <param name="internalPropertyEntry"> The internal entry. </param>
        internal DbPropertyEntry(InternalPropertyEntry internalPropertyEntry)
        {
            DebugCheck.NotNull(internalPropertyEntry);

            _internalPropertyEntry = internalPropertyEntry;
        }

        #endregion

        #region Name

        /// <summary>
        /// Gets the property name.
        /// </summary>
        /// <value> The property name. </value>
        public override string Name
        {
            get { return _internalPropertyEntry.Name; }
        }

        #endregion

        #region Current and Original values

        /// <summary>
        /// Gets or sets the original value of this property.
        /// </summary>
        /// <value> The original value. </value>
        public TProperty OriginalValue
        {
            get { return (TProperty)_internalPropertyEntry.OriginalValue; }
            set { _internalPropertyEntry.OriginalValue = value; }
        }

        /// <summary>
        /// Gets or sets the current value of this property.
        /// </summary>
        /// <value> The current value. </value>
        public override TProperty CurrentValue
        {
            get { return (TProperty)_internalPropertyEntry.CurrentValue; }
            set { _internalPropertyEntry.CurrentValue = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value of this property has been modified since
        /// it was loaded from the database.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is modified; otherwise, <c>false</c> .
        /// </value>
        public bool IsModified
        {
            get { return _internalPropertyEntry.IsModified; }
            set { _internalPropertyEntry.IsModified = value; }
        }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        /// Returns a new instance of the non-generic <see cref="DbPropertyEntry" /> class for
        /// the property represented by this object.
        /// </summary>
        /// <returns> A non-generic version. </returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbPropertyEntry(DbPropertyEntry<TEntity, TProperty> entry)
        {
            return DbPropertyEntry.Create(entry._internalPropertyEntry);
        }

        #endregion

        #region Back references

        /// <summary>
        /// The <see cref="DbEntityEntry{TEntity}" /> to which this property belongs.
        /// </summary>
        /// <value> An entry for the entity that owns this property. </value>
        public override DbEntityEntry<TEntity> EntityEntry
        {
            get { return new DbEntityEntry<TEntity>(_internalPropertyEntry.InternalEntityEntry); }
        }

        /// <summary>
        /// The <see cref="DbPropertyEntry" /> of the property for which this is a nested property.
        /// This method will only return a non-null entry for properties of complex objects; it will
        /// return null for properties of the entity itself.
        /// </summary>
        /// <value> An entry for the parent complex property, or null if this is an entity property. </value>
        public DbComplexPropertyEntry ParentProperty
        {
            get
            {
                var propertyEntry = _internalPropertyEntry.ParentPropertyEntry;
                return propertyEntry != null ? DbComplexPropertyEntry.Create(propertyEntry) : null;
            }
        }

        #endregion

        #region Internal entry access

        internal InternalPropertyEntry InternalPropertyEntry
        {
            get { return _internalPropertyEntry; }
        }

        // <summary>
        // Gets the underlying <see cref="InternalPropertyEntry" /> as an <see cref="InternalMemberEntry" />.
        // </summary>
        // <value> The internal member entry. </value>
        internal override InternalMemberEntry InternalMemberEntry
        {
            get { return InternalPropertyEntry; }
        }

        #endregion
    }
}
