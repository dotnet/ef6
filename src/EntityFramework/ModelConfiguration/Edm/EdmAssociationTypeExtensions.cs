// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;

    internal static class EdmAssociationTypeExtensions
    {
        private const string IsIndependentAnnotation = "IsIndependent";
        private const string IsPrincipalConfiguredAnnotation = "IsPrincipalConfigured";

        public static void MarkIndependent(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);

            associationType.Annotations.SetAnnotation(IsIndependentAnnotation, true);
        }

        public static bool IsIndependent(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);

            var isIndependent
                = associationType.Annotations.GetAnnotation(IsIndependentAnnotation);

            return isIndependent != null && (bool)isIndependent;
        }

        public static void MarkPrincipalConfigured(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);

            associationType.Annotations.SetAnnotation(IsPrincipalConfiguredAnnotation, true);
        }

        public static bool IsPrincipalConfigured(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);

            var isPrincipalConfigured
                = associationType.Annotations.GetAnnotation(IsPrincipalConfiguredAnnotation);

            return isPrincipalConfigured != null && (bool)isPrincipalConfigured;
        }

        public static AssociationEndMember GetOtherEnd(
            this AssociationType associationType, AssociationEndMember associationEnd)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(associationEnd != null);

            return associationEnd == associationType.SourceEnd
                       ? associationType.TargetEnd
                       : associationType.SourceEnd;
        }

        public static object GetConfiguration(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);

            return associationType.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this AssociationType associationType, object configuration)
        {
            Contract.Requires(associationType != null);

            associationType.Annotations.SetConfiguration(configuration);
        }

        public static bool IsRequiredToMany(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return associationType.SourceEnd.IsRequired()
                   && associationType.TargetEnd.IsMany();
        }

        public static bool IsManyToRequired(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return associationType.SourceEnd.IsMany()
                   && associationType.TargetEnd.IsRequired();
        }

        public static bool IsManyToMany(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return associationType.SourceEnd.IsMany()
                   && associationType.TargetEnd.IsMany();
        }

        public static bool IsOneToOne(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return !associationType.SourceEnd.IsMany()
                   && !associationType.TargetEnd.IsMany();
        }

        public static bool IsSelfReferencing(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);

            var sourceEnd = associationType.SourceEnd;
            var targetEnd = associationType.TargetEnd;

            Contract.Assert(sourceEnd != null);
            Contract.Assert(targetEnd != null);
            Contract.Assert(sourceEnd.GetEntityType() != null);
            Contract.Assert(targetEnd.GetEntityType() != null);

            return ((sourceEnd.GetEntityType().GetRootType() == targetEnd.GetEntityType().GetRootType()));
        }

        public static bool IsRequiredToNonRequired(this AssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return (associationType.SourceEnd.IsRequired() && !associationType.TargetEnd.IsRequired())
                   || (associationType.TargetEnd.IsRequired() && !associationType.SourceEnd.IsRequired());
        }

        /// <summary>
        ///     Attempt to determine the principal and dependent ends of this association.
        /// 
        ///     The following table illustrates the solution space.
        ///  
        ///     Source | Target || Prin  | Dep   |
        ///     -------|--------||-------|-------|
        ///     1      | 1      || -     | -     | 
        ///     1      | 0..1   || Sr    | Ta    |
        ///     1      | *      || Sr    | Ta    |
        ///     0..1   | 1      || Ta    | Sr    |
        ///     0..1   | 0..1   || -     | -     |
        ///     0..1   | *      || Sr    | Ta    |
        ///     *      | 1      || Ta    | Sr    |
        ///     *      | 0..1   || Ta    | Sr    |
        ///     *      | *      || -     | -     |
        /// </summary>
        public static bool TryGuessPrincipalAndDependentEnds(
            this AssociationType associationType,
            out AssociationEndMember principalEnd,
            out AssociationEndMember dependentEnd)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            principalEnd = dependentEnd = null;

            var sourceEnd = associationType.SourceEnd;
            var targetEnd = associationType.TargetEnd;

            if (sourceEnd.RelationshipMultiplicity
                != targetEnd.RelationshipMultiplicity)
            {
                principalEnd
                    = (sourceEnd.IsRequired()
                       || (sourceEnd.IsOptional() && targetEnd.IsMany()))
                          ? sourceEnd
                          : targetEnd;

                dependentEnd
                    = (principalEnd == sourceEnd)
                          ? targetEnd
                          : sourceEnd;
            }

            return (principalEnd != null);
        }
    }
}
