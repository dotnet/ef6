// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    internal static class TypeUsageExtensions
    {
        private static readonly ConstructorInfo LegacyEdmPropertyCtor =
            typeof(LegacyMetadata.EdmProperty)
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                        {
                            typeof(string),
                            typeof(LegacyMetadata.TypeUsage)
                        },
                    null);

        private static readonly ConstructorInfo LegacyRowTypeCtor =
            typeof(LegacyMetadata.RowType)
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                        {
                            typeof(IEnumerable<LegacyMetadata.EdmProperty>)
                        },
                    null);

        private static readonly ConstructorInfo LegacyTypeUsageCtor =
            typeof(LegacyMetadata.TypeUsage)
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                        {
                            typeof(LegacyMetadata.EdmType),
                            typeof(IEnumerable<LegacyMetadata.Facet>)
                        },
                    null);

        private static readonly MethodInfo LegacyFacetFactoryMethod =
            typeof(LegacyMetadata.Facet)
                .GetMethod(
                    "Create",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[]
                        {
                            /* facetDescription */ typeof(LegacyMetadata.FacetDescription),
                            /* value */ typeof(object)
                        },
                    null);

        private static readonly ConstructorInfo LegacyFacetDescriptionCtor =
            typeof(LegacyMetadata.FacetDescription)
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                        {
                            /* facetName */ typeof(string),
                            /* facetType */ typeof(LegacyMetadata.EdmType),
                            /* minValue */ typeof(int?),
                            /* maxValue */ typeof(int?),
                            /* defaultValue */ typeof(object),
                            /* isConstant */ typeof(bool),
                            /* declaringTypeName */ typeof(string)
                        },
                    null);

        private static readonly LegacyMetadata.FacetDescription ConcurrencyModeFacetDescription;
        private static readonly LegacyMetadata.FacetDescription StoreGeneratedPatternFacetDescription;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static TypeUsageExtensions()
        {
            var converterType =
                typeof(LegacyMetadata.FacetDescription)
                    .Assembly.
                    GetType("System.Data.Metadata.Edm.Converter", throwOnError: true, ignoreCase: false);

            ConcurrencyModeFacetDescription =
                (LegacyMetadata.FacetDescription)converterType
                                                     .GetField("ConcurrencyModeFacet", BindingFlags.Static | BindingFlags.NonPublic)
                                                     .GetValue(null);

            StoreGeneratedPatternFacetDescription =
                (LegacyMetadata.FacetDescription)converterType
                                                     .GetField("StoreGeneratedPatternFacet", BindingFlags.Static | BindingFlags.NonPublic)
                                                     .GetValue(null);
        }

        public static LegacyMetadata.TypeUsage ToLegacyEdmTypeUsage(this TypeUsage typeUsage)
        {
            Debug.Assert(typeUsage != null, "typeUsage != null");
            Debug.Assert(typeUsage.EdmType.GetDataSpace() == DataSpace.CSpace, "Expected CSpace type.");

            LegacyMetadata.EdmType edmType;

            if (typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                edmType = ((PrimitiveType)typeUsage.EdmType).ToLegacyEdmPrimitiveType();
            }
            else
            {
                Debug.Assert(typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType, "Unsupported EdmType.");

                edmType = ((RowType)typeUsage.EdmType).ToLegacyRowType();
            }

            return CreateLegacyTypeUsage(edmType, typeUsage.Facets.Select(f => f.ToLegacyFacet()));
        }

        public static LegacyMetadata.TypeUsage ToLegacyStoreTypeUsage(
            this TypeUsage typeUsage, LegacyMetadata.EdmType[] legacyStoreTypes)
        {
            Debug.Assert(typeUsage != null, "typeUsage != null");
            Debug.Assert(
                typeUsage.EdmType.GetDataSpace() == DataSpace.SSpace ||
                (typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType &&
                 (int)typeUsage.EdmType.GetDataSpace() == -1),
                "Expected SSpace type.");
            Debug.Assert(legacyStoreTypes != null, "legacyStoreTypes != null");

            if (typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                var storePrimitiveType =
                    (LegacyMetadata.PrimitiveType)
                    legacyStoreTypes
                        .Single(
                            t =>
                            t.BuiltInTypeKind == LegacyMetadata.BuiltInTypeKind.PrimitiveType &&
                            t.FullName == typeUsage.EdmType.FullName);

                return CreateLegacyTypeUsage(storePrimitiveType, typeUsage.Facets.Select(f => f.ToLegacyFacet()));
            }
            else if (typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
            {
                return CreateLegacyTypeUsage(
                    ((RowType)typeUsage.EdmType).ToLegacyRowType(legacyStoreTypes),
                    typeUsage.Facets.Select(f => f.ToLegacyFacet()));
            }
            else
            {
                Debug.Assert(typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.EntityType, "Expected entity type.");

                var storeEntityType =
                    (LegacyMetadata.EntityType)
                    legacyStoreTypes
                        .Single(
                            t =>
                            t.BuiltInTypeKind == LegacyMetadata.BuiltInTypeKind.EntityType &&
                            t.FullName == typeUsage.EdmType.FullName);

                return LegacyMetadata.TypeUsage.CreateDefaultTypeUsage(storeEntityType);
            }
        }

        private static LegacyMetadata.TypeUsage CreateLegacyTypeUsage
            (LegacyMetadata.EdmType edmType, IEnumerable<LegacyMetadata.Facet> facets)
        {
            return (LegacyMetadata.TypeUsage)LegacyTypeUsageCtor.Invoke(
                BindingFlags.CreateInstance,
                null,
                new object[]
                    {
                        edmType,
                        facets
                    },
                CultureInfo.InvariantCulture);
        }

        private static LegacyMetadata.Facet ToLegacyFacet(this Facet facet)
        {
            LegacyMetadata.FacetDescription facetDescription;
            object value;

            if (facet.Name == "ConcurrencyMode")
            {
                facetDescription = ConcurrencyModeFacetDescription;
                value = (LegacyMetadata.ConcurrencyMode)(int)facet.Value;
            }
            else if (facet.Name == "StoreGeneratedPattern")
            {
                facetDescription = StoreGeneratedPatternFacetDescription;
                value = (LegacyMetadata.StoreGeneratedPattern)(int)facet.Value;
            }
            else
            {
                facetDescription = facet.Description.ToLegacyFacetDescription();
                value =
                    facet.Value == TypeUsageHelper.UnboundedValue
                        ? TypeUsageHelper.LegacyUnboundedValue
                        : facet.Value == TypeUsageHelper.VariableValue
                              ? TypeUsageHelper.LegacyVariableValue
                              : facet.Value;
            }

            return
                (LegacyMetadata.Facet)LegacyFacetFactoryMethod.Invoke(
                    null,
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[]
                        {
                            facetDescription,
                            value
                        },
                    CultureInfo.InvariantCulture);
        }

        private static LegacyMetadata.FacetDescription ToLegacyFacetDescription(this FacetDescription facetDescription)
        {
            Debug.Assert(facetDescription != null, "facet != null");

            return
                LegacyMetadata.MetadataItem.GetGeneralFacetDescriptions()
                    .SingleOrDefault(f => f.FacetName == facetDescription.FacetName) ??
                (LegacyMetadata.FacetDescription)LegacyFacetDescriptionCtor.Invoke(
                    BindingFlags.CreateInstance,
                    null,
                    new[]
                        {
                            facetDescription.FacetName,
                            ((PrimitiveType)facetDescription.FacetType).ToLegacyEdmPrimitiveType(),
                            facetDescription.MinValue,
                            facetDescription.MaxValue,
                            facetDescription.DefaultValue == TypeUsageHelper.VariableValue
                                ? TypeUsageHelper.LegacyVariableValue
                                : facetDescription.DefaultValue,
                            facetDescription.IsConstant,
                            // declaringTypeName is used only in exception message if validation fails
                            /* declaringTypeName */ string.Empty
                        },
                    CultureInfo.InvariantCulture);
        }

        private static LegacyMetadata.EdmType ToLegacyEdmPrimitiveType(this PrimitiveType primitiveType)
        {
            Debug.Assert(primitiveType != null, "primitiveType != null");

            return LegacyMetadata.PrimitiveType.GetEdmPrimitiveType(
                (LegacyMetadata.PrimitiveTypeKind)(int)primitiveType.PrimitiveTypeKind);
        }

        private static LegacyMetadata.RowType ToLegacyRowType(
            this RowType rowType,
            LegacyMetadata.EdmType[] legacyStoreTypes = null)
        {
            Debug.Assert(rowType != null, "rowType != null");

            var properties =
                rowType.Properties.Select(
                    p => (LegacyMetadata.EdmProperty)LegacyEdmPropertyCtor.Invoke(
                        BindingFlags.CreateInstance,
                        null,
                        new object[]
                            {
                                p.Name,
                                p.TypeUsage.EdmType.GetDataSpace() == DataSpace.CSpace
                                    ? p.TypeUsage.ToLegacyEdmTypeUsage()
                                    : p.TypeUsage.ToLegacyStoreTypeUsage(legacyStoreTypes)
                            },
                        CultureInfo.InvariantCulture)).ToArray();

            return
                (LegacyMetadata.RowType)LegacyRowTypeCtor.Invoke(
                    BindingFlags.CreateInstance,
                    null,
                    new object[]
                        {
                            properties
                        },
                    CultureInfo.InvariantCulture);
        }
    }
}
