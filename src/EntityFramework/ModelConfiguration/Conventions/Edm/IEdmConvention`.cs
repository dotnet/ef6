// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IEdmConventionContracts<>))]
    public interface IEdmConvention<TEdmDataModelItem> : IConvention
        where TEdmDataModelItem : MetadataItem
    {
        void Apply(TEdmDataModelItem edmDataModelItem, EdmModel model);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IEdmConvention<>))]
    internal abstract class IEdmConventionContracts<TEdmDataModelItem> : IEdmConvention<TEdmDataModelItem>
        where TEdmDataModelItem : MetadataItem
    {
        void IEdmConvention<TEdmDataModelItem>.Apply(TEdmDataModelItem edmDataModelItem, EdmModel model)
        {
            Contract.Requires(edmDataModelItem != null);
            Contract.Requires(model != null);
        }
    }

    #endregion
}
