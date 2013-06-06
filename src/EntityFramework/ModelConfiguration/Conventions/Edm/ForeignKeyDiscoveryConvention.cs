// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public abstract class ForeignKeyDiscoveryConvention : IModelConvention<AssociationType>
    {
        protected virtual bool SupportsMultipleAssociations
        {
            get { return false; }
        }

        protected abstract bool MatchDependentKeyProperty(
            AssociationType associationType,
            AssociationEndMember dependentAssociationEnd,
            EdmProperty dependentProperty,
            EntityType principalEntityType,
            EdmProperty principalKeyProperty);

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Apply(AssociationType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            Debug.Assert(edmDataModelItem.SourceEnd != null);
            Debug.Assert(edmDataModelItem.TargetEnd != null);

            if ((edmDataModelItem.Constraint != null)
                || edmDataModelItem.IsIndependent()
                || (edmDataModelItem.IsOneToOne() && edmDataModelItem.IsSelfReferencing()))
            {
                return;
            }

            AssociationEndMember principalEnd, dependentEnd;
            if (!edmDataModelItem.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
            {
                return;
            }

            Debug.Assert(principalEnd != null);
            Debug.Assert(principalEnd.GetEntityType() != null);
            Debug.Assert(dependentEnd != null);
            Debug.Assert(dependentEnd.GetEntityType() != null);

            var principalKeyProperties = principalEnd.GetEntityType().KeyProperties();

            if (!principalKeyProperties.Any())
            {
                return;
            }

            if (!SupportsMultipleAssociations
                && model.GetAssociationTypesBetween(principalEnd.GetEntityType(), dependentEnd.GetEntityType()).Count() > 1)
            {
                return;
            }

            var foreignKeyProperties
                = from p in principalKeyProperties
                  from d in dependentEnd.GetEntityType().DeclaredProperties
                  where MatchDependentKeyProperty(edmDataModelItem, dependentEnd, d, principalEnd.GetEntityType(), p)
                        && (p.UnderlyingPrimitiveType == d.UnderlyingPrimitiveType)
                  select d;

            if (!foreignKeyProperties.Any()
                || (foreignKeyProperties.Count() != principalKeyProperties.Count()))
            {
                return;
            }

            var dependentKeyProperties = dependentEnd.GetEntityType().KeyProperties();

            var fkEquivalentToDependentPk
                = dependentKeyProperties.Count() == foreignKeyProperties.Count()
                  && dependentKeyProperties.All(foreignKeyProperties.Contains);

            if ((dependentEnd.IsMany() || edmDataModelItem.IsSelfReferencing()) && fkEquivalentToDependentPk)
            {
                return;
            }

            if (!dependentEnd.IsMany()
                && !fkEquivalentToDependentPk)
            {
                return;
            }

            var constraint
                = new ReferentialConstraint(
                    principalEnd,
                    dependentEnd,
                    principalKeyProperties.ToList(),
                    foreignKeyProperties.ToList());

            edmDataModelItem.Constraint = constraint;

            if (principalEnd.IsRequired())
            {
                constraint.ToProperties.Each(p => p.Nullable = false);
            }
        }
    }
}
