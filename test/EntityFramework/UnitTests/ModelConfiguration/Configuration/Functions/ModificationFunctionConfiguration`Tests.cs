// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Spatial;
    using Xunit;

    public class ModificationFunctionConfigurationTTests
    {
        [Fact]
        public void HasName_should_set_name_on_underlying_configuration()
        {
            var configuration = new ModificationFunctionConfiguration<Entity>();

            configuration.HasName("Foo");

            Assert.Equal("Foo", configuration.Configuration.Name);
        }

        [Fact]
        public void Parameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new ModificationFunctionConfiguration<Entity>();

            Assert.NotNull(configuration.Parameter(e => e.Int));
            Assert.NotNull(configuration.Parameter(e => e.Nullable));
            Assert.NotNull(configuration.Parameter(e => e.String));
            Assert.NotNull(configuration.Parameter(e => e.Bytes));
            Assert.NotNull(configuration.Parameter(e => e.Geography));
            Assert.NotNull(configuration.Parameter(e => e.Geometry));
            Assert.NotNull(configuration.Parameter(e => e.ComplexType.Int));
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
