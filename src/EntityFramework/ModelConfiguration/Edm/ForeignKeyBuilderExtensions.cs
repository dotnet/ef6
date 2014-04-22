// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    internal static class ForeignKeyBuilderExtensions
    {
        private const string IsTypeConstraint = "IsTypeConstraint";
        private const string IsSplitConstraint = "IsSplitConstraint";
        private const string AssociationType = "AssociationType";
        private const string PreferredNameAnnotation = "PreferredName";

        public static string GetPreferredName(this ForeignKeyBuilder fk)
        {
            DebugCheck.NotNull(fk);

            return (string)fk.Annotations.GetAnnotation(PreferredNameAnnotation);
        }

        public static void SetPreferredName(this ForeignKeyBuilder fk, string name)
        {
            DebugCheck.NotNull(fk);

            fk.GetMetadataProperties().SetAnnotation(PreferredNameAnnotation, name);
        }

        public static bool GetIsTypeConstraint(this ForeignKeyBuilder fk)
        {
            DebugCheck.NotNull(fk);

            var result = fk.Annotations.GetAnnotation(IsTypeConstraint);
            if (result != null)
            {
                return (bool)result;
            }
            return false;
        }

        public static void SetIsTypeConstraint(this ForeignKeyBuilder fk)
        {
            DebugCheck.NotNull(fk);

            fk.GetMetadataProperties().SetAnnotation(IsTypeConstraint, true);
        }

        public static void SetIsSplitConstraint(this ForeignKeyBuilder fk)
        {
            DebugCheck.NotNull(fk);

            fk.GetMetadataProperties().SetAnnotation(IsSplitConstraint, true);
        }

        public static AssociationType GetAssociationType(this ForeignKeyBuilder fk)
        {
            DebugCheck.NotNull(fk);

            return fk.Annotations.GetAnnotation(AssociationType) as AssociationType;
        }

        public static void SetAssociationType(
            this ForeignKeyBuilder fk, AssociationType associationType)
        {
            DebugCheck.NotNull(fk);
            DebugCheck.NotNull(associationType);

            fk.GetMetadataProperties().SetAnnotation(AssociationType, associationType);
        }
    }
}
