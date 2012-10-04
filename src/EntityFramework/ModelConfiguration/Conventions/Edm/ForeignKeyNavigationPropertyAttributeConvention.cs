// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to process instances of <see cref="ForeignKeyAttribute" /> found on navigation properties in the model.
    /// </summary>
    public class ForeignKeyNavigationPropertyAttributeConvention : IEdmConvention<NavigationProperty>
    {
        public void Apply(NavigationProperty edmDataModelItem, EdmModel model)
        {
            var associationType = edmDataModelItem.Association;

            if (associationType.Constraint != null)
            {
                return;
            }

            var foreignKeyAttribute
                = edmDataModelItem.GetClrAttributes<ForeignKeyAttribute>().SingleOrDefault();

            if (foreignKeyAttribute == null)
            {
                return;
            }

            AssociationEndMember principalEnd, dependentEnd;
            if (associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd)
                || associationType.IsPrincipalConfigured())
            {
                dependentEnd = dependentEnd ?? associationType.TargetEnd;
                principalEnd = principalEnd ?? associationType.SourceEnd;

                var dependentPropertyNames
                    = foreignKeyAttribute.Name
                        .Split(',')
                        .Select(p => p.Trim());

                var declaringEntityType
                    = model.GetEntityTypes()
                        .Single(e => e.DeclaredNavigationProperties.Contains(edmDataModelItem));

                var dependentProperties
                    = GetDependentProperties(
                        dependentEnd.GetEntityType(),
                        dependentPropertyNames,
                        declaringEntityType,
                        edmDataModelItem).ToList();

                var constraint
                    = new ReferentialConstraint(
                        principalEnd,
                        dependentEnd,
                        principalEnd.GetEntityType().KeyProperties().ToList(),
                        dependentProperties);

                var dependentKeyProperties = dependentEnd.GetEntityType().KeyProperties();

                if (dependentKeyProperties.Count() == constraint.ToProperties.Count()
                    && dependentKeyProperties.All(kp => constraint.ToProperties.Contains(kp)))
                {
                    principalEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;

                    if (dependentEnd.RelationshipMultiplicity.IsMany())
                    {
                        dependentEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
                    }
                }

                if (principalEnd.IsRequired())
                {
                    constraint.ToProperties.Each(p => p.Nullable = false);
                }

                associationType.Constraint = constraint;
            }
        }

        private static IEnumerable<EdmProperty> GetDependentProperties(
            EntityType dependentType,
            IEnumerable<string> dependentPropertyNames,
            EntityType declaringEntityType,
            NavigationProperty navigationProperty)
        {
            foreach (var dependentPropertyName in dependentPropertyNames)
            {
                if (string.IsNullOrWhiteSpace(dependentPropertyName))
                {
                    throw Error.ForeignKeyAttributeConvention_EmptyKey(
                        navigationProperty.Name, declaringEntityType.GetClrType());
                }

                var dependentProperty
                    = dependentType.Properties
                        .SingleOrDefault(p => p.Name.Equals(dependentPropertyName, StringComparison.Ordinal));

                if (dependentProperty == null)
                {
                    throw Error.ForeignKeyAttributeConvention_InvalidKey(
                        navigationProperty.Name,
                        declaringEntityType.GetClrType(),
                        dependentPropertyName,
                        dependentType.GetClrType());
                }

                yield return dependentProperty;
            }
        }
    }
}
