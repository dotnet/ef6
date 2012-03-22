namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     A non-generic version of the <see cref = "DbPropertyEntry{TEntity, TProperty}" /> class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db",
        Justification = "FxCop rule is wrong; Database is not two words.")]
    public class DbPropertyEntry : DbMemberEntry
    {
        #region Fields and constructors

        private readonly InternalPropertyEntry _internalPropertyEntry;

        /// <summary>
        ///     Creates a <see cref = "DbPropertyEntry" /> from information in the given <see cref = "InternalPropertyEntry" />.
        ///     Use this method in preference to the constructor since it may potentially create a subclass depending on
        ///     the type of member represented by the InternalCollectionEntry instance.
        /// </summary>
        /// <param name = "internalPropertyEntry">The internal property entry.</param>
        /// <returns>The new entry.</returns>
        internal static DbPropertyEntry Create(InternalPropertyEntry internalPropertyEntry)
        {
            Contract.Requires(internalPropertyEntry != null);

            return (DbPropertyEntry)internalPropertyEntry.CreateDbMemberEntry();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbPropertyEntry" /> class.
        /// </summary>
        /// <param name = "internalPropertyEntry">The internal entry.</param>
        internal DbPropertyEntry(InternalPropertyEntry internalPropertyEntry)
        {
            Contract.Requires(internalPropertyEntry != null);

            _internalPropertyEntry = internalPropertyEntry;
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets the property name.
        /// </summary>
        /// <value>The property name.</value>
        public override string Name
        {
            get { return _internalPropertyEntry.Name; }
        }

        #endregion

        #region Current and Original values

        /// <summary>
        ///     Gets or sets the original value of this property.
        /// </summary>
        /// <value>The original value.</value>
        public object OriginalValue
        {
            get { return _internalPropertyEntry.OriginalValue; }
            set { _internalPropertyEntry.OriginalValue = value; }
        }

        /// <summary>
        ///     Gets or sets the current value of this property.
        /// </summary>
        /// <value>The current value.</value>
        public override object CurrentValue
        {
            get { return _internalPropertyEntry.CurrentValue; }
            set { _internalPropertyEntry.CurrentValue = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the value of this property has been modified since
        ///     it was loaded from the database.
        /// </summary>
        /// <remarks>
        ///     Setting this value to false for a modified property will revert the change by setting the
        ///     current value to the original value. If the result is that no properties of the entity are
        ///     marked as modified, then the entity will be marked as Unchanged.
        ///     Setting this value to false for properties of Added, Unchanged, or Deleted entities
        ///     is a no-op.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if this instance is modified; otherwise, <c>false</c>.
        /// </value>
        public bool IsModified
        {
            get { return _internalPropertyEntry.IsModified; }
            set { _internalPropertyEntry.IsModified = value; }
        }

        #endregion

        #region Back references

        /// <summary>
        ///     The <see cref = "DbEntityEntry" /> to which this property belongs.
        /// </summary>
        /// <value>An entry for the entity that owns this property.</value>
        public override DbEntityEntry EntityEntry
        {
            get { return new DbEntityEntry(_internalPropertyEntry.InternalEntityEntry); }
        }

        /// <summary>
        ///     The <see cref = "DbPropertyEntry" /> of the property for which this is a nested property.
        ///     This method will only return a non-null entry for properties of complex objects; it will
        ///     return null for properties of the entity itself.
        /// </summary>
        /// <value>An entry for the parent complex property, or null if this is an entity property.</value>
        public DbComplexPropertyEntry ParentProperty
        {
            get
            {
                var propertyEntry = _internalPropertyEntry.ParentPropertyEntry;
                return propertyEntry != null ? DbComplexPropertyEntry.Create(propertyEntry) : null;
            }
        }

        #endregion

        #region InternalMemberEntry access

        /// <summary>
        ///     Gets the <see cref = "InternalPropertyEntry" /> backing this object.
        /// </summary>
        /// <value>The internal member entry.</value>
        internal override InternalMemberEntry InternalMemberEntry
        {
            get { return _internalPropertyEntry; }
        }

        #endregion

        #region Conversion to generic

        /// <summary>
        ///     Returns the equivalent generic <see cref = "DbPropertyEntry{TEntity,TProperty}" /> object.
        /// </summary>
        /// <typeparam name = "TEntity">The type of entity on which the member is declared.</typeparam>
        /// <typeparam name = "TProperty">The type of the property.</typeparam>
        /// <returns>The equivalent generic object.</returns>
        public new DbPropertyEntry<TEntity, TProperty> Cast<TEntity, TProperty>() where TEntity : class
        {
            var metadata = _internalPropertyEntry.EntryMetadata;
            if (!typeof(TEntity).IsAssignableFrom(metadata.DeclaringType)
                || !typeof(TProperty).IsAssignableFrom(metadata.ElementType))
            {
                throw Error.DbMember_BadTypeForCast(
                    typeof(DbPropertyEntry).Name,
                    typeof(TEntity).Name,
                    typeof(TProperty).Name,
                    metadata.DeclaringType.Name,
                    metadata.MemberType.Name);
            }

            return DbPropertyEntry<TEntity, TProperty>.Create(_internalPropertyEntry);
        }

        #endregion
    }
}
