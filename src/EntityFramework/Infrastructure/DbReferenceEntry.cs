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
    /// A non-generic version of the <see cref="DbReferenceEntry{TEntity, TProperty}" /> class.
    /// </summary>
    public class DbReferenceEntry : DbMemberEntry
    {
        #region Fields and constructors

        private readonly InternalReferenceEntry _internalReferenceEntry;

        /// <summary>
        /// Creates a <see cref="DbReferenceEntry" /> from information in the given <see cref="InternalReferenceEntry" />.
        /// Use this method in preference to the constructor since it may potentially create a subclass depending on
        /// the type of member represented by the InternalCollectionEntry instance.
        /// </summary>
        /// <param name="internalReferenceEntry"> The internal reference entry. </param>
        /// <returns> The new entry. </returns>
        internal static DbReferenceEntry Create(InternalReferenceEntry internalReferenceEntry)
        {
            DebugCheck.NotNull(internalReferenceEntry);

            return (DbReferenceEntry)internalReferenceEntry.CreateDbMemberEntry();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbReferenceEntry" /> class.
        /// </summary>
        /// <param name="internalReferenceEntry"> The internal entry. </param>
        internal DbReferenceEntry(InternalReferenceEntry internalReferenceEntry)
        {
            DebugCheck.NotNull(internalReferenceEntry);

            _internalReferenceEntry = internalReferenceEntry;
        }

        #endregion

        #region Name

        /// <summary>
        /// Gets the property name.
        /// </summary>
        /// <value> The property name. </value>
        public override string Name
        {
            get { return _internalReferenceEntry.Name; }
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
            get { return _internalReferenceEntry.CurrentValue; }
            set { _internalReferenceEntry.CurrentValue = value; }
        }

        #endregion

        #region Loading

        /// <summary>
        /// Loads the entity from the database.
        /// Note that if the entity already exists in the context, then it will not overwritten with values from the database.
        /// </summary>
        public void Load()
        {
            _internalReferenceEntry.Load();
        }

#if !NET40

        /// <summary>
        /// Asynchronously loads the entity from the database.
        /// Note that if the entity already exists in the context, then it will not overwritten with values from the database.
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
        /// Asynchronously loads the entity from the database.
        /// Note that if the entity already exists in the context, then it will not overwritten with values from the database.
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
            return _internalReferenceEntry.LoadAsync(cancellationToken);
        }

#endif

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been loaded from the database.
        /// </summary>
        /// <remarks>
        /// Loading the related entity from the database either using lazy-loading, as part of a query, or explicitly
        /// with one of the Load methods will set the IsLoaded flag to true.
        /// IsLoaded can be explicitly set to true to prevent the related entity from being lazy-loaded.
        /// Note that explict loading using one of the Load methods will load the related entity from the database
        /// regardless of whether or not IsLoaded is true.
        /// When a related entity is detached the IsLoaded flag is reset to false indicating that the related entity is
        /// no longer loaded.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the entity is loaded or the IsLoaded has been explicitly set to true; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded
        {
            get { return _internalReferenceEntry.IsLoaded; }
            set { _internalReferenceEntry.IsLoaded = value; }
        }

        #endregion

        #region Query

        /// <summary>
        /// Returns the query that would be used to load this entity from the database.
        /// The returned query can be modified using LINQ to perform filtering or operations in the database.
        /// </summary>
        /// <returns> A query for the entity. </returns>
        public IQueryable Query()
        {
            return _internalReferenceEntry.Query();
        }

        #endregion

        #region Back references

        /// <summary>
        /// The <see cref="DbEntityEntry" /> to which this navigation property belongs.
        /// </summary>
        /// <value> An entry for the entity that owns this navigation property. </value>
        public override DbEntityEntry EntityEntry
        {
            get { return new DbEntityEntry(_internalReferenceEntry.InternalEntityEntry); }
        }

        #endregion

        #region InternalMemberEntry access

        /// <summary>
        /// Gets the <see cref="InternalReferenceEntry" /> backing this object as an <see cref="InternalMemberEntry" />.
        /// </summary>
        /// <value> The internal member entry. </value>
        internal override InternalMemberEntry InternalMemberEntry
        {
            get { return _internalReferenceEntry; }
        }

        #endregion

        #region Conversion to generic

        /// <summary>
        /// Returns the equivalent generic <see cref="DbReferenceEntry{TEntity,TProperty}" /> object.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity on which the member is declared. </typeparam>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <returns> The equivalent generic object. </returns>
        public new DbReferenceEntry<TEntity, TProperty> Cast<TEntity, TProperty>() where TEntity : class
        {
            var metadata = _internalReferenceEntry.EntryMetadata;
            if (!typeof(TEntity).IsAssignableFrom(metadata.DeclaringType)
                || !typeof(TProperty).IsAssignableFrom(metadata.ElementType))
            {
                throw Error.DbMember_BadTypeForCast(
                    typeof(DbReferenceEntry).Name,
                    typeof(TEntity).Name,
                    typeof(TProperty).Name,
                    metadata.DeclaringType.Name,
                    metadata.MemberType.Name);
            }

            return DbReferenceEntry<TEntity, TProperty>.Create(_internalReferenceEntry);
        }

        #endregion
    }
}
