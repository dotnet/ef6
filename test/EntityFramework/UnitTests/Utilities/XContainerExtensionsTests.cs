namespace System.Data.Entity.Utilities
{
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class XContainerExtensionsTests
    {
        [Fact]
        public void GetOrAddElement_should_return_existing_element()
        {
            var childElement = new XElement("child");
            var element
                = new XElement(
                    "parent",
                    childElement);

            var result = element.GetOrAddElement("child");

            Assert.Same(childElement, result);
        }

        [Fact]
        public void GetOrAddElement_should_add_and_return_new_element()
        {
            var element = new XElement("parent");

            var result = element.GetOrAddElement("child");

            Assert.True(element.Elements().Contains(result));
            Assert.Equal<XName>("child", result.Name);
        }
    }
}