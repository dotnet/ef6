// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Common.CommandTrees;

    /// <summary>
    ///     An object that implements this interface can be registered with <see cref="Interception" /> to
    ///     receive notifications when Entity Framework creates <see cref="DbCommandTree" /> command trees.
    /// </summary>
    public interface IDbCommandTreeInterceptor : IDbInterceptor
    {
        /// <summary>
        ///     This method is called after a new <see cref="DbCommandTree" /> has been created.
        ///     This method should return the given tree. However, the tree used by Entity Framework
        ///     can be changed by returning a different value.
        /// </summary>
        /// <remarks>
        ///     Command trees are created for both queries and insert/update/delete commands. However, query
        ///     command trees are cached by model which means that command tree creation only happens the
        ///     first time a query is executed and this notification will only happen at that time
        /// </remarks>
        /// <param name="command">The tree that has been created.</param>
        /// <param name="interceptionContext">Contextual information associated with the call.</param>
        DbCommandTree TreeCreated(DbCommandTree commandTree, DbInterceptionContext interceptionContext);
    }
}
