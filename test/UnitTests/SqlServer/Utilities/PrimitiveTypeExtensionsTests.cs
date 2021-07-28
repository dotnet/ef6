// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Moq;
    using Xunit;

    public class PrimitiveTypeExtensionsTests
    {
        [Fact]
        public void IsSpatialType_returns_true_for_all_spatial_types_only()
        {
            var spatialTypes =
                new[]
                    {
                        PrimitiveTypeKind.Geography,
                        PrimitiveTypeKind.Geometry,
                        PrimitiveTypeKind.GeometryPoint,
                        PrimitiveTypeKind.GeometryLineString,
                        PrimitiveTypeKind.GeometryPolygon,
                        PrimitiveTypeKind.GeometryMultiPoint,
                        PrimitiveTypeKind.GeometryMultiLineString,
                        PrimitiveTypeKind.GeometryMultiPolygon,
                        PrimitiveTypeKind.GeometryCollection,
                        PrimitiveTypeKind.GeographyPoint,
                        PrimitiveTypeKind.GeographyLineString,
                        PrimitiveTypeKind.GeographyPolygon,
                        PrimitiveTypeKind.GeographyMultiPoint,
                        PrimitiveTypeKind.GeographyMultiLineString,
                        PrimitiveTypeKind.GeographyMultiPolygon,
                        PrimitiveTypeKind.GeographyCollection,
                    };

            foreach (var value in Enum.GetValues(typeof(PrimitiveTypeKind)).OfType<PrimitiveTypeKind>())
            {
                var mockType = new Mock<PrimitiveType>();
                mockType.Setup(m => m.PrimitiveTypeKind).Returns(value);

                Assert.Equal(spatialTypes.Contains(value), mockType.Object.IsSpatialType());
            }
        }
    }
}
