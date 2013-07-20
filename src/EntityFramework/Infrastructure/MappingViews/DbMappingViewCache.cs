// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.MappingViews
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Base abstract class for mapping view cache implementations.
    ///     Derived classes must have a parameterless constructor if used with <see cref="DbMappingViewCacheTypeAttribute" />.
    /// </summary>
    public abstract class DbMappingViewCache
    {
        /// <summary>
        ///     Gets a hash value computed over the mapping closure.
        /// </summary>
        public abstract string MappingHashValue { get; }

        /// <summary>
        ///     Gets a view corresponding to the specified extent.
        /// </summary>
        /// <param name="extent">An <see cref="EntitySetBase"/> that specifies the extent.</param>
        /// <returns>A <see cref="DbMappingView"/> that specifies the mapping view.</returns>
        public abstract DbMappingView GetView(EntitySetBase extent);
    }
}
