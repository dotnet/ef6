// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using Xunit;

    public class ModelNamespaceConventionTests
    {
        [Fact]
        public void Apply_should_set_model_namespace()
        {
            var convention = new ModelNamespaceConvention("Foo");
            var modelConfiguration = new ModelConfiguration();

            convention.ApplyModelConfiguration(modelConfiguration);

            Assert.Equal("Foo", modelConfiguration.ModelNamespace);
        }
    }
}
