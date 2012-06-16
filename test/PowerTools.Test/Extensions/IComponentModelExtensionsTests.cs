namespace Microsoft.DbContextPackage.Extensions
{
    using Microsoft.VisualStudio.ComponentModelHost;
    using Moq;
    using Xunit;

    public class IComponentModelExtensionsTests
    {
        [Fact]
        public void GetService_calls_generic_version()
        {
            var componentModelMock = new Mock<IComponentModel>();
            var componentModel = componentModelMock.Object;

            componentModel.GetService(typeof(string));

            componentModelMock.Verify(cm => cm.GetService<string>(), Times.Once());
        }
    }
}
