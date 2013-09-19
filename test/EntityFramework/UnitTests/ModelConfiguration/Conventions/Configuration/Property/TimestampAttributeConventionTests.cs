// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
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
            binaryPropertyConfiguration.Configure(EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            Assert.Equal("rowversion", binaryPropertyConfiguration.ColumnType);
            Assert.Equal(false, binaryPropertyConfiguration.IsNullable);
            Assert.Equal(ConcurrencyMode.Fixed, binaryPropertyConfiguration.ConcurrencyMode);
            Assert.Equal(DatabaseGeneratedOption.Computed, binaryPropertyConfiguration.DatabaseGeneratedOption);
            Assert.Equal(8, binaryPropertyConfiguration.MaxLength.Value);
        }
    }
}
