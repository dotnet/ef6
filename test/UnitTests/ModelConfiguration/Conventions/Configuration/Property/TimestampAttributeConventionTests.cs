// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using Moq;
    using Xunit;
    using BinaryPropertyConfiguration = System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration;

    public sealed class TimestampAttributeConventionTests
    {
        [Fact]
        public void Apply_should_set_timestamp_when_facets_empty()
        {
            var propertyConfiguration = new BinaryPropertyConfiguration();

            new TimestampAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration), new TimestampAttribute());

            Assert_Timestamp(propertyConfiguration);
        }

        [Fact]
        public void Apply_should_set_timestamp_when_length_set()
        {
            var propertyConfiguration = new BinaryPropertyConfiguration
                                            {
                                                MaxLength = 8
                                            };

            new TimestampAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration), new TimestampAttribute());

            Assert_Timestamp(propertyConfiguration);
        }

        [Fact]
        public void Apply_should_set_timestamp_when_required_set()
        {
            var propertyConfiguration = new BinaryPropertyConfiguration
                                            {
                                                IsNullable = false
                                            };

            new TimestampAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration), new TimestampAttribute());

            Assert_Timestamp(propertyConfiguration);
        }

        [Fact]
        public void Apply_should_set_timestamp_when_concurrency_token_set()
        {
            var propertyConfiguration = new BinaryPropertyConfiguration
                                            {
                                                ConcurrencyMode = ConcurrencyMode.Fixed
                                            };

            new TimestampAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration), new TimestampAttribute());

            Assert_Timestamp(propertyConfiguration);
        }

        [Fact]
        public void Apply_should_set_timestamp_when_rowversion_set()
        {
            var propertyConfiguration = new BinaryPropertyConfiguration
                                            {
                                                ColumnType = "rowversion"
                                            };

            new TimestampAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration), new TimestampAttribute());

            Assert_Timestamp(propertyConfiguration);
        }

        [Fact]
        public void Apply_should_not_set_timestamp_when_identity()
        {
            var propertyConfiguration = new BinaryPropertyConfiguration
                                            {
                                                DatabaseGeneratedOption = DatabaseGeneratedOption.Identity
                                            };

            new TimestampAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration), new TimestampAttribute());

            Assert.Null(propertyConfiguration.ColumnType);
        }

        [Fact]
        public void Apply_should_not_set_timestamp_when_maxLength()
        {
            var propertyConfiguration = new BinaryPropertyConfiguration
                                            {
                                                MaxLength = 100
                                            };

            new TimestampAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration), new TimestampAttribute());

            Assert.Null(propertyConfiguration.ColumnType);
        }

        private void Assert_Timestamp(BinaryPropertyConfiguration binaryPropertyConfiguration)
        {
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            binaryPropertyConfiguration.Configure(property);

            Assert.Equal("String", property.TypeName);
            Assert.Equal(false, property.Nullable);
            Assert.Equal(ConcurrencyMode.Fixed, property.ConcurrencyMode);
            Assert.Equal(StoreGeneratedPattern.Computed, property.GetStoreGeneratedPattern());
            Assert.Equal(8, property.MaxLength.Value);
            
            var primitiveType = new PrimitiveType();
            EdmType.Initialize(primitiveType, "rowversion", "N", DataSpace.SSpace, false, null);
            var mockDbProviderManifest = new Mock<DbProviderManifest>();
            mockDbProviderManifest.Setup(m => m.GetStoreTypes())
                .Returns(new ReadOnlyCollection<PrimitiveType>(new List<PrimitiveType> { primitiveType }));
            mockDbProviderManifest.Setup(m => m.GetFacetDescriptions(It.IsAny<EdmType>()))
                .Returns(new ReadOnlyCollection<FacetDescription>(new List<FacetDescription>()));
            PrimitiveType.Initialize(primitiveType, PrimitiveTypeKind.Binary, mockDbProviderManifest.Object);

            var column = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            binaryPropertyConfiguration.Configure(
                column,
                new EntityType("E", "N", DataSpace.SSpace),
                mockDbProviderManifest.Object);

            Assert.Equal("rowversion", column.TypeName);
            Assert.Equal(true, column.Nullable);
            Assert.Equal(ConcurrencyMode.None, column.ConcurrencyMode);
            Assert.Null(column.GetStoreGeneratedPattern());
            Assert.Null(column.MaxLength);
        }
    }
}
