// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class MappingFragmentTests
    {
        [Fact]
        public void Can_get_flattened_properties_for_nested_mapping()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));
            var property1 = EdmProperty.CreateComplex("P1", new ComplexType("CT"));
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
                new EntityTypeMapping(
                new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace))));

            Assert.Equal(
                "storeEntitySet",
                Assert.Throws<ArgumentNullException>(
                    () => new MappingFragment(null, entityTypeMapping, false)).ParamName);
        }

        [Fact]
        public void Can_not_create_mapping_fragment_with_null_type_mapping()
        {
            Assert.Equal(
                "typeMapping",
                Assert.Throws<ArgumentNullException>(
                    () => new MappingFragment(new EntitySet(), null, false)).ParamName);
        }

        [Fact]
        public void Can_add_and_remove_properties()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.PropertyMappings);

            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            mappingFragment.AddPropertyMapping(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, mappingFragment.PropertyMappings.Single());

            mappingFragment.RemovePropertyMapping(scalarPropertyMapping);

            Assert.Empty(mappingFragment.PropertyMappings);
        }

        [Fact]
        public void Can_add_scalar_column_mapping()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));
            var property = new EdmProperty("P");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Same(columnMappingBuilder, mappingFragment.ColumnMappings.Single());

            var scalarPropertyMapping = (ScalarPropertyMapping)mappingFragment.PropertyMappings.Single();

            Assert.Same(columnProperty, scalarPropertyMapping.Column);
            Assert.Same(property, scalarPropertyMapping.Property);
        }

        [Fact]
        public void Cannot_add_invalid_column_mapping_builder()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

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
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            var columnMappingBuilder =
                new ColumnMappingBuilder(new EdmProperty("S", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })), new[] { new EdmProperty("S") });

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
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));
            var property = new EdmProperty("P");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Same(columnMappingBuilder, mappingFragment.ColumnMappings.Single());
            Assert.NotEmpty(mappingFragment.PropertyMappings);

            mappingFragment.RemoveColumnMapping(columnMappingBuilder);

            Assert.Empty(mappingFragment.ColumnMappings);
            Assert.Empty(mappingFragment.PropertyMappings);
        }

        [Fact]
        public void Can_update_scalar_column_mapping()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            var property = new EdmProperty("P");

            mappingFragment.AddColumnMapping(new ColumnMappingBuilder(new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })), new[] { property }));

            var columnProperty = new EdmProperty("C'", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            var scalarPropertyMapping = (ScalarPropertyMapping)mappingFragment.PropertyMappings.Single();

            Assert.Same(columnProperty, scalarPropertyMapping.Column);
            Assert.Same(property, scalarPropertyMapping.Property);
        }

        [Fact]
        public void Can_add_complex_column_mapping()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));
            var property1 = EdmProperty.CreateComplex("P1", new ComplexType("CT"));
            var property2 = new EdmProperty("P2");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Same(columnMappingBuilder, mappingFragment.ColumnMappings.Single());

            var complexPropertyMapping = (ComplexPropertyMapping)mappingFragment.PropertyMappings.Single();

            var typeMapping = complexPropertyMapping.TypeMappings.Single();

            var scalarPropertyMapping = (ScalarPropertyMapping)typeMapping.PropertyMappings.Single();

            Assert.Same(columnProperty, scalarPropertyMapping.Column);
            Assert.Same(property2, scalarPropertyMapping.Property);
        }

        [Fact]
        public void Can_remove_complex_column_mapping()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnMappings);

            var columnProperty = new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));
            var property1 = EdmProperty.CreateComplex("P1", new ComplexType("CT"));
            var property2 = new EdmProperty("P2");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder);

            Assert.Same(columnMappingBuilder, mappingFragment.ColumnMappings.Single());
            Assert.NotEmpty(mappingFragment.PropertyMappings);

            mappingFragment.RemoveColumnMapping(columnMappingBuilder);

            Assert.Empty(mappingFragment.ColumnMappings);
            Assert.Empty(mappingFragment.PropertyMappings);
        }

        [Fact]
        public void Can_update_complex_column_mapping()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            var property1 = EdmProperty.CreateComplex("P1", new ComplexType("CT"));
            var property2 = new EdmProperty("P2");

            var columnMappingBuilder1 = new ColumnMappingBuilder(new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })), new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder1);

            var columnProperty = new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace }));

            var columnMappingBuilder2 = new ColumnMappingBuilder(columnProperty, new[] { property1, property2 });

            mappingFragment.AddColumnMapping(columnMappingBuilder2);

            var complexPropertyMapping = (ComplexPropertyMapping)mappingFragment.PropertyMappings.Single();

            var typeMapping = complexPropertyMapping.TypeMappings.Single();

            var scalarPropertyMapping = (ScalarPropertyMapping)typeMapping.PropertyMappings.Single();

            Assert.Same(columnProperty, scalarPropertyMapping.Column);
            Assert.Same(property2, scalarPropertyMapping.Property);
        }

        [Fact]
        public void Can_get_and_set_table_set()
        {
            var tableSet = new EntitySet();

            var mappingFragment
                = new MappingFragment(
                    tableSet,
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

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
                = new MappingFragment(
                    tableSet,
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Same(table, mappingFragment.Table);
        }

        [Fact]
        public void Can_add_and_remove_column_conditions()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            Assert.Empty(mappingFragment.ColumnConditions);

            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })), 42, null);

            mappingFragment.AddConditionProperty(conditionPropertyMapping);

            Assert.Same(conditionPropertyMapping, mappingFragment.ColumnConditions.Single());

            mappingFragment.RemoveConditionProperty(conditionPropertyMapping);

            Assert.Empty(mappingFragment.ColumnConditions);
        }

        [Fact]
        public void Cannot_add_property_when_read_only()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            mappingFragment.SetReadOnly();

            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mappingFragment.AddPropertyMapping(scalarPropertyMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_read_only()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);
            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            mappingFragment.AddPropertyMapping(scalarPropertyMapping);

            mappingFragment.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mappingFragment.RemovePropertyMapping(scalarPropertyMapping)).Message);
        }

        [Fact]
        public void Cannot_add_condition_when_read_only()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);

            mappingFragment.SetReadOnly();

            var conditionMapping = new IsNullConditionMapping(new EdmProperty("P"), true);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mappingFragment.AddCondition(conditionMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_condition_when_read_only()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);
            var conditionMapping = new IsNullConditionMapping(new EdmProperty("P"), true);

            mappingFragment.AddCondition(conditionMapping);

            mappingFragment.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mappingFragment.RemoveCondition(conditionMapping)).Message);
        }

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var mappingFragment
                = new MappingFragment(
                    new EntitySet(),
                    new EntityTypeMapping(
                        new EntitySetMapping(
                            new EntitySet(),
                            new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)))), false);
            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));
            var conditionMapping = new IsNullConditionMapping(new EdmProperty("P"), true);

            mappingFragment.AddPropertyMapping(scalarPropertyMapping);
            mappingFragment.AddCondition(conditionMapping);

            Assert.False(scalarPropertyMapping.IsReadOnly);
            Assert.False(conditionMapping.IsReadOnly);
            mappingFragment.SetReadOnly();
            Assert.True(scalarPropertyMapping.IsReadOnly);
            Assert.True(conditionMapping.IsReadOnly);
        }
    }
}
