// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class RelationshipMultiplicityExtensions
    {
        public static bool IsMany(this RelationshipMultiplicity associationEndKind)
        {
            return associationEndKind == RelationshipMultiplicity.Many;
        }

        public static bool IsOptional(this RelationshipMultiplicity associationEndKind)
        {
            return associationEndKind == RelationshipMultiplicity.ZeroOrOne;
        }

        public static bool IsRequired(this RelationshipMultiplicity associationEndKind)
        {
            return associationEndKind == RelationshipMultiplicity.One;
        }
    }
}
