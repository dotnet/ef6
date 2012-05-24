namespace System.Data.Entity.Spatial
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class DbSpatialDataReaderTests
    {
        [Fact]
        public void GetGeographyAsync_returns_cancelled_Task_when_requested()
        {
            var dbSpatialDataReader = new Mock<DbSpatialDataReader> { CallBase = true }.Object;
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var task = dbSpatialDataReader.GetGeographyAsync(0, cancellationTokenSource.Token);

            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void GetGeographyAsync_wraps_exceptions()
        {
            var mockDbSpatialDataReader = new Mock<DbSpatialDataReader> { CallBase = true };
            var exception = new InvalidOperationException();
            mockDbSpatialDataReader.Setup(m => m.GetGeography(0)).Throws(exception);

            var task = mockDbSpatialDataReader.Object.GetGeographyAsync(0, CancellationToken.None);

            Assert.Same(exception, task.Exception.InnerExceptions.Single());
        }

        [Fact]
        public void GetGeographyAsync_calls_GetGeography()
        {
            var mockDbSpatialDataReader = new Mock<DbSpatialDataReader> { CallBase = true };
            var dbGeography = new DbGeography();
            mockDbSpatialDataReader.Setup(m => m.GetGeography(0)).Returns((int ordinal) => dbGeography).Verifiable();

            var task = mockDbSpatialDataReader.Object.GetGeographyAsync(0, CancellationToken.None);

            mockDbSpatialDataReader.Verify(m => m.GetGeography(0), Times.Once());
            Assert.Same(dbGeography, task.Result);
        }

        [Fact]
        public void GetGeographyAsync_calls_CancellationToken_overload()
        {
            var mockDbSpatialDataReader = new Mock<DbSpatialDataReader> { CallBase = true };
            var dbGeography = new DbGeography();
            mockDbSpatialDataReader.Setup(m => m.GetGeographyAsync(0, CancellationToken.None))
                .Returns(Task.FromResult(dbGeography))
                .Verifiable();

            var task = mockDbSpatialDataReader.Object.GetGeographyAsync(0);

            mockDbSpatialDataReader.Verify(m => m.GetGeographyAsync(0, CancellationToken.None), Times.Once());
            Assert.Same(dbGeography, task.Result);
        }

        [Fact]
        public void GetGeometryAsync_returns_cancelled_Task_when_requested()
        {
            var dbSpatialDataReader = new Mock<DbSpatialDataReader> { CallBase = true }.Object;
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var task = dbSpatialDataReader.GetGeometryAsync(0, cancellationTokenSource.Token);

            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void GetGeometryAsync_wraps_exceptions()
        {
            var mockDbSpatialDataReader = new Mock<DbSpatialDataReader> { CallBase = true };
            var exception = new InvalidOperationException();
            mockDbSpatialDataReader.Setup(m => m.GetGeometry(0)).Throws(exception);

            var task = mockDbSpatialDataReader.Object.GetGeometryAsync(0, CancellationToken.None);

            Assert.Same(exception, task.Exception.InnerExceptions.Single());
        }

        [Fact]
        public void GetGeometryAsync_calls_GetGeometry()
        {
            var mockDbSpatialDataReader = new Mock<DbSpatialDataReader> { CallBase = true };
            var dbGeometry = new DbGeometry();
            mockDbSpatialDataReader.Setup(m => m.GetGeometry(0)).Returns((int ordinal) => dbGeometry).Verifiable();

            var task = mockDbSpatialDataReader.Object.GetGeometryAsync(0, CancellationToken.None);

            mockDbSpatialDataReader.Verify(m => m.GetGeometry(0), Times.Once());
            Assert.Same(dbGeometry, task.Result);
        }

        [Fact]
        public void GetGeometryAsync_calls_CancellationToken_overload()
        {
            var mockDbSpatialDataReader = new Mock<DbSpatialDataReader> { CallBase = true };
            var dbGeometry = new DbGeometry();
            mockDbSpatialDataReader.Setup(m => m.GetGeometryAsync(0, CancellationToken.None))
                .Returns(Task.FromResult(dbGeometry))
                .Verifiable();

            var task = mockDbSpatialDataReader.Object.GetGeometryAsync(0);

            mockDbSpatialDataReader.Verify(m => m.GetGeometryAsync(0, CancellationToken.None), Times.Once());
            Assert.Same(dbGeometry, task.Result);
        }
    }
}
