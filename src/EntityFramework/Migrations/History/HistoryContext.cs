// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;

    internal class HistoryContext : DbContext
    {
        public const string TableName = "__MigrationHistory";

        static HistoryContext()
        {
            Database.SetInitializer<HistoryContext>(null);
        }

        private readonly string _defaultSchema;

        public HistoryContext(DbConnection existingConnection, bool contextOwnsConnection, string defaultSchema)
            : base(existingConnection, contextOwnsConnection)
        {
            _defaultSchema = defaultSchema;

            Configuration.ValidateOnSaveEnabled = false;
        }

        public string DefaultSchema
        {
            get { return _defaultSchema; }
        }

        public virtual IDbSet<HistoryRow> History { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_defaultSchema);

            modelBuilder.Entity<HistoryRow>().ToTable(TableName);
            modelBuilder.Entity<HistoryRow>().HasKey(
                h => new
                         {
                             h.MigrationId,
                             h.ContextKey
                         });
            modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<HistoryRow>().Property(h => h.ContextKey).HasMaxLength(512).IsRequired();
            modelBuilder.Entity<HistoryRow>().Property(h => h.Model).IsRequired().IsMaxLength();
            modelBuilder.Entity<HistoryRow>().Property(h => h.ProductVersion).HasMaxLength(32).IsRequired();
        }
    }
}
