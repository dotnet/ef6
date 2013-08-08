// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Convention to configure a type as a complex type if it has no primary key, no mapped base type and no navigation properties.
    /// </summary>
    public class ComplexTypeDiscoveryConvention : IConceptualModelConvention<EdmModel>
    {
        /// <inheritdoc />
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public virtual void Apply(EdmModel item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            // Query the model for candidate complex types.
            //   - The rules for complex type discovery are as follows:
            //      1) The entity does not have a key or base type.
            //      2) The entity does not have explicit configuration or has only structural type configuration.
            //      3) The entity does not have any outbound navigation properties.
            //         The entity only has inbound associations where:
            //          4) The association does not have a constraint defined.
            //          5) The association does not have explicit configuration.
            //          6) The association is not self-referencing.
            //          7) The other end of the association is Optional.
            //      8) Any inbound navigation properties do not have explicit configuration.

            var candidates
                = from entityType in item.EntityTypes
                  where entityType.KeyProperties.Count == 0 // (1)
                        && entityType.BaseType == null
                  // (1)
                  let entityTypeConfiguration = entityType.GetConfiguration() as EntityTypeConfiguration
                  where ((entityTypeConfiguration == null) // (2)
                         || (!entityTypeConfiguration.IsExplicitEntity
                             && entityTypeConfiguration.IsStructuralConfigurationOnly)) // (2)
                        && !entityType.NavigationProperties.Any()
                  // (3)
                  let matchingAssociations
                      = from associationType in item.AssociationTypes
                        where associationType.SourceEnd.GetEntityType() == entityType ||
                              associationType.TargetEnd.GetEntityType() == entityType
                        let declaringEnd
                            = associationType.SourceEnd.GetEntityType() == entityType
                                  ? associationType.SourceEnd
                                  : associationType.TargetEnd
                        let declaringEntity
                            = associationType.GetOtherEnd(declaringEnd).GetEntityType()
                        let navigationProperties
                            = declaringEntity.NavigationProperties
                                             .Where(n => n.ResultEnd.GetEntityType() == entityType)
                        select new
                            {
                                DeclaringEnd = declaringEnd,
                                AssociationType = associationType,
                                DeclaringEntityType = declaringEntity,
                                NavigationProperties = navigationProperties.ToList()
                            }
                  where matchingAssociations.All(
                      a => a.AssociationType.Constraint == null // (4)
                           && a.AssociationType.GetConfiguration() == null // (5)
                           && !a.AssociationType.IsSelfReferencing() // (6)
                           && a.DeclaringEnd.IsOptional() // (7)
                           && a.NavigationProperties.All(n => n.GetConfiguration() == null))
                  // (8)
                  select new
                      {
                          EntityType = entityType,
                          MatchingAssociations = matchingAssociations.ToList(),
                      };

            // Transform candidate entities into complex types
            foreach (var candidate in candidates.ToList())
            {
                var complexType = item.AddComplexType(candidate.EntityType.Name, candidate.EntityType.NamespaceName);

                foreach (var property in candidate.EntityType.DeclaredProperties)
                {
                    complexType.AddMember(property);
                }

                foreach (var annotation in candidate.EntityType.Annotations)
                {
                    complexType.Annotations.Add(annotation);
                }

                foreach (var association in candidate.MatchingAssociations)
                {
                    foreach (var navigationProperty in association.NavigationProperties)
                    {
                        if (association.DeclaringEntityType.NavigationProperties.Contains(navigationProperty))
                        {
                            association.DeclaringEntityType.RemoveMember(navigationProperty);

                            var complexProperty
                                = association.DeclaringEntityType
                                             .AddComplexProperty(navigationProperty.Name, complexType);

                            foreach (var annotation in navigationProperty.Annotations)
                            {
                                complexProperty.Annotations.Add(annotation);
                            }
                        }
                    }

                    item.RemoveAssociationType(association.AssociationType);
                }

                item.RemoveEntityType(candidate.EntityType);
            }
        }
    }
}
