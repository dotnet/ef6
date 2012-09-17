// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IDbMappingConventionContracts))]
    public interface IDbMappingConvention : IConvention
    {
        void Apply(DbDatabaseMapping databaseMapping);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbMappingConvention))]
    internal abstract class IDbMappingConventionContracts : IDbMappingConvention
    {
        void IDbMappingConvention.Apply(DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);
        }
    }

    #endregion
}
