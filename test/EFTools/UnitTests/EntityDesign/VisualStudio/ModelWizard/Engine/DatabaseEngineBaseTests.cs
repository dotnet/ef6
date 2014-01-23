namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Moq;
    using Xunit;

    public class DatabaseEngineBaseTests
    {
        private class DatabaseEngineBaseMethodInvoker : DatabaseEngineBase
        {
            public bool InvokeCanCreateAndOpenConnection(StoreSchemaConnectionFactory connectionFactory, 
                string providerInvariantName, string designTimeInvariantName, string designTimeConnectionString)
            {
                return CanCreateAndOpenConnection(
                    connectionFactory, providerInvariantName, designTimeInvariantName, designTimeConnectionString);
            }
        }

        [Fact]
        public void CanCreateAndOpenConnection_returns_true_for_valid_connection()
        {
            var mockEntityConnection = new Mock<EntityConnection>();
            var mockConnectionFactory = new Mock<StoreSchemaConnectionFactory>();

            Version version;
            mockConnectionFactory
                .Setup(
                    f => f.Create(
                        It.IsAny<IDbDependencyResolver>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<Version>(), out version))
                .Returns(mockEntityConnection.Object);

            Assert.True(
                new DatabaseEngineBaseMethodInvoker().InvokeCanCreateAndOpenConnection(
                    mockConnectionFactory.Object, "fakeInvariantName", "fakeInvariantName", "fakeConnectionString"));
        }

        [Fact]
        public void CanCreateAndOpenConnection_returns_false_for_invalid_connection()
        {
            var mockEntityConnection = new Mock<EntityConnection>();
            mockEntityConnection.Setup(c => c.Open()).Throws<InvalidOperationException>();

            var mockConnectionFactory = new Mock<StoreSchemaConnectionFactory>();

            Version version;
            mockConnectionFactory
                .Setup(
                    f => f.Create(
                        It.IsAny<IDbDependencyResolver>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<Version>(), out version))
                .Returns(mockEntityConnection.Object);

            Assert.False(
                new DatabaseEngineBaseMethodInvoker().InvokeCanCreateAndOpenConnection(
                    mockConnectionFactory.Object, "fakeInvariantName", "fakeInvariantName", "fakeConnectionString"));
        }

        [Fact]
        public void CanCreateAndOpenConnection_passes_the_latest_EF_version_as_the_max_version()
        {
            var mockEntityConnection = new Mock<EntityConnection>();
            var mockConnectionFactory = new Mock<StoreSchemaConnectionFactory>();

            Version version;
            mockConnectionFactory
                .Setup(
                    f => f.Create(
                        It.IsAny<IDbDependencyResolver>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<Version>(), out version))
                .Returns(mockEntityConnection.Object);

            new DatabaseEngineBaseMethodInvoker().InvokeCanCreateAndOpenConnection(
                mockConnectionFactory.Object, "fakeInvariantName", "fakeInvariantName", "fakeConnectionString");

            mockConnectionFactory.Verify(
                f => f.Create(
                    It.IsAny<IDbDependencyResolver>(),
                    It.Is<string>(s => s == "fakeInvariantName"),
                    It.Is<string>(s => s == "fakeConnectionString"),
                    It.Is<Version>(v => v == EntityFrameworkVersion.Latest),
                    out version),
                Times.Once());
        }
    }
}