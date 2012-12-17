// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Convention to detect navigation properties to be inverses of each other when only one pair
    ///     of navigation properties exists between the related types.
    /// </summary>
    public class AssociationInverseDiscoveryConvention : IEdmConvention
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Apply(EdmModel model)
        {
            Check.NotNull(model, "model");

            var associationPairs
                = (from a1 in model.AssociationTypes
                   from a2 in model.AssociationTypes
                   where a1 != a2
                   where a1.SourceEnd.GetEntityType() == a2.TargetEnd.GetEntityType()
                         && a1.TargetEnd.GetEntityType() == a2.SourceEnd.GetEntityType()
                   let a1Configuration = a1.GetConfiguration() as NavigationPropertyConfiguration
                   let a2Configuration = a2.GetConfiguration() as NavigationPropertyConfiguration
                   where (((a1Configuration == null)
                           || ((a1Configuration.InverseEndKind == null)
                               && (a1Configuration.InverseNavigationProperty == null)))
                          && ((a2Configuration == null)
                              || ((a2Configuration.InverseEndKind == null)
                                  && (a2Configuration.InverseNavigationProperty == null))))
                   select new
                              {
                                  a1,
                                  a2
                              })
                    .Distinct((a, b) => a.a1 == b.a2 && a.a2 == b.a1)
                    .GroupBy(
                        (a, b) => a.a1.SourceEnd.GetEntityType() == b.a2.TargetEnd.GetEntityType()
                                  && a.a1.TargetEnd.GetEntityType() == b.a2.SourceEnd.GetEntityType())
                    .Where(g => g.Count() == 1)
                    .Select(g => g.Single());

            foreach (var pair in associationPairs)
            {
                var unifiedAssociation = pair.a2.GetConfiguration() != null ? pair.a2 : pair.a1;
                var redundantAssociation = unifiedAssociation == pair.a1 ? pair.a2 : pair.a1;

                unifiedAssociation.SourceEnd.RelationshipMultiplicity
                    = redundantAssociation.TargetEnd.RelationshipMultiplicity;

                if (redundantAssociation.Constraint != null)
                {
                    unifiedAssociation.Constraint = redundantAssociation.Constraint;

                    unifiedAssociation.Constraint.FromRole = unifiedAssociation.SourceEnd;
                    unifiedAssociation.Constraint.ToRole = unifiedAssociation.TargetEnd;
                }

                FixNavigationProperties(model, unifiedAssociation, redundantAssociation);

                model.RemoveAssociationType(redundantAssociation);
            }
        }

        private static void FixNavigationProperties(
            EdmModel model, AssociationType unifiedAssociation, AssociationType redundantAssociation)
        {
            foreach (var navigationProperty
                in model.EntityTypes
                        .SelectMany(e => e.NavigationProperties)
                        .Where(np => np.Association == redundantAssociation))
            {
                navigationProperty.RelationshipType = unifiedAssociation;
                navigationProperty.FromEndMember = unifiedAssociation.TargetEnd;
                navigationProperty.ToEndMember = unifiedAssociation.SourceEnd;
            }
        }
    }
}
