// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Spatial
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides an API to construct <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" />s that invoke spatial realted canonical EDM functions, and, where appropriate, allows that API to be accessed as extension methods on the expression type itself.
    /// </summary>
    public static class SpatialEdmFunctions
    {
        #region Spatial Functions - Geometry well known text Constructors

        // Geometry ‘Static’ Functions
        // Geometry – well known text Constructors

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryFromText' function with the specified argument, which must have a string result type. The result type of the expression is Edm.Geometry. Its value has the default coordinate system id (SRID) of the underlying provider.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified value.</returns>
        /// <param name="wellKnownText">An expression that provides the well known text representation of the geometry value.</param>
        public static DbFunctionExpression GeometryFromText(DbExpression wellKnownText)
        {
            Check.NotNull(wellKnownText, "wellKnownText");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromText", wellKnownText);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryFromText' function with the specified arguments. wellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified values.</returns>
        /// <param name="wellKnownText">An expression that provides the well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry value's coordinate system.</param>
        public static DbFunctionExpression GeometryFromText(DbExpression wellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(wellKnownText, "wellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromText", wellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryPointFromText' function with the specified arguments. pointWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry point value based on the specified values.</returns>
        /// <param name="pointWellKnownText">An expression that provides the well known text representation of the geometry point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry point value's coordinate system.</param>
        public static DbFunctionExpression GeometryPointFromText(DbExpression pointWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(pointWellKnownText, "pointWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryPointFromText", pointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryLineFromText' function with the specified arguments. lineWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry line value based on the specified values.</returns>
        /// <param name="lineWellKnownText">An expression that provides the well known text representation of the geometry line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry line value's coordinate system.</param>
        public static DbFunctionExpression GeometryLineFromText(DbExpression lineWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(lineWellKnownText, "lineWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryLineFromText", lineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryPolygonFromText' function with the specified arguments. polygonWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry polygon value based on the specified values.</returns>
        /// <param name="polygonWellKnownText">An expression that provides the well known text representation of the geometry polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry polygon value's coordinate system.</param>
        public static DbFunctionExpression GeometryPolygonFromText(DbExpression polygonWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(polygonWellKnownText, "polygonWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryPolygonFromText", polygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryMultiPointFromText' function with the specified arguments. multiPointWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-point value based on the specified values.</returns>
        /// <param name="multiPointWellKnownText">An expression that provides the well known text representation of the geometry multi-point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-point value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiPointFromText(DbExpression multiPointWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiPointWellKnownText, "multiPointWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPointFromText", multiPointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryMultiLineFromText' function with the specified arguments. multiLineWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-line value based on the specified values.</returns>
        /// <param name="multiLineWellKnownText">An expression that provides the well known text representation of the geometry multi-line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-line value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiLineFromText(DbExpression multiLineWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiLineWellKnownText, "multiLineWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiLineFromText", multiLineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryMultiPolygonFromText' function with the specified arguments. multiPolygonWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-polygon value based on the specified values.</returns>
        /// <param name="multiPolygonWellKnownText">An expression that provides the well known text representation of the geometry multi-polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-polygon value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiPolygonFromText(
            DbExpression multiPolygonWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPolygonFromText", multiPolygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryCollectionFromText' function with the specified arguments. geometryCollectionWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry collection value based on the specified values.</returns>
        /// <param name="geometryCollectionWellKnownText">An expression that provides the well known text representation of the geometry collection value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry collection value's coordinate system.</param>
        public static DbFunctionExpression GeometryCollectionFromText(
            DbExpression geometryCollectionWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(geometryCollectionWellKnownText, "geometryCollectionWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryCollectionFromText", geometryCollectionWellKnownText, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geometry Well Known Binary Constructors

        // Geometry – Well Known Binary Constructors

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryFromBinary' function with the specified argument, which must have a binary result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified binary value.</returns>
        /// <param name="wellKnownBinaryValue">An expression that provides the well known binary representation of the geometry value.</param>
        public static DbFunctionExpression GeometryFromBinary(DbExpression wellKnownBinaryValue)
        {
            Check.NotNull(wellKnownBinaryValue, "wellKnownBinaryValue");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromBinary", wellKnownBinaryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryFromBinary' function with the specified arguments. wellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified values.</returns>
        /// <param name="wellKnownBinaryValue">An expression that provides the well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry value's coordinate system.</param>
        public static DbFunctionExpression GeometryFromBinary(DbExpression wellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(wellKnownBinaryValue, "wellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromBinary", wellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryPointFromBinary' function with the specified arguments. pointWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry point value based on the specified values.</returns>
        /// <param name="pointWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry point value's coordinate system.</param>
        public static DbFunctionExpression GeometryPointFromBinary(DbExpression pointWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(pointWellKnownBinaryValue, "pointWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryPointFromBinary", pointWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryLineFromBinary' function with the specified arguments. lineWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry line value based on the specified values.</returns>
        /// <param name="lineWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry line value's coordinate system.</param>
        public static DbFunctionExpression GeometryLineFromBinary(DbExpression lineWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(lineWellKnownBinaryValue, "lineWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryLineFromBinary", lineWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryPolygonFromBinary' function with the specified arguments. polygonWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry polygon value based on the specified values.</returns>
        /// <param name="polygonWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry polygon value's coordinate system.</param>
        public static DbFunctionExpression GeometryPolygonFromBinary(
            DbExpression polygonWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(polygonWellKnownBinaryValue, "polygonWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryPolygonFromBinary", polygonWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryMultiPointFromBinary' function with the specified arguments. multiPointWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-point value based on the specified values.</returns>
        /// <param name="multiPointWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry multi-point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-point value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiPointFromBinary(
            DbExpression multiPointWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiPointWellKnownBinaryValue, "multiPointWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPointFromBinary", multiPointWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryMultiLineFromBinary' function with the specified arguments. multiLineWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-line value based on the specified values.</returns>
        /// <param name="multiLineWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry multi-line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-line value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiLineFromBinary(
            DbExpression multiLineWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiLineWellKnownBinaryValue, "multiLineWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiLineFromBinary", multiLineWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryMultiPolygonFromBinary' function with the specified arguments. multiPolygonWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-polygon value based on the specified values.</returns>
        /// <param name="multiPolygonWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry multi-polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-polygon value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiPolygonFromBinary(
            DbExpression multiPolygonWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiPolygonWellKnownBinaryValue, "multiPolygonWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction(
                "GeometryMultiPolygonFromBinary", multiPolygonWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryCollectionFromBinary' function with the specified arguments. geometryCollectionWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry collection value based on the specified values.</returns>
        /// <param name="geometryCollectionWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry collection value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry collection value's coordinate system.</param>
        public static DbFunctionExpression GeometryCollectionFromBinary(
            DbExpression geometryCollectionWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(geometryCollectionWellKnownBinaryValue, "geometryCollectionWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction(
                "GeometryCollectionFromBinary", geometryCollectionWellKnownBinaryValue, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geometry GML Constructors (non-OGC)

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryFromGml' function with the specified argument, which must have a string result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified value with the default coordinate system id (SRID) of the underlying provider.</returns>
        /// <param name="geometryMarkup">An expression that provides the Geography Markup Language (GML) representation of the geometry value.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml",
            Justification = "Abbreviation more meaningful than what it stands for")]
        public static DbFunctionExpression GeometryFromGml(DbExpression geometryMarkup)
        {
            Check.NotNull(geometryMarkup, "geometryMarkup");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromGml", geometryMarkup);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeometryFromGml' function with the specified arguments. geometryMarkup must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified values.</returns>
        /// <param name="geometryMarkup">An expression that provides the Geography Markup Language (GML) representation of the geometry value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml",
            Justification = "Abbreviation more meaningful than what it stands for")]
        public static DbFunctionExpression GeometryFromGml(DbExpression geometryMarkup, DbExpression coordinateSystemId)
        {
            Check.NotNull(geometryMarkup, "geometryMarkup");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromGml", geometryMarkup, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geography well known text Constructors

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyFromText' function with the specified argument, which must have a string result type. The result type of the expression is Edm.Geography. Its value has the default coordinate system id (SRID) of the underlying provider.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified value.</returns>
        /// <param name="wellKnownText">An expression that provides the well known text representation of the geography value.</param>
        public static DbFunctionExpression GeographyFromText(DbExpression wellKnownText)
        {
            Check.NotNull(wellKnownText, "wellKnownText");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromText", wellKnownText);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyFromText' function with the specified arguments. wellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified values.</returns>
        /// <param name="wellKnownText">An expression that provides the well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography value's coordinate system.</param>
        public static DbFunctionExpression GeographyFromText(DbExpression wellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(wellKnownText, "wellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromText", wellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyPointFromText' function with the specified arguments.
        /// </summary>
        /// <returns>The canonical 'GeographyPointFromText' function.</returns>
        /// <param name="pointWellKnownText">An expression that provides the well-known text representation of the geography point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography point value's coordinate systempointWellKnownTextValue.</param>
        public static DbFunctionExpression GeographyPointFromText(DbExpression pointWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(pointWellKnownText, "pointWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyPointFromText", pointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyLineFromText' function with the specified arguments. lineWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography line value based on the specified values.</returns>
        /// <param name="lineWellKnownText">An expression that provides the well known text representation of the geography line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography line value's coordinate system.</param>
        public static DbFunctionExpression GeographyLineFromText(DbExpression lineWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(lineWellKnownText, "lineWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyLineFromText", lineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyPolygonFromText' function with the specified arguments. polygonWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography polygon value based on the specified values.</returns>
        /// <param name="polygonWellKnownText">An expression that provides the well known text representation of the geography polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography polygon value's coordinate system.</param>
        public static DbFunctionExpression GeographyPolygonFromText(DbExpression polygonWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(polygonWellKnownText, "polygonWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyPolygonFromText", polygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyMultiPointFromText' function with the specified arguments. multiPointWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-point value based on the specified values.</returns>
        /// <param name="multiPointWellKnownText">An expression that provides the well known text representation of the geography multi-point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-point value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiPointFromText(
            DbExpression multiPointWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiPointWellKnownText, "multiPointWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPointFromText", multiPointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyMultiLineFromText' function with the specified arguments. multiLineWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-line value based on the specified values.</returns>
        /// <param name="multiLineWellKnownText">An expression that provides the well known text representation of the geography multi-line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-line value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiLineFromText(DbExpression multiLineWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiLineWellKnownText, "multiLineWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiLineFromText", multiLineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyMultiPolygonFromText' function with the specified arguments. multiPolygonWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-polygon value based on the specified values.</returns>
        /// <param name="multiPolygonWellKnownText">An expression that provides the well known text representation of the geography multi-polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-polygon value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiPolygonFromText(
            DbExpression multiPolygonWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPolygonFromText", multiPolygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyCollectionFromText' function with the specified arguments. geographyCollectionWellKnownText must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography collection value based on the specified values.</returns>
        /// <param name="geographyCollectionWellKnownText">An expression that provides the well known text representation of the geography collection value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography collection value's coordinate system.</param>
        public static DbFunctionExpression GeographyCollectionFromText(
            DbExpression geographyCollectionWellKnownText, DbExpression coordinateSystemId)
        {
            Check.NotNull(geographyCollectionWellKnownText, "geographyCollectionWellKnownText");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyCollectionFromText", geographyCollectionWellKnownText, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geography Well Known Binary Constructors

        // Geography – Well Known Binary Constructors

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyFromBinary' function with the specified argument, which must have a binary result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified binary value.</returns>
        /// <param name="wellKnownBinaryValue">An expression that provides the well known binary representation of the geography value.</param>
        public static DbFunctionExpression GeographyFromBinary(DbExpression wellKnownBinaryValue)
        {
            Check.NotNull(wellKnownBinaryValue, "wellKnownBinaryValue");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromBinary", wellKnownBinaryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyFromBinary' function with the specified arguments. wellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified values.</returns>
        /// <param name="wellKnownBinaryValue">An expression that provides the well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography value's coordinate system.</param>
        public static DbFunctionExpression GeographyFromBinary(DbExpression wellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(wellKnownBinaryValue, "wellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromBinary", wellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyPointFromBinary' function with the specified arguments. pointWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography point value based on the specified values.</returns>
        /// <param name="pointWellKnownBinaryValue">An expression that provides the well known binary representation of the geography point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography point value's coordinate systempointWellKnownBinaryValue.</param>
        public static DbFunctionExpression GeographyPointFromBinary(DbExpression pointWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(pointWellKnownBinaryValue, "pointWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyPointFromBinary", pointWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyLineFromBinary' function with the specified arguments. lineWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography line value based on the specified values.</returns>
        /// <param name="lineWellKnownBinaryValue">An expression that provides the well known binary representation of the geography line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography line value's coordinate system.</param>
        public static DbFunctionExpression GeographyLineFromBinary(DbExpression lineWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(lineWellKnownBinaryValue, "lineWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyLineFromBinary", lineWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyPolygonFromBinary' function with the specified arguments. polygonWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography polygon value based on the specified values.</returns>
        /// <param name="polygonWellKnownBinaryValue">An expression that provides the well known binary representation of the geography polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography polygon value's coordinate system.</param>
        public static DbFunctionExpression GeographyPolygonFromBinary(
            DbExpression polygonWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(polygonWellKnownBinaryValue, "polygonWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyPolygonFromBinary", polygonWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyMultiPointFromBinary' function with the specified arguments. multiPointWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-point value based on the specified values.</returns>
        /// <param name="multiPointWellKnownBinaryValue">An expression that provides the well known binary representation of the geography multi-point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-point value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiPointFromBinary(
            DbExpression multiPointWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiPointWellKnownBinaryValue, "multiPointWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPointFromBinary", multiPointWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyMultiLineFromBinary' function with the specified arguments. multiLineWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-line value based on the specified values.</returns>
        /// <param name="multiLineWellKnownBinaryValue">An expression that provides the well known binary representation of the geography multi-line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-line value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiLineFromBinary(
            DbExpression multiLineWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiLineWellKnownBinaryValue, "multiLineWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiLineFromBinary", multiLineWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyMultiPolygonFromBinary' function with the specified arguments. multiPolygonWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-polygon value based on the specified values.</returns>
        /// <param name="multiPolygonWellKnownBinaryValue">An expression that provides the well known binary representation of the geography multi-polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-polygon value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiPolygonFromBinary(
            DbExpression multiPolygonWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(multiPolygonWellKnownBinaryValue, "multiPolygonWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction(
                "GeographyMultiPolygonFromBinary", multiPolygonWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyCollectionFromBinary' function with the specified arguments. geographyCollectionWellKnownBinaryValue must have a binary result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography collection value based on the specified values.</returns>
        /// <param name="geographyCollectionWellKnownBinaryValue">An expression that provides the well known binary representation of the geography collection value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography collection value's coordinate system.</param>
        public static DbFunctionExpression GeographyCollectionFromBinary(
            DbExpression geographyCollectionWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            Check.NotNull(geographyCollectionWellKnownBinaryValue, "geographyCollectionWellKnownBinaryValue");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction(
                "GeographyCollectionFromBinary", geographyCollectionWellKnownBinaryValue, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geography GML Constructors (non-OGC)

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyFromGml' function with the specified argument, which must have a string result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified value with the default coordinate system id (SRID) of the underlying provider.</returns>
        /// <param name="geographyMarkup">An expression that provides the Geography Markup Language (GML) representation of the geography value.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbFunctionExpression GeographyFromGml(DbExpression geographyMarkup)
        {
            Check.NotNull(geographyMarkup, "geographyMarkup");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromGml", geographyMarkup);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'GeographyFromGml' function with the specified arguments. geographyMarkup must have a string result type, while coordinateSystemId must have an integer numeric result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified values.</returns>
        /// <param name="geographyMarkup">An expression that provides the Geography Markup Language (GML) representation of the geography value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography value's coordinate system.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbFunctionExpression GeographyFromGml(DbExpression geographyMarkup, DbExpression coordinateSystemId)
        {
            Check.NotNull(geographyMarkup, "geographyMarkup");
            Check.NotNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromGml", geographyMarkup, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Instance Member Access

        // Spatial ‘Instance’ Functions
        // Spatial Member Access

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'CoordinateSystemId' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the integer SRID value from spatialValue.</returns>
        /// <param name="spatialValue">An expression that specifies the value from which the coordinate system id (SRID) should be retrieved.</param>
        public static DbFunctionExpression CoordinateSystemId(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("CoordinateSystemId", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialTypeName' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.String.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the string Geometry Type name from spatialValue.</returns>
        /// <param name="spatialValue">An expression that specifies the value from which the Geometry Type name should be retrieved.</param>
        public static DbFunctionExpression SpatialTypeName(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialTypeName", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialDimension' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the Dimension value from spatialValue.</returns>
        /// <param name="spatialValue">An expression that specifies the value from which the Dimension value should be retrieved.</param>
        public static DbFunctionExpression SpatialDimension(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialDimension", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialEnvelope' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the the minimum bounding box for geometryValue.</returns>
        /// <param name="geometryValue">An expression that specifies the value from which the Envelope value should be retrieved.</param>
        public static DbFunctionExpression SpatialEnvelope(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialEnvelope", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'AsBinary' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Binary.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the well known binary representation of spatialValue.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial value from which the well known binary representation should be produced.</param>
        public static DbFunctionExpression AsBinary(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("AsBinary", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'AsGml' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.String.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the Geography Markup Language (GML) representation of spatialValue.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial value from which the Geography Markup Language (GML) representation should be produced.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbFunctionExpression AsGml(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("AsGml", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'AsText' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.String.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the well known text representation of spatialValue.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial value from which the well known text representation should be produced.</param>
        public static DbFunctionExpression AsText(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("AsText", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'IsEmptySpatial' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether spatialValue is empty.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial value from which the IsEmptySptiaal value should be retrieved.</param>
        public static DbFunctionExpression IsEmptySpatial(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("IsEmptySpatial", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'IsSimpleGeometry' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether geometryValue is a simple geometry.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        public static DbFunctionExpression IsSimpleGeometry(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("IsSimpleGeometry", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialBoundary' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the the boundary for geometryValue.</returns>
        /// <param name="geometryValue">An expression that specifies the geometry value from which the SpatialBoundary value should be retrieved.</param>
        public static DbFunctionExpression SpatialBoundary(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialBoundary", geometryValue);
        }

        // Non-OGC
        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'IsValidGeometry' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether geometryValue is valid.</returns>
        /// <param name="geometryValue">An expression that specifies the geometry value which should be tested for spatial validity.</param>
        public static DbFunctionExpression IsValidGeometry(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("IsValidGeometry", geometryValue);
        }

        #endregion

        #region Spatial Functions - Spatial Relation

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialEquals' function with the specified arguments, which must each have an Edm.Geography or Edm.Geometry result type. The result type of spatialValue1 must match the result type of spatialValue2. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether spatialValue1 and spatialValue2 are equal.</returns>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value that should be compared with spatialValue1 for equality.</param>
        public static DbFunctionExpression SpatialEquals(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            Check.NotNull(spatialValue1, "spatialValue1");
            Check.NotNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialEquals", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialDisjoint' function with the specified arguments, which must each have an Edm.Geography or Edm.Geometry result type. The result type of spatialValue1 must match the result type of spatialValue2. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether spatialValue1 and spatialValue2 are spatially disjoint.</returns>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value that should be compared with spatialValue1 for disjointness.</param>
        public static DbFunctionExpression SpatialDisjoint(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            Check.NotNull(spatialValue1, "spatialValue1");
            Check.NotNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialDisjoint", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialIntersects' function with the specified arguments, which must each have an Edm.Geography or Edm.Geometry result type. The result type of spatialValue1 must match the result type of spatialValue2. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether spatialValue1 and spatialValue2 intersect.</returns>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value that should be compared with spatialValue1 for intersection.</param>
        public static DbFunctionExpression SpatialIntersects(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            Check.NotNull(spatialValue1, "spatialValue1");
            Check.NotNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialIntersects", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialTouches' function with the specified arguments, which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether geometryValue1 touches geometryValue2.</returns>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with geometryValue1.</param>
        public static DbFunctionExpression SpatialTouches(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            Check.NotNull(geometryValue1, "geometryValue1");
            Check.NotNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialTouches", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialCrosses' function with the specified arguments, which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether geometryValue1 crosses geometryValue2 intersect.</returns>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with geometryValue1.</param>
        public static DbFunctionExpression SpatialCrosses(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            Check.NotNull(geometryValue1, "geometryValue1");
            Check.NotNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialCrosses", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialWithin' function with the specified arguments, which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether geometryValue1 is spatially within geometryValue2.</returns>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with geometryValue1.</param>
        public static DbFunctionExpression SpatialWithin(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            Check.NotNull(geometryValue1, "geometryValue1");
            Check.NotNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialWithin", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialContains' function with the specified arguments, which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether geometryValue1 spatially contains geometryValue2.</returns>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with geometryValue1.</param>
        public static DbFunctionExpression SpatialContains(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            Check.NotNull(geometryValue1, "geometryValue1");
            Check.NotNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialContains", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialOverlaps' function with the specified arguments, which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether geometryValue1 spatially overlaps geometryValue2.</returns>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with geometryValue1.</param>
        public static DbFunctionExpression SpatialOverlaps(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            Check.NotNull(geometryValue1, "geometryValue1");
            Check.NotNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialOverlaps", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialRelate' function with the specified arguments, which must have Edm.Geometry and string result types. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether geometryValue1 is spatially related to geometryValue2 according to the spatial relationship designated by intersectionPatternMatrix.</returns>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with geometryValue1.</param>
        /// <param name="intersectionPatternMatrix">An expression that specifies the text representation of the Dimensionally Extended Nine-Intersection Model (DE-9IM) intersection pattern used to compare geometryValue1 and geometryValue2.</param>
        public static DbFunctionExpression SpatialRelate(
            this DbExpression geometryValue1, DbExpression geometryValue2, DbExpression intersectionPatternMatrix)
        {
            Check.NotNull(geometryValue1, "geometryValue1");
            Check.NotNull(geometryValue2, "geometryValue2");
            Check.NotNull(intersectionPatternMatrix, "intersectionPatternMatrix");
            return EdmFunctions.InvokeCanonicalFunction("SpatialRelate", geometryValue1, geometryValue2, intersectionPatternMatrix);
        }

        #endregion

        #region Spatial Functions - Spatial Analysis

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialBuffer' function with the specified arguments, which must have a Edm.Geography or Edm.Geometry and Edm.Double result types. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a geometry value representing all points less than or equal to distance from spatialValue.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial value.</param>
        /// <param name="distance">An expression that specifies the buffer distance.</param>
        public static DbFunctionExpression SpatialBuffer(this DbExpression spatialValue, DbExpression distance)
        {
            Check.NotNull(spatialValue, "spatialValue");
            Check.NotNull(distance, "distance");
            return EdmFunctions.InvokeCanonicalFunction("SpatialBuffer", spatialValue, distance);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'Distance' function with the specified arguments, which must each have an Edm.Geography or Edm.Geometry result type.  The result type of spatialValue1 must match the result type of spatialValue2. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the distance between the closest points in spatialValue1 and spatialValue1.</returns>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value from which the distance from spatialValue1 should be measured.</param>
        public static DbFunctionExpression Distance(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            Check.NotNull(spatialValue1, "spatialValue1");
            Check.NotNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("Distance", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialConvexHull' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the the convex hull for geometryValue.</returns>
        /// <param name="geometryValue">An expression that specifies the geometry value from which the convex hull value should be retrieved.</param>
        public static DbFunctionExpression SpatialConvexHull(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialConvexHull", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialIntersection' function with the specified arguments, which must each have an Edm.Geography or Edm.Geometry result type. The result type of spatialValue1 must match the result type of spatialValue2. The result type of the expression is the same as the type of spatialValue1 and spatialValue2.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the spatial value representing the intersection of spatialValue1 and spatialValue2.</returns>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value for which the intersection with spatialValue1 should be computed.</param>
        public static DbFunctionExpression SpatialIntersection(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            Check.NotNull(spatialValue1, "spatialValue1");
            Check.NotNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialIntersection", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialUnion' function with the specified arguments, which must each have an Edm.Geography or Edm.Geometry result type. The result type of spatialValue1 must match the result type of spatialValue2. The result type of the expression is the same as the type of spatialValue1 and spatialValue2.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the spatial value representing the union of spatialValue1 and spatialValue2.</returns>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value for which the union with spatialValue1 should be computed.</param>
        public static DbFunctionExpression SpatialUnion(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            Check.NotNull(spatialValue1, "spatialValue1");
            Check.NotNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialUnion", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialDifference' function with the specified arguments, which must each have an Edm.Geography or Edm.Geometry result type. The result type of spatialValue1 must match the result type of spatialValue2. The result type of the expression is the same as the type of spatialValue1 and spatialValue2.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the geometry value representing the difference of spatialValue2 with spatialValue1.</returns>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value for which the difference with spatialValue1 should be computed.</param>
        public static DbFunctionExpression SpatialDifference(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            Check.NotNull(spatialValue1, "spatialValue1");
            Check.NotNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialDifference", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialSymmetricDifference' function with the specified arguments, which must each have an Edm.Geography or Edm.Geometry result type. The result type of spatialValue1 must match the result type of spatialValue2. The result type of the expression is the same as the type of spatialValue1 and spatialValue2.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the geometry value representing the symmetric difference of spatialValue2 with spatialValue1.</returns>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value for which the symmetric difference with spatialValue1 should be computed.</param>
        public static DbFunctionExpression SpatialSymmetricDifference(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            Check.NotNull(spatialValue1, "spatialValue1");
            Check.NotNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialSymmetricDifference", spatialValue1, spatialValue2);
        }

        #endregion

        #region Spatial Functions - Spatial Collection

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialElementCount' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the number of elements in spatialValue or null if spatialValue is not a collection.</returns>
        /// <param name="spatialValue">An expression that specifies the geography or geometry collection value from which the number of elements should be retrieved.</param>
        public static DbFunctionExpression SpatialElementCount(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialElementCount", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialElementAt' function with the specified arguments. The first argument must have an Edm.Geography or Edm.Geometry result type. The second argument must have an integer numeric result type. The result type of the expression is the same as that of spatialValue.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the collection element at position indexValue in spatialValue or null if spatialValue is not a collection.</returns>
        /// <param name="spatialValue">An expression that specifies the geography or geometry collection value.</param>
        /// <param name="indexValue">An expression that specifies the position of the element to be retrieved from within the geometry or geography collection.</param>
        public static DbFunctionExpression SpatialElementAt(this DbExpression spatialValue, DbExpression indexValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            Check.NotNull(indexValue, "indexValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialElementAt", spatialValue, indexValue);
        }

        #endregion

        #region Spatial Functions - GeographyPoint

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'XCoordinate' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the X co-ordinate value of geometryValue or null if geometryValue is not a point.</returns>
        /// <param name="geometryValue">An expression that specifies the geometry point value from which the X co-ordinate value should be retrieved.</param>
        public static DbFunctionExpression XCoordinate(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("XCoordinate", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'YCoordinate' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the Y co-ordinate value of geometryValue or null if geometryValue is not a point.</returns>
        /// <param name="geometryValue">An expression that specifies the geometry point value from which the Y co-ordinate value should be retrieved.</param>
        public static DbFunctionExpression YCoordinate(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("YCoordinate", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'Elevation' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the elevation value of spatialValue or null if spatialValue is not a point.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial point value from which the elevation (Z co-ordinate) value should be retrieved.</param>
        public static DbFunctionExpression Elevation(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("Elevation", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'Measure' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the Measure of spatialValue or null if spatialValue is not a point.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial point value from which the Measure (M) co-ordinate value should be retrieved.</param>
        public static DbFunctionExpression Measure(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("Measure", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'Latitude' function with the specified argument, which must have an Edm.Geography result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the Latitude value of geographyValue or null if geographyValue is not a point.</returns>
        /// <param name="geographyValue">An expression that specifies the geography point value from which the Latitude value should be retrieved.</param>
        public static DbFunctionExpression Latitude(this DbExpression geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");
            return EdmFunctions.InvokeCanonicalFunction("Latitude", geographyValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'Longitude' function with the specified argument, which must have an Edm.Geography result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the Longitude value of geographyValue or null if geographyValue is not a point.</returns>
        /// <param name="geographyValue">An expression that specifies the geography point value from which the Longitude value should be retrieved.</param>
        public static DbFunctionExpression Longitude(this DbExpression geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");
            return EdmFunctions.InvokeCanonicalFunction("Longitude", geographyValue);
        }

        #endregion

        #region Spatial Functions - Curve

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'SpatialLength' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the length of spatialValue or null if spatialValue is not a curve.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial curve value from which the length should be retrieved.</param>
        public static DbFunctionExpression SpatialLength(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialLength", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'StartPoint' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type is the same as that of spatialValue.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the start point of spatialValue or null if spatialValue is not a curve.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial curve value from which the start point should be retrieved.</param>
        public static DbFunctionExpression StartPoint(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("StartPoint", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'EndPoint' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type is the same as that of spatialValue.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the end point of spatialValue or null if spatialValue is not a curve.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial curve value from which the end point should be retrieved.</param>
        public static DbFunctionExpression EndPoint(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("EndPoint", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'IsClosedSpatial' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either a Boolean value indicating whether spatialValue is closed, or null if spatialValue is not a curve.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial curve value from which the IsClosedSpatial value should be retrieved.</param>
        public static DbFunctionExpression IsClosedSpatial(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("IsClosedSpatial", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'IsRing' function with the specified argument, which must have an Edm.Geometry result type. The result type is Edm.Boolean.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either a Boolean value indicating whether geometryValue is a ring (both closed and simple), or null if geometryValue is not a curve.</returns>
        /// <param name="geometryValue">An expression that specifies the geometry curve value from which the IsRing value should be retrieved.</param>
        public static DbFunctionExpression IsRing(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("IsRing", geometryValue);
        }

        #endregion

        #region Spatial Functions - GeographyLineString, Line, LinearRing

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'PointCount' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the number of points in spatialValue or null if spatialValue is not a line string.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial line string value from which the number of points should be retrieved.</param>
        public static DbFunctionExpression PointCount(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("PointCount", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'PointAt' function with the specified arguments. The first argument must have an Edm.Geography or Edm.Geometry result type. The second argument must have an integer numeric result type. The result type of the expression is the same as that of spatialValue.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the point at position indexValue in spatialValue or null if spatialValue is not a line string.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial line string value.</param>
        /// <param name="indexValue">An expression that specifies the position of the point to be retrieved from within the line string.</param>
        public static DbFunctionExpression PointAt(this DbExpression spatialValue, DbExpression indexValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            Check.NotNull(indexValue, "indexValue");
            return EdmFunctions.InvokeCanonicalFunction("PointAt", spatialValue, indexValue);
        }

        #endregion

        #region Spatial Functions - Surface

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'Area' function with the specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the area of spatialValue or null if spatialValue is not a surface.</returns>
        /// <param name="spatialValue">An expression that specifies the spatial surface value for which the area should be calculated.</param>
        public static DbFunctionExpression Area(this DbExpression spatialValue)
        {
            Check.NotNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("Area", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'Centroid' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the centroid point of geometryValue (which may not be on the surface itself) or null if geometryValue is not a surface.</returns>
        /// <param name="geometryValue">An expression that specifies the geometry surface value from which the centroid should be retrieved.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Centroid",
            Justification = "Standard bame")]
        public static DbFunctionExpression Centroid(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("Centroid", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'PointOnSurface' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either a point guaranteed to be on the surface geometryValue or null if geometryValue is not a surface.</returns>
        /// <param name="geometryValue">An expression that specifies the geometry surface value from which the point should be retrieved.</param>
        public static DbFunctionExpression PointOnSurface(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("PointOnSurface", geometryValue);
        }

        #endregion

        #region Spatial Functions - GeographyPolygon

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'ExteriorRing' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the exterior ring of the polygon geometryValue or null if geometryValue is not a polygon.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        public static DbFunctionExpression ExteriorRing(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("ExteriorRing", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'InteriorRingCount' function with the specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the number of interior rings in the polygon geometryValue or null if geometryValue is not a polygon.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        public static DbFunctionExpression InteriorRingCount(this DbExpression geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("InteriorRingCount", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that invokes the canonical 'InteriorRingAt' function with the specified arguments. The first argument must have an Edm.Geometry result type. The second argument must have an integer numeric result types. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns either the interior ring at position indexValue in geometryValue or null if geometryValue is not a polygon.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <param name="indexValue">An expression that specifies the position of the interior ring to be retrieved from within the polygon.</param>
        public static DbFunctionExpression InteriorRingAt(this DbExpression geometryValue, DbExpression indexValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            Check.NotNull(indexValue, "indexValue");
            return EdmFunctions.InvokeCanonicalFunction("InteriorRingAt", geometryValue, indexValue);
        }

        #endregion
    }
}
