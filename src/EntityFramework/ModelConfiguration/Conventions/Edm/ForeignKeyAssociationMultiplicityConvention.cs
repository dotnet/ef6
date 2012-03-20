namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Convention to distinguish between optional and required relationships based on CLR nullability of the foreign key property.
    /// </summary>
    public sealed class ForeignKeyAssociationMultiplicityConvention : IEdmConvention<EdmAssociationType>
    {
        internal ForeignKeyAssociationMultiplicityConvention()
        {
        }

        void IEdmConvention<EdmAssociationType>.Apply(EdmAssociationType associationType, EdmModel model)
        {
            var constraint = associationType.Constraint;

            if (constraint == null)
            {
                return;
            }

            var navPropConfig = associationType.Annotations.GetConfiguration() as NavigationPropertyConfiguration;

            if (constraint.DependentProperties
                .All(
                    p => (p.PropertyType.IsNullable != null)
                         && !p.PropertyType.IsNullable.Value))
            {
                var principalEnd = associationType.GetOtherEnd(constraint.DependentEnd);

                // find the navigation property with this end
                var navProp = model.Namespaces.SelectMany(ns => ns.EntityTypes)
                    .SelectMany(et => et.DeclaredNavigationProperties)
                    .Where(np => np.ResultEnd == principalEnd)
                    .SingleOrDefault();

                PropertyInfo navPropInfo;
                if (navPropConfig != null &&
                    navProp != null &&
                    ((navPropInfo = navProp.Annotations.GetClrPropertyInfo()) != null) &&
                    ((navPropInfo == navPropConfig.NavigationProperty && navPropConfig.EndKind.HasValue) ||
                     (navPropInfo == navPropConfig.InverseNavigationProperty && navPropConfig.InverseEndKind.HasValue)))
                {
                    return;
                }

                principalEnd.EndKind = EdmAssociationEndKind.Required;
            }
        }
    }
}