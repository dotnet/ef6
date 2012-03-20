namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IConfigurationConventionContracts))]
    internal interface IConfigurationConvention : IConvention
    {
        void Apply(ModelConfiguration modelConfiguration);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IConfigurationConvention))]
    internal abstract class IConfigurationConventionContracts : IConfigurationConvention
    {
        void IConfigurationConvention.Apply(ModelConfiguration modelConfiguration)
        {
            Contract.Requires(modelConfiguration != null);
        }
    }

    #endregion
}