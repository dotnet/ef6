// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class StringPropertyConfigurationTests : LengthPropertyConfigurationTests
    {
        [Fact]
        public void Configure_should_update_IsUnicode()
        {
            var configuration = CreateConfiguration();
            configuration.IsUnicode = true;
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            configuration.Configure(property);

            Assert.Equal(true, property.IsUnicode);
        }

        [Fact]
        public void CopyFrom_overwrites_null_IsUnicode()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(true, configurationA.IsUnicode);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_IsUnicode()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsUnicode = false;
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(true, configurationA.IsUnicode);
        }

        [Fact]
        public void FillFrom_overwrites_null_IsUnicode()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(true, configurationA.IsUnicode);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_IsUnicode()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsUnicode = false;
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(false, configurationA.IsUnicode);
        }

        [Fact]
        public void FillFrom_overwrites_null_IsUnicode_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(true, configurationA.IsUnicode);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_IsUnicode_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsUnicode = false;
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(false, configurationA.IsUnicode);
        }

        [Fact]
        public void IsCompatible_returns_errors_for_all_mismatched_string_properties()
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
            configurationA.IsUnicode = false;

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
            configurationB.IsUnicode = true;

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
                                    "IsUnicode", false, "IsUnicode", true);

            expectedMessageCSpace += additionalErrors;
            expectedMessage += additionalErrors;

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessageCSpace, errorMessage);
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_IsUnicode()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsUnicode = true;
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_IsUnicode()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsUnicode = false;
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            var expectedMessage = Environment.NewLine + "\t" +
                                  Strings.ConflictingConfigurationValue(
                                      "IsUnicode", false, "IsUnicode", true);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_IsUnicode()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsUnicode = true;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        internal new StringPropertyConfiguration CreateConfiguration()
        {
            return (StringPropertyConfiguration)base.CreateConfiguration();
        }

        internal override Type GetConfigurationType()
        {
            return typeof(StringPropertyConfiguration);
        }
    }
}
