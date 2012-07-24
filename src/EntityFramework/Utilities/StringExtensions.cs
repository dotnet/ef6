namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Text.RegularExpressions;

    [SuppressMessage("Microsoft.Contracts", "CC1036",
        Justification = "Due to a bug in code contracts IsNullOrWhiteSpace isn't recognized as pure.")]
    internal static class StringExtensions
    {
        private static readonly Regex _migrationIdPattern = new Regex(@"\d{15}_.+");

        public static IEnumerable<string> Break(this string s, int width = 1000)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(s));
            Contract.Requires(width > 0);

            var processed = 0;

            while (processed < s.Length)
            {
                yield return s.Substring(processed, Math.Min(width, s.Length - processed));

                processed += width;
            }
        }

        public static bool EqualsIgnoreCase(this string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
        }

        public static string MigrationName(this string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));
            Contract.Assert(migrationId.IsValidMigrationId());

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
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            return _migrationIdPattern.IsMatch(migrationId)
                   || migrationId == DbMigrator.InitialDatabase;
        }

        public static bool IsAutomaticMigration(this string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            return migrationId.EndsWith(Strings.AutomaticMigration, StringComparison.Ordinal);
        }

        public static string ToAutomaticMigrationId(this string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            var timeStampInt = Convert.ToInt64(migrationId.Substring(0, 15), CultureInfo.InvariantCulture) - 1;

            return timeStampInt + migrationId.Substring(15) + "_" + Strings.AutomaticMigration;
        }
    }
}
