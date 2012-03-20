namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
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