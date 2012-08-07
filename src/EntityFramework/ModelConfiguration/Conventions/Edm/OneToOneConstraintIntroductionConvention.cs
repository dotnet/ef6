// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;

    /// <summary>
    ///     Convention to configure the primary key(s) of the dependent entity type as foreign key(s) in a one:one relationship.
    /// </summary>
    public sealed class OneToOneConstraintIntroductionConvention : IEdmConvention<EdmAssociationType>
    {
        internal OneToOneConstraintIntroductionConvention()
        {
        }

        void IEdmConvention<EdmAssociationType>.Apply(EdmAssociationType associationType, EdmModel model)
        {
            if (associationType.IsOneToOne()
                && !associationType.IsSelfReferencing()
                && !associationType.IsIndependent()
                && (associationType.Constraint == null))
            {
                var sourceKeys = associationType.SourceEnd.EntityType.KeyProperties();
                var targetKeys = associationType.TargetEnd.EntityType.KeyProperties();

                if ((sourceKeys.Count() == targetKeys.Count())
                    && sourceKeys.Select(p => p.PropertyType.UnderlyingPrimitiveType)
                           .SequenceEqual(targetKeys.Select(p => p.PropertyType.UnderlyingPrimitiveType)))
                {
                    EdmAssociationEnd _, dependentEnd;
                    if (associationType.TryGuessPrincipalAndDependentEnds(out _, out dependentEnd)
                        || associationType.IsPrincipalConfigured())
                    {
                        dependentEnd = dependentEnd ?? associationType.TargetEnd;

                        var constraint = new EdmAssociationConstraint
                                             {
                                                 DependentEnd = dependentEnd,
                                                 DependentProperties = dependentEnd.EntityType.KeyProperties().ToList()
                                             };

                        associationType.Constraint = constraint;
                    }
                }
            }
        }
    }
}
