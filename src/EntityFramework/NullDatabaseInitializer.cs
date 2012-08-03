// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Config;

    /// <summary>
    /// An implementation of <see cref="IDatabaseInitializer{TContext}"/> that does nothing. Using this
    /// initializer disables database initialization for the given context type. Passing an instance
    /// of this class to <see cref="Database.SetInitializer{TContext}"/> is equivalent to passing null.
    /// When <see cref="IDbDependencyResolver"/> is being used to resolve initializers an instance of
    /// this class must be used to disable initialization.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class NullDatabaseInitializer<TContext> : IDatabaseInitializer<TContext>
        where TContext : DbContext
    {
        /// <inheritdoc/>
        public void InitializeDatabase(TContext context)
        {
        }
    }
}
