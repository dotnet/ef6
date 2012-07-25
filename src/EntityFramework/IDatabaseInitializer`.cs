// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity
{
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     An implementation of this interface is used to initialize the underlying database when
    ///     an instance of a <see cref = "DbContext" /> derived class is used for the first time.
    ///     This initialization can conditionally create the database and/or seed it with data.
    ///     The strategy used is set using the static InitializationStrategy property of the
    ///     <see cref = "Database" /> class.
    ///     The following implementations are provided: <see cref = "DropCreateDatabaseIfModelChanges{TContext}"/>,
    ///     <see cref = "DropCreateDatabaseAlways{TContext}"/>, <see cref = "CreateDatabaseIfNotExists{TContext}"/>.
    /// </summary>
    [ContractClass(typeof(IDatabaseInitializerContracts<>))]
    public interface IDatabaseInitializer<in TContext>
        where TContext : DbContext
    {
        /// <summary>
        ///     Executes the strategy to initialize the database for the given context.
        /// </summary>
        /// <param name = "context">The context.</param>
        void InitializeDatabase(TContext context);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDatabaseInitializer<>))]
    internal abstract class IDatabaseInitializerContracts<TContext> : IDatabaseInitializer<TContext>
        where TContext : DbContext
    {
        public void InitializeDatabase(TContext context)
        {
            Contract.Requires(context != null);

            throw new NotImplementedException();
        }
    }

    #endregion
}
