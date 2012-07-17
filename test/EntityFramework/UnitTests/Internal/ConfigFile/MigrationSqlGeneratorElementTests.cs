namespace System.Data.Entity.Internal.ConfigFile
{
    using Xunit;

    public class MigrationSqlGeneratorElementTests : TestBase
    {
        [Fact]
        public void Type_name_can_be_accessed()
        {
            var element = new MigrationSqlGeneratorElement
                {
                    SqlGeneratorTypeName = "Everyone's At It"
                };

            Assert.Equal("Everyone's At It", element.SqlGeneratorTypeName);
        }
    }
}
