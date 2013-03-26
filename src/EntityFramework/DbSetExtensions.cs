// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity
{
    using System.Data.Entity.Utilities;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Useful extension methods for <see cref="DbSet{TEntity}"/>.
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        ///     Asynchronously finds an entity with the given primary key values.
        ///     If an entity with the given primary key values exists in the context, then it is
        ///     returned immediately without making a request to the store.  Otherwise, a request
        ///     is made to the store for an entity with the given primary key values and this entity,
        ///     if found, is attached to the context and returned.  If no entity is found in the
        ///     context or the store, then null is returned.
        /// </summary>
        /// <remarks>
        ///     The ordering of composite key values is as defined in the EDM, which is in turn as defined in
        ///     the designer, by the Code First fluent API, or by the DataMember attribute.
        ///     
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure 
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TEntity"> The type that defines the set. </typeparam>
        /// <param name="set"> The set to find entities from. </param>
        /// <param name="keyValues"> The values of the primary key for the entity to be found. </param>
        /// <returns> A task that represents the asynchronous find operation. The task result contains the entity found, or null. </returns>
        public static Task<TEntity> FindAsync<TEntity>(this IDbSet<TEntity> set, params object[] keyValues)
            where TEntity : class
        {
            Check.NotNull(set, "set");

            return set.FindAsync(CancellationToken.None, keyValues);
        }
    }
}

#endif
