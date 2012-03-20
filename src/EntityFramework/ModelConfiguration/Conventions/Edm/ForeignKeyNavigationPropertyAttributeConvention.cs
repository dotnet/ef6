namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Convention to process instances of <see cref = "ForeignKeyAttribute" /> found on navigation properties in the model.
    /// </summary>
    public sealed class ForeignKeyNavigationPropertyAttributeConvention : IEdmConvention<EdmNavigationProperty>
    {
        internal ForeignKeyNavigationPropertyAttributeConvention()
        {
        }

        void IEdmConvention<EdmNavigationProperty>.Apply(EdmNavigationProperty navigationProperty, EdmModel model)
        {
            var associationType = navigationProperty.Association;

            if (associationType.Constraint != null)
            {
                return;
            }

            var foreignKeyAttribute
                = navigationProperty.GetClrAttributes<ForeignKeyAttribute>().SingleOrDefault();

            if (foreignKeyAttribute == null)
            {
                return;
            }

            EdmAssociationEnd principalEnd, dependentEnd;
            if (associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd)
                || associationType.IsPrincipalConfigured())
            {
                dependentEnd = dependentEnd ?? associationType.TargetEnd;

                var dependentPropertyNames
                    = foreignKeyAttribute.Name
                        .Split(',')
                        .Select(p => p.Trim());

                var declaringEntityType
                    = model.GetEntityTypes()
                        .Where(e => e.DeclaredNavigationProperties.Contains(navigationProperty))
                        .Single();

                var constraint = new EdmAssociationConstraint
                    {
                        DependentEnd = dependentEnd,
                        DependentProperties
                            = GetDependentProperties(
                                dependentEnd.EntityType,
                                dependentPropertyNames,
                                declaringEntityType,
                                navigationProperty).ToList()
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