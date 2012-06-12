namespace Microsoft.DbContextPackage.Utilities
{
    using System;
    using Xunit;

    public class TemplatesTests
    {
        [Fact]
        public void GetDefaultTemplate_returns_default_templates()
        {
            var contextTemplate = Templates.GetDefaultTemplate(Templates.ContextTemplate);
            Assert.False(string.IsNullOrWhiteSpace(contextTemplate));

            var entityTemplate = Templates.GetDefaultTemplate(Templates.EntityTemplate);
            Assert.False(string.IsNullOrWhiteSpace(entityTemplate));

            var mappingTemplate = Templates.GetDefaultTemplate(Templates.MappingTemplate);
            Assert.False(string.IsNullOrWhiteSpace(mappingTemplate));
        }
    }
}
