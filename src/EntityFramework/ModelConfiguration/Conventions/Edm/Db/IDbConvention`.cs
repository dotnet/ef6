// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IDbConventionContracts<>))]
    public interface IDbConvention<TMetadataItem> : IConvention
        where TMetadataItem : MetadataItem
    {
        void Apply(TMetadataItem dbDataModelItem, EdmModel model);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbConvention<>))]
    internal abstract class IDbConventionContracts<TMetadataItem> : IDbConvention<TMetadataItem>
        where TMetadataItem : MetadataItem
    {
        void IDbConvention<TMetadataItem>.Apply(TMetadataItem dbDataModelItem, EdmModel model)
        {
            Contract.Requires(dbDataModelItem != null);
            Contract.Requires(model != null);
        }
    }

    #endregion
}
