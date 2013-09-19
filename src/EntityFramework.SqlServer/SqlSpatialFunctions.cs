// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.Spatial;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains function stubs that expose SqlServer methods in Linq to Entities.
    /// </summary>
    public static class SqlSpatialFunctions
    {
        /// <summary>Constructs a geography instance representing a Point instance from its x and y values and a spatial reference ID (SRID). </summary>
        /// <returns>The constructed geography instance.</returns>
        /// <param name="latitude">The x-coordinate of the Point being generated.</param>
        /// <param name="longitude">The y-coordinate of the Point being generated</param>
        /// <param name="spatialReferenceId">The SRID of the geography instance.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "latitude")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "spatialReferenceId")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "longitude")]
        [DbFunction("SqlServer", "POINTGEOGRAPHY")]
        public static DbGeography PointGeography(Double? latitude, Double? longitude, Int32? spatialReferenceId)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation of a geography instance augmented with any Z (elevation) and M (measure) values carried by the instance.</summary>
        /// <returns>The Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation of a geography instance.</returns>
        /// <param name="geographyValue">The geography value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "ASTEXTZM")]
        public static String AsTextZM(DbGeography geographyValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a geometric object representing the union of all point values whose distance from a geography instance is less than or equal to a specified value, allowing for a specified tolerance.</summary>
        /// <returns>The union of all point values whose distance from a geography instance is less than or equal to a specified value</returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <param name="distance">The distance.</param>
        /// <param name="tolerance">The specified tolerance.</param>
        /// <param name="relative">Specifying whether the tolerance value is relative or absolute.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "distance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "relative")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [DbFunction("SqlServer", "BUFFERWITHTOLERANCE")]
        public static DbGeography BufferWithTolerance(DbGeography geographyValue, Double? distance, Double? tolerance, Boolean? relative)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the maximum angle between the point returned by EnvelopeCenter() and a point in the geography instance in degrees.</summary>
        /// <returns>the maximum angle between the point returned by EnvelopeCenter().</returns>
        /// <param name="geographyValue">The geography value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "ENVELOPEANGLE")]
        public static Double? EnvelopeAngle(DbGeography geographyValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a point that can be used as the center of a bounding circle for the geography instance.</summary>
        /// <returns>A SqlGeography value that specifies the location of the center of a bounding circle.</returns>
        /// <param name="geographyValue">The geography value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "ENVELOPECENTER")]
        public static DbGeography EnvelopeCenter(DbGeography geographyValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Offers a fast, index-only intersection method to determine if a geography instance intersects another SqlGeography instance, assuming an index is available.</summary>
        /// <returns>True if a geography instance potentially intersects another SqlGeography instance; otherwise, false.</returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <param name="geographyOther">Another geography instance to compare against the instance on which Filter is invoked.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyOther")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "FILTER")]
        public static Boolean? Filter(DbGeography geographyValue, DbGeography geographyOther)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Tests if the SqlGeography instance is the same as the specified type.</summary>
        /// <returns>A string that specifies one of the 12 types exposed in the geography type hierarchy.</returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <param name="geometryTypeName">A string that specifies one of the 12 types exposed in the geography type hierarchy.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryTypeName")]
        [DbFunction("SqlServer", "INSTANCEOF")]
        public static Boolean? InstanceOf(DbGeography geographyValue, String geometryTypeName)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the total number of rings in a Polygon instance.</summary>
        /// <returns>The total number of rings.</returns>
        /// <param name="geographyValue">The geography value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "NUMRINGS")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Num",
            Justification = "Naming convention prescribed by OGC specification")]
        public static Int32? NumRings(DbGeography geographyValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns an approximation of the given geography instance produced by running the Douglas-Peucker algorithm on the instance with the given tolerance.</summary>
        /// <returns>
        /// Returns <see cref="T:System.Data.Entity.Spatial.DbGeography" />.
        /// </returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <param name="tolerance">The tolerance to input to the Douglas-Peucker algorithm. tolerance must be a positive number.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [DbFunction("SqlServer", "REDUCE")]
        public static DbGeography Reduce(DbGeography geographyValue, Double? tolerance)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the specified ring of the SqlGeography instance: 1 ≤ n ≤ NumRings().</summary>
        /// <returns>A SqlGeography object that represents the ring specified by n.</returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <param name="index">An int expression between 1 and the number of rings in a polygon instance.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "index")]
        [DbFunction("SqlServer", "RINGN")]
        public static DbGeography RingN(DbGeography geographyValue, Int32? index)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Constructs a geometry instance representing a Point instance from its x and y values and a spatial reference ID (SRID). </summary>
        /// <returns>The constructed geometry instance.</returns>
        /// <param name="xCoordinate">The x-coordinate of the Point being generated.</param>
        /// <param name="yCoordinate">The y-coordinate of the Point being generated</param>
        /// <param name="spatialReferenceId">The SRID of the geography instance.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "xCoordinate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "spatialReferenceId")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "yCoordinate")]
        [DbFunction("SqlServer", "POINTGEOMETRY")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x",
            Justification = "Naming convention prescribed by OGC specification")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y",
            Justification = "Naming convention prescribed by OGC specification")]
        public static DbGeometry PointGeometry(Double? xCoordinate, Double? yCoordinate, Int32? spatialReferenceId)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation of a geography instance augmented with any Z (elevation) and M (measure) values carried by the instance.</summary>
        /// <returns>The Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation of a geometry instance.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "ASTEXTZM")]
        public static String AsTextZM(DbGeometry geometryValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a geometric object representing the union of all point values whose distance from a geometry instance is less than or equal to a specified value, allowing for a specified tolerance.</summary>
        /// <returns>The union of all point values whose distance from a geometry instance is less than or equal to a specified value</returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <param name="distance">The distance.</param>
        /// <param name="tolerance">The specified tolerance.</param>
        /// <param name="relative">Specifying whether the tolerance value is relative or absolute.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "relative")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "distance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "BUFFERWITHTOLERANCE")]
        public static DbGeometry BufferWithTolerance(DbGeometry geometryValue, Double? distance, Double? tolerance, Boolean? relative)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Tests if the SqlGeometry instance is the same as the specified type.</summary>
        /// <returns>A string that specifies one of the 12 types exposed in the geography type hierarchy.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <param name="geometryTypeName">A string that specifies one of the 12 types exposed in the geography type hierarchy.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryTypeName")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "INSTANCEOF")]
        public static Boolean? InstanceOf(DbGeometry geometryValue, String geometryTypeName)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Offers a fast, index-only intersection method to determine if a geography instance intersects another SqlGeometry instance, assuming an index is available.</summary>
        /// <returns>True if a geography instance potentially intersects another SqlGeography instance; otherwise, false.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <param name="geometryOther">Another geography instance to compare against the instance on which Filter is invoked.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryOther")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "FILTER")]
        public static Boolean? Filter(DbGeometry geometryValue, DbGeometry geometryOther)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Converts an invalid geometry instance into a geometry instance with a valid Open Geospatial Consortium (OGC) type. </summary>
        /// <returns>The converted geometry instance.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "MAKEVALID")]
        public static DbGeometry MakeValid(DbGeometry geometryValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns an approximation of the given geography instance produced by running the Douglas-Peucker algorithm on the instance with the given tolerance.</summary>
        /// <returns>
        /// Returns <see cref="T:System.Data.Entity.Spatial.DbGeometry" />.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <param name="tolerance">The tolerance to input to the Douglas-Peucker algorithm. tolerance must be a positive number.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "REDUCE")]
        public static DbGeometry Reduce(DbGeometry geometryValue, Double? tolerance)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }
    }
}
