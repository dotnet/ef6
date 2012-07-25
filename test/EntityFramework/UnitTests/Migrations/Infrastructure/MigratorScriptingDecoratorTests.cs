// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class MigratorScriptingDecoratorTests : DbTestCase
    {
        [MigrationsTheory]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal("innerMigrator", Assert.Throws<ArgumentNullException>(() => new MigratorScriptingDecorator(null)).ParamName);
        }

        [MigrationsTheory]
        public void ScriptUpdate_should_return_valid_script()
        {
            ResetDatabase();

            var migrator = new MigratorScriptingDecorator(CreateMigrator<ShopContext_v1>());

            var script = migrator.ScriptUpdate(null, null);

            Assert.True(script.Length > 6000);
        }

        [MigrationsTheory]
        public void ScriptUpdate_should_not_create_database()
        {
            var migrator
                = new MigratorScriptingDecorator(
                CreateMigrator<ShopContext_v1>(targetDatabase: "NoSuchDatabase"));

            DropDatabase();

            migrator.ScriptUpdate(null, null);

            Assert.False(DatabaseExists());
        }

        [MigrationsTheory]
        public void ScriptUpdate_should_throw_on_arbitrary_down()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();
            var scriptingDecorator = new MigratorScriptingDecorator(migrator);

            Assert.Equal(Strings.DownScriptWindowsNotSupported, Assert.Throws<MigrationsException>(() => scriptingDecorator.ScriptUpdate("000000000000001_Second", "000000000000000_First")).Message);
        }

        [MigrationsTheory]
        public void ScriptUpdate_should_throw_on_automatic_source()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();
            migrator.Update();

            var automaticMigration = migrator.GetDatabaseMigrations().Single();
            var scriptingDecorator = new MigratorScriptingDecorator(migrator);

            Assert.Equal(Strings.AutoNotValidForScriptWindows(automaticMigration), Assert.Throws<MigrationsException>(() => scriptingDecorator.ScriptUpdate(automaticMigration, null)).Message);
        }

        [MigrationsTheory]
        public void ScriptUpdate_should_throw_on_automatic_target()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();
            migrator.Update();

            var automaticMigration = migrator.GetDatabaseMigrations().Single();
            var scriptingDecorator = new MigratorScriptingDecorator(migrator);

            Assert.Equal(Strings.AutoNotValidForScriptWindows(automaticMigration), Assert.Throws<MigrationsException>(() => scriptingDecorator.ScriptUpdate(DbMigrator.InitialDatabase, automaticMigration)).Message);
        }
    }
}