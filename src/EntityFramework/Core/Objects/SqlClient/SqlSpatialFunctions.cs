namespace System.Data.Entity.Core.Objects.SqlClient
{
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains function stubs that expose SqlServer methods in Linq to Entities.
    /// </summary>
    public static class SqlSpatialFunctions
    {
        /// <summary>
        /// Proxy for the function SqlServer.POINTGEOGRAPHY
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "latitude")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "spatialReferenceId")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "longitude")]
        [EdmFunction("SqlServer", "POINTGEOGRAPHY")]
        public static DbGeography PointGeography(Double? latitude, Double? longitude, Int32? spatialReferenceId)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ASTEXTZM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [EdmFunction("SqlServer", "ASTEXTZM")]
        public static String AsTextZM(DbGeography geographyValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.BUFFERWITHTOLERANCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "distance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "relative")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [EdmFunction("SqlServer", "BUFFERWITHTOLERANCE")]
        public static DbGeography BufferWithTolerance(DbGeography geographyValue, Double? distance, Double? tolerance, Boolean? relative)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ENVELOPEANGLE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [EdmFunction("SqlServer", "ENVELOPEANGLE")]
        public static Double? EnvelopeAngle(DbGeography geographyValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ENVELOPECENTER
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [EdmFunction("SqlServer", "ENVELOPECENTER")]
        public static DbGeography EnvelopeCenter(DbGeography geographyValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.FILTER
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyOther")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [EdmFunction("SqlServer", "FILTER")]
        public static Boolean? Filter(DbGeography geographyValue, DbGeography geographyOther)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.INSTANCEOF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryTypeName")]
        [EdmFunction("SqlServer", "INSTANCEOF")]
        public static Boolean? InstanceOf(DbGeography geographyValue, String geometryTypeName)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.NUMRINGS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [EdmFunction("SqlServer", "NUMRINGS")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Num",
            Justification = "Naming convention prescribed by OGC specification")]
        public static Int32? NumRings(DbGeography geographyValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.REDUCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [EdmFunction("SqlServer", "REDUCE")]
        public static DbGeography Reduce(DbGeography geographyValue, Double? tolerance)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RINGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geographyValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "index")]
        [EdmFunction("SqlServer", "RINGN")]
        public static DbGeography RingN(DbGeography geographyValue, Int32? index)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.POINTGEOMETRY
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "xCoordinate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "spatialReferenceId")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "yCoordinate")]
        [EdmFunction("SqlServer", "POINTGEOMETRY")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x",
            Justification = "Naming convention prescribed by OGC specification")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y",
            Justification = "Naming convention prescribed by OGC specification")]
        public static DbGeometry PointGeometry(Double? xCoordinate, Double? yCoordinate, Int32? spatialReferenceId)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ASTEXTZM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [EdmFunction("SqlServer", "ASTEXTZM")]
        public static String AsTextZM(DbGeometry geometryValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.BUFFERWITHTOLERANCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "relative")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "distance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [EdmFunction("SqlServer", "BUFFERWITHTOLERANCE")]
        public static DbGeometry BufferWithTolerance(DbGeometry geometryValue, Double? distance, Double? tolerance, Boolean? relative)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.INSTANCEOF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryTypeName")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [EdmFunction("SqlServer", "INSTANCEOF")]
        public static Boolean? InstanceOf(DbGeometry geometryValue, String geometryTypeName)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.FILTER
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryOther")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [EdmFunction("SqlServer", "FILTER")]
        public static Boolean? Filter(DbGeometry geometryValue, DbGeometry geometryOther)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.MAKEVALID
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [EdmFunction("SqlServer", "MAKEVALID")]
        public static DbGeometry MakeValid(DbGeometry geometryValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.REDUCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tolerance")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "geometryValue")]
        [EdmFunction("SqlServer", "REDUCE")]
        public static DbGeometry Reduce(DbGeometry geometryValue, Double? tolerance)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }
    }
}
