// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IEdmConventionContracts))]
    public interface IEdmConvention : IConvention
    {
        void Apply(EdmModel model);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IEdmConvention))]
    internal abstract class IEdmConventionContracts : IEdmConvention
    {
        void IEdmConvention.Apply(EdmModel model)
        {
            Contract.Requires(model != null);
        }
    }

    #endregion
}
