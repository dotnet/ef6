// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;

    /// <summary>
    /// A service for obtaining the correct <see cref="DbProviderFactory" /> from a given
    /// <see cref="DbConnection" />.
    /// </summary>
    /// <remarks>
    /// On .NET 4.5 the provider is publicly accessible from the connection. On .NET 4 the
    /// default implementation of this service uses some heuristics to find the matching
    /// provider. If these fail then a new implementation of this service can be registered
    /// on <see cref="DbConfiguration" /> to provide an appropriate resolution.
    /// </remarks>
    public interface IDbProviderFactoryResolver
    {
        /// <summary>
        /// Returns the <see cref="DbProviderFactory" /> for the given connection.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <returns> The provider factory for the connection. </returns>
        DbProviderFactory ResolveProviderFactory(DbConnection connection);
    }
}
