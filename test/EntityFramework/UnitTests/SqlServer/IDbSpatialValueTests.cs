namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using Moq;
    using Xunit;

    public class IDbSpatialValueTests
    {
        [Fact]
        public void WellKnownText_for_DbGeography_uses_AsTextIncludingElevationAndMeasure_if_available()
        {
            var mockProvider = new Mock<DbSpatialServices>();
            var mockGeography = new Mock<DbGeography>();
            mockGeography.Setup(m => m.Provider).Returns(mockProvider.Object);
            mockProvider.Setup(m => m.AsTextIncludingElevationAndMeasure(mockGeography.Object)).Returns("I Am Here!");

            Assert.Equal("I Am Here!", mockGeography.Object.AsSpatialValue().WellKnownText);
        }

        [Fact]
        public void WellKnownText_for_DbGeography_falls_back_to_AsText_if_AsTextIncludingElevationAndMeasure_throws()
        {
            var mockProvider = new Mock<DbSpatialServices>();
            var mockGeography = new Mock<DbGeography>();
            mockGeography.Setup(m => m.Provider).Returns(mockProvider.Object);
            mockProvider.Setup(m => m.AsTextIncludingElevationAndMeasure(mockGeography.Object)).Throws(new NotImplementedException());
            mockGeography.Setup(m => m.AsText()).Returns("I Am Here!");

            Assert.Equal("I Am Here!", mockGeography.Object.AsSpatialValue().WellKnownText);
        }

        [Fact]
        public void WellKnownText_for_DbGeography_falls_back_to_AsText_if_AsTextIncludingElevationAndMeasure_returns_null()
        {
            var mockProvider = new Mock<DbSpatialServices>();
            var mockGeography = new Mock<DbGeography>();
            mockGeography.Setup(m => m.Provider).Returns(mockProvider.Object);
            mockGeography.Setup(m => m.AsText()).Returns("I Am Here!");

            Assert.Equal("I Am Here!", mockGeography.Object.AsSpatialValue().WellKnownText);
        }

        [Fact]
        public void WellKnownText_for_DbGeometry_uses_AsTextIncludingElevationAndMeasure_if_available()
        {
            var mockProvider = new Mock<DbSpatialServices>();
            var mockGeometry = new Mock<DbGeometry>();
            mockGeometry.Setup(m => m.Provider).Returns(mockProvider.Object);
            mockProvider.Setup(m => m.AsTextIncludingElevationAndMeasure(mockGeometry.Object)).Returns("I Am Here!");

            Assert.Equal("I Am Here!", mockGeometry.Object.AsSpatialValue().WellKnownText);
        }

        [Fact]
        public void WellKnownText_for_DbGeometry_falls_back_to_AsText_if_AsTextIncludingElevationAndMeasure_throws()
        {
            var mockProvider = new Mock<DbSpatialServices>();
            var mockGeometry = new Mock<DbGeometry>();
            mockGeometry.Setup(m => m.Provider).Returns(mockProvider.Object);
            mockProvider.Setup(m => m.AsTextIncludingElevationAndMeasure(mockGeometry.Object)).Throws(new NotImplementedException());
            mockGeometry.Setup(m => m.AsText()).Returns("I Am Here!");

            Assert.Equal("I Am Here!", mockGeometry.Object.AsSpatialValue().WellKnownText);
        }

        [Fact]
        public void WellKnownText_for_DbGeometry_falls_back_to_AsText_if_AsTextIncludingElevationAndMeasure_returns_null()
        {
            var mockProvider = new Mock<DbSpatialServices>();
            var mockGeometry = new Mock<DbGeometry>();
            mockGeometry.Setup(m => m.Provider).Returns(mockProvider.Object);
            mockGeometry.Setup(m => m.AsText()).Returns("I Am Here!");

            Assert.Equal("I Am Here!", mockGeometry.Object.AsSpatialValue().WellKnownText);
        }
    }
}
