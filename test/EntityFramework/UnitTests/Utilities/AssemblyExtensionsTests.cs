namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Migrations;
    using Xunit;

    public class AssemblyExtensionsTests
    {
        [Fact]
        public void GetInformationalVersion_returns_the_informational_version()
        {
            Assert.Equal("6.0.0-beta1", typeof(DbMigrator).Assembly.GetInformationalVersion());
        }
    }
}
