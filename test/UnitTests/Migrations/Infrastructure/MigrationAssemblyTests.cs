// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Xunit;

    public class MigrationAssemblyTests
    {
        [Fact]
        public void GetMigration_should_perform_pr()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            const string @namespace = "Migrations";

            var generatedMigration
                = codeGenerator.Generate(
                    "201108162311111_Migration",
                    new MigrationOperation[] { },
                    "Source",
                    "Target",
                    @namespace,
                    "Migration");

            var migrationAssembly = new MigrationAssembly(
                new MigrationCompiler("cs").Compile(@namespace, generatedMigration),
                @namespace);

            Assert.NotNull(migrationAssembly.GetMigration("201108162311111_M"));
        }

        [Fact]
        public void UniquifyName_should_return_name_when_already_unique()
        {
            var migrationAssembly = new MigrationAssembly(new MockAssembly(), "MockNamespace");

            Assert.Equal("Foo", migrationAssembly.UniquifyName("Foo"));
        }

        [Fact]
        public void UniquifyName_should_return_unique_name_when_conflict()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            const string @namespace = "Migrations";

            var generatedMigration1
                = codeGenerator.Generate(
                    "201108162311111_Migration",
                    new MigrationOperation[] { },
                    "Source",
                    "Target",
                    @namespace,
                    "Migration");

            var generatedMigration2
                = codeGenerator.Generate(
                    "201108162311111_Migration1",
                    new MigrationOperation[] { },
                    "Source",
                    "Target",
                    @namespace,
                    "Migration1");

            var migrationAssembly
                = new MigrationAssembly(
                    new MigrationCompiler("cs")
                        .Compile(
                            @namespace,
                            generatedMigration1,
                            generatedMigration2),
                    @namespace);

            Assert.Equal("Migration2", migrationAssembly.UniquifyName("Migration"));
        }

        [Fact]
        public void MigrationIds_should_be_empty_when_no_migrations_exist()
        {
            var migrationAssembly = new MigrationAssembly(new MockAssembly(), "MockNamespace");

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        [Fact]
        public void CreateMigrationId_should_returned_timestamped_name()
        {
            var migrationId = MigrationAssembly.CreateMigrationId("Foo");

            Assert.True(new Regex(@"^\d{15}_[\w ]+$").IsMatch(migrationId));
        }

        [Fact]
        public void MigrationIds_should_return_id_when_migration_is_valid()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            const string @namespace = "Migrations";

            var generatedMigration
                = codeGenerator.Generate(
                    "201108162311111_Migration",
                    new MigrationOperation[] { },
                    "Source",
                    "Target",
                    @namespace,
                    "Migration");

            var migrationAssembly = new MigrationAssembly(
                new MigrationCompiler("cs").Compile(@namespace, generatedMigration),
                @namespace);

            Assert.Equal(1, migrationAssembly.MigrationIds.Count());
        }

        [Fact]
        public void MigrationIds_should_order_migrations()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            const string @namespace = "Migrations";

            var generatedMigration1
                = codeGenerator.Generate(
                    "201108162311111_Migration1",
                    new MigrationOperation[] { },
                    "Source",
                    "Target",
                    @namespace,
                    "Migration1");

            var generatedMigration2
                = codeGenerator.Generate(
                    "201108162311111_Migration2",
                    new MigrationOperation[] { },
                    "Source",
                    "Target",
                    @namespace,
                    "Migration2");

            var generatedMigration3
                = codeGenerator.Generate(
                    "201108162311111_Migration3",
                    new MigrationOperation[] { },
                    "Source",
                    "Target",
                    @namespace,
                    "Migration3");

            var migrationAssembly
                = new MigrationAssembly(
                    new MigrationCompiler("cs")
                        .Compile(
                            @namespace,
                            generatedMigration1,
                            generatedMigration2,
                            generatedMigration3),
                    @namespace);

            Assert.Equal(3, migrationAssembly.MigrationIds.Count());
            Assert.Equal("201108162311111_Migration1", migrationAssembly.MigrationIds.First());
            Assert.Equal("201108162311111_Migration3", migrationAssembly.MigrationIds.Last());
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_in_wrong_namespace()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            const string @namespace = "CorrectNamespace";

            var generatedMigration
                = codeGenerator.Generate(
                    "201108162311111_Migration",
                    new MigrationOperation[] { },
                    "Source",
                    "Target",
                    @namespace,
                    "Migration");

            var migrationAssembly = new MigrationAssembly(
                new MigrationCompiler("cs").Compile(@namespace, generatedMigration),
                "WrongNamespace");

            Assert.Equal(0, migrationAssembly.MigrationIds.Count());
        }

        [Fact]
        public void MigrationIds_should_return_valid_migration()
        {
            var migrationAssembly = new MigrationAssembly(new MockAssembly(typeof(AMigration1)), typeof(AMigration1).Namespace);

            Assert.Equal(1, migrationAssembly.MigrationIds.Count());
        }

        public class AMigration1 : DbMigration, IMigrationMetadata
        {
            public override void Up()
            {
            }

            public string Id { get { return "201108162311111_Migration"; } }
            public string Source { get; private set; }
            public string Target { get; private set; }
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_not_subclass_of_db_migration()
        {
            var migrationAssembly = new MigrationAssembly(new MockAssembly(typeof(AMigration2)), typeof(AMigration2).Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        public class AMigration2 : IMigrationMetadata
        {
            public void Up()
            {
            }

            public string Id { get { return "201108162311111_Migration"; } }
            public string Source { get; private set; }
            public string Target { get; private set; }
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_name_does_not_match_pattern()
        {
            var migrationAssembly = new MigrationAssembly(new MockAssembly(typeof(AMigration3)), typeof(AMigration3).Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        public class AMigration3 : DbMigration, IMigrationMetadata
        {
            public override void Up()
            {
            }

            public string Id { get { return "Z0110816231110_Migration"; } }
            public string Source { get; private set; }
            public string Target { get; private set; }
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_no_default_ctor()
        {
            var migrationAssembly = new MigrationAssembly(new MockAssembly(typeof(AMigration4)), typeof(AMigration4).Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        public class AMigration4 : DbMigration, IMigrationMetadata
        {
            public AMigration4(string _)
            {
            }

            public override void Up()
            {
            }

            public string Id { get { return "20110816231110_Migration"; } }
            public string Source { get; private set; }
            public string Target { get; private set; }
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_abstract()
        {
            var migrationAssembly = new MigrationAssembly(new MockAssembly(typeof(AMigration5)), typeof(AMigration5).Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        public abstract class AMigration5 : DbMigration, IMigrationMetadata
        {
            public override void Up()
            {
            }

            public string Id { get { return "20110816231110_Migration"; } }
            public string Source { get; private set; }
            public string Target { get; private set; }
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_generic()
        {
            var migrationAssembly = new MigrationAssembly(new MockAssembly(typeof(AMigration6<>)), typeof(AMigration6<>).Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        public class AMigration6<T> : DbMigration, IMigrationMetadata
        {
            public override void Up()
            {
            }

            public string Id { get { return "201108162311111_Migration"; } }
            public string Source { get; private set; }
            public string Target { get; private set; }
        }
    }
}
