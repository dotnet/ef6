namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Runtime.Serialization;
    using Moq;
    using Xunit;

    public sealed class AttributeMapperTests
    {
        [Fact]
        public void AttributeMapper_should_map_annotation_attributes_for_types()
        {
            var mockType = new MockType("T");
            var mockAttributeProvider = new Mock<AttributeProvider>();
            mockAttributeProvider
                .Setup(a => a.GetAttributes(mockType.Object))
                .Returns(new Attribute[]
                    {
                        new DataContractAttribute(),
                        new TableAttribute("MyTable")
                    });

            var annotations = new List<DataModelAnnotation>();

            new AttributeMapper(mockAttributeProvider.Object).Map(mockType, annotations);

            Assert.Equal(1, annotations.Count);
            Assert.Equal(2, annotations.GetClrAttributes().Count);
        }

        [Fact]
        public void AttributeMapper_should_map_annotation_attribute_for_properties()
        {
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "P");
            var mockAttributeProvider = new Mock<AttributeProvider>();
            mockAttributeProvider
                .Setup(a => a.GetAttributes(mockPropertyInfo.Object))
                .Returns(new Attribute[]
                    {
                        new DataContractAttribute(),
                        new TableAttribute("MyTable")
                    });

            var annotations = new List<DataModelAnnotation>();

            new AttributeMapper(mockAttributeProvider.Object).Map(mockPropertyInfo, annotations);

            Assert.Equal(1, annotations.Count);
            Assert.Equal(2, annotations.GetClrAttributes().Count);
        }
    }
}