// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref = "RequiredAttribute" /> found on navigation properties in the model.
    /// </summary>
    public sealed class RequiredNavigationPropertyAttributeConvention
        : IConfigurationConvention<PropertyInfo, NavigationPropertyConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, NavigationPropertyConfiguration> _impl
            = new RequiredNavigationPropertyAttributeConventionImpl();

        internal RequiredNavigationPropertyAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, NavigationPropertyConfiguration>.Apply(
            PropertyInfo memberInfo, Func<NavigationPropertyConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class RequiredNavigationPropertyAttributeConventionImpl
            : AttributeConfigurationConvention<PropertyInfo, NavigationPropertyConfiguration, RequiredAttribute>
        {
            internal override void Apply(
                PropertyInfo propertyInfo, NavigationPropertyConfiguration navigationPropertyConfiguration,
                RequiredAttribute _)
            {
                if ((navigationPropertyConfiguration.EndKind == null)
                    && !propertyInfo.PropertyType.IsCollection())
                {
                    navigationPropertyConfiguration.EndKind = EdmAssociationEndKind.Required;
                }
            }
        }
    }
}
