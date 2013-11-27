// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class ForeignKeyAssociationMappingConfigurationTests
    {
        [Fact]
        public void Configure_should_split_key_constraint_when_to_table_configuration()
        {
            var database = new EdmModel(DataSpace.SSpace);
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
                = new AssociationSetMapping(
                    new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)), database.GetEntitySet(sourceTable)).Initialize();
            associationSetMapping.SourceEndMapping.AddProperty(new ScalarPropertyMapping(new EdmProperty("PK"), fkColumn));

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
                = new AssociationSetMapping(
                    new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)),
                    new EntitySet())
                    .Initialize();

            var database = new EdmModel(DataSpace.SSpace);

            Assert.Equal(
                Strings.TableNotFound("Split"),
                Assert.Throws<InvalidOperationException>(
                    () => independentAssociationMappingConfiguration
                              .Configure(associationSetMapping, database, new MockPropertyInfo())).
                       Message);
        }

        [Fact]
        public void Configure_should_apply_annotations_to_FK_columns()
        {
            var database = new EdmModel(DataSpace.SSpace);
            var dependentTable = database.AddTable("Source");
            var associationSetMapping = CreateIAMapping(database, dependentTable);

            var configuration = new ForeignKeyAssociationMappingConfiguration();
            configuration.MapKey("K1")
                .HasKeyAnnotation("K1", "A1", "V1")
                .HasKeyAnnotation("K1", "A2", "V2")
                .HasKeyAnnotation("K1", "A1", "V3");

            configuration.Configure(associationSetMapping, database, new MockPropertyInfo());

            var column = dependentTable.Properties.Single(p => p.Name == "K1");

            Assert.Equal("V3", column.Annotations.Single(a => a.Name == XmlConstants.CustomAnnotationNamespace + ":A1").Value);
            Assert.Equal("V2", column.Annotations.Single(a => a.Name == XmlConstants.CustomAnnotationNamespace + ":A2").Value);
        }

        [Fact]
        public void Configure_should_throw_when_annotation_key_name_not_found()
        {
            var database = new EdmModel(DataSpace.SSpace);
            var dependentTable = database.AddTable("Source");
            var associationSetMapping = CreateIAMapping(database, dependentTable);

            var configuration = new ForeignKeyAssociationMappingConfiguration();
            configuration.MapKey("K1").HasKeyAnnotation("BadKey", "A1", "V1");

            Assert.Equal(
                Strings.BadKeyNameForAnnotation("BadKey", "A1"),
                Assert.Throws<InvalidOperationException>(
                    () => configuration.Configure(associationSetMapping, database, new MockPropertyInfo())).Message);

        }

        private static AssociationSetMapping CreateIAMapping(EdmModel database, EntityType dependentTable)
        {
            var fkColumn = new EdmProperty(
                "FK", ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))));

            dependentTable.AddColumn(fkColumn);
            var foreignKeyConstraint = new ForeignKeyBuilder(database, "FK")
            {
                PrincipalTable = database.AddTable("P")
            };

            dependentTable.AddForeignKey(foreignKeyConstraint);
            foreignKeyConstraint.DependentColumns = new[] { fkColumn };

            var associationSetMapping
                = new AssociationSetMapping(
                    new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)),
                    database.GetEntitySet(dependentTable)).Initialize();

            associationSetMapping.SourceEndMapping.AddProperty(new ScalarPropertyMapping(new EdmProperty("PK"), fkColumn));
            
            return associationSetMapping;
        }

        [Fact]
        public void Equals_should_return_true_when_table_names_columns_ans_annotations_match()
        {
            var independentAssociationMappingConfiguration1 = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration1
                .ToTable("Foo", "Bar")
                .MapKey("Baz", "Biz")
                .HasKeyAnnotation("Biz", "Buz", "Knees")
                .HasKeyAnnotation("Baz", "Boz", "Bees");

            var independentAssociationMappingConfiguration2 = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration2
                .ToTable("Foo", "Bar")
                .MapKey("Baz", "Biz")
                .HasKeyAnnotation("Baz", "Boz", "Bees")
                .HasKeyAnnotation("Biz", "Buz", "Knees");

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

        [Fact]
        public void Equals_should_return_false_when_annotations_dont_match()
        {
            var independentAssociationMappingConfiguration1 = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration1
                .MapKey("Baz", "Biz")
                .HasKeyAnnotation("Baz", "Boz", "Bees")
                .HasKeyAnnotation("Biz", "Buz", "Knees");

            var independentAssociationMappingConfiguration2 = new ForeignKeyAssociationMappingConfiguration();
            independentAssociationMappingConfiguration2
                .MapKey("Baz", "Biz")
                .HasKeyAnnotation("Baz", "Boz", "Cheese")
                .HasKeyAnnotation("Biz", "Buz", "Knees");

            Assert.NotEqual(independentAssociationMappingConfiguration1, independentAssociationMappingConfiguration2);
        }

        [Fact]
        public void HasKeyAnnotation_checks_arguments()
        {
            var configuration = new ForeignKeyAssociationMappingConfiguration();

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("keyColumnName"),
                Assert.Throws<ArgumentException>(() => configuration.HasKeyAnnotation(null, "A", "V")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("keyColumnName"),
                Assert.Throws<ArgumentException>(() => configuration.HasKeyAnnotation(" ", "A", "V")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("annotationName"),
                Assert.Throws<ArgumentException>(() => configuration.HasKeyAnnotation("K", null, "V")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("annotationName"),
                Assert.Throws<ArgumentException>(() => configuration.HasKeyAnnotation("K", " ", "V")).Message);
        }
    }
}
