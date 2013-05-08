// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Config;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Base class for conventions that process CLR attributes found in the model.
    /// </summary>
    /// <typeparam name="TMemberInfo"> The type of member to look for. </typeparam>
    /// <typeparam name="TAttribute"> The type of the attribute to look for. </typeparam>
    public abstract class AttributeConfigurationConvention<TMemberInfo, TAttribute>
        : IConfigurationConvention<TMemberInfo>
        where TMemberInfo : MemberInfo
        where TAttribute : Attribute
    {
        private readonly AttributeProvider _attributeProvider;

        protected AttributeConfigurationConvention()
            : this(DbConfiguration.GetService<AttributeProvider>())
        {
        }

        private AttributeConfigurationConvention(AttributeProvider attributeProvider)
        {
            DebugCheck.NotNull(attributeProvider);

            _attributeProvider = attributeProvider;
        }

        public abstract void Apply(
            TMemberInfo memberInfo, ModelConfiguration modelConfiguration, TAttribute attribute);

        public void Apply(TMemberInfo memberInfo, ModelConfiguration modelConfiguration)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(modelConfiguration, "configuration");

            foreach (var attribute in _attributeProvider.GetAttributes(memberInfo).OfType<TAttribute>())
            {
                Apply(memberInfo,  modelConfiguration, attribute);
            }
        }
    }
}
