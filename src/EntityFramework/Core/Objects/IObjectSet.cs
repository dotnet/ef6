// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Defines behavior for implementations of IQueryable that allow modifications to the membership of the resulting set.
    /// </summary>
    /// <typeparam name="TEntity"> Type of entities returned from the queryable. </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface IObjectSet<TEntity> : IQueryable<TEntity>
        where TEntity : class
    {
        /// <summary>Notifies the set that an object that represents a new entity must be added to the set.</summary>
        /// <remarks>
        ///     Depending on the implementation, the change to the set may not be visible in an enumeration of the set
        ///     until changes to that set have been persisted in some manner.
        /// </remarks>
        /// <param name="entity">The new object to add to the set.</param>
        void AddObject(TEntity entity);

        /// <summary>Notifies the set that an object that represents an existing entity must be added to the set.</summary>
        /// <remarks>
        ///     Depending on the implementation, the change to the set may not be visible in an enumeration of the set
        ///     until changes to that set have been persisted in some manner.
        /// </remarks>
        /// <param name="entity">The existing object to add to the set.</param>
        void Attach(TEntity entity);

        /// <summary>Notifies the set that an object that represents an existing entity must be deleted from the set. </summary>
        /// <remarks>
        ///     Depending on the implementation, the change to the set may not be visible in an enumeration of the set
        ///     until changes to that set have been persisted in some manner.
        /// </remarks>
        /// <param name="entity">The existing object to delete from the set.</param>
        void DeleteObject(TEntity entity);

        /// <summary>Notifies the set that an object that represents an existing entity must be detached from the set.</summary>
        /// <remarks>
        ///     Depending on the implementation, the change to the set may not be visible in an enumeration of the set
        ///     until changes to that set have been persisted in some manner.
        /// </remarks>
        /// <param name="entity">The object to detach from the set.</param>
        void Detach(TEntity entity);
    }
}
