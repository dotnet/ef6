// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="ForeignKeyAttribute" /> found on foreign key properties in the model.
    /// </summary>
    public class ForeignKeyPrimitivePropertyAttributeConvention :
        AttributeConfigurationConvention<PropertyInfo, ModelConfiguration, ForeignKeyAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo, ModelConfiguration configuration,
            ForeignKeyAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            if (memberInfo.IsValidEdmScalarProperty())
            {
                ApplyNavigationProperty(memberInfo, configuration, attribute);
            }
        }

        private static void ApplyNavigationProperty(
            PropertyInfo propertyInfo, ModelConfiguration modelConfiguration,
            ForeignKeyAttribute foreignKeyAttribute)
        {
            var navigationPropertyInfo
                = (from pi in new PropertyFilter().GetProperties(propertyInfo.ReflectedType, false)
                   where pi.Name.Equals(foreignKeyAttribute.Name, StringComparison.Ordinal)
                   select pi).SingleOrDefault();

            if (navigationPropertyInfo == null)
            {
                throw Error.ForeignKeyAttributeConvention_InvalidNavigationProperty(
                    propertyInfo.Name, propertyInfo.ReflectedType, foreignKeyAttribute.Name);
            }

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(propertyInfo.ReflectedType).Navigation(navigationPropertyInfo);

            if (HasConfiguredConstraint(propertyInfo, modelConfiguration, navigationPropertyConfiguration))
            {
                return;
            }

            var foreignKeyConstraintConfiguration
                = (ForeignKeyConstraintConfiguration)
                  (navigationPropertyConfiguration.Constraint
                   ?? (navigationPropertyConfiguration.Constraint = new ForeignKeyConstraintConfiguration()));

            foreignKeyConstraintConfiguration.AddColumn(propertyInfo);
        }

        private static bool HasConfiguredConstraint(
            PropertyInfo propertyInfo,
            ModelConfiguration modelConfiguration,
            NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
            return ((navigationPropertyConfiguration.Constraint != null)
                    && navigationPropertyConfiguration.Constraint.IsFullySpecified)
                   || ((navigationPropertyConfiguration.InverseNavigationProperty != null)
                       && (modelConfiguration
                              .Entity(propertyInfo.PropertyType.GetTargetType())
                              .Navigation(navigationPropertyConfiguration.InverseNavigationProperty)).Constraint
                       != null);
        }
    }
}
