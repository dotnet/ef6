// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents one end of a relationship.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface IRelatedEnd
    {
        // ----------
        // Properties
        // ----------

        /// <summary>Gets a value that indicates whether all related objects have been loaded.</summary>
        /// <returns>true if the related end contains all the related entities from the database; otherwise, false.</returns>
        bool IsLoaded { get; set; }

        /// <summary>Gets the name of the relationship in which this related end participates.</summary>
        /// <returns>
        /// The name of the relationship in which this <see cref="T:System.Data.Entity.Core.Objects.DataClasses.IRelatedEnd" /> is participating. The relationship name is not namespace qualified.
        /// </returns>
        string RelationshipName { get; }

        /// <summary>Gets the role name at the source end of the relationship.</summary>
        /// <returns>The role name at the source end of the relationship.</returns>
        string SourceRoleName { get; }

        /// <summary>Gets the role name at the target end of the relationship.</summary>
        /// <returns>The role name at the target end of the relationship.</returns>
        string TargetRoleName { get; }

        /// <summary>Returns a reference to the metadata for the related end.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" /> object that contains metadata for the end of a relationship.
        /// </returns>
        RelationshipSet RelationshipSet { get; }

        // -------
        // Methods
        // -------

        /// <summary>Loads the related object or objects into this related end with the default merge option.</summary>
        void Load();

#if !NET40

        /// <summary>Asynchronously loads the related object or objects into this related end with the default merge option.</summary>
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
        Task LoadAsync(CancellationToken cancellationToken);

#endif

        /// <summary>Loads the related object or objects into the related end with the specified merge option.</summary>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when merging objects into an existing
        /// <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />.
        /// </param>
        void Load(MergeOption mergeOption);

#if !NET40

        /// <summary>Asynchronously loads the related object or objects into the related end with the specified merge option.</summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when merging objects into an existing
        /// <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        Task LoadAsync(MergeOption mergeOption, CancellationToken cancellationToken);

#endif

        /// <summary>Adds an object to the related end.</summary>
        /// <param name="entity">
        /// An object to add to the collection.  entity  must implement
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.IEntityWithRelationships" />
        /// .
        /// </param>
        void Add(IEntityWithRelationships entity);

        /// <summary>Adds an object to the related end.</summary>
        /// <param name="entity">An object to add to the collection.</param>
        void Add(object entity);

        /// <summary>Removes an object from the collection of objects at the related end.</summary>
        /// <returns>
        /// true if  entity  was successfully removed, false if  entity  was not part of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.IRelatedEnd" />
        /// .
        /// </returns>
        /// <param name="entity">
        /// The object to remove from the collection.  entity  must implement
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.IEntityWithRelationships" />
        /// .
        /// </param>
        bool Remove(IEntityWithRelationships entity);

        /// <summary>Removes an object from the collection of objects at the related end.</summary>
        /// <returns>
        /// true if  entity  was successfully removed; false if  entity  was not part of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.IRelatedEnd" />
        /// .
        /// </returns>
        /// <param name="entity">An object to remove from the collection.</param>
        bool Remove(object entity);

        /// <summary>Defines a relationship between two attached objects.</summary>
        /// <param name="entity">
        /// The object being attached.  entity  must implement
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.IEntityWithRelationships" />
        /// .
        /// </param>
        void Attach(IEntityWithRelationships entity);

        /// <summary>Defines a relationship between two attached objects.</summary>
        /// <param name="entity">The object being attached.</param>
        void Attach(object entity);

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IEnumerable" /> that represents the objects that belong to the related end.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerable" /> that represents the objects that belong to the related end.
        /// </returns>
        IEnumerable CreateSourceQuery();

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IEnumerator" /> that iterates through the collection of related objects.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> that iterates through the collection of related objects.
        /// </returns>
        IEnumerator GetEnumerator();
    }
}
