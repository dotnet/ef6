// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Model;
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
        public void MigrationIds_should_not_return_migration_when_not_subclass_of_db_migration()
        {
            var mockType = new MockType("20110816231110_Migration", @namespace: "Migrations");
            var mockAssembly = new MockAssembly(mockType);

            var migrationAssembly = new MigrationAssembly(mockAssembly, mockType.Object.Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_name_does_not_match_pattern()
        {
            var mockType = new MockType("Z0110816231110_Migration", @namespace: "Migrations");
            var mockAssembly = new MockAssembly(mockType);

            var migrationAssembly = new MigrationAssembly(mockAssembly, mockType.Object.Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_no_default_ctor()
        {
            var mockType = new MockType("20110816231110_Migration", hasDefaultCtor: false, @namespace: "Migrations");
            var mockAssembly = new MockAssembly(mockType);

            var migrationAssembly = new MigrationAssembly(mockAssembly, mockType.Object.Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_abstract()
        {
            var mockType
                = new MockType("20110816231110_Migration", @namespace: "Migrations")
                    .TypeAttributes(TypeAttributes.Abstract);

            var mockAssembly = new MockAssembly(mockType);

            var migrationAssembly = new MigrationAssembly(mockAssembly, mockType.Object.Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_nested()
        {
            var mockType = new MockType("20110816231110_Migration", @namespace: "Migrations");
            mockType.SetupGet(t => t.DeclaringType).Returns(typeof(object));

            var mockAssembly = new MockAssembly(mockType);

            var migrationAssembly = new MigrationAssembly(mockAssembly, mockType.Object.Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }

        [Fact]
        public void MigrationIds_should_not_return_migration_when_generic()
        {
            var mockType = new MockType("20110816231110_Migration", @namespace: "Migrations");
            mockType.SetupGet(t => t.IsGenericType).Returns(true);

            var mockAssembly = new MockAssembly(mockType);

            var migrationAssembly = new MigrationAssembly(mockAssembly, mockType.Object.Namespace);

            Assert.False(migrationAssembly.MigrationIds.Any());
        }
    }
}
