// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EntityContainerTests
    {
        [Fact]
        public void Can_set_and_get_name()
        {
            var entityContainer
                = new EntityContainer
                      {
                          Name = "Foo"
                      };

            Assert.Equal("Foo", entityContainer.Name);
        }

        [Fact]
        public void Can_get_collection_of_association_sets()
        {
            var entityContainer = new EntityContainer("C", DataSpace.CSpace);

            entityContainer.AddEntitySetBase(new AssociationSet("A", new AssociationType()));

            Assert.Equal(1, entityContainer.AssociationSets.Count);
            Assert.Empty(entityContainer.EntitySets);
        }

        [Fact]
        public void Can_get_collection_of_entity_sets()
        {
            var entityContainer = new EntityContainer("C", DataSpace.CSpace);

            entityContainer.AddEntitySetBase(new EntitySet("E", null, null, null, new EntityType()));

            Assert.Equal(1, entityContainer.EntitySets.Count);
            Assert.Empty(entityContainer.AssociationSets);
        }

        [Fact]
        public void Can_remove_set_from_container()
        {
            var entityContainer = new EntityContainer("C", DataSpace.CSpace);
            var associationSet = new AssociationSet("A", new AssociationType());

            entityContainer.AddEntitySetBase(associationSet);

            Assert.Equal(1, entityContainer.AssociationSets.Count);

            entityContainer.RemoveEntitySetBase(associationSet);

            Assert.Empty(entityContainer.AssociationSets);
            Assert.Null(associationSet.EntityContainer);
        }

        [Fact]
        public void Create_factory_method_sets_properties_and_seals_the_type()
        {
            var entitySets = new[] { new EntitySet() { Name = "Bar"} };

            var entityContainer = 
                EntityContainer.Create("Foo", DataSpace.SSpace, entitySets, null);

            Assert.Equal("Foo", entityContainer.Name);
            Assert.Equal(entitySets, entityContainer.EntitySets);
            Assert.Empty(entityContainer.FunctionImports);
            Assert.True(entityContainer.IsReadOnly);
        }
    }
}
