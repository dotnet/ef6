namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Instances of this class are returned from the Reference method of
    ///     <see cref = "DbEntityEntry{T}" /> and allow operations such as loading to
    ///     be performed on the an entity's reference navigation properties.
    /// </summary>
    /// <typeparam name = "TEntity">The type of the entity to which this property belongs.</typeparam>
    /// <typeparam name = "TProperty">The type of the property.</typeparam>
    public class DbReferenceEntry<TEntity, TProperty> : DbMemberEntry<TEntity, TProperty>
        where TEntity : class
    {
        #region Fields and constructors

        private readonly InternalReferenceEntry _internalReferenceEntry;

        /// <summary>
        ///     Creates a <see cref = "DbReferenceEntry{TEntity,TProperty}" /> from information in the given <see cref = "InternalReferenceEntry" />.
        ///     Use this method in preference to the constructor since it may potentially create a subclass depending on
        ///     the type of member represented by the InternalCollectionEntry instance.
        /// </summary>
        /// <param name = "internalReferenceEntry">The internal reference entry.</param>
        /// <returns>The new entry.</returns>
        internal static DbReferenceEntry<TEntity, TProperty> Create(InternalReferenceEntry internalReferenceEntry)
        {
            Contract.Requires(internalReferenceEntry != null);

            return
                (DbReferenceEntry<TEntity, TProperty>)internalReferenceEntry.CreateDbMemberEntry<TEntity, TProperty>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbReferenceEntry{TEntity, TProperty}" /> class.
        /// </summary>
        /// <param name = "internalReferenceEntry">The internal entry.</param>
        internal DbReferenceEntry(InternalReferenceEntry internalReferenceEntry)
        {
            Contract.Requires(internalReferenceEntry != null);

            _internalReferenceEntry = internalReferenceEntry;
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets the property name.
        /// </summary>
        /// <value>The property name.</value>
        public override string Name
        {
            get { return _internalReferenceEntry.Name; }
        }

        #endregion

        #region Current values

        /// <summary>
        ///     Gets or sets the current value of the navigation property.  The current value is
        ///     the entity that the navigation property references.
        /// </summary>
        /// <value>The current value.</value>
        public override TProperty CurrentValue
        {
            get { return (TProperty)_internalReferenceEntry.CurrentValue; }
            set { _internalReferenceEntry.CurrentValue = value; }
        }

        #endregion

        #region Loading

        /// <summary>
        ///     Loads the entity from the database.
        ///     Note that if the entity already exists in the context, then it will not overwritten with values from the database.
        /// </summary>
        public void Load()
        {
            _internalReferenceEntry.Load();
        }

        /// <summary>
        ///     Gets a value indicating whether the entity has been loaded from the database.
        /// </summary>
        /// <value><c>true</c> if the entity is loaded; otherwise, <c>false</c>.</value>
        public bool IsLoaded
        {
            get { return _internalReferenceEntry.IsLoaded; }
        }

        #endregion

        #region Query

        /// <summary>
        ///     Returns the query that would be used to load this entity from the database.
        ///     The returned query can be modified using LINQ to perform filtering or operations in the database.
        /// </summary>
        /// <returns>A query for the entity.</returns>
        public IQueryable<TProperty> Query()
        {
            return (IQueryable<TProperty>)_internalReferenceEntry.Query();
        }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        ///     Returns a new instance of the non-generic <see cref = "DbReferenceEntry" /> class for 
        ///     the navigation property represented by this object.
        /// </summary>
        /// <returns>A non-generic version.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbReferenceEntry(DbReferenceEntry<TEntity, TProperty> entry)
        {
            return DbReferenceEntry.Create(entry._internalReferenceEntry);
        }

        #endregion

        #region Internal entry access

        /// <summary>
        ///     Gets the underlying <see cref = "InternalReferenceEntry" /> as an <see cref = "InternalMemberEntry" />.
        /// </summary>
        /// <value>The internal member entry.</value>
        internal override InternalMemberEntry InternalMemberEntry
        {
            get { return _internalReferenceEntry; }
        }

        #endregion

        #region Back references

        /// <summary>
        ///     The <see cref = "DbEntityEntry{TEntity}" /> to which this navigation property belongs.
        /// </summary>
        /// <value>An entry for the entity that owns this navigation property.</value>
        public override DbEntityEntry<TEntity> EntityEntry
        {
            get { return new DbEntityEntry<TEntity>(_internalReferenceEntry.InternalEntityEntry); }
        }

        #endregion
    }
}
