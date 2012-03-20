namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Validation;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     This is an abstract base class use to represent a scalar or complex property, or a navigation property
    ///     of an entity.  Scalar and complex properties use the derived class <see cref = "DbPropertyEntry{TEntity, TProperty}" />,
    ///     reference navigation properties use the derived class <see cref = "DbReferenceEntry{TEntity, TProperty}" />, and collection
    ///     navigation properties use the derived class <see cref = "DbCollectionEntry{TEntity, TProperty}" />.
    /// </summary>
    /// <typeparam name = "TEntity">The type of the entity to which this property belongs.</typeparam>
    /// <typeparam name = "TProperty">The type of the property.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db",
        Justification = "FxCop rule is wrong; Database is not two words.")]
    public abstract class DbMemberEntry<TEntity, TProperty>
        where TEntity : class
    {
        #region  Factory methods

        /// <summary>
        ///     Creates a <see cref = "DbMemberEntry{TEntity,TProperty}" /> from information in the given <see cref = "InternalMemberEntry" />.
        ///     This method will create an instance of the appropriate subclass depending on the metadata contained
        ///     in the InternalMemberEntry instance.
        /// </summary>
        /// <param name = "internalMemberEntry">The internal member entry.</param>
        /// <returns>The new entry.</returns>
        internal static DbMemberEntry<TEntity, TProperty> Create(InternalMemberEntry internalMemberEntry)
        {
            Contract.Requires(internalMemberEntry != null);

            return internalMemberEntry.CreateDbMemberEntry<TEntity, TProperty>();
        }

        #endregion

        #region Name

        public abstract string Name { get; }

        #endregion

        #region Current values

        /// <summary>
        ///     Gets or sets the current value of this property.
        /// </summary>
        /// <value>The current value.</value>
        public abstract TProperty CurrentValue { get; set; }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        ///     Returns a new instance of the non-generic <see cref = "DbMemberEntry" /> class for 
        ///     the property represented by this object.
        /// </summary>
        /// <returns>A non-generic version.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbMemberEntry(DbMemberEntry<TEntity, TProperty> entry)
        {
            return DbMemberEntry.Create(entry.InternalMemberEntry);
        }

        #endregion

        #region Internal entry access

        /// <summary>
        ///     Gets the underlying <see cref = "InternalMemberEntry" />.
        /// </summary>
        /// <value>The internal member entry.</value>
        internal abstract InternalMemberEntry InternalMemberEntry { get; }

        #endregion

        #region Back references

        /// <summary>
        ///     The <see cref = "DbEntityEntry{TEntity}" /> to which this member belongs.
        /// </summary>
        /// <value>An entry for the entity that owns this member.</value>
        public abstract DbEntityEntry<TEntity> EntityEntry { get; }

        #endregion

        #region Validation

        /// <summary>
        ///     Validates this property.
        /// </summary>
        /// <returns>
        ///     Collection of <see cref = "DbValidationError" /> objects. Never null. If the entity is valid the collection will be empty.
        /// </returns>
        public ICollection<DbValidationError> GetValidationErrors()
        {
            return InternalMemberEntry.GetValidationErrors().ToList();
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion
    }
}