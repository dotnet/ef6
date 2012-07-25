// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Db;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbAliasedMetadataItemExtensions
    {
        public static string UniquifyIdentifier(
            this IEnumerable<DbAliasedMetadataItem> aliasedMetadataItems, string identifier)
        {
            Contract.Requires(aliasedMetadataItems != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(identifier));

            return Uniquify(aliasedMetadataItems.Select(n => n.DatabaseIdentifier), identifier);
        }

        public static string UniquifyName(this IEnumerable<DbAliasedMetadataItem> aliasedMetadataItems, string name)
        {
            Contract.Requires(aliasedMetadataItems != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            return Uniquify(aliasedMetadataItems.Select(n => n.Name), name);
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
