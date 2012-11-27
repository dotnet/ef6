// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;

    [SuppressMessage("Microsoft.Contracts", "CC1036",
        Justification = "Due to a bug in code contracts IsNullOrWhiteSpace isn't recognized as pure.")]
    internal static class StringExtensions
    {
        private static readonly Regex _migrationIdPattern = new Regex(@"\d{15}_.+");

        public static bool EqualsIgnoreCase(this string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
        }

        public static string MigrationName(this string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);
            Debug.Assert(migrationId.IsValidMigrationId());

            return migrationId.Substring(16);
        }

        public static bool IsValidMigrationId(this string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            return _migrationIdPattern.IsMatch(migrationId)
                   || migrationId == DbMigrator.InitialDatabase;
        }
    }
}
