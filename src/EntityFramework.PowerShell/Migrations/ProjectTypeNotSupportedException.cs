namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;

    [Serializable]
    public sealed class ProjectTypeNotSupportedException : MigrationsException
    {
        public ProjectTypeNotSupportedException(string message)
            : base(message)
        {
        }
    }
}
