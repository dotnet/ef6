// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Legacy = System.Data.Common;
using LegacyMetadata = System.Data.Metadata.Edm;
using SystemData = System.Data;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions;

    internal class LegacyDbProviderManifestWrapper : DbProviderManifest
    {
        private static readonly ConstructorInfo _primitiveTypeConstructor =
                typeof(PrimitiveType)
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[]
                            {
                                /* name */ typeof(string),
                                /* namespaceName */ typeof(string),
                                /* dataSpace */ typeof(DataSpace),
                                /* baseType */ typeof(PrimitiveType),
                                /* DbProviderManifest */ typeof(DbProviderManifest)
                            },
                        null);

        private static readonly MethodInfo SetReadOnlyMethod =
                typeof(PrimitiveType)
                    .GetMethod("SetReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
        
        private static readonly ConstructorInfo FacetDescriptionConstructor =
                typeof(FacetDescription)
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[]
                            {
                                /* facetName */ typeof(string),
                                /* facetType */ typeof(EdmType),
                                /* minValue */ typeof(int?),
                                /* maxValue */ typeof(int?),
                                /* defaultValue */ typeof(object),
                                /* isConstant */ typeof(bool),
                                /* declaringTypeName */ typeof(string)
                            },
                        null);

        private readonly Legacy.DbProviderManifest _wrappedProviderManifest;

        // store types should be accessed by full name since multiple
        // store types can have the same type kind
        private readonly ReadOnlyCollection<PrimitiveType> _storeTypes;
        private readonly LegacyMetadata.PrimitiveType[] _legacyStoreTypes;

        public LegacyDbProviderManifestWrapper(Legacy.DbProviderManifest wrappedProviderManifest)
        {
            Debug.Assert(wrappedProviderManifest != null, "wrappedProviderManifest != null");

            _wrappedProviderManifest = wrappedProviderManifest;

            _legacyStoreTypes = _wrappedProviderManifest.GetStoreTypes().ToArray();
            _storeTypes =
                new ReadOnlyCollection<PrimitiveType>(
                    _legacyStoreTypes.Select(t => ConvertFromLegacyStoreEdmType(t, this)).ToList());
        }

        internal Legacy.DbProviderManifest WrappedManifest
        {
            get { return _wrappedProviderManifest; }
        }

        public override string NamespaceName
        {
            get { return _wrappedProviderManifest.NamespaceName; }
        }

        public override ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            return _storeTypes;
        }

        public override ReadOnlyCollection<EdmFunction> GetStoreFunctions()
        {
            // EFDesigner does not need/use store functions 
            // therefore we can just return an empty collection
            return new ReadOnlyCollection<EdmFunction>(new List<EdmFunction>());
        }

        public override ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType edmType)
        {
            Debug.Assert(edmType != null, "edmType != null");
            Debug.Assert(edmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType, "Primitive type expected.");
            Debug.Assert(
                (DataSpace)typeof(EdmType)
                               .GetProperty("DataSpace", BindingFlags.Instance | BindingFlags.NonPublic)
                               .GetValue(edmType) == DataSpace.SSpace,
                "Expected SSpace type.");

            try
            {
                var legacyStoreType = _legacyStoreTypes.Single(t => t.FullName == edmType.FullName);

                return new ReadOnlyCollection<FacetDescription>(
                    _wrappedProviderManifest
                        .GetFacetDescriptions(legacyStoreType)
                        .Select(
                            legacyFacetDescription =>
                            (FacetDescription)FacetDescriptionConstructor.Invoke(
                                BindingFlags.CreateInstance,
                                null,
                                new[]
                                    {
                                        legacyFacetDescription.FacetName,
                                        FromLegacyPrimitiveType((LegacyMetadata.PrimitiveType)legacyFacetDescription.FacetType),
                                        legacyFacetDescription.MinValue,
                                        legacyFacetDescription.MaxValue,
                                        legacyFacetDescription.DefaultValue == TypeUsageHelper.LegacyVariableValue
                                            ? TypeUsageHelper.VariableValue
                                            : legacyFacetDescription.DefaultValue,
                                        legacyFacetDescription.IsConstant,
                                        legacyStoreType.FullName
                                    },
                                CultureInfo.InvariantCulture))
                        .ToList());
            }
            catch (SystemData.ProviderIncompatibleException exception)
            {
                throw new ProviderIncompatibleException(exception.Message, exception.InnerException);
            }
        }

        public override TypeUsage GetEdmType(TypeUsage storeTypeUsage)
        {
            Debug.Assert(storeTypeUsage != null, "storeTypeUsage != null");
            Debug.Assert(storeTypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType, "Primitive type expected.");
            Debug.Assert(storeTypeUsage.EdmType.GetDataSpace() == DataSpace.SSpace, "Expected SSpace type.");

            try
            {
                var legacyEdmTypeUsage =
                    _wrappedProviderManifest.GetEdmType(storeTypeUsage.ToLegacyStoreTypeUsage(_legacyStoreTypes));

                return ConvertFromLegacyEdmTypeUsage(legacyEdmTypeUsage);
            }
            catch (SystemData.ProviderIncompatibleException exception)
            {
                throw new ProviderIncompatibleException(exception.Message, exception.InnerException);
            }
        }

        public override TypeUsage GetStoreType(TypeUsage typeUsage)
        {
            Debug.Assert(typeUsage != null, "typeUsage != null");
            Debug.Assert(typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType, "Primitive type expected.");
            Debug.Assert(typeUsage.EdmType.GetDataSpace() == DataSpace.CSpace, "Expected CSpace type.");

            try
            {
                var legacyStoreTypeUsage = _wrappedProviderManifest.GetStoreType(typeUsage.ToLegacyEdmTypeUsage());
                return ConvertFromLegacyStoreTypeUsage(legacyStoreTypeUsage);
            }
            catch (SystemData.ProviderIncompatibleException exception)
            {
                throw new ProviderIncompatibleException(exception.Message, exception.InnerException);
            }
        }

        protected override XmlReader GetDbInformation(string informationType)
        {
            try
            {
                return _wrappedProviderManifest.GetInformation(informationType);
            }
            catch (SystemData.ProviderIncompatibleException exception)
            {
                throw exception.InnerException;
            }
        }

        private static PrimitiveType ConvertFromLegacyStoreEdmType(
            LegacyMetadata.PrimitiveType legacyPrimitiveType, DbProviderManifest providerManifest)
        {
            Debug.Assert(legacyPrimitiveType != null, "legacyPrimitiveType != null");
            Debug.Assert(providerManifest != null, "providerManifest != null");
            Debug.Assert(
                (LegacyMetadata.DataSpace)typeof(LegacyMetadata.EdmType)
                                              .GetProperty("DataSpace", BindingFlags.Instance | BindingFlags.NonPublic)
                                              .GetValue(legacyPrimitiveType) == LegacyMetadata.DataSpace.SSpace,
                "Expected SSpace type.");

            var newPrimitiveType = (PrimitiveType)_primitiveTypeConstructor.Invoke(
                BindingFlags.CreateInstance,
                null,
                new object[]
                    {
                        legacyPrimitiveType.Name,
                        legacyPrimitiveType.NamespaceName,
                        DataSpace.SSpace,
                        FromLegacyPrimitiveType((LegacyMetadata.PrimitiveType)legacyPrimitiveType.BaseType),
                        providerManifest
                    },
                CultureInfo.InvariantCulture
                                                      );

            SetReadOnlyMethod.Invoke(newPrimitiveType, new object[0]);

            return newPrimitiveType;
        }

        private static TypeUsage ConvertFromLegacyEdmTypeUsage(LegacyMetadata.TypeUsage legacyTypeUsage)
        {
            Debug.Assert(legacyTypeUsage != null, "legacyTypeUsage != null");
            Debug.Assert(legacyTypeUsage.EdmType is LegacyMetadata.PrimitiveType, "primitive type expected.");
            Debug.Assert(
                (LegacyMetadata.DataSpace)typeof(LegacyMetadata.EdmType)
                                              .GetProperty("DataSpace", BindingFlags.Instance | BindingFlags.NonPublic)
                                              .GetValue(legacyTypeUsage.EdmType) == LegacyMetadata.DataSpace.CSpace,
                "Expected CSpace type.");

            var legacyPrimitiveEdmType = (LegacyMetadata.PrimitiveType)legacyTypeUsage.EdmType;
            var primitiveEdmType = FromLegacyPrimitiveType(legacyPrimitiveEdmType);

            switch (legacyPrimitiveEdmType.PrimitiveTypeKind)
            {
                case LegacyMetadata.PrimitiveTypeKind.Boolean:
                case LegacyMetadata.PrimitiveTypeKind.Byte:
                case LegacyMetadata.PrimitiveTypeKind.SByte:
                case LegacyMetadata.PrimitiveTypeKind.Int16:
                case LegacyMetadata.PrimitiveTypeKind.Int32:
                case LegacyMetadata.PrimitiveTypeKind.Int64:
                case LegacyMetadata.PrimitiveTypeKind.Guid:
                case LegacyMetadata.PrimitiveTypeKind.Double:
                case LegacyMetadata.PrimitiveTypeKind.Single:
                case LegacyMetadata.PrimitiveTypeKind.Geography:
                case LegacyMetadata.PrimitiveTypeKind.GeographyPoint:
                case LegacyMetadata.PrimitiveTypeKind.GeographyLineString:
                case LegacyMetadata.PrimitiveTypeKind.GeographyPolygon:
                case LegacyMetadata.PrimitiveTypeKind.GeographyMultiPoint:
                case LegacyMetadata.PrimitiveTypeKind.GeographyMultiLineString:
                case LegacyMetadata.PrimitiveTypeKind.GeographyMultiPolygon:
                case LegacyMetadata.PrimitiveTypeKind.GeographyCollection:
                case LegacyMetadata.PrimitiveTypeKind.Geometry:
                case LegacyMetadata.PrimitiveTypeKind.GeometryPoint:
                case LegacyMetadata.PrimitiveTypeKind.GeometryLineString:
                case LegacyMetadata.PrimitiveTypeKind.GeometryPolygon:
                case LegacyMetadata.PrimitiveTypeKind.GeometryMultiPoint:
                case LegacyMetadata.PrimitiveTypeKind.GeometryMultiLineString:
                case LegacyMetadata.PrimitiveTypeKind.GeometryMultiPolygon:
                case LegacyMetadata.PrimitiveTypeKind.GeometryCollection:
                    return TypeUsage.CreateDefaultTypeUsage(primitiveEdmType);

                case LegacyMetadata.PrimitiveTypeKind.Decimal:
                    var precisionFacetValue = legacyTypeUsage.Facets[PrecisionFacetName].Value;

                    if (precisionFacetValue == null
                        || precisionFacetValue == TypeUsageHelper.LegacyUnboundedValue)
                    {
                        Debug.Assert(
                            legacyTypeUsage.Facets[ScaleFacetName].Value == precisionFacetValue,
                            "Precision and Scale facets are expected to be both unbounded (Max) or null");

                        return TypeUsage.CreateDecimalTypeUsage(primitiveEdmType);
                    }
                    else
                    {
                        var scaleFacetValue = legacyTypeUsage.Facets[ScaleFacetName].Value;

                        Debug.Assert(
                            precisionFacetValue is byte && scaleFacetValue is byte,
                            "Precision and Scale facets are expected to be both unbounded (Max) or both of byte type");

                        return
                            TypeUsage.CreateDecimalTypeUsage(
                                primitiveEdmType,
                                (byte)precisionFacetValue,
                                (byte)scaleFacetValue);
                    }

                case LegacyMetadata.PrimitiveTypeKind.Binary:
                    Debug.Assert(
                        !(legacyTypeUsage.Facets[FixedLengthFacetName].Value == null
                          ^ legacyTypeUsage.Facets[MaxLengthFacetName].Value == null),
                        "Both Fixed Length and Max Length facet values should be null or none should be null");

                    var fixedLengthFacetValue = legacyTypeUsage.Facets[FixedLengthFacetName].Value;
                    if (fixedLengthFacetValue == null)
                    {
                        return TypeUsage.CreateDefaultTypeUsage(primitiveEdmType);
                    }
                    else
                    {
                        var maxLengthBinaryFacetValue = legacyTypeUsage.Facets[MaxLengthFacetName].Value;

                        return
                            maxLengthBinaryFacetValue == TypeUsageHelper.LegacyUnboundedValue
                                ? TypeUsage.CreateBinaryTypeUsage(
                                    primitiveEdmType,
                                    (bool)fixedLengthFacetValue)
                                : TypeUsage.CreateBinaryTypeUsage(
                                    primitiveEdmType,
                                    (bool)fixedLengthFacetValue,
                                    (int)maxLengthBinaryFacetValue);
                    }

                case LegacyMetadata.PrimitiveTypeKind.DateTime:
                    return TypeUsage.CreateDateTimeTypeUsage(
                        primitiveEdmType,
                        (byte?)legacyTypeUsage.Facets[PrecisionFacetName].Value);

                case LegacyMetadata.PrimitiveTypeKind.DateTimeOffset:
                    return TypeUsage.CreateDateTimeOffsetTypeUsage(
                        primitiveEdmType,
                        (byte?)legacyTypeUsage.Facets[PrecisionFacetName].Value);

                case LegacyMetadata.PrimitiveTypeKind.Time:
                    return TypeUsage.CreateTimeTypeUsage(
                        primitiveEdmType,
                        (byte?)legacyTypeUsage.Facets[PrecisionFacetName].Value);

                case LegacyMetadata.PrimitiveTypeKind.String:
                    var maxLengthStringFacetValue = legacyTypeUsage.Facets[MaxLengthFacetName].Value;
                    if (maxLengthStringFacetValue == null)
                    {
                        return TypeUsage.CreateDefaultTypeUsage(primitiveEdmType);
                    }
                    else
                    {
                        return maxLengthStringFacetValue == TypeUsageHelper.LegacyUnboundedValue
                                   ? TypeUsage.CreateStringTypeUsage(
                                       primitiveEdmType,
                                       (bool)legacyTypeUsage.Facets[UnicodeFacetName].Value,
                                       (bool)legacyTypeUsage.Facets[FixedLengthFacetName].Value)
                                   : TypeUsage.CreateStringTypeUsage(
                                       primitiveEdmType,
                                       (bool)legacyTypeUsage.Facets[UnicodeFacetName].Value,
                                       (bool)legacyTypeUsage.Facets[FixedLengthFacetName].Value,
                                       (int)maxLengthStringFacetValue);
                    }
            }

            Debug.Fail("Unknown primitive type kind.");

            throw new NotSupportedException();
        }

        private TypeUsage ConvertFromLegacyStoreTypeUsage(LegacyMetadata.TypeUsage legacyStoreTypeUsage)
        {
            Debug.Assert(legacyStoreTypeUsage != null, "legacyStoreTypeUsage != null");
            Debug.Assert(legacyStoreTypeUsage.EdmType is LegacyMetadata.PrimitiveType, "primitive type expected");
            Debug.Assert(
                (LegacyMetadata.DataSpace)typeof(LegacyMetadata.EdmType)
                                              .GetProperty("DataSpace", BindingFlags.Instance | BindingFlags.NonPublic)
                                              .GetValue(legacyStoreTypeUsage.EdmType) == LegacyMetadata.DataSpace.SSpace,
                "Expected SSpace type.");

            var legacyStorePrimitiveType = (LegacyMetadata.PrimitiveType)legacyStoreTypeUsage.EdmType;
            var storePrimitiveType = _storeTypes.Single(t => t.FullName == legacyStorePrimitiveType.FullName);

            switch (storePrimitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Boolean:
                case PrimitiveTypeKind.Byte:
                case PrimitiveTypeKind.SByte:
                case PrimitiveTypeKind.Int16:
                case PrimitiveTypeKind.Int32:
                case PrimitiveTypeKind.Int64:
                case PrimitiveTypeKind.Guid:
                case PrimitiveTypeKind.Double:
                case PrimitiveTypeKind.Single:
                case PrimitiveTypeKind.Geography:
                case PrimitiveTypeKind.GeographyPoint:
                case PrimitiveTypeKind.GeographyLineString:
                case PrimitiveTypeKind.GeographyPolygon:
                case PrimitiveTypeKind.GeographyMultiPoint:
                case PrimitiveTypeKind.GeographyMultiLineString:
                case PrimitiveTypeKind.GeographyMultiPolygon:
                case PrimitiveTypeKind.GeographyCollection:
                case PrimitiveTypeKind.Geometry:
                case PrimitiveTypeKind.GeometryPoint:
                case PrimitiveTypeKind.GeometryLineString:
                case PrimitiveTypeKind.GeometryPolygon:
                case PrimitiveTypeKind.GeometryMultiPoint:
                case PrimitiveTypeKind.GeometryMultiLineString:
                case PrimitiveTypeKind.GeometryMultiPolygon:
                case PrimitiveTypeKind.GeometryCollection:
                    return TypeUsage.CreateDefaultTypeUsage(storePrimitiveType);

                case PrimitiveTypeKind.Decimal:
                    return TypeUsage.CreateDecimalTypeUsage(
                        storePrimitiveType,
                        (byte)legacyStoreTypeUsage.Facets[PrecisionFacetName].Value,
                        (byte)legacyStoreTypeUsage.Facets[ScaleFacetName].Value);

                case PrimitiveTypeKind.Binary:
                    return TypeUsage.CreateBinaryTypeUsage(
                        storePrimitiveType,
                        (bool)legacyStoreTypeUsage.Facets[FixedLengthFacetName].Value,
                        (int)legacyStoreTypeUsage.Facets[MaxLengthFacetName].Value);

                case PrimitiveTypeKind.DateTime:
                    return TypeUsage.CreateDateTimeTypeUsage(
                        storePrimitiveType,
                        (byte?)legacyStoreTypeUsage.Facets[PrecisionFacetName].Value);

                case PrimitiveTypeKind.DateTimeOffset:
                    return TypeUsage.CreateDateTimeOffsetTypeUsage(
                        storePrimitiveType,
                        (byte?)legacyStoreTypeUsage.Facets[PrecisionFacetName].Value);

                case PrimitiveTypeKind.Time:
                    return TypeUsage.CreateTimeTypeUsage(
                        storePrimitiveType,
                        (byte?)legacyStoreTypeUsage.Facets[PrecisionFacetName].Value);

                case PrimitiveTypeKind.String:
                    return TypeUsage.CreateStringTypeUsage(
                        storePrimitiveType,
                        (bool)legacyStoreTypeUsage.Facets[UnicodeFacetName].Value,
                        (bool)legacyStoreTypeUsage.Facets[FixedLengthFacetName].Value,
                        (int)legacyStoreTypeUsage.Facets[MaxLengthFacetName].Value);
            }

            Debug.Fail("Unknown primitive type kind.");

            throw new NotSupportedException();
        }

        private static PrimitiveType FromLegacyPrimitiveType(LegacyMetadata.PrimitiveType legacyPrimitiveType)
        {
            return
                PrimitiveType.GetEdmPrimitiveType((PrimitiveTypeKind)(int)legacyPrimitiveType.PrimitiveTypeKind);
        }
    }
}
