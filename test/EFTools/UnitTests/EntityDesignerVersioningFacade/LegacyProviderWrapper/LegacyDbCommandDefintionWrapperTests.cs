// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Legacy = System.Data.Common;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using Moq;
    using Xunit;

    public class LegacyDbCommandDefintionWrapperTests
    {
        [Fact]
        public void CreateCommand_calls_into_wrapped_legacy_DbCommandDefinition_to_create_DbCommand()
        {
            var mockCommand = new Mock<Legacy.DbCommand>();

            var mockLegacyDbCommandDefinition = new Mock<Legacy.DbCommandDefinition>();
            mockLegacyDbCommandDefinition
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            Assert.Same(
                mockCommand.Object,
                new LegacyDbCommandDefinitionWrapper(mockLegacyDbCommandDefinition.Object).CreateCommand());
        }
    }
}
