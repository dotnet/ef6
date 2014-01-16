// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class AlterTableAnnotationsScenarios : DbTestCase
    {
        private class CreateTableWithAllAnnotationMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "Foo", t => new
                    {
                        Id = t.Int(),
                        Beanie = t.String(),
                        Bar = t.String(
                            annotations:
                                new Dictionary<string, AnnotationValues>
                                {
                                    {
                                        CollationAttribute.AnnotationName,
                                        new AnnotationValues(null, new CollationAttribute("Icelandic_CS_AS"))
                                    }
                                }),
                        Boo = t.String()
                    },
                    new Dictionary<string, object>
                    {
                        {
                            CollationAttribute.AnnotationName,
                            new CollationAttribute("Finnish_Swedish_CS_AS")
                        }
                    })
                    .PrimaryKey(t => t.Id);
            }
        }

        private class AlterTableAnnotationsMigration : DbMigration
        {
            public override void Up()
            {
                AlterTableAnnotations(
                    "Foo", t => new
                    {
                        Id = t.Int(),
                        Beanie = t.String(),
                        Bar = t.String(
                            annotations:
                                new Dictionary<string, AnnotationValues>
                                {
                                    {
                                        CollationAttribute.AnnotationName,
                                        new AnnotationValues(null, new CollationAttribute("Icelandic_CS_AS"))
                                    }
                                }),
                        Boo = t.String()
                    },
                    new Dictionary<string, AnnotationValues>
                    {
                        {
                            CollationAttribute.AnnotationName,
                            new AnnotationValues(null, new CollationAttribute("Danish_Norwegian_CS_AS"))
                        }
                    });
            }
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)] // No collation on add column in CE
        public void Can_create_table_with_custom_table_and_columnannotations()
        {
            ResetDatabase();

            var sqlGenerators = new[]
            {
                Tuple.Create<string, MigrationSqlGenerator>(
                    SqlProviderServices.ProviderInvariantName,
                    new SqlServerMigrationSqlGeneratorWtihCollations()),
            };

            var migrator = CreateMigrator<ShopContext_v1>(sqlGenerators: sqlGenerators);
            migrator.Update();

            // Make sure the columns have non-default collation before we start
            migrator = CreateMigrator<ShopContext_v1>(new CreateTableWithAllAnnotationMigration(), sqlGenerators);
            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Beanie");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);

            column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Boo");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);

            column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Bar");
            Assert.Equal("Icelandic_CS_AS", column.Collation);

            migrator = CreateMigrator<ShopContext_v1>(new AlterTableAnnotationsMigration(), sqlGenerators);
            migrator.Update();

            column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Beanie");
            Assert.Equal("Danish_Norwegian_CS_AS", column.Collation);

            column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Boo");
            Assert.Equal("Danish_Norwegian_CS_AS", column.Collation);

            column = Info.Columns.Single(c => c.TableName == "Foo" && c.Name == "Bar");
            Assert.Equal("Icelandic_CS_AS", column.Collation);
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }
    }
}
