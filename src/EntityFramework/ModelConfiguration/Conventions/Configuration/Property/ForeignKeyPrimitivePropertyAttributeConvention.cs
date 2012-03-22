namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref = "ForeignKeyAttribute" /> found on foreign key properties in the model.
    /// </summary>
    public sealed class ForeignKeyPrimitivePropertyAttributeConvention :
        IConfigurationConvention<PropertyInfo, ModelConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, ModelConfiguration> _impl =
            new ForeignKeyAttributeConventionImpl();

        internal ForeignKeyPrimitivePropertyAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, ModelConfiguration>.Apply(
            PropertyInfo memberInfo, Func<ModelConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class ForeignKeyAttributeConventionImpl :
            AttributeConfigurationConvention<PropertyInfo, ModelConfiguration, ForeignKeyAttribute>
        {
            internal override void Apply(
                PropertyInfo propertyInfo, ModelConfiguration modelConfiguration,
                ForeignKeyAttribute foreignKeyAttribute)
            {
                if (propertyInfo.IsValidEdmScalarProperty())
                {
                    ApplyNavigationProperty(propertyInfo, modelConfiguration, foreignKeyAttribute);
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
}
