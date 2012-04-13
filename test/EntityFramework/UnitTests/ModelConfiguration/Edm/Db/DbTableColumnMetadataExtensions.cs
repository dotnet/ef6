namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Data.Entity.Edm.Db;
    using Xunit;

    public sealed class DbTableColumnMetadataExtensions
    {
        [Fact]
        public void Initialize_should_initialize_facets()
        {
            var tableColumn = new DbTableColumnMetadata();

            tableColumn.Initialize();

            Assert.NotNull(tableColumn.Facets);
        }

        [Fact]
        public void Can_get_and_set_can_override_annotation()
        {
            var tableColumn = new DbTableColumnMetadata();

            tableColumn.SetAllowOverride(true);

            Assert.True(tableColumn.GetAllowOverride());

            tableColumn.SetAllowOverride(false);

            Assert.False(tableColumn.GetAllowOverride());
        }
    }
}