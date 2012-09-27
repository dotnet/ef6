// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IHistoryContextFactoryContracts))]
    public interface IHistoryContextFactory
    {
        HistoryContext Create(DbConnection existingConnection, bool contextOwnsConnection, string defaultSchema);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IHistoryContextFactory))]
    internal abstract class IHistoryContextFactoryContracts : IHistoryContextFactory
    {
        public HistoryContext Create(DbConnection existingConnection, bool contextOwnsConnection, string defaultSchema)
        {
            Contract.Requires(existingConnection != null);
            Contract.Ensures(Contract.Result<HistoryContext>() != null);

            return null;
        }
    }

    #endregion
}
