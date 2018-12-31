// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.TestHelpers;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using Moq;
    using Xunit;

    public class VisualBasicMigrationCodeGeneratorTests : TestBase
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
                        DefaultValue = "Bar",
                        IsOutParameter = true
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
    
        Public Sub MigrateUp()
            CreateStoredProcedure(
                ""Foo"",
                Function(p) New With
                    {
                        .P = p.String(name := ""P'"", defaultValue := ""Bar"", outParameter := True)
                    },
                body :=
                    ""SELECT ShinyHead"" & vbCrLf & _
                    ""FROM Pilkingtons""
            )
            
        End Sub
        
        Public Sub MigrateDown()
            DropStoredProcedure(""Foo"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_alter_procedure_operations()
        {
            var alterProcedureOperation
                = new AlterProcedureOperation("Foo", "SELECT ShinyHead\r\nFROM Pilkingtons");

            alterProcedureOperation.Parameters.Add(
                new ParameterModel(PrimitiveTypeKind.String)
                {
                    Name = "P'",
                    DefaultValue = "Bar",
                    IsOutParameter = true
                });

            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[]
                        {
                            alterProcedureOperation
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
    
        Public Sub MigrateUp()
            AlterStoredProcedure(
                ""Foo"",
                Function(p) New With
                    {
                        .P = p.String(name := ""P'"", defaultValue := ""Bar"", outParameter := True)
                    },
                body :=
                    ""SELECT ShinyHead"" & vbCrLf & _
                    ""FROM Pilkingtons""
            )
            
        End Sub
        
        Public Sub MigrateDown()
            Throw New NotSupportedException(""" + Strings.ScaffoldSprocInDownNotSupported + @""")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_rename_index_operation()
        {
            var renameIndexOperation
                = new RenameIndexOperation("Foo", "Bar", "Baz");

            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[]
                        {
                            renameIndexOperation
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
    
        Public Sub MigrateUp()
            RenameIndex(table := ""Foo"", name := ""Bar"", newName := ""Baz"")
        End Sub
        
        Public Sub MigrateDown()
            RenameIndex(table := ""Foo"", name := ""Baz"", newName := ""Bar"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_rename_procedure_operation()
        {
            var renameProcedureOperation
                = new RenameProcedureOperation("Foo", "Bar");

            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new MigrationOperation[]
                        {
                            renameProcedureOperation
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
    
        Public Sub MigrateUp()
            RenameStoredProcedure(name := ""Foo"", newName := ""Bar"")
        End Sub
        
        Public Sub MigrateDown()
            RenameStoredProcedure(name := ""Bar"", newName := ""Foo"")
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
    
        Public Sub MigrateUp()
            DropStoredProcedure(""Foo"")
            DropTable(""Bar"")
        End Sub
        
        Public Sub MigrateDown()
            CreateTable(
                ""Bar"",
                Function(c) New With
                    {
                    })
            
            Throw New NotSupportedException(""" + Strings.ScaffoldSprocInDownNotSupported + @""")
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
    
        Public Sub MigrateUp()
            AddColumn(""T"", ""C"", Function(c) c.Decimal(defaultValue := 123.45D))
        End Sub
        
        Public Sub MigrateDown()
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
    
        Public Sub MigrateUp()
            AddColumn(""T"", ""C"", Function(c) c.Single(defaultValue := 123.45F))
        End Sub
        
        Public Sub MigrateDown()
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
    
        Public Sub MigrateUp()
            DropPrimaryKey(""T"", name := ""PK"")
        End Sub
        
        Public Sub MigrateDown()
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
    
        Public Sub MigrateUp()
            DropPrimaryKey(""T"")
        End Sub
        
        Public Sub MigrateDown()
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
    
        Public Sub MigrateUp()
            AddPrimaryKey(""T"", New String() { ""c1"", ""c2"" }, name := ""PK"")
        End Sub
        
        Public Sub MigrateDown()
            DropPrimaryKey(""T"", name := ""PK"")
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
    
        Public Sub MigrateUp()
            AddPrimaryKey(""T"", New String() { ""c1"", ""c2"" })
        End Sub
        
        Public Sub MigrateDown()
            DropPrimaryKey(""T"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_add_primary_key_with_non_clustered_index()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation
                {
                    Table = "T",
                    Name = "PK",
                    IsClustered = false
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
    
        Public Sub MigrateUp()
            AddPrimaryKey(""T"", New String() { ""c1"", ""c2"" }, name := ""PK"", clustered := False)
        End Sub
        
        Public Sub MigrateDown()
            DropPrimaryKey(""T"", name := ""PK"")
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
    
        Public Sub MigrateUp()
            AddForeignKey(""Orders"", ""CustomerId"", ""Customers"", ""Id"", cascadeDelete := True)
        End Sub
        
        Public Sub MigrateDown()
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
    
        Public Sub MigrateUp()
            AddForeignKey(""Orders"", New String() { ""CustomerId1"", ""CustomerId2"" }, ""Customers"", New String() { ""Id1"", ""Id2"" })
        End Sub
        
        Public Sub MigrateDown()
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
    
        Public Sub MigrateUp()
            DropColumn(""Customers"", ""Foo"")
        End Sub
        
        Public Sub MigrateDown()
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
    
        Public Sub MigrateUp()
            CreateTable(
                ""Customers"",
                Function(c) New With
                    {
                        .Version = c.Binary(timestamp := True)
                    })
            
        End Sub
        
        Public Sub MigrateDown()
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
                                   Name = "I.d",
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
            addForeignKeyOperation.DependentColumns.Add("Blog.Id");
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
    
        Public Sub MigrateUp()
            CreateTable(
                ""Customers"",
                Function(c) New With
                    {
                        .Id = c.Int(name := ""I.d"", identity := True),
                        .Name = c.String(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.Id, name := ""MyPK"") _
                .ForeignKey(""Blogs"", Function(t) t.BlogId, cascadeDelete := True) _
                .Index(Function(t) t.BlogId)
            
        End Sub
        
        Public Sub MigrateDown()
            DropIndex(""Customers"", New String() { ""Blog.Id"" })
            DropForeignKey(""Customers"", ""Blog.Id"", ""Blogs"")
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
    <GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly().GetInformationalVersion() + @""")>
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
        
        Public Overrides Sub Up()
            BeforeUp()
            MigrateUp()
            AfterUp()
        End Sub
        
        Public Overrides Sub Down()
            BeforeDown()
            MigrateDown()
            AfterDown()
        End Sub
        
        Partial Private Sub BeforeUp()
        End Sub
        Partial Private Sub MigrateUp()
        End Sub
        Partial Private Sub AfterUp()
        End Sub
        Partial Private Sub BeforeDown()
        End Sub
        Partial Private Sub MigrateDown()
        End Sub
        Partial Private Sub AfterDown()
        End Sub
        
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
        public void Generate_create_table_operation_with_non_clustered_key_and_fully_configured_index()
        {
            var createTableOperation = new CreateTableOperation("Customers");

            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32) { Name = "I.d" };
            createTableOperation.Columns.Add(idColumn);

            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation
            {
                Name = "MyPK",
                IsClustered = false
            };
            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var createIndexOperation = new CreateIndexOperation
            {
                Table = createTableOperation.Name,
                Name = "MyIndex",
                IsClustered = true,
                IsUnique = true
            };
            createIndexOperation.Columns.Add(idColumn.Name);

            var generatedMigration
                = new VisualBasicMigrationCodeGenerator().Generate(
                    "Migration",
                    new MigrationOperation[]
                        {
                            createTableOperation,
                            createIndexOperation
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
    
        Public Sub MigrateUp()
            CreateTable(
                ""Customers"",
                Function(c) New With
                    {
                        .Id = c.Int(name := ""I.d"")
                    }) _
                .PrimaryKey(Function(t) t.Id, name := ""MyPK"", clustered := False) _
                .Index(Function(t) t.Id, unique := True, clustered := True, name := ""MyIndex"")
            
        End Sub
        
        Public Sub MigrateDown()
            DropIndex(""Customers"", ""MyIndex"")
            DropTable(""Customers"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_create_fully_configured_create_index_operation()
        {
            var createIndexOperation = new CreateIndexOperation
            {
                Table = "MyTable",
                Name = "MyIndex",
                IsClustered = true,
                IsUnique = true
            };
            createIndexOperation.Columns.Add("MyColumn");

            var generatedMigration
                = new VisualBasicMigrationCodeGenerator().Generate(
                    "Migration",
                    new MigrationOperation[]
                        {
                            createIndexOperation
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
    
        Public Sub MigrateUp()
            CreateIndex(""MyTable"", ""MyColumn"", unique := True, clustered := True, name := ""MyIndex"")
        End Sub
        
        Public Sub MigrateDown()
            DropIndex(""MyTable"", ""MyIndex"")
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
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
    
        Public Sub MigrateUp()
            DropTable(""Customers"")
        End Sub
        
        Public Sub MigrateDown()
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }


        [Fact]
        public void Generate_can_output_move_procedure_statement()
        {
            var codeGenerator = new VisualBasicMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new[] { new MoveProcedureOperation("Insert_Customers", "foo") },
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
    
        Public Sub MigrateUp()
            MoveStoredProcedure(name := ""Insert_Customers"", newSchema := ""foo"")
        End Sub
        
        Public Sub MigrateDown()
            MoveStoredProcedure(name := ""foo.Insert_Customers"", newSchema := Nothing)
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
    <GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly().GetInformationalVersion() + @""")>
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
        
        Public Overrides Sub Up()
            BeforeUp()
            MigrateUp()
            AfterUp()
        End Sub
        
        Public Overrides Sub Down()
            BeforeDown()
            MigrateDown()
            AfterDown()
        End Sub
        
        Partial Private Sub BeforeUp()
        End Sub
        Partial Private Sub MigrateUp()
        End Sub
        Partial Private Sub AfterUp()
        End Sub
        Partial Private Sub BeforeDown()
        End Sub
        Partial Private Sub MigrateDown()
        End Sub
        Partial Private Sub AfterDown()
        End Sub
        
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
    
        Public Sub MigrateUp()
            AddColumn(""T"", ""C"", Function(c) c.Geography(nullable := False, defaultValue := DbGeography.FromText(""POINT (6 7)"", 4326)))
        End Sub
        
        Public Sub MigrateDown()
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
    
        Public Sub MigrateUp()
            AddColumn(""T"", ""C"", Function(c) c.Geometry(nullable := False, defaultValue := DbGeometry.FromText(""POINT (8 9)"", 0)))
        End Sub
        
        Public Sub MigrateDown()
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

<GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly().GetInformationalVersion() + @""")>
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
    
    Public Overrides Sub Up()
        BeforeUp()
        MigrateUp()
        AfterUp()
    End Sub
    
    Public Overrides Sub Down()
        BeforeDown()
        MigrateDown()
        AfterDown()
    End Sub
    
    Partial Private Sub BeforeUp()
    End Sub
    Partial Private Sub MigrateUp()
    End Sub
    Partial Private Sub AfterUp()
    End Sub
    Partial Private Sub BeforeDown()
    End Sub
    Partial Private Sub MigrateDown()
    End Sub
    Partial Private Sub AfterDown()
    End Sub
    
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

    Public Sub MigrateUp()
    End Sub
    
    Public Sub MigrateDown()
    End Sub
End Class
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Can_generate_AlterColumn_for_added_removed_and_changed_annotations()
        {
            var operations = new[]
            {
                new AlterColumnOperation(
                    "MyTable",
                    new ColumnModel(PrimitiveTypeKind.Int32)
                    {
                        Name = "MyColumn",
                        IsFixedLength = true,
                        Annotations =
                            new Dictionary<string, AnnotationValues>
                            {
                                { "A2", new AnnotationValues(null, "V2") },
                                { "A3", new AnnotationValues(null, "V3") },
                                { "A1", new AnnotationValues(null, "V1") },
                                { "A8", new AnnotationValues("V8A", "V8B") },
                                { "A7", new AnnotationValues("V7A", "V7B") },
                                { "A9", new AnnotationValues("V9A", "V9B") },
                                { "A5", new AnnotationValues("V5", null) },
                                { "A4", new AnnotationValues("V4", null) },
                                { "A6", new AnnotationValues("V6", null) }
                            }
                    },
                    false),
            };

            var generator = new VisualBasicMigrationCodeGenerator();
            var generatedMigration = generator.Generate("Migration", operations, "Source", "Target", "MyNamespace", "MyMigration");

            Assert.Equal(
                @"Imports System
Imports System.Collections.Generic
Imports System.Data.Entity.Infrastructure.Annotations
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace MyNamespace
    Public Partial Class MyMigration
        Inherits DbMigration
    
        Public Sub MigrateUp()
            AlterColumn(""MyTable"", ""MyColumn"", Function(c) c.Int(fixedLength := true,
                annotations := New Dictionary(Of String, AnnotationValues)() From _
                {
                    {
                        ""A1"",
                        New AnnotationValues(oldValue := Nothing, newValue := ""V1"")
                     },
                    {
                        ""A2"",
                        New AnnotationValues(oldValue := Nothing, newValue := ""V2"")
                     },
                    {
                        ""A3"",
                        New AnnotationValues(oldValue := Nothing, newValue := ""V3"")
                     },
                    {
                        ""A4"",
                        New AnnotationValues(oldValue := ""V4"", newValue := Nothing)
                     },
                    {
                        ""A5"",
                        New AnnotationValues(oldValue := ""V5"", newValue := Nothing)
                     },
                    {
                        ""A6"",
                        New AnnotationValues(oldValue := ""V6"", newValue := Nothing)
                     },
                    {
                        ""A7"",
                        New AnnotationValues(oldValue := ""V7A"", newValue := ""V7B"")
                     },
                    {
                        ""A8"",
                        New AnnotationValues(oldValue := ""V8A"", newValue := ""V8B"")
                     },
                    {
                        ""A9"",
                        New AnnotationValues(oldValue := ""V9A"", newValue := ""V9B"")
                     }
                }))
        End Sub
        
        Public Sub MigrateDown()
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Can_generate_AlterColumn_with_annotation_code_generator()
        {
            var operations = new[]
            {
                new AlterColumnOperation(
                    "MyTable",
                    new ColumnModel(PrimitiveTypeKind.Int32)
                    {
                        Name = "MyColumn",
                        Annotations =
                            new Dictionary<string, AnnotationValues>
                            {
                                {
                                    CollationAttribute.AnnotationName,
                                    new AnnotationValues(
                                        new CollationAttribute("At a reasonable volume..."),
                                        new CollationAttribute("While I'm collating..."))
                                }
                            }
                    },
                    false,
                    new AlterColumnOperation(
                        "MyTable",
                        new ColumnModel(PrimitiveTypeKind.Int32)
                        {
                            Name = "MyColumn",
                            Annotations =
                                new Dictionary<string, AnnotationValues>
                                {
                                    {
                                        CollationAttribute.AnnotationName,
                                        new AnnotationValues(
                                            new CollationAttribute("While I'm collating..."),
                                            new CollationAttribute("At a reasonable volume..."))
                                    }
                                }
                        }, false))
            };

            var generator = new VisualBasicMigrationCodeGenerator();
            generator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
            var generatedMigration = generator.Generate("Migration", operations, "Source", "Target", "MyNamespace", "MyMigration");

            Assert.Equal(
                @"Imports System
Imports System.Collections.Generic
Imports System.Data.Entity.Infrastructure.Annotations
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.TestHelpers
Imports Microsoft.VisualBasic

Namespace MyNamespace
    Public Partial Class MyMigration
        Inherits DbMigration
    
        Public Sub MigrateUp()
            AlterColumn(""MyTable"", ""MyColumn"", Function(c) c.Int(
                annotations := New Dictionary(Of String, AnnotationValues)() From _
                {
                    {
                        ""Collation"",
                        New AnnotationValues(oldValue := new CollationAttribute(""At a reasonable volume...""), newValue := new CollationAttribute(""While I'm collating...""))
                     }
                }))
        End Sub
        
        Public Sub MigrateDown()
            AlterColumn(""MyTable"", ""MyColumn"", Function(c) c.Int(
                annotations := New Dictionary(Of String, AnnotationValues)() From _
                {
                    {
                        ""Collation"",
                        New AnnotationValues(oldValue := new CollationAttribute(""While I'm collating...""), newValue := new CollationAttribute(""At a reasonable volume...""))
                     }
                }))
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Can_generate_AddColumn_with_annotations()
        {
            var operations = new[]
            {
                new AddColumnOperation(
                    "MyTable",
                    new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "MyColumn",
                        IsFixedLength = true,
                        Annotations =
                            new Dictionary<string, AnnotationValues>
                            {
                                { "A3", new AnnotationValues(null, "V3") },
                                { "A1", new AnnotationValues(null, "V1") },
                            }
                    },
                    false),
            };

            var generator = new VisualBasicMigrationCodeGenerator();
            var generatedMigration = generator.Generate("Migration", operations, "Source", "Target", "MyNamespace", "MyMigration");

            Assert.Equal(
                @"Imports System
Imports System.Collections.Generic
Imports System.Data.Entity.Infrastructure.Annotations
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace MyNamespace
    Public Partial Class MyMigration
        Inherits DbMigration
    
        Public Sub MigrateUp()
            AddColumn(""MyTable"", ""MyColumn"", Function(c) c.String(fixedLength := true,
                annotations := New Dictionary(Of String, AnnotationValues)() From _
                {
                    {
                        ""A1"",
                        New AnnotationValues(oldValue := Nothing, newValue := ""V1"")
                     },
                    {
                        ""A3"",
                        New AnnotationValues(oldValue := Nothing, newValue := ""V3"")
                     }
                }))
        End Sub
        
        Public Sub MigrateDown()
            DropColumn(""MyTable"", ""MyColumn"",
                removedAnnotations := New Dictionary(Of String, Object)() From _
                {
                    { ""A1"", ""V1"" },
                    { ""A3"", ""V3"" }
                })
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Can_generate_AddColumn_with_custom_annotation_code_gen()
        {
            var operations = new[]
            {
                new AddColumnOperation(
                    "MyTable",
                    new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "MyColumn",
                        IsFixedLength = true,
                        Annotations =
                            new Dictionary<string, AnnotationValues>
                            {
                                {
                                    CollationAttribute.AnnotationName,
                                    new AnnotationValues(
                                        new CollationAttribute("At a reasonable volume..."),
                                        new CollationAttribute("While I'm collating..."))
                                }
                            }
                    },
                    false),
            };

            var generator = new VisualBasicMigrationCodeGenerator();
            generator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
            var generatedMigration = generator.Generate("Migration", operations, "Source", "Target", "MyNamespace", "MyMigration");

            Assert.Equal(
                @"Imports System
Imports System.Collections.Generic
Imports System.Data.Entity.Infrastructure.Annotations
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.TestHelpers
Imports Microsoft.VisualBasic

Namespace MyNamespace
    Public Partial Class MyMigration
        Inherits DbMigration
    
        Public Sub MigrateUp()
            AddColumn(""MyTable"", ""MyColumn"", Function(c) c.String(fixedLength := true,
                annotations := New Dictionary(Of String, AnnotationValues)() From _
                {
                    {
                        ""Collation"",
                        New AnnotationValues(oldValue := new CollationAttribute(""At a reasonable volume...""), newValue := new CollationAttribute(""While I'm collating...""))
                     }
                }))
        End Sub
        
        Public Sub MigrateDown()
            DropColumn(""MyTable"", ""MyColumn"",
                removedAnnotations := New Dictionary(Of String, Object)() From _
                {
                    { ""Collation"", new CollationAttribute(""While I'm collating..."") }
                })
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Can_generate_CreateTable_with_annotations()
        {
            var createTableOperation = new CreateTableOperation(
                "Customers",
                new Dictionary<string, object>
                    {
                        { "AT1", "VT1" },
                        { "AT2", "VT2" }
                    });

            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32)
            {
                Name = "I.d",
                IsNullable = true,
                IsIdentity = true,
                Annotations =
                    new Dictionary<string, AnnotationValues>
                    {
                        { "A1", new AnnotationValues(null, "V1") },
                        { "A2", new AnnotationValues(null, "V2") }
                    }
            };
            createTableOperation.Columns.Add(idColumn);

            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                {
                    Name = "Name",
                    IsNullable = false,
                    Annotations =
                        new Dictionary<string, AnnotationValues>
                        {
                            {
                                CollationAttribute.AnnotationName,
                                new AnnotationValues(
                                    new CollationAttribute("At a reasonable volume..."),
                                    new CollationAttribute("While I'm collating..."))
                            }
                        }
                });

            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation
            {
                Name = "MyPK"
            };
            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var operations = new[] { createTableOperation };

            var generator = new VisualBasicMigrationCodeGenerator();
            generator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
            var generatedMigration = generator.Generate("Migration", operations, "Source", "Target", "MyNamespace", "MyMigration");

            Assert.Equal(
                @"Imports System
Imports System.Collections.Generic
Imports System.Data.Entity.Infrastructure.Annotations
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.TestHelpers
Imports Microsoft.VisualBasic

Namespace MyNamespace
    Public Partial Class MyMigration
        Inherits DbMigration
    
        Public Sub MigrateUp()
            CreateTable(
                ""Customers"",
                Function(c) New With
                    {
                        .Id = c.Int(name := ""I.d"", identity := True,
                            annotations := New Dictionary(Of String, AnnotationValues)() From _
                            {
                                {
                                    ""A1"",
                                    New AnnotationValues(oldValue := Nothing, newValue := ""V1"")
                                 },
                                {
                                    ""A2"",
                                    New AnnotationValues(oldValue := Nothing, newValue := ""V2"")
                                 }
                            }),
                        .Name = c.String(nullable := False,
                            annotations := New Dictionary(Of String, AnnotationValues)() From _
                            {
                                {
                                    ""Collation"",
                                    New AnnotationValues(oldValue := new CollationAttribute(""At a reasonable volume...""), newValue := new CollationAttribute(""While I'm collating...""))
                                 }
                            })
                    },
                annotations := New Dictionary(Of String, Object)() From _
                {
                    { ""AT1"", ""VT1"" },
                    { ""AT2"", ""VT2"" }
                }) _
                .PrimaryKey(Function(t) t.Id, name := ""MyPK"")
            
        End Sub
        
        Public Sub MigrateDown()
            DropTable(""Customers"",
                removedAnnotations := New Dictionary(Of String, Object)() From _
                {
                    { ""AT1"", ""VT1"" },
                    { ""AT2"", ""VT2"" }
                },
                removedColumnAnnotations := New Dictionary(Of String, IDictionary(Of String, Object)) From _
                {
                    {
                        ""I.d"",
                        New Dictionary(Of String, Object)() From _
                        {
                            { ""A1"", ""V1"" },
                            { ""A2"", ""V2"" }
                        }
                     },
                    {
                        ""Name"",
                        New Dictionary(Of String, Object)() From _
                        {
                            { ""Collation"", new CollationAttribute(""While I'm collating..."") }
                        }
                     }
                })
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Can_generate_AlterTableAnnotations_with_annotations()
        {
            var operation = new AlterTableOperation(
                "Customers",
                new Dictionary<string, AnnotationValues>
                {
                    { "AT1", new AnnotationValues(null, "VT1") },
                    {
                        CollationAttribute.AnnotationName,
                        new AnnotationValues(
                            new CollationAttribute("At a reasonable volume..."),
                            new CollationAttribute("While I'm collating..."))
                    },
                    { "AT2", new AnnotationValues(null, "VT2") }

                });

            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32)
            {
                Name = "I.d",
                IsNullable = true,
                IsIdentity = true,
                Annotations =
                    new Dictionary<string, AnnotationValues>
                    {
                        { "A1", new AnnotationValues(null, "V1") },
                        { "A2", new AnnotationValues(null, "V2") }
                    }
            };
            operation.Columns.Add(idColumn);

            operation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                {
                    Name = "Name",
                    IsNullable = false,
                    Annotations =
                        new Dictionary<string, AnnotationValues>
                        {
                            {
                                CollationAttribute.AnnotationName,
                                new AnnotationValues(
                                    new CollationAttribute("At a reasonable volume..."),
                                    new CollationAttribute("While I'm collating..."))
                            }
                        }
                });

            var operations = new[] { operation };

            var generator = new VisualBasicMigrationCodeGenerator();
            generator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
            var generatedMigration = generator.Generate("Migration", operations, "Source", "Target", "MyNamespace", "MyMigration");

            Assert.Equal(
                @"Imports System
Imports System.Collections.Generic
Imports System.Data.Entity.Infrastructure.Annotations
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.TestHelpers
Imports Microsoft.VisualBasic

Namespace MyNamespace
    Public Partial Class MyMigration
        Inherits DbMigration
    
        Public Sub MigrateUp()
            AlterTableAnnotations(
                ""Customers"",
                Function(c) New With
                    {
                        .Id = c.Int(name := ""I.d"", identity := True,
                            annotations := New Dictionary(Of String, AnnotationValues)() From _
                            {
                                {
                                    ""A1"",
                                    New AnnotationValues(oldValue := Nothing, newValue := ""V1"")
                                 },
                                {
                                    ""A2"",
                                    New AnnotationValues(oldValue := Nothing, newValue := ""V2"")
                                 }
                            }),
                        .Name = c.String(nullable := False,
                            annotations := New Dictionary(Of String, AnnotationValues)() From _
                            {
                                {
                                    ""Collation"",
                                    New AnnotationValues(oldValue := new CollationAttribute(""At a reasonable volume...""), newValue := new CollationAttribute(""While I'm collating...""))
                                 }
                            })
                    },
                annotations := New Dictionary(Of String, AnnotationValues)() From _
                {
                    {
                        ""AT1"",
                        New AnnotationValues(oldValue := Nothing, newValue := ""VT1"")
                     },
                    {
                        ""AT2"",
                        New AnnotationValues(oldValue := Nothing, newValue := ""VT2"")
                     },
                    {
                        ""Collation"",
                        New AnnotationValues(oldValue := new CollationAttribute(""At a reasonable volume...""), newValue := new CollationAttribute(""While I'm collating...""))
                     }
                })
            
        End Sub
        
        Public Sub MigrateDown()
            AlterTableAnnotations(
                ""Customers"",
                Function(c) New With
                    {
                        .Id = c.Int(name := ""I.d"", identity := True,
                            annotations := New Dictionary(Of String, AnnotationValues)() From _
                            {
                                {
                                    ""A1"",
                                    New AnnotationValues(oldValue := Nothing, newValue := ""V1"")
                                 },
                                {
                                    ""A2"",
                                    New AnnotationValues(oldValue := Nothing, newValue := ""V2"")
                                 }
                            }),
                        .Name = c.String(nullable := False,
                            annotations := New Dictionary(Of String, AnnotationValues)() From _
                            {
                                {
                                    ""Collation"",
                                    New AnnotationValues(oldValue := new CollationAttribute(""At a reasonable volume...""), newValue := new CollationAttribute(""While I'm collating...""))
                                 }
                            })
                    },
                annotations := New Dictionary(Of String, AnnotationValues)() From _
                {
                    {
                        ""AT1"",
                        New AnnotationValues(oldValue := ""VT1"", newValue := Nothing)
                     },
                    {
                        ""AT2"",
                        New AnnotationValues(oldValue := ""VT2"", newValue := Nothing)
                     },
                    {
                        ""Collation"",
                        New AnnotationValues(oldValue := new CollationAttribute(""While I'm collating...""), newValue := new CollationAttribute(""At a reasonable volume...""))
                     }
                })
            
        End Sub
    End Class
End Namespace
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void GenerateAnnotations_for_single_annotations_checks_arguments()
        {
            var generator = new VisualBasicMigrationCodeGenerator();

            Assert.Equal(
                "annotations",
                Assert.Throws<ArgumentNullException>(
                    () =>
                        generator.GenerateAnnotations(
                            (IDictionary<string, object>)null, new IndentedTextWriter(new Mock<TextWriter>().Object))).ParamName);

            Assert.Equal(
                "writer",
                Assert.Throws<ArgumentNullException>(() => generator.GenerateAnnotations(new Dictionary<string, object>(), null)).ParamName);
        }

        [Fact]
        public void GenerateAnnotations_for_annotation_pairs_checks_arguments()
        {
            var generator = new VisualBasicMigrationCodeGenerator();

            Assert.Equal(
                "annotations",
                Assert.Throws<ArgumentNullException>(
                    () =>
                        generator.GenerateAnnotations(
                            (IDictionary<string, AnnotationValues>)null, new IndentedTextWriter(new Mock<TextWriter>().Object))).ParamName);

            Assert.Equal(
                "writer",
                Assert.Throws<ArgumentNullException>(() => generator.GenerateAnnotations(new Dictionary<string, AnnotationValues>(), null))
                    .ParamName);
        }

        [Fact]
        public void GenerateAnnotation_checks_arguments()
        {
            var generator = new VisualBasicMigrationCodeGenerator();

            Assert.Equal(
                "alterTableOperation",
                Assert.Throws<ArgumentNullException>(() => generator.Generate(null, new IndentedTextWriter(new Mock<TextWriter>().Object))).ParamName);

            Assert.Equal(
                "writer",
                Assert.Throws<ArgumentNullException>(() => generator.Generate(new AlterTableOperation("N", null), null)).ParamName);
        }
    }
}
