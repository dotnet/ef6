// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;

    internal static class EdmAssociationTypeExtensions
    {
        private const string IsIndependentAnnotation = "IsIndependent";
        private const string IsPrincipalConfiguredAnnotation = "IsPrincipalConfigured";

        public static EdmAssociationType Initialize(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);

            associationType.SourceEnd = new EdmAssociationEnd();
            associationType.TargetEnd = new EdmAssociationEnd();

            return associationType;
        }

        public static void MarkIndependent(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);

            associationType.Annotations.SetAnnotation(IsIndependentAnnotation, true);
        }

        public static bool IsIndependent(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);

            var isIndependent
                = associationType.Annotations.GetAnnotation(IsIndependentAnnotation);

            return isIndependent != null ? (bool)isIndependent : false;
        }

        public static void MarkPrincipalConfigured(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);

            associationType.Annotations.SetAnnotation(IsPrincipalConfiguredAnnotation, true);
        }

        public static bool IsPrincipalConfigured(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);

            var isPrincipalConfigured
                = associationType.Annotations.GetAnnotation(IsPrincipalConfiguredAnnotation);

            return isPrincipalConfigured != null ? (bool)isPrincipalConfigured : false;
        }

        public static EdmAssociationEnd GetOtherEnd(
            this EdmAssociationType associationType, EdmAssociationEnd associationEnd)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(associationEnd != null);

            return associationEnd == associationType.SourceEnd
                       ? associationType.TargetEnd
                       : associationType.SourceEnd;
        }

        public static object GetConfiguration(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);

            return associationType.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this EdmAssociationType associationType, object configuration)
        {
            Contract.Requires(associationType != null);

            associationType.Annotations.SetConfiguration(configuration);
        }

        public static bool HasDeleteAction(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return (associationType.SourceEnd.DeleteAction.HasValue
                    || associationType.TargetEnd.DeleteAction.HasValue);
        }

        public static bool IsRequiredToMany(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return associationType.SourceEnd.IsRequired()
                   && associationType.TargetEnd.IsMany();
        }

        public static bool IsManyToRequired(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return associationType.SourceEnd.IsMany()
                   && associationType.TargetEnd.IsRequired();
        }

        public static bool IsManyToMany(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return associationType.SourceEnd.IsMany()
                   && associationType.TargetEnd.IsMany();
        }

        public static bool IsOneToOne(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            return !associationType.SourceEnd.IsMany()
                   && !associationType.TargetEnd.IsMany();
        }

        public static bool IsSelfReferencing(this EdmAssociationType associationType)
        {
            Contract.Requires(associationType != null);

            var sourceEnd = associationType.SourceEnd;
            var targetEnd = associationType.TargetEnd;

            Contract.Assert(sourceEnd != null);
            Contract.Assert(targetEnd != null);
            Contract.Assert(sourceEnd.EntityType != null);
            Contract.Assert(targetEnd.EntityType != null);

            return ((sourceEnd.EntityType.GetRootType() == targetEnd.EntityType.GetRootType()));
        }

        public static bool IsRequiredToNonRequired(this EdmAssociationType associationType)
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
            this EdmAssociationType associationType,
            out EdmAssociationEnd principalEnd,
            out EdmAssociationEnd dependentEnd)
        {
            Contract.Requires(associationType != null);
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            principalEnd = dependentEnd = null;

            var sourceEnd = associationType.SourceEnd;
            var targetEnd = associationType.TargetEnd;

            if (sourceEnd.EndKind
                != targetEnd.EndKind)
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
