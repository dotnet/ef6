// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public class SsdlSerializerTests
    {
        [Fact]
        public void SsdlSerializer_uses_entitycontainer_to_create_schema_namespace_name_if_no_provided()
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new SsdlSerializer()
                    .Serialize(new EdmModel(DataSpace.SSpace), null, null, writer);
            }

            Assert.Equal(
                "CodeFirstDatabaseSchema",
                (string)XDocument.Parse(sb.ToString()).Root.Attribute("Namespace"));
        }

        [Fact]
        public void SsdlSerializer_uses_schema_namespace_name_if_provided()
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new SsdlSerializer()
                    .Serialize(new EdmModel(DataSpace.SSpace), "MyNamespace", null, null, writer);
            }

            Assert.Equal(
                "MyNamespace",
                (string)XDocument.Parse(sb.ToString()).Root.Attribute("Namespace"));
        }
    }
}
