// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class EntityContainerTests
    {
        [Fact]
        public void Can_set_and_get_name()
        {
            var entityContainer
                = new EntityContainer("Foo", DataSpace.CSpace);

            Assert.Equal("Foo", entityContainer.Name);
        }

        [Fact]
        public void Can_get_collection_of_association_sets()
        {
            var entityContainer = new EntityContainer("C", DataSpace.CSpace);

            entityContainer.AddEntitySetBase(new AssociationSet("A", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)));

            Assert.Equal(1, entityContainer.AssociationSets.Count);
            Assert.Empty(entityContainer.EntitySets);
        }

        [Fact]
        public void Can_get_collection_of_entity_sets()
        {
            var entityContainer = new EntityContainer("C", DataSpace.CSpace);

            entityContainer.AddEntitySetBase(new EntitySet("E", null, null, null, new EntityType("E", "N", DataSpace.CSpace)));

            Assert.Equal(1, entityContainer.EntitySets.Count);
            Assert.Empty(entityContainer.AssociationSets);
        }

        [Fact]
        public void Can_remove_set_from_container()
        {
            var entityContainer = new EntityContainer("C", DataSpace.CSpace);
            var associationSet = new AssociationSet("A", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            entityContainer.AddEntitySetBase(associationSet);

            Assert.Equal(1, entityContainer.AssociationSets.Count);

            entityContainer.RemoveEntitySetBase(associationSet);

            Assert.Empty(entityContainer.AssociationSets);
            Assert.Null(associationSet.EntityContainer);
        }

        [Fact]
        public void Create_factory_method_sets_properties_and_seals_the_type()
        {
            var entitySets = new[] { new EntitySet { Name = "Bar"} };

            var functionImports =
                new[]
                    {
                        new EdmFunction(
                            "foo",
                            "bar",
                            DataSpace.CSpace,
                            new EdmFunctionPayload()
                                {
                                    IsFunctionImport = true
                                })
                    };


            var entityContainer =
                EntityContainer.Create("Foo", DataSpace.SSpace, entitySets, functionImports, 
                    new[]
                        {
                            new MetadataProperty(
                                "TestProperty",
                                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                                "value"),
                        });

            Assert.Equal("Foo", entityContainer.Name);
            Assert.Equal(entitySets, entityContainer.EntitySets);
            Assert.Equal(functionImports, entityContainer.FunctionImports);
            Assert.True(entityContainer.IsReadOnly);

            var metadataProperty = entityContainer.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("value", metadataProperty.Value);
        }

        [Fact]
        public void Cannot_create_EntityContainer_with_function_not_marked_as_function_import()
        {
            var function = new EdmFunction(
                "foo",
                "bar",
                DataSpace.CSpace,
                new EdmFunctionPayload()
                    {
                        IsFunctionImport = false
                    });

            Assert.Equal(
                Resources.Strings.OnlyFunctionImportsCanBeAddedToEntityContainer("foo"),
                Assert.Throws<ArgumentException>(
                    () =>
                    EntityContainer.Create("Foo", DataSpace.SSpace, null, new[] { function }, null)).Message);
        }

        [Fact]
        public void Can_add_function_import_to_container()
        {
            var container = new EntityContainer("container", DataSpace.CSpace);
            var function = new EdmFunction(
                "foo",
                "bar",
                DataSpace.CSpace,
                new EdmFunctionPayload()
                {
                    IsFunctionImport = true
                });

            container.AddFunctionImport(function);

            Assert.Equal(1, container.FunctionImports.Count);
            Assert.Same(function, container.FunctionImports.Single());
        }

        [Fact]
        public void Cannot_add_null_function_import_to_container()
        {
            Assert.Equal(
                "function",
                Assert.Throws<ArgumentNullException>(
                () => new EntityContainer("container", DataSpace.CSpace).AddFunctionImport(null)).ParamName);
        }

        [Fact]
        public void Cannot_add_non_function_import_to_container()
        {
            var container = new EntityContainer("container", DataSpace.CSpace);
            var function = new EdmFunction(
                "foo",
                "bar",
                DataSpace.CSpace,
                new EdmFunctionPayload()
                {
                    IsFunctionImport = false
                });

            Assert.Equal(
                Resources.Strings.OnlyFunctionImportsCanBeAddedToEntityContainer("foo"),
                Assert.Throws<ArgumentException>(() => container.AddFunctionImport(function)).Message);
        }

        [Fact]
        public void Cannot_add_function_to_readonly_container()
        {
            var container = new EntityContainer("container", DataSpace.CSpace);
            container.SetReadOnly();

            var function = new EdmFunction(
                "foo",
                "bar",
                DataSpace.CSpace,
                new EdmFunctionPayload()
                {
                    IsFunctionImport = false
                });

            Assert.Equal(
                Resources.Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(() => container.AddFunctionImport(function)).Message);
        }
    }
}
