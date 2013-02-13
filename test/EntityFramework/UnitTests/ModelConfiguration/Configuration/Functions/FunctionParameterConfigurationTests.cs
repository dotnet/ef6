// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class FunctionParameterConfigurationTests
    {
        [Fact]
        public void Can_clone_configuration()
        {
            var functionParameterConfiguration = new FunctionParameterConfiguration();

            functionParameterConfiguration.HasName("F1");

            var clone = functionParameterConfiguration.Clone();

            Assert.NotSame(functionParameterConfiguration, clone);
            Assert.Equal("F1", clone.ParameterName);
        }

        [Fact]
        public void Can_configure_parameter_name()
        {
            var functionParameter = new FunctionParameter();

            var functionParameterConfiguration = new FunctionParameterConfiguration();

            functionParameterConfiguration.HasName("F1");

            functionParameterConfiguration.Configure(functionParameter);

            Assert.Equal("F1", functionParameter.Name);
        }

        [Fact]
        public void Equals_should_compare_names()
        {
            var functionParameterConfiguration1 = new FunctionParameterConfiguration();

            functionParameterConfiguration1.HasName("F1");

            var functionParameterConfiguration2 = new FunctionParameterConfiguration();

            Assert.NotEqual(functionParameterConfiguration1, functionParameterConfiguration2);

            functionParameterConfiguration2.HasName("f1");

            Assert.Equal(functionParameterConfiguration1, functionParameterConfiguration2);
        }

        [Fact]
        public void Equals_should_compare_instances()
        {
            var functionParameterConfiguration = new FunctionParameterConfiguration();

            Assert.Equal(functionParameterConfiguration, functionParameterConfiguration);
        }

        [Fact]
        public void GetHasCode_should_return_name_hash_code_when_present()
        {
            var functionParameterConfiguration = new FunctionParameterConfiguration();

            Assert.Equal(0, functionParameterConfiguration.GetHashCode());

            functionParameterConfiguration.HasName("A");

            Assert.Equal("A".GetHashCode(), functionParameterConfiguration.GetHashCode());
        }
    }
}
