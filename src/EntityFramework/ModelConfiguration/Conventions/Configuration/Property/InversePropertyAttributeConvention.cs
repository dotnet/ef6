// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref = "InversePropertyAttribute" /> found on properties in the model.
    /// </summary>
    public sealed class InversePropertyAttributeConvention : IConfigurationConvention<PropertyInfo, ModelConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, ModelConfiguration> _impl
            = new InversePropertyAttributeConventionImpl();

        internal InversePropertyAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, ModelConfiguration>.Apply(
            PropertyInfo memberInfo, Func<ModelConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class InversePropertyAttributeConventionImpl :
            AttributeConfigurationConvention<PropertyInfo, ModelConfiguration, InversePropertyAttribute>
        {
            internal override void Apply(
                PropertyInfo propertyInfo, ModelConfiguration modelConfiguration,
                InversePropertyAttribute inversePropertyAttribute)
            {
                var navigationPropertyConfiguration
                    = modelConfiguration
                        .Entity(propertyInfo.ReflectedType)
                        .Navigation(propertyInfo);

                if (navigationPropertyConfiguration.InverseNavigationProperty != null)
                {
                    return;
                }

                var inverseType = propertyInfo.PropertyType.GetTargetType();
                var inverseNavigationProperty
                    = new PropertyFilter()
                        .GetProperties(inverseType, false)
                        .Where(
                            p =>
                            string.Equals(p.Name, inversePropertyAttribute.Property, StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();

                if (inverseNavigationProperty == null)
                {
                    throw Error.InversePropertyAttributeConvention_PropertyNotFound(
                        inversePropertyAttribute.Property,
                        inverseType,
                        propertyInfo.Name,
                        propertyInfo.ReflectedType);
                }

                if (propertyInfo == inverseNavigationProperty)
                {
                    throw Error.InversePropertyAttributeConvention_SelfInverseDetected(
                        propertyInfo.Name, propertyInfo.ReflectedType);
                }

                navigationPropertyConfiguration.InverseNavigationProperty = inverseNavigationProperty;
            }
        }
    }
}
