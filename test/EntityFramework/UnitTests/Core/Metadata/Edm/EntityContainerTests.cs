// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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

        public void AssociationSets_and_EntitySets_are_thread_safe()
        {
            var entityContainer = new EntityContainer("C", DataSpace.CSpace);

            entityContainer.AddEntitySetBase(new AssociationSet("A", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)));
            entityContainer.AddEntitySetBase(new EntitySet("E", null, null, null, new EntityType("E", "N", DataSpace.CSpace)));

            const int cycles = 200;
            const int threadCount = 30;

            Action readAssociationSets = () =>
            {
                for (var i = 0; i < cycles; ++i)
                {
                    var associationSets = entityContainer.AssociationSets;

                    //touching BaseEntitySets.Source triggers a reset to AssociationSets
                    var sourceCount = entityContainer.BaseEntitySets.Source.Count;
                    Assert.True(sourceCount == 1);

                    var associationSetsAfterReset = entityContainer.AssociationSets;

                    Assert.True(associationSets != null, "First reference to AssociationSets should not be null");
                    Assert.True(associationSetsAfterReset != null, "Second reference to AssociationSets should not be null");
                    Assert.False(ReferenceEquals(associationSets, associationSetsAfterReset), "The AssociationSets instances should be different");
                }
            };

            Action readEntitySets = () =>
            {
                for (var i = 0; i < cycles; ++i)
                {
                    var entitySets = entityContainer.EntitySets;

                    //touching BaseEntitySets.Source triggers a reset to EntitySets
                    var sourceCount = entityContainer.BaseEntitySets.Source.Count;
                    Assert.True(sourceCount == 1);

                    var entitySetsAfterReset = entityContainer.EntitySets;

                    Assert.True(entitySets != null, "First reference to EntitySets should not be null");
                    Assert.True(entitySetsAfterReset != null, "Second reference to EntitySets should not be null");
                    Assert.False(ReferenceEquals(entitySets, entitySetsAfterReset), "The EntitySets instances should be different");
                }
            };

            var tasks = new List<Thread>();
            for (var i = 0; i < (threadCount/2); ++i)
            {
                tasks.Add(new Thread(new ThreadStart(readEntitySets)));
                tasks.Add(new Thread(new ThreadStart(readAssociationSets)));
            }

            tasks.ForEach(t => t.Start());
            tasks.ForEach(t => t.Join());
        }
    }
}
