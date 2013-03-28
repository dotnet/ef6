// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class StorageMappingFragmentTests
    {
        [Fact]
        public void Can_get_flattened_properties_for_nested_mapping()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C");
            var property1 = EdmProperty.Complex("P1", new ComplexType("CT"));
            var property2 = new EdmProperty("P2");

            var columnMappingBuilder1 = new ColumnMappingBuilder(columnProperty, new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder1);

            var columnMappingBuilder2 = new ColumnMappingBuilder(columnProperty, new[] { property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder2);

            var columnMappingBuilders = mappingFragment.FlattenedProperties.ToList();

            Assert.Equal(2, columnMappingBuilders.Count());
            Assert.True(columnMappingBuilder1.PropertyPath.SequenceEqual(columnMappingBuilders.First().PropertyPath));
            Assert.True(columnMappingBuilder2.PropertyPath.SequenceEqual(columnMappingBuilders.Last().PropertyPath));
        }

        [Fact]
        public void Can_not_create_mapping_fragment_with_null_entity_set()
        {
            var entityTypeMapping = 
                new StorageEntityTypeMapping(
                new StorageEntitySetMapping(
                    new EntitySet(),
                    new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Equal(
                "tableExtent",
                Assert.Throws<ArgumentNullException>(
                    () => new StorageMappingFragment(null, entityTypeMapping, false)).ParamName);
        }

        [Fact]
        public void Can_not_create_mapping_fragment_with_null_type_mapping()
        {
            Assert.Equal(
                "typeMapping",
                Assert.Throws<ArgumentNullException>(
                    () => new StorageMappingFragment(new EntitySet(), null, false)).ParamName);
        }

        [Fact]
        public void Can_add_and_remove_properties()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.Properties);

            var scalarPropertyMapping = new StorageScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C"));

            mappingFragment.AddProperty(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, mappingFragment.Properties.Single());

            mappingFragment.RemoveProperty(scalarPropertyMapping);

            Assert.Empty(mappingFragment.Properties);
        }

        [Fact]
        public void Can_add_scalar_column_mapping()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C");
            var property = new EdmProperty("P");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Same(columnMappingBuilder, mappingFragment.ColumnMappings.Single());

            var scalarPropertyMapping = (StorageScalarPropertyMapping)mappingFragment.Properties.Single();

            Assert.Same(columnProperty, scalarPropertyMapping.ColumnProperty);
            Assert.Same(property, scalarPropertyMapping.EdmProperty);
        }

        [Fact]
        public void Cannot_add_invalid_column_mapping_builder()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Equal(
                "columnMappingBuilder",
                Assert.Throws<ArgumentNullException>(
                    () => mappingFragment.AddColumnMapping(null)).ParamName);

            Assert.Equal(
                Strings.InvalidColumnBuilderArgument("columnBuilderMapping"),
                Assert.Throws<ArgumentException>(
                () => mappingFragment.AddColumnMapping(
                    new ColumnMappingBuilder(new EdmProperty("S"), new List<EdmProperty>()))).Message);


        }

        [Fact]
        public void Cannot_add_duplicate_column_mapping_builder()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            var columnMappingBuilder =
                new ColumnMappingBuilder(new EdmProperty("S"), new[] { new EdmProperty("S") });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Equal(
                Strings.InvalidColumnBuilderArgument("columnBuilderMapping"),
                Assert.Throws<ArgumentException>(
                () => mappingFragment.AddColumnMapping(columnMappingBuilder)).Message);            
        }

        [Fact]
        public void Can_remove_scalar_column_mapping()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C");
            var property = new EdmProperty("P");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Same(columnMappingBuilder, mappingFragment.ColumnMappings.Single());
            Assert.NotEmpty(mappingFragment.Properties);

            mappingFragment.RemoveColumnMapping(columnMappingBuilder);

            Assert.Empty(mappingFragment.ColumnMappings);
            Assert.Empty(mappingFragment.Properties);
        }

        [Fact]
        public void Can_update_scalar_column_mapping()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            var property = new EdmProperty("P");

            mappingFragment.AddColumnMapping(new ColumnMappingBuilder(new EdmProperty("C"), new[] { property }));

            var columnProperty = new EdmProperty("C'");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            var scalarPropertyMapping = (StorageScalarPropertyMapping)mappingFragment.Properties.Single();

            Assert.Same(columnProperty, scalarPropertyMapping.ColumnProperty);
            Assert.Same(property, scalarPropertyMapping.EdmProperty);
        }

        [Fact]
        public void Can_add_complex_column_mapping()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C");
            var property1 = EdmProperty.Complex("P1", new ComplexType("CT"));
            var property2 = new EdmProperty("P2");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Same(columnMappingBuilder, mappingFragment.ColumnMappings.Single());

            var complexPropertyMapping = (StorageComplexPropertyMapping)mappingFragment.Properties.Single();

            var typeMapping = complexPropertyMapping.TypeMappings.Single();

            var scalarPropertyMapping = (StorageScalarPropertyMapping)typeMapping.Properties.Single();

            Assert.Same(columnProperty, scalarPropertyMapping.ColumnProperty);
            Assert.Same(property2, scalarPropertyMapping.EdmProperty);
        }

        [Fact]
        public void Can_remove_complex_column_mapping()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C");
            var property1 = EdmProperty.Complex("P1", new ComplexType("CT"));
            var property2 = new EdmProperty("P2");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Same(columnMappingBuilder, mappingFragment.ColumnMappings.Single());
            Assert.NotEmpty(mappingFragment.Properties);

            mappingFragment.RemoveColumnMapping(columnMappingBuilder);

            Assert.Empty(mappingFragment.ColumnMappings);
            Assert.Empty(mappingFragment.Properties);
        }

        [Fact]
        public void Can_update_complex_column_mapping()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            var property1 = EdmProperty.Complex("P1", new ComplexType("CT"));
            var property2 = new EdmProperty("P2");

            var columnMappingBuilder1 = new ColumnMappingBuilder(new EdmProperty("C"), new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder1);

            var columnProperty = new EdmProperty("C");

            var columnMappingBuilder2 = new ColumnMappingBuilder(columnProperty, new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder2);

            var complexPropertyMapping = (StorageComplexPropertyMapping)mappingFragment.Properties.Single();

            var typeMapping = complexPropertyMapping.TypeMappings.Single();

            var scalarPropertyMapping = (StorageScalarPropertyMapping)typeMapping.Properties.Single();

            Assert.Same(columnProperty, scalarPropertyMapping.ColumnProperty);
            Assert.Same(property2, scalarPropertyMapping.EdmProperty);
        }

        [Fact]
        public void Can_get_and_set_table_set()
        {
            var tableSet = new EntitySet();

            var mappingFragment
                = new StorageMappingFragment(
                    tableSet,
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Same(tableSet, mappingFragment.TableSet);

            tableSet = new EntitySet();

            mappingFragment.TableSet = tableSet;

            Assert.Same(tableSet, mappingFragment.TableSet);
        }

        [Fact]
        public void Can_get_table()
        {
            var table = new EntityType("E", "N", DataSpace.CSpace);

            var tableSet = new EntitySet("ES", null, null, null, table);

            var mappingFragment
                = new StorageMappingFragment(
                    tableSet,
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Same(table, mappingFragment.Table);
        }

        [Fact]
        public void Can_add_and_remove_column_conditions()
        {
            var mappingFragment
                = new StorageMappingFragment(
                    new EntitySet(),
                    new StorageEntityTypeMapping(
                        new StorageEntitySetMapping(
                            new EntitySet(),
                            new StorageEntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnConditions);

            var conditionPropertyMapping
                = new StorageConditionPropertyMapping(null, new EdmProperty("C"), 42, null);

            mappingFragment.AddConditionProperty(conditionPropertyMapping);

            Assert.Same(conditionPropertyMapping, mappingFragment.ColumnConditions.Single());

            mappingFragment.RemoveConditionProperty(conditionPropertyMapping);

            Assert.Empty(mappingFragment.ColumnConditions);
        }
    }
}
