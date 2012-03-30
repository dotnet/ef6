namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IEdmConventionContracts))]
    internal interface IEdmConvention : IConvention
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
