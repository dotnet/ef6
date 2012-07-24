namespace System.Data.Entity.Config
{
    using System.Data.Entity.Migrations.Sql;

    internal class MigrationsConfigurationResolver : IDbDependencyResolver
    {
        public virtual object GetService(Type type, string name)
        {
            return type == typeof(MigrationSqlGenerator)
                       ? (name == "System.Data.SqlClient"
                              ? new SqlServerMigrationSqlGenerator()
                              : (name == "System.Data.SqlServerCe.4.0" ? new SqlCeMigrationSqlGenerator() : null))
                       : null;
        }

        public virtual void Release(object service)
        {
        }
    }
}
