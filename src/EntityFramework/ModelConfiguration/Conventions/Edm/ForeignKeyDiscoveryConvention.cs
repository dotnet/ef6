// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;

    [ContractClass(typeof(ForeignKeyDiscoveryConventionContracts))]
    internal abstract class ForeignKeyDiscoveryConvention : IEdmConvention<EdmAssociationType>
    {
        protected virtual bool SupportsMultipleAssociations
        {
            get { return false; }
        }

        protected abstract bool MatchDependentKeyProperty(
            EdmAssociationType associationType,
            EdmAssociationEnd dependentAssociationEnd,
            EdmProperty dependentProperty,
            EdmEntityType principalEntityType,
            EdmProperty principalKeyProperty);

        void IEdmConvention<EdmAssociationType>.Apply(EdmAssociationType associationType, EdmModel model)
        {
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            if ((associationType.Constraint != null)
                || associationType.IsIndependent()
                || (associationType.IsOneToOne() && associationType.IsSelfReferencing()))
            {
                return;
            }

            EdmAssociationEnd principalEnd, dependentEnd;
            if (!associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
            {
                return;
            }

            Contract.Assert(principalEnd != null);
            Contract.Assert(principalEnd.EntityType != null);
            Contract.Assert(dependentEnd != null);
            Contract.Assert(dependentEnd.EntityType != null);

            var principalKeyProperties = principalEnd.EntityType.KeyProperties();

            if (principalKeyProperties.Count() == 0)
            {
                return;
            }

            if (!SupportsMultipleAssociations
                && model.GetAssociationTypesBetween(principalEnd.EntityType, dependentEnd.EntityType).Count() > 1)
            {
                return;
            }

            var foreignKeyProperties
                = from p in principalKeyProperties
                  from d in dependentEnd.EntityType.DeclaredProperties
                  where MatchDependentKeyProperty(associationType, dependentEnd, d, principalEnd.EntityType, p)
                        && (p.PropertyType.UnderlyingPrimitiveType == d.PropertyType.UnderlyingPrimitiveType)
                  select d;

            if (!foreignKeyProperties.Any()
                || (foreignKeyProperties.Count() != principalKeyProperties.Count()))
            {
                return;
            }

            var dependentKeyProperties = dependentEnd.EntityType.KeyProperties();

            var fkEquivalentToDependentPk
                = dependentKeyProperties.Count() == foreignKeyProperties.Count()
                  && dependentKeyProperties.All(kp => foreignKeyProperties.Contains(kp));

            if ((dependentEnd.IsMany() || associationType.IsSelfReferencing()) && fkEquivalentToDependentPk)
            {
                return;
            }

            if (!dependentEnd.IsMany()
                && !fkEquivalentToDependentPk)
            {
                return;
            }

            var constraint
                = new EdmAssociationConstraint
                    {
                        DependentEnd = dependentEnd,
                        DependentProperties = foreignKeyProperties.ToList()
                    };

            associationType.Constraint = constraint;

            if (principalEnd.IsRequired())
            {
                constraint.DependentProperties.Each(p => p.PropertyType.IsNullable = false);
            }
        }

        #region Base Member Contracts

        [ContractClassFor(typeof(ForeignKeyDiscoveryConvention))]
        private abstract class ForeignKeyDiscoveryConventionContracts : ForeignKeyDiscoveryConvention
        {
            protected override bool MatchDependentKeyProperty(
                EdmAssociationType associationType,
                EdmAssociationEnd dependentAssociationEnd,
                EdmProperty dependentProperty,
                EdmEntityType principalEntityType,
                EdmProperty principalKeyProperty)
            {
                Contract.Requires(associationType != null);
                Contract.Requires(dependentAssociationEnd != null);
                Contract.Requires(dependentProperty != null);
                Contract.Requires(principalEntityType != null);
                Contract.Requires(principalKeyProperty != null);

                return false;
            }
        }

        #endregion
    }
}
