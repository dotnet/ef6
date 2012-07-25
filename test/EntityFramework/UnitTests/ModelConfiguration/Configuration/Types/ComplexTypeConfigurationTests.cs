// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.Types.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public sealed class ComplexTypeConfigurationTests
    {
        [Fact]
        public void Configure_should_set_configuration()
        {
            var complexType = new EdmComplexType { Name = "C" };
            var complexTypeConfiguration = new ComplexTypeConfiguration(typeof(object));

            complexTypeConfiguration.Configure(complexType);

            Assert.Same(complexTypeConfiguration, complexType.GetConfiguration());
        }

        [Fact]
        public void Configure_should_configure_properties()
        {
            var complexType = new EdmComplexType { Name = "C" };
            var property = complexType.AddPrimitiveProperty("P");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;
            var complexTypeConfiguration = new ComplexTypeConfiguration(typeof(object));
            var mockPropertyConfiguration = new Mock<PrimitivePropertyConfiguration>();
            var mockPropertyInfo = new MockPropertyInfo();
            complexTypeConfiguration.Property(new PropertyPath(mockPropertyInfo), () => mockPropertyConfiguration.Object);
            property.SetClrPropertyInfo(mockPropertyInfo);

            complexTypeConfiguration.Configure(complexType);

            mockPropertyConfiguration.Verify(p => p.Configure(property));
        }

        [Fact]
        public void Configure_should_throw_when_property_not_found()
        {
            var complexType = new EdmComplexType { Name = "C" };
            var complexTypeConfiguration = new ComplexTypeConfiguration(typeof(object));
            var mockPropertyConfiguration = new Mock<PrimitivePropertyConfiguration>();
            complexTypeConfiguration.Property(new PropertyPath(new MockPropertyInfo()), () => mockPropertyConfiguration.Object);

            Assert.Equal(Strings.PropertyNotFound(("P"), "C"), Assert.Throws<InvalidOperationException>(() => complexTypeConfiguration.Configure(complexType)).Message);
        }
    }
}