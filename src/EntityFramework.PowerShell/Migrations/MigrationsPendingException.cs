namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;

    [Serializable]
    public sealed class MigrationsPendingException : MigrationsException
    {
        public MigrationsPendingException(string message)
            : base(message)
        {
        }
    }
}
