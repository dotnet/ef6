// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using Xunit;

    public class VisualBasicMigrationCodeGeneratorTests
    {
        [Fact]
        public void Generate_can_output_create_procedure_operations()
        {
            var createProcedureOperation
                = new CreateProcedureOperation("Foo", "SELECT ShinyHead\r\nFROM Pilkingtons");

            createProcedureOperation.Parameters.Add(
                new ParameterModel(PrimitiveTypeKind.String)
                    {
                        Name = "P'",
                        DefaultValue = "Bar"
                    });

            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[]
                        {
                            createProcedureOperation
                        },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            CreateStoredProcedure(
                ""Foo"",
                Function(p) New With
                    {
                        .P = p.String(name := ""P'"", defaultValue := ""Bar"")
                    },
                body :=
                    ""SELECT ShinyHead"" & vbCrLf & _
                    ""FROM Pilkingtons""
            )
            
        End Sub
        
        Public Overrides Sub Down()
            DropStoredProcedure(""Foo"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_drop_procedure_operations()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[]
                        {
                            new DropProcedureOperation("Foo"),
                            new DropTableOperation("Bar", new CreateTableOperation("Bar"))
                        },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            DropStoredProcedure(""Foo"")
            DropTable(""Bar"")
        End Sub
        
        Public Overrides Sub Down()
            CreateTable(
                ""Bar"",
                Function(c) New With
                    {
                    })
            
            Throw New NotSupportedException(""Scaffolding create procedure operations is not supported in down methods."")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_should_output_invariant_decimals_when_non_invariant_culture()
        {
            var lastCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("nl-NL");

                var generatedMigration
                    = new VisualBasicMigrationCodeGenerator().Generate(
                        "Migration",
                        new[]
                            {
                                new AddColumnOperation(
                                    "T",
                                    new ColumnModel(PrimitiveTypeKind.Decimal)
                                        {
                                            Name = "C",
                                            DefaultValue = 123.45m
                                        })
                            },
                        "Source",
                        "Target",
                        "Foo",
                        "Bar");

                Assert.Equal(
                    @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddColumn(""T"", ""C"", Function(c) c.Decimal(defaultValue := 123.45D))
        End Sub
        
        Public Overrides Sub Down()
            DropColumn(""T"", ""C"")
        End Sub
    End Class
End Namespace
",
                    generatedMigration.UserCode);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = lastCulture;
            }
        }

        [Fact]
        public void Generate_should_output_invariant_floats_when_non_invariant_culture()
        {
            var lastCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("nl-NL");

                var generatedMigration
                    = new VisualBasicMigrationCodeGenerator().Generate(
                        "Migration",
                        new[]
                            {
                                new AddColumnOperation(
                                    "T",
                                    new ColumnModel(PrimitiveTypeKind.Single)
                                        {
                                            Name = "C",
                                            DefaultValue = 123.45f
                                        })
                            },
                        "Source",
                        "Target",
                        "Foo",
                        "Bar");

                Assert.Equal(
                    @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Foo
    Public Partial Class Bar
        Inherits DbMigration
    
        Public Overrides Sub Up()
            AddColumn(""T"", ""C"", Function(c) c.Single(defaultValue := 123.45F))
        End Sub
        
        Public Overrides Sub Down()
            DropColumn(""T"", ""C"")
        End Sub
    End Class
End Namespace
",
                    generatedMigration.UserCode);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = lastCulture;
            }
        }

        [Fact]
        public void Generate_should_not_produce_lines_that_are_too_long_for_the_compiler()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] { },
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
                    new MigrationOperation[] { dropPrimaryKeyOperation },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
                    new MigrationOperation[] { dropPrimaryKeyOperation },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
                    new MigrationOperation[] { addPrimaryKeyOperation },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
                    new MigrationOperation[] { addPrimaryKeyOperation },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
                    new MigrationOperation[] { addForeignKeyOperation },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
                    new MigrationOperation[] { addForeignKeyOperation },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
                    new MigrationOperation[] { dropColumnOperation },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
            var column = new ColumnModel(PrimitiveTypeKind.Binary)
                             {
                                 Name = "Version",
                                 IsTimestamp = true
                             };
            createTableOperation.Columns.Add(column);

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[] { createTableOperation },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32)
                               {
                                   Name = "Id",
                                   IsNullable = true,
                                   IsIdentity = true
                               };
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "Name",
                        IsNullable = false
                    });
            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation
                                                  {
                                                      Name = "MyPK"
                                                  };
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
Imports Microsoft.VisualBasic

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
Imports System.CodeDom.Compiler
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.Migrations.Infrastructure
Imports System.Resources

Namespace Foo
    <GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly.GetInformationalVersion() + @""")>
    Public NotInheritable Partial Class Bar
        Implements IMigrationMetadata
    
        Private ReadOnly Resources As New ResourceManager(GetType(Bar))
        
        Private ReadOnly Property IMigrationMetadata_Id() As String Implements IMigrationMetadata.Id
            Get
                Return ""Migration""
            End Get
        End Property
        
        Private ReadOnly Property IMigrationMetadata_Source() As String Implements IMigrationMetadata.Source
            Get
                Return Resources.GetString(""Source"")
            End Get
        End Property
        
        Private ReadOnly Property IMigrationMetadata_Target() As String Implements IMigrationMetadata.Target
            Get
                Return Resources.GetString(""Target"")
            End Get
        End Property
    End Class
End Namespace
",
                generatedMigration.DesignerCode);

            Assert.Equal("vb", generatedMigration.Language);
            Assert.Equal(2, generatedMigration.Resources.Count);
            Assert.Equal("Source", generatedMigration.Resources["Source"]);
            Assert.Equal("Target", generatedMigration.Resources["Target"]);
        }

        [Fact]
        public void Generate_can_output_drop_table_statement()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new[] { new DropTableOperation("Customers") },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
                new MigrationOperation[] { },
                "Source",
                "Target",
                "Foo",
                "1$%^&DFDSH");

            Assert.True(generatedMigration.UserCode.Contains("Class _1DFDSH"));

            generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[] { },
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
                new MigrationOperation[] { },
                null,
                "Target",
                "Foo",
                "Bar");

            Assert.Equal(
                @"' <auto-generated />
Imports System.CodeDom.Compiler
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.Migrations.Infrastructure
Imports System.Resources

Namespace Foo
    <GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly.GetInformationalVersion() + @""")>
    Public NotInheritable Partial Class Bar
        Implements IMigrationMetadata
    
        Private ReadOnly Resources As New ResourceManager(GetType(Bar))
        
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
                Return Resources.GetString(""Target"")
            End Get
        End Property
    End Class
End Namespace
",
                generatedMigration.DesignerCode);

            Assert.Equal(1, generatedMigration.Resources.Count);
            Assert.Equal("Target", generatedMigration.Resources["Target"]);
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
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.Spatial
Imports Microsoft.VisualBasic

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
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.Spatial
Imports Microsoft.VisualBasic

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
Imports System.CodeDom.Compiler
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.Migrations.Infrastructure
Imports System.Resources

<GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly.GetInformationalVersion() + @""")>
Public NotInheritable Partial Class Bar
    Implements IMigrationMetadata

    Private ReadOnly Resources As New ResourceManager(GetType(Bar))
    
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
            Return Resources.GetString(""Target"")
        End Get
    End Property
End Class
",
                generatedMigration.DesignerCode);

            Assert.Equal(1, generatedMigration.Resources.Count);
            Assert.Equal("Target", generatedMigration.Resources["Target"]);

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

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
