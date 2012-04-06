namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Spatial;
    using System.IO;
    using Xunit;

    public class VisualBasicMigrationCodeGeneratorTests
    {
        [Fact]
        public void Generate_should_not_produce_lines_that_are_too_long_for_the_compiler()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {},
                    new string('a', 10000),
                    "Target",
                    "Foo",
                    "Bar");

            using (var stringReader = new StringReader(generatedMigration.DesignerCode))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    Assert.True(line.Length <= 1100);
                }
            }
        }


        [Fact]
        public void Generate_can_output_drop_primary_key_with_explicit_name()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation
                      {
                          Table = "T",
                          Name = "PK"
                      };

            dropPrimaryKeyOperation.Columns.Add("c1");
            dropPrimaryKeyOperation.Columns.Add("c2");

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {dropPrimaryKeyOperation},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            DropPrimaryKey(""T"", ""PK"")
        End Sub
        
        Public Overrides Sub Down()
            AddPrimaryKey(""T"", New String() { ""c1"", ""c2"" }, name := ""PK"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_drop_primary_key_with_implicit_name()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation
                      {
                          Table = "T"
                      };

            dropPrimaryKeyOperation.Columns.Add("c1");
            dropPrimaryKeyOperation.Columns.Add("c2");

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {dropPrimaryKeyOperation},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            DropPrimaryKey(""T"", New String() { ""c1"", ""c2"" })
        End Sub
        
        Public Overrides Sub Down()
            AddPrimaryKey(""T"", New String() { ""c1"", ""c2"" })
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_add_primary_key_with_explicit_name()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation
                      {
                          Table = "T",
                          Name = "PK"
                      };

            addPrimaryKeyOperation.Columns.Add("c1");
            addPrimaryKeyOperation.Columns.Add("c2");

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {addPrimaryKeyOperation},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddPrimaryKey(""T"", New String() { ""c1"", ""c2"" }, name := ""PK"")
        End Sub
        
        Public Overrides Sub Down()
            DropPrimaryKey(""T"", ""PK"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_add_primary_key_with_implicit_name()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation
                      {
                          Table = "T"
                      };

            addPrimaryKeyOperation.Columns.Add("c1");
            addPrimaryKeyOperation.Columns.Add("c2");

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {addPrimaryKeyOperation},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddPrimaryKey(""T"", New String() { ""c1"", ""c2"" })
        End Sub
        
        Public Overrides Sub Down()
            DropPrimaryKey(""T"", New String() { ""c1"", ""c2"" })
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_simple_add_foreign_key()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var addForeignKeyOperation
                = new AddForeignKeyOperation
                      {
                          DependentTable = "Orders",
                          PrincipalTable = "Customers",
                          CascadeDelete = true
                      };

            addForeignKeyOperation.DependentColumns.Add("CustomerId");
            addForeignKeyOperation.PrincipalColumns.Add("Id");

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {addForeignKeyOperation},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddForeignKey(""Orders"", ""CustomerId"", ""Customers"", ""Id"", cascadeDelete := True)
        End Sub
        
        Public Overrides Sub Down()
            DropForeignKey(""Orders"", ""CustomerId"", ""Customers"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_composite_add_foreign_key()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var addForeignKeyOperation
                = new AddForeignKeyOperation
                      {
                          DependentTable = "Orders",
                          PrincipalTable = "Customers"
                      };

            addForeignKeyOperation.DependentColumns.Add("CustomerId1");
            addForeignKeyOperation.DependentColumns.Add("CustomerId2");
            addForeignKeyOperation.PrincipalColumns.Add("Id1");
            addForeignKeyOperation.PrincipalColumns.Add("Id2");

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {addForeignKeyOperation},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddForeignKey(""Orders"", New String() { ""CustomerId1"", ""CustomerId2"" }, ""Customers"", New String() { ""Id1"", ""Id2"" })
        End Sub
        
        Public Overrides Sub Down()
            DropForeignKey(""Orders"", New String() { ""CustomerId1"", ""CustomerId2"" }, ""Customers"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_drop_column()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var dropColumnOperation = new DropColumnOperation("Customers", "Foo");

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {dropColumnOperation},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            DropColumn(""Customers"", ""Foo"")
        End Sub
        
        Public Overrides Sub Down()
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_timestamp_column()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var createTableOperation = new CreateTableOperation("Customers");
            var column = new ColumnModel(PrimitiveTypeKind.Binary) {Name = "Version", IsTimestamp = true};
            createTableOperation.Columns.Add(column);

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] {createTableOperation},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            CreateTable(
                ""Customers"",
                Function(c) New With
                    {
                        .Version = c.Binary(timestamp := True)
                    })
            
        End Sub
        
        Public Overrides Sub Down()
            DropTable(""Customers"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_create_table_statement()
        {
            var createTableOperation = new CreateTableOperation("Customers");
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32) {Name = "Id", IsNullable = true, IsIdentity = true};
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(new ColumnModel(PrimitiveTypeKind.String)
                                                 {Name = "Name", IsNullable = false});
            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation {Name = "MyPK"};
            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var addForeignKeyOperation = new AddForeignKeyOperation
                                             {
                                                 DependentTable = "Customers",
                                                 PrincipalTable = "Blogs",
                                                 CascadeDelete = true
                                             };
            addForeignKeyOperation.DependentColumns.Add("Blog_Id");
            addForeignKeyOperation.PrincipalColumns.Add("Id");

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[]
                        {
                            createTableOperation,
                            addForeignKeyOperation,
                            addForeignKeyOperation.CreateCreateIndexOperation()
                        },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            CreateTable(
                ""Customers"",
                Function(c) New With
                    {
                        .Id = c.Int(identity := True),
                        .Name = c.String(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.Id, name := ""MyPK"") _
                .ForeignKey(""Blogs"", Function(t) t.Blog_Id, cascadeDelete := True) _
                .Index(Function(t) t.Blog_Id)
            
        End Sub
        
        Public Overrides Sub Down()
            DropIndex(""Customers"", New String() { ""Blog_Id"" })
            DropForeignKey(""Customers"", ""Blog_Id"", ""Blogs"")
            DropTable(""Customers"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);

            Assert.Equal(
                @"' <auto-generated />
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.Migrations.Infrastructure

Namespace Foo
    Public NotInheritable Partial Class Bar
        Implements IMigrationMetadata
    
        Private ReadOnly Property IMigrationMetadata_Id() As String Implements IMigrationMetadata.Id
            Get
                Return ""Migration""
            End Get
        End Property
        
        Private ReadOnly Property IMigrationMetadata_Source() As String Implements IMigrationMetadata.Source
            Get
                Return ""Source""
            End Get
        End Property
        
        Private ReadOnly Property IMigrationMetadata_Target() As String Implements IMigrationMetadata.Target
            Get
                Return ""Target""
            End Get
        End Property
    End Class
End Namespace
",
                generatedMigration.DesignerCode);

            Assert.Equal("vb", generatedMigration.Language);
        }

        [Fact]
        public void Generate_can_output_drop_table_statement()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new[] {new DropTableOperation("Customers")},
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            DropTable(""Customers"")
        End Sub
        
        Public Overrides Sub Down()
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_should_scrub_class_name()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[] {},
                "Source",
                "Target",
                "Foo",
                "1$%^&DFDSH");

            Assert.True(generatedMigration.UserCode.Contains("Class _1DFDSH"));

            generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[] {},
                "Source",
                "Target",
                "Foo",
                "While");

            Assert.True(generatedMigration.UserCode.Contains("Class _While"));
        }

        [Fact]
        public void Generate_can_process_null_source_model()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[] {},
                null,
                "Target",
                "Foo",
                "Bar");

            Assert.Equal(
                @"' <auto-generated />
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.Migrations.Infrastructure

Namespace Foo
    Public NotInheritable Partial Class Bar
        Implements IMigrationMetadata
    
        Private ReadOnly Property IMigrationMetadata_Id() As String Implements IMigrationMetadata.Id
            Get
                Return ""Migration""
            End Get
        End Property
        
        Private ReadOnly Property IMigrationMetadata_Source() As String Implements IMigrationMetadata.Source
            Get
                Return Nothing
            End Get
        End Property
        
        Private ReadOnly Property IMigrationMetadata_Target() As String Implements IMigrationMetadata.Target
            Get
                Return ""Target""
            End Get
        End Property
    End Class
End Namespace
",
                generatedMigration.DesignerCode);
        }

        [Fact]
        public void Generate_can_output_add_column_for_geography_type_with_default_value()
        {
            var generatedMigration
                = new VisualBasicMigrationCodeGenerator().Generate(
                    "Migration",
                    new[]
                        {
                            new AddColumnOperation(
                                "T",
                                new ColumnModel(PrimitiveTypeKind.Geography)
                                    {
                                        IsNullable = false,
                                        Name = "C",
                                        DefaultValue = DbGeography.FromText("POINT (6 7)")
                                    })
                        },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Core.Spatial
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddColumn(""T"", ""C"", Function(c) c.Geography(nullable := False, defaultValue := DbGeography.FromText(""POINT (6 7)"", 4326)))
        End Sub
        
        Public Overrides Sub Down()
            DropColumn(""T"", ""C"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_add_column_for_geometry_type_with_default_value()
        {
            var generatedMigration
                = new VisualBasicMigrationCodeGenerator().Generate(
                    "Migration",
                    new[]
                        {
                            new AddColumnOperation(
                                "T",
                                new ColumnModel(PrimitiveTypeKind.Geometry)
                                    {
                                        IsNullable = false,
                                        Name = "C",
                                        DefaultValue = DbGeometry.FromText("POINT (8 9)")
                                    })
                        },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Core.Spatial
Imports System.Data.Entity.Migrations

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddColumn(""T"", ""C"", Function(c) c.Geometry(nullable := False, defaultValue := DbGeometry.FromText(""POINT (8 9)"", 0)))
        End Sub
        
        Public Overrides Sub Down()
            DropColumn(""T"", ""C"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_process_null_namespace()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[0],
                null,
                "Target",
                null,
                "Bar");

            Assert.Equal(
                @"' <auto-generated />
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.Migrations.Infrastructure

Public NotInheritable Partial Class Bar
    Implements IMigrationMetadata

    Private ReadOnly Property IMigrationMetadata_Id() As String Implements IMigrationMetadata.Id
        Get
            Return ""Migration""
        End Get
    End Property
    
    Private ReadOnly Property IMigrationMetadata_Source() As String Implements IMigrationMetadata.Source
        Get
            Return Nothing
        End Get
    End Property
    
    Private ReadOnly Property IMigrationMetadata_Target() As String Implements IMigrationMetadata.Target
        Get
            Return ""Target""
        End Get
    End Property
End Class
",
                generatedMigration.DesignerCode);

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations

Public Partial Class Bar
    Inherits DbMigration

    Public Overrides Sub Up()
    End Sub
    
    Public Overrides Sub Down()
    End Sub
End Class
",
                generatedMigration.UserCode);
        }
    }
}