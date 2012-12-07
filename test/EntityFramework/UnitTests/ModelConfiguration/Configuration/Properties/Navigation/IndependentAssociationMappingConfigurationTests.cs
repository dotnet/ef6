// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class IndependentAssociationMappingConfigurationTests
    {
        [Fact]
        public void Configure_should_split_key_constraint_when_to_table_configuration()
        {
            var database = new EdmModel().InitializeStore();
            var sourceTable = database.AddTable("Source");
            var principalTable = database.AddTable("P");

            var fkColumn
                = new EdmProperty(
                    "Fk",
                    ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                        TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))));

            sourceTable.AddColumn(fkColumn);
            var foreignKeyConstraint = new ForeignKeyBuilder(database, "FK")
                                           {
                                               PrincipalTable = principalTable
                                           };
            sourceTable.AddForeignKey(foreignKeyConstraint);
            foreignKeyConstraint.DependentColumns = new[] { fkColumn };
            var targetTable = database.AddTable("Split");
            var associationSetMapping
                = new StorageAssociationSetMapping(
                    new AssociationSet("AS", new AssociationType()), database.GetEntitySet(sourceTable)).Initialize();
            associationSetMapping.SourceEndMapping.AddProperty(new StorageScalarPropertyMapping(new EdmProperty("PK"), fkColumn));

            var independentAssociationMappingConfiguration
                = new ForeignKeyAssociationMappingConfiguration();

            independentAssociationMappingConfiguration.ToTable("Split");

            independentAssociationMappingConfiguration.Configure(associationSetMapping, database, new MockPropertyInfo());

            Assert.True(targetTable.Properties.Contains(fkColumn));
            Assert.True(targetTable.ForeignKeyBuilders.Contains(foreignKeyConstraint));
            Assert.False(sourceTable.Properties.Contains(fkColumn));
            Assert.False(sourceTable.ForeignKeyBuilders.Contains(foreignKeyConstraint));
            Assert.Same(targetTable, associationSetMapping.Table);
        }

        [Fact]
        public void Configure_should_throw_when_configured_table_not_found()
        {
            var independentAssociationMappingConfiguration
                = new ForeignKeyAssociationMappingConfiguration();

            independentAssociationMappingConfiguration.ToTable("Split");

            var associationSetMapping
                = new StorageAssociationSetMapping(
                    new AssociationSet("AS", new AssociationType()),
                    new EntitySet())
                    .Initialize();

            var database = new EdmModel().InitializeStore();

            Assert.Equal(
                Strings.TableNotFound("Split"),
                Assert.Throws<InvalidOperationException>(
                    () => independentAssociationMappingConfiguration
                              .Configure(associationSetMapping, database, new MockPropertyInfo())).
                       Message);
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
