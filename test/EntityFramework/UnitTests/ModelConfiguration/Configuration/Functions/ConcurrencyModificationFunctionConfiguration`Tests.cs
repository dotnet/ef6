// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Spatial;
    using Xunit;

    public class ConcurrencyModificationFunctionConfigurationTTests
    {
        [Fact]
        public void Parameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new ConcurrencyModificationFunctionConfiguration<Entity>();

            Assert.NotNull(configuration.Parameter(e => e.Int, true));
            Assert.NotNull(configuration.Parameter(e => e.Nullable, true));
            Assert.NotNull(configuration.Parameter(e => e.String, true));
            Assert.NotNull(configuration.Parameter(e => e.Bytes, true));
            Assert.NotNull(configuration.Parameter(e => e.Geography, true));
            Assert.NotNull(configuration.Parameter(e => e.Geometry, true));
            Assert.NotNull(configuration.Parameter(e => e.ComplexType.Int, true));
        }

        private class Entity
        {
            public int Int { get; set; }
            public short? Nullable { get; set; }
            public string String { get; set; }
            public byte[] Bytes { get; set; }
            public DbGeography Geography { get; set; }
            public DbGeometry Geometry { get; set; }
            public ComplexType ComplexType { get; set; }
        }

        private class ComplexType
        {
            public int Int { get; set; }
        }
    }
}
