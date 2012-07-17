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
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal sealed class EdmPrimitiveType
        : EdmScalarType

    {
        private static readonly EdmPrimitiveType _binaryType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Binary);
        private static readonly EdmPrimitiveType _booleanType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Boolean);
        private static readonly EdmPrimitiveType _byteType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Byte);
        private static readonly EdmPrimitiveType _dateTimeType = new EdmPrimitiveType(EdmPrimitiveTypeKind.DateTime);

        private static readonly EdmPrimitiveType _dateTimeOffsetType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset);

        private static readonly EdmPrimitiveType _decimalType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Decimal);
        private static readonly EdmPrimitiveType _doubleType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Double);
        private static readonly EdmPrimitiveType _guidType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Guid);
        private static readonly EdmPrimitiveType _int16Type = new EdmPrimitiveType(EdmPrimitiveTypeKind.Int16);
        private static readonly EdmPrimitiveType _int32Type = new EdmPrimitiveType(EdmPrimitiveTypeKind.Int32);
        private static readonly EdmPrimitiveType _int64Type = new EdmPrimitiveType(EdmPrimitiveTypeKind.Int64);
        private static readonly EdmPrimitiveType _sbyteType = new EdmPrimitiveType(EdmPrimitiveTypeKind.SByte);
        private static readonly EdmPrimitiveType _singleType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Single);
        private static readonly EdmPrimitiveType _stringType = new EdmPrimitiveType(EdmPrimitiveTypeKind.String);
        private static readonly EdmPrimitiveType _timeType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Time);
        private static readonly EdmPrimitiveType _geometryType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Geometry);

        private static readonly EdmPrimitiveType _geometricPointType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricPoint);

        private static readonly EdmPrimitiveType _geometricLinestringType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricLinestring);

        private static readonly EdmPrimitiveType _geometricPolygonType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricPolygon);

        private static readonly EdmPrimitiveType _geometricMultiPointType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricMultiPoint);

        private static readonly EdmPrimitiveType _geometricMultiLinestringType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometricMultiLinestring);

        private static readonly EdmPrimitiveType _geometricMultiPolygonType = new EdmPrimitiveType(
            EdmPrimitiveTypeKind.GeometricMultiPolygon);

        private static readonly EdmPrimitiveType _geometryCollectionType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection);

        private static readonly EdmPrimitiveType _geographyType = new EdmPrimitiveType(EdmPrimitiveTypeKind.Geography);

        private static readonly EdmPrimitiveType _geographicPointType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicPoint);

        private static readonly EdmPrimitiveType _geographicLinestringType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicLinestring);

        private static readonly EdmPrimitiveType _geographicPolygonType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicPolygon);

        private static readonly EdmPrimitiveType _geographicMultiPointType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicMultiPoint);

        private static readonly EdmPrimitiveType _geographicMultiLinestringType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicMultiLinestring);

        private static readonly EdmPrimitiveType _geographicMultiPolygonType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographicMultiPolygon);

        private static readonly EdmPrimitiveType _geographyCollectionType =
            new EdmPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection);

        private static readonly Dictionary<EdmPrimitiveTypeKind, EdmPrimitiveType> _typeKindToTypeMap
            = new Dictionary<EdmPrimitiveTypeKind, EdmPrimitiveType>
                {
                    { EdmPrimitiveTypeKind.Binary, _binaryType },
                    { EdmPrimitiveTypeKind.Boolean, _booleanType },
                    { EdmPrimitiveTypeKind.Byte, _byteType },
                    { EdmPrimitiveTypeKind.DateTime, _dateTimeType },
                    { EdmPrimitiveTypeKind.DateTimeOffset, _dateTimeOffsetType },
                    { EdmPrimitiveTypeKind.Decimal, _decimalType },
                    { EdmPrimitiveTypeKind.Double, _doubleType },
                    { EdmPrimitiveTypeKind.Guid, _guidType },
                    { EdmPrimitiveTypeKind.Int16, _int16Type },
                    { EdmPrimitiveTypeKind.Int32, _int32Type },
                    { EdmPrimitiveTypeKind.Int64, _int64Type },
                    { EdmPrimitiveTypeKind.SByte, _sbyteType },
                    { EdmPrimitiveTypeKind.Single, _singleType },
                    { EdmPrimitiveTypeKind.String, _stringType },
                    { EdmPrimitiveTypeKind.Time, _timeType },
                    { EdmPrimitiveTypeKind.Geometry, _geometryType },
                    { EdmPrimitiveTypeKind.GeometricPoint, _geometricPointType },
                    { EdmPrimitiveTypeKind.GeometricLinestring, _geometricLinestringType },
                    { EdmPrimitiveTypeKind.GeometricPolygon, _geometricPolygonType },
                    { EdmPrimitiveTypeKind.GeometricMultiPoint, _geometricMultiPointType },
                    { EdmPrimitiveTypeKind.GeometricMultiLinestring, _geometricMultiLinestringType },
                    { EdmPrimitiveTypeKind.GeometricMultiPolygon, _geometricMultiPolygonType },
                    { EdmPrimitiveTypeKind.GeometryCollection, _geometryCollectionType },
                    { EdmPrimitiveTypeKind.Geography, _geographyType },
                    { EdmPrimitiveTypeKind.GeographicPoint, _geographicPointType },
                    { EdmPrimitiveTypeKind.GeographicLinestring, _geographicLinestringType },
                    { EdmPrimitiveTypeKind.GeographicPolygon, _geographicPolygonType },
                    { EdmPrimitiveTypeKind.GeographicMultiPoint, _geographicMultiPointType },
                    { EdmPrimitiveTypeKind.GeographicMultiLinestring, _geographicMultiLinestringType },
                    { EdmPrimitiveTypeKind.GeographicMultiPolygon, _geographicMultiPolygonType },
                    { EdmPrimitiveTypeKind.GeographyCollection, _geographyCollectionType }
                };

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
            get { return _binaryType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Boolean" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Boolean
        {
            get { return _booleanType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Byte" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Byte
        {
            get { return _byteType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.DateTime" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType DateTime
        {
            get { return _dateTimeType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.DateTimeOffset" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType DateTimeOffset
        {
            get { return _dateTimeOffsetType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Decimal" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Decimal
        {
            get { return _decimalType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Double" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Double
        {
            get { return _doubleType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Guid" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Guid
        {
            get { return _guidType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Int16" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Int16
        {
            get { return _int16Type; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Int32" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Int32
        {
            get { return _int32Type; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Int64" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Int64
        {
            get { return _int64Type; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.SByte" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType SByte
        {
            get { return _sbyteType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Single" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Single
        {
            get { return _singleType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.String" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType String
        {
            get { return _stringType; }
        }

        /// <summary>
        ///     Gets the EdmPrimitiveType instance that represents the <see cref = "EdmPrimitiveTypeKind.Time" /> primitive type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmPrimitiveType Time
        {
            get { return _timeType; }
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
                Contract.Assert(_typeKindToTypeMap.ContainsKey(kind), "Added EdmPrimitiveTypeKind?");
                return _typeKindToTypeMap.TryGetValue(kind, out primitiveType);
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
