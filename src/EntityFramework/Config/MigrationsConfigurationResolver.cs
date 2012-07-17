namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Sql;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal class MigrationsConfigurationResolver : IDbDependencyResolver
    {
        private readonly Dictionary<string, MigrationSqlGenerator> _sqlGenerators
            = new Dictionary<string, MigrationSqlGenerator>();

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MigrationsConfigurationResolver()
        {
            SetSqlGenerator("System.Data.SqlClient", new SqlServerMigrationSqlGenerator());
            SetSqlGenerator("System.Data.SqlServerCe.4.0", new SqlCeMigrationSqlGenerator());
        }

        public virtual void SetSqlGenerator(string providerInvariantName, MigrationSqlGenerator migrationSqlGenerator)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));
            Contract.Requires(migrationSqlGenerator != null);

            _sqlGenerators[providerInvariantName] = migrationSqlGenerator;
        }

        public virtual object GetService(Type type, string name)
        {
            return type == typeof(MigrationSqlGenerator) && !string.IsNullOrWhiteSpace(name) && _sqlGenerators.ContainsKey(name)
                       ? _sqlGenerators[name]
                       : null;
        }

        public virtual void Release(object service)
        {
        }
    }
}
