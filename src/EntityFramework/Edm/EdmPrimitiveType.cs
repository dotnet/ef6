namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Represents one of the fixed set of Entity Data Model (EDM) primitive types.
    /// </summary>
    internal sealed class EdmPrimitiveType
        : EdmScalarType

    {
        private static readonly EdmPrimitiveType binaryType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Binary);
        private static readonly EdmPrimitiveType booleanType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Boolean);
        private static readonly EdmPrimitiveType byteType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Byte);
        private static readonly EdmPrimitiveType dateTimeType = new EdmPrimitiveType(EdmPrimitiveTypeKind.DateTime);

        private static readonly EdmPrimitiveType dateTimeOffsetType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset);

        private static readonly EdmPrimitiveType decimalType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Decimal);
        private static readonly EdmPrimitiveType doubleType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Double);
        private static readonly EdmPrimitiveType guidType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Guid);
        private static readonly EdmPrimitiveType int16Type = new EdmPrimitiveType(EdmPrimitiveTypeKind.Int16);
        private static readonly EdmPrimitiveType int32Type = new EdmPrimitiveType(EdmPrimitiveTypeKind.Int32);
        private static readonly EdmPrimitiveType int64Type = new EdmPrimitiveType(EdmPrimitiveTypeKind.Int64);
        private static readonly EdmPrimitiveType sbyteType = new EdmPrimitiveType(EdmPrimitiveTypeKind.SByte);
        private static readonly EdmPrimitiveType singleType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Single);
        private static readonly EdmPrimitiveType stringType = new EdmPrimitiveType(EdmPrimitiveTypeKind.String);
        private static readonly EdmPrimitiveType timeType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Time);
        private static readonly EdmPrimitiveType geometryType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Geometry);

        private static readonly EdmPrimitiveType geometricPointType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricPoint);

        private static readonly EdmPrimitiveType geometricLinestringType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricLinestring);

        private static readonly EdmPrimitiveType geometricPolygonType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricPolygon);

        private static readonly EdmPrimitiveType geometricMultiPointType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricMultiPoint);

        private static readonly EdmPrimitiveType geometricMultiLinestringType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricMultiLinestring);

        private static readonly EdmPrimitiveType geometricMultiPolygonType = new EdmPrimitiveType(
            EdmPrimitiveTypeKind.GeometricMultiPolygon);

        private static readonly EdmPrimitiveType geometryCollectionType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection);

        private static readonly EdmPrimitiveType geographyType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Geography);

        private static readonly EdmPrimitiveType geographicPointType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicPoint);

        private static readonly EdmPrimitiveType geographicLinestringType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicLinestring);

        private static readonly EdmPrimitiveType geographicPolygonType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicPolygon);

        private static readonly EdmPrimitiveType geographicMultiPointType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicMultiPoint);

        private static readonly EdmPrimitiveType geographicMultiLinestringType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicMultiLinestring);

        private static readonly EdmPrimitiveType geographicMultiPolygonType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicMultiPolygon);

        private static readonly EdmPrimitiveType geographyCollectionType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection);

        private static readonly Dictionary<EdmPrimitiveTypeKind, EdmPrimitiveType> typeKindToTypeMap;

        static EdmPrimitiveType()
        {
            var kindMap = new Dictionary<EdmPrimitiveTypeKind, EdmPrimitiveType>();
            kindMap.Add(EdmPrimitiveTypeKind.Binary, binaryType);
            kindMap.Add(EdmPrimitiveTypeKind.Boolean, booleanType);
            kindMap.Add(EdmPrimitiveTypeKind.Byte, byteType);
            kindMap.Add(EdmPrimitiveTypeKind.DateTime, dateTimeType);
            kindMap.Add(EdmPrimitiveTypeKind.DateTimeOffset, dateTimeOffsetType);
            kindMap.Add(EdmPrimitiveTypeKind.Decimal, decimalType);
            kindMap.Add(EdmPrimitiveTypeKind.Double, doubleType);
            kindMap.Add(EdmPrimitiveTypeKind.Guid, guidType);
            kindMap.Add(EdmPrimitiveTypeKind.Int16, int16Type);
            kindMap.Add(EdmPrimitiveTypeKind.Int32, int32Type);
            kindMap.Add(EdmPrimitiveTypeKind.Int64, int64Type);
            kindMap.Add(EdmPrimitiveTypeKind.SByte, sbyteType);
            kindMap.Add(EdmPrimitiveTypeKind.Single, singleType);
            kindMap.Add(EdmPrimitiveTypeKind.String, stringType);
            kindMap.Add(EdmPrimitiveTypeKind.Time, timeType);
            kindMap.Add(EdmPrimitiveTypeKind.Geometry, geometryType);
            kindMap.Add(EdmPrimitiveTypeKind.GeometricPoint, geometricPointType);
            kindMap.Add(EdmPrimitiveTypeKind.GeometricLinestring, geometricLinestringType);
            kindMap.Add(EdmPrimitiveTypeKind.GeometricPolygon, geometricPolygonType);
            kindMap.Add(EdmPrimitiveTypeKind.GeometricMultiPoint, geometricMultiPointType);
            kindMap.Add(EdmPrimitiveTypeKind.GeometricMultiLinestring, geometricMultiLinestringType);
            kindMap.Add(EdmPrimitiveTypeKind.GeometricMultiPolygon, geometricMultiPolygonType);
            kindMap.Add(EdmPrimitiveTypeKind.GeometryCollection, geometryCollectionType);
            kindMap.Add(EdmPrimitiveTypeKind.Geography, geographyType);
            kindMap.Add(EdmPrimitiveTypeKind.GeographicPoint, geographicPointType);
            kindMap.Add(EdmPrimitiveTypeKind.GeographicLinestring, geographicLinestringType);
            kindMap.Add(EdmPrimitiveTypeKind.GeographicPolygon, geographicPolygonType);
            kindMap.Add(EdmPrimitiveTypeKind.GeographicMultiPoint, geographicMultiPointType);
            kindMap.Add(EdmPrimitiveTypeKind.GeographicMultiLinestring, geographicMultiLinestringType);
            kindMap.Add(EdmPrimitiveTypeKind.GeographicMultiPolygon, geographicMultiPolygonType);
            kindMap.Add(EdmPrimitiveTypeKind.GeographyCollection, geographyCollectionType);

            typeKindToTypeMap = kindMap;
        }

        private readonly EdmPrimitiveTypeKind typeKind;

        private EdmPrimitiveType(EdmPrimitiveTypeKind kind)
        {
            typeKind = kind;
            base.Name = kind.ToString();
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Binary" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Binary
        {
            get { return binaryType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Boolean" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Boolean
        {
            get { return booleanType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Byte" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Byte
        {
            get { return byteType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.DateTime" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType DateTime
        {
            get { return dateTimeType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.DateTimeOffset" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType DateTimeOffset
        {
            get { return dateTimeOffsetType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Decimal" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Decimal
        {
            get { return decimalType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Double" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Double
        {
            get { return doubleType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Guid" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Guid
        {
            get { return guidType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Int16" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Int16
        {
            get { return int16Type; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Int32" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Int32
        {
            get { return int32Type; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Int64" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Int64
        {
            get { return int64Type; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.SByte" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType SByte
        {
            get { return sbyteType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Single" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Single
        {
            get { return singleType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.String" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType String
        {
            get { return stringType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Time" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Time
        {
            get { return timeType; }
        }

        /// <summary>
        ///     Retrieves the EdmPrimitiveType instance with the <see cref = "EdmPrimitiveTypeKind" /> corresponding to the specified <paramref name = "primitiveTypeName" /> value, if any.
        /// </summary>
        /// <param name = "primitiveTypeName"> The name of the primitive type instance to retrieve </param>
        /// <param name = "primitiveType"> The EdmPrimitiveType with the specified name, if successful; otherwise <c>null</c> . </param>
        /// <returns> <c>true</c> if the given name corresponds to an EDM primitive type name; otherwise <c>false</c> . </returns>
        public static bool TryGetByName(string primitiveTypeName, out EdmPrimitiveType primitiveType)
        {
            Contract.Requires(primitiveTypeName != null);

            EdmPrimitiveTypeKind kind;
            if (EdmUtil.TryGetPrimitiveTypeKindFromString(primitiveTypeName, out kind))
            {
                Contract.Assert(typeKindToTypeMap.ContainsKey(kind), "Added EdmPrimitiveTypeKind?");
                return typeKindToTypeMap.TryGetValue(kind, out primitiveType);
            }

            primitiveType = null;
            return false;
        }

        /// <summary>
        ///     Gets an <see cref = "EdmPrimitiveTypeKind" /> value that indicates which Entity Data Model (EDM) primitive type this type represents.
        /// </summary>
        public EdmPrimitiveTypeKind PrimitiveTypeKind
        {
            get { return typeKind; }
        }

        public override IList<DataModelAnnotation> Annotations
        {
            get { return new DataModelAnnotation[0]; }
            set
            {
                throw EdmUtil.NotSupported(
                    Strings.EdmPrimitiveType_SetPropertyNotSupported(EdmConstants.Property_Annotations));
            }
        }

        public override string Name
        {
            get { return base.Name; }
            set { throw EdmUtil.NotSupported(Strings.EdmPrimitiveType_SetPropertyNotSupported(EdmConstants.Property_Name)); }
        }

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.PrimitiveType;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }
    }
}
