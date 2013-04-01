// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;

    /// <summary>
    ///     Implementations of this interface create new instances of <see cref="HistoryContext"/> that are used
    ///     to read and write migrations history data.
    ///     
    ///     To customize the definition of the migrations history table you can derive from 
    ///     <see cref="HistoryContext"/> and override OnModelCreating. Derived instances can either be registered 
    ///     on a per migrations configuration basis using <see cref="DbMigrationsConfiguration.HistoryContextFactory"/>,
    ///     or globally using <see cref="System.Data.Entity.Config.DbConfiguration"/>.
    /// </summary>
    public interface IHistoryContextFactory
    {
        /// <summary>
        ///     Creates a new instance of <see cref="HistoryContext"/>.
        /// </summary>
        /// <param name="existingConnection"> 
        ///     An existing connection to use for the new context. 
        /// </param>
        /// <param name="contextOwnsConnection">
        ///     If set to <c>true</c> the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.
        /// </param>
        /// <param name="defaultSchema">
        ///     The default schema of the model being migrated.
        /// </param>
        /// <returns>
        ///     The newly created context.    
        /// </returns>
        HistoryContext Create(DbConnection existingConnection, bool contextOwnsConnection, string defaultSchema);
    }
}
