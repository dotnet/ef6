namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping.UnitTests
{
    using System.Data.Entity.Edm.Db.Mapping;
    using Xunit;

    public sealed class DbAssociationSetMappingExtensionsTests
    {
        [Fact]
        public void Initialize_should_initialize_ends()
        {
            var associationSetMapping = new DbAssociationSetMapping().Initialize();

            Assert.NotNull(associationSetMapping.SourceEndMapping);
            Assert.NotNull(associationSetMapping.TargetEndMapping);
        }

        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var associationSetMapping = new DbAssociationSetMapping();

            associationSetMapping.SetConfiguration(42);

            Assert.Equal(42, associationSetMapping.GetConfiguration());
        }
    }
}