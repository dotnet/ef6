// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="RequiredAttribute" /> found on navigation properties in the model.
    /// </summary>
    public class RequiredNavigationPropertyAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, NavigationPropertyConfiguration, RequiredAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo, NavigationPropertyConfiguration configuration,
            RequiredAttribute attribute)
        {
            if ((configuration.EndKind == null)
                && !memberInfo.PropertyType.IsCollection())
            {
                configuration.EndKind = EdmAssociationEndKind.Required;
            }
        }
    }
}
