// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
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

            var navPropConfig = edmDataModelItem.Annotations.GetConfiguration() as NavigationPropertyConfiguration;

            if (constraint.ToProperties.All(p => !p.Nullable))
            {
                var principalEnd = edmDataModelItem.GetOtherEnd(constraint.DependentEnd);

                // find the navigation property with this end
                var navProp = model.Namespaces.SelectMany(ns => ns.EntityTypes)
                                   .SelectMany(et => et.DeclaredNavigationProperties)
                                   .SingleOrDefault(np => np.ResultEnd == principalEnd);

                PropertyInfo navPropInfo;
                if (navPropConfig != null
                    &&
                    navProp != null
                    &&
                    ((navPropInfo = navProp.Annotations.GetClrPropertyInfo()) != null)
                    && ((navPropInfo == navPropConfig.NavigationProperty && navPropConfig.RelationshipMultiplicity.HasValue) ||
                        (navPropInfo == navPropConfig.InverseNavigationProperty && navPropConfig.InverseEndKind.HasValue)))
                {
                    return;
                }

                principalEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            }
        }
    }
}
