// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// Used for generating <see cref="DateTime.UtcNow" /> values that are always in sequential
    /// order for the calling thread.
    /// </summary>
    internal static class UtcNowGenerator
    {
        public const string MigrationIdFormat = "yyyyMMddHHmmssf";

        private static readonly ThreadLocal<DateTime> _lastNow = new ThreadLocal<DateTime>(() => DateTime.UtcNow);

        /// <summary>
        /// Returns the value of <see cref="DateTime.UtcNow" /> unless this value would be the same as the
        /// last value returned by this thread calling this method, in which case the thread pushes the value
        /// a little bit into the future. The comparison is in terms of the form used to store migration ID
        /// in the database--i.e. to the 1/10 second.
        /// </summary>
        /// <remarks>
        /// There should never be any pushing to the future involved for normal use of migrations, but when
        /// this method is called in rapid succession while testing or otherwise calling the DbMigrator APIs
        /// there may be occasional sleeping.
        /// </remarks>
        public static DateTime UtcNow()
        {
            var now = DateTime.UtcNow;
            var lastNow = _lastNow.Value;

            // At least on some machines DateTime.UtcNow can return values that are a little bit (< 1 second) less than the
            // last value that it returned.
            if (now <= lastNow
                || now.ToString(MigrationIdFormat, CultureInfo.InvariantCulture)
                      .Equals(lastNow.ToString(MigrationIdFormat, CultureInfo.InvariantCulture), StringComparison.Ordinal))
            {
                now = lastNow.AddMilliseconds(100);

                Debug.Assert(!now.ToString(MigrationIdFormat).Equals(lastNow.ToString(MigrationIdFormat), StringComparison.Ordinal));
            }

            _lastNow.Value = now;

            return now;
        }

        /// <summary>
        /// Same as UtcNow method bur returns the time in the timestamp format used in migration IDs.
        /// </summary>
        public static string UtcNowAsMigrationIdTimestamp()
        {
            return UtcNow().ToString(MigrationIdFormat, CultureInfo.InvariantCulture);
        }
    }
}
