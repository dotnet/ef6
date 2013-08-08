// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class is used by Code First Migrations to read and write migration history
    /// from the database.
    /// </summary>
    public class HistoryRow
    {
        /// <summary>
        /// Gets or sets the Id of the migration this row represents.
        /// </summary>
        public string MigrationId { get; set; }

        /// <summary>
        /// Gets or sets a key representing to which context the row applies.
        /// </summary>
        public string ContextKey { get; set; }

        /// <summary>
        /// Gets or sets the state of the model after this migration was applied.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] Model { get; set; }

        /// <summary>
        /// Gets or sets the version of Entity Framework that created this entry.
        /// </summary>
        public string ProductVersion { get; set; }
    }
}
