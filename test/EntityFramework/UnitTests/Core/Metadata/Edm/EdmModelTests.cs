// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class EdmModelTests
    {
        [Fact]
        public void Can_create_only_CSpace_or_SSpace_model()
        {
            foreach (DataSpace dataSpace in Enum.GetValues(typeof(DataSpace)))
            {
                if (dataSpace != DataSpace.CSpace
                    && dataSpace != DataSpace.SSpace)
                {
                    var exception = Assert.Throws<ArgumentException>(() => new EdmModel(dataSpace));

                    Assert.Equal("dataSpace", exception.ParamName);
                    Assert.True(exception.Message.StartsWith(Strings.EdmModel_InvalidDataSpace(dataSpace)));
                }
                else
                {
                    Assert.NotNull(new EdmModel(dataSpace));
                }
            }
        }

        [Fact]
        public void EdmModel_version_set_correctly()
        {
            Assert.Equal(XmlConstants.StoreVersionForV2, new EdmModel(DataSpace.CSpace, XmlConstants.StoreVersionForV2).SchemaVersion);
        }

        [Fact]
        public void EdmModel_default_container_names_set_correctly()
        {
            Assert.Equal("CodeFirstContainer", new EdmModel(DataSpace.CSpace).Containers.Single().Name);
            Assert.Equal("CodeFirstDatabase", new EdmModel(DataSpace.SSpace).Containers.Single().Name);
        }

        [Fact]
        public void GlobalItems_should_return_namespace_items_and_containers()
        {
            var model = new EdmModel(DataSpace.SSpace);

            model.AddItem(new EntityType("Entity", "Model", DataSpace.SSpace));

            Assert.Equal(2, model.GlobalItems.Count());
        }

        [Fact]
        public void Validate_should_throw_on_error()
        {
            var model = new EdmModel(DataSpace.CSpace);

            model.AddItem(new EntityType("E", "N", DataSpace.CSpace));

            Assert.Throws<ModelValidationException>(() => model.Validate());
        }

        [Fact]
        public void Cannot_add_entity_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.CSpace)
                              .AddItem(new EntityType("Entity", "Model", DataSpace.SSpace)));

            Assert.Equal("item", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }

        [Fact]
        public void Cannot_add_enum_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.SSpace)
                              .AddItem(
                                  new EnumType
                                      {
                                          DataSpace = DataSpace.CSpace
                                      }));

            Assert.Equal("item", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }

        [Fact]
        public void Cannot_add_complextype_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.SSpace)
                              .AddItem(new ComplexType("ComplexType", "Model", DataSpace.CSpace)));

            Assert.Equal("item", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }

        [Fact]
        public void Cannot_add_association_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.SSpace)
                              .AddItem(new AssociationType("AssociationType", "Model", /*foreignKey*/ false, DataSpace.CSpace)));

            Assert.Equal("item", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }

        [Fact]
        public void Cannot_add_function_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.SSpace)
                              .AddItem(new EdmFunction("F", "N", DataSpace.CSpace, new EdmFunctionPayload())));

            Assert.Equal("item", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }

        [Fact]
        public void Can_add_remove_association_type()
        {
            var model = new EdmModel(DataSpace.SSpace);
            var associationType = new AssociationType("A", "N", false, DataSpace.SSpace);

            model.AddItem(associationType);

            Assert.True(model.AssociationTypes.Contains(associationType));
            Assert.True(model.NamespaceItems.Contains(associationType));

            model.RemoveItem(associationType);

            Assert.False(model.AssociationTypes.Contains(associationType));
            Assert.False(model.NamespaceItems.Contains(associationType));
        }

        [Fact]
        public void Can_add_remove_complex_type()
        {
            var model = new EdmModel(DataSpace.SSpace);
            var complexType = new ComplexType("C", "N", DataSpace.SSpace);

            model.AddItem(complexType);

            Assert.True(model.ComplexTypes.Contains(complexType));
            Assert.True(model.NamespaceItems.Contains(complexType));

            model.RemoveItem(complexType);

            Assert.False(model.ComplexTypes.Contains(complexType));
            Assert.False(model.NamespaceItems.Contains(complexType));
        }

        [Fact]
        public void Can_add_remove_entity_type()
        {
            var model = new EdmModel(DataSpace.SSpace);
            var entityType = new EntityType("E", "N", DataSpace.SSpace);

            model.AddItem(entityType);

            Assert.True(model.EntityTypes.Contains(entityType));
            Assert.True(model.NamespaceItems.Contains(entityType));

            model.RemoveItem(entityType);

            Assert.False(model.EntityTypes.Contains(entityType));
            Assert.False(model.NamespaceItems.Contains(entityType));
        }

        [Fact]
        public void Can_add_remove_enum_type()
        {
            var model = new EdmModel(DataSpace.SSpace);
            var enumType = new EnumType { DataSpace = DataSpace.SSpace };

            model.AddItem(enumType);

            Assert.True(model.EnumTypes.Contains(enumType));
            Assert.True(model.NamespaceItems.Contains(enumType));

            model.RemoveItem(enumType);

            Assert.False(model.EnumTypes.Contains(enumType));
            Assert.False(model.NamespaceItems.Contains(enumType));
        }

        [Fact]
        public void Can_add_remove_function()
        {
            var model = new EdmModel(DataSpace.SSpace);
            var function = new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload());

            model.AddItem(function);

            Assert.True(model.Functions.Contains(function));
            Assert.True(model.NamespaceItems.Contains(function));

            model.RemoveItem(function);

            Assert.False(model.Functions.Contains(function));
            Assert.False(model.NamespaceItems.Contains(function));
        }

        [Fact]
        public void CreateStoreModel_creates_model_with_SSpace()
        {
            var providerInfo = ProviderRegistry.Sql2008_ProviderInfo;
            var providerManifest = ProviderRegistry.Sql2008_ProviderManifest;
            var model = EdmModel.CreateStoreModel(providerInfo, providerManifest);

            Assert.Equal(DataSpace.SSpace, model.DataSpace);
            Assert.Same(providerInfo, model.ProviderInfo);
            Assert.Same(providerManifest, model.ProviderManifest);
        }

        [Fact]
        public void CreateStoreModel_sets_container()
        {
            var container = new EntityContainer("MyContainer", DataSpace.SSpace);

            var model = EdmModel.CreateStoreModel(container, null, null);

            Assert.Same(container, model.Containers.Single());
            Assert.Null(model.ProviderManifest);
            Assert.Null(model.ProviderInfo);
        }

        [Fact]
        public void CreateStoreModel_sets_provider_info_and_manifest()
        {
            var container = new EntityContainer("MyContainer", DataSpace.SSpace);
            var providerInfo = ProviderRegistry.Sql2008_ProviderInfo;
            var providerManifest = ProviderRegistry.Sql2008_ProviderManifest;

            var model = EdmModel.CreateStoreModel(container, providerInfo, providerManifest);

            Assert.Same(container, model.Containers.Single());
            Assert.Same(providerInfo, model.ProviderInfo);
            Assert.Same(providerManifest, model.ProviderManifest);
        }

        [Fact]
        public void CreateConceptualModel_creates_model_with_CSpace()
        {
            var model = EdmModel.CreateConceptualModel();

            Assert.Equal(DataSpace.CSpace, model.DataSpace);
            Assert.Null(model.ProviderInfo);
            Assert.Null(model.ProviderManifest);
        }

        [Fact]
        public void CreateConceptualModel_sets_container()
        {
            var container = new EntityContainer("MyContainer", DataSpace.CSpace);

            var model = EdmModel.CreateConceptualModel(container);

            Assert.Same(container, model.Containers.Single());
        }
    }
}
