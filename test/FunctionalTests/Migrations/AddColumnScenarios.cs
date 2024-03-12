// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NETFRAMEWORK

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Hierarchy;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class AddColumnScenarios : DbTestCase
    {
        public AddColumnScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
            databaseProviderFixture.DatabaseName = nameof(AddColumnScenarios);
        }

        private class AddColumnMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.Int(nullable: false, defaultValue: 0));
            }
        }

        [MigrationsTheory]
        public void Can_add_non_nullable_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddColumnMigration());

            migrator.Update();

            var column = Info.Columns.SingleOrDefault(c => c.TableName == "MigrationsCustomers" && c.Name == "new_col");
            Assert.NotNull(column);
            Assert.Equal("NO", column.IsNullable);
            Assert.True(column.Default.Contains("0"));
        }

        [MigrationsTheory]
        public void Can_add_non_nullable_column_with_default_value_when_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                context.Customers.Add(
                    new MigrationsCustomer
                        {
                            HomeAddress = new MigrationsAddress(),
                            WorkAddress = new MigrationsAddress(),
                            DateOfBirth = DateTime.Now
                        });

                context.SaveChanges();
            }

            migrator = CreateMigrator<ShopContext_v1>(new AddColumnMigration());

            migrator.Update();

            Assert.True(ColumnExists("MigrationsCustomers", "new_col"));
        }

        private class AddColumnWithCustomSqlMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.Int(nullable: false, defaultValue: 0));
                Sql("select new_col from MigrationsCustomers");
            }
        }

        [MigrationsTheory]
        public void Can_add_column_and_then_reference_it_in_custom_sql()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddColumnWithCustomSqlMigration());

            migrator.Update();
        }

        private class AddColumnWithDateTimeDefault : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "date_col", c => c.DateTime(nullable: false, defaultValue: DateTime.Today));
            }
        }

        [MigrationsTheory]
        public void Can_add_column_with_datetime_default()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddColumnWithDateTimeDefault());

            migrator.Update();
        }

        private class AddColumnWithHierarchyIdDefault : DbMigration
        {
            public override void Up()
            {
                AddColumn(
                    "MigrationsCustomers", "hierarchyid_col",
                    c => c.HierarchyId(nullable: false, defaultValue: HierarchyId.GetRoot()));
            }
        }

        [MigrationsTheory]
        public void Can_add_column_with_hierarchyid_default()
        {
            WhenNotSqlCe(
                () =>
                    {
                        ResetDatabase();

                        var migrator = CreateMigrator<ShopContext_v1>();

                        migrator.Update();

                        migrator = CreateMigrator<ShopContext_v1>(new AddColumnWithHierarchyIdDefault());

                        migrator.Update();
                    });
        }

        private class AddColumnWithGeographyDefault : DbMigration
        {
            public override void Up()
            {
                AddColumn(
                    "MigrationsCustomers", "we_know_where_you_live",
                    c => c.Geography(nullable: false, defaultValue: DbGeography.FromText("POINT (6 7)")));
            }
        }

        [MigrationsTheory]
        public void Can_add_column_with_geography_default()
        {
            WhenNotSqlCe(
                () =>
                    {
                        ResetDatabase();

                        var migrator = CreateMigrator<ShopContext_v1>();

                        migrator.Update();

                        migrator = CreateMigrator<ShopContext_v1>(new AddColumnWithGeographyDefault());

                        migrator.Update();
                    });
        }

        private class AddColumnWithGeometryDefault : DbMigration
        {
            public override void Up()
            {
                AddColumn(
                    "MigrationsCustomers", "head_shape",
                    c => c.Geometry(nullable: false, defaultValue: DbGeometry.FromText("POINT (6 7)")));
            }
        }

        [MigrationsTheory]
        public void Can_add_column_with_geometry_default()
        {
            WhenNotSqlCe(
                () =>
                    {
                        ResetDatabase();

                        var migrator = CreateMigrator<ShopContext_v1>();

                        migrator.Update();

                        migrator = CreateMigrator<ShopContext_v1>(new AddColumnWithGeometryDefault());

                        migrator.Update();
                    });
        }

        private class AddColumnWithCustomStoreType : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "image_col", c => c.Binary(storeType: "image"));
            }
        }

        [MigrationsTheory]
        public void Can_add_column_with_custom_store_type()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddColumnWithCustomStoreType());

            migrator.Update();

            var column = Info.Columns.SingleOrDefault(c => c.TableName == "MigrationsCustomers" && c.Name == "image_col");
            Assert.NotNull(column);
            Assert.Equal("image", column.Type);
        }

        private class AddTimestampColumn : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "version", c => c.Binary(nullable: false, timestamp: true));
            }
        }

        [MigrationsTheory]
        public void Can_add_non_nullable_timestamp_column()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddTimestampColumn());

            migrator.Update();
        }

        private class AddNonNullableColumnsWithNoDefaults : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "Binary", c => c.Binary(nullable: false));
                AddColumn("MigrationsCustomers", "Boolean", c => c.Boolean(nullable: false));
                AddColumn("MigrationsCustomers", "Byte", c => c.Byte(nullable: false));
                AddColumn("MigrationsCustomers", "DateTime", c => c.DateTime(nullable: false));
                AddColumn("MigrationsCustomers", "DateTimeOffset", c => c.DateTimeOffset(nullable: false));
                AddColumn("MigrationsCustomers", "Decimal", c => c.Decimal(nullable: false));
                AddColumn("MigrationsCustomers", "Double", c => c.Double(nullable: false));
                AddColumn("MigrationsCustomers", "Guid", c => c.Guid(nullable: false));
                AddColumn("MigrationsCustomers", "Int", c => c.Int(nullable: false));
                AddColumn("MigrationsCustomers", "Long", c => c.Long(nullable: false));
                AddColumn("MigrationsCustomers", "Short", c => c.Short(nullable: false));
                AddColumn("MigrationsCustomers", "Single", c => c.Single(nullable: false));
                AddColumn("MigrationsCustomers", "String", c => c.String(nullable: false));
                AddColumn("MigrationsCustomers", "Time", c => c.Time(nullable: false));
            }
        }

        [MigrationsTheory]
        public void Can_add_non_nullable_columns_and_valid_defaults_generated_when_existing_data_in_table()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                context.Customers.Add(
                    new MigrationsCustomer
                        {
                            HomeAddress = new MigrationsAddress(),
                            WorkAddress = new MigrationsAddress(),
                            DateOfBirth = DateTime.Now
                        });

                context.SaveChanges();
            }

            var addNonNullableColumnsWithNoDefaults = new AddNonNullableColumnsWithNoDefaults();

            WhenSqlCe(
                () =>
                    {
                        addNonNullableColumnsWithNoDefaults.GetOperations().RemoveAt(13);
                        addNonNullableColumnsWithNoDefaults.GetOperations().RemoveAt(4);
                    });

            migrator = CreateMigrator<ShopContext_v1>(addNonNullableColumnsWithNoDefaults);

            migrator.Update();
        }

        private class AddStringColumnMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.String());
            }
        }

        [MigrationsTheory]
        public void Can_add_string_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddStringColumnMigration());

            migrator.Update();

            var column = Info.Columns.SingleOrDefault(c => c.TableName == "MigrationsCustomers" && c.Name == "new_col");

            WhenSqlCe(
                () =>
                {
                    Assert.Equal("ntext", column.Type);
                    Assert.Equal(SqlCeTestDatabase.NtextLength, column.MaxLength);
                });
            WhenNotSqlCe(
                () =>
                {
                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(-1, column.MaxLength);
                });
        }

        private class AddNvarcharMaxStringColumnMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.String(storeType: "nvarchar(max)"));
            }
        }
        private class AddNvarcharMaxStringColumnCEMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.String(storeType: "ntext"));
            }
        }

        [MigrationsTheory]
        public void Can_add_nvarchar_max_string_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();
            
            WhenSqlCe(
                () =>
                {
                    migrator = CreateMigrator<ShopContext_v1>(new AddNvarcharMaxStringColumnCEMigration());
                });

            WhenNotSqlCe(
                () =>
                {
                    migrator = CreateMigrator<ShopContext_v1>(new AddNvarcharMaxStringColumnMigration());
                });

            migrator.Update();

            var column = Info.Columns.SingleOrDefault(c => c.TableName == "MigrationsCustomers" && c.Name == "new_col");

            WhenSqlCe(
                () =>
                {
                    Assert.Equal("ntext", column.Type);
                    Assert.Equal(SqlCeTestDatabase.NtextLength, column.MaxLength);
                });
            WhenNotSqlCe(
                () =>
                {
                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(-1, column.MaxLength);
                });
        }
        
        private class AddNvarcharMax64StringColumnMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.String(storeType: "nvarchar(max)", maxLength: 64));
            }
        }

        private class AddNvarcharMax64StringColumnCEMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.String(storeType: "ntext", maxLength: 64));
            }
        }

        [MigrationsTheory]
        public void Can_add_nvarchar_max_64_string_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            WhenSqlCe(
                () =>
                {
                    migrator = CreateMigrator<ShopContext_v1>(new AddNvarcharMax64StringColumnCEMigration());
                });

            WhenNotSqlCe(
                () =>
                {
                    migrator = CreateMigrator<ShopContext_v1>(new AddNvarcharMax64StringColumnMigration());
                });

            migrator.Update();

            var column = Info.Columns.SingleOrDefault(c => c.TableName == "MigrationsCustomers" && c.Name == "new_col");

            WhenSqlCe(
                () =>
                {
                    Assert.Equal("ntext", column.Type);
                    Assert.Equal(SqlCeTestDatabase.NtextLength, column.MaxLength);
                });
            WhenNotSqlCe(
                () =>
                {
                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(-1, column.MaxLength);
                });
        }

        private class Add64LengthStringColumnMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.String(maxLength: 64));
            }
        }

        [MigrationsTheory]
        public void Can_add_64_length_string_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new Add64LengthStringColumnMigration());

            migrator.Update();

            var column = Info.Columns.SingleOrDefault(c => c.TableName == "MigrationsCustomers" && c.Name == "new_col");

            Assert.Equal("nvarchar", column.Type);
            Assert.Equal(64, column.MaxLength);
        }

        private class AddNvarcharStringColumnMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.String(storeType: "nvarchar"));
            }
        }

        [MigrationsTheory]
        public void Can_add_nvarchar_string_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddNvarcharStringColumnMigration());

            migrator.Update();

            var column = Info.Columns.SingleOrDefault(c => c.TableName == "MigrationsCustomers" && c.Name == "new_col");

            Assert.Equal("nvarchar", column.Type);
            Assert.Equal(4000, column.MaxLength);
        }

        private class AddNvarchar64StringColumnMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("MigrationsCustomers", "new_col", c => c.String(storeType: "nvarchar", maxLength: 64));
            }
        }

        [MigrationsTheory]
        public void Can_add_nvarchar_64_string_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddNvarchar64StringColumnMigration());

            migrator.Update();

            var column = Info.Columns.SingleOrDefault(c => c.TableName == "MigrationsCustomers" && c.Name == "new_col");

            Assert.Equal("nvarchar", column.Type);
            Assert.Equal(64, column.MaxLength);
        }

        private class AddColumnWithAnnotationMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn(
                    "MigrationsCustomers",
                    "new_col", c => c.String(
                        storeType: "nvarchar",
                        annotations:
                            new Dictionary<string, AnnotationValues>
                            {
                                {
                                    CollationAttribute.AnnotationName,
                                    new AnnotationValues(null, new CollationAttribute("Finnish_Swedish_CS_AS"))
                                }
                            }));
            }
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)] // No collation on add column in CE
        public void Can_add_column_with_custom_annotation()
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

            migrator = CreateMigrator<ShopContext_v1>(new AddColumnWithAnnotationMigration(), sqlGenerators);

            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "new_col");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }
    }
}

#endif
