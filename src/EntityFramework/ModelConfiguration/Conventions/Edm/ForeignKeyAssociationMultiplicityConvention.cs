// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Convention to distinguish between optional and required relationships based on CLR nullability of the foreign key property.
    /// </summary>
    public class ForeignKeyAssociationMultiplicityConvention : IEdmConvention<AssociationType>
    {
        public void Apply(AssociationType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            var constraint = edmDataModelItem.Constraint;

            if (constraint == null)
            {
                return;
            }

            var navigationPropertyConfiguration
                = edmDataModelItem.Annotations.GetConfiguration() as NavigationPropertyConfiguration;

            if (constraint.ToProperties.All(p => !p.Nullable))
            {
                var principalEnd = edmDataModelItem.GetOtherEnd(constraint.DependentEnd);

                // find the navigation property with this end
                var navigationProperty
                    = model.EntityTypes
                           .SelectMany(et => et.DeclaredNavigationProperties)
                           .SingleOrDefault(np => np.ResultEnd == principalEnd);

                PropertyInfo propertyInfo;

                if (navigationPropertyConfiguration != null
                    && navigationProperty != null
                    && ((propertyInfo = navigationProperty.Annotations.GetClrPropertyInfo()) != null)
                    && ((propertyInfo == navigationPropertyConfiguration.NavigationProperty
                         && navigationPropertyConfiguration.RelationshipMultiplicity.HasValue)
                        || (propertyInfo == navigationPropertyConfiguration.InverseNavigationProperty
                            && navigationPropertyConfiguration.InverseEndKind.HasValue)))
                {
                    return;
                }

                principalEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            }
        }
    }
}
