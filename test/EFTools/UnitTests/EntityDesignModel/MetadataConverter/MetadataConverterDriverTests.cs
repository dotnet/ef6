// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Xml;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Xunit;

    public class MetadataConverterDriverTests
    {
        [Fact]
        public void Convert_returns_null_for_non_SqlCE()
        {
            var xmlDoc = new XmlDocument();

            foreach (var edmxNs in SchemaManager.GetEDMXNamespaceNames())
            {
                xmlDoc.LoadXml(string.Format("<Edmx xmlns=\"{0}\" />", edmxNs));
                Assert.Null(MetadataConverterDriver.Instance.Convert(xmlDoc));
            }
        }

        [Fact]
        public void Convert_returns_converted_xml_for_SqlCE()
        {
            var xmlDoc = new XmlDocument();

            foreach (var edmxNs in SchemaManager.GetEDMXNamespaceNames())
            {
                xmlDoc.LoadXml(string.Format("<Edmx xmlns=\"{0}\" />", edmxNs));
                Assert.NotNull(MetadataConverterDriver.SqlCeInstance.Convert(xmlDoc));
            }
        }
    }
}
