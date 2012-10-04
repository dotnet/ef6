// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;

    internal static class DbForeignKeyConstraintMetadataExtensions
    {
        private const string IsTypeConstraint = "IsTypeConstraint";
        private const string IsSplitConstraint = "IsSplitConstraint";
        private const string AssociationType = "AssociationType";

        public static bool GetIsTypeConstraint(this DbForeignKeyConstraintMetadata fk)
        {
            Contract.Requires(fk != null);

            var result = fk.Annotations.GetAnnotation(IsTypeConstraint);
            if (result != null)
            {
                return (bool)result;
            }
            return false;
        }

        public static void SetIsTypeConstraint(this DbForeignKeyConstraintMetadata fk)
        {
            Contract.Requires(fk != null);

            fk.Annotations.SetAnnotation(IsTypeConstraint, true);
        }

        public static void SetIsSplitConstraint(this DbForeignKeyConstraintMetadata fk)
        {
            Contract.Requires(fk != null);

            fk.Annotations.SetAnnotation(IsSplitConstraint, true);
        }

        public static AssociationType GetAssociationType(this DbForeignKeyConstraintMetadata fk)
        {
            Contract.Requires(fk != null);

            return fk.Annotations.GetAnnotation(AssociationType) as AssociationType;
        }

        public static void SetAssociationType(
            this DbForeignKeyConstraintMetadata fk, AssociationType associationType)
        {
            Contract.Requires(fk != null);
            Contract.Requires(associationType != null);

            fk.Annotations.SetAnnotation(AssociationType, associationType);
        }
    }
}
