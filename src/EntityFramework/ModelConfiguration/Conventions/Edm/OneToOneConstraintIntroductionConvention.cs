// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to configure the primary key(s) of the dependent entity type as foreign key(s) in a one:one relationship.
    /// </summary>
    public class OneToOneConstraintIntroductionConvention : IModelConvention<AssociationType>
    {
        public virtual void Apply(AssociationType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            if (item.IsOneToOne()
                && !item.IsSelfReferencing()
                && !item.IsIndependent()
                && (item.Constraint == null))
            {
                var sourceKeys = item.SourceEnd.GetEntityType().KeyProperties();
                var targetKeys = item.TargetEnd.GetEntityType().KeyProperties();

                if ((sourceKeys.Count() == targetKeys.Count())
                    && sourceKeys.Select(p => p.UnderlyingPrimitiveType)
                                 .SequenceEqual(targetKeys.Select(p => p.UnderlyingPrimitiveType)))
                {
                    AssociationEndMember _, dependentEnd;
                    if (item.TryGuessPrincipalAndDependentEnds(out _, out dependentEnd)
                        || item.IsPrincipalConfigured())
                    {
                        dependentEnd = dependentEnd ?? item.TargetEnd;

                        var principalEnd = item.GetOtherEnd(dependentEnd);

                        var constraint
                            = new ReferentialConstraint(
                                principalEnd,
                                dependentEnd,
                                principalEnd.GetEntityType().KeyProperties().ToList(),
                                dependentEnd.GetEntityType().KeyProperties().ToList());

                        item.Constraint = constraint;
                    }
                }
            }
        }
    }
}
