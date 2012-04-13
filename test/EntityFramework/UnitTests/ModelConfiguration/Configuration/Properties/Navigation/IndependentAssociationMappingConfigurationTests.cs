namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class IndependentAssociationMappingConfigurationTests
    {
        [Fact]
        public void Configure_should_split_key_constraint_when_to_table_configuration()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var sourceTable = database.AddTable("Source");
            var fkColumn = sourceTable.AddColumn("Fk");
            var foreignKeyConstraint = new DbForeignKeyConstraintMetadata();
            foreignKeyConstraint.DependentColumns.Add(fkColumn);
            sourceTable.ForeignKeyConstraints.Add(foreignKeyConstraint);
            var targetTable = database.AddTable("Split");
            var associationSetMapping = new DbAssociationSetMapping().Initialize();
            associationSetMapping.Table = sourceTable;
            associationSetMapping.SourceEndMapping.PropertyMappings.Add(new DbEdmPropertyMapping { Column = fkColumn });

            var independentAssociationMappingConfiguration
                = new ForeignKeyAssociationMappingConfiguration();

            independentAssociationMappingConfiguration.ToTable("Split");

            independentAssociationMappingConfiguration.Configure(associationSetMapping, database);

            Assert.True(targetTable.Columns.Contains(fkColumn));
            Assert.True(targetTable.ForeignKeyConstraints.Contains(foreignKeyConstraint));
            Assert.False(sourceTable.Columns.Contains(fkColumn));
            Assert.False(sourceTable.ForeignKeyConstraints.Contains(foreignKeyConstraint));
            Assert.Same(targetTable, associationSetMapping.Table);
        }

        [Fact]
        public void Configure_should_throw_when_configured_table_not_found()
        {
            var independentAssociationMappingConfiguration
                = new ForeignKeyAssociationMappingConfiguration();

            independentAssociationMappingConfiguration.ToTable("Split");

            var associationSetMapping = new DbAssociationSetMapping().Initialize();
            var database = new DbDatabaseMetadata().Initialize();

            Assert.Equal(Strings.TableNotFound("Split"), Assert.Throws<InvalidOperationException>(() => independentAssociationMappingConfiguration.Configure(associationSetMapping, database)).Message);
        }

        [Fact]
        public void Equals_should_return_true_when_table_names_and_columns_match()
        {
            var independentAssociationMappingConfiguration1
                = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration1.ToTable("Foo", "Bar");
            independentAssociationMappingConfiguration1.MapKey("Baz");

            var independentAssociationMappingConfiguration2
                = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration2.ToTable("Foo", "Bar");
            independentAssociationMappingConfiguration2.MapKey("Baz");

            Assert.Equal(independentAssociationMappingConfiguration1, independentAssociationMappingConfiguration2);
        }

        [Fact]
        public void Equals_should_return_false_when_table_names_dont_match()
        {
            var independentAssociationMappingConfiguration1
                = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration1.ToTable("Bar", "Foo");

            var independentAssociationMappingConfiguration2
                = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration2.ToTable("Foo", "Bar");

            Assert.NotEqual(independentAssociationMappingConfiguration1, independentAssociationMappingConfiguration2);
        }

        [Fact]
        public void Equals_should_return_false_when_columns_dont_match()
        {
            var independentAssociationMappingConfiguration1
                = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration1.ToTable("Foo", "Bar");
            independentAssociationMappingConfiguration1.MapKey("Baz");

            var independentAssociationMappingConfiguration2
                = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration2.ToTable("Foo", "Bar");
            independentAssociationMappingConfiguration2.MapKey("Bob");

            Assert.NotEqual(independentAssociationMappingConfiguration1, independentAssociationMappingConfiguration2);
        }
    }
}