// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Base class for conventions that process CLR attributes found in the model.
    /// </summary>
    /// <typeparam name="TMemberInfo"> The type of member to look for. </typeparam>
    /// <typeparam name="TConfiguration"> The type of the configuration to look for. </typeparam>
    /// <typeparam name="TAttribute"> The type of the attribute to look for. </typeparam>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public abstract class AttributeConfigurationConvention<TMemberInfo, TConfiguration, TAttribute>
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
            DebugCheck.NotNull(attributeProvider);

            _attributeProvider = attributeProvider;
        }

        public abstract void Apply(TMemberInfo memberInfo, TConfiguration configuration, TAttribute attribute);

        void IConfigurationConvention<TMemberInfo, TConfiguration>.Apply(
            TMemberInfo memberInfo, Func<TConfiguration> configuration)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");

            foreach (var attribute in _attributeProvider.GetAttributes(memberInfo).OfType<TAttribute>())
            {
                Apply(memberInfo, configuration(), attribute);
            }
        }
    }
}
