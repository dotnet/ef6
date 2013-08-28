
namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.Mapping;
    using System.Linq;
    using Xunit;

    public class DbDatabaseMappingTests
    {
        [Fact]
        public void Can_set_and_get_Model()
        {
            var mapping = new DbDatabaseMapping();
            
            Assert.Null(mapping.Model);

            var model = new EdmModel(DataSpace.CSpace);
            mapping.Model = model;

            Assert.Same(model, mapping.Model);
        }

        [Fact]
        public void Can_set_and_get_Database()
        {
            var mapping = new DbDatabaseMapping();

            Assert.Null(mapping.Database);

            var storeModel = new EdmModel(DataSpace.SSpace);
            mapping.Database = storeModel;

            Assert.Same(storeModel, mapping.Database);
        }

        [Fact]
        public void Can_add_container_mappings()
        {
            var mapping = new DbDatabaseMapping();

            Assert.Empty(mapping.EntityContainerMappings);

            var containerMapping = new EntityContainerMapping(new EntityContainer());
            mapping.AddEntityContainerMapping(containerMapping);

            Assert.Same(containerMapping, mapping.EntityContainerMappings.Single());
        }

        [Fact]
        public void Can_not_add_null_container_mapping()
        {
            Assert.Equal(
                "entityContainerMapping",
                Assert.Throws<ArgumentNullException>(
                    () => new DbDatabaseMapping().AddEntityContainerMapping(null)).ParamName);
        }
    }
}
