// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using Xunit;

    public class CSharpMigrationCodeGeneratorTests
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

            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            CreateStoredProcedure(
                ""Foo"",
                p => new
                    {
                        P = p.String(name: ""P'"", defaultValue: ""Bar"", outParameter: true),
                    },
                body:
                    @""SELECT ShinyHead
                      FROM Pilkingtons""
            );
            
        }
        
        public override void Down()
        {
            DropStoredProcedure(""Foo"");
        }
    }
}
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

            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AlterStoredProcedure(
                ""Foo"",
                p => new
                    {
                        P = p.String(name: ""P'"", defaultValue: ""Bar"", outParameter: true),
                    },
                body:
                    @""SELECT ShinyHead
                      FROM Pilkingtons""
            );
            
        }
        
        public override void Down()
        {
            throw new NotSupportedException(""" + Strings.ScaffoldSprocInDownNotSupported + @""");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_rename_procedure_operation()
        {
            var renameProcedureOperation
                = new RenameProcedureOperation("Foo", "Bar");

            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            RenameStoredProcedure(name: ""Foo"", newName: ""Bar"");
        }
        
        public override void Down()
        {
            RenameStoredProcedure(name: ""Bar"", newName: ""Foo"");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_drop_procedure_operations()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            DropStoredProcedure(""Foo"");
            DropTable(""Bar"");
        }
        
        public override void Down()
        {
            CreateTable(
                ""Bar"",
                c => new
                    {
                    });
            
            throw new NotSupportedException(""" + Strings.ScaffoldSprocInDownNotSupported + @""");
        }
    }
}
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
                    = new CSharpMigrationCodeGenerator().Generate(
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
                    @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AddColumn(""T"", ""C"", c => c.Decimal(defaultValue: 123.45m));
        }
        
        public override void Down()
        {
            DropColumn(""T"", ""C"");
        }
    }
}
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
                    = new CSharpMigrationCodeGenerator().Generate(
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
                    @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AddColumn(""T"", ""C"", c => c.Single(defaultValue: 123.45f));
        }
        
        public override void Down()
        {
            DropColumn(""T"", ""C"");
        }
    }
}
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
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey(""T"", name: ""PK"");
        }
        
        public override void Down()
        {
            AddPrimaryKey(""T"", new[] { ""c1"", ""c2"" }, name: ""PK"");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_drop_primary_key_with_implicit_name()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey(""T"");
        }
        
        public override void Down()
        {
            AddPrimaryKey(""T"", new[] { ""c1"", ""c2"" });
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_add_primary_key_with_explicit_name()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AddPrimaryKey(""T"", new[] { ""c1"", ""c2"" }, name: ""PK"");
        }
        
        public override void Down()
        {
            DropPrimaryKey(""T"", name: ""PK"");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_add_primary_key_with_implicit_name()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AddPrimaryKey(""T"", new[] { ""c1"", ""c2"" });
        }
        
        public override void Down()
        {
            DropPrimaryKey(""T"");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_simple_add_foreign_key()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AddForeignKey(""Orders"", ""CustomerId"", ""Customers"", ""Id"", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey(""Orders"", ""CustomerId"", ""Customers"");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_composite_add_foreign_key()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AddForeignKey(""Orders"", new[] { ""CustomerId1"", ""CustomerId2"" }, ""Customers"", new[] { ""Id1"", ""Id2"" });
        }
        
        public override void Down()
        {
            DropForeignKey(""Orders"", new[] { ""CustomerId1"", ""CustomerId2"" }, ""Customers"");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_drop_column()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            DropColumn(""Customers"", ""Foo"");
        }
        
        public override void Down()
        {
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_timestamp_column()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                ""Customers"",
                c => new
                    {
                        Version = c.Binary(timestamp: true),
                    });
            
        }
        
        public override void Down()
        {
            DropTable(""Customers"");
        }
    }
}
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

            var codeGenerator = new CSharpMigrationCodeGenerator();

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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                ""Customers"",
                c => new
                    {
                        Id = c.Int(name: ""I.d"", identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id, name: ""MyPK"")
                .ForeignKey(""Blogs"", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
        }
        
        public override void Down()
        {
            DropIndex(""Customers"", new[] { ""Blog.Id"" });
            DropForeignKey(""Customers"", ""Blog.Id"", ""Blogs"");
            DropTable(""Customers"");
        }
    }
}
",
                generatedMigration.UserCode);

            Assert.Equal(
                @"// <auto-generated />
namespace Foo
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly.GetInformationalVersion() + @""")]
    public sealed partial class Bar : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(Bar));
        
        string IMigrationMetadata.Id
        {
            get { return ""Migration""; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return Resources.GetString(""Source""); }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString(""Target""); }
        }
    }
}
",
                generatedMigration.DesignerCode);

            Assert.Equal("cs", generatedMigration.Language);
            Assert.Equal(2, generatedMigration.Resources.Count);
            Assert.Equal("Source", generatedMigration.Resources["Source"]);
            Assert.Equal("Target", generatedMigration.Resources["Target"]);
        }

        [Fact]
        public void Generate_can_output_drop_table_statement()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new[] { new DropTableOperation("Customers") },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            DropTable(""Customers"");
        }
        
        public override void Down()
        {
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_move_procedure_statement()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            var generatedMigration
                = codeGenerator.Generate(
                    "Migration",
                    new[] { new MoveProcedureOperation("Insert_Customers", "foo") },
                    "Source",
                    "Target",
                    "Foo",
                    "Bar");

            Assert.Equal(
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            MoveStoredProcedure(name: ""Insert_Customers"", newSchema: ""foo"");
        }
        
        public override void Down()
        {
            MoveStoredProcedure(name: ""foo.Insert_Customers"", newSchema: null);
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_should_scrub_class_name()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            var generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[] { },
                "Source",
                "Target",
                "Foo",
                "1$%^&DFDSH");

            Assert.True(generatedMigration.UserCode.Contains("class _1DFDSH"));

            generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[] { },
                "Source",
                "Target",
                "Foo",
                "while");

            Assert.True(generatedMigration.UserCode.Contains("class _while"));
        }

        [Fact]
        public void Generate_can_process_null_source_model()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            var generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[] { },
                null,
                "Target",
                "Foo",
                "Bar");

            Assert.Equal(
                @"// <auto-generated />
namespace Foo
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly.GetInformationalVersion() + @""")]
    public sealed partial class Bar : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(Bar));
        
        string IMigrationMetadata.Id
        {
            get { return ""Migration""; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString(""Target""); }
        }
    }
}
",
                generatedMigration.DesignerCode);

            Assert.Equal(1, generatedMigration.Resources.Count);
            Assert.Equal("Target", generatedMigration.Resources["Target"]);
        }

        [Fact]
        public void Generate_can_output_add_column_for_geography_type_with_default_value()
        {
            var generatedMigration
                = new CSharpMigrationCodeGenerator().Generate(
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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Spatial;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AddColumn(""T"", ""C"", c => c.Geography(nullable: false, defaultValue: DbGeography.FromText(""POINT (6 7)"", 4326)));
        }
        
        public override void Down()
        {
            DropColumn(""T"", ""C"");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_output_add_column_for_geometry_type_with_default_value()
        {
            var generatedMigration
                = new CSharpMigrationCodeGenerator().Generate(
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
                @"namespace Foo
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Spatial;
    
    public partial class Bar : DbMigration
    {
        public override void Up()
        {
            AddColumn(""T"", ""C"", c => c.Geometry(nullable: false, defaultValue: DbGeometry.FromText(""POINT (8 9)"", 0)));
        }
        
        public override void Down()
        {
            DropColumn(""T"", ""C"");
        }
    }
}
",
                generatedMigration.UserCode);
        }

        [Fact]
        public void Generate_can_process_null_namespace()
        {
            var codeGenerator = new CSharpMigrationCodeGenerator();

            var generatedMigration = codeGenerator.Generate(
                "Migration",
                new MigrationOperation[0],
                null,
                "Target",
                null,
                "Bar");

            Assert.Equal(
                @"// <auto-generated />
using System.CodeDom.Compiler;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Resources;

[GeneratedCode(""EntityFramework.Migrations"", """ + typeof(DbContext).Assembly.GetInformationalVersion() + @""")]
public sealed partial class Bar : IMigrationMetadata
{
    private readonly ResourceManager Resources = new ResourceManager(typeof(Bar));
    
    string IMigrationMetadata.Id
    {
        get { return ""Migration""; }
    }
    
    string IMigrationMetadata.Source
    {
        get { return null; }
    }
    
    string IMigrationMetadata.Target
    {
        get { return Resources.GetString(""Target""); }
    }
}
",
                generatedMigration.DesignerCode);

            Assert.Equal(1, generatedMigration.Resources.Count);
            Assert.Equal("Target", generatedMigration.Resources["Target"]);

            Assert.Equal(
                @"using System;
using System.Data.Entity.Migrations;

public partial class Bar : DbMigration
{
    public override void Up()
    {
    }
    
    public override void Down()
    {
    }
}
",
                generatedMigration.UserCode);
        }
    }
}
