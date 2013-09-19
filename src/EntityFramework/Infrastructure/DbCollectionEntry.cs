// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A non-generic version of the <see cref="DbCollectionEntry{TEntity, TProperty}" /> class.
    /// </summary>
    public class DbCollectionEntry : DbMemberEntry
    {
        #region Fields and constructors

        private readonly InternalCollectionEntry _internalCollectionEntry;

        /// <summary>
        /// Creates a <see cref="DbCollectionEntry" /> from information in the given <see cref="InternalCollectionEntry" />.
        /// Use this method in preference to the constructor since it may potentially create a subclass depending on
        /// the type of member represented by the InternalCollectionEntry instance.
        /// </summary>
        /// <param name="internalCollectionEntry"> The internal collection entry. </param>
        /// <returns> The new entry. </returns>
        internal static DbCollectionEntry Create(InternalCollectionEntry internalCollectionEntry)
        {
            DebugCheck.NotNull(internalCollectionEntry);

            return (DbCollectionEntry)internalCollectionEntry.CreateDbMemberEntry();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbCollectionEntry" /> class.
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
        /// Gets the property name.
        /// </summary>
        /// <value> The property name. </value>
        public override string Name
        {
            get { return _internalCollectionEntry.Name; }
        }

        #endregion

        #region Current values

        /// <summary>
        /// Gets or sets the current value of the navigation property.  The current value is
        /// the entity that the navigation property references.
        /// </summary>
        /// <value> The current value. </value>
        public override object CurrentValue
        {
            get { return _internalCollectionEntry.CurrentValue; }
            set { _internalCollectionEntry.CurrentValue = value; }
        }

        #endregion

        #region Loading

        /// <summary>
        /// Loads the collection of entities from the database.
        /// Note that entities that already exist in the context are not overwritten with values from the database.
        /// </summary>
        public void Load()
        {
            _internalCollectionEntry.Load();
        }

#if !NET40

        /// <summary>
        /// Asynchronously loads the collection of entities from the database.
        /// Note that entities that already exist in the context are not overwritten with values from the database.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public Task LoadAsync()
        {
            return LoadAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously loads the collection of entities from the database.
        /// Note that entities that already exist in the context are not overwritten with values from the database.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public Task LoadAsync(CancellationToken cancellationToken)
        {
            return _internalCollectionEntry.LoadAsync(cancellationToken);
        }

#endif

        /// <summary>
        /// Gets or sets a value indicating whether all entities of this collection have been loaded from the database.
        /// </summary>
        /// <remarks>
        /// Loading the related entities from the database either using lazy-loading, as part of a query, or explicitly
        /// with one of the Load methods will set the IsLoaded flag to true.
        /// IsLoaded can be explicitly set to true to prevent the related entities of this collection from being lazy-loaded.
        /// This can be useful if the application has caused a subset of related entities to be loaded into this collection
        /// and wants to prevent any other entities from being loaded automatically.
        /// Note that explict loading using one of the Load methods will load all related entities from the database
        /// regardless of whether or not IsLoaded is true.
        /// When any related entity in the collection is detached the IsLoaded flag is reset to false indicating that the
        /// not all related entities are now loaded.
        /// </remarks>
        /// <value>
        /// <c>true</c> if all the related entities are loaded or the IsLoaded has been explicitly set to true; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded
        {
            get { return _internalCollectionEntry.IsLoaded; }
            set { _internalCollectionEntry.IsLoaded = value; }
        }

        /// <summary>
        /// Returns the query that would be used to load this collection from the database.
        /// The returned query can be modified using LINQ to perform filtering or operations in the database, such
        /// as counting the number of entities in the collection in the database without actually loading them.
        /// </summary>
        /// <returns> A query for the collection. </returns>
        public IQueryable Query()
        {
            return _internalCollectionEntry.Query();
        }

        #endregion

        #region Back references

        /// <summary>
        /// The <see cref="DbEntityEntry" /> to which this navigation property belongs.
        /// </summary>
        /// <value> An entry for the entity that owns this navigation property. </value>
        public override DbEntityEntry EntityEntry
        {
            get { return new DbEntityEntry(_internalCollectionEntry.InternalEntityEntry); }
        }

        #endregion

        #region InternalMemberEntry access

        /// <summary>
        /// Gets the <see cref="InternalCollectionEntry" /> backing this object as an <see cref="InternalMemberEntry" />.
        /// </summary>
        /// <value> The internal member entry. </value>
        internal override InternalMemberEntry InternalMemberEntry
        {
            get { return _internalCollectionEntry; }
        }

        #endregion

        #region Conversion to generic

        /// <summary>
        /// Returns the equivalent generic <see cref="DbCollectionEntry{TEntity,TElement}" /> object.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity on which the member is declared. </typeparam>
        /// <typeparam name="TElement"> The type of the collection element. </typeparam>
        /// <returns> The equivalent generic object. </returns>
        public new DbCollectionEntry<TEntity, TElement> Cast<TEntity, TElement>() where TEntity : class
        {
            var metadata = _internalCollectionEntry.EntryMetadata;
            if (!typeof(TEntity).IsAssignableFrom(metadata.DeclaringType)
                || !typeof(TElement).IsAssignableFrom(metadata.ElementType))
            {
                throw Error.DbMember_BadTypeForCast(
                    typeof(DbCollectionEntry).Name,
                    typeof(TEntity).Name,
                    typeof(TElement).Name,
                    metadata.DeclaringType.Name,
                    metadata.ElementType.Name);
            }

            return DbCollectionEntry<TEntity, TElement>.Create(_internalCollectionEntry);
        }

        #endregion
    }
}
