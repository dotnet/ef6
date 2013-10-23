// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Xunit;

    public class InitialModelContentsFactoryTests
    {
        [Fact]
        public void GetInitialModelContents_returns_contents()
        {
            foreach (var targetSchemaVersion in EntityFrameworkVersion.GetAllVersions())
            {
                Assert.Equal(
                    EdmUtils.CreateEdmxString(targetSchemaVersion, string.Empty, string.Empty, string.Empty),
                    new InitialModelContentsFactory().GetInitialModelContents(targetSchemaVersion));
            }
        }
    }
}
