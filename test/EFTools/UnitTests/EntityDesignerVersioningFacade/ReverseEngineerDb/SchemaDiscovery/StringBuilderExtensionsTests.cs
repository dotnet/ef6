// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System.Text;
    using Xunit;

    public class StringBuilderExtensionsTests
    {
        [Fact]
        public void StringBuilder_AppendIfNotEmpty_appends_string_to_non_empty_StringBuilder()
        {
            Assert.Equal("ab", new StringBuilder("a").AppendIfNotEmpty("b").ToString());
        }

        [Fact]
        public void StringBuilder_AppendIfNotEmpty_does_not_append_string_to_empty_StringBuilder()
        {
            Assert.Equal(string.Empty, new StringBuilder().AppendIfNotEmpty("b").ToString());
        }
    }
}
