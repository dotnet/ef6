// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Common
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class DataModelAnnotationExtensionsTests
    {
        [Fact]
        public void Can_get_and_set_configuration_facet()
        {
            var annotations = new List<MetadataProperty>();

            annotations.SetConfiguration(42);

            Assert.Equal(42, annotations.GetConfiguration());
        }

        [Fact]
        public void Can_get_and_set_clr_type_annotation()
        {
            var annotations = new List<MetadataProperty>();

            annotations.SetClrType(typeof(int));

            Assert.Equal(typeof(int), annotations.GetClrType());
        }

        [Fact]
        public void Can_get_and_set_custom_annotation()
        {
            var annotations = new List<MetadataProperty>();

            Assert.Null(annotations.GetAnnotation("Foo"));

            annotations.SetAnnotation("Foo", "Bar");

            Assert.Equal("Bar", annotations.GetAnnotation("Foo"));

            annotations.SetAnnotation("Foo", "Baz");

            Assert.Equal("Baz", annotations.GetAnnotation("Foo"));
            Assert.Equal(1, annotations.Count);
        }
    }
}
