// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using Xunit;

    public class EdmModelTests
    {
        [Fact]
        public void Can_create_only_CSpace_or_SSpace_model()
        {
            foreach (DataSpace dataSpace in Enum.GetValues(typeof(DataSpace)))
            {
                if (dataSpace != DataSpace.CSpace && dataSpace != DataSpace.SSpace)
                {
                    var exception = Assert.Throws<ArgumentException>(() => new EdmModel(dataSpace));

                    Assert.Equal("dataSpace", exception.ParamName);
                    Assert.True(exception.Message.StartsWith(Strings.EdmModel_InvalidDataSpace(dataSpace)));
                }
            }
        }

        [Fact]
        public void Custom_container_name_set_correctly()
        {
            Assert.Equal(
                "Unicorns420", 
                new EdmModel(DataSpace.CSpace, "Unicorns420").Containers.Single().Name);
        }

        [Fact]
        public void EdmModel_version_correctly()
        {
            Assert.Equal(2.0, new EdmModel(DataSpace.CSpace, 2.0).Version);
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

            model.AddItem(new EntityType());

            Assert.Throws<ModelValidationException>(() => model.Validate());
        }

        [Fact]
        public void Can_get_and_set_provider_manifest()
        {
            var model = new EdmModel(DataSpace.SSpace);

            Assert.Null(model.ProviderManifest);

            var providerManifest = new SqlProviderManifest("2008");

            model.ProviderManifest = providerManifest;

            Assert.Same(providerManifest, model.ProviderManifest);
        }

        [Fact]
        public void Can_get_and_set_provider_info()
        {
            var model = new EdmModel(DataSpace.SSpace);

            Assert.Null(model.ProviderInfo);

            var providerInfo = ProviderRegistry.Sql2008_ProviderInfo;

            model.ProviderInfo = providerInfo;

            Assert.Same(providerInfo, model.ProviderInfo);
        }

        [Fact]
        public void Cannot_add_entity_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.CSpace)
                        .AddItem(new EntityType("Entity", "Model", DataSpace.SSpace)));

            Assert.Equal("entityType", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }

        [Fact]
        public void Cannot_add_enum_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.SSpace)
                        .AddItem(new EnumType() { DataSpace = DataSpace.CSpace }));

            Assert.Equal("enumType", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }

        [Fact]
        public void Cannot_add_complextype_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.SSpace)
                        .AddItem(new ComplexType("ComplexType", "Model", DataSpace.CSpace)));

            Assert.Equal("complexType", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }

        [Fact]
        public void Cannot_add_association_from_different_data_space()
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => new EdmModel(DataSpace.SSpace)
                        .AddItem(new AssociationType("AssociationType", "Model", /*foreignKey*/ false, DataSpace.CSpace)));

            Assert.Equal("associationType", exception.ParamName);
            Assert.True(exception.Message.StartsWith(Strings.EdmModel_AddItem_NonMatchingNamespace));
        }
    }
}
