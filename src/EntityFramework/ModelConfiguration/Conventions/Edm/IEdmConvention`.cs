// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IEdmConventionContracts<>))]
    internal interface IEdmConvention<TEdmDataModelItem> : IConvention
        where TEdmDataModelItem : EdmDataModelItem
    {
        void Apply(TEdmDataModelItem edmDataModelItem, EdmModel model);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IEdmConvention<>))]
    internal abstract class IEdmConventionContracts<TEdmDataModelItem> : IEdmConvention<TEdmDataModelItem>
        where TEdmDataModelItem : EdmDataModelItem
    {
        void IEdmConvention<TEdmDataModelItem>.Apply(TEdmDataModelItem dataModelItem, EdmModel model)
        {
            Contract.Requires(dataModelItem != null);
            Contract.Requires(model != null);
        }
    }

    #endregion
}
