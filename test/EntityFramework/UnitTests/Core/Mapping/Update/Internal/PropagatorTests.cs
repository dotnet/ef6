// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Spatial;
    using Xunit;

    public class PropagatorTests
    {
        public class ExtentPlaceholderCreatorTests
        {
            [Fact]
            public static void GetPropagatorResultForPrimitiveType_outputs_expected_results_for_basic_types()
            {
                Check_result_for_basic_type(PrimitiveTypeKind.Binary, new Byte[0]);
                Check_result_for_basic_type(PrimitiveTypeKind.Boolean, default(Boolean));
                Check_result_for_basic_type(PrimitiveTypeKind.Byte, default(Byte));
                Check_result_for_basic_type(PrimitiveTypeKind.DateTime, default(DateTime));
                Check_result_for_basic_type(PrimitiveTypeKind.Time, default(TimeSpan));
                Check_result_for_basic_type(PrimitiveTypeKind.DateTimeOffset, default(DateTimeOffset));
                Check_result_for_basic_type(PrimitiveTypeKind.Decimal, default(Decimal));
                Check_result_for_basic_type(PrimitiveTypeKind.Double, default(Double));
                Check_result_for_basic_type(PrimitiveTypeKind.Guid, default(Guid));
                Check_result_for_basic_type(PrimitiveTypeKind.Int16, default(Int16));
                Check_result_for_basic_type(PrimitiveTypeKind.Int32, default(Int32));
                Check_result_for_basic_type(PrimitiveTypeKind.Int64, default(Int64));
                Check_result_for_basic_type(PrimitiveTypeKind.Single, default(Single));
                Check_result_for_basic_type(PrimitiveTypeKind.SByte, default(SByte));
                Check_result_for_basic_type(PrimitiveTypeKind.String, string.Empty);
            }

            private static void Check_result_for_basic_type(PrimitiveTypeKind primitiveTypeKind, object defaultValue)
            {
                var primitiveType = PrimitiveType.GetEdmPrimitiveType(primitiveTypeKind);
                PropagatorResult result;

                Propagator.ExtentPlaceholderCreator.GetPropagatorResultForPrimitiveType(primitiveType, out result);

                Assert.NotNull(result);
                Assert.Equal(true, result.IsSimple);
                Assert.Equal(defaultValue, result.GetSimpleValue());
            }

            [Fact]
            public static void GetPropagatorResultForPrimitiveType_outputs_expected_results_for_geometry_types()
            {
                Check_result_for_geometry_type(PrimitiveTypeKind.Geometry, "POINT EMPTY");
                Check_result_for_geometry_type(PrimitiveTypeKind.GeometryPoint, "POINT EMPTY");
                Check_result_for_geometry_type(PrimitiveTypeKind.GeometryLineString, "LINESTRING EMPTY");
                Check_result_for_geometry_type(PrimitiveTypeKind.GeometryPolygon, "POLYGON EMPTY");
                Check_result_for_geometry_type(PrimitiveTypeKind.GeometryMultiPoint, "MULTIPOINT EMPTY");
                Check_result_for_geometry_type(PrimitiveTypeKind.GeometryMultiLineString, "MULTILINESTRING EMPTY");
                Check_result_for_geometry_type(PrimitiveTypeKind.GeometryMultiPolygon, "MULTIPOLYGON EMPTY");
                Check_result_for_geometry_type(PrimitiveTypeKind.GeometryCollection, "GEOMETRYCOLLECTION EMPTY");
            }

            private static void Check_result_for_geometry_type(PrimitiveTypeKind primitiveTypeKind, object wellKnownText)
            {
                var primitiveType = PrimitiveType.GetEdmPrimitiveType(primitiveTypeKind);
                PropagatorResult result;

                Propagator.ExtentPlaceholderCreator.GetPropagatorResultForPrimitiveType(primitiveType, out result);

                Assert.NotNull(result);
                Assert.Equal(true, result.IsSimple);

                var geometry = result.GetSimpleValue() as DbGeometry;

                Assert.NotNull(geometry);
                Assert.NotNull(geometry.WellKnownValue);
                Assert.Equal(wellKnownText, geometry.WellKnownValue.WellKnownText);
            }

            [Fact]
            public static void GetPropagatorResultForPrimitiveType_outputs_expected_results_for_geography_types()
            {
                Check_result_for_geography_type(PrimitiveTypeKind.Geography, "POINT EMPTY");
                Check_result_for_geography_type(PrimitiveTypeKind.GeographyPoint, "POINT EMPTY");
                Check_result_for_geography_type(PrimitiveTypeKind.GeographyLineString, "LINESTRING EMPTY");
                Check_result_for_geography_type(PrimitiveTypeKind.GeographyPolygon, "POLYGON EMPTY");
                Check_result_for_geography_type(PrimitiveTypeKind.GeographyMultiPoint, "MULTIPOINT EMPTY");
                Check_result_for_geography_type(PrimitiveTypeKind.GeographyMultiLineString, "MULTILINESTRING EMPTY");
                Check_result_for_geography_type(PrimitiveTypeKind.GeographyMultiPolygon, "MULTIPOLYGON EMPTY");
                Check_result_for_geography_type(PrimitiveTypeKind.GeographyCollection, "GEOMETRYCOLLECTION EMPTY");
            }

            private static void Check_result_for_geography_type(PrimitiveTypeKind primitiveTypeKind, object wellKnownText)
            {
                var primitiveType = PrimitiveType.GetEdmPrimitiveType(primitiveTypeKind);
                PropagatorResult result;

                Propagator.ExtentPlaceholderCreator.GetPropagatorResultForPrimitiveType(primitiveType, out result);

                Assert.NotNull(result);
                Assert.Equal(true, result.IsSimple);

                var geography = result.GetSimpleValue() as DbGeography;

                Assert.NotNull(geography);
                Assert.NotNull(geography.WellKnownValue);
                Assert.Equal(wellKnownText, geography.WellKnownValue.WellKnownText);
            }
        }
    }
}
