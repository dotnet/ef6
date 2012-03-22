namespace System.Data.Entity.Migrations.Extensions
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Text.RegularExpressions;

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

            return migrationId.EndsWith(Strings.AutomaticMigration);
        }

        public static bool ComesBefore(this string migrationId1, string migrationId2)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId1));
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId2));
            Contract.Assert(migrationId1.IsValidMigrationId());
            Contract.Assert(migrationId2.IsValidMigrationId());

            var migration1TimeStamp = migrationId1.Substring(0, 15);
            var migration2TimeStamp = migrationId2.Substring(0, 15);

            var comparison = string.CompareOrdinal(migration1TimeStamp, migration2TimeStamp);

            if (comparison > 0)
            {
                return false;
            }

            if (comparison == 0)
            {
                if (migrationId1 == migrationId2 + "_" + Strings.AutomaticMigration)
                {
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}
