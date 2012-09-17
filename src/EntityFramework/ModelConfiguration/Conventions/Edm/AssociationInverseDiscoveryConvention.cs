// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
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
            var associationPairs
                = (from a1 in model.GetAssociationTypes()
                   from a2 in model.GetAssociationTypes()
                   where a1 != a2
                   where a1.SourceEnd.EntityType == a2.TargetEnd.EntityType
                         && a1.TargetEnd.EntityType == a2.SourceEnd.EntityType
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
                        (a, b) => a.a1.SourceEnd.EntityType == b.a2.TargetEnd.EntityType
                                  && a.a1.TargetEnd.EntityType == b.a2.SourceEnd.EntityType)
                    .Where(g => g.Count() == 1)
                    .Select(g => g.Single());

            foreach (var pair in associationPairs)
            {
                var unifiedAssociation = pair.a2.GetConfiguration() != null ? pair.a2 : pair.a1;
                var redundantAssociation = unifiedAssociation == pair.a1 ? pair.a2 : pair.a1;

                unifiedAssociation.SourceEnd.EndKind = redundantAssociation.TargetEnd.EndKind;

                if (redundantAssociation.Constraint != null)
                {
                    unifiedAssociation.Constraint = redundantAssociation.Constraint;
                    unifiedAssociation.Constraint.DependentEnd =
                        unifiedAssociation.Constraint.DependentEnd.EntityType == unifiedAssociation.SourceEnd.EntityType
                            ? unifiedAssociation.SourceEnd
                            : unifiedAssociation.TargetEnd;
                }

                FixNavigationProperties(model, unifiedAssociation, redundantAssociation);

                model.RemoveAssociationType(redundantAssociation);
            }
        }

        private static void FixNavigationProperties(
            EdmModel model, EdmAssociationType unifiedAssociation, EdmAssociationType redundantAssociation)
        {
            foreach (var navigationProperty
                in model.GetEntityTypes()
                    .SelectMany(e => e.NavigationProperties)
                    .Where(np => np.Association == redundantAssociation))
            {
                navigationProperty.Association = unifiedAssociation;
                navigationProperty.ResultEnd = unifiedAssociation.SourceEnd;
            }
        }
    }
}
