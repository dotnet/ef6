// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to configure the primary key(s) of the dependent entity type as foreign key(s) in a one:one relationship.
    /// </summary>
    public class OneToOneConstraintIntroductionConvention : IModelConvention<AssociationType>
    {
        public void Apply(AssociationType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            if (edmDataModelItem.IsOneToOne()
                && !edmDataModelItem.IsSelfReferencing()
                && !edmDataModelItem.IsIndependent()
                && (edmDataModelItem.Constraint == null))
            {
                var sourceKeys = edmDataModelItem.SourceEnd.GetEntityType().KeyProperties();
                var targetKeys = edmDataModelItem.TargetEnd.GetEntityType().KeyProperties();

                if ((sourceKeys.Count() == targetKeys.Count())
                    && sourceKeys.Select(p => p.UnderlyingPrimitiveType)
                                 .SequenceEqual(targetKeys.Select(p => p.UnderlyingPrimitiveType)))
                {
                    AssociationEndMember _, dependentEnd;
                    if (edmDataModelItem.TryGuessPrincipalAndDependentEnds(out _, out dependentEnd)
                        || edmDataModelItem.IsPrincipalConfigured())
                    {
                        dependentEnd = dependentEnd ?? edmDataModelItem.TargetEnd;

                        var principalEnd = edmDataModelItem.GetOtherEnd(dependentEnd);

                        var constraint
                            = new ReferentialConstraint(
                                principalEnd,
                                dependentEnd,
                                principalEnd.GetEntityType().KeyProperties().ToList(),
                                dependentEnd.GetEntityType().KeyProperties().ToList());

                        edmDataModelItem.Constraint = constraint;
                    }
                }
            }
        }
    }
}
