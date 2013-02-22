// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class BinaryPropertyConfigurationTests : LengthPropertyConfigurationTests
    {
        [Fact]
        public void Configure_should_update_IsRowVersion()
        {
            var configuration = CreateConfiguration();
            configuration.IsRowVersion = true;
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            configuration.Configure(property);

            Assert.Equal(8, property.MaxLength);
            Assert.Equal(false, property.Nullable);
            Assert.Equal(ConcurrencyMode.Fixed, property.ConcurrencyMode);
            Assert.Equal(StoreGeneratedPattern.Computed, property.GetStoreGeneratedPattern());

            var edmPropertyMapping = new ColumnMappingBuilder(new EdmProperty("C"), new List<EdmProperty>());

            configuration.Configure(
                new[] { Tuple.Create(edmPropertyMapping, new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace)) },
                ProviderRegistry.Sql2008_ProviderManifest);
            Assert.Equal("rowversion", edmPropertyMapping.ColumnProperty.TypeName);
        }

        [Fact]
        public void CopyFrom_overwrites_null_IsRowVersion()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(true, configurationA.IsRowVersion);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_IsRowVersion()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsRowVersion = false;
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(true, configurationA.IsRowVersion);
        }

        [Fact]
        public void FillFrom_overwrites_null_IsRowVersion()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(true, configurationA.IsRowVersion);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_IsRowVersion()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsRowVersion = false;
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(false, configurationA.IsRowVersion);
        }

        [Fact]
        public void FillFrom_overwrites_null_IsRowVersion_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(true, configurationA.IsRowVersion);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_IsRowVersion_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsRowVersion = false;
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(false, configurationA.IsRowVersion);
        }

        [Fact]
        public void IsCompatible_returns_errors_for_all_mismatched_binary_properties()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "bar";
            configurationA.ColumnType = "bar";
            configurationA.ColumnOrder = 1;
            configurationA.IsNullable = true;
            configurationA.ConcurrencyMode = ConcurrencyMode.None;
            configurationA.DatabaseGeneratedOption = DatabaseGeneratedOption.Computed;
            configurationA.MaxLength = 1;
            configurationA.IsFixedLength = false;
            configurationA.IsMaxLength = false;
            configurationA.IsRowVersion = false;

            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";
            configurationB.ColumnType = "foo";
            configurationB.ColumnOrder = 2;
            configurationB.IsNullable = false;
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;
            configurationB.MaxLength = 2;
            configurationB.IsFixedLength = true;
            configurationB.IsMaxLength = true;
            configurationB.IsRowVersion = true;

            var expectedMessageCSpace = Environment.NewLine + "\t" +
                                        Strings.ConflictingConfigurationValue(
                                            "IsNullable", true, "IsNullable", false);

            expectedMessageCSpace += Environment.NewLine + "\t" +
                                     Strings.ConflictingConfigurationValue(
                                         "ConcurrencyMode", ConcurrencyMode.None, "ConcurrencyMode", ConcurrencyMode.Fixed);

            expectedMessageCSpace += Environment.NewLine + "\t" +
                                     Strings.ConflictingConfigurationValue(
                                         "DatabaseGeneratedOption", DatabaseGeneratedOption.Computed, "DatabaseGeneratedOption",
                                         DatabaseGeneratedOption.Identity);

            var expectedMessage = Environment.NewLine + "\t" +
                                  Strings.ConflictingConfigurationValue(
                                      "ColumnName", "bar", "ColumnName", "foo");

            expectedMessage += Environment.NewLine + "\t" +
                               Strings.ConflictingConfigurationValue(
                                   "ColumnOrder", 1, "ColumnOrder", 2);

            expectedMessage += Environment.NewLine + "\t" +
                               Strings.ConflictingConfigurationValue(
                                   "ColumnType", "bar", "ColumnType", "foo");

            var additionalErrors = Environment.NewLine + "\t" +
                                   Strings.ConflictingConfigurationValue(
                                       "IsFixedLength", false, "IsFixedLength", true);

            additionalErrors += Environment.NewLine + "\t" +
                                Strings.ConflictingConfigurationValue(
                                    "IsMaxLength", false, "IsMaxLength", true);

            additionalErrors += Environment.NewLine + "\t" +
                                Strings.ConflictingConfigurationValue(
                                    "MaxLength", 1, "MaxLength", 2);

            additionalErrors += Environment.NewLine + "\t" +
                                Strings.ConflictingConfigurationValue(
                                    "IsRowVersion", false, "IsRowVersion", true);

            expectedMessageCSpace += additionalErrors;
            expectedMessage += additionalErrors;

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessageCSpace, errorMessage);
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_IsRowVersion()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsRowVersion = true;
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_IsRowVersion()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsRowVersion = false;
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            var expectedMessage = Environment.NewLine + "\t" +
                                  Strings.ConflictingConfigurationValue(
                                      "IsRowVersion", false, "IsRowVersion", true);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_IsRowVersion()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsRowVersion = true;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        internal new BinaryPropertyConfiguration CreateConfiguration()
        {
            return (BinaryPropertyConfiguration)base.CreateConfiguration();
        }

        internal override Type GetConfigurationType()
        {
            return typeof(BinaryPropertyConfiguration);
        }
    }
}
