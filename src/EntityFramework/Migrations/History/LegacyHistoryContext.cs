namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;

    /// <summary>
    /// This is a version of the HistoryContext that still includes CreatedOn in its model.
    /// It is used when figuring out whether or not the CreatedOn column exists and so should
    /// be dropped.
    /// </summary>
    internal class LegacyHistoryContext : HistoryContextBase<LegacyHistoryContext>
    {
        public LegacyHistoryContext(DbConnection existingConnection, bool contextOwnsConnection = true)
            : base(existingConnection, contextOwnsConnection)
        {
        }
    }
}