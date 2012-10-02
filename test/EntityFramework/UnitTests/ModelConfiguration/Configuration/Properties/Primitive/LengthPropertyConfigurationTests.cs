// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public abstract class LengthPropertyConfigurationTests : PrimitivePropertyConfigurationTests
    {
        [Fact]
        public void Configure_should_update_MaxLength()
        {
            var property = new EdmProperty().AsPrimitive();

            var configuration = CreateConfiguration();
            configuration.MaxLength = 1;

            configuration.Configure(property);

            Assert.Equal(1, property.PropertyType.PrimitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void Configure_should_update_IsFixedLength()
        {
            var property = new EdmProperty().AsPrimitive();

            var configuration = CreateConfiguration();
            configuration.IsFixedLength = true;

            configuration.Configure(property);

            Assert.Equal(true, property.PropertyType.PrimitiveTypeFacets.IsFixedLength);
        }

        [Fact]
        public void Configure_should_update_IsMaxLength()
        {
            var property = new EdmProperty().AsPrimitive();

            var configuration = CreateConfiguration();
            configuration.IsMaxLength = true;

            configuration.Configure(property);

            Assert.Equal(true, property.PropertyType.PrimitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void CopyFrom_overwrites_null_MaxLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(2, configurationA.MaxLength);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_MaxLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.MaxLength = 1;
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(2, configurationA.MaxLength);
        }

        [Fact]
        public void CopyFrom_overwrites_null_IsFixedLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(true, configurationA.IsFixedLength);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_IsFixedLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsFixedLength = false;
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(true, configurationA.IsFixedLength);
        }

        [Fact]
        public void CopyFrom_overwrites_null_IsMaxLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(true, configurationA.IsMaxLength);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_IsMaxLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsMaxLength = false;
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(true, configurationA.IsMaxLength);
        }

        [Fact]
        public void FillFrom_overwrites_null_MaxLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(2, configurationA.MaxLength);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_MaxLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.MaxLength = 1;
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(1, configurationA.MaxLength);
        }

        [Fact]
        public void FillFrom_overwrites_null_MaxLength_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(2, configurationA.MaxLength);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_MaxLength_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.MaxLength = 1;
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(1, configurationA.MaxLength);
        }

        [Fact]
        public void FillFrom_overwrites_null_IsFixedLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(true, configurationA.IsFixedLength);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_IsFixedLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsFixedLength = false;
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(false, configurationA.IsFixedLength);
        }

        [Fact]
        public void FillFrom_overwrites_null_IsFixedLength_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(true, configurationA.IsFixedLength);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_IsFixedLength_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsFixedLength = false;
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(false, configurationA.IsFixedLength);
        }

        [Fact]
        public void FillFrom_overwrites_null_IsMaxLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(true, configurationA.IsMaxLength);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_IsMaxLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsMaxLength = false;
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(false, configurationA.IsMaxLength);
        }

        [Fact]
        public void FillFrom_overwrites_null_IsMaxLength_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(true, configurationA.IsMaxLength);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_IsMaxLength_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsMaxLength = false;
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(false, configurationA.IsMaxLength);
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_MaxLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.MaxLength = 2;
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_MaxLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.MaxLength = 1;
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            var expectedMessage = Environment.NewLine + "\t" +
                                  Strings.ConflictingConfigurationValue(
                                      "MaxLength", 1, "MaxLength", 2);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_MaxLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.MaxLength = 2;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_IsFixedLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsFixedLength = true;
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_IsFixedLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsFixedLength = false;
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            var expectedMessage = Environment.NewLine + "\t" +
                                  Strings.ConflictingConfigurationValue(
                                      "IsFixedLength", false, "IsFixedLength", true);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_IsFixedLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsFixedLength = true;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_IsMaxLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsMaxLength = true;
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_IsMaxLength()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsMaxLength = false;
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            var expectedMessage = Environment.NewLine + "\t" +
                                  Strings.ConflictingConfigurationValue(
                                      "IsMaxLength", false, "IsMaxLength", true);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_IsMaxLength()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsMaxLength = true;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        internal new LengthPropertyConfiguration CreateConfiguration()
        {
            return (LengthPropertyConfiguration)base.CreateConfiguration();
        }
    }
}
