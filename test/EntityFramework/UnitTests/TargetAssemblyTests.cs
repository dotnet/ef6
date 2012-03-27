namespace UnitTests
{
    using System.Data.Entity;
    using System.Linq;
    using System.Security;
    using Xunit;

    public class TargetAssemblyTests
    {
        [Fact]
        public void EntityFramework_assembly_is_security_transparent()
        {
            Assert.Equal(1, typeof(DbContext).Assembly.GetCustomAttributes(true).OfType<SecurityTransparentAttribute>().Count());
        }
    }
}