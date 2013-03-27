// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Instances of this class are returned from the Collection method of
    ///     <see cref="DbEntityEntry{T}" /> and allow operations such as loading to
    ///     be performed on the an entity's collection navigation properties.
    /// </summary>
    /// <typeparam name="TEntity"> The type of the entity to which this property belongs. </typeparam>
    /// <typeparam name="TElement"> The type of the element in the collection of entities. </typeparam>
    public class DbCollectionEntry<TEntity, TElement> : DbMemberEntry<TEntity, ICollection<TElement>>
        where TEntity : class
    {
        #region Fields and constructors

        private readonly InternalCollectionEntry _internalCollectionEntry;

        /// <summary>
        ///     Creates a <see cref="DbCollectionEntry{TEntity,TElement}" /> from information in the given
        ///     <see
        ///         cref="InternalCollectionEntry" />
        ///     .
        ///     Use this method in preference to the constructor since it may potentially create a subclass depending on
        ///     the type of member represented by the InternalCollectionEntry instance.
        /// </summary>
        /// <param name="internalCollectionEntry"> The internal collection entry. </param>
        /// <returns> The new entry. </returns>
        internal static DbCollectionEntry<TEntity, TElement> Create(InternalCollectionEntry internalCollectionEntry)
        {
            DebugCheck.NotNull(internalCollectionEntry);

            // Note that the implementation of this Create method is different than for the other DbMemberEntry classes.
            // This is because the DbMemberEntry is defined in terms of the ICollection<TElement> while this class
            // is defined in terms of just TElement.  This means that we can't just call the CreateDbMemberEntry factory
            // method on InternalMemberEntry.  Instead we call the special factory method on InternalCollectionEntry.
            return internalCollectionEntry.CreateDbCollectionEntry<TEntity, TElement>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbCollectionEntry{TEntity, TProperty}" /> class.
        /// </summary>
        /// <param name="internalCollectionEntry"> The internal entry. </param>
        internal DbCollectionEntry(InternalCollectionEntry internalCollectionEntry)
        {
            DebugCheck.NotNull(internalCollectionEntry);

            _internalCollectionEntry = internalCollectionEntry;
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets the property name.
        /// </summary>
        /// <value> The property name. </value>
        public override string Name
        {
            get { return _internalCollectionEntry.Name; }
        }

        #endregion

        #region Current values

        /// <summary>
        ///     Gets or sets the current value of the navigation property.  The current value is
        ///     the entity that the navigation property references.
        /// </summary>
        /// <value> The current value. </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public override ICollection<TElement> CurrentValue
        {
            get { return (ICollection<TElement>)_internalCollectionEntry.CurrentValue; }
            set { _internalCollectionEntry.CurrentValue = value; }
        }

        #endregion

        #region Loading

        /// <summary>
        ///     Loads the collection of entities from the database.
        ///     Note that entities that already exist in the context are not overwritten with values from the database.
        /// </summary>
        public void Load()
        {
            _internalCollectionEntry.Load();
        }

#if !NET40

        /// <summary>
        ///     Asynchronously loads the collection of entities from the database.
        ///     Note that entities that already exist in the context are not overwritten with values from the database.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public Task LoadAsync()
        {
            return LoadAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Asynchronously loads the collection of entities from the database.
        ///     Note that entities that already exist in the context are not overwritten with values from the database.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public Task LoadAsync(CancellationToken cancellationToken)
        {
            return _internalCollectionEntry.LoadAsync(cancellationToken);
        }

#endif

        /// <summary>
        ///     Gets or sets a value indicating whether all entities of this collection have been loaded from the database.
        /// </summary>
        /// <remarks>
        ///     Loading the related entities from the database either using lazy-loading, as part of a query, or explicitly
        ///     with one of the Load methods will set the IsLoaded flag to true.
        ///     IsLoaded can be explicitly set to true to prevent the related entities of this collection from being lazy-loaded.
        ///     This can be useful if the application has caused a subset of related entities to be loaded into this collection
        ///     and wants to prevent any other entities from being loaded automatically.
        ///     Note that explict loading using one of the Load methods will load all related entities from the database
        ///     regardless of whether or not IsLoaded is true.
        ///     When any related entity in the collection is detached the IsLoaded flag is reset to false indicating that the
        ///     not all related entities are now loaded.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if all the related entities are loaded or the IsLoaded has been explicitly set to true; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded
        {
            get { return _internalCollectionEntry.IsLoaded; }
            set { _internalCollectionEntry.IsLoaded = value; }
        }

        /// <summary>
        ///     Returns the query that would be used to load this collection from the database.
        ///     The returned query can be modified using LINQ to perform filtering or operations in the database, such
        ///     as counting the number of entities in the collection in the database without actually loading them.
        /// </summary>
        /// <returns> A query for the collection. </returns>
        public IQueryable<TElement> Query()
        {
            return (IQueryable<TElement>)_internalCollectionEntry.Query();
        }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        ///     Returns a new instance of the non-generic <see cref="DbCollectionEntry" /> class for
        ///     the navigation property represented by this object.
        /// </summary>
        /// <returns> A non-generic version. </returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbCollectionEntry(DbCollectionEntry<TEntity, TElement> entry)
        {
            return DbCollectionEntry.Create(entry._internalCollectionEntry);
        }

        #endregion

        #region Internal entry access

        /// <summary>
        ///     Gets the underlying <see cref="InternalCollectionEntry" /> as an <see cref="InternalMemberEntry" />.
        /// </summary>
        /// <value> The internal member entry. </value>
        internal override InternalMemberEntry InternalMemberEntry
        {
            get { return _internalCollectionEntry; }
        }

        #endregion

        #region Back references

        /// <summary>
        ///     The <see cref="DbEntityEntry{TEntity}" /> to which this navigation property belongs.
        /// </summary>
        /// <value> An entry for the entity that owns this navigation property. </value>
        public override DbEntityEntry<TEntity> EntityEntry
        {
            get { return new DbEntityEntry<TEntity>(_internalCollectionEntry.InternalEntityEntry); }
        }

        #endregion
    }
}
