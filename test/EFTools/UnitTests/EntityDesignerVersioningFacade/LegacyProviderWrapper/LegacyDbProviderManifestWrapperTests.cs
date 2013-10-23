// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using SystemData = System.Data;
using Legacy = System.Data.Common;
using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class LegacyDbProviderManifestWrapperTests
    {
        // Using real DbProviderManifest implementation since mocking 
        // legacy EdmType and TypeUsage types is not possible
        private static readonly Legacy.DbProviderManifest LegacyProviderManifest;
        private static readonly DbProviderManifest ProviderManifestWrapper;

        private static readonly Dictionary<string, PrimitiveType> EdmPrimitiveTypes;
        private static readonly Dictionary<string, LegacyMetadata.PrimitiveType> LegacyEdmPrimitiveTypes;

        static LegacyDbProviderManifestWrapperTests()
        {
            LegacyProviderManifest =
                ((Legacy.DbProviderServices)
                 ((IServiceProvider)Legacy.DbProviderFactories.GetFactory("System.Data.SqlClient"))
                     .GetService(typeof(Legacy.DbProviderServices)))
                    .GetProviderManifest("2008");

            ProviderManifestWrapper = new LegacyDbProviderManifestWrapper(LegacyProviderManifest);

            const string emptyCsdl =
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""dummy"" />";

            using (var reader = XmlReader.Create(new StringReader(emptyCsdl)))
            {
                EdmPrimitiveTypes =
                    new EdmItemCollection(new[] { reader }).GetItems<PrimitiveType>().ToDictionary(t => t.Name, t => t);
            }

            using (var reader = XmlReader.Create(new StringReader(emptyCsdl)))
            {
                LegacyEdmPrimitiveTypes =
                    new LegacyMetadata.EdmItemCollection(new[] { reader })
                        .GetItems<LegacyMetadata.PrimitiveType>()
                        .ToDictionary(t => t.Name, t => t);
            }
        }

        [Fact]
        public void GetStoreTypes_returns_all_store_types_correctly()
        {
            var storeTypes =
                new LegacyDbProviderManifestWrapper(LegacyProviderManifest)
                    .GetStoreTypes()
                    .OrderBy(t => t.Name)
                    .ToArray();

            var legacyStoreTypes =
                LegacyProviderManifest
                    .GetStoreTypes()
                    .OrderBy(t => t.Name)
                    .ToArray();

            Assert.Equal(storeTypes.Length, legacyStoreTypes.Length);

            for (var idx = 0; idx < storeTypes.Length; idx++)
            {
                TypeUsageVerificationHelper.VerifyEdmTypesEquivalent(legacyStoreTypes[idx], storeTypes[idx]);
            }
        }

        [Fact]
        public void GetStoreTypes_converts_legacy_ProviderIncompatibleException_to_non_legacy_ProviderIncompatibleException()
        {
            var expectedInnerException = new InvalidOperationException();

            var mockLegacyManifest = new Mock<Legacy.DbProviderManifest>();
            mockLegacyManifest
                .Setup(m => m.GetStoreTypes())
                .Returns(LegacyProviderManifest.GetStoreTypes());

            mockLegacyManifest
                .Setup(m => m.GetFacetDescriptions(It.IsAny<LegacyMetadata.EdmType>()))
                .Throws(new SystemData.ProviderIncompatibleException("Test", expectedInnerException));

            var providerManifestWrapper = new LegacyDbProviderManifestWrapper(mockLegacyManifest.Object);

            // need an SSpace type as the arguemnt
            var edmType = new LegacyDbProviderManifestWrapper(LegacyProviderManifest).GetStoreTypes().First();

            var exception =
                Assert.Throws<ProviderIncompatibleException>(
                    () => providerManifestWrapper.GetFacetDescriptions(edmType));

            Assert.Equal("Test", exception.Message);
            Assert.Same(expectedInnerException, exception.InnerException);
        }

        [Fact]
        public void GetStoreType_returns_correct_default_type_usages_for_all_primitive_types()
        {
            // SqlProvider does not support SByte
            foreach (var legacyEdmPrimitiveType in LegacyEdmPrimitiveTypes.Where(t => t.Key != "SByte"))
            {
                TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateDefaultTypeUsage(legacyEdmPrimitiveType.Value)),
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateDefaultTypeUsage(EdmPrimitiveTypes[legacyEdmPrimitiveType.Key])));
            }
        }

        [Fact]
        public void GetStoreType_returns_correct_type_usages_for_specific_Binary_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateBinaryTypeUsage(LegacyEdmPrimitiveTypes["Binary"], isFixedLength: false)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateBinaryTypeUsage(EdmPrimitiveTypes["Binary"], isFixedLength: false)));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateBinaryTypeUsage(LegacyEdmPrimitiveTypes["Binary"], isFixedLength: true)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateBinaryTypeUsage(EdmPrimitiveTypes["Binary"], isFixedLength: true)));
        }

        [Fact]
        public void GetStoreType_returns_correct_type_usages_for_specific_DateTime_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateDateTimeTypeUsage(LegacyEdmPrimitiveTypes["DateTime"], precision: null)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateDateTimeTypeUsage(EdmPrimitiveTypes["DateTime"], precision: null)));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateDateTimeTypeUsage(LegacyEdmPrimitiveTypes["DateTime"], precision: 4)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateDateTimeTypeUsage(EdmPrimitiveTypes["DateTime"], precision: 4)));
        }

        [Fact]
        public void GetStoreType_returns_correct_type_usages_for_specific_DateTimeOffset_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateDateTimeOffsetTypeUsage(LegacyEdmPrimitiveTypes["DateTimeOffset"], precision: null)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateDateTimeOffsetTypeUsage(EdmPrimitiveTypes["DateTimeOffset"], precision: null)));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateDateTimeOffsetTypeUsage(LegacyEdmPrimitiveTypes["DateTimeOffset"], precision: 6)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateDateTimeOffsetTypeUsage(EdmPrimitiveTypes["DateTimeOffset"], precision: 6)));
        }

        [Fact]
        public void GetStoreType_returns_correct_type_usages_for_specific_Time_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateTimeTypeUsage(LegacyEdmPrimitiveTypes["Time"], precision: null)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateTimeTypeUsage(EdmPrimitiveTypes["Time"], precision: null)));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateTimeTypeUsage(LegacyEdmPrimitiveTypes["Time"], precision: 6)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateTimeTypeUsage(EdmPrimitiveTypes["Time"], precision: 6)));
        }

        [Fact]
        public void GetStoreType_returns_correct_type_usages_for_specific_Decimal_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateDecimalTypeUsage(LegacyEdmPrimitiveTypes["Decimal"])),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateDecimalTypeUsage(EdmPrimitiveTypes["Decimal"])));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetStoreType(
                    LegacyMetadata.TypeUsage.CreateDecimalTypeUsage(LegacyEdmPrimitiveTypes["Decimal"], precision: 6, scale: 10)),
                ProviderManifestWrapper.GetStoreType(
                    TypeUsage.CreateDecimalTypeUsage(EdmPrimitiveTypes["Decimal"], precision: 6, scale: 10)));
        }

        [Fact]
        public void GetStoreType_returns_correct_type_usages_for_specific_String_type_usages()
        {
            foreach (var isUnicode in new[] { true, false })
            {
                foreach (var isFixedLength in new[] { true, false })
                {
                    TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                        LegacyProviderManifest.GetStoreType(
                            LegacyMetadata.TypeUsage.CreateStringTypeUsage(LegacyEdmPrimitiveTypes["String"], isUnicode, isFixedLength)),
                        ProviderManifestWrapper.GetStoreType(
                            TypeUsage.CreateStringTypeUsage(EdmPrimitiveTypes["String"], isUnicode, isFixedLength)));

                    TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                        LegacyProviderManifest.GetStoreType(
                            LegacyMetadata.TypeUsage.CreateStringTypeUsage(
                                LegacyEdmPrimitiveTypes["String"], isUnicode, isFixedLength, maxLength: 1000)),
                        ProviderManifestWrapper.GetStoreType(
                            TypeUsage.CreateStringTypeUsage(EdmPrimitiveTypes["String"], isUnicode, isFixedLength, maxLength: 1000)));
                }
            }
        }

        [Fact]
        public void GetStoreType_converts_legacy_ProviderIncompatibleException_to_non_legacy_ProviderIncompatibleException()
        {
            var expectedInnerException = new InvalidOperationException();

            var mockLegacyManifest = new Mock<Legacy.DbProviderManifest>();
            mockLegacyManifest
                .Setup(m => m.GetStoreTypes())
                .Returns(LegacyProviderManifest.GetStoreTypes());

            mockLegacyManifest
                .Setup(m => m.GetStoreType(It.IsAny<LegacyMetadata.TypeUsage>()))
                .Throws(new SystemData.ProviderIncompatibleException("Test", expectedInnerException));

            var providerManifestWrapper = new LegacyDbProviderManifestWrapper(mockLegacyManifest.Object);

            var exception =
                Assert.Throws<ProviderIncompatibleException>(
                    () => providerManifestWrapper.GetStoreType(TypeUsage.CreateDefaultTypeUsage(EdmPrimitiveTypes["String"])));

            Assert.Equal("Test", exception.Message);
            Assert.Same(expectedInnerException, exception.InnerException);
        }

        [Fact]
        public void GetFacetDescriptions_returns_correct_facets_for_all_primitive_types()
        {
            var storeTypes =
                ProviderManifestWrapper
                    .GetStoreTypes()
                    .OrderBy(t => t.Name)
                    .ToArray();

            var legacyStoreTypes =
                LegacyProviderManifest
                    .GetStoreTypes()
                    .OrderBy(t => t.Name)
                    .ToArray();

            for (var i = 0; i < storeTypes.Length; i++)
            {
                var facetDescriptions =
                    ProviderManifestWrapper.GetFacetDescriptions(storeTypes[i]).OrderBy(f => f.FacetName).ToArray();
                var legacyFacetDescriptions =
                    LegacyProviderManifest.GetFacetDescriptions(legacyStoreTypes[i]).OrderBy(f => f.FacetName).ToArray();

                Assert.Equal(facetDescriptions.Length, legacyFacetDescriptions.Length);

                for (var j = 0; j < facetDescriptions.Count(); j++)
                {
                    TypeUsageVerificationHelper.VerifyFacetDescriptionsEquivalent(facetDescriptions[j], legacyFacetDescriptions[j]);
                }
            }
        }

        [Fact]
        public void GetEdmType_returns_correct_default_type_usages_for_all_primitive_types()
        {
            var storeTypes = ProviderManifestWrapper.GetStoreTypes().ToDictionary(t => t.Name, t => t);

            foreach (var legacyStoreType in LegacyProviderManifest.GetStoreTypes())
            {
                TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                    LegacyProviderManifest.GetEdmType(LegacyMetadata.TypeUsage.CreateDefaultTypeUsage(legacyStoreType)),
                    ProviderManifestWrapper.GetEdmType(TypeUsage.CreateDefaultTypeUsage(storeTypes[legacyStoreType.Name])));
            }
        }

        [Fact]
        public void GetEdmType_returns_correct_type_usages_for_specific_Binary_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateBinaryTypeUsage(LegacyEdmPrimitiveTypes["Binary"], isFixedLength: false))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateBinaryTypeUsage(EdmPrimitiveTypes["Binary"], isFixedLength: false))));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateBinaryTypeUsage(LegacyEdmPrimitiveTypes["Binary"], isFixedLength: true))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateBinaryTypeUsage(EdmPrimitiveTypes["Binary"], isFixedLength: true))));
        }

        [Fact]
        public void GetEdmType_returns_correct_type_usages_for_specific_DateTime_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateDateTimeTypeUsage(LegacyEdmPrimitiveTypes["DateTime"], precision: null))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateDateTimeTypeUsage(EdmPrimitiveTypes["DateTime"], precision: null))));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateDateTimeTypeUsage(LegacyEdmPrimitiveTypes["DateTime"], precision: 6))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateDateTimeTypeUsage(EdmPrimitiveTypes["DateTime"], precision: 6))));
        }

        [Fact]
        public void GetEdmType_returns_correct_type_usages_for_specific_DateTimeOffset_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateDateTimeOffsetTypeUsage(LegacyEdmPrimitiveTypes["DateTimeOffset"], precision: null))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateDateTimeOffsetTypeUsage(EdmPrimitiveTypes["DateTimeOffset"], precision: null))));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateDateTimeOffsetTypeUsage(LegacyEdmPrimitiveTypes["DateTimeOffset"], precision: 8))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateDateTimeOffsetTypeUsage(EdmPrimitiveTypes["DateTimeOffset"], precision: 8))));
        }

        [Fact]
        public void GetEdmType_returns_correct_type_usages_for_specific_Time_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateTimeTypeUsage(LegacyEdmPrimitiveTypes["Time"], precision: null))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateTimeTypeUsage(EdmPrimitiveTypes["Time"], precision: null))));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateTimeTypeUsage(LegacyEdmPrimitiveTypes["Time"], precision: 6))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateTimeTypeUsage(EdmPrimitiveTypes["Time"], precision: 6))));
        }

        [Fact]
        public void GetEdmType_returns_correct_type_usages_for_specific_Decimal_type_usages()
        {
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateDecimalTypeUsage(LegacyEdmPrimitiveTypes["Decimal"]))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateDecimalTypeUsage(EdmPrimitiveTypes["Decimal"]))));

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                LegacyProviderManifest.GetEdmType(
                    LegacyProviderManifest.GetStoreType(
                        LegacyMetadata.TypeUsage.CreateDecimalTypeUsage(LegacyEdmPrimitiveTypes["Decimal"], precision: 6, scale: 10))),
                ProviderManifestWrapper.GetEdmType(
                    ProviderManifestWrapper.GetStoreType(
                        TypeUsage.CreateDecimalTypeUsage(EdmPrimitiveTypes["Decimal"], precision: 6, scale: 10))));
        }

        [Fact]
        public void GetEdmType_returns_correct_type_usages_for_specific_String_type_usages()
        {
            foreach (var isUnicode in new[] { true, false })
            {
                foreach (var isFixedLength in new[] { true, false })
                {
                    TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                        LegacyProviderManifest.GetEdmType(
                            LegacyProviderManifest.GetStoreType(
                                LegacyMetadata.TypeUsage.CreateStringTypeUsage(LegacyEdmPrimitiveTypes["String"], isUnicode, isFixedLength))),
                        ProviderManifestWrapper.GetEdmType(
                            ProviderManifestWrapper.GetStoreType(
                                TypeUsage.CreateStringTypeUsage(EdmPrimitiveTypes["String"], isUnicode, isFixedLength))));

                    TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(
                        LegacyProviderManifest.GetEdmType(
                            LegacyProviderManifest.GetStoreType(
                                LegacyMetadata.TypeUsage.CreateStringTypeUsage(
                                    LegacyEdmPrimitiveTypes["String"], isUnicode, isFixedLength, maxLength: 1000))),
                        ProviderManifestWrapper.GetEdmType(
                            ProviderManifestWrapper.GetStoreType(
                                TypeUsage.CreateStringTypeUsage(EdmPrimitiveTypes["String"], isUnicode, isFixedLength, maxLength: 1000))));
                }
            }
        }

        [Fact]
        public void GetEdmType_converts_legacy_ProviderIncompatibleException_to_non_legacy_ProviderIncompatibleException()
        {
            var expectedInnerException = new InvalidOperationException();

            var mockLegacyManifest = new Mock<Legacy.DbProviderManifest>();
            mockLegacyManifest
                .Setup(m => m.GetStoreTypes())
                .Returns(LegacyProviderManifest.GetStoreTypes());

            mockLegacyManifest
                .Setup(m => m.GetEdmType(It.IsAny<LegacyMetadata.TypeUsage>()))
                .Throws(new SystemData.ProviderIncompatibleException("Test", expectedInnerException));

            var providerManifestWrapper = new LegacyDbProviderManifestWrapper(mockLegacyManifest.Object);

            var exception =
                Assert.Throws<ProviderIncompatibleException>(
                    () => providerManifestWrapper.GetEdmType(
                        ProviderManifestWrapper.GetStoreType(
                            TypeUsage.CreateDefaultTypeUsage(EdmPrimitiveTypes["String"]))));

            Assert.Equal("Test", exception.Message);
            Assert.Same(expectedInnerException, exception.InnerException);
        }

        [Fact]
        public void NamespaceName_returns_NamespaceName_of_wrapped_manifest()
        {
            var mockLegacyManifest = new Mock<Legacy.DbProviderManifest>();
            mockLegacyManifest.Setup(p => p.NamespaceName).Returns("Namespace");

            mockLegacyManifest
                .Setup(m => m.GetStoreTypes())
                .Returns(LegacyProviderManifest.GetStoreTypes());

            Assert.Equal("Namespace", new LegacyDbProviderManifestWrapper(mockLegacyManifest.Object).NamespaceName);
        }

        [Fact]
        public void Can_get_wrapped_provider_manifest()
        {
            var mockLegacyManifest = new Mock<Legacy.DbProviderManifest>();

            mockLegacyManifest
                .Setup(m => m.GetStoreTypes())
                .Returns(LegacyProviderManifest.GetStoreTypes());

            Assert.Same(
                mockLegacyManifest.Object,
                new LegacyDbProviderManifestWrapper(mockLegacyManifest.Object).WrappedManifest);
        }

        [Fact]
        public void GetDbInformation_returns_db_information_from_wrapped_manifest()
        {
            var mockXmlReader = new Mock<XmlReader>();

            var mockLegacyManifest = new Mock<Legacy.DbProviderManifest>();

            mockLegacyManifest
                .Setup(m => m.GetStoreTypes())
                .Returns(LegacyProviderManifest.GetStoreTypes());

            mockLegacyManifest
                .Protected()
                .Setup<XmlReader>("GetDbInformation", ItExpr.IsAny<string>())
                .Returns(mockXmlReader.Object);

            Assert.Same(
                mockXmlReader.Object,
                new LegacyDbProviderManifestWrapper(mockLegacyManifest.Object).GetInformation(string.Empty));
        }

        [Fact]
        public void GetDbInformation_unwraps_legacy_ProviderIncompatibleException()
        {
            var expectedInnerException = new InvalidOperationException();

            var mockLegacyManifest = new Mock<Legacy.DbProviderManifest>();

            mockLegacyManifest
                .Setup(m => m.GetStoreTypes())
                .Returns(LegacyProviderManifest.GetStoreTypes());

            mockLegacyManifest
                .Protected()
                .Setup<XmlReader>("GetDbInformation", ItExpr.IsAny<string>())
                .Throws(expectedInnerException);

            try
            {
                new LegacyDbProviderManifestWrapper(mockLegacyManifest.Object).GetInformation(string.Empty);

                throw new InvalidOperationException("Expected exception but none thrown.");
            }
            catch (ProviderIncompatibleException exception)
            {
                Assert.Same(expectedInnerException, exception.InnerException);
            }
        }
    }
}
