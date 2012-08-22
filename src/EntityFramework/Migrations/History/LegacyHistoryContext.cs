// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Common;

    /// <summary>
    ///     This is a version of the HistoryContext that still includes CreatedOn in its model.
    ///     It is used when figuring out whether or not the CreatedOn column exists and so should
    ///     be dropped.
    /// </summary>
    internal sealed class LegacyHistoryContext : DbContext
    {
        static LegacyHistoryContext()
        {
            Database.SetInitializer<LegacyHistoryContext>(null);
        }

        public LegacyHistoryContext(DbConnection existingConnection)
            : base(existingConnection, false)
        {
        }

        public IDbSet<LegacyHistoryRow> History { get; set; }
    }

    [Table(HistoryContext.TableName)]
    internal sealed class LegacyHistoryRow
    {
        public int Id { get; set; } // dummy
        public DateTime CreatedOn { get; set; }
    }
}
