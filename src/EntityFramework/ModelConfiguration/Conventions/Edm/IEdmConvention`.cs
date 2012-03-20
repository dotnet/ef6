namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [ContractClass(typeof(IEdmConventionContracts<>))]
    internal interface IEdmConvention<TEdmDataModelItem> : IConvention
        where TEdmDataModelItem : EdmDataModelItem
    {
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
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