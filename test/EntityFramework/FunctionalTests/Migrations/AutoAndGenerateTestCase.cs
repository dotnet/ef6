namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Xunit;
    using Xunit.Extensions;

    public abstract class AutoAndGenerateTestCase<TContextV1, TContextV2> : DbTestCase
        where TContextV1 : DbContext
        where TContextV2 : DbContext
    {
        private SqlInterceptor _upVerifier;
        private SqlInterceptor _downVerifier;
        private ScaffoldedMigration _generatedMigration_v1;

        protected bool UpDataLoss { get; set; }
        protected bool DownDataLoss { get; set; }

        public override void Init(DatabaseProvider provider, ProgrammingLanguage language)
        {
            base.Init(provider, language);

            _upVerifier = new SqlInterceptor(VerifyUpOperations);
            _downVerifier = new SqlInterceptor(VerifyDownOperations);
        }

        [MigrationsTheory]
        public void Automatic()
        {
            ResetDatabaseToV1();

            DbMigrationsConfiguration migrationsConfiguration;
            try
            {
                migrationsConfiguration =
                    CreateMigrationsConfiguration<TContextV2>(scaffoldedMigrations: _generatedMigration_v1);
                migrationsConfiguration.SetSqlGenerator(DbProviders.Sql, _upVerifier);
                migrationsConfiguration.SetSqlGenerator(DbProviders.SqlCe, _upVerifier);
                new DbMigrator(migrationsConfiguration).Update();

                Assert.False(UpDataLoss);
            }
            catch (AutomaticDataLossException)
            {
                Assert.True(UpDataLoss);

                migrationsConfiguration = CreateMigrationsConfiguration<TContextV2>(
                    automaticDataLossEnabled: true, scaffoldedMigrations: _generatedMigration_v1);
                migrationsConfiguration.SetSqlGenerator(DbProviders.Sql, _upVerifier);
                migrationsConfiguration.SetSqlGenerator(DbProviders.SqlCe, _upVerifier);
                new DbMigrator(migrationsConfiguration).Update();
            }

            // NOTE: This prevents no-op migrations from silently succeeding
            if (!_upVerifier.WasCalled())
            {
                VerifyUpOperations(Enumerable.Empty<MigrationOperation>());
            }

            // Bring up via automatic            
            CreateMigrator<TContextV2>(automaticDataLossEnabled: true, scaffoldedMigrations: _generatedMigration_v1).Update();

            try
            {
                migrationsConfiguration =
                    CreateMigrationsConfiguration<TContextV2>(scaffoldedMigrations: _generatedMigration_v1);
                migrationsConfiguration.SetSqlGenerator(DbProviders.Sql, _downVerifier);
                migrationsConfiguration.SetSqlGenerator(DbProviders.SqlCe, _downVerifier);
                new DbMigrator(migrationsConfiguration).Update(_generatedMigration_v1.MigrationId);
                Assert.False(DownDataLoss);
            }
            catch (AutomaticDataLossException)
            {
                Assert.True(DownDataLoss);
                migrationsConfiguration = CreateMigrationsConfiguration<TContextV2>(
                    automaticDataLossEnabled: true, scaffoldedMigrations: _generatedMigration_v1);
                migrationsConfiguration.SetSqlGenerator(DbProviders.Sql, _downVerifier);
                migrationsConfiguration.SetSqlGenerator(DbProviders.SqlCe, _downVerifier);
                new DbMigrator(migrationsConfiguration).Update(_generatedMigration_v1.MigrationId);
            }

            if (!_downVerifier.WasCalled())
            {
                VerifyDownOperations(Enumerable.Empty<MigrationOperation>());
            }
        }

        [MigrationsTheory]
        [InlineData(ProgrammingLanguage.CSharp)]
        [InlineData(ProgrammingLanguage.VB)]
        public void Generated(ProgrammingLanguage programmingLanguage)
        {
            ProgrammingLanguage = programmingLanguage;
            ResetDatabaseToV1();

            var migrator = CreateMigrator<TContextV2>(scaffoldedMigrations: _generatedMigration_v1);

            var generatedMigration_v2 = new MigrationScaffolder(migrator.Configuration).Scaffold("V2");

            var migrationsConfiguration =
                CreateMigrationsConfiguration<TContextV2>(
                    scaffoldedMigrations: new[] { _generatedMigration_v1, generatedMigration_v2 });
            migrationsConfiguration.SetSqlGenerator(DbProviders.Sql, _upVerifier);
            migrationsConfiguration.SetSqlGenerator(DbProviders.SqlCe, _upVerifier);
            new DbMigrator(migrationsConfiguration).Update();

            // Bring up via generated
            CreateMigrator<TContextV2>(
                scaffoldedMigrations: new[] { _generatedMigration_v1, generatedMigration_v2 })
                .Update();

            migrationsConfiguration =
                CreateMigrationsConfiguration<TContextV2>(
                    scaffoldedMigrations: new[] { _generatedMigration_v1, generatedMigration_v2 });
            migrationsConfiguration.SetSqlGenerator(DbProviders.Sql, _downVerifier);
            migrationsConfiguration.SetSqlGenerator(DbProviders.SqlCe, _downVerifier);
            new DbMigrator(migrationsConfiguration).Update(_generatedMigration_v1.MigrationId);
        }

        private void ResetDatabaseToV1()
        {
            ResetDatabase();

            var migrator = CreateMigrator<TContextV1>();

            _generatedMigration_v1 = new MigrationScaffolder(migrator.Configuration).Scaffold("V1");

            CreateMigrator<TContextV1>(scaffoldedMigrations: _generatedMigration_v1).Update();
        }

        protected abstract void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations);
        protected abstract void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations);

        private class SqlInterceptor : MigrationSqlGenerator
        {
            private static readonly IList<Type> _excludedTypes
                = new List<Type>
                    {
                        typeof(InsertHistoryOperation),
                        typeof(DeleteHistoryOperation)
                    };

            private readonly Action<IEnumerable<MigrationOperation>> _verifyAction;

            private bool _wasCalled;

            public SqlInterceptor(Action<IEnumerable<MigrationOperation>> verifyAction)
            {
                Contract.Requires(verifyAction != null);

                _verifyAction = verifyAction;
            }

            public override IEnumerable<MigrationStatement> Generate(
                IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
            {
                _wasCalled = true;
                _verifyAction(migrationOperations.Where(mo => !_excludedTypes.Contains(mo.GetType())).ToList());

                return Enumerable.Empty<MigrationStatement>();
            }

            public bool WasCalled()
            {
                var wasCalled = _wasCalled;
                _wasCalled = false;

                return wasCalled;
            }
        }
    }

    #region Model stubs

    public class AutoAndGenerateContext_v1 : DbContext
    {
        public AutoAndGenerateContext_v1()
            : base("AutoAndGenerateContext_v1")
        {
        }
    }

    public class AutoAndGenerateContext_v2 : DbContext
    {
        public AutoAndGenerateContext_v2()
            : base("AutoAndGenerateContext_v2")
        {
        }
    }

    #endregion
}