// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal static class EntitySetExtensions
    {
        public static object GetConfiguration(this EntitySet entitySet)
        {
            DebugCheck.NotNull(entitySet);

            return entitySet.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this EntitySet entitySet, object configuration)
        {
            DebugCheck.NotNull(entitySet);

            entitySet.Annotations.SetConfiguration(configuration);
        }

        public static string UniquifyIdentifier(
            this IEnumerable<EntitySet> aliasedMetadataItems, string identifier)
        {
            DebugCheck.NotNull(aliasedMetadataItems);
            DebugCheck.NotEmpty(identifier);

            return Uniquify(aliasedMetadataItems.Select(n => n.Table), identifier);
        }

        private static string Uniquify(IEnumerable<string> inputStrings, string targetString)
        {
            var uniqueString = targetString;
            var i = 0;

            while (inputStrings.Any(n => string.Equals(n, uniqueString, StringComparison.Ordinal)))
            {
                uniqueString = targetString + ++i;
            }

            return uniqueString;
        }
    }
}
