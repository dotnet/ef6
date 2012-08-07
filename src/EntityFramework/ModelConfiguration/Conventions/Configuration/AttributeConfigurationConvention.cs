// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Base class for conventions that process CLR attributes found in the model.
    /// </summary>
    /// <typeparam name="TMemberInfo"> The type of member to look for. </typeparam>
    /// <typeparam name="TConfiguration"> The type of the configuration to look for. </typeparam>
    /// <typeparam name="TAttribute"> The type of the attribute to look for. </typeparam>
    [ContractClass(typeof(AttributeConfigurationConventionContracts<,,>))]
    internal abstract class AttributeConfigurationConvention<TMemberInfo, TConfiguration, TAttribute>
        : IConfigurationConvention<TMemberInfo, TConfiguration>
        where TMemberInfo : MemberInfo
        where TConfiguration : ConfigurationBase
        where TAttribute : Attribute
    {
        private readonly AttributeProvider _attributeProvider;

        protected AttributeConfigurationConvention()
            : this(new AttributeProvider())
        {
        }

        private AttributeConfigurationConvention(AttributeProvider attributeProvider)
        {
            Contract.Requires(attributeProvider != null);

            _attributeProvider = attributeProvider;
        }

        internal abstract void Apply(TMemberInfo memberInfo, TConfiguration configuration, TAttribute attribute);

        void IConfigurationConvention<TMemberInfo, TConfiguration>.Apply(
            TMemberInfo memberInfo, Func<TConfiguration> configuration)
        {
            foreach (var attribute in _attributeProvider.GetAttributes(memberInfo).OfType<TAttribute>())
            {
                Apply(memberInfo, configuration(), attribute);
            }
        }
    }

    #region Base Member Contracts

    [ContractClassFor(typeof(AttributeConfigurationConvention<,,>))]
    internal abstract class AttributeConfigurationConventionContracts<TMemberInfo, TConfiguration, TAttribute>
        : AttributeConfigurationConvention<TMemberInfo, TConfiguration, TAttribute>
        where TMemberInfo : MemberInfo
        where TConfiguration : ConfigurationBase
        where TAttribute : Attribute
    {
        internal override void Apply(TMemberInfo memberInfo, TConfiguration configuration, TAttribute attribute)
        {
            Contract.Requires(memberInfo != null);
            Contract.Requires(configuration != null);
            Contract.Requires(attribute != null);
        }
    }

    #endregion
}
