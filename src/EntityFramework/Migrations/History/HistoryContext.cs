namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;

    internal class HistoryContext : HistoryContextBase<HistoryContext>
    {
        public HistoryContext(DbConnection existingConnection, bool contextOwnsConnection = true)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
#pragma warning disable 612,618
            modelBuilder.Entity<HistoryRow>().Ignore(h => h.CreatedOn);
#pragma warning restore 612,618
        }
    }
}