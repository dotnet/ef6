// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Convention to distinguish between optional and required relationships based on CLR nullability of the foreign key property.
    /// </summary>
    public class ForeignKeyAssociationMultiplicityConvention : IConceptualModelConvention<AssociationType>
    {
        /// <inheritdoc />
        public virtual void Apply(AssociationType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            var constraint = item.Constraint;

            if (constraint == null)
            {
                return;
            }

            var navigationPropertyConfiguration
                = item.Annotations.GetConfiguration() as NavigationPropertyConfiguration;

            if (constraint.ToProperties.All(p => !p.Nullable))
            {
                var principalEnd = item.GetOtherEnd(constraint.DependentEnd);

                // find the navigation property with this end
                var navigationProperty
                    = model.GetConceptualModel().EntityTypes
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
