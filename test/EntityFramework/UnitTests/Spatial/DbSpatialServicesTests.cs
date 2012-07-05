namespace System.Data.Entity.Spatial
{
    using Moq;
    using Xunit;

    public class DbSpatialServicesTests
    {
        [Fact]
        public void NativeTypesAvailable_returns_true()
        {
            Assert.True(
                new Mock<DbSpatialServices>
                    {
                        CallBase = true
                    }.Object.NativeTypesAvailable);
        }
    }
}