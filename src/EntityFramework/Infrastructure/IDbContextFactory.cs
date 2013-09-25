// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// A factory for creating derived <see cref="DbContext" /> instances. Implement this
    /// interface to enable design-time services for context types that do not have a
    /// public default constructor.
    /// At design-time, derived <see cref="DbContext" /> instances can be created in order to enable specific
    /// design-time experiences such as model rendering, DDL generation etc. To enable design-time instantiation
    /// for derived <see cref="DbContext" /> types that do not have a public, default constructor, implement
    /// this interface. Design-time services will auto-discover implementations of this interface that are in the
    /// same assembly as the derived <see cref="DbContext" /> type.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface IDbContextFactory<out TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Creates a new instance of a derived <see cref="DbContext" /> type.
        /// </summary>
        /// <returns> An instance of TContext </returns>
        TContext Create();
    }
}
