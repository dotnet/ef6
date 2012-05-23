namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;

    internal class HistoryContextBase<TContext> : DbContext
        where TContext : DbContext
    {
        static HistoryContextBase()
        {
            Database.SetInitializer<TContext>(null);
        }

        public const string TableName = "__MigrationHistory";

        public HistoryContextBase(DbConnection existingConnection, bool contextOwnsConnection = true)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public virtual IDbSet<HistoryRow> History { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoryRow>().ToTable(TableName);
            modelBuilder.Entity<HistoryRow>().HasKey(h => h.MigrationId);
            modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<HistoryRow>().Property(h => h.Model).IsRequired().IsMaxLength();
            modelBuilder.Entity<HistoryRow>().Property(h => h.ProductVersion).HasMaxLength(32).IsRequired();
        }
    }
}
