namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    [ContractClass(typeof(IConfigurationConventionContracts<,>))]
    internal interface IConfigurationConvention<TMemberInfo, TConfiguration> : IConvention
        where TMemberInfo : MemberInfo
        where TConfiguration : ConfigurationBase
    {
        void Apply(TMemberInfo memberInfo, Func<TConfiguration> configuration);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IConfigurationConvention<,>))]
    internal abstract class IConfigurationConventionContracts<TMemberInfo, TConfiguration>
        : IConfigurationConvention<TMemberInfo, TConfiguration>
        where TMemberInfo : MemberInfo
        where TConfiguration : ConfigurationBase
    {
        void IConfigurationConvention<TMemberInfo, TConfiguration>.Apply(
            TMemberInfo memberInfo, Func<TConfiguration> configuration)
        {
            Contract.Requires(memberInfo != null);
            Contract.Requires(configuration != null);
        }
    }

    #endregion
}