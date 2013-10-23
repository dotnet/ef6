// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Xml;
    using Xunit;

    public class VersionConverterHandlerTests
    {
        [Fact]
        public void VersionConverterHandler_sets_correct_Version_attribute()
        {
            for (var i = 1; i <= 3; i++)
            {
                var schemaVersion = new Version(i, 0, 0, 0);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<root />");

                new VersionConverterHandler(schemaVersion).HandleConversion(xmlDoc);

                Assert.Equal(schemaVersion.ToString(2), xmlDoc.DocumentElement.Attributes["Version"].Value);
            }
        }

        [Fact]
        public void VersionConverterHandler_overrides_existing_Version_attribute()
        {
            for (var i = 1; i <= 3; i++)
            {
                var schemaVersion = new Version(i, 0, 0, 0);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<root Version=\"X.Y\"/>");

                new VersionConverterHandler(schemaVersion).HandleConversion(xmlDoc);

                Assert.Equal(schemaVersion.ToString(2), xmlDoc.DocumentElement.Attributes["Version"].Value);
            }
        }
    }
}
