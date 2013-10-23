// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Xunit;

    public class EdmxUtilsTests
    {
        [Fact]
        public void GetEDMXXsdResource_returns_valid_xsd_for_requested_version()
        {
            for (var majorVersion = 1; majorVersion <= 3; majorVersion++)
            {
                var version = new Version(majorVersion, 0, 0, 0);
                var reader = EdmxUtils.GetEDMXXsdResource(version);

                Assert.NotNull(reader);
                var edmxXsd = XDocument.Load(reader);

                Assert.Equal(
                    SchemaManager.GetEDMXNamespaceName(version),
                    (string)edmxXsd.Root.Attribute("targetNamespace"));
            }
        }
    }
}
