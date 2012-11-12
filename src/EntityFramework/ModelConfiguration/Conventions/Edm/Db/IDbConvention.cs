// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IDbConventionContracts))]
    public interface IDbConvention : IConvention
    {
        void Apply(EdmModel model);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbConvention))]
    internal abstract class IDbConventionContracts : IDbConvention
    {
        void IDbConvention.Apply(EdmModel model)
        {
            Contract.Requires(model != null);
        }
    }

    #endregion
}
