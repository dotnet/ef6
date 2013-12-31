// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    internal static class EdmMemberExtensions
    {
        private static readonly string[] _timestampTypes = new[]
            {
                "timestamp",
                "rowversion"
            };

        public static bool IsKey(this EdmMember member)
        {
            Debug.Assert(member != null, "member is null.");

            return ((EntityType)member.DeclaringType).KeyMembers.Contains(member);
        }

        public static bool HasConventionalKeyName(this EdmMember member)
        {
            Debug.Assert(member != null, "member is null.");

            return member.Name.EqualsIgnoreCase("Id")
                || member.Name.EqualsIgnoreCase(member.DeclaringType.Name + "Id");
        }

        public static bool IsTimestamp(this EdmProperty property)
        {
            Debug.Assert(property != null, "property is null.");

            // NOTE: This should also check that the property's ConcurrencyMode is Fixed, but
            //       that information is not conveyed in the reverse engineered model.
            return _timestampTypes.ContainsIgnoreCase(property.TypeName)
                && property.IsStoreGeneratedComputed
                && !property.Nullable
                && property.MaxLength == 8;
        }
    }
}
