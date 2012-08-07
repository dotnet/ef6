// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;
    using Strings = System.Data.Entity.Resources.Strings;

    public sealed class BinaryPropertyConfigurationTests : LengthPropertyConfigurationTests
    {
        [Fact]
        public void Configure_should_update_IsRowVersion()
        {
            var configuration = CreateConfiguration();
            configuration.IsRowVersion = true;
            var property = new EdmProperty().AsPrimitive();

            configuration.Configure(property);

            Assert.Equal(8, property.PropertyType.PrimitiveTypeFacets.MaxLength);
            Assert.Equal(false, property.PropertyType.IsNullable);
            Assert.Equal(EdmConcurrencyMode.Fixed, property.ConcurrencyMode);
            Assert.Equal(DbStoreGeneratedPattern.Computed, property.GetStoreGeneratedPattern());

            var edmPropertyMapping = new DbEdmPropertyMapping
                                         {
                                             Column = new DbTableColumnMetadata
                                                          {
                                                              Facets = new DbPrimitiveTypeFacets()
                                                          }
                                         };

            configuration.Configure(
                new[] { Tuple.Create(edmPropertyMapping, new DbTableMetadata()) }, ProviderRegistry.Sql2008_ProviderManifest);
            Assert.Equal("rowversion", edmPropertyMapping.Column.TypeName);
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
            configurationA.ConcurrencyMode = EdmConcurrencyMode.None;
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
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;
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
                                         "ConcurrencyMode", EdmConcurrencyMode.None, "ConcurrencyMode", EdmConcurrencyMode.Fixed);

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
