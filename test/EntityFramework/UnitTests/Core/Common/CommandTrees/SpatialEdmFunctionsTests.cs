// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Spatial;
    using System.Data.Entity.Spatial;
    using Xunit;

    public class SpatialEdmFunctionsTests
    {
            [Fact]
            public void GeometryFromText_produces_correct_EdmFunction()
            {
                var argument = DbExpressionBuilder.Constant("POINT(1 1)");
                var function = SpatialEdmFunctions.GeometryFromText(argument);

                Assert.Equal("GeometryFromText", function.Function.FunctionName);
                Assert.Same(argument, function.Arguments[0]);
            }

            [Fact]
            public void GeometryFromText_with_coordinate_system_id_produce_correct_EdmFunctions()
            {
                GeometryFromTextHelper(SpatialEdmFunctions.GeometryFromText, "GeometryFromText");
                GeometryFromTextHelper(SpatialEdmFunctions.GeometryPointFromText, "GeometryPointFromText");
                GeometryFromTextHelper(SpatialEdmFunctions.GeometryLineFromText, "GeometryLineFromText");
                GeometryFromTextHelper(SpatialEdmFunctions.GeometryPolygonFromText, "GeometryPolygonFromText");
                GeometryFromTextHelper(SpatialEdmFunctions.GeometryMultiPointFromText, "GeometryMultiPointFromText");
                GeometryFromTextHelper(SpatialEdmFunctions.GeometryMultiLineFromText, "GeometryMultiLineFromText");
                GeometryFromTextHelper(SpatialEdmFunctions.GeometryMultiPolygonFromText, "GeometryMultiPolygonFromText");
                GeometryFromTextHelper(SpatialEdmFunctions.GeometryCollectionFromText, "GeometryCollectionFromText");
            }

            [Fact]
            public void GeometryFromBinary_produces_correct_EdmFunction()
            {
                var argument = DbExpressionBuilder.Constant(new byte[] { 0x00, 0x01 });
                var function = SpatialEdmFunctions.GeometryFromBinary(argument);

                Assert.Equal("GeometryFromBinary", function.Function.FunctionName);
                Assert.Same(argument, function.Arguments[0]);
            }

            [Fact]
            public void GeometryFromBinary_with_coordinate_system_id_produce_correct_EdmFunctions()
            {
                GeometryFromBinaryHelper(SpatialEdmFunctions.GeometryFromBinary, "GeometryFromBinary");
                GeometryFromBinaryHelper(SpatialEdmFunctions.GeometryPointFromBinary, "GeometryPointFromBinary");
                GeometryFromBinaryHelper(SpatialEdmFunctions.GeometryLineFromBinary, "GeometryLineFromBinary");
                GeometryFromBinaryHelper(SpatialEdmFunctions.GeometryPolygonFromBinary, "GeometryPolygonFromBinary");
                GeometryFromBinaryHelper(SpatialEdmFunctions.GeometryMultiPointFromBinary, "GeometryMultiPointFromBinary");
                GeometryFromBinaryHelper(SpatialEdmFunctions.GeometryMultiLineFromBinary, "GeometryMultiLineFromBinary");
                GeometryFromBinaryHelper(SpatialEdmFunctions.GeometryMultiPolygonFromBinary, "GeometryMultiPolygonFromBinary");
                GeometryFromBinaryHelper(SpatialEdmFunctions.GeometryCollectionFromBinary, "GeometryCollectionFromBinary");
            }

            [Fact]
            public void GeometryFromGml_produces_correct_EdmFunction()
            {
                var argument = DbExpressionBuilder.Constant("<gml>some GML</gml>");
                var function = SpatialEdmFunctions.GeometryFromGml(argument);

                Assert.Equal("GeometryFromGml", function.Function.FunctionName);
                Assert.Same(argument, function.Arguments[0]);
            }

            [Fact]
            public void GeometryFromGml_with_coordinate_id_produces_correct_EdmFunction()
            {
                var argument1 = DbExpressionBuilder.Constant("<gml>some GML</gml>");
                var argument2 = DbExpressionBuilder.Constant(1);
                var function = SpatialEdmFunctions.GeometryFromGml(argument1, argument2);

                Assert.Equal("GeometryFromGml", function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
            }

            [Fact]
            public void GeographyFromText_produces_correct_EdmFunction()
            {
                var argument = DbExpressionBuilder.Constant("POINT(1 1)");
                var function = SpatialEdmFunctions.GeographyFromText(argument);

                Assert.Equal("GeographyFromText", function.Function.FunctionName);
                Assert.Same(argument, function.Arguments[0]);
            }

            [Fact]
            public void GeographyFromText_with_coordinate_system_id_produce_correct_EdmFunctions()
            {
                GeographyFromTextHelper(SpatialEdmFunctions.GeographyFromText, "GeographyFromText");
                GeographyFromTextHelper(SpatialEdmFunctions.GeographyPointFromText, "GeographyPointFromText");
                GeographyFromTextHelper(SpatialEdmFunctions.GeographyLineFromText, "GeographyLineFromText");
                GeographyFromTextHelper(SpatialEdmFunctions.GeographyPolygonFromText, "GeographyPolygonFromText");
                GeographyFromTextHelper(SpatialEdmFunctions.GeographyMultiPointFromText, "GeographyMultiPointFromText");
                GeographyFromTextHelper(SpatialEdmFunctions.GeographyMultiLineFromText, "GeographyMultiLineFromText");
                GeographyFromTextHelper(SpatialEdmFunctions.GeographyMultiPolygonFromText, "GeographyMultiPolygonFromText");
                GeographyFromTextHelper(SpatialEdmFunctions.GeographyCollectionFromText, "GeographyCollectionFromText");
            }

            [Fact]
            public void GeographyFromBinary_produces_correct_EdmFunction()
            {
                var argument = DbExpressionBuilder.Constant(new byte[] { 0x00, 0x01 });
                var function = SpatialEdmFunctions.GeographyFromBinary(argument);

                Assert.Equal("GeographyFromBinary", function.Function.FunctionName);
                Assert.Same(argument, function.Arguments[0]);
            }

            [Fact]
            public void GeographyFromBinary_with_coordinate_system_id_produce_correct_EdmFunctions()
            {
                GeographyFromBinaryHelper(SpatialEdmFunctions.GeographyFromBinary, "GeographyFromBinary");
                GeographyFromBinaryHelper(SpatialEdmFunctions.GeographyPointFromBinary, "GeographyPointFromBinary");
                GeographyFromBinaryHelper(SpatialEdmFunctions.GeographyLineFromBinary, "GeographyLineFromBinary");
                GeographyFromBinaryHelper(SpatialEdmFunctions.GeographyPolygonFromBinary, "GeographyPolygonFromBinary");
                GeographyFromBinaryHelper(SpatialEdmFunctions.GeographyMultiPointFromBinary, "GeographyMultiPointFromBinary");
                GeographyFromBinaryHelper(SpatialEdmFunctions.GeographyMultiLineFromBinary, "GeographyMultiLineFromBinary");
                GeographyFromBinaryHelper(SpatialEdmFunctions.GeographyMultiPolygonFromBinary, "GeographyMultiPolygonFromBinary");
                GeographyFromBinaryHelper(SpatialEdmFunctions.GeographyCollectionFromBinary, "GeographyCollectionFromBinary");
            }

            [Fact]
            public void GeographyFromGml_produces_correct_EdmFunction()
            {
                var argument = DbExpressionBuilder.Constant("<gml>some GML</gml>");
                var function = SpatialEdmFunctions.GeographyFromGml(argument);

                Assert.Equal("GeographyFromGml", function.Function.FunctionName);
                Assert.Same(argument, function.Arguments[0]);
            }

            [Fact]
            public void GeographyFromGml_with_coordinate_id_produces_correct_EdmFunction()
            {
                var argument1 = DbExpressionBuilder.Constant("<gml>some GML</gml>");
                var argument2 = DbExpressionBuilder.Constant(1);
                var function = SpatialEdmFunctions.GeographyFromGml(argument1, argument2);

                Assert.Equal("GeographyFromGml", function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
            }

            [Fact]
            public void Unary_spatial_functions_produce_correct_EdmFunction()
            {
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.CoordinateSystemId, "CoordinateSystemId");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.SpatialTypeName, "SpatialTypeName");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.SpatialDimension, "SpatialDimension");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.SpatialEnvelope, "SpatialEnvelope");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.AsBinary, "AsBinary");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.AsGml, "AsGml");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.AsText, "AsText");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.IsEmptySpatial, "IsEmptySpatial");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.IsSimpleGeometry, "IsSimpleGeometry");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.SpatialBoundary, "SpatialBoundary");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.IsValidGeometry, "IsValidGeometry");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.CoordinateSystemId, "CoordinateSystemId");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.CoordinateSystemId, "CoordinateSystemId");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.SpatialConvexHull, "SpatialConvexHull");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.SpatialElementCount, "SpatialElementCount");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.XCoordinate, "XCoordinate");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.YCoordinate, "YCoordinate");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.Elevation, "Elevation");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.Measure, "Measure");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.SpatialLength, "SpatialLength");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.StartPoint, "StartPoint");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.EndPoint, "EndPoint");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.IsClosedSpatial, "IsClosedSpatial");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.IsRing, "IsRing");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.PointCount, "PointCount");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.Area, "Area");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.Centroid, "Centroid");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.PointOnSurface, "PointOnSurface");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.ExteriorRing, "ExteriorRing");
                UnarySpatialFunctionsHelper(SpatialEdmFunctions.InteriorRingCount, "InteriorRingCount");
            }

            [Fact]
            public void Unary_geograpgy_functions_produce_correct_EdmFunction()
            {
                UnaryGeographySpatialFunctionsHelper(SpatialEdmFunctions.Latitude, "Latitude");
                UnaryGeographySpatialFunctionsHelper(SpatialEdmFunctions.Longitude, "Longitude");
            }

            [Fact]
            public void Binary_spatial_functions_produce_correct_EdmFunctions()
            {
                SpatialRelationHelper(SpatialEdmFunctions.SpatialEquals, "SpatialEquals");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialDisjoint, "SpatialDisjoint");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialIntersects, "SpatialIntersects");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialTouches, "SpatialTouches");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialCrosses, "SpatialCrosses");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialWithin, "SpatialWithin");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialContains, "SpatialContains");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialOverlaps, "SpatialOverlaps");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialEquals, "SpatialEquals");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialEquals, "SpatialEquals");
                SpatialRelationHelper(SpatialEdmFunctions.Distance, "Distance");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialIntersection, "SpatialIntersection");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialUnion, "SpatialUnion");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialDifference, "SpatialDifference");
                SpatialRelationHelper(SpatialEdmFunctions.SpatialSymmetricDifference, "SpatialSymmetricDifference");
            }

            [Fact]
            public void SpatialRelate_produces_correct_EdmFunction()
            {
                var argument1 = DbExpressionBuilder.Constant(DbGeometry.FromText("POINT(1 1)"));
                var argument2 = DbExpressionBuilder.Constant(DbGeometry.FromText("POINT(2 2)"));
                var argument3 = DbExpressionBuilder.Constant("intersection pattern");
                var function = SpatialEdmFunctions.SpatialRelate(argument1, argument2, argument3);

                Assert.Equal("SpatialRelate", function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
                Assert.Same(argument3, function.Arguments[2]);
            }

            [Fact]
            public void Spatial_functions_with_integer_argument_produce_correct_EdmFunctions()
            {
                SpatialFunctionWithIntegerArgumentHelper(SpatialEdmFunctions.SpatialBuffer, "SpatialBuffer");
                SpatialFunctionWithIntegerArgumentHelper(SpatialEdmFunctions.SpatialElementAt, "SpatialElementAt");
                SpatialFunctionWithIntegerArgumentHelper(SpatialEdmFunctions.PointAt, "PointAt");
                SpatialFunctionWithIntegerArgumentHelper(SpatialEdmFunctions.InteriorRingAt, "InteriorRingAt");
            }

            private void GeometryFromTextHelper(
                Func<DbExpression, DbExpression, DbFunctionExpression> operation,
                string functionName)
            {
                var argument1 = DbExpressionBuilder.Constant("SPATIAL VALUE");
                var argument2 = DbExpressionBuilder.Constant(1);
                var function = operation(argument1, argument2);

                Assert.Equal(functionName, function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
            }

            private void GeometryFromBinaryHelper(
                Func<DbExpression, DbExpression, DbFunctionExpression> operation,
                string functionName)
            {
                var argument1 = DbExpressionBuilder.Constant(new byte[] { 0x00, 0x01 });
                var argument2 = DbExpressionBuilder.Constant(1);
                var function = operation(argument1, argument2);

                Assert.Equal(functionName, function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
            }

            private void GeographyFromTextHelper(
                Func<DbExpression, DbExpression, DbFunctionExpression> operation,
                string functionName)
            {
                var argument1 = DbExpressionBuilder.Constant("SPATIAL VALUE");
                var argument2 = DbExpressionBuilder.Constant(1);
                var function = operation(argument1, argument2);

                Assert.Equal(functionName, function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
            }

            private void GeographyFromBinaryHelper(
                Func<DbExpression, DbExpression, DbFunctionExpression> operation,
                string functionName)
            {
                var argument1 = DbExpressionBuilder.Constant(new byte[] { 0x00, 0x01 });
                var argument2 = DbExpressionBuilder.Constant(1);
                var function = operation(argument1, argument2);

                Assert.Equal(functionName, function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
            }

            private void UnarySpatialFunctionsHelper(Func<DbExpression, DbFunctionExpression> operation, string functionName)
            {
                var argument = DbExpressionBuilder.Constant(DbGeometry.FromText("POINT(1 1)"));
                var function = operation(argument);

                Assert.Equal(functionName, function.Function.FunctionName);
                Assert.Same(argument, function.Arguments[0]);
            }

            private void UnaryGeographySpatialFunctionsHelper(
                Func<DbExpression, DbFunctionExpression> operation,
                string functionName)
            {
                var argument = DbExpressionBuilder.Constant(DbGeography.FromText("POINT(1 1)"));
                var function = operation(argument);

                Assert.Equal(functionName, function.Function.FunctionName);
                Assert.Same(argument, function.Arguments[0]);
            }

            private void SpatialRelationHelper(
                Func<DbConstantExpression, DbConstantExpression, DbFunctionExpression> opertaion,
                string functionName)
            {
                var argument1 = DbExpressionBuilder.Constant(DbGeometry.FromText("POINT(1 1)"));
                var argument2 = DbExpressionBuilder.Constant(DbGeometry.FromText("POINT(2 2)"));
                var function = opertaion(argument1, argument2);

                Assert.Equal(functionName, function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
            }

            private void SpatialFunctionWithIntegerArgumentHelper(
                Func<DbExpression, DbExpression, DbFunctionExpression> operation,
                string functionName)
            {
                var argument1 = DbExpressionBuilder.Constant(DbGeometry.FromText("POINT(1 1)"));
                var argument2 = DbExpressionBuilder.Constant(1);
                var function = operation(argument1, argument2);

                Assert.Equal(functionName, function.Function.FunctionName);
                Assert.Same(argument1, function.Arguments[0]);
                Assert.Same(argument2, function.Arguments[1]);
            }
    }
}
