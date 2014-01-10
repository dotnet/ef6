// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlServerCe;
    using System.Linq;
    using Xunit;

    #region AutoAndGenerateScenarios

    public class AutoAndGenerateScenarios_Empty :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_Empty.V1, AutoAndGenerateScenarios_Empty.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
        }

        public class V2 : AutoAndGenerateContext_v2
        {
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(0, migrationOperations.Count());
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(0, migrationOperations.Count());
        }
    }

    #endregion

    #region TableScenarios

    public class AutoAndGenerateScenarios_AddTable :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddTable.V1, AutoAndGenerateScenarios_AddTable.V2>
    {
        public AutoAndGenerateScenarios_AddTable()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().ToTable("MigrationsStores", "my");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var createTableOperation =
                migrationOperations.OfType<CreateTableOperation>().SingleOrDefault(o => o.Name == "my.MigrationsStores");
            Assert.NotNull(createTableOperation);

            Assert.True(createTableOperation.PrimaryKey.Columns.Count == 1);
            Assert.Equal("Id", createTableOperation.PrimaryKey.Columns.Single());
            Assert.Equal(IsSqlCe ? 5 : 7, createTableOperation.Columns.Count);

            var idColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "Id");
            Assert.NotNull(idColumn);

            Assert.Equal(PrimitiveTypeKind.Int32, idColumn.Type);
            Assert.False(idColumn.IsNullable.Value);
            Assert.True(idColumn.IsIdentity);

            var nameColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "Name");
            Assert.NotNull(nameColumn);

            Assert.Equal(PrimitiveTypeKind.String, nameColumn.Type);
            Assert.Null(nameColumn.IsNullable);
            Assert.Null(nameColumn.IsFixedLength);
            Assert.Equal(IsSqlCe ? (int?)4000 : null, nameColumn.MaxLength);

            var addressCityColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "Address_City");
            Assert.NotNull(addressCityColumn);

            Assert.Equal(PrimitiveTypeKind.String, addressCityColumn.Type);
            Assert.Null(addressCityColumn.IsNullable);
            Assert.Null(addressCityColumn.IsFixedLength);
            Assert.Equal(150, addressCityColumn.MaxLength);

            var rowVersionColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "RowVersion");
            Assert.NotNull(rowVersionColumn);

            Assert.Equal(PrimitiveTypeKind.Binary, rowVersionColumn.Type);
            Assert.False(rowVersionColumn.IsNullable.Value);
            Assert.True(rowVersionColumn.IsFixedLength.Value);
            Assert.Null(rowVersionColumn.MaxLength);
            Assert.True(rowVersionColumn.IsTimestamp);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropTableOperation = migrationOperations.OfType<DropTableOperation>().SingleOrDefault(
                o => o.Name == "my.MigrationsStores");
            Assert.NotNull(dropTableOperation);
        }
    }

    public class AutoAndGenerateScenarios_AddTableWithGuidKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddTableWithGuidKey.V1, AutoAndGenerateScenarios_AddTableWithGuidKey.V2>
    {
        public AutoAndGenerateScenarios_AddTableWithGuidKey()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<WithGuidKey>();
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var createTableOperation = migrationOperations.OfType<CreateTableOperation>().SingleOrDefault(o => o.Name == "dbo.WithGuidKeys");
            Assert.NotNull(createTableOperation);

            Assert.True(createTableOperation.PrimaryKey.Columns.Count == 1);
            Assert.Equal("Id", createTableOperation.PrimaryKey.Columns.Single());
            Assert.Equal(2, createTableOperation.Columns.Count);

            var idColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "Id");
            Assert.NotNull(idColumn);
            Assert.Equal(PrimitiveTypeKind.Guid, idColumn.Type);
            Assert.False(idColumn.IsNullable.Value);
            Assert.True(idColumn.IsIdentity);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropTableOperation = migrationOperations.OfType<DropTableOperation>().SingleOrDefault(o => o.Name == "dbo.WithGuidKeys");
            Assert.NotNull(dropTableOperation);
        }
    }

    public class AutoAndGenerateScenarios_RemoveTable :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RemoveTable.V1, AutoAndGenerateScenarios_RemoveTable.V2>
    {
        public AutoAndGenerateScenarios_RemoveTable()
        {
            UpDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropTableOperation = migrationOperations.OfType<DropTableOperation>().SingleOrDefault(o => o.Name == "dbo.MigrationsStores");
            Assert.NotNull(dropTableOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var createTableOperation =
                migrationOperations.OfType<CreateTableOperation>().SingleOrDefault(o => o.Name == "dbo.MigrationsStores");
            Assert.NotNull(createTableOperation);

            Assert.Equal(IsSqlCe ? 5 : 7, createTableOperation.Columns.Count);
        }
    }

    public class AutoAndGenerateScenarios_ChangeTableSchema :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_ChangeTableSchema.V1, AutoAndGenerateScenarios_ChangeTableSchema.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().ToTable("MigrationsStores", "new");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var moveTableOperation = migrationOperations.OfType<MoveTableOperation>().SingleOrDefault(o => o.Name == "dbo.MigrationsStores");
            Assert.NotNull(moveTableOperation);

            Assert.Equal("new", moveTableOperation.NewSchema);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var moveTableOperation =
                migrationOperations.OfType<MoveTableOperation>().SingleOrDefault(o => o.Name == "new.MigrationsStores");
            Assert.NotNull(moveTableOperation);

            Assert.Equal("dbo", moveTableOperation.NewSchema);
        }
    }

    public class AutoAndGenerateScenarios_ChangeTableName :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_ChangeTableName.V1, AutoAndGenerateScenarios_ChangeTableName.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().ToTable("Renamed");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var renameTableOperation =
                migrationOperations.OfType<RenameTableOperation>().SingleOrDefault(o => o.Name == "dbo.MigrationsStores");
            Assert.NotNull(renameTableOperation);

            Assert.Equal("Renamed", renameTableOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var renameTableOperation =
                migrationOperations.OfType<RenameTableOperation>().SingleOrDefault(o => o.Name == "dbo.Renamed");
            Assert.NotNull(renameTableOperation);

            Assert.Equal("MigrationsStores", renameTableOperation.NewName);
        }
    }

    public class AutoAndGenerateScenarios_ChangeTablePrimaryKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_ChangeTablePrimaryKey.V1, AutoAndGenerateScenarios_ChangeTablePrimaryKey.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                // Compensate for convention differences
                if (this.IsSqlCe())
                {
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsRequired().HasMaxLength(4000);
                }
                else
                {
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsRequired().HasMaxLength(128);
                }

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().HasKey(s => s.Name);

                // Compensate for convention differences
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            Assert.True(migrationOperations.First() is DropPrimaryKeyOperation);
            Assert.True(migrationOperations.Last() is AddPrimaryKeyOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            Assert.True(migrationOperations.First() is DropPrimaryKeyOperation);
            Assert.True(migrationOperations.Last() is AddPrimaryKeyOperation);
        }
    }

    public class AutoAndGenerateScenarios_CreateTableWithAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_CreateTableWithAnnotations.V1, AutoAndGenerateScenarios_CreateTableWithAnnotations.V2>
    {
        public AutoAndGenerateScenarios_CreateTableWithAnnotations()
        {
            IsDownDataLoss = true;
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .HasTableAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"))
                    .HasTableAnnotation("A2", "V2")
                    .HasTableAnnotation("A1", "V1");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<CreateTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(3, operation.Annotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"),  operation.Annotations["Collation"]);
            Assert.Equal("V1", operation.Annotations["A1"]);
            Assert.Equal("V2", operation.Annotations["A2"]);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<DropTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(3, operation.RemovedAnnotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), operation.RemovedAnnotations["Collation"]);
            Assert.Equal("V1", operation.RemovedAnnotations["A1"]);
            Assert.Equal("V2", operation.RemovedAnnotations["A2"]);
        }
    }

    public class AutoAndGenerateScenarios_CreateTableWithColumnAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_CreateTableWithColumnAnnotations.V1, AutoAndGenerateScenarios_CreateTableWithColumnAnnotations.V2>
    {
        public AutoAndGenerateScenarios_CreateTableWithColumnAnnotations()
        {
            IsDownDataLoss = true;
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .Property(e => e.Address.City)
                    .HasColumnAnnotation("A2", "V2")
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A3", "V3");

                modelBuilder.Entity<MigrationsStore>()
                    .Property(e => e.Name)
                    .HasColumnAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"));

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<CreateTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(4, operation.Columns.Sum(o => o.Annotations.Count));

            Assert.Null(operation.Columns.Single(c => c.Name == "Name").Annotations["Collation"].OldValue);
            Assert.Equal(
                new CollationAttribute("Icelandic_CS_AS"),
                operation.Columns.Single(c => c.Name == "Name").Annotations["Collation"].NewValue);

            Assert.Null(operation.Columns.Single(c => c.Name == "Address_City").Annotations["A1"].OldValue);
            Assert.Equal("V1", operation.Columns.Single(c => c.Name == "Address_City").Annotations["A1"].NewValue);

            Assert.Null(operation.Columns.Single(c => c.Name == "Address_City").Annotations["A2"].OldValue);
            Assert.Equal("V2", operation.Columns.Single(c => c.Name == "Address_City").Annotations["A2"].NewValue);

            Assert.Null(operation.Columns.Single(c => c.Name == "Address_City").Annotations["A3"].OldValue);
            Assert.Equal("V3", operation.Columns.Single(c => c.Name == "Address_City").Annotations["A3"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<DropTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(4, operation.RemovedColumnAnnotations.Sum(o => o.Value.Count));

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), operation.RemovedColumnAnnotations["Name"]["Collation"]);

            Assert.Equal("V1", operation.RemovedColumnAnnotations["Address_City"]["A1"]);
            Assert.Equal("V2", operation.RemovedColumnAnnotations["Address_City"]["A2"]);
            Assert.Equal("V3", operation.RemovedColumnAnnotations["Address_City"]["A3"]);
        }
    }

    public class AutoAndGenerateScenarios_DropTableWithColumnAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_DropTableWithColumnAnnotations.V1, AutoAndGenerateScenarios_DropTableWithColumnAnnotations.V2>
    {
        public AutoAndGenerateScenarios_DropTableWithColumnAnnotations()
        {
            UpDataLoss = true;
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .Property(e => e.Address.City)
                    .HasColumnAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"));

                modelBuilder.Entity<MigrationsStore>()
                    .Property(e => e.Name)
                    .HasColumnAnnotation("A2", "V2")
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A3", "V3");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<DropTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(4, operation.RemovedColumnAnnotations.Sum(o => o.Value.Count));

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), operation.RemovedColumnAnnotations["Address_City"]["Collation"]);

            Assert.Equal("V1", operation.RemovedColumnAnnotations["Name"]["A1"]);
            Assert.Equal("V2", operation.RemovedColumnAnnotations["Name"]["A2"]);
            Assert.Equal("V3", operation.RemovedColumnAnnotations["Name"]["A3"]);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<CreateTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(4, operation.Columns.Sum(o => o.Annotations.Count));

            Assert.Null(operation.Columns.Single(c => c.Name == "Address_City").Annotations["Collation"].OldValue);
            Assert.Equal(
                new CollationAttribute("Icelandic_CS_AS"),
                operation.Columns.Single(c => c.Name == "Address_City").Annotations["Collation"].NewValue);

            Assert.Null(operation.Columns.Single(c => c.Name == "Name").Annotations["A1"].OldValue);
            Assert.Equal("V1", operation.Columns.Single(c => c.Name == "Name").Annotations["A1"].NewValue);

            Assert.Null(operation.Columns.Single(c => c.Name == "Name").Annotations["A2"].OldValue);
            Assert.Equal("V2", operation.Columns.Single(c => c.Name == "Name").Annotations["A2"].NewValue);

            Assert.Null(operation.Columns.Single(c => c.Name == "Name").Annotations["A3"].OldValue);
            Assert.Equal("V3", operation.Columns.Single(c => c.Name == "Name").Annotations["A3"].NewValue);
        }
    }

    public class AutoAndGenerateScenarios_DropTableWithAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_DropTableWithAnnotations.V1, AutoAndGenerateScenarios_DropTableWithAnnotations.V2>
    {
        public AutoAndGenerateScenarios_DropTableWithAnnotations()
        {
            UpDataLoss = true;
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .HasTableAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"))
                    .HasTableAnnotation("A2", "V2")
                    .HasTableAnnotation("A1", "V1");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<DropTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(3, operation.RemovedAnnotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), operation.RemovedAnnotations["Collation"]);
            Assert.Equal("V1", operation.RemovedAnnotations["A1"]);
            Assert.Equal("V2", operation.RemovedAnnotations["A2"]);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<CreateTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(3, operation.Annotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), operation.Annotations["Collation"]);
            Assert.Equal("V1", operation.Annotations["A1"]);
            Assert.Equal("V2", operation.Annotations["A2"]);
        }
    }

    public class AutoAndGenerateScenarios_AlterTableAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterTableAnnotations.V1, AutoAndGenerateScenarios_AlterTableAnnotations.V2>
    {
        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .HasTableAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"))
                    .HasTableAnnotation("A2", "V2")
                    .HasTableAnnotation("A1", "V1");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .HasTableAnnotation("Collation", new CollationAttribute("Finnish_Swedish_CS_AS"))
                    .HasTableAnnotation("A1", "V1")
                    .HasTableAnnotation("A3", "V3");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<AlterTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(3, operation.Annotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), operation.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Finnish_Swedish_CS_AS"), operation.Annotations["Collation"].NewValue);

            Assert.Equal("V2", operation.Annotations["A2"].OldValue);
            Assert.Null(operation.Annotations["A2"].NewValue);

            Assert.Null(operation.Annotations["A3"].OldValue);
            Assert.Equal("V3", operation.Annotations["A3"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());
            var operation = migrationOperations.OfType<AlterTableOperation>().Single(o => o.Name == "dbo.MigrationsStores");

            Assert.Equal(3, operation.Annotations.Count);

            Assert.Equal(new CollationAttribute("Finnish_Swedish_CS_AS"), operation.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), operation.Annotations["Collation"].NewValue);

            Assert.Null(operation.Annotations["A2"].OldValue);
            Assert.Equal("V2", operation.Annotations["A2"].NewValue);

            Assert.Equal("V3", operation.Annotations["A3"].OldValue);
            Assert.Null(operation.Annotations["A3"].NewValue);
        }
    }

    public class AutoAndGenerateScenarios_RenameTableWithAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RenameTableWithAnnotations.V1, AutoAndGenerateScenarios_RenameTableWithAnnotations.V2>
    {
        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .HasTableAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"))
                    .HasTableAnnotation("A2", "V2")
                    .HasTableAnnotation("A1", "V1")
                    .ToTable("EekyBear", "dbo");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .HasTableAnnotation("Collation", new CollationAttribute("Finnish_Swedish_CS_AS"))
                    .HasTableAnnotation("A1", "V1")
                    .HasTableAnnotation("A3", "V3")
                    .ToTable("MrsPandy", "dbo");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var renameOperation = (RenameTableOperation)migrationOperations.First();
            Assert.Equal("dbo.EekyBear", renameOperation.Name);

            var alterOperation = (AlterTableOperation)migrationOperations.Skip(1).First();
            Assert.Equal("dbo.MrsPandy", alterOperation.Name);

            Assert.Equal(3, alterOperation.Annotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), alterOperation.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Finnish_Swedish_CS_AS"), alterOperation.Annotations["Collation"].NewValue);

            Assert.Equal("V2", alterOperation.Annotations["A2"].OldValue);
            Assert.Null(alterOperation.Annotations["A2"].NewValue);

            Assert.Null(alterOperation.Annotations["A3"].OldValue);
            Assert.Equal("V3", alterOperation.Annotations["A3"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var alterOperation = migrationOperations.OfType<AlterTableOperation>().Single();

            Assert.Equal(3, alterOperation.Annotations.Count);

            Assert.Equal(new CollationAttribute("Finnish_Swedish_CS_AS"), alterOperation.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), alterOperation.Annotations["Collation"].NewValue);

            Assert.Null(alterOperation.Annotations["A2"].OldValue);
            Assert.Equal("V2", alterOperation.Annotations["A2"].NewValue);

            Assert.Equal("V3", alterOperation.Annotations["A3"].OldValue);
            Assert.Null(alterOperation.Annotations["A3"].NewValue);
        }
    }

    #endregion

    #region  ForeignKeyScenarios

    public class AutoAndGenerateScenarios_AddForeignKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddForeignKey.V1, AutoAndGenerateScenarios_AddForeignKey.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().Ignore(o => o.OrderLines);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var createIndexOperation =
                migrationOperations.OfType<CreateIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(createIndexOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var dropIndexOperation =
                migrationOperations.OfType<DropIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(dropIndexOperation);

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);
        }
    }

    public class AutoAndGenerateScenarios_AddPromotedForeignKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddPromotedForeignKey.V1, AutoAndGenerateScenarios_AddPromotedForeignKey.V2>
    {
        public AutoAndGenerateScenarios_AddPromotedForeignKey()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId).
                    WillCascadeOnDelete(false);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count());

            var createTableOperation =
                migrationOperations.OfType<CreateTableOperation>().SingleOrDefault(o => o.Name == "dbo.Orders");
            Assert.NotNull(createTableOperation);

            var createIndexOperation =
                migrationOperations.OfType<CreateIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(createIndexOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);

            Assert.False(addForeignKeyOperation.CascadeDelete);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count());

            var dropTableOperation = migrationOperations.OfType<DropTableOperation>().SingleOrDefault(o => o.Name == "dbo.Orders");
            Assert.NotNull(dropTableOperation);

            var dropIndexOperation =
                migrationOperations.OfType<DropIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(dropIndexOperation);

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);
        }
    }

    public class AutoAndGenerateScenarios_RemoveForeignKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RemoveForeignKey.V1, AutoAndGenerateScenarios_RemoveForeignKey.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().Ignore(o => o.OrderLines);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var dropIndexOperation =
                migrationOperations.OfType<DropIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(dropIndexOperation);

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var createIndexOperation =
                migrationOperations.OfType<CreateIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(createIndexOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);
        }
    }

    public class AutoAndGenerateScenarios_ChangeForeignKeyOnDeleteAction :
        AutoAndGenerateTestCase
            <AutoAndGenerateScenarios_ChangeForeignKeyOnDeleteAction.V1, AutoAndGenerateScenarios_ChangeForeignKeyOnDeleteAction.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId).
                    WillCascadeOnDelete(false);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);

            Assert.False(addForeignKeyOperation.CascadeDelete);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);

            Assert.True(addForeignKeyOperation.CascadeDelete);
        }
    }

    #endregion

    #region ColumnScenarios

    public class AutoAndGenerateScenarios_AddColumn :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddColumn.V1, AutoAndGenerateScenarios_AddColumn.V2>
    {
        public AutoAndGenerateScenarios_AddColumn()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);

            Assert.Null(addColumnOperation.Column.IsFixedLength);
            Assert.Null(addColumnOperation.Column.IsNullable);
            Assert.Equal(IsSqlCe ? (int?)4000 : null, addColumnOperation.Column.MaxLength);
            Assert.Null(addColumnOperation.Column.StoreType);
            Assert.Equal(PrimitiveTypeKind.String, addColumnOperation.Column.Type);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }
    }

    public class AutoAndGenerateScenarios_AddColumnNvarcharMax :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddColumnNvarcharMax.V1, AutoAndGenerateScenarios_AddColumnNvarcharMax.V2>
    {
        public AutoAndGenerateScenarios_AddColumnNvarcharMax()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                if (Database.Connection is SqlCeConnection)
                {
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.Name)
                        .HasColumnType("ntext");
                }
                else
                {
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.Name)
                        .HasColumnType("nvarchar(max)");
                }

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);

            Assert.Null(addColumnOperation.Column.IsFixedLength);
            Assert.Null(addColumnOperation.Column.IsNullable);
            Assert.Null(addColumnOperation.Column.MaxLength);
            Assert.Null(addColumnOperation.Column.StoreType);
            Assert.Equal(PrimitiveTypeKind.String, addColumnOperation.Column.Type);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }
    }
    
    public class AutoAndGenerateScenarios_AddColumnNvarcharMax64 :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddColumnNvarcharMax64.V1, AutoAndGenerateScenarios_AddColumnNvarcharMax64.V2>
    {
        public AutoAndGenerateScenarios_AddColumnNvarcharMax64()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                if (Database.Connection is SqlCeConnection)
                {
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.Name)
                        .HasColumnType("ntext")
                        .HasMaxLength(64);
                }
                else
                {
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.Name)
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(64);
                }

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);

            Assert.Null(addColumnOperation.Column.IsFixedLength);
            Assert.Null(addColumnOperation.Column.IsNullable);
            Assert.Null(addColumnOperation.Column.MaxLength);
            Assert.Null(addColumnOperation.Column.StoreType);
            Assert.Equal(PrimitiveTypeKind.String, addColumnOperation.Column.Type);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }
    }
    
    public class AutoAndGenerateScenarios_AddColumnNvarchar :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddColumnNvarchar.V1, AutoAndGenerateScenarios_AddColumnNvarchar.V2>
    {
        public AutoAndGenerateScenarios_AddColumnNvarchar()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name)
                        .HasColumnType("nvarchar");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);

            Assert.Null(addColumnOperation.Column.IsFixedLength);
            Assert.Null(addColumnOperation.Column.IsNullable);
            Assert.Equal(4000, addColumnOperation.Column.MaxLength.Value);
            Assert.Null(addColumnOperation.Column.StoreType);
            Assert.Equal(PrimitiveTypeKind.String, addColumnOperation.Column.Type);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }
    }

    public class AutoAndGenerateScenarios_AddColumnNvarcharMaxLength :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddColumnNvarcharMaxLength.V1, AutoAndGenerateScenarios_AddColumnNvarcharMaxLength.V2>
    {
        public AutoAndGenerateScenarios_AddColumnNvarcharMaxLength()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name)
                        .HasColumnType("nvarchar")
                        .IsMaxLength();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);

            Assert.Null(addColumnOperation.Column.IsFixedLength);
            Assert.Null(addColumnOperation.Column.IsNullable);
            //Assert.Null(addColumnOperation.Column.MaxLength);
            Assert.Equal(4000, addColumnOperation.Column.MaxLength);
            //Assert.Equal("nvarchar", addColumnOperation.Column.StoreType);
            Assert.Null(addColumnOperation.Column.StoreType);
            Assert.Equal(PrimitiveTypeKind.String, addColumnOperation.Column.Type);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }
    }

    public class AutoAndGenerateScenarios_AddColumnNvarchar64 :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddColumnNvarchar64.V1, AutoAndGenerateScenarios_AddColumnNvarchar64.V2>
    {
        public AutoAndGenerateScenarios_AddColumnNvarchar64()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name)
                    .HasColumnType("nvarchar")
                    .HasMaxLength(64);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);

            Assert.Null(addColumnOperation.Column.IsFixedLength);
            Assert.Null(addColumnOperation.Column.IsNullable);
            Assert.Equal(64, addColumnOperation.Column.MaxLength.Value);
            Assert.Null(addColumnOperation.Column.StoreType);
            Assert.Equal(PrimitiveTypeKind.String, addColumnOperation.Column.Type);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }
    }
    
    public class AutoAndGenerateScenarios_RemoveColumn :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RemoveColumn.V1, AutoAndGenerateScenarios_RemoveColumn.V2>
    {
        public AutoAndGenerateScenarios_RemoveColumn()
        {
            UpDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnName :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnName.V1, AutoAndGenerateScenarios_AlterColumnName.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                if (!this.IsSqlCe())
                {
                    modelBuilder.Entity<MigrationsStore>();
                }
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                if (!this.IsSqlCe())
                {
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasColumnName("Renamed");
                }
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            // Renames not supported on SQL CE
            if (IsSqlCe)
            {
                return;
            }

            Assert.Equal(1, migrationOperations.Count());

            var renameColumnOperation =
                migrationOperations.OfType<RenameColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(renameColumnOperation);

            Assert.Equal("Renamed", renameColumnOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            // Renames not supported on SQL CE
            if (IsSqlCe)
            {
                return;
            }

            Assert.Equal(1, migrationOperations.Count());

            var renameColumnOperation =
                migrationOperations.OfType<RenameColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Renamed");
            Assert.NotNull(renameColumnOperation);

            Assert.Equal("Name", renameColumnOperation.NewName);
        }
    }

    public class AutoAndGenerateScenarios_AlterSpatialColumnNames :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterSpatialColumnNames.V1, AutoAndGenerateScenarios_AlterSpatialColumnNames.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                // Spatial types not supported on SQL CE
                if (!this.IsSqlCe())
                {
                    modelBuilder.Entity<MigrationsStore>();
                }
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                // Spatial types not supported on SQL CE
                if (!this.IsSqlCe())
                {
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.Location).HasColumnName("Locomotion");
                    modelBuilder.Entity<MigrationsStore>().Property(s => s.FloorPlan).HasColumnName("PoorPlan");
                }
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            // Spatial types not supported on SQL CE
            if (IsSqlCe)
            {
                return;
            }

            Assert.Equal(2, migrationOperations.Count());

            Assert.Equal(
                "Locomotion",
                migrationOperations.OfType<RenameColumnOperation>()
                    .Single(o => o.Table == "dbo.MigrationsStores" && o.Name == "Location")
                    .NewName);

            Assert.Equal(
                "PoorPlan",
                migrationOperations.OfType<RenameColumnOperation>()
                    .Single(o => o.Table == "dbo.MigrationsStores" && o.Name == "FloorPlan")
                    .NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            // Spatial types not supported on SQL CE
            if (IsSqlCe)
            {
                return;
            }

            Assert.Equal(2, migrationOperations.Count());

            Assert.Equal(
                "Location",
                migrationOperations.OfType<RenameColumnOperation>()
                    .Single(o => o.Table == "dbo.MigrationsStores" && o.Name == "Locomotion")
                    .NewName);

            Assert.Equal(
                "FloorPlan",
                migrationOperations.OfType<RenameColumnOperation>()
                    .Single(o => o.Table == "dbo.MigrationsStores" && o.Name == "PoorPlan")
                    .NewName);
        }
    }

    public abstract class AutoAndGenerateScenarios_AlterColumnType<TContextV1, TContextV2>
        : AutoAndGenerateTestCase<TContextV1, TContextV2>
        where TContextV1 : DbContext
        where TContextV2 : DbContext
    {
        private readonly string _columnName;

        protected AutoAndGenerateScenarios_AlterColumnType(string columnName)
        {
            _columnName = columnName;

            IsDownDataLoss = true;
        }

        public abstract class BaseV1 : AutoAndGenerateContext_v1
        {
            private readonly string _columnName;

            protected BaseV1(string columnName)
            {
                _columnName = columnName;
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TypeCasts>().IgnoreAllBut("Id", _columnName);

                switch (_columnName)
                {
                    case "Decimal15ToDouble":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.Decimal15ToDouble).HasPrecision(15, 2);
                        break;

                    case "Decimal6ToDouble":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.Decimal6ToDouble).HasPrecision(6, 2);
                        break;

                    case "Decimal6ToSingle":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.Decimal6ToSingle).HasPrecision(6, 2);
                        break;

                    case "Decimal7ToSingle":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.Decimal7ToSingle).HasPrecision(7, 2);
                        break;
                }
            }
        }

        public abstract class BaseV2 : AutoAndGenerateContext_v2
        {
            private readonly string _columnName;

            protected BaseV2(string columnName)
            {
                _columnName = columnName;
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TypeCasts>().IgnoreAllBut("Id", _columnName + "_v2");

                switch (_columnName)
                {
                    case "DoubleToDecimal16":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.DoubleToDecimal16_v2).HasPrecision(16, 2);
                        break;

                    case "SingleToDecimal11":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.SingleToDecimal11_v2).HasPrecision(11, 2);
                        break;

                    case "SingleToDecimal16":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.SingleToDecimal16_v2).HasPrecision(16, 2);
                        break;
                }
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.TypeCasts" && o.Column.Name == _columnName);
            Assert.NotNull(alterColumnOperation);

            PrimitiveTypeKind toType;
            var toTypeName = _columnName.Substring(_columnName.IndexOf("To") + 2);

            byte precision;
            var isDecimal = IsDecimal(ref toTypeName, out precision);

            Assert.True(Enum.TryParse(toTypeName, out toType));

            Assert.Equal(toType, alterColumnOperation.Column.Type);

            if (isDecimal)
            {
                Assert.Equal(precision, alterColumnOperation.Column.Precision);
            }
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.TypeCasts" && o.Column.Name == _columnName);
            Assert.NotNull(alterColumnOperation);
            PrimitiveTypeKind fromType;
            var fromTypeName = _columnName.Substring(0, _columnName.IndexOf("To"));

            byte precision;
            var isDecimal = IsDecimal(ref fromTypeName, out precision);

            Assert.True(Enum.TryParse(fromTypeName, out fromType));

            Assert.Equal(fromType, alterColumnOperation.Column.Type);

            if (isDecimal)
            {
                Assert.Equal(precision, alterColumnOperation.Column.Precision);
            }
        }

        private static bool IsDecimal(ref string typeName, out byte precision)
        {
            if (!typeName.StartsWith("Decimal"))
            {
                precision = 0;

                return false;
            }

            Assert.True(Byte.TryParse(typeName.Substring(7), out precision));
            typeName = "Decimal";

            return true;
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Decimal15ToDouble
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Decimal15ToDouble.V1,
                AutoAndGenerateScenarios_AlterColumnType_Decimal15ToDouble.V2>
    {
        private const string ColumnName = "Decimal15ToDouble";

        public AutoAndGenerateScenarios_AlterColumnType_Decimal15ToDouble()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal16
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal16.V1,
                AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal16.V2>
    {
        private const string ColumnName = "SingleToDecimal16";

        public AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal16()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_SingleToDouble
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_SingleToDouble.V1, AutoAndGenerateScenarios_AlterColumnType_SingleToDouble.V2>
    {
        private const string ColumnName = "SingleToDouble";

        public AutoAndGenerateScenarios_AlterColumnType_SingleToDouble()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal11
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal11.V1,
                AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal11.V2>
    {
        private const string ColumnName = "SingleToDecimal11";

        public AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal11()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Decimal6ToDouble
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Decimal6ToDouble.V1, AutoAndGenerateScenarios_AlterColumnType_Decimal6ToDouble.V2>
    {
        private const string ColumnName = "Decimal6ToDouble";

        public AutoAndGenerateScenarios_AlterColumnType_Decimal6ToDouble()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Int32ToInt64
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Int32ToInt64.V1, AutoAndGenerateScenarios_AlterColumnType_Int32ToInt64.V2>
    {
        private const string ColumnName = "Int32ToInt64";

        public AutoAndGenerateScenarios_AlterColumnType_Int32ToInt64()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Int16ToInt64
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Int16ToInt64.V1, AutoAndGenerateScenarios_AlterColumnType_Int16ToInt64.V2>
    {
        private const string ColumnName = "Int16ToInt64";

        public AutoAndGenerateScenarios_AlterColumnType_Int16ToInt64()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Int16ToInt32
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Int16ToInt32.V1, AutoAndGenerateScenarios_AlterColumnType_Int16ToInt32.V2>
    {
        private const string ColumnName = "Int16ToInt32";

        public AutoAndGenerateScenarios_AlterColumnType_Int16ToInt32()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_ByteToInt64
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_ByteToInt64.V1, AutoAndGenerateScenarios_AlterColumnType_ByteToInt64.V2>
    {
        private const string ColumnName = "ByteToInt64";

        public AutoAndGenerateScenarios_AlterColumnType_ByteToInt64()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_ByteToInt32
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_ByteToInt32.V1, AutoAndGenerateScenarios_AlterColumnType_ByteToInt32.V2>
    {
        private const string ColumnName = "ByteToInt32";

        public AutoAndGenerateScenarios_AlterColumnType_ByteToInt32()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_ByteToInt16
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_ByteToInt16.V1, AutoAndGenerateScenarios_AlterColumnType_ByteToInt16.V2>
    {
        private const string ColumnName = "ByteToInt16";

        public AutoAndGenerateScenarios_AlterColumnType_ByteToInt16()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnFixedLength :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnFixedLength.V1, AutoAndGenerateScenarios_AlterColumnFixedLength.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnFixedLength()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(64);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsFixedLength();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.True(alterColumnOperation.Column.IsFixedLength.Value);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Null(alterColumnOperation.Column.IsFixedLength);
        }
    }

    public abstract class AutoAndGenerateScenarios_AlterColumnMaxLength<TContextV1, TContextV2>
        : AutoAndGenerateTestCase<TContextV1, TContextV2>
        where TContextV1 : DbContext
        where TContextV2 : DbContext
    {
        private readonly int? _newMaxLength;

        protected AutoAndGenerateScenarios_AlterColumnMaxLength(int? newMaxLength)
        {
            _newMaxLength = newMaxLength;

            IsDownDataLoss = true;
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(_newMaxLength, alterColumnOperation.Column.MaxLength);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(256, alterColumnOperation.Column.MaxLength);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumn256_Max
        :
            AutoAndGenerateScenarios_AlterColumnMaxLength
                <AutoAndGenerateScenarios_AlterColumn256_Max.V1, AutoAndGenerateScenarios_AlterColumn256_Max.V2>
    {
        public AutoAndGenerateScenarios_AlterColumn256_Max()
            : base(null)
        {
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(256);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(null);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnMaxLength_Max
        :
            AutoAndGenerateScenarios_AlterColumnMaxLength
                <AutoAndGenerateScenarios_AlterColumnMaxLength_Max.V1, AutoAndGenerateScenarios_AlterColumnMaxLength_Max.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnMaxLength_Max()
            : base(null)
        {
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name)
                    .HasColumnType("nvarchar").IsMaxLength();

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Null(alterColumnOperation.Column.MaxLength);
            Assert.Null(alterColumnOperation.Column.StoreType);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(4000, alterColumnOperation.Column.MaxLength);
            Assert.Null(alterColumnOperation.Column.StoreType);
        }
    }
    
    public class AutoAndGenerateScenarios_AlterColumnMaxLength_512
        :
            AutoAndGenerateScenarios_AlterColumnMaxLength
                <AutoAndGenerateScenarios_AlterColumnMaxLength_512.V1, AutoAndGenerateScenarios_AlterColumnMaxLength_512.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnMaxLength_512()
            : base(512)
        {
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(256);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(512);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnNullable :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnNullable.V1, AutoAndGenerateScenarios_AlterColumnNullable.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnNullable()
        {
            UpDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsRequired();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.False(alterColumnOperation.Column.IsNullable.Value);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Null(alterColumnOperation.Column.IsNullable);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnPrecision :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnPrecision.V1, AutoAndGenerateScenarios_AlterColumnPrecision.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnPrecision()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().Property(ol => ol.Price).HasPrecision(9, 0);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().Property(ol => ol.Price).HasPrecision(18, 0);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal((byte?)18, alterColumnOperation.Column.Precision);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal((byte?)9, alterColumnOperation.Column.Precision);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnScale :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnScale.V1, AutoAndGenerateScenarios_AlterColumnScale.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnScale()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().Property(ol => ol.Price).HasPrecision(18, 0);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().Property(ol => ol.Price).HasPrecision(18, 2);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal((byte?)2, alterColumnOperation.Column.Scale);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal((byte?)0, alterColumnOperation.Column.Scale);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnUnicode :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnUnicode.V1, AutoAndGenerateScenarios_AlterColumnUnicode.V2>
    {
        public override void Init(DatabaseProvider provider, ProgrammingLanguage language)
        {
            base.Init(provider, language);

            if (!IsSqlCe)
            {
                // SQL CE columns are always Unicode
                IsDownDataLoss = true;
                UpDataLoss = true;
            }
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsUnicode(false);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            // SQL CE columns are always Unicode
            if (IsSqlCe)
            {
                Assert.Empty(migrationOperations);
                return;
            }

            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.False(alterColumnOperation.Column.IsUnicode.Value);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            // SQL CE columns are always Unicode
            if (IsSqlCe)
            {
                Assert.Empty(migrationOperations);
                return;
            }

            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Null(alterColumnOperation.Column.IsUnicode);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnChangedAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnChangedAnnotations.V1, AutoAndGenerateScenarios_AlterColumnChangedAnnotations.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Price)
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A2", "V2A")
                    .HasColumnAnnotation("A3", "V3")
                    .HasColumnAnnotation("A4", "V4A");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Price)
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A2", "V2B")
                    .HasColumnAnnotation("A3", "V3")
                    .HasColumnAnnotation("A4", "V4B");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(2, alterColumnOperation.Column.Annotations.Count);
            Assert.Equal("V2A", alterColumnOperation.Column.Annotations["A2"].OldValue);
            Assert.Equal("V2B", alterColumnOperation.Column.Annotations["A2"].NewValue);
            Assert.Equal("V4A", alterColumnOperation.Column.Annotations["A4"].OldValue);
            Assert.Equal("V4B", alterColumnOperation.Column.Annotations["A4"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(2, alterColumnOperation.Column.Annotations.Count);

            Assert.Equal("V2B", alterColumnOperation.Column.Annotations["A2"].OldValue);
            Assert.Equal("V2A", alterColumnOperation.Column.Annotations["A2"].NewValue);

            Assert.Equal("V4B", alterColumnOperation.Column.Annotations["A4"].OldValue);
            Assert.Equal("V4A", alterColumnOperation.Column.Annotations["A4"].NewValue);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnAddedAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnAddedAnnotations.V1, AutoAndGenerateScenarios_AlterColumnAddedAnnotations.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>();
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Sku)
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A2", "V2")
                    .HasColumnAnnotation("A3", "V3");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Sku");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(3, alterColumnOperation.Column.Annotations.Count);

            Assert.Null(alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Equal("V1", alterColumnOperation.Column.Annotations["A1"].NewValue);

            Assert.Null(alterColumnOperation.Column.Annotations["A2"].OldValue);
            Assert.Equal("V2", alterColumnOperation.Column.Annotations["A2"].NewValue);

            Assert.Null(alterColumnOperation.Column.Annotations["A3"].OldValue);
            Assert.Equal("V3", alterColumnOperation.Column.Annotations["A3"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Sku");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(3, alterColumnOperation.Column.Annotations.Count);

            Assert.Equal("V1", alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A1"].NewValue);

            Assert.Equal("V2", alterColumnOperation.Column.Annotations["A2"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A2"].NewValue);

            Assert.Equal("V3", alterColumnOperation.Column.Annotations["A3"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A3"].NewValue);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnRemovedAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnRemovedAnnotations.V1, AutoAndGenerateScenarios_AlterColumnRemovedAnnotations.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Sku)
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A2", "V2")
                    .HasColumnAnnotation("A3", "V3");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>();
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Sku");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(3, alterColumnOperation.Column.Annotations.Count);

            Assert.Equal("V1", alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A1"].NewValue);

            Assert.Equal("V2", alterColumnOperation.Column.Annotations["A2"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A2"].NewValue);

            Assert.Equal("V3", alterColumnOperation.Column.Annotations["A3"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A3"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Sku");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(3, alterColumnOperation.Column.Annotations.Count);

            Assert.Null(alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Equal("V1", alterColumnOperation.Column.Annotations["A1"].NewValue);

            Assert.Null(alterColumnOperation.Column.Annotations["A2"].OldValue);
            Assert.Equal("V2", alterColumnOperation.Column.Annotations["A2"].NewValue);

            Assert.Null(alterColumnOperation.Column.Annotations["A3"].OldValue);
            Assert.Equal("V3", alterColumnOperation.Column.Annotations["A3"].NewValue);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnEverythingAnnotations :
    AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnEverythingAnnotations.V1, AutoAndGenerateScenarios_AlterColumnEverythingAnnotations.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Price)
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A2", "V2")
                    .HasColumnAnnotation("A3", "V3A")
                    .HasColumnAnnotation("A4", "V4A");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Price)
                    .HasColumnAnnotation("A3", "V3B")
                    .HasColumnAnnotation("A4", "V4B")
                    .HasColumnAnnotation("A5", "V5")
                    .HasColumnAnnotation("A6", "V6");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(6, alterColumnOperation.Column.Annotations.Count);

            Assert.Equal("V3A", alterColumnOperation.Column.Annotations["A3"].OldValue);
            Assert.Equal("V3B", alterColumnOperation.Column.Annotations["A3"].NewValue);

            Assert.Equal("V4A", alterColumnOperation.Column.Annotations["A4"].OldValue);
            Assert.Equal("V4B", alterColumnOperation.Column.Annotations["A4"].NewValue);

            Assert.Null(alterColumnOperation.Column.Annotations["A5"].OldValue);
            Assert.Equal("V5", alterColumnOperation.Column.Annotations["A5"].NewValue);

            Assert.Null(alterColumnOperation.Column.Annotations["A6"].OldValue);
            Assert.Equal("V6", alterColumnOperation.Column.Annotations["A6"].NewValue);

            Assert.Equal("V1", alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A1"].NewValue);

            Assert.Equal("V2", alterColumnOperation.Column.Annotations["A2"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A2"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(6, alterColumnOperation.Column.Annotations.Count);

            Assert.Equal("V3B", alterColumnOperation.Column.Annotations["A3"].OldValue);
            Assert.Equal("V3A", alterColumnOperation.Column.Annotations["A3"].NewValue);

            Assert.Equal("V4B", alterColumnOperation.Column.Annotations["A4"].OldValue);
            Assert.Equal("V4A", alterColumnOperation.Column.Annotations["A4"].NewValue);

            Assert.Equal("V5", alterColumnOperation.Column.Annotations["A5"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A5"].NewValue);

            Assert.Equal("V6", alterColumnOperation.Column.Annotations["A6"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A6"].NewValue);

            Assert.Null(alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Equal("V1", alterColumnOperation.Column.Annotations["A1"].NewValue);

            Assert.Null(alterColumnOperation.Column.Annotations["A2"].OldValue);
            Assert.Equal("V2", alterColumnOperation.Column.Annotations["A2"].NewValue);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnCustomAnnotation :
    AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnCustomAnnotation.V1, AutoAndGenerateScenarios_AlterColumnCustomAnnotation.V2>
    {
        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .Property(ol => ol.Address.City)
                    .HasColumnAnnotation("Collation", new CollationAttribute("Finnish_Swedish_CS_AS"));

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .Property(ol => ol.Address.City)
                    .HasColumnAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"));

                modelBuilder.Entity<MigrationsStore>()
                    .Property(ol => ol.Name)
                    .HasColumnAnnotation("Collation", new CollationAttribute("Danish_Norwegian_CS_AS"));

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var cityOperation =
                migrationOperations.OfType<AlterColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Address_City");

            Assert.Equal(1, cityOperation.Column.Annotations.Count);

            Assert.Equal(new CollationAttribute("Finnish_Swedish_CS_AS"), cityOperation.Column.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), cityOperation.Column.Annotations["Collation"].NewValue);

            var nameOperation =
                migrationOperations.OfType<AlterColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");

            Assert.Equal(1, nameOperation.Column.Annotations.Count);

            Assert.Null(nameOperation.Column.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Danish_Norwegian_CS_AS"), nameOperation.Column.Annotations["Collation"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var cityOperation =
                migrationOperations.OfType<AlterColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Address_City");

            Assert.Equal(1, cityOperation.Column.Annotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), cityOperation.Column.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Finnish_Swedish_CS_AS"), cityOperation.Column.Annotations["Collation"].NewValue);

            var nameOperation =
                migrationOperations.OfType<AlterColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");

            Assert.Equal(1, nameOperation.Column.Annotations.Count);

            Assert.Equal(new CollationAttribute("Danish_Norwegian_CS_AS"), nameOperation.Column.Annotations["Collation"].OldValue);
            Assert.Null(nameOperation.Column.Annotations["Collation"].NewValue);
        }
    }

    public class AutoAndGenerateScenarios_AddColumnWithAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddColumnWithAnnotations.V1, AutoAndGenerateScenarios_AddColumnWithAnnotations.V2>
    {
        public AutoAndGenerateScenarios_AddColumnWithAnnotations()
        {
            IsDownDataLoss = true;
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .Ignore(e => e.Address);

                modelBuilder.Entity<MigrationsStore>()
                    .Ignore(e => e.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .Property(e => e.Address.City)
                    .HasColumnAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"));

                modelBuilder.Entity<MigrationsStore>()
                    .Property(e => e.Name)
                    .HasColumnAnnotation("A2", "V2")
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A3", "V3");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var cityOperation =
                migrationOperations.OfType<AddColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Address_City");

            Assert.Equal(1, cityOperation.Column.Annotations.Count);

            Assert.Null(cityOperation.Column.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), cityOperation.Column.Annotations["Collation"].NewValue);

            var nameOperation =
                migrationOperations.OfType<AddColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");

            Assert.Equal(3, nameOperation.Column.Annotations.Count);

            Assert.Null(nameOperation.Column.Annotations["A1"].OldValue);
            Assert.Equal("V1", nameOperation.Column.Annotations["A1"].NewValue);

            Assert.Null(nameOperation.Column.Annotations["A2"].OldValue);
            Assert.Equal("V2", nameOperation.Column.Annotations["A2"].NewValue);

            Assert.Null(nameOperation.Column.Annotations["A3"].OldValue);
            Assert.Equal("V3", nameOperation.Column.Annotations["A3"].NewValue);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var cityOperation =
                migrationOperations.OfType<DropColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Address_City");

            Assert.Equal(1, cityOperation.RemovedAnnotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), cityOperation.RemovedAnnotations["Collation"]);
            
            var nameOperation =
                migrationOperations.OfType<DropColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");

            Assert.Equal(3, nameOperation.RemovedAnnotations.Count);

            Assert.Equal("V1", nameOperation.RemovedAnnotations["A1"]);
            Assert.Equal("V2", nameOperation.RemovedAnnotations["A2"]);
            Assert.Equal("V3", nameOperation.RemovedAnnotations["A3"]);
        }
    }

    public class AutoAndGenerateScenarios_DropColumnWithAnnotations :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_DropColumnWithAnnotations.V1, AutoAndGenerateScenarios_DropColumnWithAnnotations.V2>
    {
        public AutoAndGenerateScenarios_DropColumnWithAnnotations()
        {
            UpDataLoss = true;
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .Property(e => e.Address.City)
                    .HasColumnAnnotation("Collation", new CollationAttribute("Icelandic_CS_AS"));

                modelBuilder.Entity<MigrationsStore>()
                    .Property(e => e.Name)
                    .HasColumnAnnotation("A2", "V2")
                    .HasColumnAnnotation("A1", "V1")
                    .HasColumnAnnotation("A3", "V3");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>()
                    .Ignore(e => e.Address);

                modelBuilder.Entity<MigrationsStore>()
                    .Ignore(e => e.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var cityOperation =
                migrationOperations.OfType<DropColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Address_City");

            Assert.Equal(1, cityOperation.RemovedAnnotations.Count);

            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), cityOperation.RemovedAnnotations["Collation"]);

            var nameOperation =
                migrationOperations.OfType<DropColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");

            Assert.Equal(3, nameOperation.RemovedAnnotations.Count);

            Assert.Equal("V1", nameOperation.RemovedAnnotations["A1"]);
            Assert.Equal("V2", nameOperation.RemovedAnnotations["A2"]);
            Assert.Equal("V3", nameOperation.RemovedAnnotations["A3"]);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var cityOperation =
                migrationOperations.OfType<AddColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Address_City");

            Assert.Equal(1, cityOperation.Column.Annotations.Count);

            Assert.Null(cityOperation.Column.Annotations["Collation"].OldValue);
            Assert.Equal(new CollationAttribute("Icelandic_CS_AS"), cityOperation.Column.Annotations["Collation"].NewValue);

            var nameOperation =
                migrationOperations.OfType<AddColumnOperation>().Single(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");

            Assert.Equal(3, nameOperation.Column.Annotations.Count);

            Assert.Null(nameOperation.Column.Annotations["A1"].OldValue);
            Assert.Equal("V1", nameOperation.Column.Annotations["A1"].NewValue);

            Assert.Null(nameOperation.Column.Annotations["A2"].OldValue);
            Assert.Equal("V2", nameOperation.Column.Annotations["A2"].NewValue);

            Assert.Null(nameOperation.Column.Annotations["A3"].OldValue);
            Assert.Equal("V3", nameOperation.Column.Annotations["A3"].NewValue);
        }
    }

    #endregion

    #region IndexScenarios

    public class AutoAndGenerateScenarios_AddIndex :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddIndex.V1, AutoAndGenerateScenarios_AddIndex.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("MyIndex") { IsUnique = true }));
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var operation = migrationOperations.OfType<CreateIndexOperation>().Single();

            Assert.Equal("dbo.OrderLines", operation.Table);
            Assert.Equal("MyIndex", operation.Name);
            Assert.False(operation.IsClustered);
            Assert.True(operation.IsUnique);
            Assert.Equal(new List<string> { "Quantity" }, operation.Columns);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var operation = migrationOperations.OfType<DropIndexOperation>().Single();

            Assert.Equal("dbo.OrderLines", operation.Table);
            Assert.Equal("MyIndex", operation.Name);
        }
    }

    public class AutoAndGenerateScenarios_ChangeIndex :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_ChangeIndex.V1, AutoAndGenerateScenarios_ChangeIndex.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("MyIndex")));
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("MyIndex") { IsUnique = true }));
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var dropOperation = (DropIndexOperation)migrationOperations.First();

            Assert.Equal("dbo.OrderLines", dropOperation.Table);
            Assert.Equal("MyIndex", dropOperation.Name);
            
            var createOperation = (CreateIndexOperation)migrationOperations.Skip(1).First();

            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.Equal("MyIndex", createOperation.Name);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity" }, createOperation.Columns);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var dropOperation = (DropIndexOperation)migrationOperations.First();

            Assert.Equal("dbo.OrderLines", dropOperation.Table);
            Assert.Equal("MyIndex", dropOperation.Name);

            var createOperation = (CreateIndexOperation)migrationOperations.Skip(1).First();

            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.Equal("MyIndex", createOperation.Name);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity" }, createOperation.Columns);
        }
    }

    public class AutoAndGenerateScenarios_DropIndex :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_DropIndex.V1, AutoAndGenerateScenarios_DropIndex.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("MyIndex") { IsUnique = true }));
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var operation = migrationOperations.OfType<DropIndexOperation>().Single();

            Assert.Equal("dbo.OrderLines", operation.Table);
            Assert.Equal("MyIndex", operation.Name);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var operation = migrationOperations.OfType<CreateIndexOperation>().Single();

            Assert.Equal("dbo.OrderLines", operation.Table);
            Assert.Equal("MyIndex", operation.Name);
            Assert.False(operation.IsClustered);
            Assert.True(operation.IsUnique);
            Assert.Equal(new List<string> { "Quantity" }, operation.Columns);
        }
    }

    public class AutoAndGenerateScenarios_LotsOfIndexStuff :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_LotsOfIndexStuff.V1, AutoAndGenerateScenarios_LotsOfIndexStuff.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity)
                    .HasColumnAnnotation(
                        IndexAnnotation.AnnotationName,
                        new IndexAnnotation(
                            new[]
                            {
                                new IndexAttribute("MyIndex"),
                                new IndexAttribute("CompositeIndex3") { IsUnique = true, Order = 3 },
                                new IndexAttribute("CompositeIndex2") { Order = 2 },
                                new IndexAttribute("CompositeIndex4") { Order = 1 },
                                new IndexAttribute("CompositeIndex5") { Order = 1 },
                                new IndexAttribute("CompositeIndex6") { Order = 1 }
                            }));

                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Sku)
                    .HasColumnAnnotation(
                        IndexAnnotation.AnnotationName,
                        new IndexAnnotation(
                            new[]
                            {
                                new IndexAttribute("SkuIndex") { IsUnique = true },
                                new IndexAttribute("AnotherSkuIndex"),
                                new IndexAttribute("CompositeIndex3") { Order = 2 },
                                new IndexAttribute("CompositeIndex2") { IsUnique = true, Order = 1 },
                                new IndexAttribute("CompositeIndex4") { Order = 2 },
                                new IndexAttribute("CompositeIndex5") { Order = 2 },
                                new IndexAttribute("CompositeIndex6") { Order = 2 }
                            }));

                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.OrderId)
                    .HasColumnAnnotation(
                        IndexAnnotation.AnnotationName,
                        new IndexAnnotation(
                            new[]
                            {
                                new IndexAttribute("NewFKIndex") { IsUnique = true },
                                new IndexAttribute("AnotherFKIndex"),
                                new IndexAttribute("CompositeIndex2") { IsUnique = true, Order = 3 },
                                new IndexAttribute("CompositeIndex4") { Order = 3 },
                                new IndexAttribute("CompositeIndex5") { Order = 3 },
                                new IndexAttribute("CompositeIndex6") { Order = 3 }
                            }));
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity)
                    .HasColumnAnnotation(
                        IndexAnnotation.AnnotationName,
                        new IndexAnnotation(
                            new[]
                            {
                                new IndexAttribute("MyIndex"),
                                new IndexAttribute("MySecondIndex") { IsUnique = true },
                                new IndexAttribute("CompositeIndex3") { IsUnique = true, Order = 3 },
                                new IndexAttribute("CompositeIndex1") { IsUnique = true, Order = 1 },
                                new IndexAttribute("CompositeIndex4") { Order = 1 },
                                new IndexAttribute("CompositeIndex5") { Order = 1 },
                                new IndexAttribute("CompositeIndex6") { Order = 3 }
                            }));

                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Sku)
                    .HasColumnAnnotation(
                        IndexAnnotation.AnnotationName,
                        new IndexAnnotation(
                            new[]
                            {
                                new IndexAttribute("SkuIndex") { IsUnique = false },
                                new IndexAttribute("AnotherSkuIndex"),
                                new IndexAttribute("CompositeIndex1") { IsUnique = true, Order = 3 },
                                new IndexAttribute("CompositeIndex4") { IsUnique = true, Order = 2 },
                                new IndexAttribute("CompositeIndex5") { Order = 2 },
                                new IndexAttribute("CompositeIndex6") { Order = 2 }
                            }));

                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.OrderId)
                    .HasColumnAnnotation(
                        IndexAnnotation.AnnotationName,
                        new IndexAnnotation(
                            new[]
                            {
                                new IndexAttribute("AnotherFKIndex"),
                                new IndexAttribute("CompositeIndex1") { IsUnique = true, Order = 2 },
                                new IndexAttribute("CompositeIndex3") { IsUnique = true, Order = 1 },
                                new IndexAttribute("CompositeIndex4") { Order = 3 },
                                new IndexAttribute("CompositeIndex5") { Order = 3 },
                                new IndexAttribute("CompositeIndex6") { Order = 1 }
                            }));
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            var operations = migrationOperations.ToArray();

            Assert.Equal(12, operations.Length);

            for (var i = 0; i < 6; i++)
            {
                Assert.IsType<DropIndexOperation>(operations[i]);
            }

            for (var i = 6; i < 12; i++)
            {
                Assert.IsType<CreateIndexOperation>(operations[i]);
            }

            var dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "SkuIndex");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "NewFKIndex");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "CompositeIndex2");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "CompositeIndex3");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "CompositeIndex4");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "CompositeIndex6");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            var createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "SkuIndex");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Sku" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "MySecondIndex");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "CompositeIndex1");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity", "OrderId", "Sku" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "CompositeIndex3");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "OrderId", "Quantity" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "CompositeIndex4");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity", "Sku", "OrderId" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "CompositeIndex6");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "OrderId", "Sku", "Quantity" }, createOperation.Columns);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            var operations = migrationOperations.ToArray();

            Assert.Equal(12, operations.Length);

            for (var i = 0; i < 6; i++)
            {
                Assert.IsType<DropIndexOperation>(operations[i]);
            }

            for (var i = 6; i < 12; i++)
            {
                Assert.IsType<CreateIndexOperation>(operations[i]);
            }

            var dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "SkuIndex");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "MySecondIndex");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "CompositeIndex1");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "CompositeIndex3");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "CompositeIndex4");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "CompositeIndex6");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            var createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "SkuIndex");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Sku" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "NewFKIndex");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "OrderId" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "CompositeIndex2");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Sku", "Quantity", "OrderId" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "CompositeIndex3");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Sku", "Quantity" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "CompositeIndex4");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity", "Sku", "OrderId" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "CompositeIndex6");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity", "Sku", "OrderId" }, createOperation.Columns);
        }
    }

    public class AutoAndGenerateScenarios_ImplicitIndexChanges :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_ImplicitIndexChanges.V1, AutoAndGenerateScenarios_ImplicitIndexChanges.V2>
    {
        public override void Init(DatabaseProvider provider, ProgrammingLanguage language)
        {
            base.Init(provider, language);

            // Renames not supported on SQL CE
            if (!IsSqlCe)
            {
                IsDownDataLoss = true;
                UpDataLoss = true;
            }
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                // Renames not supported on SQL CE
                if (!this.IsSqlCe())
                {
                    modelBuilder.Entity<OrderLine>()
                        .Ignore(ol => ol.Quantity);
                
                    modelBuilder.Entity<OrderLine>()
                        .Property(ol => ol.Price)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("PriceIndex")));

                    modelBuilder.Entity<OrderLine>()
                        .Property(ol => ol.Sku)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("SkuIndex")));

                    modelBuilder.Entity<WithGuidKey>()
                        .Property(ol => ol.Id)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("GuidIndex")));

                    modelBuilder.Entity<MigrationsProduct>()
                        .Property(p => p.ProductId)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("ProductIdIndex")));

                    modelBuilder.Entity<Order>()
                        .Property(p => p.OrderId)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("OrderIdIndex")));

                    modelBuilder.Ignore<MigrationsStore>();

                    modelBuilder.Entity<MigrationsProduct>();
                }
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                // Renames not supported on SQL CE
                if (!this.IsSqlCe())
                {
                    modelBuilder.Entity<OrderLine>()
                        .Property(ol => ol.Quantity)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("QuantityIndex")));
            
                    modelBuilder.Entity<OrderLine>()
                        .Ignore(ol => ol.Price);
            
                    modelBuilder.Entity<OrderLine>()
                        .Property(ol => ol.Sku)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("SkuIndex")))
                        .HasColumnName("NuSku");
            
                    modelBuilder.Entity<WithGuidKey>()
                        .ToTable("WithGooieKey")
                        .Property(ol => ol.Id)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("GuidIndex")));
            
                    modelBuilder.Entity<MigrationsStore>()
                        .Property(s => s.Id)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("IdIndex")));
            
                    modelBuilder.Ignore<MigrationsProduct>();
            
                    modelBuilder.Entity<Order>()
                        .ToTable("Oeuvres")
                        .Property(p => p.OrderId)
                        .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("OrderIdIndex")))
                        .HasColumnName("OeuvresId");
                }
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            // Renames not supported on SQL CE
            if (IsSqlCe)
            {
                return;
            }

            var operations = migrationOperations.ToArray();

            Assert.Equal(2, operations.OfType<DropIndexOperation>().Count());
            Assert.Equal(2, operations.OfType<CreateIndexOperation>().Count());

            var dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "PriceIndex");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "ProductIdIndex");
            Assert.Equal("dbo.MigrationsProducts", dropOperation.Table);

            var createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "QuantityIndex");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "IdIndex");
            Assert.Equal("dbo.MigrationsStores", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Id" }, createOperation.Columns);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            // Renames not supported on SQL CE
            if (IsSqlCe)
            {
                return;
            }

            var operations = migrationOperations.ToArray();

            Assert.Equal(2, operations.OfType<DropIndexOperation>().Count());
            Assert.Equal(2, operations.OfType<CreateIndexOperation>().Count());

            var dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "QuantityIndex");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "IdIndex");
            Assert.Equal("dbo.MigrationsStores", dropOperation.Table);

            var createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "PriceIndex");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Price" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "ProductIdIndex");
            Assert.Equal("dbo.MigrationsProducts", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "ProductId" }, createOperation.Columns);
        }
    }

    public class AutoAndGenerateScenarios_DefaultNameIndexes :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_DefaultNameIndexes.V1, AutoAndGenerateScenarios_DefaultNameIndexes.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity);

                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Price);

                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Sku);

                modelBuilder.Entity<WithGuidKey>()
                    .Property(ol => ol.Id);

                modelBuilder.Entity<MigrationsStore>()
                    .Property(s => s.Id)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

                modelBuilder.Entity<MigrationsProduct>()
                    .Property(p => p.ProductId)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Quantity)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Price)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

                modelBuilder.Entity<OrderLine>()
                    .Property(ol => ol.Sku)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("SkuedIndex")));

                modelBuilder.Entity<WithGuidKey>()
                    .Property(ol => ol.Id)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

                modelBuilder.Entity<MigrationsStore>()
                    .Property(s => s.Id)
                    .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute { IsUnique = true }));

                modelBuilder.Entity<MigrationsProduct>()
                    .Property(p => p.ProductId);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            var operations = migrationOperations.ToArray();

            Assert.Equal(7, operations.Length);
            Assert.Equal(2, operations.OfType<DropIndexOperation>().Count());
            Assert.Equal(5, operations.OfType<CreateIndexOperation>().Count());

            var dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "IX_Id");
            Assert.Equal("dbo.MigrationsStores", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "IX_ProductId");
            Assert.Equal("dbo.MigrationsProducts", dropOperation.Table);

            var createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "IX_Quantity");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Quantity" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "IX_Price");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Price" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "SkuedIndex");
            Assert.Equal("dbo.OrderLines", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Sku" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>()
                .Single(o => o.Name == "IX_Id" && o.Table == "dbo.WithGuidKeys");
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Id" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>()
                .Single(o => o.Name == "IX_Id" && o.Table == "dbo.MigrationsStores");
            Assert.False(createOperation.IsClustered);
            Assert.True(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Id" }, createOperation.Columns);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            var operations = migrationOperations.ToArray();

            Assert.Equal(7, operations.Length);
            Assert.Equal(5, operations.OfType<DropIndexOperation>().Count());
            Assert.Equal(2, operations.OfType<CreateIndexOperation>().Count());

            var dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "IX_Quantity");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "IX_Price");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "SkuedIndex");
            Assert.Equal("dbo.OrderLines", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "IX_Id" && o.Table == "dbo.WithGuidKeys");
            Assert.Equal("dbo.WithGuidKeys", dropOperation.Table);

            dropOperation = operations.OfType<DropIndexOperation>().Single(o => o.Name == "IX_Id" && o.Table == "dbo.MigrationsStores");
            Assert.Equal("dbo.MigrationsStores", dropOperation.Table);

            var createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "IX_Id");
            Assert.Equal("dbo.MigrationsStores", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "Id" }, createOperation.Columns);

            createOperation = operations.OfType<CreateIndexOperation>().Single(o => o.Name == "IX_ProductId");
            Assert.Equal("dbo.MigrationsProducts", createOperation.Table);
            Assert.False(createOperation.IsClustered);
            Assert.False(createOperation.IsUnique);
            Assert.Equal(new List<string> { "ProductId" }, createOperation.Columns);
        }
    }

    #endregion

    #region Modification Functions

    public class AutoAndGenerateScenarios_RenameProcedure :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RenameProcedure.V1, AutoAndGenerateScenarios_RenameProcedure.V2>
    {
        public AutoAndGenerateScenarios_RenameProcedure()
        {
            IsDownDataLoss = false;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.HasName("ins_customer"));
                                m.Update(c => c.HasName("upd_customer"));
                                m.Delete(c => c.HasName("del_customer"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count(o => o is RenameProcedureOperation));

            var renameProcedureOperation
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "dbo.MigrationsCustomer_Insert");

            Assert.Equal("ins_customer", renameProcedureOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count(o => o is RenameProcedureOperation));

            var renameProcedureOperation
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "dbo.ins_customer");

            Assert.Equal("MigrationsCustomer_Insert", renameProcedureOperation.NewName);
        }
    }

    public class AutoAndGenerateScenarios_AlterProcedure :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterProcedure.V1, AutoAndGenerateScenarios_AlterProcedure.V2>
    {
        public AutoAndGenerateScenarios_AlterProcedure()
        {
            IsDownNotSupported = true;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.Parameter(mc => mc.Age, "old"));
                                m.Update(c => c.Parameter(mc => mc.HomeAddress.City, "addr_city"));
                                m.Delete(c => c.Parameter(mc => mc.Id, "key"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyMigrationsException(MigrationsException migrationsException)
        {
            migrationsException.ValidateMessage("AutomaticStaleFunctions");
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count(o => o is AlterProcedureOperation));

            var alterProcedureOperation
                = migrationOperations
                    .OfType<AlterProcedureOperation>()
                    .Single(o => o.Name == "dbo.MigrationsCustomer_Insert");

            Assert.True(alterProcedureOperation.Parameters.Any(p => p.Name == "old"));
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(0, migrationOperations.Count(o => o is AlterProcedureOperation));
        }
    }

    public class AutoAndGenerateScenarios_MoveProcedure :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_MoveProcedure.V1, AutoAndGenerateScenarios_MoveProcedure.V2>
    {
        public AutoAndGenerateScenarios_MoveProcedure()
        {
            IsDownDataLoss = false;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.HasName("MigrationsCustomer_Insert", "foo"));
                                m.Update(c => c.HasName("upd_customer", "bar"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is MoveProcedureOperation));
            Assert.Equal(1, migrationOperations.Count(o => o is RenameProcedureOperation));

            var moveProcedureOperation
                = migrationOperations
                    .OfType<MoveProcedureOperation>()
                    .Single(o => o.Name == "dbo.MigrationsCustomer_Update");

            Assert.Equal("bar", moveProcedureOperation.NewSchema);

            var renameProcedureOperation
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "bar.MigrationsCustomer_Update");

            Assert.Equal("upd_customer", renameProcedureOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is MoveProcedureOperation));
            Assert.Equal(1, migrationOperations.Count(o => o is RenameProcedureOperation));
        }
    }

    public class AutoAndGenerateScenarios_MoveProcedure_ManyToMany :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_MoveProcedure_ManyToMany.V1, AutoAndGenerateScenarios_MoveProcedure_ManyToMany.V2>
    {
        public AutoAndGenerateScenarios_MoveProcedure_ManyToMany()
        {
            IsDownDataLoss = false;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Order>()
                    .HasMany(o => o.OrderLines)
                    .WithMany()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Order>()
                    .HasMany(o => o.OrderLines)
                    .WithMany()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.HasName("OrderOrderLine_Insert", "foo"));
                                m.Delete(c => c.HasName("del_order_orderlines", "bar"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is MoveProcedureOperation));
            Assert.Equal(1, migrationOperations.Count(o => o is RenameProcedureOperation));

            var moveProcedureOperation
                = migrationOperations
                    .OfType<MoveProcedureOperation>()
                    .Single(o => o.Name == "dbo.OrderOrderLine_Delete");

            Assert.Equal("bar", moveProcedureOperation.NewSchema);

            var renameProcedureOperation
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "bar.OrderOrderLine_Delete");

            Assert.Equal("del_order_orderlines", renameProcedureOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is MoveProcedureOperation));
            Assert.Equal(1, migrationOperations.Count(o => o is RenameProcedureOperation));
        }
    }

    public class AutoAndGenerateScenarios_RenameProcedure_ManyToMany :
        AutoAndGenerateTestCase
            <AutoAndGenerateScenarios_RenameProcedure_ManyToMany.V1, AutoAndGenerateScenarios_RenameProcedure_ManyToMany.V2>
    {
        public AutoAndGenerateScenarios_RenameProcedure_ManyToMany()
        {
            IsDownDataLoss = false;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Order>()
                    .HasMany(o => o.OrderLines)
                    .WithMany()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Order>()
                    .HasMany(o => o.OrderLines)
                    .WithMany()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.HasName("ins_order_orderlines"));
                                m.Delete(c => c.HasName("del_order_orderlines"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is RenameProcedureOperation));

            var renameProcedureOperationInsert
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "dbo.OrderOrderLine_Insert");

            Assert.Equal("ins_order_orderlines", renameProcedureOperationInsert.NewName);

            var renameProcedureOperationDelete
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "dbo.OrderOrderLine_Delete");

            Assert.Equal("del_order_orderlines", renameProcedureOperationDelete.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is RenameProcedureOperation));
        }
    }

    public class AutoAndGenerateScenarios_AlterProcedure_ManyToMany :
        AutoAndGenerateTestCase
            <AutoAndGenerateScenarios_AlterProcedure_ManyToMany.V1, AutoAndGenerateScenarios_AlterProcedure_ManyToMany.V2>
    {
        public AutoAndGenerateScenarios_AlterProcedure_ManyToMany()
        {
            IsDownDataLoss = false;
            IsDownNotSupported = true;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Order>()
                    .HasMany(o => o.OrderLines)
                    .WithMany()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Order>()
                    .HasMany(o => o.OrderLines)
                    .WithMany()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.LeftKeyParameter(o => o.OrderId, "order_id"));
                                m.Delete(c => c.RightKeyParameter(ol => ol.Id, "order_line_id"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is AlterProcedureOperation));

            var alterProcedureOperationInsert
                = migrationOperations
                    .OfType<AlterProcedureOperation>()
                    .Single(o => o.Name == "dbo.OrderOrderLine_Insert");

            Assert.True(alterProcedureOperationInsert.Parameters.Any(p => p.Name == "order_id"));

            var alterProcedureOperationDelete
                = migrationOperations
                    .OfType<AlterProcedureOperation>()
                    .Single(o => o.Name == "dbo.OrderOrderLine_Delete");

            Assert.True(alterProcedureOperationDelete.Parameters.Any(p => p.Name == "order_line_id"));
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(0, migrationOperations.Count(o => o is AlterProcedureOperation));
        }
    }

    #endregion
}
