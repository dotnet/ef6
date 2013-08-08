// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Convention to process instances of <see cref="InversePropertyAttribute" /> found on properties in the model.
    /// </summary>
    public class InversePropertyAttributeConvention : PropertyAttributeConfigurationConvention<InversePropertyAttribute>
    {
        /// <inheritdoc/>
        public override void Apply(
            PropertyInfo memberInfo, ConventionTypeConfiguration configuration, InversePropertyAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            if (!memberInfo.IsValidEdmNavigationProperty())
            {
                return;
            }

            var inverseType = memberInfo.PropertyType.GetTargetType();
            var inverseNavigationProperty
                = new PropertyFilter()
                    .GetProperties(inverseType, false)
                    .SingleOrDefault(
                        p =>
                        string.Equals(p.Name, attribute.Property, StringComparison.OrdinalIgnoreCase));

            if (inverseNavigationProperty == null)
            {
                throw Error.InversePropertyAttributeConvention_PropertyNotFound(
                    attribute.Property,
                    inverseType,
                    memberInfo.Name,
                    memberInfo.ReflectedType);
            }

            if (memberInfo == inverseNavigationProperty)
            {
                throw Error.InversePropertyAttributeConvention_SelfInverseDetected(
                    memberInfo.Name, memberInfo.ReflectedType);
            }

            configuration.NavigationProperty(memberInfo).HasInverseNavigationProperty(p => inverseNavigationProperty);
        }
    }
}
