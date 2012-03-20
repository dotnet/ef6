namespace System.Data.Entity.Migrations.Utilities
{
    using System.Data.Common;

    internal class EmptyContext : DbContext
    {
        static EmptyContext()
        {
            Database.SetInitializer<EmptyContext>(null);
        }

        public EmptyContext(DbConnection existingConnection)
            : base(existingConnection, false)
        {
        }
    }
}