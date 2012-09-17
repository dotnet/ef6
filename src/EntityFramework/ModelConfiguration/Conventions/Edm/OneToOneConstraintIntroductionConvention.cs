// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;

    /// <summary>
    ///     Convention to configure the primary key(s) of the dependent entity type as foreign key(s) in a one:one relationship.
    /// </summary>
    public class OneToOneConstraintIntroductionConvention : IEdmConvention<EdmAssociationType>
    {
        public void Apply(EdmAssociationType edmDataModelItem, EdmModel model)
        {
            if (edmDataModelItem.IsOneToOne()
                && !edmDataModelItem.IsSelfReferencing()
                && !edmDataModelItem.IsIndependent()
                && (edmDataModelItem.Constraint == null))
            {
                var sourceKeys = edmDataModelItem.SourceEnd.EntityType.KeyProperties();
                var targetKeys = edmDataModelItem.TargetEnd.EntityType.KeyProperties();

                if ((sourceKeys.Count() == targetKeys.Count())
                    && sourceKeys.Select(p => p.PropertyType.UnderlyingPrimitiveType)
                           .SequenceEqual(targetKeys.Select(p => p.PropertyType.UnderlyingPrimitiveType)))
                {
                    EdmAssociationEnd _, dependentEnd;
                    if (edmDataModelItem.TryGuessPrincipalAndDependentEnds(out _, out dependentEnd)
                        || edmDataModelItem.IsPrincipalConfigured())
                    {
                        dependentEnd = dependentEnd ?? edmDataModelItem.TargetEnd;

                        var constraint = new EdmAssociationConstraint
                                             {
                                                 DependentEnd = dependentEnd,
                                                 DependentProperties = dependentEnd.EntityType.KeyProperties().ToList()
                                             };

                        edmDataModelItem.Constraint = constraint;
                    }
                }
            }
        }
    }
}
