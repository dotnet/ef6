// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if SQLSERVER
namespace System.Data.Entity.SqlServer.Utilities
#elif SQLSERVERCOMPACT
namespace System.Data.Entity.SqlServerCompact.Utilities
#else
namespace System.Data.Entity.Utilities
#endif
{
    using Resources;
    using System.Data.Entity.Migrations;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal static class StringExtensions
    {
        private const string StartCharacterExp = @"[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\p{Nl}]";
        private const string OtherCharacterExp = @"[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}]";

        private const string NameExp = StartCharacterExp + OtherCharacterExp + "{0,}";

        private static readonly Regex _undottedNameValidator
            = new Regex(@"^" + NameExp + @"$", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex _migrationIdPattern = new Regex(@"\d{15}_.+");

        public static bool EqualsIgnoreCase(this string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EqualsOrdinal(this string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.Ordinal);
        }

        public static string MigrationName(this string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);
            Debug.Assert(migrationId.IsValidMigrationId());

            return migrationId.Substring(16);
        }

        public static string RestrictTo(this string s, int size)
        {
            if (string.IsNullOrEmpty(s)
                || s.Length <= size)
            {
                return s;
            }

            return s.Substring(0, size);
        }

        public static bool IsValidMigrationId(this string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            return _migrationIdPattern.IsMatch(migrationId)
                   || migrationId == DbMigrator.InitialDatabase;
        }

        public static bool IsAutomaticMigration(this string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            return migrationId.EndsWith(Strings.AutomaticMigration, StringComparison.Ordinal);
        }

        public static string ToAutomaticMigrationId(this string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            var timeStampInt = Convert.ToInt64(migrationId.Substring(0, 15), CultureInfo.InvariantCulture) - 1;

            return timeStampInt + migrationId.Substring(15) + "_" + Strings.AutomaticMigration;
        }

        public static bool IsValidUndottedName(this string name)
        {
            return !string.IsNullOrEmpty(name) && _undottedNameValidator.IsMatch(name);
        }
    }
}
