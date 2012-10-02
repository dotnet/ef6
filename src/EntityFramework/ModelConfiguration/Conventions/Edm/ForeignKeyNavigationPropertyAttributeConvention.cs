// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to process instances of <see cref="ForeignKeyAttribute" /> found on navigation properties in the model.
    /// </summary>
    public class ForeignKeyNavigationPropertyAttributeConvention : IEdmConvention<EdmNavigationProperty>
    {
        public void Apply(EdmNavigationProperty edmDataModelItem, EdmModel model)
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

            EdmAssociationEnd principalEnd, dependentEnd;
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

                var constraint = new EdmAssociationConstraint
                                     {
                                         DependentEnd = dependentEnd,
                                         DependentProperties
                                             = GetDependentProperties(
                                                 dependentEnd.EntityType,
                                                 dependentPropertyNames,
                                                 declaringEntityType,
                                                 edmDataModelItem).ToList()
                                     };

                var dependentKeyProperties = dependentEnd.EntityType.KeyProperties();

                if (dependentKeyProperties.Count() == constraint.DependentProperties.Count()
                    && dependentKeyProperties.All(kp => constraint.DependentProperties.Contains(kp)))
                {
                    principalEnd.EndKind = EdmAssociationEndKind.Required;

                    if (dependentEnd.EndKind.IsMany())
                    {
                        dependentEnd.EndKind = EdmAssociationEndKind.Optional;
                    }
                }

                if (principalEnd.IsRequired())
                {
                    constraint.DependentProperties.Each(p => p.PropertyType.IsNullable = false);
                }

                associationType.Constraint = constraint;
            }
        }

        private static IEnumerable<EdmProperty> GetDependentProperties(
            EdmEntityType dependentType,
            IEnumerable<string> dependentPropertyNames,
            EdmEntityType declaringEntityType,
            EdmNavigationProperty navigationProperty)
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
