// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Linq;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    public class PropertyBaseTests
    {
        [Fact]
        public void PreviousSiblingInPropertyXElementOrder_returns_previous_property_if_exists()
        {
            var properties = CreateProperties();
            Assert.Same(properties[0], properties[1].PreviousSiblingInPropertyXElementOrder);
        }

        [Fact]
        public void PreviousSiblingInPropertyXElementOrder_returns_null_if_previous_property_does_not_exist()
        {
            Assert.Null(CreateProperties()[0].PreviousSiblingInPropertyXElementOrder);
        }

        [Fact]
        public void NextSiblingInPropertyXElementOrder_returns_next_property_if_exists()
        {
            var properties = CreateProperties();
            Assert.Same(properties[1], properties[0].NextSiblingInPropertyXElementOrder);
        }

        [Fact]
        public void NextSiblingInPropertyXElementOrder_returns_null_if_next_property_does_not_exist()
        {
            Assert.Null(CreateProperties()[1].NextSiblingInPropertyXElementOrder);
        }

        private static PropertyBase[] CreateProperties()
        {
            var complexType = XElement.Parse(
                "<ComplexType Name=\"Category\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\">" +
                "  <Property Name=\"CategoryID\" Type=\"Int32\" Nullable=\"false\" />" +
                "  <Property Name=\"Description\" Type=\"String\" MaxLength=\"4000\" FixedLength=\"false\" Unicode=\"true\" />" +
                "</ComplexType>");

            var mockCategoryId = new Mock<PropertyBase>(null, complexType.Elements().First(), null);
            mockCategoryId.Setup(m => m.EFTypeName).Returns("Property");

            var mockDescription = new Mock<PropertyBase>(null, complexType.Elements().Last(), null);
            mockDescription.Setup(m => m.EFTypeName).Returns("Property");

            return new[] { mockCategoryId.Object, mockDescription.Object };
        }
    }
}
