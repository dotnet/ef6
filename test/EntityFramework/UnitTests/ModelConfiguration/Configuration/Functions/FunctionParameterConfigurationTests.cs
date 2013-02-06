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
    }
}
