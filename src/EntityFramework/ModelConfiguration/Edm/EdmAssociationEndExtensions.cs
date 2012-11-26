// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class EdmAssociationEndExtensions
    {
        public static bool IsMany(this RelationshipEndMember associationEnd)
        {
            return associationEnd.RelationshipMultiplicity.IsMany();
        }

        public static bool IsOptional(this RelationshipEndMember associationEnd)
        {
            return associationEnd.RelationshipMultiplicity.IsOptional();
        }

        public static bool IsRequired(this RelationshipEndMember associationEnd)
        {
            return associationEnd.RelationshipMultiplicity.IsRequired();
        }
    }
}
