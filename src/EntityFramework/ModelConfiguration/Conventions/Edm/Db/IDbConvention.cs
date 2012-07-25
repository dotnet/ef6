// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IDbConventionContracts))]
    internal interface IDbConvention : IConvention
    {
        void Apply(DbDatabaseMetadata database);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbConvention))]
    internal abstract class IDbConventionContracts : IDbConvention
    {
        void IDbConvention.Apply(DbDatabaseMetadata database)
        {
            Contract.Requires(database != null);
        }
    }

    #endregion
}
