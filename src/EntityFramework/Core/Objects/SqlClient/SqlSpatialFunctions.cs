// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.SqlClient
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Contains function stubs that expose SqlServer methods in Linq to Entities.
    /// </summary>
    public static class SqlSpatialFunctions
    {
        /// <summary>
        ///     Proxy for the function SqlServer.POINTGEOGRAPHY
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "latitude")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "spatialReferenceId")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "longitude")]
        [DbFunction("SqlServer", "POINTGEOGRAPHY")]
        public static DbGeography PointGeography(Double? latitude, Double? longitude, Int32? spatialReferenceId)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.ASTEXTZM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "ASTEXTZM")]
        public static String AsTextZM(DbGeography geographyValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.BUFFERWITHTOLERANCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "distance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "relative")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [DbFunction("SqlServer", "BUFFERWITHTOLERANCE")]
        public static DbGeography BufferWithTolerance(DbGeography geographyValue, Double? distance, Double? tolerance, Boolean? relative)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.ENVELOPEANGLE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "ENVELOPEANGLE")]
        public static Double? EnvelopeAngle(DbGeography geographyValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.ENVELOPECENTER
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "ENVELOPECENTER")]
        public static DbGeography EnvelopeCenter(DbGeography geographyValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.FILTER
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyOther")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "FILTER")]
        public static Boolean? Filter(DbGeography geographyValue, DbGeography geographyOther)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.INSTANCEOF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryTypeName")]
        [DbFunction("SqlServer", "INSTANCEOF")]
        public static Boolean? InstanceOf(DbGeography geographyValue, String geometryTypeName)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.NUMRINGS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [DbFunction("SqlServer", "NUMRINGS")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Num",
            Justification = "Naming convention prescribed by OGC specification")]
        public static Int32? NumRings(DbGeography geographyValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.REDUCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [DbFunction("SqlServer", "REDUCE")]
        public static DbGeography Reduce(DbGeography geographyValue, Double? tolerance)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.RINGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "index")]
        [DbFunction("SqlServer", "RINGN")]
        public static DbGeography RingN(DbGeography geographyValue, Int32? index)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.POINTGEOMETRY
        /// </summary>
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

        /// <summary>
        ///     Proxy for the function SqlServer.ASTEXTZM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "ASTEXTZM")]
        public static String AsTextZM(DbGeometry geometryValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.BUFFERWITHTOLERANCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "relative")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "distance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "BUFFERWITHTOLERANCE")]
        public static DbGeometry BufferWithTolerance(DbGeometry geometryValue, Double? distance, Double? tolerance, Boolean? relative)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.INSTANCEOF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryTypeName")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "INSTANCEOF")]
        public static Boolean? InstanceOf(DbGeometry geometryValue, String geometryTypeName)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.FILTER
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryOther")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "FILTER")]
        public static Boolean? Filter(DbGeometry geometryValue, DbGeometry geometryOther)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.MAKEVALID
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "MAKEVALID")]
        public static DbGeometry MakeValid(DbGeometry geometryValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        ///     Proxy for the function SqlServer.REDUCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [DbFunction("SqlServer", "REDUCE")]
        public static DbGeometry Reduce(DbGeometry geometryValue, Double? tolerance)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }
    }
}
